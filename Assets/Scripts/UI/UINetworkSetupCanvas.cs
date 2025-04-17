using System.Net;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TCP;

namespace UI
{
    public class UINetworkSetupCanvas : MonoBehaviour
    {
        [Header("Text Fields")]
        [SerializeField] private TMP_InputField serverIpField;
        [SerializeField] private TMP_InputField serverPortField;

        [Header("Buttons")]
        [SerializeField] private Button startSeverButton;
        [SerializeField] private Button startClientButton;

        [Header("Canvas")]
        [SerializeField] private GameObject chatCanvas;

        void Awake()
        {
            chatCanvas.SetActive(false);

            startSeverButton.onClick.AddListener(OnStartServer);
            startClientButton.onClick.AddListener(OnConnectToServer);
        }

        void OnDestroy()
        {
            startSeverButton.onClick.RemoveListener(OnStartServer);
            startClientButton.onClick.RemoveListener(OnConnectToServer);
        }


        private void OnStartServer()
        {
            int port = Convert.ToInt32(serverPortField.text);
            TCPManager.Instance.StartServer(port);

            MoveToChatScreen();
        }

        private void OnConnectToServer()
        {
            IPAddress ipAddress = IPAddress.Parse(serverIpField.text);
            int port = Convert.ToInt32(serverPortField.text);

            TCPManager.Instance.StartClient(ipAddress, port);
            TCPManager.Instance.OnClientConnected += MoveToChatScreen;
        }

        private void MoveToChatScreen()
        {
            gameObject.SetActive(false);
            chatCanvas.SetActive(true);
        }
    }
}
