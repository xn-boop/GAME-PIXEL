using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Map0
{
    [System.Serializable]
    public struct Map0_DialogueLine
    {
        public string speakerName;
        [TextArea(2, 5)] public string content;
        public Color speakerColor;

        public Map0_DialogueLine(string speaker, string text, Color color)
        {
            speakerName = speaker;
            content = text;
            speakerColor = color;
        }

        public Map0_DialogueLine(string speaker, string text)
        {
            speakerName = speaker;
            content = text;
            // Rich high-contrast colors on light yellow parchment paper background
            speakerColor = (speaker == "Hạ Ân") ? new Color32(185, 30, 20, 255) :
                           (speaker == "Mèo Vàng") ? new Color32(205, 110, 15, 255) :
                           new Color32(65, 35, 20, 255);
        }
    }

    public class Map0_UIManager : MonoBehaviour
    {
        public static Map0_UIManager Instance { get; private set; }

        [Header("Fade Overlay")]
        [SerializeField] private Image fadeOverlay;

        [Header("HUD Objectives & Quest")]
        [SerializeField] private TextMeshProUGUI txtObjective;
        [SerializeField] private TextMeshProUGUI txtQuestHint;

        [Header("Toast Notification")]
        [SerializeField] private GameObject toastPanel;
        [SerializeField] private TextMeshProUGUI txtToast;

        [Header("Interaction Prompt")]
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private TextMeshProUGUI txtPrompt;

        [Header("Inventory HUD (5 Slots)")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Image[] slotIcons = new Image[5];
        [SerializeField] private TextMeshProUGUI[] slotLabels = new TextMeshProUGUI[5];

        [Header("Dialogue Box UI")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI txtSpeakerName;
        [SerializeField] private TextMeshProUGUI txtDialogueContent;
        [SerializeField] private Button btnDialogueNext;
        [SerializeField] private TextMeshProUGUI txtDialogueHint;

        private Coroutine toastCoroutine;
        private Coroutine fadeCoroutine;
        private Coroutine typewriterCoroutine;

        private Queue<Map0_DialogueLine> dialogueQueue = new Queue<Map0_DialogueLine>();
        private Action onDialogueCompletedCallback;
        private bool isTyping = false;
        private string currentFullLine = "";
        private float lastAdvanceTime = 0f;

        public bool IsDialogueActive => dialoguePanel != null && dialoguePanel.activeSelf;
        public Image FirstSlotIcon => (slotIcons != null && slotIcons.Length > 0) ? slotIcons[0] : null;
        public TextMeshProUGUI FirstSlotLabel => (slotLabels != null && slotLabels.Length > 0) ? slotLabels[0] : null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ApplyLayoutSafeguards();

            if (toastPanel != null) toastPanel.SetActive(false);
            if (promptPanel != null) promptPanel.SetActive(false);
            if (dialoguePanel != null) dialoguePanel.SetActive(false);

            if (btnDialogueNext != null)
            {
                btnDialogueNext.onClick.AddListener(AdvanceDialogue);
            }

            if (fadeOverlay != null)
            {
                fadeOverlay.gameObject.SetActive(true);
            }
        }

        private void Start()
        {
            ApplyLayoutSafeguards();
        }

        /// <summary>
        /// Căn chỉnh toàn bộ chữ trong các khung vàng (Bảng nhiệm vụ & Khung hội thoại)
        /// Tham khảo phong cách Map 3: Chữ lớn hơn, in đậm (Bold), khoảng cách dòng 6f, màu chữ nâu đậm ấm #372016
        /// Nằm trọn vẹn trong lòng bảng vàng (Quiz/Mission frame) và không bị tràn ra ngoài.
        /// </summary>
        public void ApplyLayoutSafeguards()
        {
            // 1. Objective badge (Bảng vàng Mục tiêu góc trên bên trái)
            if (txtObjective != null)
            {
                var objPanelRt = txtObjective.transform.parent as RectTransform;
                if (objPanelRt != null)
                {
                    if (objPanelRt.sizeDelta.x < 460f)
                    {
                        objPanelRt.sizeDelta = new Vector2(460f, 85f);
                    }
                }
                var txtObjRt = txtObjective.rectTransform;
                txtObjRt.anchorMin = Vector2.zero;
                txtObjRt.anchorMax = Vector2.one;
                txtObjRt.offsetMin = new Vector2(36f, 10f);
                txtObjRt.offsetMax = new Vector2(-36f, -10f);
                txtObjective.alignment = TextAlignmentOptions.Center;
                txtObjective.enableAutoSizing = true;
                txtObjective.fontSizeMin = 17f;
                txtObjective.fontSizeMax = 22f;
                txtObjective.fontSize = 21f;
                txtObjective.fontStyle = FontStyles.Bold;
                txtObjective.color = new Color32(55, 32, 22, 255);
                txtObjective.textWrappingMode = TextWrappingModes.Normal;
            }

            // 2. Dialogue Card (Bảng hội thoại ở phía dưới)
            // Quiz.png có top wood border = 110px, bottom border = 50px, left/right = 50px.
            // Để TOÀN BỘ NỘI DUNG NẰM GỌN BÊN TRONG KHUNG MÀU VÀNG NHẠT:
            // - Khung vàng nhạt bắt đầu từ top >= 118px và kết thúc ở bottom >= 50px.
            // - Tên người nói: đặt ở góc trên bên trái khung vàng nhạt (y = -122f).
            // - Nội dung thoại: đặt bên dưới tên người nói (y = -150f đến y = 62f).
            // - Dòng gợi ý: đặt ở góc dưới bên phải khung vàng nhạt (y = 54f).
            if (txtDialogueContent != null)
            {
                var cardRt = txtDialogueContent.transform.parent as RectTransform;
                if (cardRt != null)
                {
                    cardRt.sizeDelta = new Vector2(1120f, 300f);
                    cardRt.anchoredPosition = new Vector2(0f, 165f);
                }

                var contentRt = txtDialogueContent.rectTransform;
                contentRt.anchorMin = Vector2.zero;
                contentRt.anchorMax = Vector2.one;
                contentRt.offsetMin = new Vector2(75f, 62f);
                contentRt.offsetMax = new Vector2(-75f, -150f);
                txtDialogueContent.alignment = TextAlignmentOptions.TopLeft;
                txtDialogueContent.enableAutoSizing = true;
                txtDialogueContent.fontSizeMin = 16f;
                txtDialogueContent.fontSizeMax = 21f;
                txtDialogueContent.fontSize = 20f;
                txtDialogueContent.fontStyle = FontStyles.Bold;
                txtDialogueContent.color = new Color32(55, 32, 22, 255); // #372016 mực nâu đậm ấm
                txtDialogueContent.lineSpacing = 4f;
                txtDialogueContent.textWrappingMode = TextWrappingModes.Normal;
            }

            if (txtSpeakerName != null)
            {
                var speakerRt = txtSpeakerName.rectTransform;
                speakerRt.anchorMin = new Vector2(0f, 1f);
                speakerRt.anchorMax = new Vector2(0f, 1f);
                speakerRt.pivot = new Vector2(0f, 1f);
                speakerRt.anchoredPosition = new Vector2(75f, -122f);
                speakerRt.sizeDelta = new Vector2(280f, 26f);
                txtSpeakerName.fontSize = 21f;
                txtSpeakerName.fontStyle = FontStyles.Bold;
                txtSpeakerName.alignment = TextAlignmentOptions.MidlineLeft;
            }

            if (txtDialogueHint != null)
            {
                var hintRt = txtDialogueHint.rectTransform;
                hintRt.anchorMin = new Vector2(1f, 0f);
                hintRt.anchorMax = new Vector2(1f, 0f);
                hintRt.pivot = new Vector2(1f, 0f);
                hintRt.anchoredPosition = new Vector2(-75f, 54f);
                hintRt.sizeDelta = new Vector2(340f, 18f);
                txtDialogueHint.fontSize = 13f;
                txtDialogueHint.fontStyle = FontStyles.Italic;
                txtDialogueHint.color = new Color32(140, 95, 60, 220);
                txtDialogueHint.alignment = TextAlignmentOptions.Right;
            }

            // 3. Quest hint (Banner hướng dẫn phía trên giữa phòng)
            if (txtQuestHint != null)
            {
                var questRt = txtQuestHint.rectTransform;
                questRt.anchorMin = Vector2.zero;
                questRt.anchorMax = Vector2.one;
                questRt.offsetMin = new Vector2(30f, 8f);
                questRt.offsetMax = new Vector2(-30f, -8f);
                txtQuestHint.alignment = TextAlignmentOptions.Center;
                txtQuestHint.enableAutoSizing = true;
                txtQuestHint.fontSizeMin = 16f;
                txtQuestHint.fontSizeMax = 22f;
                txtQuestHint.fontSize = 21f;
                txtQuestHint.fontStyle = FontStyles.Bold;
                txtQuestHint.textWrappingMode = TextWrappingModes.Normal;
            }
        }

        private void Update()
        {
            // When dialogue is active, UIManager is the sole listener for advance inputs: [E], [Space], [Enter], Left-click
            if (IsDialogueActive)
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
                {
                    AdvanceDialogue();
                }
            }
        }

        #region Screen Fade
        public void SetFadeAlpha(float alpha)
        {
            if (fadeOverlay != null)
            {
                fadeOverlay.gameObject.SetActive(true);
                var c = fadeOverlay.color;
                c.a = Mathf.Clamp01(alpha);
                fadeOverlay.color = c;
            }
        }

        public void FadeFromBlack(float duration, Action onComplete = null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(1f, 0f, duration, onComplete));
        }

        public void FadeToBlack(float duration, Action onComplete = null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(0f, 1f, duration, onComplete));
        }

        private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration, Action onComplete)
        {
            if (fadeOverlay == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            fadeOverlay.gameObject.SetActive(true);
            Color c = fadeOverlay.color;
            c.a = fromAlpha;
            fadeOverlay.color = c;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(fromAlpha, toAlpha, Mathf.Clamp01(elapsed / duration));
                fadeOverlay.color = c;
                yield return null;
            }

            c.a = toAlpha;
            fadeOverlay.color = c;
            if (toAlpha <= 0.001f)
            {
                fadeOverlay.gameObject.SetActive(false);
            }

            fadeCoroutine = null;
            onComplete?.Invoke();
        }
        #endregion

        #region Objectives & Hints
        public void SetObjective(string text)
        {
            if (txtObjective != null)
            {
                txtObjective.text = text;
            }
        }

        public void SetQuestHint(string text)
        {
            if (txtQuestHint != null)
            {
                txtQuestHint.text = text;
            }
        }

        public void SetInteractPrompt(bool show, string text = "[E] Tương tác")
        {
            if (promptPanel != null)
            {
                promptPanel.SetActive(show);
            }
            if (txtPrompt != null)
            {
                txtPrompt.text = text;
            }
        }
        #endregion

        #region Toast
        public void ShowToast(string message, float duration = 3.5f)
        {
            if (toastCoroutine != null) StopCoroutine(toastCoroutine);
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
        #endregion

        #region Inventory
        public void SetInventorySlot(int index, Sprite icon, string label)
        {
            if (index < 0 || index >= slotIcons.Length) return;

            if (slotIcons[index] != null)
            {
                slotIcons[index].sprite = icon;
                slotIcons[index].color = Color.white;
                slotIcons[index].preserveAspect = true;
                slotIcons[index].enabled = true;
                slotIcons[index].gameObject.SetActive(icon != null);
            }

            if (slotLabels[index] != null)
            {
                slotLabels[index].text = label;
                slotLabels[index].gameObject.SetActive(!string.IsNullOrEmpty(label));
            }
        }

        public void ClearInventorySlot(int index)
        {
            if (index < 0 || index >= slotIcons.Length) return;

            if (slotIcons[index] != null)
            {
                slotIcons[index].sprite = null;
                slotIcons[index].gameObject.SetActive(false);
            }

            if (slotLabels[index] != null)
            {
                slotLabels[index].text = "";
                slotLabels[index].gameObject.SetActive(false);
            }
        }
        #endregion

        #region Dialogue System
        public void ShowDialogue(Map0_DialogueLine[] lines, Action onComplete = null)
        {
            if (lines == null || lines.Length == 0)
            {
                onComplete?.Invoke();
                return;
            }

            dialogueQueue.Clear();
            foreach (var line in lines)
            {
                dialogueQueue.Enqueue(line);
            }

            onDialogueCompletedCallback = onComplete;

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            ApplyLayoutSafeguards();

            // Lock movement and clear interact prompt while dialogue is open
            if (Map0_PlayerController.Instance != null)
            {
                Map0_PlayerController.Instance.SetMovementLocked(true);
            }
            SetInteractPrompt(false);

            lastAdvanceTime = Time.unscaledTime;
            ShowNextDialogueLine();
        }

        public void AdvanceDialogue()
        {
            if (!IsDialogueActive) return;

            // Debounce multiple events in same frame / within 0.12s
            if (Time.unscaledTime - lastAdvanceTime < 0.12f) return;
            lastAdvanceTime = Time.unscaledTime;

            if (isTyping)
            {
                // 1st press while typing: immediately display full line
                if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
                isTyping = false;
                if (txtDialogueContent != null) txtDialogueContent.text = currentFullLine;
                return;
            }

            if (dialogueQueue.Count > 0)
            {
                ShowNextDialogueLine();
            }
            else
            {
                CloseDialogue();
            }
        }

        private void ShowNextDialogueLine()
        {
            if (dialogueQueue.Count == 0) return;

            var line = dialogueQueue.Dequeue();
            currentFullLine = line.content;

            if (txtSpeakerName != null)
            {
                txtSpeakerName.text = line.speakerName;
                txtSpeakerName.color = line.speakerColor;
            }

            if (txtDialogueContent != null)
            {
                txtDialogueContent.color = new Color32(55, 32, 22, 255);
            }

            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypewriterRoutine(currentFullLine));
        }

        private IEnumerator TypewriterRoutine(string line)
        {
            isTyping = true;
            if (txtDialogueContent != null)
            {
                txtDialogueContent.text = "";
                for (int i = 0; i <= line.Length; i++)
                {
                    txtDialogueContent.text = line.Substring(0, i);
                    yield return new WaitForSeconds(0.015f);
                }
            }
            isTyping = false;
            typewriterCoroutine = null;
        }

        private void CloseDialogue()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            isTyping = false;

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            // Unlock player movement and apply debounce cooldown (0.3s) so the dismissal key does not re-trigger interact
            if (Map0_PlayerController.Instance != null)
            {
                Map0_PlayerController.Instance.StartInteractionCooldown(0.3f);
                Map0_PlayerController.Instance.SetMovementLocked(false);
                Map0_PlayerController.Instance.UpdateInteractPrompt();
            }

            var callback = onDialogueCompletedCallback;
            onDialogueCompletedCallback = null;
            callback?.Invoke();
        }
        #endregion
    }
}
