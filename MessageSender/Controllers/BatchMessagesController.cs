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

namespace MessageSender.Controllers
{
    public class BatchMessagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BatchMessages
        public ActionResult Index()
        {
            return View(db.BatchMessages.ToList());
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
            return View();
        }

        // POST: BatchMessages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,MessageContent,StartTime,EndTime,Sender,ServiceId")] BatchMessage batchMessage, HttpPostedFileBase contactsFile)
        {
            if ((contactsFile != null) && (contactsFile.ContentLength > 0) && !string.IsNullOrEmpty(contactsFile.FileName))
            {
                if (ModelState.IsValid)
                {
                    db.BatchMessages.Add(batchMessage);
                }
                else
                {
                    return View(batchMessage);
                }

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

                        // Disable automatic detection of changes  and validation to efficiently load large files
                        db.Configuration.AutoDetectChangesEnabled = false;
                        db.Configuration.ValidateOnSaveEnabled = false;

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

                    db.SaveChanges();

                    // Re-enable automatic detection of changes and validation
                    db.Configuration.AutoDetectChangesEnabled = true;
                    db.Configuration.ValidateOnSaveEnabled = true;
                }

                BackgroundJob.Enqueue(() => MessageJobs.SendBatchMessage(batchMessage.Id));
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
        public ActionResult Edit([Bind(Include = "Id,MessageContent,StartTime,EndTime")] BatchMessage batchMessage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(batchMessage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(batchMessage);
        }

        // GET: BatchMessages/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: BatchMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BatchMessage batchMessage = db.BatchMessages.Find(id);
            db.BatchMessages.Remove(batchMessage);
            db.SaveChanges();
            return RedirectToAction("Index");
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
