using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MessageSender.Models;

namespace MessageSender.Controllers
{
    public class SubscriptionWelcomeMessagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SubscriptionWelcomeMessages
        public ActionResult Index()
        {
            var subscriptionWelcomeMessages = db.SubscriptionWelcomeMessages.Include(s => s.Service);
            return View(subscriptionWelcomeMessages.ToList());
        }

        // GET: SubscriptionWelcomeMessages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubscriptionWelcomeMessage subscriptionWelcomeMessage = db.SubscriptionWelcomeMessages.Find(id);
            if (subscriptionWelcomeMessage == null)
            {
                return HttpNotFound();
            }
            return View(subscriptionWelcomeMessage);
        }

        // GET: SubscriptionWelcomeMessages/Create
        public ActionResult Create()
        {
            ViewBag.ServiceId = new SelectList(db.Services, "Id", "Name");
            return View();
        }

        // POST: SubscriptionWelcomeMessages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,MessageText,StartTime,EndTime,StartDate,EndDate,Priority,ServiceId")] SubscriptionWelcomeMessage subscriptionWelcomeMessage)
        {
            if (ModelState.IsValid)
            {
                db.SubscriptionWelcomeMessages.Add(subscriptionWelcomeMessage);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ServiceId = new SelectList(db.Services, "Id", "Name", subscriptionWelcomeMessage.ServiceId);
            return View(subscriptionWelcomeMessage);
        }

        // GET: SubscriptionWelcomeMessages/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubscriptionWelcomeMessage subscriptionWelcomeMessage = db.SubscriptionWelcomeMessages.Find(id);
            if (subscriptionWelcomeMessage == null)
            {
                return HttpNotFound();
            }
            ViewBag.ServiceId = new SelectList(db.Services, "Id", "Name", subscriptionWelcomeMessage.ServiceId);
            return View(subscriptionWelcomeMessage);
        }

        // POST: SubscriptionWelcomeMessages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,MessageText,StartTime,EndTime,StartDate,EndDate,Priority,ServiceId")] SubscriptionWelcomeMessage subscriptionWelcomeMessage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(subscriptionWelcomeMessage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ServiceId = new SelectList(db.Services, "Id", "Name", subscriptionWelcomeMessage.ServiceId);
            return View(subscriptionWelcomeMessage);
        }

        // GET: SubscriptionWelcomeMessages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubscriptionWelcomeMessage subscriptionWelcomeMessage = db.SubscriptionWelcomeMessages.Find(id);
            if (subscriptionWelcomeMessage == null)
            {
                return HttpNotFound();
            }
            return View(subscriptionWelcomeMessage);
        }

        // POST: SubscriptionWelcomeMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SubscriptionWelcomeMessage subscriptionWelcomeMessage = db.SubscriptionWelcomeMessages.Find(id);
            db.SubscriptionWelcomeMessages.Remove(subscriptionWelcomeMessage);
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
