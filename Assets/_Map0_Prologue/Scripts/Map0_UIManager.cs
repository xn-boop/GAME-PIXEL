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
            // Rich high-contrast colors on parchment paper background
            speakerColor = (speaker == "Hạ Ân") ? new Color32(165, 42, 24, 255) :
                           (speaker == "Mèo Vàng") ? new Color32(195, 100, 15, 255) :
                           new Color32(45, 28, 16, 255);
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
