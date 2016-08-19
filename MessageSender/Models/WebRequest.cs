using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MessageSender.Models
{
    public class WebRequest
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public DateTime Timestamp { get; set; }
    }
}