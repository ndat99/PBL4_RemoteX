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

        private readonly List<ClientInfo> _clients = new();
        private readonly Dictionary<string, string> _mappings = new(); //key: A_ID, Value: B_ID (2 chieu A->B & B->A)

        public void AddClient(ClientInfo clientInfo)
        {
            lock (_clients)
            {
                _clients.Add(clientInfo);
            }
        }

        public void RemoveClient(ClientInfo client)
        {
            lock (_clients)
            {
                _clients.Remove(client);

                //Xoa mapping neu co
                _mappings.Remove(client.Id);
                var partner = _mappings.FirstOrDefault(x => x.Value == client.Id).Key;
                if (partner != null) _mappings.Remove(partner);
            }
        }

        public ClientInfo? FindById(string Id)
        {
            lock (_clients)
                return _clients.FirstOrDefault(c => c.Id == Id);
        }

        public bool TryMapClients(string requesterId, string targetId, string password)
        {
            var target = FindById(targetId);
            if (target == null || target.Password != password) return false;

            lock (_mappings)
            {
                _mappings[requesterId] = targetId;
                _mappings[targetId] = requesterId;
            }
            return true;
        }

        public string? GetPartnerId(string clientId)
        {
            lock (_mappings)
            {
                return _mappings.TryGetValue(clientId, out var partnerId) ? partnerId : null;
            }
        }
        public IEnumerable<ClientInfo> GetAllClients()
        {
            lock (_clients)
            {
                return _clients.ToList();
            }
        }

        public void Clear()
        {
            lock (_clients)
            {
                _clients.Clear();
            }
            lock (_mappings)
            {
                _mappings.Clear();
            }
        }
    }
}