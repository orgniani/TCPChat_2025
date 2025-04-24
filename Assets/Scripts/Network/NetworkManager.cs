using Core;
using System;
using Network.Handlers;
using UnityEngine;

namespace Network
{
    public class NetworkManager : MonoBehaviourSingleton<NetworkManager>
    {
        private INetworkHandler _handler;

        public NetworkProtocol ProtocolType { get; private set; }
        public bool IsServer { get; private set; }

        public event Action<byte[]> OnDataReceived;
        public event Action OnClientConnected;
        public event Action OnConnectionFailed;

        private void Update()
        {
            if (_handler is IUpdatableHandler updatable)
                updatable.Update();
        }

        public void SetProtocol(NetworkProtocol protocol)
        {
            ProtocolType = protocol;

            if (protocol == NetworkProtocol.TCP)
                _handler = new TCPHandler();

            else if (protocol == NetworkProtocol.UDP)
                _handler = new UDPHandler();

            _handler.OnDataReceived += (data) =>
            {
                if (IsServer) 
                    _handler.Send(data);

                OnDataReceived?.Invoke(data);
            };

            _handler.OnConnected += () => OnClientConnected?.Invoke();
            _handler.OnConnectionFailed += () => OnConnectionFailed?.Invoke();
        }


        public void StartServer(int port)
        {
            IsServer = true;
            _handler?.StartServer(port);
        }

        public void ConnectToServer(string ip, int port)
        {
            IsServer = false;
            _handler?.Connect(ip, port);
        }

        public void SendData(byte[] data)
        {
            _handler?.Send(data);
        }

        public void Disconnect()
        {
            _handler?.Disconnect();
        }
    }
}
