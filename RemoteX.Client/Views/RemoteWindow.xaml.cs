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
        public RemoteWindow(ClientController clientController, string partnerId)
        {
            InitializeComponent();
            _rvm = new RemoteViewModel(clientController, partnerId);
            this.DataContext = _rvm;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

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
            _ = _rvm.clientController.SendAsync(disconnectMsg);
        }

        private void btnScreenshot_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
