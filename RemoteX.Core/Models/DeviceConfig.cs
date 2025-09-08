using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RemoteX.Core.Models
{
    public class DeviceConfig
    {
        [JsonPropertyName("DeviceID")]
        public string DeviceID { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }
        [JsonPropertyName("MachineName")]
        public string MachineName { get; set; }
    }
}
