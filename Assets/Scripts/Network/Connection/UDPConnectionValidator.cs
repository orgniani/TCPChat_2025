using Core;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using UnityEngine;

namespace Network.Connection
{
    public class UDPConnectionValidator
    {
        private readonly UdpClient _client;
        private readonly IPEndPoint _target;

        private Action _onSuccess;
        private Action _onFailure;

        private bool _responseReceived = false;
        private float _timeout = 2f;
        private float _startTime;

        public UDPConnectionValidator(UdpClient client, IPEndPoint target)
        {
            _client = client;
            _target = target;
        }

        public void Validate(Action onSuccess, Action onFailure)
        {
            _onSuccess = onSuccess;
            _onFailure = onFailure;
            _startTime = Time.realtimeSinceStartup;

            byte[] ping = Encoding.UTF8.GetBytes("PING");
            _client.Send(ping, ping.Length, _target);

            Debug.Log($"Validator: Sent PING to {_target}");

            MainThreadDispatcher.Enqueue(WaitForResponse);
        }

        public void OnPongReceived()
        {
            Debug.Log("Validator: Received PONG!");
            _responseReceived = true;
        }

        private void WaitForResponse()
        {
            if (_responseReceived)
            {
                _onSuccess?.Invoke();
                return;
            }

            if (Time.realtimeSinceStartup - _startTime > _timeout)
            {
                _onFailure?.Invoke();
                return;
            }

            MainThreadDispatcher.Enqueue(WaitForResponse);
        }
    }
}