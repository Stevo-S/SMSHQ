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
            XNamespace ns1 = SMSConfiguration.SOAPRequestNamespaces["ns1"];
            XNamespace loc = SMSConfiguration.SOAPRequestNamespaces["locSync"];
            XNamespace v2 = SMSConfiguration.SOAPRequestNamespaces["v2"];

            string notificationSoapString = Request.Content.ReadAsStringAsync().Result;
            XElement soapEnvelope = XElement.Parse(notificationSoapString);

            // Read content from XML sent by service provider
            string userId = (string)
                                (from el in soapEnvelope.Descendants("ID")
                                 select el).First();

            int userType = (int)
                                (from el in soapEnvelope.Descendants("type")
                                 select el).First();

            string productId = (string)
                                (from el in soapEnvelope.Descendants(loc + "productID")
                                 select el).First();

            string serviceId = (string)
                                (from el in soapEnvelope.Descendants(loc + "serviceID")
                                 select el).First();

            string servicesList = (string)
                                (from el in soapEnvelope.Descendants(loc + "serviceList")
                                 select el).First();

            int updateType = (int)
                                (from el in soapEnvelope.Descendants(loc + "updateType")
                                 select el).First();

            string updateTime = (string)
                                (from el in soapEnvelope.Descendants(loc + "updateTime")
                                 select el).First();

            string updateDescription = (string)
                                (from el in soapEnvelope.Descendants(loc + "updateDesc")
                                 select el).First();

            string effectiveTime = (string)
                                (from el in soapEnvelope.Descendants(loc + "effectiveTime")
                                 select el).First();

            string expiryTime = (string)
                                (from el in soapEnvelope.Descendants(loc + "expiryTime")
                                 select el).First();

            IEnumerable<XElement> extensionInfo = from el in soapEnvelope.Descendants("key")
                                                  select el;

            string transactionId = "", objectTypestring = "", traceUniqueId = "", rentSuccess = "", orderKey = "", mdspSubExpModeString = "";

            foreach (var el in extensionInfo)
            {
                var a = el.NextNode;

                switch (el.Value)
                {
                    case "TransactionID":
                        transactionId = (el.NextNode as XElement).Value;
                        break;

                    case "orderKey":
                        orderKey = (el.NextNode as XElement).Value;
                        break;

                    case "MDSPSUBEXPMODE":
                        mdspSubExpModeString = (el.NextNode as XElement).Value;
                        break;

                    case "objectType":
                        objectTypestring = (el.NextNode as XElement).Value;
                        break;

                    case "traceUniqueId":
                        traceUniqueId = (el.NextNode as XElement).Value;
                        break;

                    case "rentSuccess":
                        rentSuccess = (el.NextNode as XElement).Value;
                        break;

                    default:
                        break;
                }
            }

            int mdspSubExpMode, objectType;
            Int32.TryParse(mdspSubExpModeString, out mdspSubExpMode);
            Int32.TryParse(objectTypestring, out objectType);

            // Create SyncOrder Record
            var syncOrder = new SyncOrder
            {
                UserId = userId,
                UserType = userType,
                EffectiveTime = DateTime.ParseExact(effectiveTime, "yyyyMMddHHmmss", null),
                ExpiryTime = DateTime.ParseExact(effectiveTime, "yyyyMMddHHmmss", null),
                MDSPSUBEXPMODE = mdspSubExpMode,
                ObjectType = objectType,
                OrderKey = orderKey,
                ProductId = productId,
                ServiceId = serviceId,
                RentSuccess = rentSuccess.Equals("true") ? true : false,
                ServicesList = servicesList,
                Timestamp = DateTime.Now,
                TraceUniqueId = traceUniqueId,
                TransactionId = transactionId,
                UpdateDescription = updateDescription,
                UpdateType = updateType,
                UpdateTime = DateTime.ParseExact(effectiveTime, "yyyyMMddHHmmss", null)
            };

            db.SyncOrders.Add(syncOrder);

            // Create/Update Subscriber record upon subscription
            var subscriber = db.Subscribers.Where(s => s.PhoneNumber.Equals(syncOrder.UserId) && s.ServiceId.Equals(syncOrder.ServiceId)).FirstOrDefault();
            if (subscriber == null) // Create subscriber if they do not exist
            {
                subscriber = new Subscriber
                {
                    PhoneNumber = syncOrder.UserId,
                    ServiceId = syncOrder.ServiceId,
                    FirstSubscriptionDate = DateTime.Now,
                    LastSubscriptionDate = DateTime.Now
                };
                db.Subscribers.Add(subscriber);
            }

            if (syncOrder.UpdateDescription.Equals("Addition"))
            {
                subscriber.isActive = true;
                subscriber.LastSubscriptionDate = DateTime.Now;
            }
            else if (syncOrder.UpdateDescription.Equals("Deletion"))
            {
                subscriber.isActive = false;
                subscriber.LastUnsubscriptionDate = DateTime.Now;
            }

            db.SaveChanges();

            string responseMessage = "<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:loc='http://www.csapi.org/schema/parlayx/data/sync/v1_0/local'>"
                               + "<soapenv:Header/>"
                                + "<soapenv:Body>"
                                    + "<loc:syncOrderRelationResponse>"
                                        + "< loc:result>0</loc:result>"
                                        + "<loc:resultDescription>OK</loc:resultDescription>"
                                    + "</loc:syncOrderRelationResponse>"
                                + "</soapenv:Body>"
                                + "</soapenv:Envelope>";

            // Add Response message to OK status response
            return Ok(responseMessage);
        }

        // DELETE: api/SyncOrders/5
        [ResponseType(typeof(SyncOrder))]
        public IHttpActionResult DeleteSyncOrder(int id)
        {
            SyncOrder syncOrder = db.SyncOrders.Find(id);
            if (syncOrder == null)
            {
                return NotFound();
            }

            db.SyncOrders.Remove(syncOrder);
            db.SaveChanges();

            return Ok(syncOrder);
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