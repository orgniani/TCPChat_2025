using System;

namespace Network.Handlers
{
    public interface INetworkHandler
    {
        void Connect(string ip, int port);
        void StartServer(int port);
        void Send(byte[] data);
        void Disconnect();

        event Action<byte[]> OnDataReceived;
        event Action OnConnected;
        event Action OnConnectionFailed;
    }
}
