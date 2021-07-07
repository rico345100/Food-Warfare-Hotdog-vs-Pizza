using UnityEngine;
using UnityEngine.UI;
using Localization;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;

namespace View.StartScene {
    public partial class ServerRoomView: ViewBase, IConnectionCallbacks, IInRoomCallbacks, IMatchmakingCallbacks {
        [SerializeField] private ViewBase m_MainView = null;
        [SerializeField] private ViewBase m_ServerListView = null;
        [SerializeField] private Text m_TitleText = null;
        [SerializeField] private Button m_LeaveButton = null;
        [SerializeField] private Button m_PlayButton = null;

        [Header("Server Info")]
        [SerializeField] private Text m_ServerNameLabel = null;
        [SerializeField] private Text m_ServerNameText = null;
        [SerializeField] private Text m_MapLabel = null;
        [SerializeField] private Text m_MapText = null;
        [SerializeField] private Text m_MaxPlayersLabel = null;
        [SerializeField] private Text m_MaxPlayersText = null;
        
        [Header("Player List")]
        [SerializeField] private Transform m_PlayersContainer = null;
        [SerializeField] private GameObject m_PlayerInfoPrefab = null;
        [SerializeField] private GameObject m_PlayerInfoButtonPrefab = null;

        private const int m_MinimumPlayerCountForStart = 2;

        protected override void OnInitialize() {
            m_LeaveButton.onClick.AddListener(SwitchToServerListView);
            m_PlayButton.onClick.AddListener(StartMultiplayer);
            m_ChatSendButton.onClick.AddListener(SubmitChatMessage);
        }

        protected override void OnShow() {
            m_PlayButton.interactable = false;
            
            if (PhotonNetwork.IsConnected == false || PhotonNetwork.InRoom == false) {
                Dialog.Alert(new AlertOptions() {
                    Message = LanguageManager.Data.Multiplayer.Message.InvalidServer,
                });
                
                AudioManager.PlayErrorSound();
                SwitchToServerListView();
                return;
            }

            if (PhotonNetwork.IsMasterClient) {
                m_PlayButton.GetComponentInChildren<Text>().text = string.Format(
                    LanguageManager.Data.View.ServerRoom.NeedMorePlayers,
                    m_MinimumPlayerCountForStart
                );
            }
            else {
                m_PlayButton.GetComponentInChildren<Text>().text = LanguageManager.Data.View.ServerRoom.OnlyMasterCanStartGame;
            }

            PhotonNetwork.AddCallbackTarget(this);

            Utils.RemoveAllChildren(m_ChatContainer);
            UpdateServerInfo();

            EnableChat();

            MultiplayerManager.OnMessageReceived.AddListener(HandleMessageReceive);

            CreateSystemMessage(string.Format("{0} entered.", MultiplayerManager.PlayerName));
            CheckPlayAvailable();
        }

        protected override void OnHide() {
            if (PhotonNetwork.InRoom) {
                PhotonNetwork.LeaveRoom();
            }

            MultiplayerManager.OnMessageReceived.RemoveListener(HandleMessageReceive);
            
            PhotonNetwork.RemoveCallbackTarget(this);
            DisableChat();
        }

        public override void OnLanguageChanged(LanguageScheme locale) {
            m_TitleText.text = locale.View.ServerRoom.Title;
            m_LeaveButton.GetComponentInChildren<Text>().text = locale.View.ServerRoom.Leave;

            m_ServerNameLabel.text = locale.View.ServerRoom.ServerName;
            m_MapLabel.text = locale.View.ServerRoom.Map;
            m_MaxPlayersLabel.text = locale.View.ServerRoom.MaxPlayers;

            m_ChatTitleText.text = locale.View.ServerRoom.Chat;
            m_ChatInputField.placeholder.GetComponent<Text>().text = locale.View.ServerRoom.ChatPlaceholder;
            m_ChatSendButton.GetComponentInChildren<Text>().text = locale.View.ServerRoom.Send;
        }

        void OnDestroy() {
            PhotonNetwork.RemoveCallbackTarget(this);
            MultiplayerManager.OnMessageReceived.RemoveListener(HandleMessageReceive);
        }

