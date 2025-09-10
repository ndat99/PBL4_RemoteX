using RemoteX.Client.Controllers;
using RemoteX.Core.Models;
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

        public readonly ClientController _clientController;
        public string PartnerId { get; set; }

        public ClientViewModel(ClientController clientController)
        {
            _clientController = clientController;
            InfoVM = new InfoViewModel();
            ChatVM = new ChatViewModel(_clientController);

            _clientController.ClientConnected += info =>
            {
                InfoVM.ClientInfo = info;
                InfoVM.StatusText = " ⬤  Đã kết nối tới Server";
                InfoVM.StatusColor = System.Windows.Media.Brushes.Green;
            };

            _clientController.ConnectRequestReceived += request =>
            {
                if (string.IsNullOrEmpty(request.Status))
                {
                    PartnerId = request.From;
                    InfoVM.StatusText = $" ⬤  Có người kết nối: {request.From}";
                    InfoVM.StatusColor = System.Windows.Media.Brushes.AliceBlue;
                }
                else
                {
                    InfoVM.StatusText = request.Status;
                    InfoVM.StatusColor = System.Windows.Media.Brushes.Red;

                    if (!request.Status.Contains("❌"))
                    {
                        StartStreaming();
                    }
                }
            };

            //Chat message nhận về
            _clientController.ChatMessageReceived += chat =>
            {
                //ChatVM.AddMessage(chat);
            };
        }

        public Task SendConnectRquestAsync(string targetId, string password)
        {
            if (string.IsNullOrEmpty(targetId)) return Task.CompletedTask;

            var request = new ConnectRequest
            {
                From = _clientController.ClientId,
                To = targetId,
                Password = password,
                Status = null
            };
            return _clientController.SendAsync(request);
        }

        public Task SendChatAsync(string text)
        {
            if (string.IsNullOrEmpty(PartnerId)) return Task.CompletedTask;

            var msg = new ChatMessage
            {
                From = InfoVM.ClientInfo.Id,
                To = PartnerId,
                Message = text,
                Timestamp = DateTime.Now,
            };
            return _clientController.SendAsync(msg);
        }
    }
}
