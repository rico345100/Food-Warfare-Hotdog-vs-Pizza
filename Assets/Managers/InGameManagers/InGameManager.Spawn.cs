using System.Collections.Generic;
using UnityEngine;
using FoodWarfare;
using Photon.Pun;

public partial class InGameManager {
    [SerializeField] private List<IDamagable> m_Allies = new List<IDamagable>();
    [SerializeField] private List<IDamagable> m_Enemies = new List<IDamagable>();

    public List<IDamagable> Allies => m_Allies;
    public List<IDamagable> Enemies => m_Enemies;

    void SpawnHQ() {
        GameObject hqObject = PhotonNetwork.InstantiateRoomObject("HQ", m_HQSpawn.position, m_HQSpawn.rotation);
        HQ hq = hqObject.GetComponent<HQ>();

        PhotonView.RPC("InitializeHQHealthSlider", RpcTarget.All, hq.Health);

        hq.OnDamage.AddListener(HandleHQDamage);
        hq.OnDead.AddListener(HandleHQDestroy);
    }

    [PunRPC]
    void InitializeHQHealthSlider(int health) {
        InGameUIManager.Instance.HQHealthSlider.maxValue = health;
        InGameUIManager.Instance.HQHealthSlider.value = health;
    }

    void HandleHQDamage(IDamagable damagable) {
        PhotonView.RPC("UpdateHQHealthSlider", RpcTarget.All, damagable.Health);
    }

    void HandleHQDestroy(IDamagable damagable) {
        damagable.OnDamage.RemoveListener(HandleHQDamage);
        damagable.OnDead.RemoveListener(HandleHQDestroy);
        this.PhotonView.RPC("GameOver", RpcTarget.All);
    }

    [PunRPC]
    void UpdateHQHealthSlider(int health) {
        InGameUIManager.Instance.HQHealthSlider.value = health;
    }

    void SpawnPlayer(Transform spawnPoint) {
        GameObject playerObject = PhotonNetwork.Instantiate("Player", spawnPoint.position, spawnPoint.rotation);
        Player player = playerObject.GetComponent<Player>();
        player.SpawnPoint = spawnPoint;
        player.Reinitialize();
        player.OnDead.AddListener(HandlePlayerDead);

        m_DefaultCamera.gameObject.SetActive(false);
    }

    private const float m_PlayerRespawnTime = 3;

    void HandlePlayerDead(IDamagable damagable) {
        damagable.OnDead.RemoveListener(HandlePlayerDead);

        Transform cameraPos = Player.Instance.Camera.transform;
        m_DefaultCamera.transform.SetPositionAndRotation(cameraPos.position, cameraPos.rotation);
        m_DefaultCamera.gameObject.SetActive(true);
        
        int upgradeLevel = InGameManager.Instance.GetUpgradeLevel(Upgrade.RespawnSpeed);
        float respawnTime = m_PlayerRespawnTime - (upgradeLevel * 0.15f);

        Invoke("RespawnPlayer", respawnTime);
    }

    void RespawnPlayer() {
        if (IsGameOver) {
            return;
        }

        SpawnPlayer(m_PlayerSpawns[MultiplayerManager.PlayerID]);
    }

    void SpawnEnemy(Transform spawnPoint) {
        GameObject enemyObject = PhotonNetwork.InstantiateRoomObject("Enemy", spawnPoint.position, spawnPoint.rotation);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        enemy.OnDead.AddListener(HandleEnemyDead);
    }

    void HandleEnemyDead(IDamagable damagable) {
        damagable.OnDead.RemoveListener(HandleEnemyDead);
        m_Score += 1;
        
        PhotonView.RPC("UpdateScore", RpcTarget.All, m_Score);
    }

    [PunRPC]
    void UpdateScore(int newScore) {
        m_Score = newScore;
        InGameUIManager.Instance.SetScoreText(newScore);
    }
}
