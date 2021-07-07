using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Localization;
using ExitGames.Client.Photon;

public partial class InGameManager : LocalSingleton<InGameManager>, IInRoomCallbacks, IOnEventCallback {
    [SerializeField] private Camera m_DefaultCamera = null;
    [SerializeField] private Transform[] m_PlayerSpawns = null;
    [SerializeField] private Transform[] m_EnemySpawns = null;
    [SerializeField] private Transform m_HQSpawn = null;
    private PhotonView m_PhotonView;

    private Vector3 m_InitialCameraPosition;
    private Quaternion m_InitialCameraRotation;
    private int m_Wave = 0;
    private int m_Score = 0;
    private bool m_GameOver = false;
    private bool m_CanRestart = false;

    public PhotonView PhotonView {
        get {
            if (m_PhotonView == null) {
                m_PhotonView = GetComponent<PhotonView>();
            }

            return m_PhotonView;
        }
    }
    
    public bool IsGameOver => m_GameOver;

    public int Highscore {
        get => PlayerPrefs.GetInt("Highscore", 0);
        set {
            PlayerPrefs.SetInt("Highscore", value);
            PlayerPrefs.Save();
        }
    }

    public Camera MainCamera {
        get {
            if (FoodWarfare.Player.Instance == null) {
                return m_DefaultCamera;
            }
            else {
                return FoodWarfare.Player.Instance.Camera;
            }
        }
    }

    void Start() {
        m_InitialCameraPosition = m_DefaultCamera.transform.position;
        m_InitialCameraRotation = m_DefaultCamera.transform.rotation;

        if (MultiplayerManager.IsMultiplayer) {
            PhotonNetwork.AddCallbackTarget(this);
            MultiplayerManager.OnMessageReceived.AddListener(HandleMessageReceive);
            InGameUIManager.Instance.HideHighscore();
        }
        
        StartGame();
    }

