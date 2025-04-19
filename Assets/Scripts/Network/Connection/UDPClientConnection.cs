using System.Net;
using System;
using System.Collections.Generic;

namespace Network.Connection
{
    public class UDPClientConnection
    {
        public IPEndPoint RemoteEndPoint { get; }
        private readonly Queue<byte[]> _dataQueue = new Queue<byte[]>();
        private readonly object _lock = new object();

        public UDPClientConnection() { }

        public UDPClientConnection(IPEndPoint endPoint)
        {
            RemoteEndPoint = endPoint;
        }

        public bool Matches(IPEndPoint endPoint)
        {
            return RemoteEndPoint?.Equals(endPoint) == true;
        }

        public void EnqueueData(byte[] data)
        {
            lock (_lock)
                _dataQueue.Enqueue(data);
        }

        public void FlushReceivedData(Action<byte[]> callback)
        {
            lock (_lock)
            {
                while (_dataQueue.Count > 0)
                    callback?.Invoke(_dataQueue.Dequeue());
            }
        }
    }
}
