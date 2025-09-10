using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RemoteX.Core;
using RemoteX.Client.ViewModels;
using RemoteX.Client.Controllers;
using RemoteX.Client.Services;
using RemoteX.Core.Models;

namespace RemoteX.Client.Views
{
    public partial class RemoteWindow : Window
    {
        private RemoteViewModel _rvm;
        private readonly ClientController _clientController;
        private CancellationTokenSource _cts;
        public RemoteWindow(ClientController clientController, string partnerId)
        {
            InitializeComponent();
            _rvm = new RemoteViewModel();
            this.DataContext = _rvm;

            _rvm = new RemoteViewModel
            {
                PartnerId = partnerId
            };
            this.DataContext = _rvm;

            //Khi nhan screenframe tu server
            _clientController.ScreenFrameReceived += frame =>
            {
                if (frame.From == partnerId)
                {
                    var bmp = ScreenService.ConvertToBitmapImage(frame.ImageData);
                    Dispatcher.Invoke(() =>
                    {
                        _rvm.ScreenView = bmp;
                    });
                }
            };
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _cts = new CancellationTokenSource();
            _ = _rvm.StartStreamingAsync(_cts.Token);
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();

            var disconnectMsg = new ConnectRequest
            {
                From = _clientController.ClientId,
                To = _rvm.PartnerId,
                Status = "disconnect"
            };
            _ = _clientController.SendAsync(disconnectMsg);

            this.Close();
        }

        private void btnScreenshot_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
