using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Shared.Utils
{
    public static class NetworkHelper
    {
        public static string GeneratePassword(int length = 5)
        {
            const string digits = "0123456789";
            var data = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
            }

            var result = new StringBuilder(length);
            foreach (var b in data)
            {
                result.Append(digits[b % digits.Length]);
            }
            return result.ToString();
        }
    }
}
