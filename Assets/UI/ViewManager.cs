using UnityEngine;
using View;
using Localization;

public class ViewManager : LocalSingleton<ViewManager> {
    public static ViewBase CurrentView => Instance.m_CurrentView;

    private ViewBase m_CurrentView = null;
    private ViewBase m_PopupView = null;

    [SerializeField] private ViewBase[] m_ActiveViewsAfterInit = null;

    private ViewBase[] m_Views = null;
    
    protected override void OnInit() {
        LanguageManager.Instance.OnLanguageChanged.AddListener(BroadcastLanguageChange);
        
        m_Views = FindObjectsOfType<ViewBase>();

        foreach(ViewBase view in m_Views) {
            view.Initialize();
        }

        foreach(ViewBase view in m_Views) {
            view.OnLanguageChanged(LanguageManager.Data);
        }

        foreach(ViewBase activeView in m_ActiveViewsAfterInit) {
            activeView.Show();
        }
    }

    public static void SwitchView(ViewBase newView) {
        if (Instance.m_CurrentView != null) {
            Instance.m_CurrentView.Hide();
        }
        

        Instance.m_CurrentView = newView;
        Instance.m_CurrentView.Show();
    }

    public static void DisplayAsPopup(ViewBase popupView) {
        Instance.m_PopupView = popupView;
        Instance.m_PopupView.Show();
    }

    public static void HidePopup() {
        if (Instance.m_PopupView != null) {
            Instance.m_PopupView.Hide();
        }

        Instance.m_PopupView = null;
    }

    public static void HandleEscape() {
        if (Instance.m_PopupView) {
            Instance.m_PopupView.Escape();
        }
        else if (Instance.m_CurrentView) {
            Instance.m_CurrentView.Escape();
        }
    }

    void BroadcastLanguageChange(LanguageScheme locale) {
        foreach(ViewBase view in m_Views) {
            view.OnLanguageChanged(locale);
        }
    }
}
