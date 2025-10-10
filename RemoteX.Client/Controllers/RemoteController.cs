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
        private int fps = 15; //tốc độ khung hình
        private int quality = 25 ; //chất lượng ảnh

        public RemoteController(ClientController clientController)
        {
            _clientController = clientController;
        }

        //Gửi frame lên server
        public void StartStreaming(string partnerId, CancellationToken token)
        {
            Thread streamThread = new Thread(() =>
            {
                long frameId = 0; //ID khung hình tăng dần
                const int MAX_PACKET_SIZE = 1024; //kích thước gói tin tối đa
                while (!token.IsCancellationRequested)
                {
                    using var bmp = ScreenService.CaptureScreen(); //Bitmap screenshot
                    int newWidth = bmp.Width /3*2;
                    int newHeight = bmp.Height /3*2;
                    using var smallBmp = new Bitmap(newWidth, newHeight);
                    using (var g = Graphics.FromImage(smallBmp))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(bmp, 0, 0, newWidth, newHeight);
                    }
                    byte[] fullImageData = ScreenService.CompressToJpeg(smallBmp, quality); //JPEG quality

                    int totalPackets = (fullImageData.Length + MAX_PACKET_SIZE - 1) / MAX_PACKET_SIZE; //tính số gói tin cần gửi

                    for (int i = 0; i < totalPackets; i++)
                    {
                        int offset = i * MAX_PACKET_SIZE;
                        int size = Math.Min(MAX_PACKET_SIZE, fullImageData.Length - offset);
                        byte[] chunk = new byte[size];
                        Array.Copy(fullImageData, offset, chunk, 0, size);

                        var packet = new ScreenFrameMessage
                        {
                            From = _clientController.ClientId,
                            To = partnerId,
                            ImageData = chunk, //dữ liệu của gói tin nhỏ
                            FrameID = frameId, //ID của khung hình
                            PacketIndex = i, //index của gói tin nhỏ này
                            TotalPackets = totalPackets, //tổng số gói tin nhỏ
                            Width = bmp.Width,
                            Height = bmp.Height,
                            //Timestamp = DateTime.Now,
                        };
                        //MessageSender.Send(_clientController.TcpClient, frame);
                        System.Diagnostics.Debug.WriteLine($"[STREAMING] Sending frame | Quality: {quality} | Size: {chunk.Length} bytes");
                        _clientController.Send(packet);
                    }
                    frameId++; //tăng ID cho khung hình tiếp theo
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
