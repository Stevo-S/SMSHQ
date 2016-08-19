using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MessageSender.Models
{
    public class BatchMessage
    {
        public int Id { get; set; }

        [DataType(DataType.MultilineText)]
        public string MessageContent { get; set; }

        public string ServiceId { get; set; }
        public string Sender { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public virtual ICollection<BatchMessageRecipient> Recipients { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            // StartTime should be some time in the future
            if (StartTime < DateTime.Now)
            {
                yield return new
                    ValidationResult("The Start time should be sometime in the future.");
            }

            // Expiration date should be greater than SendTime
            TimeSpan interval = EndTime - StartTime;
            if (interval.Hours < 2)
            {
                yield return new
                    ValidationResult("The expiration should be at least two hours after the send time");
            }

        }
    }
}