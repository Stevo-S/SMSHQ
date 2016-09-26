using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MessageSender.Models
{
    public class ShortCode
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string Code { get; set; }

        public bool Activated { get; set; }

        public virtual ICollection<Service> Services { get; set; }
    }
}