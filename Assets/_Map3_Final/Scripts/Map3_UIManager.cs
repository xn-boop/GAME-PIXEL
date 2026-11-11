using System;
using System.Collections;
using System.Linq;
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
        [SerializeField] private TextMeshProUGUI txtToastTitle;

        [Header("Hint / Dialogue Modal Panel")]
        [SerializeField] private GameObject hintDialogPanel;
        [SerializeField] private TextMeshProUGUI txtHintTitle;
        [SerializeField] private TextMeshProUGUI txtHintMessage;
        [SerializeField] private Button btnHintConfirm;
        [SerializeField] private Button btnHintClose;

        [Header("Cat Hint Shortcut Button")]
        [SerializeField] private Button btnCatHint;

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
        private Action onHintDialogClosed;

        private bool hasReceivedCatHint = false;
        private string lastHintTitle = "GỢI Ý TỪ CHÚ MÈO";
        private string lastHintMessage = Map3_CatHint.CatHintMessage;

        public bool IsHintDialogOpen => hintDialogPanel != null && hintDialogPanel.activeSelf;
        public Button BtnCatHint => btnCatHint;
        public bool HasReceivedCatHint => hasReceivedCatHint;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (toastPanel != null) toastPanel.SetActive(false);
            if (hintDialogPanel != null) hintDialogPanel.SetActive(false);
            if (interactPrompt != null) interactPrompt.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);

            if (btnHintConfirm != null)
            {
                btnHintConfirm.onClick.AddListener(CloseHintDialog);
            }
            if (btnHintClose != null)
            {
                btnHintClose.onClick.AddListener(CloseHintDialog);
            }

            if (btnCatHint != null)
            {
                btnCatHint.onClick.AddListener(OnCatHintButtonClicked);
                btnCatHint.gameObject.SetActive(false);
            }
            else
            {
                EnsureCatHintButtonExists();
            }

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

            if (Map3_GameManager.Instance != null && Map3_GameManager.Instance.hasTalkedToCat)
            {
                hasReceivedCatHint = true;
                if (btnCatHint == null) EnsureCatHintButtonExists();
                if (btnCatHint != null)
                {
                    btnCatHint.gameObject.SetActive(true);
                }
            }
        }

        private void Update()
        {
            // Tự động đảm bảo nút [ ! ] luôn hiển thị khi người chơi đã nhận gợi ý từ chú Mèo
            if (Map3_GameManager.Instance != null && Map3_GameManager.Instance.hasTalkedToCat && !IsHintDialogOpen)
            {
                hasReceivedCatHint = true;
                if (btnCatHint == null) EnsureCatHintButtonExists();
                if (btnCatHint != null && !btnCatHint.gameObject.activeSelf)
                {
                    btnCatHint.gameObject.SetActive(true);
                }
            }
        }

        public void SetQuestHint(string hint)
        {
            if (txtQuestHint != null) txtQuestHint.text = hint;

            // Nếu nhiệm vụ đã chuyển sang tìm chìa khóa & gỗ quanh bờ Việt -> chú Mèo đã cho gợi ý
            if (!string.IsNullOrEmpty(hint) && (hint.Contains("Chìa khóa") || hint.Contains("mảnh gỗ") || hint.Contains("Mảnh gỗ")))
            {
                hasReceivedCatHint = true;
                if (btnCatHint == null) EnsureCatHintButtonExists();
                if (btnCatHint != null && !IsHintDialogOpen && !btnCatHint.gameObject.activeSelf)
                {
                    btnCatHint.gameObject.SetActive(true);
                }
            }
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

        public void ShowToast(string message, float duration = 5.0f, string title = "")
        {
            if (toastCoroutine != null)
            {
                StopCoroutine(toastCoroutine);
            }
            toastCoroutine = StartCoroutine(ToastRoutine(message, duration, title));
        }

        private IEnumerator ToastRoutine(string message, float duration, string title)
        {
            if (toastPanel != null && txtToast != null)
            {
                if (txtToastTitle != null)
                {
                    if (!string.IsNullOrEmpty(title))
                    {
                        txtToastTitle.text = title;
                    }
                    else if (message.Contains("Chìa khóa") || message.Contains("mảnh gỗ") || message.Contains("Đền Việt") || message.Contains("chú Mèo"))
                    {
                        txtToastTitle.text = "GỢI Ý TỪ CHÚ MÈO";
                    }
                    else
                    {
                        txtToastTitle.text = "THÔNG BÁO";
                    }
                }

                txtToast.text = message;
                toastPanel.SetActive(true);

                yield return new WaitForSeconds(duration);

                toastPanel.SetActive(false);
            }
            toastCoroutine = null;
        }

        public void ShowHintDialog(string title, string message, Action onClose = null)
        {
            hasReceivedCatHint = true;
            if (!string.IsNullOrEmpty(title))
            {
                lastHintTitle = title;
            }
            if (!string.IsNullOrEmpty(message))
            {
                lastHintMessage = message;
            }

            if (hintDialogPanel == null)
            {
                ShowToast(message, 6.0f, title);
                onClose?.Invoke();
                return;
            }

            onHintDialogClosed = onClose;
            if (txtHintTitle != null)
            {
                txtHintTitle.text = string.IsNullOrEmpty(title) ? "GỢI Ý TỪ CHÚ MÈO" : title;
            }
            if (txtHintMessage != null)
            {
                txtHintMessage.text = message;
            }

            hintDialogPanel.SetActive(true);

            if (Map3_PlayerController.Instance != null)
            {
                Map3_PlayerController.Instance.SetMovementLocked(true);
            }
        }

        public void CloseHintDialog()
        {
            if (hintDialogPanel != null)
            {
                hintDialogPanel.SetActive(false);
            }

            if (toastPanel != null)
            {
                toastPanel.SetActive(false);
            }
            if (toastCoroutine != null)
            {
                StopCoroutine(toastCoroutine);
                toastCoroutine = null;
            }

            if (Map3_PlayerController.Instance != null)
            {
                Map3_PlayerController.Instance.SetMovementLocked(false);
            }

            // Khi người chơi xác nhận ĐÃ HIỂU (hoặc đóng bảng gợi ý), xuất hiện nút icon "!" góc trên phải
            if (hasReceivedCatHint)
            {
                if (btnCatHint == null)
                {
                    EnsureCatHintButtonExists();
                }
                if (btnCatHint != null)
                {
                    btnCatHint.gameObject.SetActive(true);
                }
            }

            var callback = onHintDialogClosed;
            onHintDialogClosed = null;
            callback?.Invoke();
        }

        public void EnsureCatHintButtonExists()
        {
            if (btnCatHint != null) return;

            // 1. Search existing button in hierarchy
            Canvas rootCanvas = GetComponentInParent<Canvas>();
            if (rootCanvas == null)
            {
                rootCanvas = FindFirstObjectByType<Canvas>();
            }

            Transform parentT = rootCanvas != null ? rootCanvas.transform : transform;
            Transform found = parentT.Find("Btn_CatHint");
            if (found == null)
            {
                found = parentT.Find("HUD_TopRight_Buttons/Btn_CatHint");
            }

            if (found != null)
            {
                btnCatHint = found.GetComponent<Button>();
                if (btnCatHint != null)
                {
                    btnCatHint.onClick.RemoveListener(OnCatHintButtonClicked);
                    btnCatHint.onClick.AddListener(OnCatHintButtonClicked);
                }
                return;
            }

            // 2. If not found in hierarchy, dynamically create it at runtime
            GameObject btnGo = new GameObject("Btn_CatHint");
            btnGo.transform.SetParent(parentT, false);

            var rt = btnGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-350f, -33f);
            rt.sizeDelta = new Vector2(54f, 54f);

            var img = btnGo.AddComponent<Image>();
            Sprite btnWoodSprite = null;
            Sprite iconExclamation = null;

            var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (var s in allSprites)
            {
                if (s == null) continue;
                if (s.name == "Btn_Wood_Square" && btnWoodSprite == null) btnWoodSprite = s;
                if (s.name == "Icon_Exclamation" && iconExclamation == null) iconExclamation = s;
            }

            img.sprite = btnWoodSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btn.colors = colors;

            GameObject iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(btnGo.transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(54f * 0.62f, 54f * 0.62f);

            var iconImg = iconGo.AddComponent<Image>();
            if (iconExclamation != null)
            {
                iconImg.sprite = iconExclamation;
                iconImg.preserveAspect = true;
                iconImg.color = Color.white;
            }
            else
            {
                // Fallback text if sprite not loaded
                GameObject txtGo = new GameObject("Text");
                txtGo.transform.SetParent(btnGo.transform, false);
                var txtRt = txtGo.AddComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.sizeDelta = Vector2.zero;
                var tmp = txtGo.AddComponent<TextMeshProUGUI>();
                tmp.text = "!";
                tmp.fontSize = 28;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(1f, 0.84f, 0.31f, 1f);
            }
            iconImg.raycastTarget = false;

            btnCatHint = btn;
            btnCatHint.onClick.AddListener(OnCatHintButtonClicked);
            btnGo.SetActive(false);
        }

        public void OnCatHintButtonClicked()
        {
            ShowHintDialog(lastHintTitle, lastHintMessage);
        }

        public void SetCatHintButtonVisible(bool visible)
        {
            if (btnCatHint == null && visible)
            {
                EnsureCatHintButtonExists();
            }
            if (btnCatHint != null)
            {
                btnCatHint.gameObject.SetActive(visible);
            }
        }

        public void SetHasReceivedCatHintForTesting(bool val)
        {
            hasReceivedCatHint = val;
        }

        public void SetBtnCatHintForTesting(Button btn)
        {
            btnCatHint = btn;
            if (btnCatHint != null)
            {
                btnCatHint.onClick.AddListener(OnCatHintButtonClicked);
            }
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

            // Tự động chuyển tiếp sang màn hình Puzzle sau 4.5 giây để hoàn tất mảnh ghép cuối cùng
            StartCoroutine(AutoTransitionToPuzzleRoutine());
        }

        private IEnumerator AutoTransitionToPuzzleRoutine()
        {
            yield return new WaitForSeconds(4.5f);
            ContinueToPuzzle();
        }

        public void ContinueToPuzzle()
        {
            // Ghi nhận hoàn thành Màn 3 để mở khóa Mảnh ghép cuối cùng trong scene "Puzzle"
            PlayerPrefs.SetInt("Puzzle_RecentCompletedMap", 3);
            PlayerPrefs.SetInt("Puzzle_UnlockedPieces", 3);
            PlayerPrefs.Save();

            if (Application.CanStreamedLevelBeLoaded("Puzzle"))
            {
                SceneManager.LoadScene("Puzzle");
            }
            else if (Application.CanStreamedLevelBeLoaded("Assets/Scenes/Puzzle.unity"))
            {
                SceneManager.LoadScene("Assets/Scenes/Puzzle.unity");
            }
            else if (Application.CanStreamedLevelBeLoaded("Puzzel"))
            {
                SceneManager.LoadScene("Puzzel");
            }
            else if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                SceneManager.LoadScene("MainMenu");
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
