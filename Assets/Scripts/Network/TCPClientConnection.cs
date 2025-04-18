using System.Collections.Generic;
using System.Net.Sockets;
using System;
using UnityEngine;

namespace Network
{
    public class TCPClientConnection
    {
        private TcpClient _client;
        private NetworkStream _stream;

        private byte[] _readBuffer = new byte[5000];
        private readonly object _readLock = new object();

        private readonly Queue<byte[]> _dataReceived = new Queue<byte[]>();

        public event Action<byte[]> OnDataReceived;
        public event Action OnDisconnected;
        public event Action OnConnectionFailed;

        public bool IsConnected { get; set; } = false;

        public TCPClientConnection(TcpClient client, bool isServer = false)
        {
            _client = client;

            if (isServer)
            {
                _stream = client.GetStream();
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
                BeginRead();

                IsConnected = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Client connection failed: {e.Message}");
                OnConnectionFailed?.Invoke();
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
                OnDisconnected?.Invoke();
                return;
            }

            if (bytesRead == 0)
            {
                OnDisconnected?.Invoke();
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
