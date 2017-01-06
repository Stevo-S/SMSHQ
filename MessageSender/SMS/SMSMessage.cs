using Elmah;
using MessageSender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace MessageSender.SMS
{
    public class SMSMessage
    {
        public string Destination { get; set; }
        public string Text { get; set; }
        public string Correlator { get; set; }
        public string Sender { get; set; }
        public string ServiceId { get; set; }

        private static DateTime TimeStamp = DateTime.Now;    
        private static HttpClientHandler handler = new HttpClientHandler { Credentials = new NetworkCredential(SMSConfiguration.GetUsername(), SMSConfiguration.HashPassword(SMSConfiguration.GetSpID() + SMSConfiguration.GetPassword() + timestampString())) };
        private static HttpClient client = new HttpClient(handler)
        {
            BaseAddress = new Uri(SMSConfiguration.GetRemoteSMSServiceURI())
        };

        public string ToXML()
        {
            return buildSMSXML();
        }

        private string buildSMSXML()
        {
            XNamespace soapenv = SMSConfiguration.SOAPRequestNamespaces["soapenv"];
            XNamespace v2 = SMSConfiguration.SOAPRequestNamespaces["v2"];
            XNamespace loc = SMSConfiguration.SOAPRequestNamespaces["locSend"];
            XElement soapEnvelope =
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "v2", v2.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "loc", loc.NamespaceName),
                    new XElement(soapenv + "Header",
                        new XElement(v2 + "RequestSOAPHeader",
                            new XElement(v2 + "spId", SMSConfiguration.GetSpID()),
                            new XElement(v2 + "spPassword", getHashedPassword()),
                            new XElement(v2 + "serviceId", ServiceId),
                            new XElement(v2 + "timeStamp", timestampString())//,
                                                                             //new XElement(v2 + "linkid"),
                                                                             //new XElement(v2 + "OA", "tel:" + message.GetDestination()),
                                                                             //new XElement(v2 + "FA", "tel:" + message.GetDestination())
                            ) // End of RequestSOAPHeader
                        ), // End of RequestSOAPHeader
                    new XElement(soapenv + "Body",
                        new XElement(loc + "sendSms",
                            new XElement(loc + "addresses", "tel:" + Destination),
                            new XElement(loc + "senderName", Sender),
                            new XElement(loc + "message", Text),
                            new XElement(loc + "receiptRequest",
                                new XElement("endpoint", "http://" + SMSConfiguration.GetHostPPPAddress() + "/MessageSender/SMSService/ReceiveDeliveryNotification"),
                                new XElement("interfaceName", "SmsNotification"),
                                new XElement("correlator", Correlator)
                            ) // End of receiptRequest
                        ) // End of sendSms
                    ) // End of Soap Body
                ); // End of Soap Envelope

            return soapEnvelope.ToString();
        }

        public string Send()
        {
            string requestContentString = "";
            string resultContent = "";
            do
            {
                try
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/SendSmsService/services/SendSms/");
                    request.Content = new StringContent(buildSMSXML(), Encoding.UTF8, "text/xml");
                    requestContentString = request.Content.ReadAsStringAsync().Result;
                    var result = client.SendAsync(request).Result;
                    resultContent = result.Content.ReadAsStringAsync().Result;
                }
                catch (Exception ex)
                {
                    Elmah.ErrorLog.GetDefault(null).Log(new Error(ex));
                }
            }
            while (resultContent.Contains("request rate control not pass"));

            using (var db = new ApplicationDbContext())
            {
                var webRequest = new Models.WebRequest()
                {
                    Body = requestContentString,
                    Timestamp = DateTime.Now
                };
                db.WebRequests.Add(webRequest);

                var webResponse = new Models.WebResponse()
                {
                    Body = resultContent,
                    Timestamp = DateTime.Now
                };
                db.WebResponses.Add(webResponse);

                db.SaveChanges();
            }

            return resultContent;
        }

        private static string timestampString()
        {
            return TimeStamp.ToString("yyyyMMddHHmmss");
        }

        private string getHashedPassword()
        {
            return SMSConfiguration.HashPassword(SMSConfiguration.GetSpID() + SMSConfiguration.GetPassword() + timestampString());
        }
    }
}