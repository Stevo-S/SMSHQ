using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MessageSender.Models
{
    public class SubscriptionWelcomeMessage
    {
        public static int DefaultPriority = 5;

        public int Id { get; set; }

        [StringLength(1024)]
        [DataType(DataType.MultilineText)]
        public string MessageText { get; set; }

        [StringLength(8)]
        public string StartTime { get; set; }

        [StringLength(8)]
        public string EndTime { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int Priority { get; set; }

        public int ServiceId { get; set; }

        public virtual Service Service { get; set; }
    }
}