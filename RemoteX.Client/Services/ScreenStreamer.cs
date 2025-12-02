using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RemoteX.Core.Models;
using RemoteX.Core.Networking;
using RemoteX.Core.Services;
using RemoteX.Core.Enums;
using System.Net.Sockets;
using RemoteX.Core;

namespace RemoteX.Client.Services
{
    public class ScreenStreamer
    {

        public readonly ClientNetworkManager _clientController;
        private int fps = 15; //tốc độ khung hình
        private int quality = 25 ; //chất lượng ảnh
        private int resolution = 66; //tỉ lệ phân giải 

        public ScreenStreamer(ClientNetworkManager clientNetwork)
        {
            _clientController = clientNetwork;
        }

        public void SetQuality(QualityLevel level)
        {
            switch (level)
            {
                case QualityLevel.Low:
                    fps = 20;
                    quality = 20;
                    resolution = 55;
                    System.Diagnostics.Debug.WriteLine("[QUALITY] Set to Thấp");
                    break;
                case QualityLevel.Medium:
                    fps = 15;
                    quality = 25;
                    resolution = 66;
                    System.Diagnostics.Debug.WriteLine("[QUALITY] Set to Trung bình");
                    break;
                case QualityLevel.High:
                    fps = 12;
                    quality = 23;
                    resolution = 73;
                    System.Diagnostics.Debug.WriteLine("[QUALITY] Set to Cao");
                    break;
            }
        }

        //Gửi frame lên server
        public void StartStreaming(string partnerId, CancellationToken token)
        {
            Task.Run( async () =>
            {
                long frameId = 0; //ID khung hình tăng dần
                const int MAX_PACKET_SIZE = 1024; //kích thước gói tin tối đa
                while (!token.IsCancellationRequested)
                {
                    Task.Delay(1); //nhường luồng khác có cơ hội chạy (để dọn rác, tránh rò rỉ)
                    using var bmp = ScreenService.CaptureScreen(); //Bitmap screenshot
                    int newWidth = bmp.Width * resolution/100;
                    int newHeight = bmp.Height * resolution/100;
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
                        await Task.Delay(1000 / fps, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break; //khi token bị cancel, ném exception, thoát vòng while
                    }
                }
            }, token);
        }
    }
}
