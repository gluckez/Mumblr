using Mumblr.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mumblr.ViewModels
{
    public class InboxViewModel
    {
        public List<string> UserFriends { get; set; }

        public string SelectedFriend { get; set; }

        public IEnumerable<Messages> Chat { get; set; }

        public Messages NewChatMessage { get; set; }

    }
}
