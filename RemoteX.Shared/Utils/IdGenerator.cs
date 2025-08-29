using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Shared.Utils
{
    public static class IdGenerator
    {
        public static string GenerateRandomId()
        {
            Random rnd = new Random();
            return rnd.Next(100000000, 999999999).ToString();
        }
    }
}
