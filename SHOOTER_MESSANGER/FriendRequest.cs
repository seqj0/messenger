using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHOOTER_MESSANGER
{
    public class FriendRequest
    {
        public int RequesterId { get; set; }
        public int ReceiverId { get; set; }
        public string RequesterName { get; set; }  // Новый параметр
        public string ReceiverName { get; set; }   // Новый параметр
        public string Status { get; set; }
    }

}
