using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MessageSender.Models
{
    public class OutboundMessage
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string Destination { get; set; }

        public string Text { get; set; }

        [StringLength(16)]
        public string Sender { get; set; }

        [StringLength(50)]
        public string ServiceId { get; set; }

        [StringLength(100)]
        public string LinkId { get; set; }
        
        public DateTime Timestamp { get; set; }

    }
}