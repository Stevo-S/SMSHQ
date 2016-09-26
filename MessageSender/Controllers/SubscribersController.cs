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
using PagedList;
using EntityFramework.BulkInsert.Extensions;
using System.IO;

namespace MessageSender.Controllers
{
    public class SubscribersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Subscribers
        public ActionResult Index(string phoneNumber, DateTime? startDate, DateTime? endDate, string subscriptionStatus, int? page)
        {
            var subscribers = from s in db.Subscribers
                              select s;

            // Filter by phoneNumber
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                subscribers = subscribers.Where(s => s.PhoneNumber.Contains(phoneNumber));
                ViewBag.phoneFilter = phoneNumber;
            }

            // Filter by subscription status
            if (!string.IsNullOrEmpty(subscriptionStatus))
            {
                if (subscriptionStatus == "active")
                {
                    subscribers = subscribers.Where(s => s.isActive == true);
                }
                else if (subscriptionStatus == "inactive")
                {
                    subscribers = subscribers.Where(p => !p.isActive == true);
                }

                ViewBag.subscriptionStatusFilter = subscriptionStatus;
            }

            if (startDate != null)
            {
                if (endDate < startDate || endDate == null)
                {
                    endDate = DateTime.Now;
                }
                subscribers = subscribers.Where(s => s.FirstSubscriptionDate > startDate && s.FirstSubscriptionDate < endDate);
                ViewBag.startDateFilter = startDate;
                ViewBag.endDateFilter = endDate;
            }

            subscribers = subscribers.OrderByDescending(s => s.FirstSubscriptionDate);
            ViewBag.Total = subscribers.Count();
            int pageSize = 25;
            int pageNumber = (page ?? 1);
            return View(subscribers.ToPagedList(pageNumber, pageSize));
        }

        // GET: Subscribers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber subscriber = db.Subscribers.Find(id);
            if (subscriber == null)
            {
                return HttpNotFound();
            }
            return View(subscriber);
        }

        // GET: Subscribers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Subscribers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,PhoneNumber,ServiceId,isActive,FirstSubscriptionDate,LastSubscriptionDate,LastUnsubscriptionDate")] Subscriber subscriber)
        {
            if (ModelState.IsValid)
            {
                db.Subscribers.Add(subscriber);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(subscriber);
        }

        // GET: Subscribers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber subscriber = db.Subscribers.Find(id);
            if (subscriber == null)
            {
                return HttpNotFound();
            }
            return View(subscriber);
        }

        // POST: Subscribers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,PhoneNumber,ServiceId,isActive,FirstSubscriptionDate,LastSubscriptionDate,LastUnsubscriptionDate")] Subscriber subscriber)
        {
            if (ModelState.IsValid)
            {
                db.Entry(subscriber).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(subscriber);
        }

        // GET: Subscribers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Subscriber subscriber = db.Subscribers.Find(id);
            if (subscriber == null)
            {
                return HttpNotFound();
            }
            return View(subscriber);
        }

        // POST: Subscribers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Subscriber subscriber = db.Subscribers.Find(id);
            db.Subscribers.Remove(subscriber);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase subscribersFile)
        {
            var ErrorMessages = new List<string>() { };
            var SuccessMessages = new List<string>() { };

            try
            {
                ViewBag.hasErrors = false;
                int duplicates = 0;
                var contentLength = subscribersFile.ContentLength;
                if ((subscribersFile != null) && (contentLength > 0) && !string.IsNullOrEmpty(subscribersFile.FileName))
                {
                    BinaryReader uploadedFileReader = new BinaryReader(subscribersFile.InputStream);
                    byte[] uploadedSubscriberData = uploadedFileReader.ReadBytes(contentLength);
                    var subscriberDataStream = new MemoryStream(uploadedSubscriberData);
                    using (var package = new ExcelPackage(subscriberDataStream))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var worksheet = currentSheet.First();
                        var numberOfColumns = worksheet.Dimension.End.Column;
                        var numberOfRows = worksheet.Dimension.End.Row;

                        string phoneColumn, serviceIdColumn;
                        using (var headers = worksheet.Cells[1, 1, 1, numberOfColumns])
                        {
                            var expectedHeaders = new[] { "phone",
                                                          "service",
                                                        };
                            if (!expectedHeaders.All(eh => headers.Any(h => h.Value.ToString().ToLower().StartsWith(eh))))
                            {
                                ErrorMessages.Add("Missing and/or incorrectly named fields");
                                ViewBag.ErrorNotifications = ErrorMessages;
                                ViewBag.hasErrors = true;
                                return View();
                            }

                            phoneColumn = headers.First(h => h.Value.ToString().ToLower().StartsWith("phone")).Address[0].ToString();
                            serviceIdColumn = headers.First(h => h.Value.ToString().ToLower().StartsWith("service")).Address[0].ToString();
                        }

                        // Disable automatic detection of changes  and validation to efficiently load large files
                        db.Configuration.AutoDetectChangesEnabled = false;
                        db.Configuration.ValidateOnSaveEnabled = false;


                        List<Subscriber> newSubscribers = new List<Subscriber>();
                        for (int row = 2; row <= numberOfRows; row++)
                        {
                            var phone = worksheet.Cells[phoneColumn + row].Value.ToString();
                            var serviceId = worksheet.Cells[serviceIdColumn + row].Value.ToString();

                            // Ignore records with the phone number already existing in the database
                            if (db.Subscribers.Where(s => s.PhoneNumber.Equals(phone) && s.ServiceId.Equals(serviceId)).Any())
                            {
                                duplicates++;
                                continue;
                            }

                            var subscriber = new Subscriber()
                            {
                                PhoneNumber = phone,
                                ServiceId = serviceId,
                                isActive = true,
                                FirstSubscriptionDate = DateTime.Now,
                                LastSubscriptionDate = DateTime.Now
                            };

                            newSubscribers.Add(subscriber);

                        }

                        using (var transaction = db.Database.BeginTransaction())
                        {
                            db.BulkInsert(newSubscribers);
                            transaction.Commit();
                        }
                        // Re-enable automatic detection of changes and validation
                        db.Configuration.AutoDetectChangesEnabled = true;
                        db.Configuration.ValidateOnSaveEnabled = true;

                        SuccessMessages.Add("Upload Successful.");
                        if (duplicates > 0)
                        {
                            SuccessMessages.Add(duplicates.ToString() + " duplicate/existing phone numbers detected. They have been ignored.");
                        }
                    }

                    db.SaveChanges();
                    TempData["SuccessNotifications"] = SuccessMessages;
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                ViewBag.hasErrors = true;
                ErrorMessages.Add("Could not add Subscribers into the database. The Uploaded file is not properly formatted.");
                ViewBag.ErrorNotifications = ErrorMessages;
                return View();
            }

            return View();

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
