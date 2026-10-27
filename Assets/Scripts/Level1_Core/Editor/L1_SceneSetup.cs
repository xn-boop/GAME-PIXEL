using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class L1_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Level1_Prototype.unity";

    [MenuItem("Tools/Map 1/Rebuild Map 1 Scene")]
    public static void GenerateScene()
    {
        Debug.Log("[L1_SceneSetup] Starting Map 1 Scene Generation...");

        // 1. Create a fresh empty scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. Load sprites & font
        Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Square.png");
        Sprite capsuleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Capsule.png");
        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.asset");
        if (defaultFont == null) defaultFont = TMP_Settings.defaultFontAsset;

        Sprite map1Bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Map_1.png");
        Sprite plateSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Plate.png");
        Sprite iconThitKho = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Icon_ThitKho.png");
        Sprite iconKimchi = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Icon_Kimchi.png");
        Sprite iconKimbap = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Icon_Kimbap.png");

        RuntimeAnimatorController playerAnimCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Visual_Sprite.controller");

        // 3. Create the 5 Root Groups matching Map 2 architecture
        GameObject rootManagers = new GameObject("--- MANAGERS ---");
        GameObject rootCameras = new GameObject("--- CAMERAS ---");
        GameObject rootEnvironment = new GameObject("--- ENVIRONMENT ---");
        GameObject rootGameplay = new GameObject("--- GAMEPLAY OBJECTS ---");
        GameObject rootCanvas = new GameObject("--- UI CANVAS ---");

        // -------------------------------------------------------------
        // GROUP 1: MANAGERS
        // -------------------------------------------------------------
        var gm = rootManagers.AddComponent<L1_GameManager>();
        var qm = rootManagers.AddComponent<L1_QuizManager>();
        var im = rootManagers.AddComponent<L1_InventoryManager>();

        im.thitKhoIcon = iconThitKho;
        im.kimchiIcon = iconKimchi;
        im.kimbapIcon = iconKimbap;

        // EventSystem
        GameObject eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.transform.SetParent(rootManagers.transform, false);
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<InputSystemUIInputModule>();

        // Optional Audio Starter
        GameObject audioGo = new GameObject("AudioManager");
        audioGo.transform.SetParent(rootManagers.transform, false);
        audioGo.AddComponent<Level1MusicStarter>();

        // -------------------------------------------------------------
        // GROUP 3: ENVIRONMENT
        // -------------------------------------------------------------
        // Map_Background
        GameObject bgGo = new GameObject("Map_Background");
        bgGo.transform.SetParent(rootEnvironment.transform, false);
        bgGo.transform.position = Vector3.zero;
        bgGo.transform.localScale = Vector3.one;
        var bgSr = bgGo.AddComponent<SpriteRenderer>();
        bgSr.sprite = map1Bg != null ? map1Bg : squareSprite;
        bgSr.sortingOrder = -10;

        // Walls_Container (Pure collision blocks, no SpriteRenderer)
        GameObject wallsContainer = new GameObject("Walls_Container");
        wallsContainer.transform.SetParent(rootEnvironment.transform, false);

        // Outer bounds
        CreateWall(wallsContainer.transform, "Wall_Top", new Vector3(0f, 4.8f, 0f), new Vector2(20.5f, 0.8f));
        CreateWall(wallsContainer.transform, "Wall_Bottom", new Vector3(0f, -4.8f, 0f), new Vector2(20.5f, 0.8f));
        CreateWall(wallsContainer.transform, "Wall_Left", new Vector3(-9.6f, 0f, 0f), new Vector2(0.8f, 10.5f));
        CreateWall(wallsContainer.transform, "Wall_Right", new Vector3(9.6f, 0f, 0f), new Vector2(0.8f, 10.5f));

        // Partition Divider (Living Room & Kitchen)
        CreateWall(wallsContainer.transform, "Partition_Divider_Top", new Vector3(0.1f, 3.3f, 0f), new Vector2(0.6f, 2.5f));
        CreateWall(wallsContainer.transform, "Partition_Divider_Bottom", new Vector3(0.1f, -3.3f, 0f), new Vector2(0.6f, 2.5f));

        // Furniture obstacle colliders
        CreateObstacle(wallsContainer.transform, "Obstacle_Sofa", new Vector3(-7.8f, -1.0f, 0f), new Vector2(1.8f, 3.5f));
        CreateObstacle(wallsContainer.transform, "Obstacle_LivingShelves", new Vector3(-7.5f, 3.5f, 0f), new Vector2(2.5f, 1.2f));
        CreateObstacle(wallsContainer.transform, "Obstacle_KitchenStove", new Vector3(3.8f, 3.5f, 0f), new Vector2(5.5f, 1.4f));
        CreateObstacle(wallsContainer.transform, "Obstacle_KitchenFridge", new Vector3(8.0f, 3.5f, 0f), new Vector2(2.0f, 1.8f));
        CreateObstacle(wallsContainer.transform, "Obstacle_KitchenIsland", new Vector3(4.8f, -0.4f, 0f), new Vector2(3.6f, 1.4f));

        // -------------------------------------------------------------
        // GROUP 4: GAMEPLAY OBJECTS (Empty blocks ready for manual art tuning)
        // -------------------------------------------------------------
        // 1. Player (Hạ Ân - keeps sprite and animator for movement)
        GameObject playerGo = CreateGameplayRoot(rootGameplay.transform, "Player", new Vector3(-2.2f, -1.0f, 0f),
            defaultFont, isPlayer: true, playerSprite: capsuleSprite);
        var playerCtrl = playerGo.AddComponent<L1_PlayerController>();

        Transform pVisual = playerGo.transform.Find("Visual_Sprite");
        if (pVisual != null && playerAnimCtrl != null)
        {
            var anim = pVisual.gameObject.AddComponent<Animator>();
            anim.runtimeAnimatorController = playerAnimCtrl;
        }

        // 2. NPC_Cat (Empty trigger block)
        GameObject catGo = CreateGameplayRoot(rootGameplay.transform, "NPC_Cat", new Vector3(-1.8f, 1.8f, 0f), defaultFont);
        var cat = catGo.AddComponent<L1_NPC_Cat>();
        WireInteractableFields(cat, catGo);

        // 3. Dining Table (Bàn ăn lớn giữa phòng khách)
        GameObject tableGo = CreateGameplayRoot(rootGameplay.transform, "Dining_Table", new Vector3(-4.5f, 0.6f, 0f), defaultFont);
        var tableCol = tableGo.GetComponent<BoxCollider2D>();
        if (tableCol != null) tableCol.size = new Vector2(3.6f, 2.2f);
        var tableCtrl = tableGo.AddComponent<L1_DiningTableController>();
        WireInteractableFields(tableCtrl, tableGo);

        // 3 Plates horizontally on table
        L1_Plate plateThitKho = CreatePlate(tableGo.transform, "Plate_ThitKho", new Vector3(-0.9f, 0f, 0f), plateSprite, iconThitKho, L1_ItemType.ThitKho);
        L1_Plate plateKimchi = CreatePlate(tableGo.transform, "Plate_Kimchi", new Vector3(0f, 0f, 0f), plateSprite, iconKimchi, L1_ItemType.Kimchi);
        L1_Plate plateKimbap = CreatePlate(tableGo.transform, "Plate_Kimbap", new Vector3(0.9f, 0f, 0f), plateSprite, iconKimbap, L1_ItemType.Kimbap);

        SetSerializedObjectReference(tableCtrl, "plateThitKho", plateThitKho);
        SetSerializedObjectReference(tableCtrl, "plateKimchi", plateKimchi);
        SetSerializedObjectReference(tableCtrl, "plateKimbap", plateKimbap);

        // 4. Main Food Items (Empty trigger blocks with bobbing arrow anchor)
        // Meat Stew (Thịt kho trứng)
        GameObject thitKhoGo = CreateGameplayRoot(rootGameplay.transform, "Interact_ThitKho", new Vector3(3.2f, 3.0f, 0f), defaultFont);
        var thitKhoPickup = thitKhoGo.AddComponent<L1_ItemPickup>();
        SetSerializedEnum(thitKhoPickup, "itemType", (int)L1_ItemType.ThitKho);
        AttachArrowIndicator(thitKhoGo);
        WireInteractableFields(thitKhoPickup, thitKhoGo);

        // Kimchi
        GameObject kimchiGo = CreateGameplayRoot(rootGameplay.transform, "Interact_Kimchi", new Vector3(7.8f, 3.0f, 0f), defaultFont);
        var kimchiPickup = kimchiGo.AddComponent<L1_ItemPickup>();
        SetSerializedEnum(kimchiPickup, "itemType", (int)L1_ItemType.Kimchi);
        AttachArrowIndicator(kimchiGo);
        WireInteractableFields(kimchiPickup, kimchiGo);

        // Kimbap
        GameObject kimbapGo = CreateGameplayRoot(rootGameplay.transform, "Interact_Kimbap", new Vector3(7.8f, -0.4f, 0f), defaultFont);
        var kimbapPickup = kimbapGo.AddComponent<L1_ItemPickup>();
        SetSerializedEnum(kimbapPickup, "itemType", (int)L1_ItemType.Kimbap);
        AttachArrowIndicator(kimbapGo);
        WireInteractableFields(kimbapPickup, kimbapGo);

        // 5. Cultural Items (Empty trigger blocks)
        // Flag VN
        GameObject flagVnGo = CreateGameplayRoot(rootGameplay.transform, "Interact_FlagVN", new Vector3(-5.5f, 3.6f, 0f), defaultFont);
        var cultVn = flagVnGo.AddComponent<L1_CulturalItem>();
        SetSerializedEnum(cultVn, "culturalType", (int)L1_CulturalType.FlagVN);
        WireInteractableFields(cultVn, flagVnGo);

        // Flag KR
        GameObject flagKrGo = CreateGameplayRoot(rootGameplay.transform, "Interact_FlagKR", new Vector3(-3.5f, 3.6f, 0f), defaultFont);
        var cultKr = flagKrGo.AddComponent<L1_CulturalItem>();
        SetSerializedEnum(cultKr, "culturalType", (int)L1_CulturalType.FlagKR);
        WireInteractableFields(cultKr, flagKrGo);

        // Bánh Mì
        GameObject banhMiGo = CreateGameplayRoot(rootGameplay.transform, "Interact_BanhMi", new Vector3(3.8f, -0.3f, 0f), defaultFont);
        var cultBm = banhMiGo.AddComponent<L1_CulturalItem>();
        SetSerializedEnum(cultBm, "culturalType", (int)L1_CulturalType.BanhMi);
        WireInteractableFields(cultBm, banhMiGo);

        // Tteokbokki
        GameObject tbkGo = CreateGameplayRoot(rootGameplay.transform, "Interact_Tteokbokki", new Vector3(5.8f, -0.3f, 0f), defaultFont);
        var cultTbk = tbkGo.AddComponent<L1_CulturalItem>();
        SetSerializedEnum(cultTbk, "culturalType", (int)L1_CulturalType.Tteokbokki);
        WireInteractableFields(cultTbk, tbkGo);

        // -------------------------------------------------------------
        // GROUP 2: CAMERAS
        // -------------------------------------------------------------
        GameObject camGo = new GameObject("Main Camera");
        camGo.transform.SetParent(rootCameras.transform, false);
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(-2.2f, -1.0f, -10f);

        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = HexColor("#1E1E24");

        camGo.AddComponent<AudioListener>();

        var camFollow = camGo.AddComponent<L1_CameraFollow>();
        SetSerializedObjectReference(camFollow, "target", playerGo.transform);

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
            new Vector2(30f, -30f), new Vector2(380f, 60f),
            uiSprite, new Color(0.12f, 0.15f, 0.2f, 0.85f));

        GameObject objTxtGo = CreateUIText(objPanelGo.transform, "Txt_Objective",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero,
            "MÓN ĂN: 0/3 | BÀN ĂN: 0/3", 24, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, defaultFont);
        var txtObjective = objTxtGo.GetComponent<TextMeshProUGUI>();

        // 2. HUD_TopCenter_Quest
        GameObject questPanelGo = CreateUIPanel(rootCanvas.transform, "HUD_TopCenter_Quest",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -30f), new Vector2(800f, 60f),
            uiSprite, new Color(0.12f, 0.15f, 0.2f, 0.9f));

        GameObject questTxtGo = CreateUIText(questPanelGo.transform, "Txt_QuestHint",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero,
            "Hãy tìm Chú Mèo để nhận gợi ý chuẩn bị bữa cơm gia đình.", 22, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);
        var txtQuestHint = questTxtGo.GetComponent<TextMeshProUGUI>();

        // 3. HUD_BottomCenter_Inventory
        GameObject invPanelGo = CreateUIPanel(rootCanvas.transform, "HUD_BottomCenter_Inventory",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 35f), new Vector2(440f, 85f),
            uiSprite, new Color(0.12f, 0.15f, 0.2f, 0.85f));

        Image[] invSlotIcons = new Image[5];
        float startX = -160f;
        float slotSpacing = 80f;

        for (int i = 0; i < 5; i++)
        {
            GameObject slotGo = CreateUIPanel(invPanelGo.transform, $"Slot_0{i + 1}",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(startX + i * slotSpacing, 0f), new Vector2(64f, 64f),
                uiSprite, new Color(0.25f, 0.3f, 0.38f, 0.9f));

            GameObject iconGo = CreateUIPanel(slotGo.transform, "Icon_Item",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(50f, 50f),
                squareSprite, Color.white);

            invSlotIcons[i] = iconGo.GetComponent<Image>();
            iconGo.SetActive(false);
        }

        // 4. Controls_Mobile
        GameObject controlsGo = new GameObject("Controls_Mobile");
        controlsGo.transform.SetParent(rootCanvas.transform, false);
        var controlsRt = controlsGo.AddComponent<RectTransform>();
        controlsRt.anchorMin = Vector2.zero;
        controlsRt.anchorMax = Vector2.one;
        controlsRt.offsetMin = Vector2.zero;
        controlsRt.offsetMax = Vector2.zero;

        // Virtual_Joystick
        GameObject joystickBaseGo = CreateUIPanel(controlsGo.transform, "Virtual_Joystick",
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(160f, 160f), new Vector2(170f, 170f),
            uiSprite, new Color(1f, 1f, 1f, 0.25f));

        GameObject joystickKnobGo = CreateUIPanel(joystickBaseGo.transform, "Joystick_Knob",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(75f, 75f),
            uiSprite, new Color(1f, 1f, 1f, 0.75f));

        var vj = joystickBaseGo.AddComponent<L1_VirtualJoystick>();
        SetSerializedObjectReference(vj, "joystickHandle", joystickKnobGo.GetComponent<RectTransform>());
        SetSerializedObjectReference(vj, "playerController", playerCtrl);

        // Btn_Interact (Text child is inactive like Map 2)
        GameObject btnInteractGo = CreateUIButton(controlsGo.transform, "Btn_Interact",
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(-160f, 160f), new Vector2(180f, 80f),
            uiSprite, new Color(0.18f, 0.7f, 0.35f, 1f),
            "", 22, FontStyles.Bold, Color.white, defaultFont);

        var btnTxtGo = btnInteractGo.transform.Find("Text")?.gameObject;
        if (btnTxtGo != null) btnTxtGo.SetActive(false);

        var interactBtnScript = btnInteractGo.AddComponent<L1_InteractButton>();
        SetSerializedObjectReference(interactBtnScript, "playerController", playerCtrl);
        SetSerializedObjectReference(interactBtnScript, "buttonImage", btnInteractGo.GetComponent<Image>());
        SetSerializedObjectReference(interactBtnScript, "buttonText", btnInteractGo.GetComponentInChildren<TextMeshProUGUI>());

        // 5. Panel_Quiz_Dialog
        GameObject quizModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Quiz_Dialog",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero,
            uiSprite, new Color(0f, 0f, 0f, 0.75f));

        GameObject quizCardGo = CreateUIPanel(quizModalGo.transform, "Card_Content",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(860f, 620f),
            uiSprite, HexColor("#1E232A"));

        // Close Button
        GameObject btnCloseGo = CreateUIButton(quizCardGo.transform, "Btn_Close",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(-35f, -35f), new Vector2(44f, 44f),
            uiSprite, HexColor("#D32F2F"),
            "✕", 24, FontStyles.Bold, Color.white, defaultFont);

        // Title
        GameObject txtTitleGo = CreateUIText(quizCardGo.transform, "Txt_Title",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -45f), new Vector2(760f, 50f),
            "CÂU HỎI", 28, FontStyles.Bold, HexColor("#FFCA28"), TextAlignmentOptions.Center, defaultFont);

        // Question
        GameObject txtQuestContentGo = CreateUIText(quizCardGo.transform, "Txt_Question",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -145f), new Vector2(780f, 130f),
            "Nội dung câu hỏi...", 21, FontStyles.Normal, Color.white, TextAlignmentOptions.TopLeft, defaultFont);
        var txtQuestionTmp = txtQuestContentGo.GetComponent<TextMeshProUGUI>();
        txtQuestionTmp.textWrappingMode = TextWrappingModes.Normal;

        // 4 Answer Buttons
        Button[] ansBtns = new Button[4];
        TextMeshProUGUI[] ansTexts = new TextMeshProUGUI[4];
        string[] btnNames = { "Btn_Answer_A", "Btn_Answer_B", "Btn_Answer_C", "Btn_Answer_D" };
        float startBtnY = -250f;
        float btnStepY = 62f;

        for (int i = 0; i < 4; i++)
        {
            GameObject aBtnGo = CreateUIButton(quizCardGo.transform, btnNames[i],
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, startBtnY - i * btnStepY), new Vector2(780f, 54f),
                uiSprite, HexColor("#2C3642"),
                $"Phương án {i + 1}", 18, FontStyles.Normal, Color.white, defaultFont);

            ansBtns[i] = aBtnGo.GetComponent<Button>();
            ansTexts[i] = aBtnGo.GetComponentInChildren<TextMeshProUGUI>();
            ansTexts[i].alignment = TextAlignmentOptions.MidlineLeft;
            ansTexts[i].margin = new Vector4(20f, 0f, 20f, 0f);
            ansTexts[i].textWrappingMode = TextWrappingModes.Normal;
        }

        // Feedback Text
        GameObject txtFeedGo = CreateUIText(quizCardGo.transform, "Txt_Feedback",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 35f), new Vector2(780f, 40f),
            "", 22, FontStyles.Bold, HexColor("#4CAF50"), TextAlignmentOptions.Center, defaultFont);

        quizModalGo.SetActive(false);

        // 6. Panel_Victory
        GameObject victoryModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Victory",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero,
            uiSprite, new Color(0f, 0f, 0f, 0.85f));

        GameObject victoryCardGo = CreateUIPanel(victoryModalGo.transform, "Card_Victory",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(780f, 440f),
            uiSprite, HexColor("#182234"));

        CreateUIText(victoryCardGo.transform, "Txt_Victory_Header",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -50f), new Vector2(700f, 50f),
            "XUẤT SẮC!", 32, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);

        CreateUIText(victoryCardGo.transform, "Txt_Victory_Title",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -120f), new Vector2(700f, 70f),
            "HOÀN THÀNH MÀN 1: BỮA CƠM GIA ĐÌNH VIỆT - HÀN!", 26, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, defaultFont);

        CreateUIText(victoryCardGo.transform, "Txt_Victory_Desc",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -200f), new Vector2(680f, 60f),
            "Bữa cơm gia đình Việt - Hàn đã sẵn sàng! Bạn đã hoàn thành Màn 1!", 20, FontStyles.Normal, HexColor("#CFD8DC"), TextAlignmentOptions.Center, defaultFont);

        GameObject btnRestartGo = CreateUIButton(victoryCardGo.transform, "Btn_Restart",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(-120f, 65f), new Vector2(200f, 55f),
            uiSprite, HexColor("#4CAF50"),
            "Chơi lại", 22, FontStyles.Bold, Color.white, defaultFont);

        GameObject btnContinueGo = CreateUIButton(victoryCardGo.transform, "Btn_Continue",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(120f, 65f), new Vector2(200f, 55f),
            uiSprite, HexColor("#2196F3"),
            "Tiếp tục", 22, FontStyles.Bold, Color.white, defaultFont);

        victoryModalGo.SetActive(false);

        // -------------------------------------------------------------
        // SERIALIZE ALL REFERENCES
        // -------------------------------------------------------------
        // Wire L1_GameManager
        SerializedObject soGm = new SerializedObject(gm);
        soGm.FindProperty("txtObjective").objectReferenceValue = txtObjective;
        soGm.FindProperty("txtQuestHint").objectReferenceValue = txtQuestHint;
        soGm.FindProperty("panelVictory").objectReferenceValue = victoryModalGo;
        soGm.FindProperty("btnRestart").objectReferenceValue = btnRestartGo.GetComponent<Button>();
        soGm.FindProperty("btnContinue").objectReferenceValue = btnContinueGo.GetComponent<Button>();
        soGm.FindProperty("thitKhoPickup").objectReferenceValue = thitKhoPickup;
        soGm.FindProperty("kimchiPickup").objectReferenceValue = kimchiPickup;
        soGm.FindProperty("kimbapPickup").objectReferenceValue = kimbapPickup;
        soGm.ApplyModifiedPropertiesWithoutUndo();

        // Wire L1_QuizManager
        SerializedObject soQm = new SerializedObject(qm);
        soQm.FindProperty("quizPanel").objectReferenceValue = quizModalGo;
        soQm.FindProperty("txtTitle").objectReferenceValue = txtTitleGo.GetComponent<TextMeshProUGUI>();
        soQm.FindProperty("txtQuestion").objectReferenceValue = txtQuestionTmp;
        soQm.FindProperty("txtFeedback").objectReferenceValue = txtFeedGo.GetComponent<TextMeshProUGUI>();
        soQm.FindProperty("btnClose").objectReferenceValue = btnCloseGo.GetComponent<Button>();

        SerializedProperty spAnsBtns = soQm.FindProperty("answerButtons");
        spAnsBtns.arraySize = 4;
        for (int i = 0; i < 4; i++) spAnsBtns.GetArrayElementAtIndex(i).objectReferenceValue = ansBtns[i];

        SerializedProperty spAnsTxts = soQm.FindProperty("answerTexts");
        spAnsTxts.arraySize = 4;
        for (int i = 0; i < 4; i++) spAnsTxts.GetArrayElementAtIndex(i).objectReferenceValue = ansTexts[i];
        soQm.ApplyModifiedPropertiesWithoutUndo();

        // Wire L1_InventoryManager
        SerializedObject soIm = new SerializedObject(im);
        SerializedProperty spSlots = soIm.FindProperty("slotIcons");
        spSlots.arraySize = 5;
        for (int i = 0; i < 5; i++) spSlots.GetArrayElementAtIndex(i).objectReferenceValue = invSlotIcons[i];
        soIm.ApplyModifiedPropertiesWithoutUndo();

        // 4. Ensure all Text components use the Determination font
        foreach (var txt in UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (txt.gameObject.scene == scene && defaultFont != null) txt.font = defaultFont;
        }
        foreach (var txt in UnityEngine.Object.FindObjectsByType<TextMeshPro>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (txt.gameObject.scene == scene && defaultFont != null) txt.font = defaultFont;
        }

        // 5. Save Scene to Assets/Scenes/Level1_Prototype.unity
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log($"[L1_SceneSetup] Successfully saved new scene to: {ScenePath}");

        // 6. Ensure scene is in EditorBuildSettings
        AddSceneToBuildSettings(ScenePath);

        Debug.Log("[L1_SceneSetup] Map 1 Scene setup COMPLETE!");
    }

    private static L1_Plate CreatePlate(Transform parent, string name, Vector3 localPos, Sprite plateSprite, Sprite foodSprite, L1_ItemType type)
    {
        GameObject plateGo = new GameObject(name);
        plateGo.transform.SetParent(parent, false);
        plateGo.transform.localPosition = localPos;
        plateGo.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        var srPlate = plateGo.AddComponent<SpriteRenderer>();
        srPlate.sprite = plateSprite;
        srPlate.sortingOrder = 5;

        // Food sprite child
        GameObject foodGo = new GameObject("Food_Sprite");
        foodGo.transform.SetParent(plateGo.transform, false);
        foodGo.transform.localPosition = Vector3.zero;
        foodGo.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

        var srFood = foodGo.AddComponent<SpriteRenderer>();
        srFood.sprite = foodSprite;
        srFood.sortingOrder = 6;
        foodGo.SetActive(false);

        var plateScript = plateGo.AddComponent<L1_Plate>();
        SetSerializedEnum(plateScript, "requiredItemType", (int)type);
        SetSerializedObjectReference(plateScript, "plateRenderer", srPlate);
        SetSerializedObjectReference(plateScript, "foodRenderer", srFood);

        return plateScript;
    }

    private static void AttachArrowIndicator(GameObject root)
    {
        GameObject arrowGo = new GameObject("Arrow_Indicator");
        arrowGo.transform.SetParent(root.transform, false);
        arrowGo.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        arrowGo.transform.localScale = Vector3.one;

        arrowGo.AddComponent<L1_ArrowBobber>();
        arrowGo.SetActive(false);
    }

    private static void WireInteractableFields(L1_Interactable interactable, GameObject root)
    {
        if (interactable == null || root == null) return;
        Transform ind = root.transform.Find("Interact_Indicator");
        if (ind != null)
        {
            SetSerializedObjectReference(interactable, "interactIndicator", ind.gameObject);
        }

        Transform arrow = root.transform.Find("Arrow_Indicator");
        if (arrow != null && interactable is L1_ItemPickup pickup)
        {
            SetSerializedObjectReference(pickup, "arrowIndicator", arrow.gameObject);
        }
    }

    private static void CreateWall(Transform parent, string name, Vector3 pos, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent, false);
        wall.transform.position = pos;
        wall.transform.localScale = Vector3.one;

        var col = wall.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.size = size;
    }

    private static void CreateObstacle(Transform parent, string name, Vector3 pos, Vector2 size)
    {
        GameObject obs = new GameObject(name);
        obs.transform.SetParent(parent, false);
        obs.transform.position = pos;

        var col = obs.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.size = size;
    }

    private static GameObject CreateGameplayRoot(Transform parent, string rootName, Vector3 position, TMP_FontAsset font, bool isPlayer = false, Sprite playerSprite = null)
    {
        GameObject root = new GameObject(rootName);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        if (isPlayer)
        {
            root.tag = "Player";

            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
            col.size = new Vector2(0.8f, 1.27f);
            col.offset = new Vector2(0f, -0.4f);

            // Child: Visual_Sprite for Player
            GameObject visualGo = new GameObject("Visual_Sprite");
            visualGo.transform.SetParent(root.transform, false);
            visualGo.transform.localPosition = Vector3.zero;
            visualGo.transform.localScale = Vector3.one;

            var sr = visualGo.AddComponent<SpriteRenderer>();
            sr.sprite = playerSprite;
            sr.color = Color.white;
            sr.sortingOrder = 5;
        }
        else
        {
            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.8f, 1.8f);

            // Child: Interact_Indicator [!]
            GameObject indGo = new GameObject("Interact_Indicator");
            indGo.transform.SetParent(root.transform, false);
            indGo.transform.localPosition = new Vector3(0f, 1.35f, 0f);

            var tmpInd = indGo.AddComponent<TextMeshPro>();
            if (font != null) tmpInd.font = font;
            tmpInd.text = "[!]";
            tmpInd.fontSize = 3.6f;
            tmpInd.fontStyle = FontStyles.Bold;
            tmpInd.alignment = TextAlignmentOptions.Center;
            tmpInd.color = HexColor("#FFD54F");
            tmpInd.sortingOrder = 12;

            indGo.SetActive(false);
        }

        return root;
    }

    private static GameObject CreateUIPanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPosition, Vector2 sizeDelta,
        Sprite sprite, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = sizeDelta;

        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = color;

        return go;
    }

    private static GameObject CreateUIText(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPosition, Vector2 sizeDelta,
        string text, float fontSize, FontStyles style, Color color,
        TextAlignmentOptions alignment, TMP_FontAsset font)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = sizeDelta;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = alignment;

        return go;
    }

    private static GameObject CreateUIButton(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPosition, Vector2 sizeDelta,
        Sprite sprite, Color color,
        string btnText, float fontSize, FontStyles style, Color textColor, TMP_FontAsset font)
    {
        GameObject btnGo = CreateUIPanel(parent, name, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta, sprite, color);
        btnGo.AddComponent<Button>();

        GameObject txtGo = CreateUIText(btnGo.transform, "Text",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero,
            btnText, fontSize, style, textColor, TextAlignmentOptions.Center, font);

        return btnGo;
    }

    private static Color HexColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color c))
            return c;
        return Color.white;
    }

    private static void SetSerializedObjectReference(UnityEngine.Object target, string propName, UnityEngine.Object refValue)
    {
        if (target == null) return;
        SerializedObject so = new SerializedObject(target);
        var prop = so.FindProperty(propName);
        if (prop != null)
        {
            prop.objectReferenceValue = refValue;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }

    private static void SetSerializedEnum(UnityEngine.Object target, string propName, int enumVal)
    {
        if (target == null) return;
        SerializedObject so = new SerializedObject(target);
        var prop = so.FindProperty(propName);
        if (prop != null)
        {
            prop.enumValueIndex = enumVal;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path == scenePath)
            {
                scenes[i].enabled = true;
                EditorBuildSettings.scenes = scenes.ToArray();
                return;
            }
        }
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
