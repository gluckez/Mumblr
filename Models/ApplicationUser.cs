using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mumblr.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Bio { get; set; }
        public int Age { get; set; }
        public string City { get; set; }
        public string PhotoPath { get; set; }
        
    }
}
