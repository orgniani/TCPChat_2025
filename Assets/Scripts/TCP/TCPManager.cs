using Core;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine;

namespace TCP
{
    public class TCPManager : MonoBehaviourSingleton<TCPManager>
    {
        private readonly List<TCPConnectedClient> _serverClients = new List<TCPConnectedClient>();
        private TCPConnectedClient _connectedClient;
        private TcpListener _listener;
        private bool _clientJustConnected;

        public bool IsServer { get; private set; }

        public event Action<byte[]> OnDataReceived;
        public event Action OnClientConnected;
        public event Action OnConnectionFailed;

        private void Update()
        {
            if (IsServer)
                UpdateServer();

            else
                UpdateClient();
        }

        private void OnDestroy()
        {
            _listener?.Stop();

            foreach (TCPConnectedClient client in _serverClients)
                client.CloseClient();

            if (_connectedClient) _connectedClient.CloseClient(); //TODO: Check if this is correct
        }

        private void UpdateServer()
        {
            foreach (TCPConnectedClient client in _serverClients)
                client.FlushReceivedData();
        }

        private void UpdateClient()
        {
            if (_clientJustConnected)
                OnClientConnected?.Invoke();

            _connectedClient?.FlushReceivedData();
        }

        private void OnClientConnectToServer(IAsyncResult asyncResult)
        {
            TcpClient client = _listener.EndAcceptTcpClient(asyncResult);
            TCPConnectedClient connectedClient = new TCPConnectedClient(client);

            _serverClients.Add(connectedClient);
            _listener.BeginAcceptTcpClient(OnClientConnectToServer, null);
        }

        private void OnConnectClient(IAsyncResult asyncResult)
        {
            _connectedClient.OnEndConnection(asyncResult);
            _clientJustConnected = true;
        }

        public void StartServer(int port)
        {
            IsServer = true;
            _listener = new TcpListener(IPAddress.Any, port);

            _listener.Start();
            _listener.BeginAcceptTcpClient(OnClientConnectToServer, null);
        }

        public void StartClient(IPAddress serverIp, int port)
        {
            IsServer = false;

            TcpClient client = new TcpClient();

            _connectedClient = new TCPConnectedClient(client);

            client.BeginConnect(serverIp, port, OnConnectClient, null);
        }

        public void ReceiveData(byte[] data)
        {
            OnDataReceived?.Invoke(data);
        }

        public void DisconnectClient(TCPConnectedClient client)
        {
            if (_serverClients.Contains(client))
                _serverClients.Remove(client);
        }

        public void BroadcastData(byte[] data)
        {
            foreach (TCPConnectedClient client in _serverClients)
                client.SendData(data);
        }

        public void SendDataToServer(byte[] data)
        {
            _connectedClient.SendData(data);
        }

        public void NotifyConnectionFailed()
        {
            OnConnectionFailed?.Invoke();
        }
    }
}
