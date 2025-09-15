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
using System.Net;

namespace RemoteX.Client.Controllers
{
    public class RemoteController
    {
        public readonly ClientController _clientController;
        private int fps = 20; //tốc độ khung hình
        private int quality = 50; //chất lượng ảnh
        private readonly UdpClient _udpSender;
        private readonly IPEndPoint _serverUdpEndPoint;

        public RemoteController(ClientController clientController)
        {
            _clientController = clientController;

            //Lay TCP EndPoint hien tai
            var tcpEndPoint = (IPEndPoint)_clientController.TcpClient.Client.LocalEndPoint!;
            int udpPort = tcpEndPoint.Port + 1;

            //Tao UdpClient de gui
            _udpSender = new UdpClient(udpPort);

            // Server UDP endpoint = server TCP IP + (server TCP port + 1)
            var serverTcp = (IPEndPoint)_clientController.TcpClient.Client.RemoteEndPoint!;
            _serverUdpEndPoint = new IPEndPoint(serverTcp.Address, serverTcp.Port + 1);
        }

        //Gửi frame lên server
        public async Task StartStreamingAsync(string partnerId, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                using var bmp = ScreenService.CaptureScreen(); //Bitmap screenshot
                byte[] jpeg = ScreenService.CompressToJpeg(bmp, quality); //JPEG quality

                //var frame = new ScreenFrameMessage
                //{
                //    From = _clientController.ClientId,
                //    To = partnerId,
                //    ImageData = jpeg,
                //    Width = bmp.Width,
                //    Height = bmp.Height,
                //    Timestamp = DateTime.Now,
                //};

                byte[] widthBytes = BitConverter.GetBytes(bmp.Width);
                byte[] heightBytes = BitConverter.GetBytes(bmp.Height);

                byte[] data = new byte[8 + jpeg.Length];
                Array.Copy(widthBytes, 0, data, 0, 4);
                Array.Copy(heightBytes, 0, data, 4, 4);
                Array.Copy(jpeg, 0, data, 8, jpeg.Length);

                await _udpSender.SendAsync(data, data.Length, _serverUdpEndPoint);
                await Task.Delay(1000/fps, token); //10fps
            }
        }
    }
}
