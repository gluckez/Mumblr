using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Mumblr.Models
{
    public class PostLikes
    {
        public int PostLikesId { get; set; }

        [Required]
        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        public string UserId { get; set; }
    }
}
