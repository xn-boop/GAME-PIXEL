using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Map3
{
    public class Map3_UIManager : MonoBehaviour
    {
        public static Map3_UIManager Instance { get; private set; }

        [Header("Screen Fade Overlay")]
        [SerializeField] private Image fadeOverlay;

        [Header("Toast / Notification Panel")]
        [SerializeField] private GameObject toastPanel;
        [SerializeField] private TextMeshProUGUI txtToast;

        [Header("Interaction Prompt")]
        [SerializeField] private GameObject interactPrompt;
        [SerializeField] private TextMeshProUGUI txtInteractPrompt;

        [Header("Quest / Objectives")]
        [SerializeField] private TextMeshProUGUI txtObjective;
        [SerializeField] private TextMeshProUGUI txtQuestHint;

        [Header("Victory Panel")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private TextMeshProUGUI txtVictoryTitle;
        [SerializeField] private TextMeshProUGUI txtVictoryDesc;
        [SerializeField] private Button btnRestart;

        private Coroutine toastCoroutine;
        private Coroutine fadeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (toastPanel != null) toastPanel.SetActive(false);
            if (interactPrompt != null) interactPrompt.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            if (fadeOverlay != null)
            {
                fadeOverlay.gameObject.SetActive(true);
                var c = fadeOverlay.color;
                c.a = 0f;
                fadeOverlay.color = c;
            }

            if (btnRestart != null)
            {
                btnRestart.onClick.AddListener(OnRestartClicked);
            }
        }

        private void Start()
        {
            SetQuestHint("Nói chuyện với chú Mèo để bắt đầu hành trình...");
            SetObjective("MỤC TIÊU: Khám phá Vườn Giao Thoa");
        }

        public void SetQuestHint(string hint)
        {
            if (txtQuestHint != null) txtQuestHint.text = hint;
        }

        public void SetObjective(string obj)
        {
            if (txtObjective != null) txtObjective.text = obj;
        }

        public void SetInteractPrompt(bool show, string text = "[E] Tương tác")
        {
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(show);
            }
            if (txtInteractPrompt != null)
            {
                txtInteractPrompt.text = text;
            }
        }

        public void ShowToast(string message, float duration = 3.0f)
        {
            if (toastCoroutine != null)
            {
                StopCoroutine(toastCoroutine);
            }
            toastCoroutine = StartCoroutine(ToastRoutine(message, duration));
        }

        private IEnumerator ToastRoutine(string message, float duration)
        {
            if (toastPanel != null && txtToast != null)
            {
                txtToast.text = message;
                toastPanel.SetActive(true);

                yield return new WaitForSeconds(duration);

                toastPanel.SetActive(false);
            }
            toastCoroutine = null;
        }

        public void FadeOutIn(float fadeDuration, Action onFadedOut)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeRoutine(fadeDuration, onFadedOut));
        }

        private IEnumerator FadeRoutine(float duration, Action onFadedOut)
        {
            if (fadeOverlay == null)
            {
                onFadedOut?.Invoke();
                yield break;
            }

            fadeOverlay.gameObject.SetActive(true);
            Color c = fadeOverlay.color;

            // Fade Out (Alpha 0 -> 1)
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsed / duration);
                fadeOverlay.color = c;
                yield return null;
            }
            c.a = 1f;
            fadeOverlay.color = c;

            // Perform transition action at pitch black
            onFadedOut?.Invoke();
            yield return new WaitForSeconds(0.05f);

            // Fade In (Alpha 1 -> 0)
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(1f - (elapsed / duration));
                fadeOverlay.color = c;
                yield return null;
            }
            c.a = 0f;
            fadeOverlay.color = c;

            fadeCoroutine = null;
        }

        public void ShowVictory(string title = "", string desc = "")
        {
            if (victoryPanel != null)
            {
                if (txtVictoryTitle != null && !string.IsNullOrEmpty(title))
                {
                    txtVictoryTitle.text = title;
                }
                if (txtVictoryDesc != null && !string.IsNullOrEmpty(desc))
                {
                    txtVictoryDesc.text = desc;
                }
                victoryPanel.SetActive(true);
            }

            if (Map3_PlayerController.Instance != null)
            {
                Map3_PlayerController.Instance.SetMovementLocked(true);
            }
        }

        private void OnRestartClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
