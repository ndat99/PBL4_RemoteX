using RemoteX.Client.Controllers;
using RemoteX.Client.Services;
using RemoteX.Core.Models;
using RemoteX.Core.Utils;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Input;

namespace RemoteX.Client.ViewModels
{
    public class RemoteViewModel : BaseViewModel
    {
        private BitmapImage _screen;
        private readonly ClientController _clientController;
        private readonly RemoteController _remoteController;

        private int _remoteScreenWidth;
        private int _remoteScreenHeight;
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
            ////Màn hình loading
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
                        _remoteScreenHeight = frame.Height;
                        _remoteScreenWidth = frame.Width;

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

        //gửi click event
        public async Task SendMouseEventAsync(MouseEventMessage.MouseAction action, System.Windows.Point localPoint, System.Windows.Size imageControlSize)
        {
            if (_remoteScreenWidth == 0 || _remoteScreenHeight == 0)
            {
                System.Diagnostics.Debug.WriteLine("[REMOTE] Remote screen size not available yet");
                return;
            }

            var (remoteX, remoteY) = MouseService.ConvertToRemoteCoordinates(
                localPoint, imageControlSize,
                _remoteScreenWidth, _remoteScreenHeight);
            if (remoteX == -1 || remoteY == -1)
            {
                System.Diagnostics.Debug.WriteLine("[REMOTE] Clicked on letterbox area, ignoring");
                return;
            }
            System.Diagnostics.Debug.WriteLine(
            $"[MOUSE] Local({localPoint.X:F0},{localPoint.Y:F0}) -> Remote({remoteX},{remoteY}) | " +
            $"Control:{imageControlSize.Width}x{imageControlSize.Height} Remote:{_remoteScreenWidth}x{_remoteScreenHeight}");

            //gửi click event
            var mouseEvent = new MouseEventMessage
            {
                From = _clientController.ClientId,
                To = PartnerId,
                Action = (MouseEventMessage.MouseAction)action,
                X = remoteX,
                Y = remoteY,
                ScreenWidth = _remoteScreenWidth,
                ScreenHeight = _remoteScreenHeight
            };
            await _clientController.SendAsync(mouseEvent);
        }
        //gửi scroll event
        public async Task SendScrollEventAsync(int delta)
        {
            var mouseEvent = new MouseEventMessage
            {
                From = _clientController.ClientId,
                To = PartnerId,
                Action = MouseEventMessage.MouseAction.Scroll,
                X = delta, //dùng X để gửi delta
                Y = 0,
                ScreenWidth = _remoteScreenWidth,
                ScreenHeight = _remoteScreenHeight
            };
            await _clientController.SendAsync(mouseEvent);
        }
    }

}
