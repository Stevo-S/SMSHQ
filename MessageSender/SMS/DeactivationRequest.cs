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
    public class DeactivationRequest
    {
        Subscriber subscriber;
        private static DateTime TimeStamp = DateTime.Now;
        private static HttpClientHandler handler = new HttpClientHandler { Credentials = new NetworkCredential(SMSConfiguration.GetUsername(), SMSConfiguration.HashPassword(SMSConfiguration.GetSpID() + SMSConfiguration.GetPassword() + timestampString())) };
        private static HttpClient client = new HttpClient(handler)
        {
            BaseAddress = new Uri(SMSConfiguration.GetRemoteSMSServiceURI())
        };
    
        public DeactivationRequest(Subscriber subscriber)
        {
            this.subscriber = subscriber;
        }

        public string Send()
        {
            string requestContentString = "";
            string resultContent = "";

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/SubscribeManageService/services/SubscribeManage/");
                request.Content = new StringContent(BuildXMLRequest(), Encoding.UTF8, "text/xml");
                requestContentString = request.Content.ReadAsStringAsync().Result;
                var result = client.SendAsync(request).Result;
                resultContent = result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                Elmah.ErrorLog.GetDefault(null).Log(new Error(ex));
            }

            return resultContent;
        }

        private string BuildXMLRequest()
        {
            XNamespace soapenv = SMSConfiguration.SOAPRequestNamespaces["soapenv"];
            XNamespace loc = SMSConfiguration.SOAPRequestNamespaces["locSubscribe"];
            XNamespace tns = SMSConfiguration.SOAPRequestNamespaces["tns"];

            XElement soapEnvelope =
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "loc", loc.NamespaceName),
                    new XElement(soapenv + "Header",
                        new XElement(tns + "RequestSOAPHeader",
                            new XAttribute(XNamespace.Xmlns + "tns", tns.NamespaceName),
                            new XElement(tns + "spId", SMSConfiguration.GetSpID()),
                            new XElement(tns + "spPassword", SMSConfiguration.HashPassword(SMSConfiguration.GetSpID() + SMSConfiguration.GetPassword() + timestampString())),
                            new XElement(tns + "timeStamp", timestampString())
                            ) // End of RequestSOAPHeader
                        ), // End of Header
                    new XElement(soapenv + "Body",
                        new XElement(loc + "unSubscribeProductRequest",
                            new XElement(loc + "unSubscribeProductReq",
                                new XElement("userID",
                                    new XElement("ID", subscriber.PhoneNumber),
                                    new XElement("type", 0)
                                ), // End of UserID
                                new XElement("subInfo",
                                    new XElement("productID", subscriber.ProductId),
                                    new XElement("channelID", 1)
                                ) // End of subInfo
                            ) // End of unSubscribeProductReq
                        ) // End of unSubscribeProductRequest
                    ) // End of Soap Body
                ); // End of Soap Envelope

            return soapEnvelope.ToString();
        }

        private static string timestampString()
        {
            return TimeStamp.ToString("yyyyMMddHHmmss");
        }
    }
}