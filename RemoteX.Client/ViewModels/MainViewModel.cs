using RemoteX.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Client.ViewModels
{
    public class MainViewModel
    {
        public ClientViewModel ClientVM { get; set; }
        public ChatViewModel ChatVM { get; set; }

        public MainViewModel(RemoteXClient client)
        {
            ClientVM = new ClientViewModel();
            ChatVM = new ChatViewModel(client);
        }
    }
}
