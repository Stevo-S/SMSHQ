using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MessageSender.Models;
using PagedList;
using System.Data.Entity.Core;

namespace MessageSender.Controllers
{
    public class DeliveriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Deliveries
        public ActionResult Index(string destination, DateTime? startDate, DateTime? endDate, string deliveryStatus, string serviceId, int? page)
        {
            // Log database commands
            db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);

            var deliveries = from d in db.Deliveries
                             select d;

            List<SelectListItem> deliveryStatuses = new List<SelectListItem>
            {
                new SelectListItem { Text = "Any Delivery Status", Value = "" },
                new SelectListItem { Text = "Delivered", Value = "DeliveredToTerminal" },
                new SelectListItem { Text = "Insufficient Balance", Value = "Insufficient_Balance" },
                new SelectListItem { Text = "User Not Subscribed", Value = "UserNotSubscribed" },
                new SelectListItem { Text = "Delivery Impossible", Value = "DeliveryImpossible" },
                new SelectListItem { Text = "Invalid Link ID", Value = "Invalid_LinkId" },
                new SelectListItem { Text = "User Abnormal State", Value = "UserAbnormalState" },
                new SelectListItem { Text = "User Not Exist", Value = "UserNotExist" },
                new SelectListItem { Text = "User MT Call Barred", Value = "UserMTCallBarred" },
                new SelectListItem { Text = "User In Blacklist", Value = "UserInBlacklist" }
            };

            var serviceIds = new SelectList(db.Services, "ServiceId", "ServiceId");

            // Filter by destination
            if (!string.IsNullOrWhiteSpace(destination))
            {
                deliveries = deliveries.Where(d => d.Destination.Contains(destination));
                ViewBag.destinationFilter = destination;
            }

            // Filter by delivery status
            if (!string.IsNullOrWhiteSpace(deliveryStatus))
            {
                deliveries = deliveries.Where(d => d.DeliveryStatus.Equals(deliveryStatus));
                deliveryStatuses.Find(status => status.Value.Equals(deliveryStatus)).Selected = true;
                ViewBag.deliveryStatusFilter = deliveryStatus;
            }
            ViewBag.deliveryStatus = deliveryStatuses;

            // Filter by date
            // Default date range is today
            if (startDate == null)
            {
                startDate = DateTime.Today;
            }
            if (endDate == null)
            {
                endDate = DateTime.Today.AddDays(1);
            }
            deliveries = deliveries.Where(d => d.TimeStamp > startDate && d.TimeStamp < endDate);
            ViewBag.startDateFilter = startDate.Value.ToString("s");
            ViewBag.endDateFilter = endDate.Value.ToString("s");

            // Filter By Service ID
            if (!string.IsNullOrWhiteSpace(serviceId))
            {
                deliveries = deliveries.Where(d => d.ServiceId.Equals(serviceId));
                serviceIds.Where(s => s.Value.Equals(serviceId)).FirstOrDefault().Selected = true;
            }
            ViewBag.serviceId = serviceIds;

            deliveries = deliveries.OrderByDescending(d => d.TimeStamp);
            int pageNumber = (page ?? 1);
            int pageSize = 25;

            try
            {
                return View(deliveries.ToPagedList(pageNumber, pageSize));
            }
            catch (EntityCommandExecutionException ex)
            {
                // Handle Wait operation timeouts
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return View("TimeoutError");
            }
        }

        // GET: Deliveries/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Delivery delivery = db.Deliveries.Find(id);
            if (delivery == null)
            {
                return HttpNotFound();
            }
            return View(delivery);
        }

        // GET: Deliveries/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Deliveries/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Destination,DeliveryStatus,ServiceId,Correlator,TraceUniqueId,TimeStamp")] Delivery delivery)
        {
            if (ModelState.IsValid)
            {
                db.Deliveries.Add(delivery);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(delivery);
        }

        // GET: Deliveries/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Delivery delivery = db.Deliveries.Find(id);
            if (delivery == null)
            {
                return HttpNotFound();
            }
            return View(delivery);
        }

        // POST: Deliveries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Destination,DeliveryStatus,ServiceId,Correlator,TraceUniqueId,TimeStamp")] Delivery delivery)
        {
            if (ModelState.IsValid)
            {
                db.Entry(delivery).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(delivery);
        }

        // GET: Deliveries/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Delivery delivery = db.Deliveries.Find(id);
            if (delivery == null)
            {
                return HttpNotFound();
            }
            return View(delivery);
        }

        // POST: Deliveries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Delivery delivery = db.Deliveries.Find(id);
            db.Deliveries.Remove(delivery);
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
