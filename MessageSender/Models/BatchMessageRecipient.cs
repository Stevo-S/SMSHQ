using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MessageSender.Models
{
    public class BatchMessageRecipient
    {
        public int Id { get; set; }
        public string Destination { get; set; }
        public DateTime Timestamp { get; set; }
        public virtual BatchMessage Message { get; set; }
    }
}