using Inputs;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIDialogueBox : MonoBehaviour
    {
        [Header("Layout References")]
        [SerializeField] private RectTransform contentRect;

        [Header("Text")]
        [SerializeField] private TMP_Text usernameText;
        [SerializeField] private TMP_Text messageText;

        [Header("Reply UI")]
        [SerializeField] private GameObject replyToPreviewObject;
        [SerializeField] private TMP_Text replyToPreviewText;
        [SerializeField] private Button replyButton;

        private RectTransform _dialogueBoxRect;

        private int _messageId;
        private string _username;
        private string _rawMessage;

        private UIChatCanvas _chat;

        public void Setup(UIChatCanvas chat, int messageId, string username, string message, int replyToId = -1)
        {
            ValidateReferences();

            _chat = chat;

            _messageId = messageId;
            _username = username;
            _rawMessage = message;

            usernameText.text = username;
            messageText.text = message;

            if (replyToId >= 0 && chat.TryGetMessagePreview(replyToId, out string preview, out string previewUsername))
            {
                replyToPreviewObject.SetActive(true);
                replyToPreviewText.text = $"Replying to {previewUsername}: {preview}";
            }

            else
                replyToPreviewObject.SetActive(false);

            replyButton.onClick.AddListener(OnReplyClicked);

            _dialogueBoxRect = GetComponent<RectTransform>();
            StartCoroutine(AdjustHeightNextFrame());
        }

        private void OnReplyClicked()
        {
            _chat.SetReplyTarget(_messageId, _username, _rawMessage);
        }

        private IEnumerator AdjustHeightNextFrame()
        {
            yield return new WaitForEndOfFrame();

            float targetHeight = contentRect.rect.height;

            Vector2 size = _dialogueBoxRect.sizeDelta;
            size.y = targetHeight;
            _dialogueBoxRect.sizeDelta = size;
        }

        private void ValidateReferences()
        {
            if (!ValidateReference(contentRect, nameof(contentRect))) return;

            if (!ValidateReference(usernameText, nameof(usernameText))) return;
            if (!ValidateReference(messageText, nameof(messageText))) return;

            if (!ValidateReference(replyToPreviewObject, nameof(replyToPreviewObject))) return;
            if (!ValidateReference(replyToPreviewText, nameof(replyToPreviewText))) return;
            if (!ValidateReference(replyButton, nameof(replyButton))) return;
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