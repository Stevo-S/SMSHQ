using System;
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
        public ActionResult Index(int? page)
        {
            var messages = from m in db.BatchMessages
                           select m;
            messages = messages.OrderByDescending(m => m.Id);

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
            ViewBag.Services = db.Services.Select(s => new { Name = s.Name, ServiceId = s.ServiceId }).ToArray();
            ViewBag.ShortCodes = db.ShortCodes.Select(sc => sc.Code).ToArray();
            return View(newBatchMessage);
        }

        // POST: BatchMessages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,MessageContent,StartTime,EndTime,Sender,ServiceId")] BatchMessage batchMessage, HttpPostedFileBase contactsFile)
        {
            if (ModelState.IsValid)
            {
                db.BatchMessages.Add(batchMessage);
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
            else // Pick subscribers from database
            {
                var subscribers = db.Subscribers.Where(s => s.isActive && s.ServiceId.Equals(batchMessage.ServiceId)).ToList();
                var recipients = from r in subscribers
                                 select new BatchMessageRecipient
                                 {
                                     Destination = r.PhoneNumber,
                                     MessageId = batchMessage.Id,
                                     Timestamp = DateTime.Now
                                 };

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.SaveChanges();
                        db.BulkInsert(recipients);

                        BackgroundJob.Enqueue(() => MessageJobs.SendBatchMessage(batchMessage.Id));

                        // Re-enable automatic detection of changes and validation
                        db.Configuration.AutoDetectChangesEnabled = true;
                        db.Configuration.ValidateOnSaveEnabled = true;

                        transaction.Commit();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        transaction.Rollback();
                        return View(batchMessage);
                    }
                }
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
