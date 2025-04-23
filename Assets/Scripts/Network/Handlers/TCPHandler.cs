using System.Net.Sockets;
using System.Net;
using System;
using System.Collections.Generic;

namespace Network.Handlers
{
    public class TCPHandler : INetworkHandler, IUpdatableHandler
    {
        private TcpListener _listener;

        private readonly List<TCPClientConnection> _clients = new List<TCPClientConnection>();
        private TCPClientConnection _connectedClient;

        public event Action<byte[]> OnDataReceived;
        public event Action OnConnected;
        public event Action OnConnectionFailed;

        public void Update()
        {
            if (NetworkManager.Instance.IsServer)
            {
                foreach (var client in _clients)
                    client.FlushReceivedData();
            }

            else
            {
                if (_connectedClient != null)
                {
                    _connectedClient.FlushReceivedData();

                    if (_connectedClient != null && _connectedClient.IsConnected)
                        _connectedClient.IsConnected = false;
                }
            }
        }


        public void StartServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            _listener.BeginAcceptTcpClient(OnClientConnect, null);
        }

        private void OnClientConnect(IAsyncResult result)
        {
            TcpClient tcpClient = _listener.EndAcceptTcpClient(result);
            var clientConnection = new TCPClientConnection(tcpClient, isServer: true);

            clientConnection.OnDataReceived += (data) => OnDataReceived?.Invoke(data);
            clientConnection.OnDisconnected += () => _clients.Remove(clientConnection);

            _clients.Add(clientConnection);

            _listener.BeginAcceptTcpClient(OnClientConnect, null);
            OnConnected?.Invoke();
        }

        public void Connect(string ip, int port)
        {
            TcpClient tcpClient = new TcpClient();
            _connectedClient = new TCPClientConnection(tcpClient);

            _connectedClient.OnDataReceived += (data) => OnDataReceived?.Invoke(data);
            _connectedClient.OnConnectionFailed += () => OnConnectionFailed?.Invoke();

            _connectedClient.OnConnected += () => OnConnected?.Invoke();
            tcpClient.BeginConnect(IPAddress.Parse(ip), port, _connectedClient.OnEndConnection, null);
        }

        public void Send(byte[] data)
        {
            if (NetworkManager.Instance.IsServer)
            {
                foreach (var client in _clients)
                    client.SendData(data);
            }

            else
            {
                _connectedClient?.SendData(data);
            }
        }

        public void Disconnect()
        {
            _listener?.Stop();
            _connectedClient?.Close();

            foreach (var client in _clients)
                client.Close();

            _clients.Clear();
        }
    }
}
