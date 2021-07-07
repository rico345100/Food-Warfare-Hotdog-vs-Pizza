using UnityEngine.Events;

namespace Chat {
    public class ChatEvent: UnityEvent<MessageType, string, string> {}
}
