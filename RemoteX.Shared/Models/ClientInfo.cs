using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteX.Shared.Utils;

namespace RemoteX.Shared.Models
{
    //Định nghĩa client model chung cho server và client
    public class ClientInfo
    {
        public string Id { get; set; } //ID client
        public string Password { get; set; }
        public ClientInfo()
        {
            Id = IdGenerator.GetMacAddress();
            Password = PasswordGenerator.GenerateRandomPassword();
        }
    }
}
