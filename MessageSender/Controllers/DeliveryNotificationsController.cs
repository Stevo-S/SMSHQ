using MessageSender.Models;
using MessageSender.SMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;

namespace MessageSender.Controllers
{
    public class DeliveryNotificationsController : ApiController
    {
        [Route("Deliveries/Receive")]
        [HttpPost]
        public IHttpActionResult ReceiveDeliveryNotification()
        {
            XNamespace ns1 = SMSConfiguration.SOAPRequestNamespaces["ns1"];
            XNamespace ns2 = SMSConfiguration.SOAPRequestNamespaces["ns2"];
            XNamespace loc = SMSConfiguration.SOAPRequestNamespaces["locNotification"];
            XNamespace v2 = SMSConfiguration.SOAPRequestNamespaces["v2"];

            string notificationSoapString = Request.Content.ReadAsStringAsync().Result;
            XElement soapEnvelope = XElement.Parse(notificationSoapString);

            string destination = (string)
                                    (from el in soapEnvelope.Descendants("address")
                                     select el).First();
            destination = destination.Substring(4);
            
            string deliveryStatus = (string)
                                        (from el in soapEnvelope.Descendants("deliveryStatus")
                                         select el).First();

            string serviceId = (string)
                                    (from el in soapEnvelope.Descendants(ns1 + "serviceId")
                                     select el).First();

            string correlatorString = (string)
                                (from el in soapEnvelope.Descendants(ns2 + "correlator")
                                 select el).First();
            
            string traceUniqueId = (string)
                                        (from el in soapEnvelope.Descendants(ns1 + "traceUniqueID")
                                         select el).First();

            using (var db = new ApplicationDbContext())
            {

                var deliveryNotification = new Delivery()
                {
                    Destination = destination,
                    DeliveryStatus = deliveryStatus,
                    ServiceId = serviceId,
                    Correlator = correlatorString,
                    TimeStamp = DateTime.Now,
                    TraceUniqueId = traceUniqueId
                };

                
                db.Deliveries.Add(deliveryNotification);
                db.SaveChanges();
                
                return Ok();
            }
        }
    }
}
