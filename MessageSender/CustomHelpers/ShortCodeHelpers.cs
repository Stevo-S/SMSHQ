using MessageSender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MessageSender.CustomHelpers
{
    public static class ShortCodeHelpers
    {
        static readonly List<ShortCodeService> services;

        static ShortCodeHelpers()
        {
            using(var db = new ApplicationDbContext())
            {
                services = (from shortCode in db.ShortCodes
                            join service in db.Services
                            on shortCode.Id equals service.ShortCodeId
                            select new ShortCodeService { ShortCode = shortCode.Code, ServiceName = service.Name, ServiceId = service.ServiceId }).ToList(); 
            }
        }

        public static string ShortCodeFromServiceId(this HtmlHelper helper, string serviceId)
        {
            var shortCodeService = services.Where(s => s.ServiceId.Equals(serviceId)).FirstOrDefault();
            return shortCodeService == null ? "" : shortCodeService.ShortCode;
        }
    }

    class ShortCodeService
    {
        public string ShortCode { get; set; }
        public string ServiceName { get; set; }
        public string ServiceId { get; set; }
    }
}