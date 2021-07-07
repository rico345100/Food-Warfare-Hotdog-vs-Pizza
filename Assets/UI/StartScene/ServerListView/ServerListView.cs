using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Localization;

namespace View.StartScene {
    public class ServerListView: ViewBase, IConnectionCallbacks, ILobbyCallbacks, IMatchmakingCallbacks {
        [SerializeField] private ViewBase m_MainView = null;
        [SerializeField] private ViewBase m_CreateServerView = null;
        [SerializeField] private ViewBase m_ServerRoomView = null;
        [SerializeField] private Text m_TitleText = null;
        [SerializeField] private Button m_ExitButton = null;
        [SerializeField] private Button m_RefreshButton = null;
        [SerializeField] private Button m_CreateServerButton = null;
        [SerializeField] private Transform m_TableHeader = null;
        [SerializeField] private Transform m_ServerListContainer = null;
        [SerializeField] private GameObject m_ServerPrefab = null;
        [SerializeField] private GameObject m_LoadingIndicator = null;

        private bool m_KeepConnect = false;

        void OnApplicationQuit() {
            if (PhotonNetwork.IsConnected) {
                PhotonNetwork.RemoveCallbackTarget(this);
                PhotonNetwork.Disconnect();
            }
        }

        protected override void OnInitialize() {
            m_ExitButton.onClick.AddListener(SwitchToMainView);
            m_RefreshButton.onClick.AddListener(RefreshServerList);
            m_CreateServerButton.onClick.AddListener(SwitchToCreateServerView);

            Utils.RemoveAllChildren(m_ServerListContainer);
        }

        protected override void OnShow() {
            PhotonNetwork.AddCallbackTarget(this);

            if (PhotonNetwork.IsConnected) {
                RefreshServerList();
                return;
            }

            ShowLoading(LanguageManager.Data.Multiplayer.Message.Connecting);
            ConnectToPhotonNetwork();
        }

        protected override void OnHide() {
            PhotonNetwork.RemoveCallbackTarget(this);

            if (m_KeepConnect == false && PhotonNetwork.IsConnected) {
                DisconnectFromPhotonNetwork();
            }

            HideLoading();
        }

        public override void OnLanguageChanged(LanguageScheme locale) {
            m_TitleText.text = locale.View.ServerList.Title;
            m_ExitButton.GetComponentInChildren<Text>().text = locale.View.ServerList.Exit;
            m_RefreshButton.GetComponentInChildren<Text>().text = locale.View.ServerList.Refresh;
            m_CreateServerButton.GetComponentInChildren<Text>().text = locale.View.ServerList.CreateServer;

            m_TableHeader.Find("HContainer/ServerName").GetComponent<Text>().text = locale.View.ServerList.ServerName;
            m_TableHeader.Find("HContainer/Players").GetComponent<Text>().text = locale.View.ServerList.Players;
            m_TableHeader.Find("HContainer/Map").GetComponent<Text>().text = locale.View.ServerList.Map;
        }

        void ConnectToPhotonNetwork() {
            PhotonNetwork.ConnectUsingSettings();
        }

        void DisconnectFromPhotonNetwork() {
            PhotonNetwork.Disconnect();
        }

        void SwitchToMainView() {
            m_KeepConnect = false;

            ViewManager.SwitchView(m_MainView);
            AudioManager.PlayCancelSound();
        }

        void RefreshServerList() {
            ShowLoading(LanguageManager.Data.Multiplayer.Message.FetchingServers);

            Utils.RemoveAllChildren(m_ServerListContainer);

            if (PhotonNetwork.InLobby) {
                PhotonNetwork.LeaveLobby();
            }

            PhotonNetwork.JoinLobby();
        }

        void SwitchToCreateServerView() {
            m_KeepConnect = true;

            ViewManager.SwitchView(m_CreateServerView);
            AudioManager.PlayClickSound();
        }

        void SwitchToServerRoomView() {
            m_KeepConnect = true;

            ViewManager.SwitchView(m_ServerRoomView);
            AudioManager.PlayClickSound();
        }

        GameObject CreateServerItem(RoomInfo roomInfo) {
            GameObject serverItem = Instantiate(m_ServerPrefab);
            Button button = serverItem.GetComponent<Button>();

            button.onClick.AddListener(() => {
                ShowLoading(string.Format(
                    LanguageManager.Data.Multiplayer.Message.JoiningServer,
                    roomInfo.Name
                ));

                MultiplayerManager.ServerName = roomInfo.Name;
                PhotonNetwork.JoinRoom(roomInfo.Name);
            });

            button.transform.Find("HContainer/ServerName").GetComponent<Text>().text = roomInfo.Name;
            button.transform.Find("HContainer/Players").GetComponent<Text>().text = string.Format(
                "{0} / {1}",
                roomInfo.PlayerCount,
                roomInfo.MaxPlayers
            );

            button.transform.Find("HContainer/Map").GetComponent<Text>().text = "Game";

            return serverItem;
        }

        void ShowLoading(string message, string sub = "") {
            m_LoadingIndicator.transform.Find("Message").GetComponent<Text>().text = message;

            Text subText = m_LoadingIndicator.transform.Find("Sub").GetComponent<Text>();

            if (string.IsNullOrEmpty(sub)) {
                subText.text = "";
            }
            else {
                subText.text = sub;
            }

            m_LoadingIndicator.SetActive(true);
        }
        
        void HideLoading() {
            m_LoadingIndicator.SetActive(false);
        }

        public void OnConnected() {}

        public void OnConnectedToMaster() {
            m_LoadingIndicator.SetActive(false);
            RefreshServerList();
        }

        public void OnDisconnected(DisconnectCause cause) {
            m_KeepConnect = false;

            Dialog.Alert(new AlertOptions() {
                Message = string.Format(
                    "{0} ({1})", 
                    LanguageManager.Data.Multiplayer.Message.Disconnected,
                    cause.ToString()
                ),
                OnClose = SwitchToMainView,
            });

            AudioManager.PlayErrorSound();
        }

        public void OnRegionListReceived(RegionHandler regionHandler) {}

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data) {}

        public void OnCustomAuthenticationFailed(string debugMessage) {}

        public void OnJoinedLobby() {}

        public void OnLeftLobby() {}

        public void OnRoomListUpdate(List<RoomInfo> roomList) {
            Utils.RemoveAllChildren(m_ServerListContainer);

            for (int i = 0; i < roomList.Count; i += 1) {
                GameObject serverItem = CreateServerItem(roomList[i]);
                serverItem.transform.SetParent(m_ServerListContainer, false);
            }

            HideLoading();
        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics) {}

        public void OnFriendListUpdate(List<FriendInfo> friendList) {}

        public void OnCreatedRoom() {}

        public void OnCreateRoomFailed(short returnCode, string message) {}

        public void OnJoinedRoom() {
            SwitchToServerRoomView();
        }

        public void OnJoinRoomFailed(short returnCode, string message) {
            AudioManager.PlayErrorSound();

            Dialog.Alert(new AlertOptions() {
                Message = LanguageManager.Data.Multiplayer.Message.InvalidServer,
                OnClose = () => {
                    AudioManager.PlayCancelSound();
                    HideLoading();
                }
            });
        }

        public void OnJoinRandomFailed(short returnCode, string message) {}

        public void OnLeftRoom() {}
    }
}