        void UpdateServerInfo() {
            LanguageScheme locale = LanguageManager.Data;
            RoomInfo roomInfo = PhotonNetwork.CurrentRoom;

            m_ServerNameText.text = roomInfo.Name;
            m_MapText.text = "Game";
            m_MaxPlayersText.text = string.Format("{0} Player(s)", roomInfo.MaxPlayers);

            Utils.RemoveAllChildren(m_PlayersContainer);

            Dictionary<int, Player> players = PhotonNetwork.CurrentRoom.Players;

            foreach (KeyValuePair<int, Player> kv in players) {
                Player player = kv.Value;
                GameObject targetPrefab = m_PlayerInfoPrefab;
                bool canKickPlayer = false;

                if (PhotonNetwork.LocalPlayer.IsMasterClient && player != PhotonNetwork.MasterClient) {
                    targetPrefab = m_PlayerInfoButtonPrefab;
                    canKickPlayer = true;
                }
                
                GameObject playerObject = Instantiate(targetPrefab);
                playerObject.GetComponentInChildren<Text>().text = player.NickName;

                if (canKickPlayer) {
                    Button kickButton = playerObject.GetComponent<Button>();
                    kickButton.onClick.AddListener(() => {
                        AudioManager.PlayClickSound();

                        Dialog.Confirm(new ConfirmOptions() {
                            Message = string.Format(
                                locale.View.ServerRoom.AskKickPlayer,
                                player.NickName
                            ),
                            OnClose = (confirmed) => {
                                if (confirmed) {
                                    bool kicked = PhotonNetwork.CloseConnection(player);

                                    if (kicked) {
                                        Dialog.Alert(new AlertOptions() {
                                            Message = locale.View.ServerRoom.PlayerKicked,
                                        });
                                    }
                                    else {
                                        Dialog.Alert(new AlertOptions() {
                                            Message = locale.View.ServerRoom.PlayerKickFailure
                                        });
                                    }
                                }
                            },
                        });
                    });
                }

                playerObject.transform.SetParent(m_PlayersContainer, false);
            }
        }

        void SwitchToServerListView() {
            ViewManager.SwitchView(m_ServerListView);
            AudioManager.PlayCancelSound();
        }

        void SwitchToMainView() {
            ViewManager.SwitchView(m_MainView);
            AudioManager.PlayCancelSound();
        }

        void StartMultiplayer() {
            if (PhotonNetwork.CurrentRoom.PlayerCount < m_MinimumPlayerCountForStart) {
                AudioManager.PlayErrorSound();
                
                Dialog.Alert(new AlertOptions() {
                    Message = string.Format(
                        LanguageManager.Data.View.ServerRoom.NeedMorePlayers,
                        m_MinimumPlayerCountForStart
                    ),
                    OnClose = AudioManager.PlayCancelSound,
                });

                return;
            }

            AssignPlayerIds();
            MultiplayerManager.LoadScene("Game");
        }

        void AssignPlayerIds() {
            Player[] players = PhotonNetwork.PlayerList;

            for (int i = 0; i < players.Length; i += 1) {
                MultiplayerManager.SetPlayerID(players[i], i);
            }
        }

        void CheckPlayAvailable() {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= m_MinimumPlayerCountForStart) {
                m_PlayButton.interactable = true;
                m_PlayButton.GetComponentInChildren<Text>().text = LanguageManager.Data.View.ServerRoom.Play;
            }
        }

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

        public void OnPlayerEnteredRoom(Player newPlayer) {
            UpdateServerInfo();
            CheckPlayAvailable();
            CreateSystemMessage(string.Format("{0} entered.", newPlayer.NickName));
        }

        public void OnPlayerLeftRoom(Player otherPlayer) {
            UpdateServerInfo();
            CheckPlayAvailable();
            CreateSystemMessage(string.Format("{0} left.", otherPlayer.NickName));
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {}

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {}

        public void OnMasterClientSwitched(Player newMasterClient) {
            UpdateServerInfo();
        }

        public void OnFriendListUpdate(List<FriendInfo> friendList) {}

        public void OnCreatedRoom() {}

        public void OnCreateRoomFailed(short returnCode, string message) {}

        public void OnJoinedRoom() {}

        public void OnJoinRoomFailed(short returnCode, string message) {}

        public void OnJoinRandomFailed(short returnCode, string message) {}

        public void OnLeftRoom() {
            Dialog.Alert(new AlertOptions() {
                Message = LanguageManager.Data.View.ServerRoom.ServerLostConnection,
                OnClose = AudioManager.PlayClickSound,
            });

            AudioManager.PlayErrorSound();
            SwitchToServerListView();
        }

        public void OnConnected() {}
    }
}
