using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Shared.Utils
{
    public class IdGenerator
    {
        public static string GenerateId()
        {
            Random rand = new Random();
            int randomNumber = rand.Next(100000000, 999999999);
            return randomNumber.ToString();
        }
    }
}
