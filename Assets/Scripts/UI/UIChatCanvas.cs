using System;
using System.Text;
using TCP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIChatCanvas : MonoBehaviour
    {
        [Header("Scrolls")]
        [SerializeField] private ScrollRect chatScroll;

        [Header("Text")]
        [SerializeField] private TMP_Text chatText;
        [SerializeField] private TMP_InputField messageInputField;

        [Header("Buttons")]
        [SerializeField] private Button sendButton;

        void Start()
        {
            chatText.text = string.Empty;
            TCPManager.Instance.OnDataReceived += OnReceiveData;
            sendButton.onClick.AddListener(OnSendMessage);
        }

        void OnDestroy()
        {
            TCPManager.Instance.OnDataReceived -= OnReceiveData;
            sendButton.onClick.RemoveListener(OnSendMessage);
        }


        private void UpdateScroll()
        {
            chatScroll.verticalNormalizedPosition = 0f;
        }

        private void OnReceiveData(byte[] data)
        {
            if (TCPManager.Instance.IsServer)
                TCPManager.Instance.BroadcastData(data);

            chatText.text += Encoding.UTF8.GetString(data, 0, data.Length) + Environment.NewLine;
            UpdateScroll();
        }

        private void OnSendMessage()
        {
            if (string.IsNullOrEmpty(messageInputField.text))
                return;

            byte[] data = Encoding.UTF8.GetBytes(messageInputField.text);

            if (TCPManager.Instance.IsServer)
            {
                chatText.text += messageInputField.text + Environment.NewLine;
                UpdateScroll();
                TCPManager.Instance.BroadcastData(data);
            }

            else
                TCPManager.Instance.SendDataToServer(data);

            messageInputField.text = string.Empty;
        }
    }
}
