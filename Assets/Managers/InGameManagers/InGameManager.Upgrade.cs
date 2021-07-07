using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Localization;
using FoodWarfare;

public partial class InGameManager {
    private Dictionary<Upgrade, int> m_Upgrades = new Dictionary<Upgrade, int>() {
        { Upgrade.MaxHP, 0 },
        { Upgrade.ProjectileSpeed, 0 },
        { Upgrade.AttackSpeed, 0 },
        { Upgrade.ProjectileSpread, 0 },
        { Upgrade.MoveSpeed, 0 },
        { Upgrade.RotatingSpeed, 0 },
        { Upgrade.RespawnSpeed, 0 },
    };

    private Dictionary<Upgrade, int> m_MaxUpgrades = new Dictionary<Upgrade, int>() {
        { Upgrade.MaxHP, 10 },
        { Upgrade.ProjectileSpeed, 10 },
        { Upgrade.AttackSpeed, 10 },
        { Upgrade.ProjectileSpread, 10 },
        { Upgrade.MoveSpeed, 10 },
        { Upgrade.RotatingSpeed, 10 },
        { Upgrade.RespawnSpeed, 10 },
    };

    private static Upgrade[] AvailableUpgrades = new Upgrade[] {
        Upgrade.MaxHP,
        Upgrade.ProjectileSpeed,
        Upgrade.AttackSpeed,
        Upgrade.ProjectileSpread,
        Upgrade.MoveSpeed,
        Upgrade.RotatingSpeed,
        Upgrade.RespawnSpeed,
    };

    private List<Upgrade> m_CurrentAvailableUpgrade = new List<Upgrade>();

    void ResetUpgrades() {
        for (int i = 0; i < AvailableUpgrades.Length; i += 1) {
            m_Upgrades[AvailableUpgrades[i]] = 0;
        }
    }

    void SetRandomUpgrades(int upgradeCount) {
        InGameUIManager.Instance.OnUpgradeSelect.RemoveListener(HandleUpgradeSelect);

        List<Upgrade> upgrades = GetRandomUpgrades(upgradeCount);
        Button[] buttons = InGameUIManager.Instance.UpgradeButtons;

        for (int i = 0; i < buttons.Length; i += 1) {
            buttons[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < upgradeCount; i += 1) {
            buttons[i].GetComponentInChildren<Text>().text = GetUpgradeName(upgrades[i]);
            buttons[i].gameObject.SetActive(true);
        }

        m_CurrentAvailableUpgrade = upgrades;

        InGameUIManager.Instance.OnUpgradeSelect.AddListener(HandleUpgradeSelect);
    }

    List<Upgrade> GetRandomUpgrades(int upgradeCount) {
        List<Upgrade> randomUpgrades = new List<Upgrade>();
        int added = 0;
        int availableUpgradeCount = GetAvailableUpgradeCount();

        if (upgradeCount > availableUpgradeCount) {
            upgradeCount = availableUpgradeCount;
        }

        while (added < upgradeCount) {
            Upgrade upgrade = GetRandomUpgrade();
            bool duplicated = false;

            for (int i = 0; i < randomUpgrades.Count; i += 1) {
                if (upgrade.Equals(randomUpgrades[i])) {
                    duplicated = true;
                    break;
                }
            }

            if (duplicated == false && m_Upgrades[upgrade] < m_MaxUpgrades[upgrade]) {
                randomUpgrades.Add(upgrade);
                added += 1;
            }
        }

        return randomUpgrades;
    }

    Upgrade GetRandomUpgrade() {
        return AvailableUpgrades[Random.Range(0, AvailableUpgrades.Length)];
    }

    void HandleUpgradeSelect(int index) {
        Upgrade upgrade = m_CurrentAvailableUpgrade[index];
        m_Upgrades[upgrade] += 1;

        InGameUIManager.Instance.HideUpgrade();

        if (upgrade.Equals(Upgrade.MaxHP)) {
            if (Player.Instance != null && Player.Instance.Dead == false) {
                Player.Instance.Health += 10;
                InGameUIManager.Instance.SetHealthText(Player.Instance.Health);
            }
        }

        AudioManager.PlayClickSound();
    }

    int GetAvailableUpgradeCount() {
        int fullyUpgraded = 0;

        for (int i = 0; i < AvailableUpgrades.Length; i += 1) {
            Upgrade upgrade = AvailableUpgrades[i];
            
            if (m_Upgrades[upgrade] >= m_MaxUpgrades[upgrade]) {
                fullyUpgraded += 1;
            }
        }

        return AvailableUpgrades.Length - fullyUpgraded;
    }

    string GetUpgradeName(Upgrade upgrade) {
        LanguageSchemeGameUpgrade locale = LanguageManager.Data.Game.Upgrade;

        switch (upgrade){ 
            case Upgrade.MaxHP:
                return locale.MaxHP;
            case Upgrade.ProjectileSpeed:
                return locale.ProjectileSpeed;
            case Upgrade.AttackSpeed:
                return locale.AttackSpeed;
            case Upgrade.ProjectileSpread:
                return locale.ProjectileSpread;
            case Upgrade.MoveSpeed:
                return locale.MoveSpeed;
            case Upgrade.RotatingSpeed:
                return locale.RotatingSpeed;
            case Upgrade.RespawnSpeed:
                return locale.RespawnSpeed;
        }

        throw new System.Exception("Invalid Upgrade Name: " + upgrade.ToString());
    }

    public int GetUpgradeLevel(Upgrade upgrade) {
        return m_Upgrades[upgrade];
    }
}
