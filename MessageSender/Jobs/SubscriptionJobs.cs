using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Xml.Linq;
using Hangfire;
using MessageSender.Jobs;
using MessageSender.Models;
using MessageSender.SMS;

/// <summary>
/// Define tasks to be run in the background
/// that update subscriber information
/// </summary>
public class SubscriptonJobs
{
    public SubscriptonJobs()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static void ProcessSyncOrder(XElement soapEnvelope)
    {
        XNamespace ns1 = SMSConfiguration.SOAPRequestNamespaces["ns1"];
        XNamespace loc = SMSConfiguration.SOAPRequestNamespaces["locSync"];
        XNamespace v2 = SMSConfiguration.SOAPRequestNamespaces["v2"];

        using (var db = new ApplicationDbContext())
        {
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

            string transactionId = "", objectTypestring = "", traceUniqueId = "",
                rentSuccess = "", orderKey = "", mdspSubExpModeString = "", keyword = "";

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

                    case "keyword":
                        keyword = (el.NextNode as XElement).Value;
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
                Keyword = keyword,
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
                    LastSubscriptionDate = DateTime.Now,
                    ProductId = syncOrder.ProductId
                };
                db.Subscribers.Add(subscriber);
            }

            if (syncOrder.UpdateDescription.Equals("Addition"))
            {
                subscriber.isActive = true;
                subscriber.LastSubscriptionDate = DateTime.Now;
                var service = db.Services.Where(s => s.ServiceId.Equals(syncOrder.ServiceId)).Include(s => s.ShortCode).FirstOrDefault();
                if (service != null)
                {
                    //var subscriptionResponse = new SMSMessage
                    //{
                    //    Correlator = "S" + DateTime.Now.ToString("yyyymmddHHMMss"),
                    //    Destination = subscriber.PhoneNumber,
                    //    Sender = service.ShortCode.Code,
                    //    ServiceId = syncOrder.ServiceId,
                    //    Text = service.GetSubscriptionWelcomeMessage()
                    //};

                    //subscriptionResponse.Send();

                    var outboundMessage = new OutboundMessage
                    {
                        Destination = subscriber.PhoneNumber,
                        ServiceId = syncOrder.ServiceId,
                        Text = service.GetSubscriptionWelcomeMessage(),
                        Sender = service.ShortCode.Code,
                        Timestamp = DateTime.Now
                    };

                    db.OutboundMessages.Add(outboundMessage);
                    db.SaveChanges();
                    // Wait the specified number of seconds in order to send Subscription welcome message
                    // This so as to allow enough time for the provider to register the sender as subscribed
                    // once this controller action returns the result
                    int secondsToWait = 2;
                    BackgroundJob.Schedule(() => MessageJobs.SendSubscriptionWelcomeMessage(outboundMessage.Id),
                        DateTime.Now.AddSeconds(secondsToWait));
                }

            }
            else if (syncOrder.UpdateDescription.Equals("Deletion"))
            {
                subscriber.isActive = false;
                subscriber.LastUnsubscriptionDate = DateTime.Now;
            }

            db.SaveChanges();
        }

    }
}