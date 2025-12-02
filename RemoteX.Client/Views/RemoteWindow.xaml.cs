using System.Windows;
using RemoteX.Client.ViewModels;
using RemoteX.Core.Models;
using System.Windows.Input;
using RemoteX.Client.Services;
using System.Windows.Controls;
using RemoteX.Core.Enums;

namespace RemoteX.Client.Views
{
    public partial class RemoteWindow : Window
    {
        private RemoteViewModel _rvm;
        private CancellationTokenSource _cts;
        private bool _isMouseDown = false;
        public RemoteWindow(ClientNetworkManager clientNetwork, string partnerId)
        {
            InitializeComponent();
            _rvm = new RemoteViewModel(clientNetwork, partnerId);
            this.DataContext = _rvm;

            RegisterMouseEvent();
            this.KeyDown += OnKeyEvent;
            this.KeyUp += OnKeyEvent;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            imgRemoteScreen.Focus(); //đặt focus vào image để nhận sự kiện chuột
            this.Activate(); //đặt focus vào cửa sổ để nhận sự kiện bàn phím
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("Bạn có chắc chắn muốn ngắt kết nối không?",
                 "Xác nhận",
                 MessageBoxButton.YesNo,
                 MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true; // Hủy việc đóng
                return;
            }

            _cts?.Cancel();

            var disconnectMsg = new ConnectRequest
            {
                From = _rvm.clientNetwork.ClientId,
                To = _rvm.PartnerId,
                Status = "Disconnect"
            };
            _rvm.clientNetwork.Send(disconnectMsg);
        }

