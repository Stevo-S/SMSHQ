﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MessageSender.Models;
using OfficeOpenXml;
using Hangfire;
using MessageSender.Jobs;
using EntityFramework.BulkInsert.Extensions;
using PagedList;

namespace MessageSender.Controllers
{
    public class BatchMessagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BatchMessages
        public ActionResult Index(String sender, String messageText, DateTime? startDate, DateTime? endDate, int? page)
        {
            var messages = from m in db.BatchMessages
                           select m;
            messages = messages.OrderByDescending(m => m.Id);

            // Filter by sender
            if (!String.IsNullOrEmpty(sender))
            {
                messages = messages.Where(m => m.Sender.Contains(sender));
                ViewBag.SenderFilter = sender;
            }

            // Filter by message context
            if (!String.IsNullOrEmpty(messageText))
            {
                messages = messages.Where(m => m.MessageContent.Contains(messageText));
                ViewBag.MessageFilter = messageText;
            }

            // Filter by startTime
            if (startDate != null)
            {
                if (endDate < startDate || endDate == null)
                {
                    endDate = DateTime.Now;
                }
                messages = messages.Where(m => m.StartTime > startDate && m.StartTime < endDate);
                ViewBag.StartDateFilter = startDate.Value.ToString("s");
                ViewBag.EndDateFilter = endDate.Value.ToString("s");
            }

            int pageNumber = (page ?? 1);
            int pageSize = 25;
            ViewBag.Total = messages.Count();

            return View(messages.ToPagedList(pageNumber, pageSize));
        }

        // GET: BatchMessages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BatchMessage batchMessage = db.BatchMessages.Find(id);
            if (batchMessage == null)
            {
                return HttpNotFound();
            }
            return View(batchMessage);
        }

        // GET: BatchMessages/Create
        public ActionResult Create()
        {
            var newBatchMessage = new BatchMessage()
            {
                StartTime = DateTime.Now.AddMinutes(2),
                EndTime = DateTime.Now.AddHours(2)
            };
            var services = db.Services.Include(s => s.ShortCode);
            ViewBag.Services = db.Services.Where(s => s.ShortCode.Activated).Select(s => new { Name = s.Name, ServiceId = s.ServiceId }).ToArray();
            ViewBag.ShortCodes = db.ShortCodes.Where(sc => sc.Activated).Select(sc => sc.Code).ToArray();
            return View(newBatchMessage);
        }

        // POST: BatchMessages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,MessageContent,StartTime,EndTime,Sender,ServiceId")] BatchMessage batchMessage, string[] ServiceIds, HttpPostedFileBase contactsFile)
        {
            var timeLeft = batchMessage.StartTime - DateTime.Now;

            // If it is a chained batch message
            if (ServiceIds != null && ServiceIds.Any())
            {
                var selectedServices = db.Services.Where(s => ServiceIds.Contains(s.ServiceId)).Include(s => s.ShortCode).ToList();
                string parentJobId = "";
                var currentTime = DateTime.Now;
                foreach (var service in selectedServices)
                {
                    var bm = new BatchMessage
                    {
                        MessageContent = batchMessage.MessageContent,
                        StartTime = batchMessage.StartTime,
                        EndTime = batchMessage.EndTime,
                        Sender = service.ShortCode.Code,
                        ServiceId = service.ServiceId,
                        CreatedAt = currentTime
                    };

                    db.BatchMessages.Add(bm);
                    db.SaveChanges();
                    if (!String.IsNullOrEmpty(parentJobId))
                    {
                        // If this is not the first job in the batch, chain current job to already scheduled job
                        parentJobId = BackgroundJob.ContinueWith(parentJobId, () => MessageJobs.SendBatchMessage(bm.Id));
                    }
                    else
                    {
                        // This is the first job in this batch, schedule it then
                        parentJobId = BackgroundJob.Schedule(() => MessageJobs.SendBatchMessage(bm.Id), timeLeft);
                    }

                }

                return RedirectToAction("Index");
            }

            // Then if it is an individual message
            if (ModelState.IsValid)
            {
                batchMessage.CreatedAt = DateTime.Now;
                db.BatchMessages.Add(batchMessage);
                db.SaveChanges();
            }
            else
            {
                return View(batchMessage);
            }

            // Display SQL Output during debug sessions
            db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

            // Disable automatic detection of changes  and validation to efficiently load large files
            db.Configuration.AutoDetectChangesEnabled = false;
            db.Configuration.ValidateOnSaveEnabled = false;

            // Send message to contacts on file if available
            if ((contactsFile != null) && (contactsFile.ContentLength > 0) && !string.IsNullOrEmpty(contactsFile.FileName))
            {

                using (var package = new ExcelPackage(contactsFile.InputStream))
                {
                    var sheets = package.Workbook.Worksheets;
                    var worksheet = sheets.First();
                    var numberOfColumns = worksheet.Dimension.End.Column;
                    var numberOfRows = worksheet.Dimension.End.Row;

                    string phoneColumn;

                    using (var headers = worksheet.Cells[1, 1, 1, numberOfColumns])
                    {
                        var expectedHeaders = new[] { "phone" };
                        if (!expectedHeaders.All(eh => headers.Any(h => h.Value.ToString().ToLower().StartsWith(eh))))
                        {
                            //ErrorMessages.Add("Missing and/or incorrectly named fields");
                            ViewBag.hasErrors = true;
                            return View();
                        }

                        phoneColumn = headers.First(h => h.Value.ToString().ToLower().StartsWith("phone")).Address[0].ToString();


                        for (int row = 2; row <= numberOfRows; row++)
                        {
                            var phone = worksheet.Cells[phoneColumn + row].Value != null ? worksheet.Cells[phoneColumn + row].Value.ToString() : "";

                            if (!phone.StartsWith("254") || phone.Length != 12)
                            {
                                continue;
                            }

                            var recipient = new BatchMessageRecipient()
                            {
                                Destination = phone,
                                Message = batchMessage,
                                Timestamp = DateTime.Now
                            };

                            db.BatchMessageRecipients.Add(recipient);
                        }
                    }
                }

            }
            else // Subscribers will be picked from database when the job runs
            {

                // Send now if StartTime is within two minutes from now
                if (timeLeft.Minutes < 2)
                {
                    BackgroundJob.Enqueue(() => MessageJobs.SendBatchMessage(batchMessage.Id));
                }
                else
                {
                    BackgroundJob.Schedule(() => MessageJobs.SendBatchMessage(batchMessage.Id), timeLeft);
                }


                // Re-enable automatic detection of changes and validation
                db.Configuration.AutoDetectChangesEnabled = true;
                db.Configuration.ValidateOnSaveEnabled = true;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(batchMessage);

        }

        // GET: BatchMessages/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BatchMessage batchMessage = db.BatchMessages.Find(id);
            if (batchMessage == null)
            {
                return HttpNotFound();
            }
            return View(batchMessage);
        }

        // POST: BatchMessages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,MessageContent,ServiceId,Sender,StartTime,EndTime,CreatedAt")] BatchMessage batchMessage)
        {
            if (ModelState.IsValid && batchMessage.StartTime > DateTime.Now)
            {
                db.Entry(batchMessage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            var ErrorMessages = new List<string>() { };
            ErrorMessages.Add("Sorry you can't edit this message because it has already been sent!");
            ViewBag.ErrorNotifications = ErrorMessages;
            return View(batchMessage);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
