using UnityEngine;
using UnityEngine.UI;
using Localization;
using Photon.Pun;
using Photon.Realtime;

namespace View.StartScene {
    public class PlayerSetupView: ViewBase {
        [SerializeField] private ViewBase m_MainView = null;
        [SerializeField] private ViewBase m_ServerListView = null;
        [SerializeField] private Text m_TitleText = null;
        [SerializeField] private InputField m_PlayerNameInputField = null;
        [SerializeField] private Button m_PlayButton = null;
        [SerializeField] private Button m_ExitButton = null;
        [SerializeField] private Text m_ErrorText = null;
        private const string m_PlayerNamePrefsKey = "PlayerName";

        protected override void OnInitialize() {
            m_PlayButton.onClick.AddListener(UpdatePlayerNameAndGoToServerListView);
            m_ExitButton.onClick.AddListener(SwitchToMainView);
        }

        protected override void OnShow() {
            HideError();
            m_PlayerNameInputField.text = PlayerPrefs.GetString(m_PlayerNamePrefsKey, "");
        }

        public override void OnLanguageChanged(LanguageScheme locale) {
            m_TitleText.text = locale.View.PlayerSetup.Title;
            m_PlayerNameInputField.placeholder.GetComponent<Text>().text = locale.View.PlayerSetup.PlayerNameInputPlaceholder;
            m_PlayButton.GetComponentInChildren<Text>().text = locale.View.PlayerSetup.Play;
            m_ExitButton.GetComponentInChildren<Text>().text = locale.View.PlayerSetup.Exit;
        }

        void UpdatePlayerNameAndGoToServerListView() {
            string playerName = m_PlayerNameInputField.text;

            if (string.IsNullOrEmpty(playerName)) {
                ShowError(LanguageManager.Data.View.PlayerSetup.RequiredPlayerName);
                AudioManager.PlayErrorSound();
                return;
            }

            PlayerPrefs.SetString(m_PlayerNamePrefsKey, playerName);
            PlayerPrefs.Save();

            PhotonNetwork.LocalPlayer.NickName = playerName;
            MultiplayerManager.PlayerName = playerName;

            ViewManager.SwitchView(m_ServerListView);
            AudioManager.PlayClickSound();
        }

        void SwitchToMainView() {
            ViewManager.SwitchView(m_MainView);
            AudioManager.PlayCancelSound();
        }

        void ShowError(string message) {
            m_ErrorText.text = message;
            m_ErrorText.gameObject.SetActive(true);
        }

        void HideError() {
            m_ErrorText.gameObject.SetActive(false);
        }
    }
}
