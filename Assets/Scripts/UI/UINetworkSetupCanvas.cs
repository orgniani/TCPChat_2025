using System.Net;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Network;
using Core;
using Inputs;

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

        private NetworkErrorType _currentErrorType = NetworkErrorType.Unknown;

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

            if (NetworkManager.Instance) NetworkManager.Instance.OnConnectionFailed -= HandleFailedConnection;
        }

        private void OnStartServer()
        {
            if (!ValidateInputs(out string ip, out int port)) return;

            NetworkProtocol selectedProtocol = (NetworkProtocol)protocolDropdown.value;
            NetworkManager.Instance.SetProtocol(selectedProtocol);

            UserInfoManager.Instance.Username = usernameField.text;

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

            UserInfoManager.Instance.Username = usernameField.text;

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
            chatCanvas.SetActive(true);
        }

        private void ShowError(string message, NetworkErrorType type = NetworkErrorType.Unknown)
        {
            _currentErrorType = type;
            errorText.text = message;

            errorCanvas.SetActive(true);
        }

        private void OnCloseErrorCanvas()
        {
            if (_currentErrorType == NetworkErrorType.ServerDisconnected)
            {
                Application.Quit();

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }

            errorCanvas.SetActive(false);
        }

        private void HandleFailedConnection()
        {
            Debug.LogWarning("HandleFailedConnection CALLED!");

            MainThreadDispatcher.Enqueue(() =>
            {
                NetworkManager.Instance.OnClientConnected -= MoveToChatScreen;

                var wasInChat = chatCanvas.activeSelf;
                var type = wasInChat ? NetworkErrorType.ServerDisconnected : NetworkErrorType.ConnectionFailed;

                ShowError("Failed to connect to the server. It may be offline or unreachable.", type);
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
            if (!ValidateReference(protocolDropdown, nameof(protocolDropdown))) return;

            if (!ValidateReference(serverIpField, nameof(serverIpField))) return;
            if (!ValidateReference(serverPortField, nameof(serverPortField))) return;
            if (!ValidateReference(usernameField, nameof(usernameField))) return;

            if (!ValidateReference(startSeverButton, nameof(startSeverButton))) return;
            if (!ValidateReference(startClientButton, nameof(startClientButton))) return;

            if (!ValidateReference(chatCanvas, nameof(chatCanvas))) return;

            if (!ValidateReference(errorCanvas, nameof(errorCanvas))) return;
            if (!ValidateReference(errorText, nameof(errorText))) return;
            if (!ValidateReference(closeButton, nameof(closeButton))) return;
        }

        private bool ValidateReference(UnityEngine.Object reference, string referenceName)
        {
            if (reference != null) return true;

            Debug.LogError($"{name}: {referenceName} is null!" +
                           $"\nDisabling component to avoid errors.");
            enabled = false;
            return false;
        }
    }
}
