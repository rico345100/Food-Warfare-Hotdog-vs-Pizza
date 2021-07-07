using UnityEngine;
using Chat;

public partial class InGameManager {
    void HandleSendMessageInput() {
        if (Input.GetKey(KeyCode.T)) {
            if (InGameUIManager.Instance.SendMessageContainerVisible == false) {
                InGameUIManager.Instance.ShowSendMessage();
            }
        }
        else if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)) {
            if (InGameUIManager.Instance.SendMessageContainerVisible == false) {
                return;
            }

            HandleMessageSubmit();
        }
        else if (Input.GetKey(KeyCode.Escape)) {
            InGameUIManager.Instance.HideSendMessage();
        }
    }

    void HandleMessageSubmit() {
        string message = InGameUIManager.Instance.SendMessageInputField.text;

        if (string.IsNullOrEmpty(message) == false) {
            MultiplayerManager.SendChatMessage(MessageType.User, message);
        }
        
        InGameUIManager.Instance.HideSendMessage();
    }

    void HandleMessageReceive(MessageType messageType, string sender, string message) {
        if (messageType.Equals(MessageType.System)) {
            InGameUIManager.Instance.CreateSystemMessage(message);
        }
        else {
            InGameUIManager.Instance.CreateMessage(sender, message);
        }
    }
}
