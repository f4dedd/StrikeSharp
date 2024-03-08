using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Bot.Database
{
    internal class UserInfo
    {
        public ulong Id { get; set; }

        public int TotalStrikes { get; set; }

        public DateTime? StrikeReset { get; set; }

        public int CurrentStrikes { get; set; }

    }
}
