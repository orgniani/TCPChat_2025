using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;

namespace Network.Handlers
{
    public class UDPHandler : INetworkHandler, IUpdatableHandler
    {
        private UdpClient _udpClient;
        private IPEndPoint _remoteEndPoint;
        private bool _isServer;

        private readonly Queue<byte[]> _receivedDataQueue = new Queue<byte[]>();

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
                OnConnected?.Invoke();
            }
            catch (Exception)
            {
                OnConnectionFailed?.Invoke();
            }
        }

        public void Connect(string ip, int port)
        {
            _isServer = false;

            try
            {
                _remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                _udpClient = new UdpClient();
                _udpClient.Connect(_remoteEndPoint);

                BeginReceive();
                OnConnected?.Invoke();
            }
            catch (Exception)
            {
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

                if (_isServer)
                    _remoteEndPoint = sender;

                lock (_receivedDataQueue)
                    _receivedDataQueue.Enqueue(data);
            }

            catch (Exception e)
            {
                Debug.LogWarning($"UDPHandler receive error: {e.Message}");
            }

            if (_udpClient?.Client?.IsBound ?? false)
                BeginReceive();
        }

        public void Update()
        {
            lock (_receivedDataQueue)
            {
                while (_receivedDataQueue.Count > 0)
                    OnDataReceived?.Invoke(_receivedDataQueue.Dequeue());
            }
        }

        public void Send(byte[] data)
        {
            try
            {
                if (_isServer)
                {
                    if (_remoteEndPoint != null)
                        _udpClient.Send(data, data.Length, _remoteEndPoint);
                }

                else
                    _udpClient.Send(data, data.Length);
            }

            catch (Exception e)
            {
                Debug.LogWarning($"UDP Send failed: {e.Message}");
            }
        }

        public void Disconnect()
        {
            _udpClient?.Close();
            _udpClient = null;
        }
    }
}
