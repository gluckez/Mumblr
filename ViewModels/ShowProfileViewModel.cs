using Microsoft.AspNetCore.Mvc;
using Mumblr.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Mumblr.ViewModels
{
    public class ShowProfileViewModel
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string PhotoPath { get; set; }

        public string Bio { get; set; }

        public IEnumerable<Post> Feed { get; set; }

        public Post Post { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string PostMessage { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string PostTitle { get; set; }

    }
}
