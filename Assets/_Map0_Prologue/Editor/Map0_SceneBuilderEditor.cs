using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Game.Map0.Editor
{
    public static class Map0_SceneBuilderEditor
    {
        private const string SceneDirectory = "Assets/_Map0_Prologue/Scenes";
        private const string ScenePath = "Assets/_Map0_Prologue/Scenes/Map0_Prologue.unity";

        [MenuItem("Tools/Map 0/Build Complete Map 0 Prologue Scene")]
        public static void BuildMap0Scene()
        {
            Debug.Log("[Map0_SceneBuilder] Starting Map 0 Scene Generation...");

            if (!Directory.Exists(SceneDirectory))
            {
                Directory.CreateDirectory(SceneDirectory);
                AssetDatabase.Refresh();
            }

            // 1. Create a fresh empty scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Safe read-only asset resolution
            Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Square.png");
            Sprite capsuleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Capsule.png");
            Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.asset");
            if (defaultFont == null) defaultFont = TMP_Settings.defaultFontAsset;

            // Load Map_0 background
            Sprite map0Bg = LoadFirstSprite("Assets/Sprites/Level 0/Map_0.png");
            if (map0Bg == null) map0Bg = squareSprite;

            // Characters & Animators
            Sprite playerSprite = LoadSprite("Assets/Sprites/HaEun_1.png");
            if (playerSprite == null) playerSprite = capsuleSprite != null ? capsuleSprite : squareSprite;

            Sprite catSprite = LoadSprite("Assets/Sprites/Meo_1.png");
            if (catSprite == null) catSprite = squareSprite;

            RuntimeAnimatorController playerAnimCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Visual_Sprite.controller");
            RuntimeAnimatorController catAnimCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/NPC_Cat.controller");

            // UI Sprites
            Sprite quizPanelSprite = LoadSprite("Assets/UI/Quiz.png");
            Sprite buttonSprite = LoadSprite("Assets/UI/Button.png");
            Sprite inventorySprite = LoadSprite("Assets/Sprites/Inventory.png");
            Sprite journalIcon = LoadSprite("Assets/_Map0_Prologue/Sprites/Icon_WhiteJournal.png");

            // Diary Sprite for Inventory (prefer Diary_3 from Assets/Sprites/Level 0/Diary.png)
            Sprite diarySprite = null;
            var diaryAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Level 0/Diary.png");
            if (diaryAssets != null && diaryAssets.Length > 0)
            {
                foreach (var a in diaryAssets)
                {
                    if (a is Sprite s && s.name == "Diary_3")
                    {
                        diarySprite = s;
                        break;
                    }
                }
                if (diarySprite == null)
                {
                    foreach (var a in diaryAssets)
                    {
                        if (a is Sprite s)
                        {
                            diarySprite = s;
                            break;
                        }
                    }
                }
            }
            if (diarySprite == null) diarySprite = journalIcon;
            if (diarySprite == null) diarySprite = squareSprite;

            // Audio Clips
            AudioClip bgmAudio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/BGM_MainMenu.mp3");
            AudioClip birdsSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/freesound_community-birds-in-the-morning-24147.mp3");

            // 3. Hierarchy 5 Root Groups
            GameObject rootManagers = new GameObject("--- MANAGERS ---");
            GameObject rootCameras = new GameObject("--- CAMERAS ---");
            GameObject rootEnvironment = new GameObject("--- ENVIRONMENT ---");
            GameObject rootGameplay = new GameObject("--- GAMEPLAY OBJECTS ---");
            GameObject rootCanvas = new GameObject("--- UI CANVAS ---");

            // -------------------------------------------------------------
            // GROUP 1: MANAGERS
            // -------------------------------------------------------------
            var prologueMgr = rootManagers.AddComponent<Map0_PrologueManager>();
            var uiMgr = rootManagers.AddComponent<Map0_UIManager>();
            var lightingCtrl = rootManagers.AddComponent<Map0_LightingController>();

            // EventSystem
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.transform.SetParent(rootManagers.transform, false);
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            // Audio BGM
            GameObject bgmGo = new GameObject("BGM_Source");
            bgmGo.transform.SetParent(rootManagers.transform, false);
            var bgmSource = bgmGo.AddComponent<AudioSource>();
            bgmSource.clip = bgmAudio;
            bgmSource.loop = true;
            bgmSource.playOnAwake = true;
            bgmSource.volume = 0.5f;
            if (bgmAudio != null) bgmSource.Play();

            // Audio SFX
            GameObject sfxGo = new GameObject("SFX_Source");
            sfxGo.transform.SetParent(rootManagers.transform, false);
            var sfxSource = sfxGo.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            // -------------------------------------------------------------
            // GROUP 3: ENVIRONMENT
            // -------------------------------------------------------------
            // 1. Room Background
            GameObject bgGo = new GameObject("Map0_Background");
            bgGo.transform.SetParent(rootEnvironment.transform, false);
            bgGo.transform.position = Vector3.zero;
            var srBg = bgGo.AddComponent<SpriteRenderer>();
            srBg.sprite = map0Bg;
            srBg.sortingOrder = -10;

            // 2. Camera Bounds (Framing cozy living room)
            GameObject camBoundsGo = new GameObject("Room_Camera_Bounds");
            camBoundsGo.transform.SetParent(rootEnvironment.transform, false);
            camBoundsGo.transform.position = Vector3.zero;
            var camBoundsCol = camBoundsGo.AddComponent<BoxCollider2D>();
            camBoundsCol.isTrigger = true;
            camBoundsCol.size = new Vector2(6.0f, 5.0f);
            camBoundsCol.offset = new Vector2(0f, 0.45f);

            // 3. Walls Container & Obstacles
            GameObject wallsGo = new GameObject("Walls_Container");
            wallsGo.transform.SetParent(rootEnvironment.transform, false);

            // Outer Perimeter Walls
            // Top walls flanking kitchen entrance (entrance at x: -0.1 to 0.8)
            CreateWall(wallsGo.transform, "Wall_Top_Left", new Vector3(-1.85f, 2.45f, 0f), new Vector2(3.5f, 0.6f));
            CreateWall(wallsGo.transform, "Wall_Top_Right", new Vector3(2.05f, 2.45f, 0f), new Vector2(2.5f, 0.6f));

            // Northern corridor walls leading up
            CreateWall(wallsGo.transform, "Hallway_Left", new Vector3(-0.25f, 3.2f, 0f), new Vector2(0.4f, 1.5f));
            CreateWall(wallsGo.transform, "Hallway_Right", new Vector3(0.95f, 3.2f, 0f), new Vector2(0.4f, 1.5f));
            CreateWall(wallsGo.transform, "Hallway_Top_Boundary", new Vector3(0.35f, 3.95f, 0f), new Vector2(1.6f, 0.4f));

            // Bottom walls flanking south entrance door mat (rug at x: 0.59, y: -1.34)
            CreateWall(wallsGo.transform, "Wall_Bottom_Left", new Vector3(-1.75f, -1.95f, 0f), new Vector2(3.7f, 0.6f));
            CreateWall(wallsGo.transform, "Wall_Bottom_Right", new Vector3(2.35f, -1.95f, 0f), new Vector2(2.7f, 0.6f));
            CreateWall(wallsGo.transform, "Door_Bottom_Boundary", new Vector3(0.59f, -2.15f, 0f), new Vector2(1.8f, 0.4f));

            // Left & Right perimeter walls
            CreateWall(wallsGo.transform, "Wall_Left", new Vector3(-3.25f, 0.4f, 0f), new Vector2(0.6f, 5.2f));
            CreateWall(wallsGo.transform, "Wall_Right", new Vector3(3.25f, 0.4f, 0f), new Vector2(0.6f, 5.2f));

            // Interior Furniture Obstacles
            CreateObstacle(wallsGo.transform, "Obstacle_LeftDeskCabinet", new Vector3(-1.85f, -0.95f, 0f), new Vector2(2.1f, 1.1f));
            CreateObstacle(wallsGo.transform, "Obstacle_LeftTVCabinet", new Vector3(-1.5f, 1.65f, 0f), new Vector2(1.9f, 0.9f));
            CreateObstacle(wallsGo.transform, "Obstacle_TeaTable", new Vector3(-1.37f, 0.65f, 0f), new Vector2(1.5f, 0.9f));
            CreateObstacle(wallsGo.transform, "Obstacle_RightConsoleTable", new Vector3(1.75f, 1.55f, 0f), new Vector2(2.1f, 0.9f));
            CreateObstacle(wallsGo.transform, "Obstacle_RightSofaLeft", new Vector3(1.05f, 0.45f, 0f), new Vector2(0.6f, 1.2f));
            CreateObstacle(wallsGo.transform, "Obstacle_RightSofaRight", new Vector3(2.45f, 0.45f, 0f), new Vector2(0.6f, 1.2f));
            CreateObstacle(wallsGo.transform, "Obstacle_RightBoxes", new Vector3(2.4f, -1.05f, 0f), new Vector2(1.1f, 1.1f));

            // 4. Lighting Overlay (Warm sunset orange at start)
            GameObject lightingGo = new GameObject("Lighting_Overlay");
            lightingGo.transform.SetParent(rootEnvironment.transform, false);
            lightingGo.transform.localScale = new Vector3(22f, 16f, 1f);
            var srLighting = lightingGo.AddComponent<SpriteRenderer>();
            srLighting.sprite = squareSprite;
            srLighting.sortingOrder = 25;
            srLighting.color = new Color(1.0f, 0.62f, 0.25f, 0.28f);

            // 5. Room Magical Dust Particles (Activated when opening capsule)
            GameObject dustGo = new GameObject("Room_Sparkle_Particles");
            dustGo.transform.SetParent(rootEnvironment.transform, false);
            dustGo.transform.position = new Vector3(0f, 0.5f, 0f);
            var dustPs = CreateSparkleParticleSystem(dustGo, new Color(0.95f, 0.75f, 1.0f, 0.85f), 18f, 3.5f, new Vector3(7.5f, 5.5f, 1f));

            // -------------------------------------------------------------
            // GROUP 2: CAMERAS
            // -------------------------------------------------------------
            GameObject camGo = new GameObject("Main Camera");
            camGo.transform.SetParent(rootCameras.transform, false);
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0.45f, -10f);

            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 2.65f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.05f, 0.08f, 1f);

            camGo.AddComponent<AudioListener>();

            var camCtrl = camGo.AddComponent<Map0_CameraController>();
            SetSerializedObjectReference(camCtrl, "cameraBounds", camBoundsCol);

            // -------------------------------------------------------------
            // GROUP 4: GAMEPLAY OBJECTS & PLAYER
            // -------------------------------------------------------------
            // 1. Player (Hạ Ân) at South Red Welcome Rug (0.59f, -1.35f)
            GameObject playerGo = new GameObject("Player");
            playerGo.transform.SetParent(rootGameplay.transform, false);
            playerGo.tag = "Player";
            playerGo.transform.position = new Vector3(0.59f, -1.35f, 0f);
            playerGo.transform.localScale = new Vector3(0.65f, 0.65f, 1f);

            var playerRb = playerGo.AddComponent<Rigidbody2D>();
            playerRb.gravityScale = 0f;
            playerRb.freezeRotation = true;
            playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var playerCol = playerGo.AddComponent<BoxCollider2D>();
            playerCol.isTrigger = false;
            playerCol.size = new Vector2(0.7f, 1.1f);
            playerCol.offset = new Vector2(0f, -0.15f);

            GameObject playerVisualGo = new GameObject("Visual_Sprite");
            playerVisualGo.transform.SetParent(playerGo.transform, false);
            var playerSr = playerVisualGo.AddComponent<SpriteRenderer>();
            playerSr.sprite = playerSprite;
            playerSr.sortingOrder = 5;

            if (playerAnimCtrl != null)
            {
                var pAnim = playerVisualGo.AddComponent<Animator>();
                pAnim.runtimeAnimatorController = playerAnimCtrl;
            }

            var playerCtrl = playerGo.AddComponent<Map0_PlayerController>();
            SetSerializedObjectReference(camCtrl, "target", playerGo.transform);

            // 2. WallCalendar_Zone (Lịch treo tường: -0.65, 2.3)
            GameObject calGo = CreateInteractableRoot(rootGameplay.transform, "WallCalendar_Zone",
                new Vector3(-0.65f, 2.3f, 0f), "Lịch Treo Tường", "[E] Xem Lịch", defaultFont,
                new Color(1f, 0.75f, 0.3f, 1f), new Vector2(1.2f, 1.2f));
            var calZone = calGo.AddComponent<Map0_InteractZone>();
            calZone.SetupZone(Map0_ZoneType.WallCalendar, "Lịch Treo Tường", "[E] Xem Lịch");

            // 3. FamilyPhoto_Zone (Ảnh gia đình tại tủ gỗ: -0.72, -0.6)
            GameObject photoGo = CreateInteractableRoot(rootGameplay.transform, "FamilyPhoto_Zone",
                new Vector3(-0.72f, -0.6f, 0f), "Ảnh Gia Đình", "[E] Ngắm Ảnh Gia Đình", defaultFont,
                new Color(0.95f, 0.85f, 0.4f, 1f), new Vector2(1.2f, 1.2f));
            var photoZone = photoGo.AddComponent<Map0_InteractZone>();
            photoZone.SetupZone(Map0_ZoneType.FamilyPhoto, "Ảnh Gia Đình", "[E] Ngắm Ảnh Gia Đình");

            // 4. TimeCapsule_Zone (Hộp Thời Gian trên bàn giữa phòng: 1.68, 0.45)
            GameObject capsuleGo = CreateInteractableRoot(rootGameplay.transform, "TimeCapsule_Zone",
                new Vector3(1.68f, 0.45f, 0f), "Chiếc Hộp Thời Gian", "[E] Mở Hộp Thời Gian", defaultFont,
                new Color(1f, 0.85f, 0.2f, 1f), new Vector2(1.5f, 1.4f));
            var capsuleZone = capsuleGo.AddComponent<Map0_InteractZone>();
            capsuleZone.SetupZone(Map0_ZoneType.TimeCapsule, "Chiếc Hộp Thời Gian", "[E] Mở Hộp Thời Gian");

            // Glow particles on Time Capsule to attract player attention
            GameObject capsuleGlowGo = new GameObject("Capsule_Glow_Particles");
            capsuleGlowGo.transform.SetParent(capsuleGo.transform, false);
            capsuleGlowGo.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            CreateGlowParticleSystem(capsuleGlowGo, new Color(1.0f, 0.82f, 0.35f, 0.85f));

            // 5. KitchenDoor_Zone (Cửa Gian Bếp ở hành lang phía Bắc: 0.35, 2.8)
            GameObject kitchenDoorGo = CreateInteractableRoot(rootGameplay.transform, "KitchenDoor_Zone",
                new Vector3(0.35f, 2.8f, 0f), "Cửa Gian Bếp", "[E] Sang Gian Bếp", defaultFont,
                new Color(0.35f, 0.9f, 0.5f, 1f), new Vector2(1.6f, 1.4f));
            var kitchenDoorZone = kitchenDoorGo.AddComponent<Map0_InteractZone>();
            kitchenDoorZone.SetupZone(Map0_ZoneType.KitchenDoor, "Cửa Gian Bếp", "[E] Sang Gian Bếp");

            // 6. YellowCat_NPC (Mèo Vàng: 0.35, 1.8 - Inactive initially)
            GameObject catGo = CreateInteractableRoot(rootGameplay.transform, "YellowCat_NPC",
                new Vector3(0.35f, 1.8f, 0f), "Mèo Vàng", "[E] Trò Chuyện", defaultFont,
                new Color(1f, 0.72f, 0.25f, 1f), new Vector2(1.3f, 1.3f));
            catGo.transform.localScale = new Vector3(0.65f, 0.65f, 1f);
            var catZone = catGo.AddComponent<Map0_InteractZone>();
            catZone.SetupZone(Map0_ZoneType.YellowCat, "Mèo Vàng", "[E] Trò Chuyện");

            GameObject catVisual = new GameObject("Cat_Visual");
            catVisual.transform.SetParent(catGo.transform, false);
            var catSr = catVisual.AddComponent<SpriteRenderer>();
            catSr.sprite = catSprite;
            catSr.sortingOrder = 5;
            if (catAnimCtrl != null)
            {
                var cAnim = catVisual.AddComponent<Animator>();
                cAnim.runtimeAnimatorController = catAnimCtrl;
            }

            // Cat waypoint for moving towards kitchen door
            GameObject catWalkTargetGo = new GameObject("CatWalk_Target");
            catWalkTargetGo.transform.SetParent(rootGameplay.transform, false);
            catWalkTargetGo.transform.position = new Vector3(0.35f, 2.7f, 0f);

            // Initially deactivate YellowCat_NPC as requested
            catGo.SetActive(false);

            // -------------------------------------------------------------
            // GROUP 5: UI CANVAS
            // -------------------------------------------------------------
            var canvas = rootCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = rootCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            rootCanvas.AddComponent<GraphicRaycaster>();

            // 1. HUD_TopLeft_Objective
            GameObject objPanelGo = CreateUIPanel(rootCanvas.transform, "HUD_TopLeft_Objective",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(30f, -30f), new Vector2(430f, 65f),
                uiSprite, new Color(0.12f, 0.15f, 0.2f, 0.88f));

            GameObject objTxtGo = CreateUIText(objPanelGo.transform, "Txt_Objective",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero,
                "Mục tiêu: Mở Chiếc Hộp Thời Gian", 22, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, defaultFont);
            var txtObjective = objTxtGo.GetComponent<TextMeshProUGUI>();

            // 2. HUD_TopCenter_Quest
            GameObject questPanelGo = CreateUIPanel(rootCanvas.transform, "HUD_TopCenter_Quest",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -30f), new Vector2(850f, 65f),
                uiSprite, new Color(0.12f, 0.15f, 0.2f, 0.92f));

            GameObject questTxtGo = CreateUIText(questPanelGo.transform, "Txt_QuestHint",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero,
                "Hãy tiến lại chiếc bàn giữa phòng để mở Chiếc Hộp Thời Gian.", 21, FontStyles.Bold, new Color(1f, 0.85f, 0.35f, 1f), TextAlignmentOptions.Center, defaultFont);
            var txtQuestHint = questTxtGo.GetComponent<TextMeshProUGUI>();

            // 3. HUD_BottomCenter_Inv (5 Slots matching Map 1 & Map 2)
            GameObject invPanelGo = CreateUIPanel(rootCanvas.transform, "HUD_BottomCenter_Inv",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 35f), new Vector2(460f, 90f),
                inventorySprite != null ? inventorySprite : uiSprite,
                inventorySprite != null ? Color.white : new Color(0.12f, 0.15f, 0.2f, 0.88f));

            Image[] invSlotIcons = new Image[5];
            TextMeshProUGUI[] invSlotLabels = new TextMeshProUGUI[5];
            float startX = -170f;
            float slotSpacing = 85f;

            for (int i = 0; i < 5; i++)
            {
                GameObject slotGo = CreateUIPanel(invPanelGo.transform, $"Slot_0{i + 1}",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(startX + i * slotSpacing, 0f), new Vector2(68f, 68f),
                    uiSprite, new Color(0.25f, 0.3f, 0.38f, 0.92f));

                GameObject iconGo = CreateUIPanel(slotGo.transform, "Icon_Item",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(50f, 50f),
                    squareSprite, Color.white);
                var iconImg = iconGo.GetComponent<Image>();
                iconImg.preserveAspect = true;
                invSlotIcons[i] = iconImg;
                iconGo.SetActive(false);

                GameObject labelGo = CreateUIText(slotGo.transform, "Txt_ItemName",
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(0f, 8f), new Vector2(66f, 18f),
                    "", 11, FontStyles.Normal, Color.white, TextAlignmentOptions.Center, defaultFont);
                invSlotLabels[i] = labelGo.GetComponent<TextMeshProUGUI>();
                labelGo.SetActive(false);
            }

            // 4. Controls_Mobile
            GameObject controlsGo = new GameObject("Controls_Mobile");
            controlsGo.transform.SetParent(rootCanvas.transform, false);
            var controlsRt = controlsGo.AddComponent<RectTransform>();
            controlsRt.anchorMin = Vector2.zero;
            controlsRt.anchorMax = Vector2.one;
            controlsRt.offsetMin = Vector2.zero;
            controlsRt.offsetMax = Vector2.zero;

            // Virtual Joystick
            GameObject joystickBaseGo = CreateUIPanel(controlsGo.transform, "Virtual_Joystick",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(160f, 160f), new Vector2(170f, 170f),
                uiSprite, new Color(1f, 1f, 1f, 0.25f));

            GameObject joystickKnobGo = CreateUIPanel(joystickBaseGo.transform, "Joystick_Knob",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(75f, 75f),
                uiSprite, new Color(1f, 1f, 1f, 0.75f));

            var vj = joystickBaseGo.AddComponent<Map0_VirtualJoystick>();
            SetSerializedObjectReference(vj, "joystickHandle", joystickKnobGo.GetComponent<RectTransform>());
            SetSerializedObjectReference(vj, "playerController", playerCtrl);

            // Btn_Interact (Mobile Hand Button)
            GameObject btnInteractGo = CreateUIButton(controlsGo.transform, "Btn_Interact",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(-160f, 160f), new Vector2(180f, 80f),
                buttonSprite != null ? buttonSprite : uiSprite,
                new Color(0.18f, 0.7f, 0.35f, 1f),
                "TƯƠNG TÁC [E]", 22, FontStyles.Bold, Color.white, defaultFont);

            var interactBtnScript = btnInteractGo.AddComponent<Map0_InteractButton>();
            SetSerializedObjectReference(interactBtnScript, "playerController", playerCtrl);
            SetSerializedObjectReference(interactBtnScript, "buttonImage", btnInteractGo.GetComponent<Image>());
            SetSerializedObjectReference(interactBtnScript, "buttonText", btnInteractGo.GetComponentInChildren<TextMeshProUGUI>());

            // 5. Toast Notification Panel
            GameObject toastPanelGo = CreateUIPanel(rootCanvas.transform, "Panel_Toast",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -120f), new Vector2(780f, 90f),
                uiSprite, new Color(0.08f, 0.1f, 0.15f, 0.95f));

            GameObject txtToastGo = CreateUIText(toastPanelGo.transform, "Txt_Toast",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                new Vector2(25f, 15f), new Vector2(-50f, -30f),
                "Thông báo trò chơi...", 20, FontStyles.Normal, Color.white, TextAlignmentOptions.Center, defaultFont);
            toastPanelGo.SetActive(false);

            // 6. Interaction Prompt [E]
            GameObject promptPanelGo = CreateUIPanel(rootCanvas.transform, "Prompt_Interact",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 140f), new Vector2(360f, 50f),
                uiSprite, new Color(0.1f, 0.12f, 0.18f, 0.85f));

            GameObject txtPromptGo = CreateUIText(promptPanelGo.transform, "Txt_Prompt",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero,
                "[E] Tương tác", 21, FontStyles.Bold, new Color(1f, 0.85f, 0.2f, 1f), TextAlignmentOptions.Center, defaultFont);
            promptPanelGo.SetActive(false);

            // 7. Panel_Dialogue (Clean Visual Novel style dialogue box with parchment frame)
            GameObject dialogueModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Dialogue",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero,
                uiSprite, new Color(0f, 0f, 0f, 0.45f));

            GameObject dialogueCardGo = CreateUIPanel(dialogueModalGo.transform, "Card_Dialogue",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 160f), new Vector2(1100f, 260f),
                quizPanelSprite != null ? quizPanelSprite : uiSprite,
                quizPanelSprite != null ? Color.white : new Color(0.12f, 0.14f, 0.18f, 0.98f));

            // Transparent full-card button to tap anywhere on mobile/PC
            GameObject btnNextGo = CreateUIButton(dialogueCardGo.transform, "Btn_NextDialogue",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero,
                squareSprite, new Color(1f, 1f, 1f, 0.001f),
                "", 14, FontStyles.Normal, Color.clear, defaultFont);

            // Speaker Tag / Name (Padded inside parchment, 46px down from top frame)
            GameObject txtSpeakerGo = CreateUIText(dialogueCardGo.transform, "Txt_SpeakerName",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(55f, -46f), new Vector2(350f, 34f),
                "Hạ Ân", 24, FontStyles.Bold, new Color32(165, 42, 24, 255), TextAlignmentOptions.TopLeft, defaultFont);

            // Dialogue Content Text (Positioned strictly below speaker name, with 55px left/right, 88px top, and 42px bottom padding)
            GameObject txtContentGo = CreateUITextMargins(dialogueCardGo.transform, "Txt_DialogueContent",
                55f, 55f, 88f, 42f,
                "Nội dung thoại...", 20, FontStyles.Normal, new Color32(45, 28, 16, 255), TextAlignmentOptions.TopLeft, defaultFont);
            var txtContentTmp = txtContentGo.GetComponent<TextMeshProUGUI>();
            txtContentTmp.textWrappingMode = TextWrappingModes.Normal;
            txtContentTmp.lineSpacing = 16f;

            // Continue Hint at bottom-right inside parchment
            GameObject txtHintGo = CreateUIText(dialogueCardGo.transform, "Txt_ContinueHint",
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-50f, 16f), new Vector2(400f, 22f),
                "Chạm vào màn hình để tiếp tục", 14, FontStyles.Italic, new Color32(120, 85, 55, 220), TextAlignmentOptions.Right, defaultFont);

            dialogueModalGo.SetActive(false);

            // 8. Fade Overlay (Topmost UI element)
            GameObject fadeGo = CreateUIPanel(rootCanvas.transform, "Fade_Overlay",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero,
                squareSprite, Color.black);
            var fadeImg = fadeGo.GetComponent<Image>();
            fadeImg.raycastTarget = false;
            Color fc = fadeImg.color;
            fc.a = 1f; // Starts fully black for smooth fade-in
            fadeImg.color = fc;

            // -------------------------------------------------------------
            // WIRE ALL SERIALIZED REFERENCES
            // -------------------------------------------------------------
            // Wire Map0_UIManager
            SetSerializedObjectReference(uiMgr, "fadeOverlay", fadeImg);
            SetSerializedObjectReference(uiMgr, "txtObjective", txtObjective);
            SetSerializedObjectReference(uiMgr, "txtQuestHint", txtQuestHint);
            SetSerializedObjectReference(uiMgr, "toastPanel", toastPanelGo);
            SetSerializedObjectReference(uiMgr, "txtToast", txtToastGo.GetComponent<TextMeshProUGUI>());
            SetSerializedObjectReference(uiMgr, "promptPanel", promptPanelGo);
            SetSerializedObjectReference(uiMgr, "txtPrompt", txtPromptGo.GetComponent<TextMeshProUGUI>());
            SetSerializedObjectReference(uiMgr, "inventoryPanel", invPanelGo);
            SetSerializedObjectReference(uiMgr, "dialoguePanel", dialogueModalGo);
            SetSerializedObjectReference(uiMgr, "txtSpeakerName", txtSpeakerGo.GetComponent<TextMeshProUGUI>());
            SetSerializedObjectReference(uiMgr, "txtDialogueContent", txtContentTmp);
            SetSerializedObjectReference(uiMgr, "btnDialogueNext", btnNextGo.GetComponent<Button>());
            SetSerializedObjectReference(uiMgr, "txtDialogueHint", txtHintGo.GetComponent<TextMeshProUGUI>());

            SerializedObject soUi = new SerializedObject(uiMgr);
            SerializedProperty spSlots = soUi.FindProperty("slotIcons");
            spSlots.arraySize = 5;
            for (int i = 0; i < 5; i++) spSlots.GetArrayElementAtIndex(i).objectReferenceValue = invSlotIcons[i];

            SerializedProperty spLabels = soUi.FindProperty("slotLabels");
            spLabels.arraySize = 5;
            for (int i = 0; i < 5; i++) spLabels.GetArrayElementAtIndex(i).objectReferenceValue = invSlotLabels[i];
            soUi.ApplyModifiedProperties();

            // Wire Map0_LightingController
            SetSerializedObjectReference(lightingCtrl, "lightingOverlayRenderer", srLighting);
            SetSerializedObjectReference(lightingCtrl, "magicDustParticles", dustPs);

            // Wire Map0_PrologueManager
            SetSerializedObjectReference(prologueMgr, "playerController", playerCtrl);
            SetSerializedObjectReference(prologueMgr, "uiManager", uiMgr);
            SetSerializedObjectReference(prologueMgr, "lightingController", lightingCtrl);
            SetSerializedObjectReference(prologueMgr, "bgmSource", bgmSource);
            SetSerializedObjectReference(prologueMgr, "sfxSource", sfxSource);
            SetSerializedObjectReference(prologueMgr, "openBoxSfx", birdsSfx);
            SetSerializedObjectReference(prologueMgr, "timeCapsuleZone", capsuleZone);
            SetSerializedObjectReference(prologueMgr, "kitchenDoorZone", kitchenDoorZone);
            SetSerializedObjectReference(prologueMgr, "yellowCatNpc", catGo);
            SetSerializedObjectReference(prologueMgr, "catWalkTarget", catWalkTargetGo.transform);
            SetSerializedObjectReference(prologueMgr, "whiteJournalIcon", diarySprite);
            SetSerializedObjectReference(prologueMgr, "diarySprite", diarySprite);
            SetSerializedObjectReference(prologueMgr, "slotImage", invSlotIcons[0]);
            SetSerializedObjectReference(prologueMgr, "slotLabel", invSlotLabels[0]);
            SetSerializedString(prologueMgr, "nextSceneName", "Level1_Prototype");

            // -------------------------------------------------------------
            // SAVE SCENE & UPDATE BUILD SETTINGS
            // -------------------------------------------------------------
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[Map0_SceneBuilder] Map 0 scene saved successfully to {ScenePath}");

            RegisterSceneInBuildSettings(ScenePath);
        }

        private static void RegisterSceneInBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (!scenes.Any(s => s.path == scenePath))
            {
                // Insert Map 0 right before Map 1 (Level1_Prototype) or at appropriate index
                int insertIndex = scenes.FindIndex(s => s.path.Contains("Level1_Prototype"));
                var newScene = new EditorBuildSettingsScene(scenePath, true);
                if (insertIndex >= 0)
                {
                    scenes.Insert(insertIndex, newScene);
                }
                else
                {
                    scenes.Add(newScene);
                }
                EditorBuildSettings.scenes = scenes.ToArray();
                Debug.Log($"[Map0_SceneBuilder] Registered {scenePath} in EditorBuildSettings");
            }
        }

        #region Helper Factory Methods
        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite LoadFirstSprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;

            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in all)
            {
                if (a is Sprite s) return s;
            }
            return null;
        }

        private static GameObject CreateWall(Transform parent, string name, Vector3 pos, Vector2 size)
        {
            GameObject wall = new GameObject(name);
            wall.transform.SetParent(parent, false);
            wall.transform.position = pos;
            var col = wall.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
            col.size = size;
            return wall;
        }

        private static GameObject CreateObstacle(Transform parent, string name, Vector3 pos, Vector2 size)
        {
            return CreateWall(parent, name, pos, size);
        }

        private static GameObject CreateInteractableRoot(Transform parent, string name, Vector3 pos,
            string label, string prompt, TMP_FontAsset font, Color themeColor, Vector2 colliderSize)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = pos;

            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = colliderSize;

            // Indicator floating above
            GameObject ind = new GameObject("Interact_Indicator");
            ind.transform.SetParent(root.transform, false);
            ind.transform.localPosition = new Vector3(0f, colliderSize.y * 0.5f + 0.35f, 0f);

            var tmp = ind.AddComponent<TextMeshPro>();
            tmp.text = "▼";
            tmp.fontSize = 4f;
            tmp.font = font;
            tmp.color = themeColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = 15;

            ind.SetActive(false);
            return root;
        }

        private static ParticleSystem CreateSparkleParticleSystem(GameObject go, Color color, float emissionRate, float startLifetime, Vector3 boxSize)
        {
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = true;
            main.startLifetime = startLifetime;
            main.startSpeed = 0.15f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 80;

            var emission = ps.emission;
            emission.rateOverTime = emissionRate;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = boxSize;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(Color.white, 0.5f), new GradientColorKey(color, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.9f, 0.3f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;

            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.sortingOrder = 30;

            return ps;
        }

        private static ParticleSystem CreateGlowParticleSystem(GameObject go, Color color)
        {
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = true;
            main.loop = true;
            main.startLifetime = 1.6f;
            main.startSpeed = 0.25f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 30;

            var emission = ps.emission;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.45f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(new Color(1f, 1f, 0.8f, 1f), 0.5f), new GradientColorKey(color, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.85f, 0.4f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;

            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.sortingOrder = 12;

            return ps;
        }

        private static GameObject CreateUIPanel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta, Sprite sprite, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.type = (sprite != null) ? Image.Type.Sliced : Image.Type.Simple;
            img.color = color;

            return go;
        }

        private static GameObject CreateUIButton(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta, Sprite sprite, Color color,
            string label, float fontSize, FontStyles style, Color textColor, TMP_FontAsset font)
        {
            GameObject btnGo = CreateUIPanel(parent, name, anchorMin, anchorMax, pivot, anchoredPos, sizeDelta, sprite, color);
            btnGo.AddComponent<Button>();

            if (!string.IsNullOrEmpty(label))
            {
                CreateUIText(btnGo.transform, "Txt_Label",
                    Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                    Vector2.zero, Vector2.zero,
                    label, fontSize, style, textColor, TextAlignmentOptions.Center, font);
            }

            return btnGo;
        }

        private static GameObject CreateUITextMargins(Transform parent, string name,
            float leftMargin, float rightMargin, float topMargin, float bottomMargin,
            string text, float fontSize, FontStyles style, Color color,
            TextAlignmentOptions alignment, TMP_FontAsset font)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(leftMargin, bottomMargin);
            rt.offsetMax = new Vector2(-rightMargin, -topMargin);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            if (font != null) tmp.font = font;

            return go;
        }

        private static GameObject CreateUIText(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta,
            string text, float fontSize, FontStyles style, Color color,
            TextAlignmentOptions alignment, TMP_FontAsset font)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            if (font != null) tmp.font = font;

            return go;
        }

        private static void SetSerializedObjectReference(UnityEngine.Object target, string propName, UnityEngine.Object value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty sp = so.FindProperty(propName);
            if (sp != null)
            {
                sp.objectReferenceValue = value;
                so.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning($"[Map0_SceneBuilder] Property {propName} not found on {target.GetType().Name}");
            }
        }

        private static void SetSerializedString(UnityEngine.Object target, string propName, string value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty sp = so.FindProperty(propName);
            if (sp != null)
            {
                sp.stringValue = value;
                so.ApplyModifiedProperties();
            }
        }
        #endregion
    }
}
