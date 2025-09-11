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
            //_cts = new CancellationTokenSource();
            //_ = _rvm.StartStreamingAsync(_cts.Token); //Bắt đầu stream màn hình
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();

            var disconnectMsg = new ConnectRequest
            {
                From = _rvm.clientController.ClientId,
                To = _rvm.PartnerId,
                Status = "disconnect"
            };
            _ = _rvm.clientController.SendAsync(disconnectMsg);

            this.Close();
        }

        private void btnScreenshot_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
