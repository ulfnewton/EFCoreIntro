using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCoreIntro.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        // navigations-property
        public ICollection<Message> Messages { get; set; }
        public ICollection<Membership> Memberships { get; set; }
    }
}
