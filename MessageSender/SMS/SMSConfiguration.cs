using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MessageSender.SMS
{
    public class SMSConfiguration
    {
        private static string username = ConfigurationManager.AppSettings["SPUserName"];
        private static string password = ConfigurationManager.AppSettings["SPPassword"];
        private static string spId = ConfigurationManager.AppSettings["SPID"];
        private static string HostPPPAddress = ConfigurationManager.AppSettings["ServerPPPIPAddress"];
        private static string RemoteSMSServiceURI = ConfigurationManager.AppSettings["RemoteSMSServiceURI"];
        private static string ServerPublicDomainName = ConfigurationManager.AppSettings["ServerPublicDomainName"];
        private static string groupMessageLimit = ConfigurationManager.AppSettings["GroupMessageLimit"];

        public static readonly Dictionary<string, string> SOAPRequestNamespaces = new Dictionary<string, string>()
        {
            { "soapenv",  "http://schemas.xmlsoap.org/soap/envelope/" },
            { "v2",  "http://www.huawei.com.cn/schema/common/v2_1" },
            { "locSend", "http://www.csapi.org/schema/parlayx/sms/send/v2_2/local" },
            { "locNotification", "http://www.csapi.org/schema/parlayx/sms/notification/v2_2/local" },
            { "locSync", "http://www.csapi.org/schema/parlayx/data/sync/v1_0/local" },
            { "ns1", "http://www.huawei.com.cn/schema/common/v2_1" },
            { "ns2", "http://www.csapi.org/schema/parlayx/sms/notification/v2_2/local" }
        };

        public static string GetUsername()
        {
            return username;
        }

        public static string GetPassword()
        {
            return password;
        }

        public static string GetSpID()
        {
            return spId;
        }

        public static string GetRemoteSMSServiceURI()
        {
            return RemoteSMSServiceURI;
        }
        
        public static string HashPassword(string input)
        {
            return string.Join("", MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input)).Select(s => s.ToString("x2")));
        }

        public static string GetServerPublicDomainName()
        {
            return ServerPublicDomainName;
        }

        public static string GetHostPPPAddress()
        {
            return HostPPPAddress;
        }

        // Read the maximum number of recipients per group message
        // from config file
        public static int GetGroupMessageLimit()
        {
            int minimum = 1;
            int maximum = 100;
            int defaultLimit = 100;                                                                                                                                                                                                                                                                                                                                                                                                                      ;
            int limit;
            if(Int32.TryParse(groupMessageLimit, out limit))
            {
                if (limit < minimum || limit > maximum)
                {
                    limit = limit > maximum ? maximum : minimum;
                }
            }
            else
            {
                limit = defaultLimit;
            }
            return limit;
        }
    }
}