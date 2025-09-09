using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RemoteX.Core.Models;
using RemoteX.Core.Networking;
using RemoteX.Client.Services;
using RemoteX.Core.Services;
using RemoteX.Core.Enums;
using System.Net.Sockets;
using RemoteX.Core;

namespace RemoteX.Client.Controllers
{
    public class RemoteController
    {
        private readonly ClientController _client;

        public RemoteController(ClientController clientController)
        {
            _client = clientController;
        }

        public async Task StartStreaming(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using var bmp = ScreenService.CaptureScreen(); //Bitmap screenShot
                byte[] compressed = ScreenService.CompressToJpeg(bmp, 50); //JPEG 50% quality

                var frame = new ScreenFrameMessage
                {
                    ImageData = compressed,
                    Width = bmp.Width,
                    Height = bmp.Height,
                    Timestamp = DateTime.Now,
                };
                await MessageSender.Send(_client.TcpClient, frame);
                await Task.Delay(100, token);
            }
        }
    }
}
