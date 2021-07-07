using UnityEngine;
using View;

public class StartScene : MonoBehaviour {
    [SerializeField] private ViewBase m_MainView = null;
    [SerializeField] private ViewBase m_ServerRoomView = null;

    void Start() {
        AudioManager.PlayMusic(AudioManager.Instance.MainMusic);

        if (MultiplayerManager.ForceDisconnected) {
            MultiplayerManager.ForceDisconnected = false;
            
            Dialog.Alert(new AlertOptions() {
                Message = LanguageManager.Data.Multiplayer.Message.HostLeft,
                OnClose = AudioManager.PlayClickSound,
            });
        }

        if (MultiplayerManager.GameOver) {
            ViewManager.SwitchView(m_ServerRoomView);
        }
        else {
            ViewManager.SwitchView(m_MainView);
        }
    }
}
