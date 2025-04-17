using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System;
using UnityEngine;

namespace TCP
{
    public class TCPConnectedClient : MonoBehaviour
    {
        private TcpClient _client;
        private Queue<byte[]> _dataReceived = new Queue<byte[]>();

        private byte[] _readBuffer = new byte[5000];
        private object _readHandler = new object();

        private NetworkStream NetworkStream => _client?.GetStream();

        public TCPConnectedClient(TcpClient client)
        {
            this._client = client;

            if (TCPManager.Instance.IsServer)
                NetworkStream.BeginRead(_readBuffer, 0, _readBuffer.Length, OnRead, null);
        }

        private void OnRead(IAsyncResult asyncResult)
        {
            if (NetworkStream.EndRead(asyncResult) == 0)
            {
                TCPManager.Instance.DisconnectClient(this);
                return;
            }

            lock (_readHandler)
            {
                byte[] data = _readBuffer.TakeWhile(b => (char)b != '\0').ToArray();
                _dataReceived.Enqueue(data);
            }

            Array.Clear(_readBuffer, 0, _readBuffer.Length);
            NetworkStream.BeginRead(_readBuffer, 0, _readBuffer.Length, OnRead, null);
        }

        public void SendData(byte[] data)
        {
            NetworkStream.Write(data, 0, data.Length);
        }

        public void FlushReceivedData()
        {
            lock (_readHandler)
            {
                while (_dataReceived.Count > 0)
                {
                    byte[] data = _dataReceived.Dequeue();
                    TCPManager.Instance.ReceiveData(data);
                }
            }
        }

        public void OnEndConnection(IAsyncResult asyncResult)
        {
            _client.EndConnect(asyncResult);
            NetworkStream.BeginRead(_readBuffer, 0, _readBuffer.Length, OnRead, null);
        }

        public void CloseClient()
        {
            _client.Close();
        }
    }
}
