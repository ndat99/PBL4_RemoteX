using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RemoteX.Shared.Models;

namespace RemoteX.Shared.Utils
{
    public static class IdGenerator
    {

        private const string CONFIG_FILE_PATH = "device_config.json";
        private static DeviceConfig _deviceConfig;

        public static DeviceConfig DeviceConfig()
        {
            if(_deviceConfig != null) 
                return _deviceConfig;
            if (File.Exists(CONFIG_FILE_PATH))
            {
                string json = File.ReadAllText(CONFIG_FILE_PATH);
                _deviceConfig = JsonSerializer.Deserialize<DeviceConfig>(json);

                _deviceConfig.DeviceID = IdGenerator.GetMacAddress();
                _deviceConfig.Password = PasswordGenerator.GenerateRandomPassword();
                SaveConfig(_deviceConfig);
            }
            else
            {
                _deviceConfig = new DeviceConfig
                {
                    DeviceID = GetMacAddress(),
                    Password = PasswordGenerator.GenerateRandomPassword(),
                    MachineName = Environment.MachineName
                };
                SaveConfig(_deviceConfig);
            }

            return _deviceConfig;
        }
        public static string GetMacAddress()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var ni in interfaces)
            {
                if(ni.OperationalStatus == OperationalStatus.Up 
                    && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var mac = ni.GetPhysicalAddress().ToString();
                    if(!string.IsNullOrEmpty(mac) )
                        return mac;
                }
            }
            return "UNKNOWN";
        }

        public static void SaveConfig(DeviceConfig config)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(config, options);
                File.WriteAllText(CONFIG_FILE_PATH, jsonString);
            }
            catch (Exception ex){
                Console.WriteLine($"Loi khi luu cau hinh: {ex.Message}");
            }
        }
    }
}
