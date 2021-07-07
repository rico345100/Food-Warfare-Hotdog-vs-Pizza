using UnityEngine;
using Localization;

namespace View {
    public abstract class ViewBase : MonoBehaviour {
        [Header("ViewBase")]
        [SerializeField] private GameObject m_ViewObject = null;

        protected virtual void OnInitialize() {}
        protected virtual void OnShow() {}
        protected virtual void OnHide() {}
        protected virtual void OnEscape() {}

        public abstract void OnLanguageChanged(LanguageScheme locale);

        public bool Active => m_ViewObject.activeSelf;

        public void Initialize() {
            OnInitialize();
            m_ViewObject.SetActive(false);
        }

        public void Escape() {
            OnEscape();
        }

        public void Show() {
            m_ViewObject.SetActive(true);
            OnShow();
        }

        public void Hide() {
            m_ViewObject.SetActive(false);
            OnHide();
        }
    }
}
