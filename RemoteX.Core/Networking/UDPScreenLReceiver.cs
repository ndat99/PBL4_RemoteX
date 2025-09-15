using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace RemoteX.Core.Networking
{
    public class UDPScreenLReceiver
    {
        private readonly UdpClient udpClient;
        //Tao mot su kien khi nhan du 1 frame
        public event Action<byte[]> FrameReceived;

        public UDPScreenLReceiver(int tcpPort)
        {
            int udpPort = tcpPort + 1;
            //UDPClient lang nghe packet
            udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, udpPort));
        }

        //Start listen loop
        public async Task StartAsync()
        {
            while (true)
            {
                try
                {
                    //Nhan packet tu bat ky IP nao
                    var result = await udpClient.ReceiveAsync();

                    //Raise event cho UI xu ly -> thanh BitmapImage hien thi
                    FrameReceived?.Invoke(result.Buffer);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"UDP receive error: {ex.Message}");
                    break;
                }
            }
        }

        public async Task<byte[]> ReceiveAsync()
        {
            var result = await udpClient.ReceiveAsync();
            return result.Buffer;
        }
    }
}
