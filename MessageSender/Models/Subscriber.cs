using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MessageSender.Models
{
    public class Subscriber
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(16)]
        public string ServiceId { get; set; }

        public bool isActive { get; set; }

        public DateTime FirstSubscriptionDate { get; set; }
        public DateTime LastSubscriptionDate { get; set; }
        public DateTime? LastUnsubscriptionDate { get; set; }
    }
}