using Inputs;
using System;
using System.Text;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UI
{
    public class UIChatCanvas : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader inputReader;

        [Header("Scrolls")]
        [SerializeField] private ScrollRect chatScroll;

        [Header("Text")]
        [SerializeField] private TMP_Text chatText;
        [SerializeField] private TMP_InputField messageInputField;

        [Header("Buttons")]
        [SerializeField] private Button sendButton;

        private void OnEnable()
        {
            ValidateReferences();

            chatText.text = string.Empty;
            NetworkManager.Instance.OnDataReceived += OnReceiveData;
            sendButton.onClick.AddListener(OnSendMessage);

            inputReader.OnSendMessagePressed += OnSendMessage;
        }

        private void OnDisable()
        {
            NetworkManager.Instance.OnDataReceived -= OnReceiveData;
            sendButton.onClick.RemoveListener(OnSendMessage);

            inputReader.OnSendMessagePressed -= OnSendMessage;
        }

        private void UpdateScroll()
        {
            StartCoroutine(ScrollToBottomNextFrame());
        }

        private IEnumerator ScrollToBottomNextFrame()
        {
            yield return new WaitForEndOfFrame();
            chatScroll.verticalNormalizedPosition = 0f;
        }

        private void OnReceiveData(byte[] data)
        {
            chatText.text += Encoding.UTF8.GetString(data, 0, data.Length) + Environment.NewLine;
            UpdateScroll();
        }

        private void OnSendMessage()
        {
            if (string.IsNullOrEmpty(messageInputField.text))
                return;

            byte[] data = Encoding.UTF8.GetBytes(messageInputField.text);

            if (NetworkManager.Instance.IsServer)
            {
                chatText.text += messageInputField.text + Environment.NewLine;
                UpdateScroll();
            }

            NetworkManager.Instance.SendData(data);

            messageInputField.text = string.Empty;
            messageInputField.ActivateInputField();
        }

        private void ValidateReferences()
        {
            if (!inputReader)
            {
                Debug.LogError($"{name}: {nameof(inputReader)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }

            if (!chatScroll)
            {
                Debug.LogError($"{name}: {nameof(chatScroll)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }

            if (!chatText)
            {
                Debug.LogError($"{name}: {nameof(chatText)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }

            if (!messageInputField)
            {
                Debug.LogError($"{name}: {nameof(messageInputField)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }

            if (!sendButton)
            {
                Debug.LogError($"{name}: {nameof(sendButton)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }
        }
    }
}
