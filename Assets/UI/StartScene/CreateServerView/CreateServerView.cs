using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Localization;
using System.Collections.Generic;

namespace View.StartScene {
    public class CreateServerView: ViewBase, IConnectionCallbacks, IMatchmakingCallbacks {
        [SerializeField] private ViewBase m_MainView = null;
        [SerializeField] private ViewBase m_ServerListView = null;
        [SerializeField] private ViewBase m_ServerRoomView = null;
        [SerializeField] private Text m_TitleText = null;
        [SerializeField] private Button m_ExitButton = null;
        [SerializeField] private Text m_ServerNameLabel = null;
        [SerializeField] private InputField m_ServerNameInputField = null;
        [SerializeField] private Text m_MapLabel = null;
        [SerializeField] private Dropdown m_MapDropdown = null;
        [SerializeField] private Text m_MaxPlayersLabel = null;
        [SerializeField] private Dropdown m_MaxPlayersDropdown = null;
        [SerializeField] private Text m_ErrorMessage = null;
        [SerializeField] private Button m_CreateButton = null;
        [SerializeField] private GameObject m_CreatingServerIndicator = null;
        private const string m_ServerNamePrefsKey = "ServerName";

        protected override void OnInitialize() {
            m_ExitButton.onClick.AddListener(SwitchToServerListView);
            m_CreateButton.onClick.AddListener(CreateServer);
        }
        
        protected override void OnShow() {
            HideErrorMessage();
            m_CreatingServerIndicator.SetActive(false);

            m_ServerNameInputField.text = PlayerPrefs.GetString(m_ServerNamePrefsKey, "");
            m_MapDropdown.value = 0;
            m_MaxPlayersDropdown.value = 0;

            PhotonNetwork.AddCallbackTarget(this);
        }
        
        protected override void OnHide() {
            m_CreatingServerIndicator.SetActive(false);
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public override void OnLanguageChanged(LanguageScheme locale) {
            m_TitleText.text = locale.View.CreateServer.Title;
            m_ExitButton.GetComponentInChildren<Text>().text = locale.View.CreateServer.Exit;
            m_CreateButton.GetComponentInChildren<Text>().text = locale.View.CreateServer.CreateServer;

            m_ServerNameLabel.text = locale.View.CreateServer.ServerName;
            m_ServerNameInputField.placeholder.GetComponent<Text>().text = locale.View.CreateServer.ServerNamePlaceholder;
            m_MapLabel.text = locale.View.CreateServer.Map;
            m_MaxPlayersLabel.text = locale.View.CreateServer.MaxPlayers;

            m_CreatingServerIndicator.GetComponentInChildren<Text>().text = locale.Multiplayer.Message.CreatingServer;
        }

        void SwitchToServerListView() {
            ViewManager.SwitchView(m_ServerListView);
            AudioManager.PlayCancelSound();
        }

        void SwitchToServerRoomView() {
            ViewManager.SwitchView(m_ServerRoomView);
            AudioManager.PlayClickSound();
        }

        void SwitchToMainView() {
            ViewManager.SwitchView(m_MainView);
            AudioManager.PlayCancelSound();
        }

        void CreateServer() {
            LanguageSchemeViewCreateServer locale = LanguageManager.Data.View.CreateServer;
            string serverName = m_ServerNameInputField.text;

            if (string.IsNullOrEmpty(serverName)) {
                ShowErrorMessage(locale.RequiredServerName);
                AudioManager.PlayErrorSound();
                return;
            }

            PlayerPrefs.SetString(m_ServerNamePrefsKey, serverName);
            PlayerPrefs.Save();

            HideErrorMessage();

            m_CreatingServerIndicator.SetActive(true);

            MultiplayerManager.ServerName = serverName;

            PhotonNetwork.CreateRoom(serverName, new RoomOptions() {
                MaxPlayers = 2,
            });
        }

        void ShowErrorMessage(string message) {
            m_ErrorMessage.text = message;
            m_ErrorMessage.gameObject.SetActive(true);
        }

        void HideErrorMessage() {
            m_ErrorMessage.gameObject.SetActive(false);
        }

        public void OnConnected() {}

        public void OnConnectedToMaster() {}

        public void OnDisconnected(DisconnectCause cause) {
            Dialog.Alert(new AlertOptions() {
                Message = string.Format(
                    "{0} ({1})",
                    LanguageManager.Data.Multiplayer.Message.Disconnected,
                    cause.ToString()
                ),
                OnClose = SwitchToMainView,
            });
        }

        public void OnRegionListReceived(RegionHandler regionHandler) {}

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data) {}

        public void OnCustomAuthenticationFailed(string debugMessage) {}

        public void OnFriendListUpdate(List<FriendInfo> friendList) {}

        public void OnCreatedRoom() {}

        public void OnCreateRoomFailed(short returnCode, string message) {
            Dialog.Alert(new AlertOptions() {
                Message = string.Format(
                    "{0} ({1} {2})",
                    LanguageManager.Data.Multiplayer.Message.CreateServerFailure,
                    returnCode.ToString(),
                    message
                ),
            });

            AudioManager.PlayErrorSound();
            m_CreatingServerIndicator.SetActive(false);
        }

        public void OnJoinedRoom() {
            SwitchToServerRoomView();
        }

        public void OnJoinRoomFailed(short returnCode, string message) {
            Dialog.Alert(new AlertOptions() {
                Message = string.Format(
                    "{0} ({1} {2})",
                    LanguageManager.Data.Multiplayer.Message.JoinServerFailure,
                    returnCode.ToString(),
                    message
                )
            });
            
            AudioManager.PlayErrorSound();
            m_CreatingServerIndicator.SetActive(false);
        }

        public void OnJoinRandomFailed(short returnCode, string message) {}

        public void OnLeftRoom() {}
    }
}
