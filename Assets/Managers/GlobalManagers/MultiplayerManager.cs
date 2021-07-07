using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Chat;

public enum ForceDisconnectedReason {
    HostLeft = 0,
}

public class MultiplayerManager : GlobalSingleton<MultiplayerManager>, IOnEventCallback {
    private static string m_ServerName = "";
    private static string m_PlayerName = "";
    private static int m_PlayerID = 0;

    private static bool m_ForceDisconnected = false;
    private static ForceDisconnectedReason m_ForceDisconnectedReason = ForceDisconnectedReason.HostLeft;

    private static bool m_GameOver = false;

    public static string ServerName {
        get => m_ServerName;
        set => m_ServerName = value;
    }

    public static string PlayerName {
        get => m_PlayerName;
        set => m_PlayerName = value;
    }

    public static int PlayerID {
        get => m_PlayerID;
        set => m_PlayerID = value;
    }

    public static bool ForceDisconnected {
        get => m_ForceDisconnected;
        set => m_ForceDisconnected = value;
    }

    public static ForceDisconnectedReason ForceDisconnectedReason {
        get => m_ForceDisconnectedReason;
        set => m_ForceDisconnectedReason = value;
    }

    public static bool GameOver {
        get => m_GameOver;
        set => m_GameOver = value;
    }

    public static bool IsMultiplayer => PhotonNetwork.OfflineMode == false && PhotonNetwork.IsConnected;

    private static ChatEvent m_OnMessageReceived = new ChatEvent();
    public static ChatEvent OnMessageReceived => m_OnMessageReceived;

    protected override void OnInit() {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public static void SendChatMessage(MessageType messageType, string message) {
        object[] content = new object[] {
            messageType,
            PlayerName,
            message,
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions() {
            Receivers = ReceiverGroup.All,
        };

        PhotonNetwork.RaiseEvent(MultiplayerEventCode.Chat, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public static void LoadScene(string sceneName) {
        object[] content = new object[] {
            sceneName,
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions() {
            Receivers = ReceiverGroup.All,
        };

        PhotonNetwork.RaiseEvent(MultiplayerEventCode.LoadScene, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public static void SetPlayerID(Player player, int playerID) {
        object content = new object[] {
            playerID,
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions() {
            TargetActors = new int[] { player.ActorNumber },
        };

        PhotonNetwork.RaiseEvent(MultiplayerEventCode.SetPlayerID, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent) {
        byte eventCode = photonEvent.Code;

        if (eventCode == MultiplayerEventCode.Chat) {
            HandleMultiplayerEventChat(photonEvent);
        }
        else if (eventCode == MultiplayerEventCode.LoadScene) {
            HandleMultiplayerEventLoadScene(photonEvent);
        }
        else if (eventCode == MultiplayerEventCode.SetPlayerID) {
            HandleSetPlayerID(photonEvent);
        }
    }

    void HandleMultiplayerEventChat(EventData photonEvent) {
        object[] data = (object[]) photonEvent.CustomData;

        MessageType messageType = (MessageType) data[0];
        string sender = (string) data[1];
        string message = (string) data[2];

        OnMessageReceived.Invoke(messageType, sender, message);
    }

    void HandleMultiplayerEventLoadScene(EventData photonEvent) {
        object[] data = (object[]) photonEvent.CustomData;

        data = (object[]) photonEvent.CustomData;

        string sceneName = (string) data[0];

        SceneManager.LoadScene(sceneName);
    }

    void HandleSetPlayerID(EventData photonEvent) {
        object[] data = (object[]) photonEvent.CustomData;

        data = (object[]) photonEvent.CustomData;

        PlayerID = (int) data[0];
    }
}
