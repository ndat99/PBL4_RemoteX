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
        public readonly ClientController _clientController;
        private int fps = 20; //tốc độ khung hình
        private int quality = 1 ; //chất lượng ảnh

        public RemoteController(ClientController clientController)
        {
            _clientController = clientController;
        }

        //Gửi frame lên server
        public void StartStreaming(string partnerId, CancellationToken token)
        {
            Thread streamThread = new Thread(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    using var bmp = ScreenService.CaptureScreen(); //Bitmap screenshot
                    byte[] jpeg = ScreenService.CompressToJpeg(bmp, quality); //JPEG quality

                    var frame = new ScreenFrameMessage
                    {
                        From = _clientController.ClientId,
                        To = partnerId,
                        ImageData = jpeg,
                        Width = bmp.Width,
                        Height = bmp.Height,
                        Timestamp = DateTime.Now,
                    };
                    //MessageSender.Send(_clientController.TcpClient, frame);
                    System.Diagnostics.Debug.WriteLine($"[STREAMING] Sending frame | Quality: {quality} | Size: {jpeg.Length} bytes");
                    _clientController.Send(frame);
                    try
                    {
                        Thread.Sleep(1000/fps);
                    }
                    catch (ThreadInterruptedException)
                    {
                        break; //khi token bị cancel, ném exception, thoát vòng while
                    }
                }
            });

            streamThread.IsBackground = true; //để luồng tự tắt khi thoát ứng dụng
            streamThread.Start();
        }
    }
}
