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
    public class MassMessage
    {
        public string Text { get; set; }
        public string Correlator { get; set; }
        public string TimeStamp { get; set; }
        public string Sender { get; set; }
        public string ServiceId { get; set; }

        public List<string> Destinations { get; set; }

        public string Send()
        {
            using (var handler = new HttpClientHandler() { Credentials = new NetworkCredential(SMSConfiguration.GetUsername(), SMSConfiguration.HashPassword(SMSConfiguration.GetSpID() + SMSConfiguration.GetPassword() + TimeStamp)) })
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(SMSConfiguration.GetRemoteSMSServiceURI());
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



                //try
                //{
                //    var result = client.SendAsync(request).Result;
                //    resultContent = result.Content.ReadAsStringAsync().Result;
                //}
                //catch (Exception ex)
                //{
                //    Elmah.ErrorLog.GetDefault(null).Log(new Error(ex));
                //}

                // Log request and response
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
        }

        public string ToXML()
        {
            return buildSMSXML();
        }

        private string buildSMSXML()
        {
            //string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
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
                            new XElement(v2 + "spPassword", SMSConfiguration.HashPassword(SMSConfiguration.GetSpID() + SMSConfiguration.GetPassword() + TimeStamp)),
                            new XElement(v2 + "serviceId", ServiceId),
                            new XElement(v2 + "timeStamp", TimeStamp)//,
                                                                     //new XElement(v2 + "linkid"),
                                                                     //new XElement(v2 + "OA", "tel:" + message.GetDestination()),
                                                                     //new XElement(v2 + "FA", "tel:" + message.GetDestination())
                            ) // End of RequestSOAPHeader
                        ), // End of Header
                    new XElement(soapenv + "Body",
                        new XElement(loc + "sendSms",
                            //new XElement(loc + "addresses", "tel:" + Destination),
                            from destination in Destinations select new XElement(loc + "addresses", "tel:" + destination),
                            new XElement(loc + "senderName", Sender),
                            new XElement(loc + "message", Text),
                            new XElement(loc + "receiptRequest",
                                new XElement("endpoint", "http://" + SMSConfiguration.GetHostPPPAddress() + "/SMSHQ/Deliveries/Receive"),
                                new XElement("interfaceName", "SmsNotification"),
                                new XElement("correlator", Correlator)
                            ) // End of receiptRequest
                        ) // End of sendSms
                    ) // End of Soap Body
                ); // End of Soap Envelope

            return soapEnvelope.ToString();
        }
    }
}