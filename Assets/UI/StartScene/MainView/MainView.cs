using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Localization;

namespace View.StartScene {
    public class MainView : ViewBase {
        [SerializeField] private ViewBase m_PlayerSetupView = null;
        [SerializeField] private Text m_TitleText = null;
        [SerializeField] private Button m_SingleplayerButton = null;
        [SerializeField] private Button m_MultiplayerButton = null;
        [SerializeField] private Button m_ExitGameButton = null;
        [SerializeField] private Button m_SetEnglishButton = null;
        [SerializeField] private Button m_SetKoreanButton = null;

        protected override void OnInitialize() {
            m_SingleplayerButton.onClick.AddListener(StartSingleplay);
            m_MultiplayerButton.onClick.AddListener(SwitchToPlayerSetupView);
            m_ExitGameButton.onClick.AddListener(ExitGame);

            m_SetEnglishButton.onClick.AddListener(SetEnglish);
            m_SetKoreanButton.onClick.AddListener(SetKorean);

            SetLanguage(LanguageManager.Language);
        }

        protected override void OnShow() {
            if (PhotonNetwork.IsConnected) {
                PhotonNetwork.Disconnect();
            }

            MultiplayerManager.ServerName = "";
        }

        public override void OnLanguageChanged(LanguageScheme locale) {
            m_TitleText.text = locale.View.Main.Title;
            m_SingleplayerButton.GetComponentInChildren<Text>().text = locale.View.Main.Singleplayer;
            m_MultiplayerButton.GetComponentInChildren<Text>().text = locale.View.Main.Multiplayer;
            m_ExitGameButton.GetComponentInChildren<Text>().text = locale.View.Main.ExitGame;
        }

        void StartSingleplay() {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom(null);
            
            SceneManager.LoadScene("Game");
        }

        void SwitchToPlayerSetupView() {
            PhotonNetwork.OfflineMode = false;
            ViewManager.SwitchView(m_PlayerSetupView);
            AudioManager.PlayClickSound();
        }

        void ExitGame() {
            Application.Quit();
        }

        void SetEnglish() {
            SetLanguage(Language.English);
            AudioManager.PlayClickSound();
        }

        void SetKorean() {
            SetLanguage(Language.Korean);
            AudioManager.PlayClickSound();
        }

        void SetLanguage(Language language) {
            if (language.Equals(Language.English)) {
                m_SetEnglishButton.gameObject.SetActive(false);
                m_SetKoreanButton.gameObject.SetActive(true);
            }
            else if (language.Equals(Language.Korean)) {
                m_SetEnglishButton.gameObject.SetActive(true);
                m_SetKoreanButton.gameObject.SetActive(false);
            }
            else {
                throw new System.Exception("Invalid Lanauge: " + language.ToString());
            }

            LanguageManager.Language = language;
        }
    }
}
