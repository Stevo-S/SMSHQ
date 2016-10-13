using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MessageSender.Models
{
    public class SyncOrder
    {
        public int Id { get; set; }
        
        // Phone Number
        [StringLength(20)]
        public string UserId { get; set; }

        public int UserType { get; set; }

        [StringLength(32)]
        public string ProductId { get; set; }

        [StringLength(32)]
        public string  ServiceId { get; set; }

        [StringLength(1024)]
        public string ServicesList { get; set; }

        public int UpdateType { get; set; }

        [StringLength(32)]
        public string UpdateDescription { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        public DateTime UpdateTime { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        public DateTime EffectiveTime { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}")]
        public DateTime ExpiryTime { get; set; }

        [StringLength(64)]
        public string TransactionId { get; set; }

        [StringLength(64)]
        public string OrderKey { get; set; }

        public int MDSPSUBEXPMODE { get; set; }
        public int ObjectType { get; set; }
        public bool RentSuccess { get; set; }

        [StringLength(64)]
        public string TraceUniqueId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}