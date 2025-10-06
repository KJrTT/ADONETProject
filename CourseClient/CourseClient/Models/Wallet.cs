using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseClient.Models
{
    public class Wallet
    {
        public int WalletId { get; set; }
        public double balance { get; set; }


        public User User { get; set; } = null!;
    }
}
