using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Map0
{
    public enum PrologueStage
    {
        Stage1_ArriveHome,
        Stage2_ExploreLivingRoom,
        Stage3_OpenTimeCapsule,
        Stage4_CatGuidance,
        Completed
    }

    public class Map0_PrologueManager : MonoBehaviour
    {
        public static Map0_PrologueManager Instance { get; private set; }

        [Header("Current Story State")]
        [SerializeField] private PrologueStage currentStage = PrologueStage.Stage1_ArriveHome;

        [Header("Target Next Scene")]
        [SerializeField] private string nextSceneName = "Level1_Prototype";

        [Header("Core References")]
        [SerializeField] private Map0_PlayerController playerController;
        [SerializeField] private Map0_UIManager uiManager;
        [SerializeField] private Map0_LightingController lightingController;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip openBoxSfx;
        [SerializeField] private AudioClip catMeowSfx;

        [Header("Interactive Scene Elements")]
        [SerializeField] private Map0_InteractZone timeCapsuleZone;
        [SerializeField] private Map0_InteractZone kitchenDoorZone;
        [SerializeField] private GameObject yellowCatNpc;
        [SerializeField] private Transform catWalkTarget;
        [SerializeField] private Sprite whiteJournalIcon;

        [Header("Inventory Item Settings")]
        [SerializeField] private Sprite diarySprite;
        [SerializeField] private UnityEngine.UI.Image slotImage;
        [SerializeField] private TMPro.TextMeshProUGUI slotLabel;

        private Coroutine stageCoroutine;

        public PrologueStage CurrentStage => currentStage;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (playerController == null) playerController = Map0_PlayerController.Instance;
            if (uiManager == null) uiManager = Map0_UIManager.Instance;
            if (lightingController == null) lightingController = Map0_LightingController.Instance;

            // Auto-resolve slotImage fallback if not assigned
            if (slotImage == null && uiManager != null)
            {
                slotImage = uiManager.FirstSlotIcon;
            }
            if (slotImage == null)
            {
                var inv = GameObject.Find("HUD_BottomCenter_Inv");
                if (inv != null)
                {
                    var slot1 = inv.transform.Find("Slot_01");
                    if (slot1 != null)
                    {
                        var iconTr = slot1.Find("Icon_Item");
                        if (iconTr != null) slotImage = iconTr.GetComponent<UnityEngine.UI.Image>();
                    }
                }
            }

            // Auto-resolve slotLabel fallback if not assigned
            if (slotLabel == null && uiManager != null)
            {
                slotLabel = uiManager.FirstSlotLabel;
            }
            if (slotLabel == null && slotImage != null && slotImage.transform.parent != null)
            {
                var lblTr = slotImage.transform.parent.Find("Txt_ItemName");
                if (lblTr != null) slotLabel = lblTr.GetComponent<TMPro.TextMeshProUGUI>();
                if (slotLabel == null) slotLabel = slotImage.transform.parent.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            }

            // Ensure YellowCat is inactive initially
            if (yellowCatNpc != null)
            {
                yellowCatNpc.SetActive(false);
            }

            // Ensure sound controller for top-right sound icon sync
            EnsureSoundController();

            if (bgmSource != null)
            {
                bool isMuted = AudioManager.Instance != null ? AudioManager.Instance.IsMuted : (PlayerPrefs.GetInt("Game_Audio_Muted", 0) == 1);
                bgmSource.mute = isMuted;
                if (isMuted && bgmSource.isPlaying)
                {
                    bgmSource.Pause();
                }
            }

            // Set initial state and start Stage 1 sequence
            currentStage = PrologueStage.Stage1_ArriveHome;
            StartStage1Sequence();
        }

        private void Update()
        {
            if (bgmSource != null && AudioManager.Instance != null)
            {
                if (AudioManager.Instance.IsMuted && bgmSource.isPlaying)
                {
                    bgmSource.Pause();
                }
                else if (!AudioManager.Instance.IsMuted && !bgmSource.isPlaying && currentStage < PrologueStage.Stage4_CatGuidance)
                {
                    bgmSource.UnPause();
                }
            }
        }

        private void EnsureSoundController()
        {
            var soundCtrl = GetComponent<MenuSoundController>();
            if (soundCtrl == null)
            {
                soundCtrl = gameObject.AddComponent<MenuSoundController>();
            }
        }

        #region Stage 1: Bước vào ngôi nhà cũ
        private void StartStage1Sequence()
        {
            if (stageCoroutine != null) StopCoroutine(stageCoroutine);
            stageCoroutine = StartCoroutine(Stage1Routine());
        }

        private IEnumerator Stage1Routine()
        {
            // 1. Lock player movement immediately
            if (playerController != null)
            {
                playerController.SetMovementLocked(true);
            }

            // 2. Set fade screen black initially, then fade in
            if (uiManager != null)
            {
                uiManager.SetFadeAlpha(1f);
                uiManager.FadeFromBlack(1.2f);
            }

            yield return new WaitForSeconds(1.3f);

            // 3. Play initial monologue of Hạ Ân
            var stage1Lines = new Map0_DialogueLine[]
            {
                new Map0_DialogueLine("Hạ Ân", "Vậy là mình đã tròn 20 tuổi... Ngôi nhà vẫn chẳng thay đổi gì sau ngần ấy năm, chỉ có mình là mang đầy những chông chênh."),
                new Map0_DialogueLine("Hạ Ân", "Hôm nay là ngày hẹn mở Chiếc Hộp Thời Gian cùng ba mẹ sau 12 năm. Nó vẫn nằm trên chiếc bàn giữa phòng kìa.")
            };

            if (uiManager != null)
            {
                uiManager.ShowDialogue(stage1Lines, OnStage1DialogueComplete);
            }
            else
            {
                OnStage1DialogueComplete();
            }
        }

        private void OnStage1DialogueComplete()
        {
            // Advance to Stage 2: Explore living room
            currentStage = PrologueStage.Stage2_ExploreLivingRoom;

            // Unlock player movement
            if (playerController != null)
            {
                playerController.SetMovementLocked(false);
            }

            // Show HUD updates and tutorial toast
            if (uiManager != null)
            {
                uiManager.ShowToast("Sử dụng Joystick bên trái để di chuyển. Chạm vào nút Bàn tay bên phải để tương tác.", 4.5f);
                uiManager.SetObjective("Mục tiêu: Mở Chiếc Hộp Thời Gian");
                uiManager.SetQuestHint("Hãy tiến lại chiếc bàn giữa phòng để mở Chiếc Hộp Thời Gian.");
            }
        }
        #endregion

        #region Stage 2: Khám phá phòng khách & Interaction Handling
        public void OnZoneInteracted(Map0_InteractZone zone)
        {
            if (zone == null) return;

            switch (zone.ZoneType)
            {
                case Map0_ZoneType.WallCalendar:
                    HandleCalendarInteraction();
                    break;

                case Map0_ZoneType.FamilyPhoto:
                    HandlePhotoInteraction();
                    break;

                case Map0_ZoneType.KitchenDoor:
                    HandleKitchenDoorInteraction();
                    break;

                case Map0_ZoneType.TimeCapsule:
                    HandleTimeCapsuleInteraction();
                    break;

                case Map0_ZoneType.YellowCat:
                    HandleYellowCatInteraction();
                    break;

                default:
                    break;
            }
        }

        private void HandleCalendarInteraction()
        {
            var lines = new Map0_DialogueLine[]
            {
                new Map0_DialogueLine("Hạ Ân", "Tờ lịch cũ dừng lại ở nhiều năm trước. Hồi 8 tuổi, mình cứ nghĩ năm 20 tuổi sẽ là một người lớn thật vững vàng...")
            };

            if (uiManager != null)
            {
                uiManager.ShowDialogue(lines);
            }
        }

        private void HandlePhotoInteraction()
        {
            var lines = new Map0_DialogueLine[]
            {
                new Map0_DialogueLine("Hạ Ân", "Bức ảnh ba mẹ chụp cùng mình ngày xưa. Dù hai ngôn ngữ, hai nếp sống khác nhau, nhưng ngôi nhà lúc nào cũng rộn rã tiếng cười và ngập tràn tình yêu thương.")
            };

            if (uiManager != null)
            {
                uiManager.ShowDialogue(lines);
            }
        }

        private void HandleKitchenDoorInteraction()
        {
            if (currentStage < PrologueStage.Stage4_CatGuidance)
            {
                var lines = new Map0_DialogueLine[]
                {
                    new Map0_DialogueLine("Hạ Ân", "Mình nên mở Chiếc Hộp Thời Gian trên bàn trước khi bước vào các căn phòng khác.")
                };

                if (uiManager != null)
                {
                    uiManager.ShowDialogue(lines);
                }
            }
            else
            {
                // In or after Stage 4, entering door triggers transition to Map 1
                StartSceneTransition();
            }
        }
        #endregion

        #region Stage 3: Mở Chiếc Hộp Thời Gian
        private void HandleTimeCapsuleInteraction()
        {
            if (currentStage == PrologueStage.Stage2_ExploreLivingRoom)
            {
                currentStage = PrologueStage.Stage3_OpenTimeCapsule;
                if (stageCoroutine != null) StopCoroutine(stageCoroutine);
                stageCoroutine = StartCoroutine(Stage3Routine());
            }
            else if (currentStage > PrologueStage.Stage3_OpenTimeCapsule)
            {
                var lines = new Map0_DialogueLine[]
                {
                    new Map0_DialogueLine("Hạ Ân", "Chiếc hộp đã mở, bên trong là cuốn nhật ký trắng kỳ lạ...")
                };

                if (uiManager != null)
                {
                    uiManager.ShowDialogue(lines);
                }
            }
        }

        private IEnumerator Stage3Routine()
        {
            // 1. Lock player
            if (playerController != null)
            {
                playerController.SetMovementLocked(true);
            }

            // 2. Play sound & Toast "Cuốn Nhật Ký Trắng"
            if (sfxSource != null && openBoxSfx != null)
            {
                sfxSource.PlayOneShot(openBoxSfx);
            }

            if (uiManager != null)
            {
                uiManager.ShowToast("Đã nhận: Cuốn Nhật Ký Trắng", 3.5f);
            }

            // Kích hoạt hiển thị vào Slot 0 của thanh Inventory (chỉ hiển thị duy nhất Sprite cuốn nhật ký, không hiện nhãn text)
            Sprite itemSprite = (diarySprite != null) ? diarySprite : whiteJournalIcon;

            if (slotImage == null && uiManager != null)
            {
                slotImage = uiManager.FirstSlotIcon;
            }

            if (slotImage != null)
            {
                slotImage.sprite = itemSprite;
                slotImage.gameObject.SetActive(true);
                slotImage.enabled = true;
                slotImage.color = Color.white;
                slotImage.preserveAspect = true;

                // Tắt hoàn toàn Text / TextMeshPro hiển thị tên vật phẩm ở ô slot đầu tiên
                var parentSlot = slotImage.transform.parent;
                if (parentSlot != null)
                {
                    var txtItem = parentSlot.Find("Txt_ItemName");
                    if (txtItem != null)
                    {
                        txtItem.gameObject.SetActive(false);
                    }
                    foreach (var tmp in parentSlot.GetComponentsInChildren<TMPro.TMP_Text>(true))
                    {
                        tmp.text = string.Empty;
                        tmp.gameObject.SetActive(false);
                    }
                    foreach (var txt in parentSlot.GetComponentsInChildren<UnityEngine.UI.Text>(true))
                    {
                        txt.text = string.Empty;
                        txt.gameObject.SetActive(false);
                    }
                }
            }

            // Tắt GameObject Text hiển thị nhãn tên vật phẩm
            if (slotLabel != null)
            {
                slotLabel.text = string.Empty;
                slotLabel.gameObject.SetActive(false);
            }

            if (uiManager != null)
            {
                uiManager.SetInventorySlot(0, itemSprite, string.Empty);
            }

            yield return new WaitForSeconds(0.8f);

            // 3. Dialogue sequence 1
            var part1Lines = new Map0_DialogueLine[]
            {
                new Map0_DialogueLine("Hạ Ân", "Mở được rồi! Đây là cuốn nhật ký lưu giữ kỷ niệm của cả nhà mà mình đã cất giấu cùng ba mẹ 12 năm trước..."),
                new Map0_DialogueLine("Hạ Ân", "Khoan đã... Tại sao tất cả các trang giấy đều trắng phau thế này?!"),
                new Map0_DialogueLine("Hạ Ân", "Không có lấy một dòng chữ. Cứ như thể những ký ức gia đình suốt thời thơ ấu chưa từng tồn tại vậy...")
            };

            bool part1Done = false;
            if (uiManager != null)
            {
                uiManager.ShowDialogue(part1Lines, () => part1Done = true);
            }
            else
            {
                part1Done = true;
            }

            while (!part1Done) yield return null;

            // 4. Event effects:
            // Stop/fade BGM
            if (bgmSource != null && bgmSource.isPlaying)
            {
                StartCoroutine(FadeOutBgmRoutine(1.5f));
            }

            // Trigger lighting flicker and purple transition with magical particles
            if (lightingController != null)
            {
                lightingController.TriggerCapsuleEventSequence();
            }

            yield return new WaitForSeconds(1.8f);

            // 5. Monologue continuation
            var part2Lines = new Map0_DialogueLine[]
            {
                new Map0_DialogueLine("Hạ Ân", "Chuyện gì đang xảy ra thế này? Hoàng hôn đang buông xuống, và không gian quanh mình như đang dần khép lại...")
            };

            bool part2Done = false;
            if (uiManager != null)
            {
                uiManager.ShowDialogue(part2Lines, () => part2Done = true);
            }
            else
            {
                part2Done = true;
            }

            while (!part2Done) yield return null;

            // 6. Automatically trigger Stage 4
            TriggerStage4();
        }

        private IEnumerator FadeOutBgmRoutine(float duration)
        {
            if (bgmSource == null) yield break;
            float startVol = bgmSource.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
                yield return null;
            }
            bgmSource.Stop();
            bgmSource.volume = startVol;
        }
        #endregion

        #region Stage 4: Lời dẫn của Mèo Vàng & Chuyển sang Map 1
        private void TriggerStage4()
        {
            currentStage = PrologueStage.Stage4_CatGuidance;

            // 1. Play "Meo~" sound & Toast
            if (sfxSource != null && catMeowSfx != null)
            {
                sfxSource.PlayOneShot(catMeowSfx);
            }

            if (uiManager != null)
            {
                uiManager.ShowToast("Meo~", 2.5f);
                uiManager.SetObjective("Mục tiêu: Trò chuyện với Mèo Vàng");
                uiManager.SetQuestHint("Chú Mèo Vàng kỳ lạ đã xuất hiện ở lối vào Gian Bếp...");
            }

            // 2. Activate YellowCat NPC
            if (yellowCatNpc != null)
            {
                yellowCatNpc.SetActive(true);
            }

            // 3. Unlock movement so player can walk up to cat
            if (playerController != null)
            {
                playerController.SetMovementLocked(false);
            }
        }

        private void HandleYellowCatInteraction()
        {
            if (currentStage != PrologueStage.Stage4_CatGuidance) return;

            if (playerController != null)
            {
                playerController.SetMovementLocked(true);
            }

            var catLines = new Map0_DialogueLine[]
            {
                new Map0_DialogueLine("Mèo Vàng", "Meo~ Ký ức không hề biến mất đâu Hạ Ân. Chúng chỉ đang tự đóng lại vì lòng cô còn đầy những khoảng cách và hoài nghi."),
                new Map0_DialogueLine("Hạ Ân", "Cậu... cậu hiểu mình nói gì sao? Làm thế nào mình mới tìm lại được những trang nhật ký của gia đình?"),
                new Map0_DialogueLine("Mèo Vàng", "Trước khi bóng hoàng hôn tắt hẳn, hãy bước vào từng không gian cũ và tự tay ghép lại những mảnh ký ức bị lãng quên."),
                new Map0_DialogueLine("Mèo Vàng", "Hãy bắt đầu từ nơi giữ ngọn lửa ấm nhất của ngôi nhà. Đi theo ta vào gian bếp cũ nào, hương vị quen thuộc vẫn đang đợi cô đấy.")
            };

            if (uiManager != null)
            {
                uiManager.ShowDialogue(catLines, OnCatDialogueComplete);
            }
            else
            {
                OnCatDialogueComplete();
            }
        }

        private void OnCatDialogueComplete()
        {
            StartCoroutine(FinishPrologueRoutine());
        }

        private IEnumerator FinishPrologueRoutine()
        {
            currentStage = PrologueStage.Completed;

            if (playerController != null)
            {
                playerController.SetMovementLocked(true);
            }

            // Cat moves slightly toward kitchen door, then fades out
            if (yellowCatNpc != null)
            {
                Vector3 startPos = yellowCatNpc.transform.position;
                Vector3 targetPos = (catWalkTarget != null) ? catWalkTarget.position : (startPos + new Vector3(0f, 0.9f, 0f));
                float walkDuration = 1.0f;
                float elapsed = 0f;

                while (elapsed < walkDuration)
                {
                    elapsed += Time.deltaTime;
                    yellowCatNpc.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / walkDuration);
                    yield return null;
                }

                // Fade cat out
                var sr = yellowCatNpc.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    float fadeCatElapsed = 0f;
                    float fadeCatDuration = 0.5f;
                    Color initColor = sr.color;
                    while (fadeCatElapsed < fadeCatDuration)
                    {
                        fadeCatElapsed += Time.deltaTime;
                        sr.color = new Color(initColor.r, initColor.g, initColor.b, Mathf.Lerp(1f, 0f, fadeCatElapsed / fadeCatDuration));
                        yield return null;
                    }
                }

                yellowCatNpc.SetActive(false);
            }

            // Screen fade out to black (1.5 seconds)
            yield return new WaitForSeconds(0.2f);
            StartSceneTransition();
        }

        private void StartSceneTransition()
        {
            if (uiManager != null)
            {
                uiManager.FadeToBlack(1.5f, OnFadedOutLoadScene);
            }
            else
            {
                OnFadedOutLoadScene();
            }
        }

        private void OnFadedOutLoadScene()
        {
            Debug.Log($"[Map0_PrologueManager] Transitioning to scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        #endregion
    }
}
