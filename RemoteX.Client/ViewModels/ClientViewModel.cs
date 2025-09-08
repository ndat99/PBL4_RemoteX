using RemoteX.Client.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RemoteX.Client.ViewModels
{
    public class ClientViewModel
    {
        public InfoViewModel InfoVM { get; set; }
        public ChatViewModel ChatVM { get; set; }

        public ClientViewModel(ClientController client)
        {
            InfoVM = new InfoViewModel();
            ChatVM = new ChatViewModel(client);

            client.ClientConnected += info =>
            {
                InfoVM.ClientInfo = info;
                InfoVM.StatusText = " ⬤  Đã kết nối tới Server";
                InfoVM.StatusColor = Brushes.Green;
            };
        }
    }
}
