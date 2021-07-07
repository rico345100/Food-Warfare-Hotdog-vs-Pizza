using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace View {
    public class Dialog : GlobalSingleton<Dialog> {
        public static bool HasActiveDialog => Instance.m_AlertActive || Instance.m_ConfirmActive;

        [Header("Dialog")]
        [SerializeField] private GameObject m_Dialog = null;

        [Header("Alert")]
        [SerializeField] private GameObject m_Alert = null;
        [SerializeField] private Text m_AlertText = null;
        [SerializeField] private Button m_AlertCloseButton = null;

        [Header("Confirm")]
        [SerializeField] private GameObject m_Confirm = null;
        [SerializeField] private Text m_ConfirmText = null;
        [SerializeField] private Button m_ConfirmOkayButton = null;
        [SerializeField] private Button m_ConfirmNoButton = null;

        private bool m_AlertActive = false;
        private bool m_ConfirmActive = false;

        private DialogAlertClose m_OnAlertClosed = null;
		private DialogConfirmClose m_OnConfirmClosed = null;

        protected override void OnInit() {
            m_Dialog.SetActive(false);

            m_AlertCloseButton.onClick.AddListener(CloseAlert);
			m_ConfirmOkayButton.onClick.AddListener(() => CloseConfirm(true));
			m_ConfirmNoButton.onClick.AddListener(() => CloseConfirm(false));
        }

        void CloseAlert() {
			if (m_OnAlertClosed != null) {
				DialogAlertClose previousCallback = m_OnAlertClosed;
				m_OnAlertClosed();

				// Check callback was changed. If it is, means that user invoked alert inside of callback
				// So if both same, means no more alert so free the callback
				if (previousCallback == m_OnAlertClosed) {
					m_OnAlertClosed = null;
				}
			}
			else {
				m_OnAlertClosed = null;
			}

			m_Dialog.SetActive(false);
			m_Alert.SetActive(false);

			m_AlertActive = false;
		}

        void CloseConfirm(bool confirmed) {
			if (m_OnConfirmClosed != null) {
				DialogConfirmClose previousCallback = m_OnConfirmClosed;
				m_OnConfirmClosed(confirmed);

				if (previousCallback == m_OnConfirmClosed) {
					m_OnConfirmClosed = null;
				}
			}
			else {
				m_OnConfirmClosed = null;
			}

			m_Dialog.SetActive(false);
			m_Confirm.SetActive(false);

			m_ConfirmActive = false;
		}

        public static void Alert(AlertOptions options) {
			Instance.m_AlertText.text = options.Message;
			Instance.m_AlertCloseButton.GetComponentInChildren<Text>().text = options.CloseText ?? LanguageManager.Data.View.Dialog.Close;
			Instance.m_OnAlertClosed = options.OnClose;
			
			Instance.ShowAlert();
			Instance.m_AlertActive = true;
		}

		public static void Confirm(ConfirmOptions options) {
			Instance.m_ConfirmText.text = options.Message;
			Instance.m_ConfirmOkayButton.GetComponentInChildren<Text>().text = options.OkText ?? LanguageManager.Data.View.Dialog.Confirm;
			Instance.m_ConfirmNoButton.GetComponentInChildren<Text>().text = options.CloseText ?? LanguageManager.Data.View.Dialog.Close;
			Instance.m_OnConfirmClosed = options.OnClose;

			Instance.ShowConfirm();
			Instance.m_ConfirmActive = true;
		}

        private void ShowAlert() {
			StartCoroutine(CoShow(m_Alert));
		}


		private void ShowConfirm() {
			StartCoroutine(CoShow(m_Confirm));
		}

        public static void CloseAll() {
            if (Instance.m_AlertActive) {
                Instance.CloseAlert();
            }
            if (Instance.m_ConfirmActive) {
                Instance.CloseConfirm(false);
            }
        }

		IEnumerator CoShow(GameObject obj) {
			yield return new WaitForSeconds(0.1f);
			obj.SetActive(true);
			m_Dialog.SetActive(true);

			LayoutRebuilder.ForceRebuildLayoutImmediate(obj.GetComponent<RectTransform>());
		}
    }
}
