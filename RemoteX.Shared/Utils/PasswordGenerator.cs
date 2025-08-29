using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Shared.Utils
{
    public static class PasswordGenerator
    {
        public static string GenerateRandomPassword()
        {
            Random rnd = new Random();
            return rnd.Next(1000, 9999).ToString();
        }
    }
}