        private void btnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            _rvm.SaveScreenShot();
            imgRemoteScreen.Focus();
        }
        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            // Tự mở ContextMenu của chính nó
            (sender as System.Windows.Controls.Button).ContextMenu.IsOpen = true;
        }

        // các hàm xử lý chọn chất lượng
        private void Quality_Click_Thap(object sender, RoutedEventArgs e)
        {
            var msg = new QualityChangeMessage
            {
                From = _rvm.clientNetwork.ClientId,
                To = _rvm.PartnerId,
                Quality = QualityLevel.Low
            };
            _rvm.clientNetwork.Send(msg);

            menuItemThap.IsChecked = true;
            menuItemTrungBinh.IsChecked = false;
            menuItemCao.IsChecked = false;

            imgRemoteScreen.Focus(); //trả focus về màn hình
        }

        private void Quality_Click_TrungBinh(object sender, RoutedEventArgs e)
        {
            var msg = new QualityChangeMessage
            {
                From = _rvm.clientNetwork.ClientId,
                To = _rvm.PartnerId,
                Quality = QualityLevel.Medium
            };
            _rvm.clientNetwork.Send(msg);

            menuItemThap.IsChecked = false;
            menuItemTrungBinh.IsChecked = true;
            menuItemCao.IsChecked = false;

            imgRemoteScreen.Focus();
        }

        private void Quality_Click_Cao(object sender, RoutedEventArgs e)
        {
            var msg = new QualityChangeMessage
            {
                From = _rvm.clientNetwork.ClientId,
                To = _rvm.PartnerId,
                Quality = QualityLevel.High
            };
            _rvm.clientNetwork.Send(msg);

            menuItemThap.IsChecked = false;
            menuItemTrungBinh.IsChecked = false;
            menuItemCao.IsChecked = true;

            imgRemoteScreen.Focus();
        }

        private void RegisterMouseEvent()
        {
            //move
            imgRemoteScreen.MouseMove += async (s, e) =>
            {
                //if (!_isMouseDown) return; //nếu chuột trái không được click thì không bắt event move
                var pos = e.GetPosition(imgRemoteScreen);
                var size = new System.Windows.Size(imgRemoteScreen.ActualWidth, imgRemoteScreen.ActualHeight);
                await Task.Run(() =>
                {
                    _rvm.SendMouseEvent(MouseEventMessage.MouseAction.Move, pos, size);
                });
            };

            //left down
            imgRemoteScreen.MouseLeftButtonDown += async (s, e) =>
            {
                //kiểm tra xem phải double click ko
                if (e.ClickCount == 2)
                {
                    System.Diagnostics.Debug.WriteLine("[REMOTE] Double Click detected");
                    var pos = e.GetPosition(imgRemoteScreen);
                    var size = new System.Windows.Size(imgRemoteScreen.ActualWidth, imgRemoteScreen.ActualHeight);
                    await Task.Run(() =>
                    {
                        _rvm.SendMouseEvent(MouseEventMessage.MouseAction.DoubleClick, pos, size);
                    });
                }
                else
                {
                    _isMouseDown = true;
                    imgRemoteScreen.CaptureMouse(); //bắt sự kiện chuột ngay cả khi chuột di chuyển ra ngoài control

                    var pos = e.GetPosition(imgRemoteScreen);
                    var size = new System.Windows.Size(imgRemoteScreen.ActualWidth, imgRemoteScreen.ActualHeight);
                    await Task.Run(() =>
                    {
                        _rvm.SendMouseEvent(MouseEventMessage.MouseAction.LeftDown, pos, size);
                    });
                }
                e.Handled = true; //ngăn chặn sự kiện được xử lý tiếp
            };
            //left up
            imgRemoteScreen.MouseLeftButtonUp += async (s, e) =>
            {
                _isMouseDown = false;
                imgRemoteScreen.ReleaseMouseCapture(); //thả bắt sự kiện chuột

                var pos = e.GetPosition(imgRemoteScreen);
                var size = new System.Windows.Size(imgRemoteScreen.ActualWidth, imgRemoteScreen.ActualHeight);
                await Task.Run(() =>
                {
                    _rvm.SendMouseEvent(MouseEventMessage.MouseAction.LeftUp, pos, size);
                });
                e.Handled = true;
            };
            //right down
            imgRemoteScreen.MouseRightButtonDown += async (s, e) =>
            {
                var pos = e.GetPosition(imgRemoteScreen);
                var size = new System.Windows.Size(imgRemoteScreen.ActualWidth, imgRemoteScreen.ActualHeight);
                await Task.Run(() =>
                {
                    _rvm.SendMouseEvent(MouseEventMessage.MouseAction.RightDown, pos, size);
                });
                e.Handled = true;
            };
            //right up
            imgRemoteScreen.MouseRightButtonUp += async (s, e) =>
            {
                var pos = e.GetPosition(imgRemoteScreen);
                var size = new System.Windows.Size(imgRemoteScreen.ActualWidth, imgRemoteScreen.ActualHeight);
                await Task.Run(() =>
                {
                    _rvm.SendMouseEvent(MouseEventMessage.MouseAction.RightUp, pos, size);
                });
                e.Handled = true;
            };
            //wheel
            imgRemoteScreen.MouseWheel += async (s, e) =>
            {
                await Task.Run(() =>
                {
                    _rvm.SendScrollEvent(e.Delta);
                });
                e.Handled = true;
            };
        }

        //bắt sự kiện bàn phím
        private async void OnKeyEvent(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (KeyboardService.IsSimulating)
            {
                KeyboardService.IsSimulating = false; //reset cờ
                e.Handled = true; //ngăn chặn sự kiện được xử lý tiếp
                return; //nếu đang giả lập bàn phím thì không gửi sự kiện
            }
            int keyCode = KeyInterop.VirtualKeyFromKey(e.Key); //lấy mã phím
            bool isKeyUp = e.IsUp; //kiểm tra phím nhấn hay thả

            await Task.Run(() =>
            {
                var keyEventMsg = new KeyboardEventMessage
                {
                    From = _rvm.clientNetwork.ClientId,
                    To = _rvm.PartnerId,
                    KeyCode = keyCode,
                    IsKeyUp = isKeyUp
                };
                _rvm.clientNetwork.Send(keyEventMsg);
            });
            e.Handled = true; //đánh dấu sự kiện đã được xử lý
        }
    }
}
