using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Mumblr.ViewModels
{
    public class EditUserViewModel
    {
        [Required]
        [MaxLength(25)]
        [MinLength(4, ErrorMessage = "Username must be at least 4 characters long.")]
        public string Name { get; set; }

        public int Age { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(400, ErrorMessage = "Biolength exceeded 400 characters.")]
        public string Bio { get; set; }

        public IFormFile Photo { get; set; }

    }
}
