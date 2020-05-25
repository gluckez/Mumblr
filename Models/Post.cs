using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Mumblr.Models
{
    public class Post
    {
        public int PostId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public string Title { get; set; }

        public string UserId { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

    }
}
