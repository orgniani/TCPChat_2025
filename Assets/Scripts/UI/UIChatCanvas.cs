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

        [Header("Dialogue Box")]
        [SerializeField] private UIDialogueBox dialogueBoxPrefab;
        [SerializeField] private Transform chatContentParent;

        [Header("Text")]
        [SerializeField] private TMP_InputField messageInputField;

        [Header("Buttons")]
        [SerializeField] private Button sendButton;

        private void OnEnable()
        {
            ValidateReferences();

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
            string received = Encoding.UTF8.GetString(data, 0, data.Length);
            string[] parts = received.Split(new[] { ':' }, 2);

            if (parts.Length == 2)
            {
                string username = parts[0];
                string message = parts[1];

                AddDialogue(username, message);
            }

            else
                Debug.LogWarning($"Invalid message format: {received}");
        }


        private void OnSendMessage()
        {
            if (string.IsNullOrEmpty(messageInputField.text))
                return;

            string message = messageInputField.text;
            string username = UserInfoManager.Instance.Username;

            string fullMessage = $"{username}:{message}";
            byte[] data = Encoding.UTF8.GetBytes(fullMessage);

            if (NetworkManager.Instance.IsServer)
            {
                AddDialogue(username, message);
            }

            NetworkManager.Instance.SendData(data);

            messageInputField.text = string.Empty;
            messageInputField.ActivateInputField();
        }

        private void AddDialogue(string username, string message)
        {
            var dialogueBox = Instantiate(dialogueBoxPrefab, chatContentParent);
            dialogueBox.Setup(username, message);
            UpdateScroll();
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
