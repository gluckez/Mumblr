using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Mumblr.Models
{
    public class Messages
    {
        public int MessagesId { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime Date { get; set; }

        [Required]
        public string sender { get; set; }

        [Required]
        public string recipient { get; set; }

        public bool isRead { get; set; }
    }
}
