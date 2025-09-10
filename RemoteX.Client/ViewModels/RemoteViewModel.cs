using RemoteX.Client.Controllers;
using RemoteX.Client.Services;
using RemoteX.Core.Models;
using RemoteX.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RemoteX.Client.ViewModels
{
    public class RemoteViewModel : BaseViewModel
    {
        private BitmapImage _screen;
        public readonly ClientController _clientController;

        public BitmapImage ScreenView
        {
            get => _screen;
            set 
            { _screen = value;
               OnPropertyChanged(nameof(ScreenView)); 
            }
        }
        public string PartnerId { get; set; }

        public RemoteViewModel()
        {
            //Để test thử hiển thị như nào thôi
            var bmp = new BitmapImage(new Uri("pack://application:,,,/Views/Screenshot.png"));
            ScreenView = bmp;
        }

        private async Task StartStreamingAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using var bmp = ScreenService.CaptureScreen();
                byte[] compressed = ScreenService.CompressToJpeg(bmp, 50);

                var frame = new ScreenFrameMessage
                {
                    From = _clientController.ClientId,
                    To = _rvm.PartnerId,
                    ImageData = compressed,
                    Width = bmp.Width,
                    Height = bmp.Height,
                    Timestamp = DateTime.Now
                };

                await _clientController.SendAsync(frame);
                await Task.Delay(100, token);
            }
        }
    }

}
