using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using Network.Connection;
using System.Text;
using System.Linq;

namespace Network.Handlers
{
    /*
    public class UDPHandler : INetworkHandler, IUpdatableHandler
    {

        private UdpClient _udpClient;

        private readonly List<UDPClientConnection> _clients = new List<UDPClientConnection>();
        private UDPClientConnection _connectedClient;
        private IPEndPoint _clientEndPoint;

        private bool _isServer;
        private UDPConnectionValidator _validator;

        public event Action<byte[]> OnDataReceived;
        public event Action OnConnected;
        public event Action OnConnectionFailed;

        public void StartServer(int port)
        {
            _isServer = true;

            try
            {
                _udpClient = new UdpClient(port);
                BeginReceive();
                Debug.Log($"[UDP Server] Listening on port {port}");

                OnConnected?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[UDP Server] Failed to start: {e.Message}");
                OnConnectionFailed?.Invoke();
            }
        }

        public void Connect(string ip, int port)
        {
            _isServer = false;

            try
            {
                _clientEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                _udpClient = new UdpClient(0);
                _connectedClient = new UDPClientConnection();

                BeginReceive();
                Debug.Log($"[UDP Client] Connecting to {_clientEndPoint}");

                _validator = new UDPConnectionValidator(_udpClient, _clientEndPoint);
                _validator.Validate(
                    onSuccess: () =>
                    {
                        Debug.Log("[UDP Client] Connection validated.");
                        OnConnected?.Invoke();
                    },
                    onFailure: () =>
                    {
                        Debug.LogWarning("[UDP Client] Connection validation failed.");
                        OnConnectionFailed?.Invoke();
                    });
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[UDP Client] Connect failed: {e.Message}");
                OnConnectionFailed?.Invoke();
            }
        }

        private void BeginReceive()
        {
            _udpClient.BeginReceive(OnReceive, null);
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udpClient.EndReceive(ar, ref sender);
                string message = Encoding.UTF8.GetString(data);

                if (_isServer)
                {
                    Debug.Log($"[UDP Server] Received: {message} from {sender}");

                    if (message == "PING")
                    {
                        _udpClient.Send(Encoding.UTF8.GetBytes("PONG"), 4, sender);
                        return;
                    }

                    UDPClientConnection client = _clients.FirstOrDefault(c => c.Matches(sender));
                    if (client == null)
                    {
                        client = new UDPClientConnection(sender);
                        _clients.Add(client);
                        Debug.Log($"[UDP Server] New client: {sender}");
                    }

                    client.EnqueueData(data);

                    foreach (var c in _clients)
                        _udpClient.Send(data, data.Length, c.RemoteEndPoint);
                }
                else
                {
                    if (_validator != null && message == "PONG")
                    {
                        Debug.Log("[UDP Client] Received PONG (validator)");
                        _validator.OnPongReceived();
                        return;
                    }

                    Debug.Log($"[UDP Client] Received: {message} from {sender}");
                    _connectedClient.EnqueueData(data);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[UDP Receive] Error: {e.Message}");
            }

            if (_udpClient?.Client?.IsBound ?? false)
                BeginReceive();
        }

        public void Update()
        {
            if (_isServer)
            {
                foreach (var client in _clients)
                    client.FlushReceivedData(OnDataReceived);
            }
            else
            {
                _connectedClient?.FlushReceivedData(OnDataReceived);
            }
        }

        public void Send(byte[] data)
        {
            try
            {
                if (_isServer)
                {
                    foreach (var client in _clients)
                        _udpClient.Send(data, data.Length, client.RemoteEndPoint);
                }
                else
                {
                    if (_clientEndPoint != null)
                    {
                        Debug.Log($"[UDP Client] Sending to server {_clientEndPoint}");
                        _udpClient.Send(data, data.Length, _clientEndPoint);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[UDP Send] Failed: {e.Message}");
            }
        }

        public void Disconnect()
        {
            _udpClient?.Close();
            _udpClient = null;
            _clients.Clear();
            _connectedClient = null;

            Debug.Log("[UDP] Disconnected");
        }
    }
    */
}
