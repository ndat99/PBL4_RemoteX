using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Shared.Utils
{
    public class PasswordGenerator
    {
        public static string GeneratePassword()
        {
            Random rand = new Random();
            int randomNumber = rand.Next(100000, 999999);
            return randomNumber.ToString();
        }
    }
}
