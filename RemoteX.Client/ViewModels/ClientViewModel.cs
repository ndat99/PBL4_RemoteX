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

            //Sự kiện kết nối tới server
            _clientController.ClientConnected += info =>
            {
                InfoVM.ClientInfo = info;
                InfoVM.StatusText = " ⬤  Đã kết nối tới Server";
                InfoVM.StatusColor = System.Windows.Media.Brushes.Green;
            };

            //Sự kiện nhận log về từ server
            _clientController.LogReceived += log =>
            {
                System.Diagnostics.Debug.WriteLine($"[CLIENT] Received Log: {log.Content}");

                InfoVM.StatusText = log.Content;
                if (log.Content.Contains("❌"))
                {
                    InfoVM.StatusColor = System.Windows.Media.Brushes.Red;
                    PartnerId = null;
                }
                else if (log.Content.Contains("✔"))
                {
                    InfoVM.StatusColor = System.Windows.Media.Brushes.Yellow;

                    //Lấy PartnerId từ log
                    if (log.Content.Contains("Đang kết nối tới"))
                    {
                        PartnerId = log.Content.Split(' ').Last();
                        System.Diagnostics.Debug.WriteLine($"[CLIENT] Extracted PartnerId: {PartnerId}");
                        System.Diagnostics.Debug.WriteLine($"[CLIENT] Triggering PartnerConnected event");
                        PartnerConnected?.Invoke(PartnerId); //Mở RemoteWindow
                    }
                    else if (log.Content.Contains("Đang được điều khiển"))
                    {
                        PartnerId = log.Content.Split(' ').Last();
                        //PartnerConnected?.Invoke(PartnerId); //Mở RemoteWindow
                        var cts = new CancellationTokenSource();
                        new RemoteController(_clientController).StartStreaming(PartnerId, cts.Token);
                    }
                }
                else if (log.Content.Contains("⬤"))
                {
                    InfoVM.StatusColor = System.Windows.Media.Brushes.Green;
                }
            };

            //Chat message nhận về
            _clientController.ChatMessageReceived += chat =>
            {
                ChatVM.ReceiveMessage(chat);
            };
        }

        public void SendConnectRequest(string targetId, string password)
        {
            if (string.IsNullOrEmpty(targetId)) return;

            System.Diagnostics.Debug.WriteLine($"[CONNECT] Creating ConnectRequest from {_clientController.ClientId} to {targetId}");

            var request = new ConnectRequest
            {
                From = _clientController.ClientId,
                To = targetId,
                Password = password,
                Status = null
            };
            _clientController.Send(request);
        }

        public void SendChat(string text)
        {
            if (string.IsNullOrEmpty(PartnerId)) return;

            var msg = new ChatMessage
            {
                From = InfoVM.ClientInfo.Id,
                To = PartnerId,
                Message = text,
                Timestamp = DateTime.Now,
            };
            _clientController.Send(msg);
        }
    }
}
