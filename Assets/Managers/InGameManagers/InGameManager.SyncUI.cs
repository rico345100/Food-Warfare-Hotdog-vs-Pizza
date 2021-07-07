using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Localization;

public partial class InGameManager {
    private enum SyncUIType {
        ShowInfoText = 0,
        SetWaveText = 1,
        ShowUpgrade = 2,
    }

    private enum InfoTextType {
        ProtectPizzaFromEnemies = 0,
        WaveStart = 1,
        NextWaveIncoming = 2,
    }

    void HandleUISyncEvent(object rawData) {
        object[] data = (object[]) rawData;

        SyncUIType syncUIType = (SyncUIType) data[0];

        switch (syncUIType) {
            case SyncUIType.ShowInfoText:
                InfoTextType infoTextType = (InfoTextType) data[1];
                LanguageSchemeGame locale = LanguageManager.Data.Game;
                string message;

                switch (infoTextType) {
                    case InfoTextType.ProtectPizzaFromEnemies:
                        message = locale.ProtectPizzaFromEnemies;
                        break;
                    case InfoTextType.WaveStart:
                        message = string.Format(
                            locale.WaveStart,
                            (int) data[2]
                        );
                        break;
                    case InfoTextType.NextWaveIncoming:
                        message = locale.NextWaveIncoming;
                        break;
                    default:
                        throw new System.Exception("Invalid Info Text Type:"  + infoTextType.ToString());
                }

                InGameUIManager.Instance.ShowInfoText(message);
                break;
            case SyncUIType.SetWaveText:
                InGameUIManager.Instance.SetWaveText((int) data[1]);
                break;
            case SyncUIType.ShowUpgrade:
                ShowUpgrade();
                break;
            default:
                throw new System.Exception("Invalid Sync UI Type: " + syncUIType);
        }
    }

    void SendSyncUI(SyncUIType syncUIType, params object[] args) {
        List<object> contentList = new List<object>();
        contentList.Add(syncUIType);
        contentList.AddRange(args);

        object[] content = contentList.ToArray();

        if (PhotonNetwork.OfflineMode) {
            HandleUISyncEvent(content);
        }
        else {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions() {
                Receivers = ReceiverGroup.All,
            };

            PhotonNetwork.RaiseEvent(MultiplayerEventCode.SyncUI, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    void ShowUpgrade() {
        if (InGameUIManager.Instance.UpgradePanelVisible) {
            int upgradeCount = m_CurrentAvailableUpgrade.Count;
            int randomIndex = Random.Range(0, upgradeCount);
            InGameUIManager.Instance.OnUpgradeSelect.Invoke(randomIndex);
        }

        int availableUpgradeCount = GetAvailableUpgradeCount();

        if (availableUpgradeCount > 0) {
            int upgradeCount = availableUpgradeCount;

            if (upgradeCount > 3) {
                upgradeCount = 3;
            }

            SetRandomUpgrades(upgradeCount);
            InGameUIManager.Instance.ShowUpgrade();
        }
    }
}
