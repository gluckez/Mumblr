using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Mumblr.Models
{
    public class FriendSystem
    {
        public int id { get; set; }

        public string UserId { get; set; }

        public string FriendId { get; set; }

    }
}
