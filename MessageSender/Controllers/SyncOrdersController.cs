using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using MessageSender.Models;
using System.Xml.Linq;
using MessageSender.SMS;
using Hangfire;
using MessageSender.Jobs;

namespace MessageSender.Controllers
{
    public class SyncOrdersController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/SyncOrders
        public IQueryable<SyncOrder> GetSyncOrders()
        {
            return db.SyncOrders;
        }

        // GET: api/SyncOrders/5
        [ResponseType(typeof(SyncOrder))]
        public IHttpActionResult GetSyncOrder(int id)
        {
            SyncOrder syncOrder = db.SyncOrders.Find(id);
            if (syncOrder == null)
            {
                return NotFound();
            }

            return Ok(syncOrder);
        }

        // PUT: api/SyncOrders/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutSyncOrder(int id, SyncOrder syncOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != syncOrder.Id)
            {
                return BadRequest();
            }

            db.Entry(syncOrder).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SyncOrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/SyncOrders
        [ResponseType(typeof(SyncOrder))]
        public IHttpActionResult PostSyncOrder(SyncOrder syncOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.SyncOrders.Add(syncOrder);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = syncOrder.Id }, syncOrder);
        }

        [Route("SyncOrders")]
        public IHttpActionResult PostSyncOrder()
        {

            string notificationSoapString = Request.Content.ReadAsStringAsync().Result;
            XElement soapEnvelope = XElement.Parse(notificationSoapString);
            BackgroundJob.Enqueue(() => SubscriptonJobs.ProcessSyncOrder(soapEnvelope));
            
            return Ok(SyncOrderResponse());
        }

        // SyncOrderRespose
        private XElement SyncOrderResponse()
        {
            XNamespace soapenv = SMSConfiguration.SOAPRequestNamespaces["soapenv"];
            XNamespace loc = SMSConfiguration.SOAPRequestNamespaces["locSync"];

            XElement soapEnvelope =
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "loc", loc.NamespaceName),
                    new XElement(soapenv + "Header"), // End of Header
                    new XElement(soapenv + "Body",
                        new XElement(loc + "syncOrderRelationResponse",
                            new XElement(loc + "result", 0),
                            new XElement(loc + "resultDescription", "OK")
                        ) // End of syncOrderRelationResponse
                    ) // End of Soap Body
                ); // End of Soap Envelope

            return soapEnvelope;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SyncOrderExists(int id)
        {
            return db.SyncOrders.Count(e => e.Id == id) > 0;
        }
    }
}