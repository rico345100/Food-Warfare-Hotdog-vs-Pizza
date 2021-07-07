using UnityEngine;
using Localization;

public class LanguageManager : GlobalSingleton<LanguageManager> {
	private Language m_Language = Language.English;
	private LanguageScheme m_Data;
	private LanguageEvent m_OnLanguageChanged = new LanguageEvent();

	public LanguageEvent OnLanguageChanged => m_OnLanguageChanged;

	public static Language Language {
		get => Instance.m_Language;
		set {
			Instance.m_Language = value;
			Instance.SaveSettings();
			Instance.ApplySettings();

			Instance.m_OnLanguageChanged.Invoke(Instance.m_Data);
		}
	}

	public static LanguageScheme Data => Instance.m_Data;

    protected override void OnInit() {
        InitSettings();
		LoadSettings();
		ApplySettings();
    }
	
	void InitSettings() {
		int wasSet = PlayerPrefs.GetInt("LanguageSet", 0);

		if (wasSet == 0) {
			PlayerPrefs.SetInt("Language", (int) Language.English);
			PlayerPrefs.SetInt("LanguageSet", 1);
			PlayerPrefs.Save();
		}
	}

	public void SaveSettings() {
		PlayerPrefs.SetInt("Language", (int) m_Language);
		PlayerPrefs.SetInt("LanguageSet", 1);
		PlayerPrefs.Save();
	}

	public void LoadSettings() {
		m_Language = (Language) PlayerPrefs.GetInt("Language");
	}

	public void ApplySettings() {
		string fileName;

		switch(m_Language) {
			case Language.English:
				fileName = "Texts/us-en";
				break;
			case Language.Korean:
				fileName = "Texts/ko-kr";
				break;
			default:
				throw new System.NotSupportedException("Language " + m_Language.ToString() + " doesn't support");
		}

		TextAsset textAsset = Resources.Load<TextAsset>(fileName);
		string jsonText = textAsset.text;

		m_Data = JsonUtility.FromJson<LanguageScheme>(jsonText);
	}
}
