using RemoteX.Client.Controllers;
using RemoteX.Core.Models;

namespace RemoteX.Client.ViewModels
{
    public class ClientViewModel
    {
        public InfoViewModel InfoVM { get; set; }
        public ChatViewModel ChatVM { get; set; }
        public event Action<string> PartnerConnected;

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

            _clientController.LogReceived += log =>
            {
                InfoVM.StatusText = log.Content;
                if (log.Content.Contains("❌"))
                {
                    InfoVM.StatusColor = System.Windows.Media.Brushes.Red;
                    PartnerId = null;
                }
                else if (log.Content.Contains("✅"))
                {
                    InfoVM.StatusColor = System.Windows.Media.Brushes.Green;

                    //Lấy PartnerId từ log
                    if (log.Content.Contains("Đang"))
                    {
                        PartnerId = log.Content.Split(' ').Last();

                        PartnerConnected?.Invoke(PartnerId);
                    }
                }
            };

            //Chat message nhận về
            _clientController.ChatMessageReceived += chat =>
            {
                //ChatVM.AddMessage(chat);
            };
        }

        public Task SendConnectRequestAsync(string targetId, string password)
        {
            if (string.IsNullOrEmpty(targetId)) return Task.CompletedTask;

            System.Diagnostics.Debug.WriteLine($"[CONNECT] Creating ConnectRequest from {_clientController.ClientId} to {targetId}");

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
