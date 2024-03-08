using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Bot
{
    internal class Strike
    {
        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("role-id")]
        public long RoleId { get; set; }

        [JsonProperty("mute-time")]
        public int MuteTime { get; set; }
    }
}
