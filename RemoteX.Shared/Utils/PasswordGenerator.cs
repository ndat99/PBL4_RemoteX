using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Shared.Utils
{
    public static class PasswordGenerator
    {
        public static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
        public static string GenerateRandomPassword(int length = 5)
        {
            byte[] randomNumber = new byte[1];
            char[] digits = new char[length];

            for(int i = 0; i < length; i++)
            {
                rng.GetBytes(randomNumber);
                digits[i] = (char)('0' + (randomNumber[0] % 10));
            }
            return new string(digits);
        }
    }
}
