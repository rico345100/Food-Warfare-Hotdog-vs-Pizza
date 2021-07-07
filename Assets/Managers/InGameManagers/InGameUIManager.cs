using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Realtime;
using Localization;

public class IntEvent: UnityEvent<int> {}

public class InGameUIManager : LocalSingleton<InGameUIManager> {
    [SerializeField] private GameObject m_HealthContainer = null;
    [SerializeField] private Animator m_HitAnimator = null;
    [SerializeField] private Text m_ControlsText = null;
    [SerializeField] private Text m_HealthText = null;
    [SerializeField] private GameObject m_MissionContainer = null;
    [SerializeField] private Slider m_HQHealthSlider = null;
    [SerializeField] private Text m_WaveText = null;
    [SerializeField] private Text m_InfoText = null;
    [SerializeField] private GameObject m_UpgradeContainer = null;
    [SerializeField] private Button[] m_UpgradeButtons = null;
    [SerializeField] private GameObject m_GameOver = null;
    [SerializeField] private Text m_GameOverText = null;
    [SerializeField] private GameObject m_Restart = null;
    [SerializeField] private Text m_ScoreText = null;
    [SerializeField] private Text m_HighscoreText = null;
    [SerializeField] private GameObject m_SendMessageContainer = null;
    [SerializeField] private InputField m_SendMessageInputField = null;
    [SerializeField] private ScrollRect m_ChatScrollRect = null;
    [SerializeField] private Transform m_ChatContainer = null;
    [SerializeField] private GameObject m_ChatMessagePrefab = null;
    [SerializeField] private Transform m_PlayerIndicatorsContainer = null;
    [SerializeField] private GameObject m_PlayerIndicatorPrefab = null;

    private Dictionary<int, GameObject> m_PlayerIndicators = new Dictionary<int, GameObject>();

    private IEnumerator m_CoShowInfoText = null;
    private IntEvent m_OnUpgradeSelect = new IntEvent();

    private WaitForSeconds m_WaitBeforeScrollUp = new WaitForSeconds(0.1f);

    public Slider HQHealthSlider => m_HQHealthSlider;
    public Button[] UpgradeButtons => m_UpgradeButtons;
    public bool UpgradePanelVisible => m_UpgradeContainer.activeSelf;
    public bool SendMessageContainerVisible => m_SendMessageContainer.activeSelf;
    public InputField SendMessageInputField => m_SendMessageInputField;

    public IntEvent OnUpgradeSelect => m_OnUpgradeSelect;

    void Start() {
        Utils.RemoveAllChildren(m_PlayerIndicatorsContainer);
        Utils.RemoveAllChildren(m_ChatContainer);

        HideGameOver();
        HideRestart();
        HideSendMessage();

        for (int i = 0; i < m_UpgradeButtons.Length; i += 1) {
            int index = i;
            m_UpgradeButtons[i].onClick.AddListener(() => OnUpgradeSelect.Invoke(index));
        }

        LanguageScheme locale = LanguageManager.Data;

        string controlText = locale.Game.Controls;

        if (MultiplayerManager.IsMultiplayer) {
            controlText = string.Format("{0}\n{1}", controlText, locale.Game.MultiplayerControls);
        }

        m_ControlsText.text = controlText;
        m_MissionContainer.GetComponentInChildren<Text>().text = locale.Game.ProtectPizza;
        m_Restart.GetComponent<Text>().text = locale.Game.Replay;
        m_UpgradeContainer.GetComponentInChildren<Text>().text = locale.Game.ChooseAnUpgrade;

        m_SendMessageInputField.placeholder.GetComponent<Text>().text = locale.Game.SendMessagePlaceholder;
    }

    public void Reset() {
        m_HitAnimator.CrossFadeInFixedTime("Reset", 0.01f);
    }

    public void ShowPlayerUI() {
        m_HealthContainer.SetActive(true);
    }

    public void HidePlayerUI() {
        m_HealthContainer.SetActive(false);
    }

