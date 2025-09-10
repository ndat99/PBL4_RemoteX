using RemoteX.Client.Controllers;
using RemoteX.Client.Services;
using RemoteX.Core.Models;
using RemoteX.Core.Utils;

using System.Windows.Media.Imaging;

namespace RemoteX.Client.ViewModels
{
    public class RemoteViewModel : BaseViewModel
    {
        private BitmapImage _screen;
        private readonly ClientController _clientController;
        private readonly RemoteController _remoteController;
        public ClientController clientController => _clientController;

        public BitmapImage ScreenView
        {
            get => _screen;
            set 
            {
                _screen = value;
               OnPropertyChanged(nameof(ScreenView)); 
            }
        }
        public string PartnerId { get; set; }

        public RemoteViewModel(ClientController clientController, string partnerId)
        {
            ////Để test thử hiển thị như nào thôi
            var bmp = new BitmapImage(new Uri("pack://application:,,,/Views/Screenshot.png"));
            ScreenView = bmp;

            _clientController = clientController;
            _remoteController = new RemoteController(_clientController);
            PartnerId = partnerId;

            //nhận frame từ partner
            _clientController.ScreenFrameReceived += frame =>
            {
                System.Diagnostics.Debug.WriteLine($"[REMOTE] Received frame from {frame.From}, expecting from {PartnerId}");
                System.Diagnostics.Debug.WriteLine($"[REMOTE] Frame size: {frame.ImageData?.Length} bytes, {frame.Width}x{frame.Height}");
                if (frame.From == PartnerId)
                {
                    System.Diagnostics.Debug.WriteLine($"[REMOTE] Converting frame to BitmapImage");
                    try
                    {
                        var bmp = ScreenService.ConvertToBitmapImage(frame.ImageData);
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            System.Diagnostics.Debug.WriteLine($"[REMOTE] Updating ScreenView on UI thread");
                            ScreenView = bmp;
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[REMOTE ERROR] {ex.Message}");
                    }

                }
            };
        }

        public Task StartStreamingAsync(CancellationToken token)
        {
            return _remoteController.StartStreamingAsync(PartnerId, token);
        }
    }

}
