using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RemoteX.Core.Models;

namespace RemoteX.Core.Utils
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
                //_deviceConfig.Password = PasswordGenerator.GenerateRandomPassword();
                _deviceConfig.Password = 12345.ToString();
                SaveConfig(_deviceConfig);
            }
            else
            {
                _deviceConfig = new DeviceConfig
                {
                    DeviceID = GetMacAddress(),
                    //Password = PasswordGenerator.GenerateRandomPassword(),
                    Password = 12345.ToString(),
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




        //Dùng để debug thôi, sau này xóa
        public static DeviceConfig RandomDeviceConfig()
        {
            _deviceConfig = new DeviceConfig
            {
                DeviceID = GenerateRandomId(),
                //Password = PasswordGenerator.GenerateRandomPassword(),
                Password = 12345.ToString(),
                MachineName = RandomMachineName()
            };
            SaveConfig(_deviceConfig);

            return _deviceConfig;
        }

        public static string GenerateRandomId()
        {
            byte[] randomBytes = new byte[6];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        public static string RandomMachineName()
        {
            return "RemoteX-" + GenerateRandomId().Substring(0, 2);
        }
    }
}
