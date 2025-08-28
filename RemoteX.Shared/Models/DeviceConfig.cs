using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RemoteX.Shared.Models
{
    public class DeviceConfig
    {
        [JsonPropertyName("deviceID")]
        public string DeviceID { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
        [JsonPropertyName("firstRun")]
        public DateTime FirstTime { get; set; }
        [JsonPropertyName("lastRun")]
        public DateTime LastTime { get; set; }
        [JsonPropertyName("machineInfo")]
        public MachineInfo MachineInfo { get; set; }
    }

    public class MachineInfo
    {
        [JsonPropertyName("machineName")]
        public string MachineName { get; set; }
        [JsonPropertyName("osVersion")]
        public string OsVersion { get; set; }
    }
}
