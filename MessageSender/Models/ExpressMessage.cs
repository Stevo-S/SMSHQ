using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MessageSender.Models
{
    public class ExpressMessage
    {
        public int Id { get; set; }

        [StringLength(8)]
        public string ShortCode { get; set; }

        [StringLength(1024)]
        public string Message { get; set; }

        [StringLength(16)]
        public string Destination { get; set; }

        [StringLength(32)]
        public string ServiceId { get; set; }

        public DateTime DateScheduled { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}