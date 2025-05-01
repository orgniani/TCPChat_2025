using Inputs;
using System;
using System.Text;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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

        [Header("Reply UI")]
        [SerializeField] private GameObject replyPreviewObject;
        [SerializeField] private TMP_Text replyPreviewText;
        [SerializeField] private Button cancelReplyButton;

        private int _replyingToId = -1;

        private Dictionary<int, ChatMessage> _messageDict = new();
        private int _messageCounter = 0;

        private void OnEnable()
        {
            ValidateReferences();

            NetworkManager.Instance.OnDataReceived += OnReceiveData;
            inputReader.OnSendMessagePressed += OnSendMessage;

            sendButton.onClick.AddListener(OnSendMessage);
            cancelReplyButton.onClick.AddListener(CancelReply);

            replyPreviewObject.SetActive(false);
        }

        private void OnDisable()
        {
            NetworkManager.Instance.OnDataReceived -= OnReceiveData;
            sendButton.onClick.RemoveListener(OnSendMessage);

            inputReader.OnSendMessagePressed -= OnSendMessage;

            CancelReply();
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
            string[] parts = received.Split('|');

            if (parts.Length == 3)
            {
                string username = parts[0];
                int.TryParse(parts[1], out int replyToId);
                string message = parts[2];

                AddDialogue(username, message, replyToId);
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

            string fullMessage = $"{username}|{_replyingToId}|{message}";
            byte[] data = Encoding.UTF8.GetBytes(fullMessage);

            if (NetworkManager.Instance.IsServer)
            {
                AddDialogue(username, message, _replyingToId);
            }

            NetworkManager.Instance.SendData(data);

            CancelReply();
        }

        private void AddDialogue(string username, string message, int replyToId)
        {
            _messageDict[_messageCounter] = new ChatMessage(username, message);

            var dialogueBox = Instantiate(dialogueBoxPrefab, chatContentParent);
            dialogueBox.Setup(this, _messageCounter, username, message, replyToId);
            _messageCounter++;

            UpdateScroll();

        }

        public void SetReplyTarget(int messageId, string author, string messagePreview)
        {
            _replyingToId = messageId;
            replyPreviewText.text = $"Replying to {author}: {TrimMessage(messagePreview)}";
            replyPreviewObject.SetActive(true);
        }


        public bool TryGetMessagePreview(int id, out string preview, out string username)
        {
            if (_messageDict.TryGetValue(id, out var chatMessage))
            {
                preview = TrimMessage(chatMessage.Message);
                username = chatMessage.Username;
                return true;
            }

            preview = "Message not found";
            username = "Unknown";
            return false;
        }

        private string TrimMessage(string message)
        {
            return message.Length > 50 ? message.Substring(0, 50) + "..." : message;
        }

        private void CancelReply()
        {
            _replyingToId = -1;
            replyPreviewObject.SetActive(false);

            messageInputField.text = string.Empty;
            messageInputField.ActivateInputField();
        }

        private void ValidateReferences()
        {
            if (!ValidateReference(inputReader, nameof(inputReader))) return;
            if (!ValidateReference(chatScroll, nameof(chatScroll))) return;

            if (!ValidateReference(dialogueBoxPrefab, nameof(dialogueBoxPrefab))) return;
            if (!ValidateReference(chatContentParent, nameof(chatContentParent))) return;

            if (!ValidateReference(messageInputField, nameof(messageInputField))) return;
            if (!ValidateReference(sendButton, nameof(sendButton))) return;

            if (!ValidateReference(replyPreviewObject, nameof(replyPreviewObject))) return;
            if (!ValidateReference(replyPreviewText, nameof(replyPreviewText))) return;
            if (!ValidateReference(cancelReplyButton, nameof(cancelReplyButton))) return;
        }

        private bool ValidateReference(UnityEngine.Object reference, string referenceName)
        {
            if (reference != null) return true;

            Debug.LogError($"{name}: {referenceName} is null!" +
                           $"\nDisabling component to avoid errors.");
            enabled = false;
            return false;
        }

        private struct ChatMessage
        {
            public string Username;
            public string Message;

            public ChatMessage(string username, string message)
            {
                Username = username;
                Message = message;
            }
        }
    }
}
