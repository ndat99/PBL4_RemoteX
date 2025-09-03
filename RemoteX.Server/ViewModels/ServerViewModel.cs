using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using RemoteX.Shared.Utils;
using RemoteX.Shared.Models;
using System.Collections.ObjectModel;

namespace RemoteX.Server.ViewModels
{
    class ServerViewModel : BaseViewModel
    {
        private string _status;
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }
        public ObservableCollection<ClientInfo> Clients { get; set; }
        public ServerViewModel()
        {
            Clients = new ObservableCollection<ClientInfo>();
            Status = "Server chưa khởi động";
            //Clients.Add(new ClientInfo { Id = "123456", Password = "1234" }); //test
        }
    }
}
