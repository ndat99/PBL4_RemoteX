using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RemoteX.Shared.Models;
using System.Text.Json;

namespace RemoteX.Shared.Utils
{
    public static class IdGenerator
    {
        private const string CONFIG_FILE_PATH = "device_config.json";
        private static DeviceConfig _deviceConfig;
        public static string GetOrCreateDeviceID()
        {
            var config = LoadOrCreateConfig();
            return config.DeviceID;
        }

        public static DeviceConfig GetDeviceConfig()
        {
            return LoadOrCreateConfig();
        }

        public static DeviceConfig LoadOrCreateConfig()
        {
            if (_deviceConfig != null)
                return _deviceConfig;

            if (File.Exists(CONFIG_FILE_PATH))
            {
                try
                {
                    string json = File.ReadAllText(CONFIG_FILE_PATH);
                    _deviceConfig = JsonSerializer.Deserialize<DeviceConfig>(json);

                    _deviceConfig.Password = NetworkHelper.GeneratePassword();
                    _deviceConfig.LastTime = DateTime.Now;

                    SaveConfig(_deviceConfig);

                    return _deviceConfig;

                }
                catch (Exception e)
                {
                    return CreateNewConfig();
                }
            }
             return CreateNewConfig();
        }

        public static DeviceConfig CreateNewConfig()
        {
            _deviceConfig = new DeviceConfig
            {
                DeviceID = GenerateNumberID(9),
                Password = NetworkHelper.GeneratePassword(),
                FirstTime = DateTime.Now,
                LastTime = DateTime.Now,
                MachineInfo = new MachineInfo
                {
                    MachineName = Environment.MachineName,
                    OsVersion = Environment.OSVersion.ToString()
                }
            };

            SaveConfig(_deviceConfig);
            return _deviceConfig;
        }

        public static void SaveConfig(DeviceConfig config)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(config, options);
                File.WriteAllText(CONFIG_FILE_PATH, jsonString);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Loi khi luu cau hinh thiet bi: {e.Message}");
            }
        }

        private static string GenerateNumberID(int length)
        {
            var random = new Random();
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(random.Next(0, length));
            return sb.ToString();
        }
    }
}