using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MessageSender.Models
{
    public class Delivery
    {
        public int Id { get; set; }
        
        [StringLength(16)]
        public string Destination { get; set; }
        
        [StringLength(64)]
        public string DeliveryStatus { get; set; }

        [StringLength(32)]
        public string ServiceId { get; set; }

        [StringLength(64)]
        public string Correlator { get; set; }

        [StringLength(128)]
        public string TraceUniqueId { get; set; }
        
        public DateTime TimeStamp { get; set; }
    }
}