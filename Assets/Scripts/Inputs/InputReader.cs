using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Inputs
{
    public class InputReader : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] protected InputActionAsset inputActions;

        [Header("Keys")]
        [SerializeField] private string actionMapKey = "Chat";
        [SerializeField] private string sendMessageActionKey = "SendMessage";

        private InputActionMap _chatActionMap;
        private InputAction _sendMessageAction;

        public event Action OnSendMessagePressed;

        private void Awake()
        {
            ValidateReferences();

            _chatActionMap = inputActions.FindActionMap(actionMapKey, true);

            if (_chatActionMap == null)
            {
                Debug.LogError($"{name}: {nameof(_chatActionMap)} not found!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            _chatActionMap?.Enable();

            _sendMessageAction = inputActions.FindAction(sendMessageActionKey);

            if (_sendMessageAction != null)
            {
                _sendMessageAction.started += HandleSendMessage;
                _sendMessageAction.canceled += HandleSendMessage;
            }
        }

        private void OnDisable()
        {
            if (_sendMessageAction != null)
            {
                _sendMessageAction.started -= HandleSendMessage;
                _sendMessageAction.canceled -= HandleSendMessage;
            }
        }

        private void HandleSendMessage(InputAction.CallbackContext ctx)
        {
            if (ctx.phase == InputActionPhase.Started)
                OnSendMessagePressed?.Invoke();
        }

        private void ValidateReferences()
        {
            if (!inputActions)
            {
                Debug.LogError($"{name}: {nameof(inputActions)} is null!" +
                               $"\nDisabling component to avoid errors.");
                enabled = false;
                return;
            }
        }
    }
}
