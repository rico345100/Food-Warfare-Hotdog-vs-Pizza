using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Chat;

namespace View.StartScene {
    public partial class ServerRoomView {
        [Header("Chat")]
        [SerializeField] private Text m_ChatTitleText = null;
        [SerializeField] private InputField m_ChatInputField = null;
        [SerializeField] private Button m_ChatSendButton = null;
        [SerializeField] private ScrollRect m_ChatScrollRect = null;
        [SerializeField] private Transform m_ChatContainer = null;
        [SerializeField] private GameObject m_ChatMessagePrefab = null;

        private WaitForSeconds m_WaitBeforeScrollUp = new WaitForSeconds(0.1f);

        void Update() {
            HandleMessageSubmit();
        }

        void HandleMessageSubmit() {
            if (m_ChatInputField.interactable) {
                if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)) {
                    SubmitChatMessage();
                }
                else if (Input.GetKey(KeyCode.Escape)) {
                    m_ChatInputField.text = "";
                }
            }
        }

        void EnableChat() {
            m_ChatInputField.text = "";
            m_ChatInputField.ActivateInputField();
        }

        void DisableChat() {
            MultiplayerManager.OnMessageReceived.RemoveListener(HandleMessageReceive);
        }

        void SubmitChatMessage() {
            string message = m_ChatInputField.text;

            if (string.IsNullOrEmpty(message)) {
                return;
            }

            MultiplayerManager.SendChatMessage(MessageType.User, message);

            m_ChatInputField.text = "";
            m_ChatInputField.ActivateInputField();
        }

        void HandleMessageReceive(MessageType messageType, string sender, string message) {
            if (messageType.Equals(MessageType.System)) {
                CreateSystemMessage(message);
            }
            else {
                CreateMessage(sender, message);
            }
        }

        void CreateMessage(string sender, string message) {
            GameObject messageObject = Instantiate(m_ChatMessagePrefab);

            messageObject.GetComponent<Text>().text = string.Format(
                "<b><color=#FFF100>{0}</color></b>: {1}",
                sender,
                message
            );

            messageObject.transform.SetParent(m_ChatContainer, false);

            StartCoroutine(CoScrollChatContainerToBottom());
        }

        void CreateSystemMessage(string message) {
            GameObject messageObject = Instantiate(m_ChatMessagePrefab);

            messageObject.GetComponent<Text>().text = string.Format(
                "<b><i><color=#00F8FF>{0}</color></i></b>",
                message
            );

            messageObject.transform.SetParent(m_ChatContainer, false);

            StartCoroutine(CoScrollChatContainerToBottom());
        }

        IEnumerator CoScrollChatContainerToBottom() {
            yield return m_WaitBeforeScrollUp;
            m_ChatScrollRect.normalizedPosition = new Vector2(0, 0);
        }
    }
}