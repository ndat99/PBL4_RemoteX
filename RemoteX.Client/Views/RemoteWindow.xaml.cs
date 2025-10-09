using System.Windows;
using RemoteX.Client.ViewModels;
using RemoteX.Client.Controllers;
using RemoteX.Core.Models;

namespace RemoteX.Client.Views
{
    public partial class RemoteWindow : Window
    {
        private RemoteViewModel _rvm;
        private CancellationTokenSource _cts;
        private bool _isMouseDown = false;
        public RemoteWindow(ClientController clientController, string partnerId)
        {
            InitializeComponent();
            _rvm = new RemoteViewModel(clientController, partnerId);
            this.DataContext = _rvm;
            RegisterMouseEvent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            imgRemoteScreen.Focus(); //đặt focus vào image để nhận sự kiện chuột
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
                From = _rvm.clientController.ClientId,
                To = _rvm.PartnerId,
                Status = "Disconnect"
            };
            _rvm.clientController.Send(disconnectMsg);
        }

        private void btnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            _rvm.SaveScreenShot();
        }

        private void RegisterMouseEvent()
        {
            //move
            imgRemoteScreen.MouseMove += (s, e) =>
            {
                //if (!_isMouseDown) return; //nếu nút chuột trái không được nhấn giữ thì không làm gì cả
                var pos = e.GetPosition(imgRemoteScreen);
                var size = new System.Windows.Size(imgRemoteScreen.ActualWidth, imgRemoteScreen.ActualHeight);
                new Thread(() =>
                {
                    _rvm.SendMouseEvent(MouseEventMessage.MouseAction.Move, pos, size);
                }).Start();
            };

            //left down
            imgRemoteScreen.MouseLeftButtonDown += (s, e) =>
            {
                //kiểm tra xem phải double click ko
                if (e.ClickCount == 2)
                {
                    System.Diagnostics.Debug.WriteLine("[REMOTE] Double Click detected");
                    var pos = e.GetPosition(imgRemoteScreen);
                    var size = new System.Windows.Size(imgRemoteScreen.ActualWidth, imgRemoteScreen.ActualHeight);
                    new Thread(() =>
                    {
                        _rvm.SendMouseEvent(MouseEventMessage.MouseAction.DoubleClick, pos, size);
                    }).Start();
                }
                else
                {
                    _isMouseDown = true;
                    imgRemoteScreen.CaptureMouse(); //bắt sự kiện chuột ngay cả khi chuột di chuyển ra ngoài control

                    var pos = e.GetPosition(imgRemoteScreen);
                    var size = new System.Windows.Size(imgRemoteScreen.ActualWidth, imgRemoteScreen.ActualHeight);
                    new Thread(() =>
                    {
                        _rvm.SendMouseEvent(MouseEventMessage.MouseAction.LeftDown, pos, size);
                    }).Start();
                }
                e.Handled = true; //ngăn chặn sự kiện được xử lý tiếp
            };
            //left up
            imgRemoteScreen.MouseLeftButtonUp += (s, e) =>
            {
                _isMouseDown = false;
                imgRemoteScreen.ReleaseMouseCapture(); //thả bắt sự kiện chuột

                var pos = e.GetPosition(imgRemoteScreen);
                var size = new System.Windows.Size(imgRemoteScreen.ActualWidth, imgRemoteScreen.ActualHeight);
                new Thread(() =>
                {
                    _rvm.SendMouseEvent(MouseEventMessage.MouseAction.LeftUp, pos, size);
                }).Start();
                e.Handled = true;
            };
            //right down
            imgRemoteScreen.MouseRightButtonDown += (s, e) =>
            {
                var pos = e.GetPosition(imgRemoteScreen);
                var size = new System.Windows.Size(imgRemoteScreen.ActualWidth, imgRemoteScreen.ActualHeight);
                new Thread(() =>
                {
                    _rvm.SendMouseEvent(MouseEventMessage.MouseAction.RightDown, pos, size);
                }).Start();
                e.Handled = true;
            };
            //right up
            imgRemoteScreen.MouseRightButtonUp += (s, e) =>
            {
                var pos = e.GetPosition(imgRemoteScreen);
                var size = new System.Windows.Size(imgRemoteScreen.ActualWidth, imgRemoteScreen.ActualHeight);
                new Thread(() =>
                {
                    _rvm.SendMouseEvent(MouseEventMessage.MouseAction.RightUp, pos, size);
                }).Start();
                e.Handled = true;
            };
            //wheel
            imgRemoteScreen.MouseWheel += (s, e) =>
            {
                new Thread(() =>
                {
                    _rvm.SendScrollEvent(e.Delta);
                }).Start();
                e.Handled = true;
            };
        }
    }
}
