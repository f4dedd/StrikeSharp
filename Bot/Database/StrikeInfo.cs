using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace App.Bot.Database
{
    internal class StrikeInfo
    {
        public long Id {  get; set; }
        public ulong UserId { get; set; }

        public DateTime Timestamp { get; set; }

        public ulong IssuerId { get; set; }

        public string? Reason { get; set; }
    }
}
