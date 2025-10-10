using RemoteX.Client.Controllers;
using RemoteX.Client.Services;
using RemoteX.Core.Models;
using RemoteX.Core.Utils;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace RemoteX.Client.ViewModels
{
    public class RemoteViewModel : BaseViewModel
    {
        //kho chứa các gói tin chưa hoàn chỉnh với key là FrameID
        private readonly Dictionary<long, List<ScreenFrameMessage>> _frameBuffer = new();
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

            //nhận frame packet từ partner
            _clientController.ScreenFrameReceived += OnScreenFramePacketReceived;
        }

        private void OnScreenFramePacketReceived(ScreenFrameMessage packet)
        {
            if (packet == null || packet.From != PartnerId) return;
            //nếu chưa tồn tại FrameID này trong kho thì tạo một ds mới
            if (!_frameBuffer.ContainsKey(packet.FrameID))
            {
                _frameBuffer[packet.FrameID] = new List<ScreenFrameMessage>();
            }

            _frameBuffer[packet.FrameID].Add(packet); //thêm gói tin vào kho

            //kiểm tra đã nhận đủ gói tin cho FrameID này chưa
            if (_frameBuffer[packet.FrameID].Count == packet.TotalPackets)
            {
                var completedPackets = _frameBuffer[packet.FrameID];
                _frameBuffer.Remove(packet.FrameID); //xóa kho chứa gói tin này

                //sắp xếp gói tin theo PacketIndex
                completedPackets.Sort((p1, p2) => p1.PacketIndex.CompareTo(p2.PacketIndex));

                //ghép các gói tin lại thành mảng byte hoàn chỉnh
                int totalImageSize = 0;
                foreach (var p in completedPackets)
                {
                    totalImageSize += p.ImageData.Length;
                }
                byte[] finalImageData = new byte[totalImageSize];
                int currentPosition = 0;
                
                foreach (var p in completedPackets)
                {
                    Buffer.BlockCopy(p.ImageData, 0, finalImageData, currentPosition, p.ImageData.Length);
                    currentPosition += p.ImageData.Length;
                }
                var bmp = ScreenService.ConvertToBitmapImage(finalImageData);
                App.Current.Dispatcher.Invoke(() =>
                {
                    ScreenView = bmp;
                    _remoteScreenHeight = packet.Height;
                    _remoteScreenWidth = packet.Width;
                });
            }
        }

        public void StartStreaming(CancellationToken token)
        {
            _remoteController.StartStreaming(PartnerId, token);
        }

        //chụp màn hình
        public void SaveScreenShot()
        {
            if (ScreenView == null)
            {
                return;
            }

            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string fileName = $"Screenshot_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.jpg";
                string fullPath = Path.Combine(path, fileName);

                //tạo mã hóa JPEG
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 100;
                encoder.Frames.Add(BitmapFrame.Create(ScreenView));

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                    System.Diagnostics.Debug.WriteLine($"[SCREENSHOT] Screenshot saved to {fullPath}");
                    System.Windows.MessageBox.Show($"Đã lưu ảnh chụp màn hình thành công vào {path}", "Hoàn tất",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"[SCREENSHOT ERROR] {e.Message}");
            }
        }

        //gửi click event
        public void SendMouseEvent(MouseEventMessage.MouseAction action, System.Windows.Point localPoint, System.Windows.Size imageControlSize)
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
            _clientController.Send(mouseEvent);
        }
        //gửi scroll event
        public void SendScrollEvent(int delta)
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
            _clientController.Send(mouseEvent);
        }
    }

}
