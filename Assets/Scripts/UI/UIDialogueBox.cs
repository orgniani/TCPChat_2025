using System.Collections;
using TMPro;
using UnityEngine;

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

        private RectTransform _dialogueBoxRect;

        public void Setup(string username, string message)
        {
            usernameText.text = username;
            messageText.text = message;

            _dialogueBoxRect = GetComponent<RectTransform>();
            StartCoroutine(AdjustHeightNextFrame());
        }

        private IEnumerator AdjustHeightNextFrame()
        {
            yield return new WaitForEndOfFrame();

            float targetHeight = contentRect.rect.height;

            Vector2 size = _dialogueBoxRect.sizeDelta;
            size.y = targetHeight;
            _dialogueBoxRect.sizeDelta = size;
        }
    }
}