    void OnDestroy() {
        if (MultiplayerManager.IsMultiplayer) {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }

    void OnApplicationQuit() {
        if (PhotonNetwork.IsConnected) {
            PhotonNetwork.Disconnect();
        }
    }

    void StartGame() {
        AudioManager.PlayMusic(AudioManager.Instance.BattleMusic);

        InGameUIManager.Instance.ShowMissionContainer();
        InGameUIManager.Instance.HideRestart();
        InGameUIManager.Instance.HideGameOver();

        m_Wave = 0;
        m_Score = 0;

        m_GameOver = false;
        m_CanRestart = false;

        InGameUIManager.Instance.SetWaveText(m_Wave);
        InGameUIManager.Instance.SetScoreText(m_Score);
        InGameUIManager.Instance.SetHighscoreText(Highscore);
        
        ResetUpgrades();

        Allies.Clear();

        if (PhotonNetwork.IsMasterClient) {
            while (Enemies.Count > 0) {
                IDamagable enemy = Enemies[0];
                enemy.OnDead.RemoveListener(HandleEnemyDead);

                enemy.TakeDamage(int.MaxValue);
                Enemies.Remove(enemy);
            }
        }

        Enemies.Clear();

        InGameUIManager.Instance.HidePlayerUI();

        if (MultiplayerManager.IsMultiplayer == false) {
            SpawnHQ();
            SpawnPlayer(m_PlayerSpawns[0]);
            StartCoroutine(CoProgressWaves());
        }
        else {
            if (PhotonNetwork.IsMasterClient) {
                SpawnHQ();
            }

            SpawnPlayer(m_PlayerSpawns[MultiplayerManager.PlayerID]);

            if (PhotonNetwork.IsMasterClient) {
                StartCoroutine(CoProgressWaves());
            }
        }
    }

    void Update() {
        if (MultiplayerManager.IsMultiplayer) {
            HandleSendMessageInput();
        }

        if (m_CanRestart == false) {
            return;
        }

        if (Input.GetKey(KeyCode.Space)) {
            StartGame();
        }
        else if (Input.GetKey(KeyCode.Escape)) {
            BackToStartScene();
        }
    }

    [PunRPC]
    void GameOver() {
        if (FoodWarfare.Player.Instance.Dead == false) {
            FoodWarfare.Player.Instance.TakeDamage(int.MaxValue);
        }

        AudioManager.PlayMusicOnce(AudioManager.Instance.GameOverMusic);

        m_GameOver = true;

        if (MultiplayerManager.IsMultiplayer == false && m_Score > Highscore) {
            Highscore = m_Score;
            InGameUIManager.Instance.SetHighscoreText(m_Score);
        }

        InGameUIManager.Instance.SetGameOverText(m_Score);

        InGameUIManager.Instance.HideMissionContainer();
        InGameUIManager.Instance.HideUpgrade();
        InGameUIManager.Instance.ShowGameOver();

        m_DefaultCamera.transform.SetPositionAndRotation(m_InitialCameraPosition, m_InitialCameraRotation);
        m_DefaultCamera.gameObject.SetActive(true);

        if (MultiplayerManager.IsMultiplayer == false) {
            Invoke("SetRestartAvailable", 3);
        }
        else {
            MultiplayerManager.GameOver = true;
            Invoke("BackToStartScene", 5);
        }
    }

    void SetRestartAvailable() {
        InGameUIManager.Instance.ShowRestart();
        m_CanRestart = true;
    }

    IEnumerator CoProgressWaves() {
        LanguageSchemeGame locale = LanguageManager.Data.Game;
        yield return new WaitForSeconds(1f);

        SendSyncUI(SyncUIType.ShowInfoText, InfoTextType.ProtectPizzaFromEnemies);

        yield return new WaitForSeconds(4f);

        WaitForSeconds m_WaveBreakTimeForMessage = new WaitForSeconds(6);

        while (m_GameOver == false) {
            m_Wave += 1;

            SendSyncUI(SyncUIType.SetWaveText, m_Wave);
            SendSyncUI(SyncUIType.ShowInfoText, InfoTextType.WaveStart, m_Wave);

            int enemyCount = 3 + m_Wave * 1;
            float respawnTime = (2 - m_Wave * 0.1f);

            if (respawnTime < 0.25f) {
                respawnTime = 0.25f;
            }

            WaitForSeconds respawnWait = new WaitForSeconds(respawnTime);

            for (int i = 0; i < enemyCount; i += 1) {
                if (m_GameOver) {
                    break;
                }

                Transform spawnPos = m_EnemySpawns[Random.Range(0, m_EnemySpawns.Length)];
                SpawnEnemy(spawnPos);
                yield return respawnWait;
            }

            if (m_GameOver) {
                yield break;
            }

            SendSyncUI(SyncUIType.ShowInfoText, InfoTextType.NextWaveIncoming);
            SendSyncUI(SyncUIType.ShowUpgrade);

            yield return m_WaveBreakTimeForMessage;
        }
    }

    void BackToStartScene() {
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.LoadScene("Start");
    }

    public void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer) {}

    public void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) {
        InGameUIManager.Instance.DeletePlayerIndicator(otherPlayer.ActorNumber);
        // Don't need to exclude this player's object from m_Allies, because it's done automatically when the object returned to the pool
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) {}

    public void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) {}

    public void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient) {
        if (IsGameOver == false) {
            MultiplayerManager.ForceDisconnected = true;
            MultiplayerManager.ForceDisconnectedReason = ForceDisconnectedReason.HostLeft;
        }

        PhotonNetwork.LeaveRoom();
        BackToStartScene();
    }

    public void OnEvent(EventData photonEvent) {
        if (photonEvent.Code == MultiplayerEventCode.SyncUI) {
            object[] data = (object[]) photonEvent.CustomData;
            HandleUISyncEvent(data);
        }
    }
}
