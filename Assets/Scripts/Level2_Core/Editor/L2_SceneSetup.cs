using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class L2_SceneSetup
{
    private const string ScenePath = "Assets/Scenes/Scene_Level2.unity";

    [MenuItem("Tools/Level 2/Generate Scene_Level2")]
    public static void GenerateScene()
    {
        Debug.Log("[L2_SceneSetup] Starting Scene_Level2 Generation...");

        // 1. Create a fresh empty scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. Load basic primitives & assets
        Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Square.png");
        Sprite capsuleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/Capsule.png");
        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.asset");
        if (defaultFont == null) defaultFont = TMP_Settings.defaultFontAsset;

        if (squareSprite == null)
        {
            Debug.LogError("[L2_SceneSetup] Could not load Square.png from 2D Sprite package!");
            return;
        }

        // 3. Create the 5 required Hierarchy Root Groups
        GameObject rootManagers = new GameObject("--- MANAGERS ---");
        GameObject rootCameras = new GameObject("--- CAMERAS ---");
        GameObject rootEnvironment = new GameObject("--- ENVIRONMENT (GRAYBOX) ---");
        GameObject rootGameplay = new GameObject("--- GAMEPLAY OBJECTS ---");
        GameObject rootCanvas = new GameObject("--- UI CANVAS ---");

        // -------------------------------------------------------------
        // GROUP 1: MANAGERS
        // -------------------------------------------------------------
        var gm = rootManagers.AddComponent<L2_GameManager>();
        var qm = rootManagers.AddComponent<L2_QuizManager>();
        var im = rootManagers.AddComponent<L2_InventoryManager>();

        // EventSystem
        GameObject eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.transform.SetParent(rootManagers.transform, false);
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<InputSystemUIInputModule>();

        // -------------------------------------------------------------
        // GROUP 3: ENVIRONMENT (GRAYBOX)
        // -------------------------------------------------------------
        // Map_Background (Scale 24x14, Color #3E2723, Order = -10)
        GameObject bgGo = new GameObject("Map_Background");
        bgGo.transform.SetParent(rootEnvironment.transform, false);
        bgGo.transform.position = Vector3.zero;
        bgGo.transform.localScale = new Vector3(24f, 14f, 1f);
        var bgSr = bgGo.AddComponent<SpriteRenderer>();
        bgSr.sprite = squareSprite;
        bgSr.color = HexColor("#3E2723");
        bgSr.sortingOrder = -10;

        // Walls_Container
        GameObject wallsContainer = new GameObject("Walls_Container");
        wallsContainer.transform.SetParent(rootEnvironment.transform, false);

        // Outer perimeter walls (approx 24x14 bounds)
        CreateWall(wallsContainer.transform, "Wall_Top", new Vector3(0f, 7.25f, 0f), new Vector2(25f, 0.5f), squareSprite);
        CreateWall(wallsContainer.transform, "Wall_Bottom", new Vector3(0f, -7.25f, 0f), new Vector2(25f, 0.5f), squareSprite);
        CreateWall(wallsContainer.transform, "Wall_Left", new Vector3(-12.25f, 0f, 0f), new Vector2(0.5f, 15f), squareSprite);
        CreateWall(wallsContainer.transform, "Wall_Right", new Vector3(12.25f, 0f, 0f), new Vector2(0.5f, 15f), squareSprite);

        // Partition Walls:
        // Left room partition at x = -3.5 (Doorway from y = -2.0 to y = 2.0)
        CreateWall(wallsContainer.transform, "Partition_Left_Top", new Vector3(-3.5f, 4.5f, 0f), new Vector2(0.5f, 5f), squareSprite);
        CreateWall(wallsContainer.transform, "Partition_Left_Bottom", new Vector3(-3.5f, -4.5f, 0f), new Vector2(0.5f, 5f), squareSprite);

        // Right partition at x = 3.5:
        // Upper wall (y = 3.5 to 7.0) -> doorway into Top-Right from y = 1.0 to 3.5
        CreateWall(wallsContainer.transform, "Partition_Right_Top", new Vector3(3.5f, 5.25f, 0f), new Vector2(0.5f, 3.5f), squareSprite);
        // Middle wall (y = -2.0 to 1.0) -> doorway into Bottom-Right from y = -4.5 to -2.0
        CreateWall(wallsContainer.transform, "Partition_Right_Mid", new Vector3(3.5f, -0.5f, 0f), new Vector2(0.5f, 3f), squareSprite);
        // Lower wall (y = -7.0 to -4.5)
        CreateWall(wallsContainer.transform, "Partition_Right_Bottom", new Vector3(3.5f, -5.75f, 0f), new Vector2(0.5f, 2.5f), squareSprite);

        // Horizontal dividing wall between Top-Right (Mannequins) & Bottom-Right (Korea) at y = 0.5
        CreateWall(wallsContainer.transform, "Partition_Horizontal_Right", new Vector3(7.75f, 0.5f, 0f), new Vector2(8.5f, 0.5f), squareSprite);

        // -------------------------------------------------------------
        // GROUP 4: GAMEPLAY OBJECTS
        // -------------------------------------------------------------
        // 1. Player
        GameObject playerGo = CreateGameplayRoot(rootGameplay.transform, "Player", new Vector3(0f, -2.5f, 0f),
            HexColor("#FFFFFF"), capsuleSprite != null ? capsuleSprite : squareSprite, "HẠ ÂN (PLAYER)", defaultFont, isPlayer: true);
        var playerCtrl = playerGo.AddComponent<L2_PlayerController>();

        // 2. NPC_Cat
        GameObject catGo = CreateGameplayRoot(rootGameplay.transform, "NPC_Cat", new Vector3(0f, 2.8f, 0f),
            HexColor("#FF9800"), squareSprite, "MÈO", defaultFont);
        var cat = catGo.AddComponent<L2_NPC_Cat>();
        WireInteractableFields(cat, catGo);

        // 3. 6 Paintings (Color #00BCD4)
        L2_Painting[] paintings = new L2_Painting[6];

        // Painting 1: Ha Long (Top wall of Left Room)
        GameObject p1 = CreateGameplayRoot(rootGameplay.transform, "Painting_01", new Vector3(-8f, 6.0f, 0f),
            HexColor("#00BCD4"), squareSprite, "TRANH 1: HẠ LONG", defaultFont);
        paintings[0] = p1.AddComponent<L2_Painting>();
        SetSerializedInt(paintings[0], "paintingIndex", 1);
        WireInteractableFields(paintings[0], p1);

        // Painting 2: Chua Mot Cot (Left wall of Left Room)
        GameObject p2 = CreateGameplayRoot(rootGameplay.transform, "Painting_02", new Vector3(-11.2f, 0f, 0f),
            HexColor("#00BCD4"), squareSprite, "TRANH 2: CHÙA MỘT CỘT", defaultFont);
        paintings[1] = p2.AddComponent<L2_Painting>();
        SetSerializedInt(paintings[1], "paintingIndex", 2);
        WireInteractableFields(paintings[1], p2);

        // Painting 3: Cau Vang (Bottom wall of Left Room)
        GameObject p3 = CreateGameplayRoot(rootGameplay.transform, "Painting_03", new Vector3(-8f, -6.0f, 0f),
            HexColor("#00BCD4"), squareSprite, "TRANH 3: CẦU VÀNG", defaultFont);
        paintings[2] = p3.AddComponent<L2_Painting>();
        SetSerializedInt(paintings[2], "paintingIndex", 3);
        WireInteractableFields(paintings[2], p3);

        // Painting 4: N Seoul Tower (Bottom wall of Bottom-Right Room)
        GameObject p4 = CreateGameplayRoot(rootGameplay.transform, "Painting_04", new Vector3(7.5f, -6.0f, 0f),
            HexColor("#00BCD4"), squareSprite, "TRANH 4: THÁP N SEOUL", defaultFont);
        paintings[3] = p4.AddComponent<L2_Painting>();
        SetSerializedInt(paintings[3], "paintingIndex", 4);
        WireInteractableFields(paintings[3], p4);

        // Painting 5: Cheomseongdae (Right wall of Bottom-Right Room)
        GameObject p5 = CreateGameplayRoot(rootGameplay.transform, "Painting_05", new Vector3(11.2f, -3.25f, 0f),
            HexColor("#00BCD4"), squareSprite, "TRANH 5: CHEOMSEONGDAE", defaultFont);
        paintings[4] = p5.AddComponent<L2_Painting>();
        SetSerializedInt(paintings[4], "paintingIndex", 5);
        WireInteractableFields(paintings[4], p5);

        // Painting 6: Gyeongbokgung (Upper partition wall of Bottom-Right Room)
        GameObject p6 = CreateGameplayRoot(rootGameplay.transform, "Painting_06", new Vector3(7.5f, -0.2f, 0f),
            HexColor("#00BCD4"), squareSprite, "TRANH 6: GYEONGBOKGUNG", defaultFont);
        paintings[5] = p6.AddComponent<L2_Painting>();
        SetSerializedInt(paintings[5], "paintingIndex", 6);
        WireInteractableFields(paintings[5], p6);

        // 4. 2 Outfit Spawns (Color #FFEB3B, initial alpha = 0.3)
        GameObject aoDaiSpawnGo = CreateGameplayRoot(rootGameplay.transform, "ItemSpawn_AoDai", new Vector3(-7.5f, 0f, 0f),
            new Color(1f, 0.92f, 0.23f, 0.3f), squareSprite, "ÁO DÀI", defaultFont);
        var aoDaiPickup = aoDaiSpawnGo.AddComponent<L2_ItemPickup>();
        SetSerializedEnum(aoDaiPickup, "itemType", (int)L2_ItemType.AoDai);
        WireInteractableFields(aoDaiPickup, aoDaiSpawnGo);

        GameObject hanbokSpawnGo = CreateGameplayRoot(rootGameplay.transform, "ItemSpawn_Hanbok", new Vector3(7.5f, -3.25f, 0f),
            new Color(1f, 0.92f, 0.23f, 0.3f), squareSprite, "HANBOK", defaultFont);
        var hanbokPickup = hanbokSpawnGo.AddComponent<L2_ItemPickup>();
        SetSerializedEnum(hanbokPickup, "itemType", (int)L2_ItemType.Hanbok);
        WireInteractableFields(hanbokPickup, hanbokSpawnGo);

        // 5. 2 Mannequins (Color #E91E63)
        GameObject manAoDaiGo = CreateGameplayRoot(rootGameplay.transform, "Mannequin_AoDai", new Vector3(6.5f, 3.8f, 0f),
            HexColor("#E91E63"), squareSprite, "MA NƠ CANH: ÁO DÀI", defaultFont);
        var manAoDai = manAoDaiGo.AddComponent<L2_Mannequin>();
        SetSerializedEnum(manAoDai, "requiredItem", (int)L2_ItemType.AoDai);
        WireInteractableFields(manAoDai, manAoDaiGo);

        GameObject manHanbokGo = CreateGameplayRoot(rootGameplay.transform, "Mannequin_Hanbok", new Vector3(9.5f, 3.8f, 0f),
            HexColor("#E91E63"), squareSprite, "MA NƠ CANH: HANBOK", defaultFont);
        var manHanbok = manHanbokGo.AddComponent<L2_Mannequin>();
        SetSerializedEnum(manHanbok, "requiredItem", (int)L2_ItemType.Hanbok);
        WireInteractableFields(manHanbok, manHanbokGo);

        // -------------------------------------------------------------
        // GROUP 2: CAMERAS
        // -------------------------------------------------------------
        GameObject camGo = new GameObject("Main Camera");
        camGo.transform.SetParent(rootCameras.transform, false);
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(0f, -2.5f, -10f);

        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = HexColor("#1E1E24");

        camGo.AddComponent<AudioListener>();

        var camFollow = camGo.AddComponent<L2_CameraFollow>();
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
            new Vector2(30f, -30f), new Vector2(360f, 60f),
            uiSprite, new Color(0.12f, 0.15f, 0.2f, 0.85f));

        GameObject objTxtGo = CreateUIText(objPanelGo.transform, "Txt_Objective",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero,
            "TRANG PHỤC: 0/2 (Đã nhặt: 0)", 24, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, defaultFont);
        var txtObjective = objTxtGo.GetComponent<TextMeshProUGUI>();

        // 2. HUD_TopCenter_Quest
        GameObject questPanelGo = CreateUIPanel(rootCanvas.transform, "HUD_TopCenter_Quest",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -30f), new Vector2(720f, 60f),
            uiSprite, new Color(0.12f, 0.15f, 0.2f, 0.9f));

        GameObject questTxtGo = CreateUIText(questPanelGo.transform, "Txt_QuestHint",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero,
            "Tương tác với chú mèo để tìm gợi ý...", 22, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);
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

        var vj = joystickBaseGo.AddComponent<L2_VirtualJoystick>();
        SetSerializedObjectReference(vj, "joystickHandle", joystickKnobGo.GetComponent<RectTransform>());
        SetSerializedObjectReference(vj, "playerController", playerCtrl);

        // Btn_Interact
        GameObject btnInteractGo = CreateUIButton(controlsGo.transform, "Btn_Interact",
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(-160f, 160f), new Vector2(180f, 80f),
            uiSprite, new Color(0.18f, 0.7f, 0.35f, 1f),
            "TƯƠNG TÁC [E]", 22, FontStyles.Bold, Color.white, defaultFont);

        var interactBtnScript = btnInteractGo.AddComponent<L2_InteractButton>();
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
            "HOÀN THÀNH MÀN 2: CHUYẾN ĐI CỦA NHỮNG KÝ ỨC!", 26, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, defaultFont);

        CreateUIText(victoryCardGo.transform, "Txt_Victory_Desc",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -200f), new Vector2(680f, 60f),
            "Bạn đã thu thập và phục hồi thành công cả 2 bộ trang phục truyền thống Áo Dài và Hanbok!", 20, FontStyles.Normal, HexColor("#CFD8DC"), TextAlignmentOptions.Center, defaultFont);

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
        // Wire L2_GameManager
        SerializedObject soGm = new SerializedObject(gm);
        soGm.FindProperty("txtObjective").objectReferenceValue = txtObjective;
        soGm.FindProperty("txtQuestHint").objectReferenceValue = txtQuestHint;
        soGm.FindProperty("panelVictory").objectReferenceValue = victoryModalGo;
        soGm.FindProperty("btnRestart").objectReferenceValue = btnRestartGo.GetComponent<Button>();
        soGm.FindProperty("btnContinue").objectReferenceValue = btnContinueGo.GetComponent<Button>();
        soGm.FindProperty("aoDaiPickup").objectReferenceValue = aoDaiPickup;
        soGm.FindProperty("hanbokPickup").objectReferenceValue = hanbokPickup;
        soGm.ApplyModifiedPropertiesWithoutUndo();

        // Wire L2_QuizManager
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

        // Wire L2_InventoryManager
        SerializedObject soIm = new SerializedObject(im);
        SerializedProperty spSlots = soIm.FindProperty("slotIcons");
        spSlots.arraySize = 5;
        for (int i = 0; i < 5; i++) spSlots.GetArrayElementAtIndex(i).objectReferenceValue = invSlotIcons[i];
        soIm.ApplyModifiedPropertiesWithoutUndo();

        // 4. Save Scene
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log($"[L2_SceneSetup] Successfully saved new scene to: {ScenePath}");

        // 5. Ensure scene is in EditorBuildSettings
        AddSceneToBuildSettings(ScenePath);

        Debug.Log("[L2_SceneSetup] Scene_Level2 setup COMPLETE!");
    }

    private static void WireInteractableFields(L2_Interactable interactable, GameObject root)
    {
        if (interactable == null || root == null) return;
        Transform ind = root.transform.Find("Interact_Indicator");
        if (ind != null)
        {
            SetSerializedObjectReference(interactable, "interactIndicator", ind.gameObject);
        }

        Transform vis = root.transform.Find("Visual_Sprite");
        if (vis != null)
        {
            var sr = vis.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                SetSerializedObjectReference(interactable, "visualSprite", sr);
            }
        }

        Transform arrow = root.transform.Find("Arrow_Indicator");
        if (arrow != null)
        {
            SetSerializedObjectReference(interactable, "arrowIndicator", arrow.gameObject);
        }
    }

    private static void CreateWall(Transform parent, string name, Vector3 pos, Vector2 size, Sprite sprite)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent, false);
        wall.transform.position = pos;
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);

        var sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = HexColor("#757575");
        sr.sortingOrder = 0;

        var col = wall.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.size = Vector2.one;
    }

    private static GameObject CreateGameplayRoot(Transform parent, string rootName, Vector3 position,
        Color spriteColor, Sprite sprite, string labelText, TMP_FontAsset font, bool isPlayer = false)
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
            col.size = new Vector2(0.8f, 0.8f);
        }
        else
        {
            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.6f, 1.6f);
        }

        // Child 1: Visual_Sprite
        GameObject visualGo = new GameObject("Visual_Sprite");
        visualGo.transform.SetParent(root.transform, false);
        visualGo.transform.localPosition = Vector3.zero;
        visualGo.transform.localScale = Vector3.one;

        var sr = visualGo.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = spriteColor;
        sr.sortingOrder = 5;

        // Child 2: Label_Debug
        GameObject labelGo = new GameObject("Label_Debug");
        labelGo.transform.SetParent(root.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 0.85f, 0f);

        var tmp = labelGo.AddComponent<TextMeshPro>();
        if (font != null) tmp.font = font;
        tmp.text = labelText;
        tmp.fontSize = 2.4f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.sortingOrder = 10;

        // Child 3: Interact_Indicator (for non-player objects)
        if (!isPlayer)
        {
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
        }
    }

    private static void SetSerializedInt(UnityEngine.Object target, string propName, int val)
    {
        if (target == null) return;
        SerializedObject so = new SerializedObject(target);
        var prop = so.FindProperty(propName);
        if (prop != null)
        {
            prop.intValue = val;
            so.ApplyModifiedPropertiesWithoutUndo();
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

    [MenuItem("Tools/Level 2/Apply SVN-Determination Font to Scene_Level2")]
    public static void ApplySvnDeterminationFontToScene()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.asset");
        if (font == null)
        {
            string[] guids = AssetDatabase.FindAssets("SVN-Determination t:TMP_FontAsset");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            }
        }

        if (font == null)
        {
            Debug.LogError("[L2_SceneSetup] Could not find SVN-Determination Sans font asset!");
            return;
        }

        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.path != ScenePath)
        {
            activeScene = EditorSceneManager.OpenScene(ScenePath);
        }

        var allTexts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int modifiedCount = 0;

        foreach (var txt in allTexts)
        {
            if (txt != null && txt.gameObject.scene == activeScene)
            {
                txt.font = font;
                EditorUtility.SetDirty(txt);
                EditorUtility.SetDirty(txt.gameObject);
                modifiedCount++;
            }
        }

        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);

        Debug.Log($"[L2_SceneSetup] Successfully updated {modifiedCount} TextMeshProUGUI components to SVN-Determination Sans in Scene_Level2.");
    }
}
