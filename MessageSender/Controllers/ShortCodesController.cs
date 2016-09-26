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
    public class ShortCodesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ShortCodes
        public ActionResult Index()
        {
            return View(db.ShortCodes.ToList());
        }

        // GET: ShortCodes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShortCode shortCode = db.ShortCodes.Find(id);
            if (shortCode == null)
            {
                return HttpNotFound();
            }
            return View(shortCode);
        }

        // GET: ShortCodes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ShortCodes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,Activated")] ShortCode shortCode)
        {
            if (ModelState.IsValid)
            {
                db.ShortCodes.Add(shortCode);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(shortCode);
        }

        // GET: ShortCodes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShortCode shortCode = db.ShortCodes.Find(id);
            if (shortCode == null)
            {
                return HttpNotFound();
            }
            return View(shortCode);
        }

        // POST: ShortCodes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Code,Activated")] ShortCode shortCode)
        {
            if (ModelState.IsValid)
            {
                db.Entry(shortCode).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(shortCode);
        }

        // GET: ShortCodes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShortCode shortCode = db.ShortCodes.Find(id);
            if (shortCode == null)
            {
                return HttpNotFound();
            }
            return View(shortCode);
        }

        // POST: ShortCodes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ShortCode shortCode = db.ShortCodes.Find(id);
            db.ShortCodes.Remove(shortCode);
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
