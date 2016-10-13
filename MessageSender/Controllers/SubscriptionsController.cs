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

namespace MessageSender.Controllers
{
    public class SubscriptionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Subscriptions
        public ActionResult Index(string userId, int? updateType, DateTime? startDate, DateTime? endDate, string serviceId, int? page)
        {
            var serviceIds = new SelectList(db.Services, "ServiceId", "ServiceId");

            List<SelectListItem> updateDescriptions = new List<SelectListItem>
            {
                new SelectListItem { Text = "Any Type", Value = "" },
                new SelectListItem { Text = "Subscriptions", Value = "1" },
                new SelectListItem { Text = "Un-subscriptions", Value = "2" },
                new SelectListItem { Text = "Modification", Value = "3" }
            };

            var syncOrders = from s in db.SyncOrders
                             select s;

            // Filter by UserId
            if (!string.IsNullOrWhiteSpace(userId))
            {
                syncOrders = syncOrders.Where(s => s.UserId.Contains(userId));
                ViewBag.destinationFilter = userId;
            }

            // Filter by updateType
            if (updateType != null)
            {
                syncOrders = syncOrders.Where(s => s.UpdateType.Equals(updateType.Value));
                updateDescriptions.Find(u => u.Value.Equals(updateType.ToString())).Selected = true;
                ViewBag.updateTypeFilter = updateType;
            }
            ViewBag.updateType = updateDescriptions;

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
            syncOrders = syncOrders.Where(s => s.Timestamp > startDate && s.Timestamp < endDate);
            ViewBag.startDateFilter = startDate.Value.ToString("s");
            ViewBag.endDateFilter = endDate.Value.ToString("s");

            // Filter By Service ID
            if (!string.IsNullOrWhiteSpace(serviceId))
            {
                syncOrders = syncOrders.Where(s => s.ServiceId.Equals(serviceId));
                serviceIds.Where(s => s.Value.Equals(serviceId)).FirstOrDefault().Selected = true;
            }
            ViewBag.serviceId = serviceIds;

            int pageNumber = (page ?? 1);
            int pageSize = 25;
            syncOrders = syncOrders.OrderByDescending(s => s.Id);
            return View(syncOrders.ToPagedList(pageNumber, pageSize));
        }

        // GET: Subscriptions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SyncOrder syncOrder = db.SyncOrders.Find(id);
            if (syncOrder == null)
            {
                return HttpNotFound();
            }
            return View(syncOrder);
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
