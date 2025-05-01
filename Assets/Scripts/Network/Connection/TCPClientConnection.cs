using System.Collections.Generic;
using System.Net.Sockets;
using System;
using UnityEngine;
using Core;

namespace Network.Connection
{
    public class TCPClientConnection
    {
        private TcpClient _client;
        private NetworkStream _stream;

        private byte[] _readBuffer = new byte[5000];
        private readonly object _readLock = new object();

        private readonly Queue<byte[]> _dataReceived = new Queue<byte[]>();

        public event Action<byte[]> OnDataReceived;

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action OnConnectionFailed;

        private bool _isConnected = false;
        public bool IsConnected => _isConnected;

        public TCPClientConnection(TcpClient client, bool isServer = false)
        {
            _client = client;

            if (isServer)
            {
                _stream = client.GetStream();
                _isConnected = true;
                BeginRead();
            }
        }

        public void OnEndConnection(IAsyncResult asyncResult)
        {
            try
            {
                _client.EndConnect(asyncResult);

                if (!_client.Connected)
                {
                    OnConnectionFailed?.Invoke();
                    return;
                }

                _stream = _client.GetStream();
                _isConnected = true;
                BeginRead();

                MainThreadDispatcher.Enqueue(() => OnConnected?.Invoke());
            }

            catch (Exception e)
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    Debug.LogError($"Client connection failed: {e.Message}");
                    OnConnectionFailed?.Invoke();
                });
            }
        }

        public void BeginRead()
        {
            if (_client.Connected)
            {
                _stream.BeginRead(_readBuffer, 0, _readBuffer.Length, OnRead, null);
            }

            else
            {
                Debug.LogWarning("BeginRead called but client not connected");
            }
        }


        private void OnRead(IAsyncResult asyncResult)
        {
            int bytesRead;

            try
            {
                bytesRead = _stream.EndRead(asyncResult);
            }
            catch
            {
                HandleDisconnect();
                return;
            }

            if (bytesRead == 0)
            {
                HandleDisconnect();
                return;
            }

            byte[] data = new byte[bytesRead];
            Array.Copy(_readBuffer, 0, data, 0, bytesRead);

            lock (_readLock)
            {
                _dataReceived.Enqueue(data);
            }

            Array.Clear(_readBuffer, 0, _readBuffer.Length);
            BeginRead();
        }

        public void FlushReceivedData()
        {
            lock (_readLock)
            {
                while (_dataReceived.Count > 0)
                {
                    byte[] data = _dataReceived.Dequeue();
                    OnDataReceived?.Invoke(data);
                }
            }
        }

        public void SendData(byte[] data)
        {
            if (_stream != null && _client.Connected)
                _stream.Write(data, 0, data.Length);
        }

        private void HandleDisconnect()
        {
            if (!_isConnected) return;

            _isConnected = false;
            Close();

            MainThreadDispatcher.Enqueue(() => OnDisconnected?.Invoke());
        }

        public void Close()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error closing connection: {e.Message}");
            }
        }
    }
}
