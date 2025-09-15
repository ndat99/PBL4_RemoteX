using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Core.Networking
{
    public class UDPScreenSender
    {
        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _remoteEndpoint;
        private const int MAX_PACKET_SIZE = 1400; //Tranh fragmentation
        private const int HEADER_SIZE = 16; // frameId(4) + fragmentId(4) + totalFragments(4) + dataSize(4)
        private uint _frameNumber = 0;

        public UDPScreenSender(string remoteIP, int remoteTcpPort) {
            int remoteUdpPort = remoteTcpPort + 1;
            
            //Endpoint cua may nhan
            _remoteEndpoint = new IPEndPoint(IPAddress.Parse(remoteIP), remoteUdpPort);
            //UDP client gui packet di
            _udpClient = new UdpClient();
        }

        //Ham gui frame man hinh (byte[] da encode truoc)
        public async Task SendFrameAsync(byte[] frameData)
        {
            if(frameData == null || frameData.Length == 0) return;

            //Tăng stt frame
            _frameNumber++;

            //Tinh so packet can thiet de gui het frame
            int dataPerPacket = MAX_PACKET_SIZE - 12; //Tru di 12 bytes header
            int totalPackets = (frameData.Length + dataPerPacket - 1) / dataPerPacket;

            try
            {
                for (int i = 0; i < totalPackets; i++)
                {
                    int startPos = i * dataPerPacket;

                    int bytesInThisPacket = Math.Min(dataPerPacket, frameData.Length - startPos); //So byte thuc te trong packet 
                    //Tao packet = header + data
                    byte[] packet = new byte[12 + bytesInThisPacket];

                    Array.Copy(BitConverter.GetBytes(_frameNumber), 0, packet, 0, 4);      // Frame số mấy
                    Array.Copy(BitConverter.GetBytes(i), 0, packet, 4, 4);                // Packet thứ mấy
                    Array.Copy(BitConverter.GetBytes(totalPackets), 0, packet, 8, 4);     // Tổng cộng bao nhiêu packet

                    // Copy data vào packet
                    Array.Copy(frameData, startPos, packet, 12, bytesInThisPacket);
                    //Gui packet qua UDP
                    await _udpClient.SendAsync(frameData, frameData.Length, _remoteEndpoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        public void Dispose()
        {
            _udpClient?.Dispose();
        }
    }
}