    public void ShowHit() {
        m_HitAnimator.CrossFadeInFixedTime("Hit", 0.01f);
    }

    public void SetHealthText(float value) {
        m_HealthText.text = string.Format("{0}: {1}", LanguageManager.Data.Game.HP, Mathf.Floor(value));
    }

    public void SetWaveText(int wave) {
        m_WaveText.text = string.Format("{0} {1}", LanguageManager.Data.Game.Wave, wave);
    }

    public void ShowInfoText(string text, float time = 5) {
        if (m_CoShowInfoText != null) {
            StopCoroutine(m_CoShowInfoText);
        }

        m_CoShowInfoText = CoShowInfoText(text, time);
        StartCoroutine(m_CoShowInfoText);
    }

    IEnumerator CoShowInfoText(string text, float time) {
        m_InfoText.text = text;
        m_InfoText.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);

        m_InfoText.gameObject.SetActive(false);
    }

    public void HideInfoText() {
        m_InfoText.gameObject.SetActive(false);
    }

    public void ShowUpgrade() {
        m_UpgradeContainer.SetActive(true);
    }

    public void HideUpgrade() {
        m_UpgradeContainer.SetActive(false);
    }

    public void ShowGameOver() {
        m_GameOver.SetActive(true);
    }

    public void HideGameOver() {
        m_GameOver.SetActive(false);
    }

    public void ShowRestart() {
        m_Restart.SetActive(true);
    }

    public void HideRestart() {
        m_Restart.SetActive(false);
    }

    public void ShowMissionContainer() {
        m_MissionContainer.SetActive(true);
    }

    public void HideMissionContainer() {
        m_MissionContainer.SetActive(false);
    }

    public void SetScoreText(int score) {
        m_ScoreText.text = string.Format("{0}: {1}", LanguageManager.Data.Game.Score, score.ToString("N0"));
    }

    public void SetHighscoreText(int score) {
        m_HighscoreText.text = string.Format("{0}: {1}", LanguageManager.Data.Game.Highscore, score.ToString("N0"));
    }

    public void HideHighscore() {
        m_HighscoreText.gameObject.SetActive(false);
    }

    public void SetGameOverText(int score) {
        LanguageSchemeGame locale = LanguageManager.Data.Game;

        m_GameOverText.text = string.Format(
            "{0}!\n{1}: {2}",
            locale.RestaurantDestroyed,
            locale.YourScore,
            score.ToString("N0")
        );
    }

    public void ShowSendMessage() {
        m_SendMessageInputField.text = "";
        m_SendMessageContainer.SetActive(true);

        m_SendMessageInputField.ActivateInputField();
    }

    public void HideSendMessage() {
        m_SendMessageContainer.SetActive(false);
    }

    public void CreateMessage(string sender, string message) {
        GameObject messageObject = Instantiate(m_ChatMessagePrefab);

        messageObject.GetComponent<Text>().text = string.Format(
            "<b><color=#FFF100>{0}</color></b>: {1}",
            sender,
            message
        );

        messageObject.transform.SetParent(m_ChatContainer, false);

        StartCoroutine(CoScrollChatContainerToBottom());
    }

    public void CreateSystemMessage(string message) {
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

    public GameObject CreatePlayerIndicator(Player player, Transform targetTransform) {
        GameObject playerIndicatorObject = Instantiate(m_PlayerIndicatorPrefab);
        playerIndicatorObject.GetComponent<DisplayUIElementOverCanvas>().Target = targetTransform;
        playerIndicatorObject.transform.SetParent(m_PlayerIndicatorsContainer, false);

        playerIndicatorObject.GetComponent<Text>().text = player.NickName;

        m_PlayerIndicators.Add(player.ActorNumber, playerIndicatorObject);

        return playerIndicatorObject;
    }

    public void DeletePlayerIndicator(int actorNumber) {
        if (m_PlayerIndicators.ContainsKey(actorNumber)) {
            Destroy(m_PlayerIndicators[actorNumber]);
            m_PlayerIndicators.Remove(actorNumber);
        }
    }
}
