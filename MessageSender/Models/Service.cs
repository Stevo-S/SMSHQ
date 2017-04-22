using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MessageSender.Models
{
    public class Service
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string Name { get; set; }

        [Index(IsUnique = true)]
        [StringLength(32)]
        public string ServiceId { get; set; }

        [StringLength(16)]
        public string SmsNotificationCorrelator { get; set; }

        [StringLength(16)]
        public string SmsNotificationCriteria { get; set; }

        public int ShortCodeId { get; set; }

        public virtual ShortCode ShortCode { get; set; }

        public virtual ICollection<SubscriptionWelcomeMessage> SubscriptionWelcomeMessages { get; set; }

        public string GetSubscriptionWelcomeMessage()
        {
            var welcomeMessage = this.SubscriptionWelcomeMessages
                .Where(swm => swm.StartDate < DateTime.Now 
                    && TimeSpan.Parse(swm.StartTime) < DateTime.Now.TimeOfDay
                    && TimeSpan.Parse(swm.EndTime) > DateTime.Now.TimeOfDay
                    && ((swm.EndDate > DateTime.Now) || (swm.EndDate == null))) 
                .OrderByDescending(swm => swm.Priority).FirstOrDefault();
            
            if (welcomeMessage == null)
            {
                welcomeMessage = this.SubscriptionWelcomeMessages.Where(swm => swm.Priority == SubscriptionWelcomeMessage.DefaultPriority).FirstOrDefault();
            }

            return welcomeMessage.MessageText;
        }
    }
}