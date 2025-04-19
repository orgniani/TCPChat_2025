using System.Net;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Network;
using Core;

namespace UI
{
    public class UINetworkSetupCanvas : MonoBehaviour
    {
        [Header("Drop Down")]
        [SerializeField] private TMP_Dropdown protocolDropdown;

        [Header("Text Fields")]
        [SerializeField] private TMP_InputField serverIpField;
        [SerializeField] private TMP_InputField serverPortField;
        [SerializeField] private TMP_InputField usernameField;

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

            NetworkManager.Instance.OnConnectionFailed += HandleFailedConnection;
        }

        private void OnDisable()
        {
            startSeverButton.onClick.RemoveListener(OnStartServer);
            startClientButton.onClick.RemoveListener(OnConnectToServer);

            closeButton.onClick.RemoveListener(OnCloseErrorCanvas);

            NetworkManager.Instance.OnConnectionFailed -= HandleFailedConnection;
        }

        private void OnStartServer()
        {
            if (!ValidateInputs(out string ip, out int port)) return;

            NetworkProtocol selectedProtocol = (NetworkProtocol)protocolDropdown.value;
            NetworkManager.Instance.SetProtocol(selectedProtocol);

            //UserData.Username = usernameField.text;

            try
            {
                NetworkManager.Instance.StartServer(port);
                MoveToChatScreen();
            }

            catch (Exception e)
            {
                ShowError($"Server error: {e.Message}");
            }
        }

        private void OnConnectToServer()
        {
            if (!ValidateInputs(out string ip, out int port)) return;

            NetworkProtocol selectedProtocol = (NetworkProtocol)protocolDropdown.value;
            NetworkManager.Instance.SetProtocol(selectedProtocol);

            //UserData.Username = usernameField.text;

            NetworkManager.Instance.OnClientConnected += MoveToChatScreen;

            try
            {
                NetworkManager.Instance.ConnectToServer(ip, port);
            }

            catch (Exception e)
            {
                ShowError($"Failed to connect to server: {e.Message}");
            }
        }

        private void MoveToChatScreen()
        {
            NetworkManager.Instance.OnClientConnected -= MoveToChatScreen;
            NetworkManager.Instance.OnConnectionFailed -= HandleFailedConnection;

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
            MainThreadDispatcher.Enqueue(() =>
            {
                NetworkManager.Instance.OnClientConnected -= MoveToChatScreen;
                NetworkManager.Instance.OnConnectionFailed -= HandleFailedConnection;

                ShowError("Failed to connect to the server. It may be offline or unreachable.");
            });
        }

        private bool ValidateInputs(out string ip, out int port)
        {
            ip = serverIpField.text;
            port = 0;

            if (string.IsNullOrWhiteSpace(usernameField.text))
            {
                ShowError("Enter a username.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(ip) || !IPAddress.TryParse(ip, out _))
            {
                ShowError("Invalid IP.");
                return false;
            }

            if (!int.TryParse(serverPortField.text, out port) || port < 1024 || port > 65535)
            {
                ShowError("Port must be between 1024 and 65535.");
                return false;
            }

            return true;
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
