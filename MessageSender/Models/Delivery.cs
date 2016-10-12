using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MessageSender.Models
{
    public class Delivery
    {
        public int Id { get; set; }
        
        [StringLength(16)]
        [Index(name: "IX_Deliveries_Destination")]
        public string Destination { get; set; }
        
        [StringLength(64)]
        [Index(name: "IX_DeliveryStatus")]
        public string DeliveryStatus { get; set; }

        [StringLength(32)]
        public string ServiceId { get; set; }

        [StringLength(64)]
        public string Correlator { get; set; }

        [StringLength(128)]
        public string TraceUniqueId { get; set; }
        
        [Index(name: "IX_Deliveries_Timestamp")]
        public DateTime TimeStamp { get; set; }
    }
}