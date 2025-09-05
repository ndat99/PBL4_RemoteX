using RemoteX.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteX.Server.Services
{
    public class ClientManager
    {
        private readonly List<ClientInfo> _clients = new List<ClientInfo>();

        public void AddClient(ClientInfo clientInfo)
        {
            lock (_clients)
            {
                _clients.Add(clientInfo);
            }
        }

        public void RemoveClient(ClientInfo clientInfo)
        {
            lock (_clients)
            {
                _clients.Remove(clientInfo);
            }
        }

        public ClientInfo GetClientByID(string id)
        {
            lock (_clients)
            {
                return _clients.FirstOrDefault(c => c.Id == id);
            }
        }
        public List<ClientInfo> GetAllClients()
        {
            lock (_clients) // Lock để đọc dữ liệu an toàn
            {
                // Trả về bản sao danh sách (không trả trực tiếp _clients để tránh thay đổi ngoài ý muốn)
                return _clients.ToList();
            }
        }
    }
}
