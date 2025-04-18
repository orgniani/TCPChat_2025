using System.Net;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TCP;
using Inputs;

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

        [Header("Error UI")]
        [SerializeField] private GameObject errorCanvas;
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private Button closeButton;

        private void OnEnable()
        {
            ValidateReferences();

            chatCanvas.SetActive(false);
            errorCanvas.SetActive(false);

            startSeverButton.onClick.AddListener(OnStartServer);
            startClientButton.onClick.AddListener(OnConnectToServer);

            closeButton.onClick.AddListener(OnCloseErrorCanvas);

            TCPManager.Instance.OnConnectionFailed += HandleFailedConnection;
        }

        private void OnDisable()
        {
            startSeverButton.onClick.RemoveListener(OnStartServer);
            startClientButton.onClick.RemoveListener(OnConnectToServer);

            closeButton.onClick.RemoveListener(OnCloseErrorCanvas);

            TCPManager.Instance.OnConnectionFailed -= HandleFailedConnection;
        }

        private void OnStartServer()
        {
            if (string.IsNullOrWhiteSpace(serverPortField.text))
            {
                ShowError("Please enter a valid port.");
                return;
            }

            if (!int.TryParse(serverPortField.text, out int port) || port < 1024 || port > 65535)
            {
                ShowError("Port must be a number between 1024 and 65535.");
                return;
            }

            try
            {
                TCPManager.Instance.StartServer(port);
                MoveToChatScreen();
            }

            catch (Exception e)
            {
                ShowError($"Server error: {e.Message}");
            }
        }

        private void OnConnectToServer()
        {
            if (string.IsNullOrWhiteSpace(serverIpField.text) || string.IsNullOrWhiteSpace(serverPortField.text))
            {
                ShowError("Please enter both IP address and port.");
                return;
            }

            if (!IPAddress.TryParse(serverIpField.text, out IPAddress ipAddress))
            {
                ShowError("Invalid IP format.");
                return;
            }

            if (!int.TryParse(serverPortField.text, out int port) || port < 1024 || port > 65535)
            {
                ShowError("Port must be a number between 1024 and 65535.");
                return;
            }

            try
            {
                TCPManager.Instance.OnClientConnected += MoveToChatScreen;
                TCPManager.Instance.StartClient(ipAddress, port);
            }

            catch (Exception e)
            {
                ShowError($"Failed to connect to server: {e.Message}");
            }
        }

        private void MoveToChatScreen()
        {
            gameObject.SetActive(false);
            chatCanvas.SetActive(true);
        }

        private void ShowError(string message)
        {
            errorText.text = message;
            errorCanvas.SetActive(true);
        }

        private void OnCloseErrorCanvas()
        {
            errorCanvas.SetActive(false);
        }

        private void HandleFailedConnection()
        {
            ShowError("Failed to connect to the server. It may be offline or unreachable.");
        }

        private void ValidateReferences()
        {
            if (!serverIpField)
            {
                Debug.LogError($"{name}: {nameof(serverIpField)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }

            if (!serverPortField)
            {
                Debug.LogError($"{name}: {nameof(serverPortField)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }

            if (!startSeverButton)
            {
                Debug.LogError($"{name}: {nameof(startSeverButton)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }

            if (!startClientButton)
            {
                Debug.LogError($"{name}: {nameof(startClientButton)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }

            if (!chatCanvas)
            {
                Debug.LogError($"{name}: {nameof(chatCanvas)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }

            if (!errorCanvas)
            {
                Debug.LogError($"{name}: {nameof(errorCanvas)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }

            if (!errorText)
            {
                Debug.LogError($"{name}: {nameof(errorText)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }

            if (!closeButton)
            {
                Debug.LogError($"{name}: {nameof(closeButton)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }
        }
    }
}
