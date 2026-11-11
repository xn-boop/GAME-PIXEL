using System.Collections.Generic;

using TMPro;

using UnityEditor;

using UnityEditor.SceneManagement;

using UnityEngine;

using UnityEngine.EventSystems;

using UnityEngine.InputSystem.UI;

using UnityEngine.UI;



[InitializeOnLoad]

public static class L2_SceneSetup

{

    private static Sprite LoadSpriteSubAsset(string path)

    {

        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        if (s != null) return s;

        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))

        {

            if (obj is Sprite sp) return sp;

        }

        return null;

    }





    private const string ScenePath = "Assets/Scenes/Scene_Level2.unity";

    private const string TriggerFlagFile = "Temp/UpdateL2Scene.flag";

    private const string TriggerHudFlagFile = "Temp/UpdateL2HUD.flag";



    static L2_SceneSetup()

    {

        EditorApplication.update -= CheckForTrigger;

        EditorApplication.update += CheckForTrigger;

    }



    private static void CheckForTrigger()

    {

        if (System.IO.File.Exists(TriggerFlagFile) || System.IO.File.Exists(TriggerHudFlagFile))

        {

            if (EditorApplication.isPlaying)

            {

                EditorApplication.isPlaying = false;

                return;

            }

            if (EditorApplication.isCompiling) return;

        }



        if (System.IO.File.Exists(TriggerFlagFile))

        {

            try { System.IO.File.Delete(TriggerFlagFile); } catch { }

            UpdateQuizUIInScene();

        }



        if (System.IO.File.Exists(TriggerHudFlagFile))

        {

            try { System.IO.File.Delete(TriggerHudFlagFile); } catch { }

            SetupTopRightHUDInScene();

        }

    }



    [MenuItem("Tools/Level 2/Setup Top-Right HUD & Modals")]

    public static void SetupTopRightHUDInScene()

    {

        Debug.Log("[L2_SceneSetup] Setting up Top-Right HUD Buttons (Pause, Sound, Home, Guide) & Modals in Scene_Level2...");

        string scenePath = ScenePath;

        var scene = EditorSceneManager.GetActiveScene();

        if (scene.path != scenePath)

        {

            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        }



        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.asset");

        if (defaultFont == null) defaultFont = TMP_Settings.defaultFontAsset;



        Canvas canvas = Object.FindFirstObjectByType<Canvas>();

        if (canvas == null)

        {

            Debug.LogError("[L2_SceneSetup] Could not find Canvas in Scene_Level2!");

            return;

        }



        SetupTopRightHUDAndModals(canvas, defaultFont, uiSprite);



        EditorSceneManager.MarkSceneDirty(scene);

        EditorSceneManager.SaveScene(scene);

        Debug.Log("[L2_SceneSetup] Top-Right HUD Buttons and Modals successfully configured and saved!");

    }



    [MenuItem("Tools/Level 2/Update Quiz UI in Scene_Level2")]

    public static void UpdateQuizUIInScene()

    {

        Debug.Log("[L2_SceneSetup] Updating Quiz UI with Illustration in Scene_Level2...");

        if (EditorApplication.isPlaying || EditorApplication.isCompiling)

        {

            Debug.LogWarning("[L2_SceneSetup] Cannot update Scene_Level2 while playing or compiling.");

            return;

        }



        string scenePath = ScenePath;

        var scene = EditorSceneManager.GetActiveScene();

        if (scene.path != scenePath)

        {

            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        }



        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.asset");

        if (defaultFont == null) defaultFont = TMP_Settings.defaultFontAsset;



        var qm = Object.FindFirstObjectByType<L2_QuizManager>();

        if (qm == null)

        {

            Debug.LogError("[L2_SceneSetup] Could not find L2_QuizManager in Scene_Level2!");

            return;

        }



        Canvas canvas = Object.FindFirstObjectByType<Canvas>();

        if (canvas == null)

        {

            Debug.LogError("[L2_SceneSetup] Could not find Canvas in Scene_Level2!");

            return;

        }



        Transform quizModalT = canvas.transform.Find("Panel_Quiz_Dialog");

        if (quizModalT == null)

        {

            Debug.LogError("[L2_SceneSetup] Could not find Panel_Quiz_Dialog in Canvas!");

            return;

        }



        Transform cardContentT = quizModalT.Find("Card_Content");

        if (cardContentT == null)

        {

            Debug.LogError("[L2_SceneSetup] Could not find Card_Content in Panel_Quiz_Dialog!");

            return;

        }



        RectTransform cardRt = cardContentT.GetComponent<RectTransform>();

        cardRt.sizeDelta = new Vector2(1400f, 760f);

        var cardImg = cardContentT.GetComponent<Image>();

        if (cardImg != null)

        {

            cardImg.type = Image.Type.Sliced;

            cardImg.preserveAspect = false;

        }

        cardRt.anchoredPosition = Vector2.zero;

        // 1. Setup Panel_Illustration on Left

        Transform panelIllT = cardContentT.Find("Panel_Illustration");

        GameObject panelIllGo;

        if (panelIllT == null)

        {

            panelIllGo = CreateUIPanel(cardContentT, "Panel_Illustration",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                new Vector2(-440f, -40f), new Vector2(390f, 520f),

                uiSprite, HexColor("#141820"));

        }

        else

        {

            panelIllGo = panelIllT.gameObject;

            var rt = panelIllGo.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0.5f);

            rt.anchorMax = new Vector2(0.5f, 0.5f);

            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = new Vector2(-420f, -50f);

            rt.sizeDelta = new Vector2(380f, 480f);

        }

        // Img_Illustration

        Transform imgIllT = panelIllGo.transform.Find("Img_Illustration");

        GameObject imgIllGo;

        if (imgIllT == null)

        {

            imgIllGo = CreateUIPanel(panelIllGo.transform, "Img_Illustration",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, 40f), new Vector2(310f, 310f),

                null, Color.white);

        }

        else

        {

            imgIllGo = imgIllT.gameObject;

            var rt = imgIllGo.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0.5f);

            rt.anchorMax = new Vector2(0.5f, 0.5f);

            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = new Vector2(0f, 35f);

            rt.sizeDelta = new Vector2(300f, 300f);

        }

        var imgIll = imgIllGo.GetComponent<Image>();

        imgIll.preserveAspect = true;

        // Txt_Illustration_Name

        Transform txtNameT = panelIllGo.transform.Find("Txt_Illustration_Name");

        GameObject txtNameGo;

        if (txtNameT == null)

        {

            txtNameGo = CreateUIText(panelIllGo.transform, "Txt_Illustration_Name",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, -175f), new Vector2(370f, 46f),

                "", 24, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);

        }

        else

        {

            txtNameGo = txtNameT.gameObject;

            var rt = txtNameGo.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0.5f);

            rt.anchorMax = new Vector2(0.5f, 0.5f);

            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = new Vector2(0f, -170f);

            rt.sizeDelta = new Vector2(360f, 46f);

            var tmp = txtNameGo.GetComponent<TextMeshProUGUI>();

            tmp.fontSize = 24f;

            tmp.color = HexColor("#FFD54F");

            if (defaultFont != null) tmp.font = defaultFont;

        }

        // 2. Adjust Top Header Title (On Brown Wooden Header)

        Transform txtTitleT = cardContentT.Find("Txt_Title");

        if (txtTitleT != null)

        {

            var rt = txtTitleT.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0.5f);

            rt.anchorMax = new Vector2(0.5f, 0.5f);

            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = new Vector2(0f, 290f);

            rt.sizeDelta = new Vector2(900f, 54f);

            var tmp = txtTitleT.GetComponent<TextMeshProUGUI>();

            tmp.alignment = TextAlignmentOptions.Center;

            tmp.fontSize = 26f;

            tmp.fontStyle = FontStyles.Bold;

            tmp.color = HexColor("#FFD54F");

            if (defaultFont != null) tmp.font = defaultFont;

        }

        // 3. Adjust Right Column Elements

        Transform txtQuestT = cardContentT.Find("Txt_Question");

        if (txtQuestT != null)

        {

            var rt = txtQuestT.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0.5f);

            rt.anchorMax = new Vector2(0.5f, 0.5f);

            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = new Vector2(220f, 135f);

            rt.sizeDelta = new Vector2(840f, 130f);

            var tmp = txtQuestT.GetComponent<TextMeshProUGUI>();

            tmp.alignment = TextAlignmentOptions.TopLeft;

            tmp.fontSize = 28f;

            tmp.enableAutoSizing = true;

            tmp.fontSizeMin = 22f;

            tmp.fontSizeMax = 28f;

            tmp.margin = new Vector4(8f, 2f, 8f, 2f);

            tmp.color = HexColor("#372016");

            tmp.textWrappingMode = TextWrappingModes.Normal;

            tmp.overflowMode = TextOverflowModes.Ellipsis;

            if (defaultFont != null) tmp.font = defaultFont;

        }

        string[] btnNames = { "Btn_Answer_A", "Btn_Answer_B", "Btn_Answer_C", "Btn_Answer_D" };

        float[] btnYs = { -6f, -92f, -178f, -264f };

        for (int i = 0; i < 4; i++)

        {

            Transform bT = cardContentT.Find(btnNames[i]);

            if (bT != null)

            {

                var rt = bT.GetComponent<RectTransform>();

                rt.anchorMin = new Vector2(0.5f, 0.5f);

                rt.anchorMax = new Vector2(0.5f, 0.5f);

                rt.pivot = new Vector2(0.5f, 0.5f);

                rt.anchoredPosition = new Vector2(220f, btnYs[i]);

                rt.sizeDelta = new Vector2(840f, 76f);

                var txt = bT.GetComponentInChildren<TextMeshProUGUI>();

                if (txt != null)

                {

                    txt.fontSize = 21f;

                    txt.enableAutoSizing = true;

                    txt.fontSizeMin = 18f;

                    txt.fontSizeMax = 21f;

                    txt.alignment = TextAlignmentOptions.MidlineLeft;

                    txt.margin = new Vector4(24f, 2f, 24f, 2f);

                    txt.textWrappingMode = TextWrappingModes.Normal;

                    if (defaultFont != null) txt.font = defaultFont;

                }

            }

        }

        Transform txtFeedT = cardContentT.Find("Txt_Feedback");

        if (txtFeedT != null)

        {

            var rt = txtFeedT.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0.5f);

            rt.anchorMax = new Vector2(0.5f, 0.5f);

            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = new Vector2(220f, 54f);

            rt.sizeDelta = new Vector2(840f, 26f);

            var tmp = txtFeedT.GetComponent<TextMeshProUGUI>();

            tmp.alignment = TextAlignmentOptions.Center;

            tmp.fontSize = 19f;

            tmp.fontStyle = FontStyles.Bold;

            if (defaultFont != null) tmp.font = defaultFont;

        }

        Transform btnCloseT = cardContentT.Find("Btn_Close");

        if (btnCloseT != null)

        {

            var rt = btnCloseT.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(1f, 1f);

            rt.anchorMax = new Vector2(1f, 1f);

            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = new Vector2(-55f, -55f);

            rt.sizeDelta = new Vector2(44f, 44f);

        }

        // 3. Load item sprites and wire references in L2_QuizManager

        Sprite sAoDai = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level2/Icon_AoDai.png");

        Sprite sNonLa = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level2/Icon_NonLa.png");

        Sprite sHanbok = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level2/Icon_Hanbok.png");

        Sprite sMuGat = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level2/Icon_MuGat.png");

        Sprite sCat = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cat_question_mark.png");



        SerializedObject soQm = new SerializedObject(qm);

        soQm.FindProperty("illustrationContainer").objectReferenceValue = panelIllGo;

        soQm.FindProperty("imgIllustration").objectReferenceValue = imgIll;

        soQm.FindProperty("txtIllustrationName").objectReferenceValue = txtNameGo.GetComponent<TextMeshProUGUI>();

        soQm.FindProperty("aoDaiSprite").objectReferenceValue = sAoDai;

        soQm.FindProperty("nonLaSprite").objectReferenceValue = sNonLa;

        soQm.FindProperty("hanbokSprite").objectReferenceValue = sHanbok;

        soQm.FindProperty("muGatSprite").objectReferenceValue = sMuGat;

        soQm.FindProperty("catSprite").objectReferenceValue = sCat;

        soQm.ApplyModifiedPropertiesWithoutUndo();



        EditorSceneManager.MarkSceneDirty(scene);

        EditorSceneManager.SaveScene(scene);

        Debug.Log("[L2_SceneSetup] Successfully updated Quiz UI with Illustration in Scene_Level2!");

    }



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

            "TRANG PHỤC: 0/4 (Đã nhặt: 0)", 24, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, defaultFont);

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

            Vector2.zero, new Vector2(1400f, 760f),

            uiSprite, HexColor("#1E232A"));



        // Close Button

        GameObject btnCloseGo = CreateUIButton(quizCardGo.transform, "Btn_Close",

            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(-40f, -40f), new Vector2(44f, 44f),

            uiSprite, HexColor("#D32F2F"),

            "X", 24, FontStyles.Bold, Color.white, defaultFont);



        // --- LEFT COLUMN: ILLUSTRATION PANEL ---

        GameObject panelIllustrationGo = CreateUIPanel(quizCardGo.transform, "Panel_Illustration",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            new Vector2(-440f, -40f), new Vector2(390f, 520f),

            uiSprite, HexColor("#141820"));



        GameObject imgIllustrationGo = CreateUIPanel(panelIllustrationGo.transform, "Img_Illustration",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, 40f), new Vector2(310f, 310f),

            null, Color.white);

        var imgIllustration = imgIllustrationGo.GetComponent<Image>();

        imgIllustration.preserveAspect = true;



        GameObject txtNameGo = CreateUIText(panelIllustrationGo.transform, "Txt_Illustration_Name",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -175f), new Vector2(370f, 46f),

            "", 24, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



        // --- RIGHT COLUMN: QUESTION & CHOICES ---

        // Title

        GameObject txtTitleGo = CreateUIText(quizCardGo.transform, "Txt_Title",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, 312f), new Vector2(950f, 54f),

            "CÂU HỎI", 26, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



        // Question (hạ xuống Y = 135 để nằm trọn vẹn trong khung vàng, không tràn lên viền gỗ)

        GameObject txtQuestContentGo = CreateUIText(quizCardGo.transform, "Txt_Question",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            new Vector2(235f, 125f), new Vector2(850f, 130f),

            "Nội dung câu hỏi...", 28f, FontStyles.Normal, HexColor("#372016"), TextAlignmentOptions.TopLeft, defaultFont);

        var txtQuestionTmp = txtQuestContentGo.GetComponent<TextMeshProUGUI>();

        txtQuestionTmp.margin = new Vector4(8f, 2f, 8f, 2f);

        txtQuestionTmp.enableAutoSizing = true;

        txtQuestionTmp.fontSizeMin = 22f;

        txtQuestionTmp.fontSizeMax = 28f;

        txtQuestionTmp.textWrappingMode = TextWrappingModes.Normal;

        txtQuestionTmp.overflowMode = TextOverflowModes.Ellipsis;



        // Feedback Text

        GameObject txtFeedGo = CreateUIText(quizCardGo.transform, "Txt_Feedback",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            new Vector2(235f, 48f), new Vector2(850f, 26f),

            "", 19, FontStyles.Bold, HexColor("#4CAF50"), TextAlignmentOptions.Center, defaultFont);



        // 4 Answer Buttons

        Button[] ansBtns = new Button[4];

        TextMeshProUGUI[] ansTexts = new TextMeshProUGUI[4];

        string[] btnNames = { "Btn_Answer_A", "Btn_Answer_B", "Btn_Answer_C", "Btn_Answer_D" };

        float[] btnYs = { -6f, -92f, -178f, -264f };



        for (int i = 0; i < 4; i++)

        {

            GameObject aBtnGo = CreateUIButton(quizCardGo.transform, btnNames[i],

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                new Vector2(235f, btnYs[i]), new Vector2(850f, 78f),

                uiSprite, HexColor("#2C3642"),

                $"Phương án {i + 1}", 21, FontStyles.Normal, Color.white, defaultFont);



            ansBtns[i] = aBtnGo.GetComponent<Button>();

            ansTexts[i] = aBtnGo.GetComponentInChildren<TextMeshProUGUI>();

            ansTexts[i].alignment = TextAlignmentOptions.MidlineLeft;

            ansTexts[i].margin = new Vector4(24f, 2f, 24f, 2f);

            ansTexts[i].enableAutoSizing = true;

            ansTexts[i].fontSizeMin = 18f;

            ansTexts[i].fontSizeMax = 21f;

            ansTexts[i].textWrappingMode = TextWrappingModes.Normal;

        }



        quizModalGo.SetActive(false);



        Sprite quizCardSprite = LoadSpriteSubAsset("Assets/UI/Quiz.png");
        if (quizCardSprite == null) quizCardSprite = uiSprite;

        // 6. Panel_Victory

        GameObject victoryModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Victory",

            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

            Vector2.zero, Vector2.zero,

            uiSprite, new Color(0f, 0f, 0f, 0.85f));



        GameObject victoryCardGo = CreateUIPanel(victoryModalGo.transform, "Card_Victory",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            Vector2.zero, new Vector2(780f, 440f),

            quizCardSprite != null ? quizCardSprite : uiSprite, Color.white);

        var vicCardImg = victoryCardGo.GetComponent<Image>();

        if (vicCardImg != null)

        {

            vicCardImg.type = Image.Type.Sliced;

            vicCardImg.preserveAspect = false;

        }



        CreateUIText(victoryCardGo.transform, "Txt_Victory_Header",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -48f), new Vector2(680f, 48f),
            "XUẤT SẮC!", 34, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);

        CreateUIText(victoryCardGo.transform, "Txt_Victory_Title",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 45f), new Vector2(700f, 65f),
            "HOÀN THÀNH MÀN 2: CHUYẾN ĐI CỦA NHỮNG KÝ ỨC!", 26, FontStyles.Bold, HexColor("#372016"), TextAlignmentOptions.Center, defaultFont);

        CreateUIText(victoryCardGo.transform, "Txt_Victory_Desc",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -32f), new Vector2(680f, 85f),
            "Bạn đã thu thập và phục hồi thành công cả 2 bộ trang phục truyền thống Áo Dài và Hanbok!", 21, FontStyles.Bold, HexColor("#4E342E"), TextAlignmentOptions.Center, defaultFont);

        Sprite btnWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Btn_Wood_Square.png");
        if (btnWoodSprite == null) btnWoodSprite = uiSprite;

        GameObject btnRestartGo = CreateUIButton(victoryCardGo.transform, "Btn_Restart",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(-125f, 60f), new Vector2(210f, 52f),
            btnWoodSprite, Color.white,
            "Chơi lại", 22, FontStyles.Bold, HexColor("#FFE082"), defaultFont);

        GameObject btnContinueGo = CreateUIButton(victoryCardGo.transform, "Btn_Continue",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(125f, 60f), new Vector2(210f, 52f),
            btnWoodSprite, Color.white,
            "Tiếp tục", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



        victoryModalGo.SetActive(false);



        // 7. Top-Right HUD Buttons & Modals (Pause, Sound, Home, Guide)

        SetupTopRightHUDAndModals(rootCanvas.GetComponent<Canvas>(), defaultFont, uiSprite);



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



        soQm.FindProperty("illustrationContainer").objectReferenceValue = panelIllustrationGo;

        soQm.FindProperty("imgIllustration").objectReferenceValue = imgIllustration;

        soQm.FindProperty("txtIllustrationName").objectReferenceValue = txtNameGo.GetComponent<TextMeshProUGUI>();



        Sprite sAoDai = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level2/Icon_AoDai.png");

        Sprite sNonLa = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level2/Icon_NonLa.png");

        Sprite sHanbok = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level2/Icon_Hanbok.png");

        Sprite sMuGat = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level2/Icon_MuGat.png");

        Sprite sCat = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cat_question_mark.png");



        soQm.FindProperty("aoDaiSprite").objectReferenceValue = sAoDai;

        soQm.FindProperty("nonLaSprite").objectReferenceValue = sNonLa;

        soQm.FindProperty("hanbokSprite").objectReferenceValue = sHanbok;

        soQm.FindProperty("muGatSprite").objectReferenceValue = sMuGat;

        soQm.FindProperty("catSprite").objectReferenceValue = sCat;

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



    public static void SetupTopRightHUDAndModals(Canvas rootCanvas, TMP_FontAsset defaultFont, Sprite uiSprite)

    {

        if (rootCanvas == null) return;



        // Clean existing elements if present (idempotent)

        Transform existingTopRight = rootCanvas.transform.Find("HUD_TopRight_Buttons");

        if (existingTopRight != null) Object.DestroyImmediate(existingTopRight.gameObject);

        Transform existingPause = rootCanvas.transform.Find("Panel_Pause");

        if (existingPause != null) Object.DestroyImmediate(existingPause.gameObject);

        Transform existingGuide = rootCanvas.transform.Find("Panel_Guide");

        if (existingGuide != null) Object.DestroyImmediate(existingGuide.gameObject);

        Transform existingConfirmHome = rootCanvas.transform.Find("Panel_Confirm_Home");

        if (existingConfirmHome != null) Object.DestroyImmediate(existingConfirmHome.gameObject);

        Transform existingSoundModal = rootCanvas.transform.Find("Panel_Sound");

        if (existingSoundModal != null) Object.DestroyImmediate(existingSoundModal.gameObject);



        // Load Sprites

        Sprite quizCardSprite = LoadSpriteSubAsset("Assets/UI/Quiz.png");

        if (quizCardSprite == null) quizCardSprite = uiSprite;



        Sprite btnWoodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Btn_Wood_Square.png");

        if (btnWoodSprite == null) btnWoodSprite = uiSprite;



        Sprite iconPause = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Icon_Pause.png");

        Sprite iconSoundOn = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Icon_Sound_On.png");

        Sprite iconSoundOff = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Icon_Sound_Off.png");

        Sprite iconHome = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Icon_Home.png");

        Sprite iconHelp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Icons/Icon_Help.png");



        // Container: HUD_TopRight_Buttons

        GameObject topRightContainer = new GameObject("HUD_TopRight_Buttons");

        topRightContainer.transform.SetParent(rootCanvas.transform, false);

        var rtContainer = topRightContainer.AddComponent<RectTransform>();

        rtContainer.anchorMin = new Vector2(1f, 1f);

        rtContainer.anchorMax = new Vector2(1f, 1f);

        rtContainer.pivot = new Vector2(1f, 1f);

        rtContainer.anchoredPosition = new Vector2(-30f, -30f);

        rtContainer.sizeDelta = new Vector2(250f, 60f);



        var hlg = topRightContainer.AddComponent<HorizontalLayoutGroup>();

        hlg.spacing = 10f;

        hlg.childAlignment = TextAnchor.MiddleRight;

        hlg.childControlWidth = false;

        hlg.childControlHeight = false;

        hlg.childForceExpandWidth = false;

        hlg.childForceExpandHeight = false;



        // 4 Buttons in order: Guide (?), Sound (🔊), Pause (❚❚), Home (🏠)

        GameObject btnHelpGo = CreateIconButton(topRightContainer.transform, "Btn_Help", btnWoodSprite, iconHelp, 54f);

        GameObject btnSoundGo = CreateIconButton(topRightContainer.transform, "Btn_Sound", btnWoodSprite, iconSoundOn, 54f);

        GameObject btnPauseGo = CreateIconButton(topRightContainer.transform, "Btn_Pause", btnWoodSprite, iconPause, 54f);

        GameObject btnHomeGo = CreateIconButton(topRightContainer.transform, "Btn_Home", btnWoodSprite, iconHome, 54f);



        Image imgSound = btnSoundGo.transform.Find("Icon")?.GetComponent<Image>();



        // 2. Panel_Pause

        GameObject pauseModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Pause",

            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

            Vector2.zero, Vector2.zero,

            uiSprite, new Color(0f, 0f, 0f, 0.75f));



        GameObject pauseCardGo = CreateUIPanel(pauseModalGo.transform, "Card_Pause",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            Vector2.zero, new Vector2(620f, 470f),

            quizCardSprite, Color.white);



        CreateUIText(pauseCardGo.transform, "Txt_Title",

            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -55f), new Vector2(450f, 45f),

            "TẠM DỪNG", 28, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



        CreateUIText(pauseCardGo.transform, "Txt_Subtitle",

            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -125f), new Vector2(480f, 32f),

            "Trò chơi đang tạm dừng.", 18, FontStyles.Normal, HexColor("#372016"), TextAlignmentOptions.Center, defaultFont);



        GameObject btnResumePause = CreateUIButton(pauseCardGo.transform, "Btn_Resume",

            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -190f), new Vector2(360f, 48f),

            btnWoodSprite, Color.white,

            "Tiếp tục", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



        GameObject btnRestartPause = CreateUIButton(pauseCardGo.transform, "Btn_Restart",

            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -250f), new Vector2(360f, 48f),

            btnWoodSprite, Color.white,

            "Chơi lại màn 2", 22, FontStyles.Bold, HexColor("#FFE082"), defaultFont);



        GameObject btnHomePause = CreateUIButton(pauseCardGo.transform, "Btn_Home",

            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -310f), new Vector2(360f, 48f),

            btnWoodSprite, Color.white,

            "Về trang chủ", 22, FontStyles.Bold, Color.white, defaultFont);



        GameObject btnClosePause = CreateUIButton(pauseCardGo.transform, "Btn_Close",

            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(-55f, -55f), new Vector2(44f, 44f),

            btnWoodSprite, Color.white,

            "X", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



        pauseModalGo.SetActive(false);



        // 3. Panel_Guide (Tutorial / Help)

        GameObject guideModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Guide",

            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

            Vector2.zero, Vector2.zero,

            uiSprite, new Color(0f, 0f, 0f, 0.75f));



        GameObject guideCardGo = CreateUIPanel(guideModalGo.transform, "Card_Guide",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            Vector2.zero, new Vector2(1100f, 700f),

            quizCardSprite, Color.white);



        CreateUIText(guideCardGo.transform, "Txt_Title",

            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -55f), new Vector2(850f, 44f),

            "HƯỚNG DẪN CHƠI - BẢN ĐỒ 2", 28, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



        string guideText =

            "<color=#7C2D12><b>1. HỎI GỢI Ý TỪ MÈO:</b></color>\n" +

            "Gặp và trò chuyện với <b>Chú Mèo</b> để trả lời câu hỏi và mở khóa các mũi tên chỉ dẫn.\n\n" +

            "<color=#7C2D12><b>2. THU THẬP ĐỦ 4 TRANG PHỤC TRUYỀN THỐNG:</b></color>\n" +

            "• <b>Áo dài & Nón lá</b> (Việt Nam)    • <b>Hanbok & Mũ Gat</b> (Hàn Quốc)\n\n" +

            "<color=#7C2D12><b>3. TRƯNG BÀY VÀO MA-NƠ-CANH:</b></color>\n" +

            "Mang các trang phục đến phòng Ma-nơ-canh, bấm <b>TƯƠNG TÁC</b> để mặc đúng cho 2 ma-nơ-canh.\n\n" +

            "<color=#7C2D12><b>4. KHÁM PHÁ VÀ GIẢI MÃ KÝ ỨC:</b></color>\n" +

            "Tương tác với 6 bức tranh trên tường để tìm lại các mảnh ghép lịch sử văn hóa.\n\n" +

            "<color=#7C2D12><b>ĐIỀU KHIỂN:</b></color>\n" +

            "• Di chuyển: Dùng <b>Joystick ảo</b> hoặc các phím <b>W, A, S, D / Mũi tên</b>.\n" +

            "• Tương tác: Nhấn nút <b>TƯƠNG TÁC [E]</b> hoặc phím <b>[E] / [Space]</b> khi đứng gần mục tiêu.";



        var txtContentGo = CreateUIText(guideCardGo.transform, "Txt_Content",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -20f), new Vector2(920f, 470f),

            guideText, 19.5f, FontStyles.Normal, HexColor("#372016"), TextAlignmentOptions.TopLeft, defaultFont);

        var contentTmp = txtContentGo.GetComponent<TextMeshProUGUI>();

        if (contentTmp != null)

        {

            contentTmp.alignment = TextAlignmentOptions.TopLeft;

            contentTmp.margin = new Vector4(12f, 8f, 12f, 8f);

            contentTmp.enableAutoSizing = true;

            contentTmp.fontSizeMin = 18f;

            contentTmp.fontSizeMax = 20.5f;

            contentTmp.lineSpacing = 0f;

            contentTmp.paragraphSpacing = 8f;

            contentTmp.textWrappingMode = TextWrappingModes.Normal;

            contentTmp.overflowMode = TextOverflowModes.Truncate;

        }



        GameObject btnCloseGuide = CreateUIButton(guideCardGo.transform, "Btn_Understood",

            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, 48f), new Vector2(240f, 46f),

            btnWoodSprite, Color.white,

            "ĐÃ HIỂU", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



        GameObject btnCloseGuideX = CreateUIButton(guideCardGo.transform, "Btn_Close",

            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(-55f, -55f), new Vector2(46f, 46f),

            btnWoodSprite, Color.white,

            "X", 24, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



        guideModalGo.SetActive(false);



        // 4. Panel_Confirm_Home

        GameObject confirmHomeModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Confirm_Home",

            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

            Vector2.zero, Vector2.zero,

            uiSprite, new Color(0f, 0f, 0f, 0.75f));



        GameObject confirmHomeCardGo = CreateUIPanel(confirmHomeModalGo.transform, "Card_Confirm_Home",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            Vector2.zero, new Vector2(620f, 370f),

            quizCardSprite, Color.white);



        CreateUIText(confirmHomeCardGo.transform, "Txt_Title",

            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -55f), new Vector2(500f, 45f),

            "VỀ TRANG CHỦ", 28, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



        CreateUIText(confirmHomeCardGo.transform, "Txt_Msg",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -10f), new Vector2(500f, 85f),

            "Bạn có chắc chắn muốn quay về Trang chủ không?\nTiến trình chưa hoàn thành có thể sẽ phải chơi lại.",

            18, FontStyles.Normal, HexColor("#372016"), TextAlignmentOptions.Center, defaultFont);



        GameObject btnYesHome = CreateUIButton(confirmHomeCardGo.transform, "Btn_Yes",

            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),

            new Vector2(-110f, 58f), new Vector2(180f, 46f),

            btnWoodSprite, Color.white,

            "Xác nhận", 20, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



        GameObject btnNoHome = CreateUIButton(confirmHomeCardGo.transform, "Btn_No",

            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),

            new Vector2(110f, 58f), new Vector2(180f, 46f),

            btnWoodSprite, Color.white,

            "Hủy", 20, FontStyles.Bold, Color.white, defaultFont);



        GameObject btnCloseConfirmHome = CreateUIButton(confirmHomeCardGo.transform, "Btn_Close",

            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(-55f, -55f), new Vector2(44f, 44f),

            btnWoodSprite, Color.white,

            "X", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



        confirmHomeModalGo.SetActive(false);



        // 5. Panel_Sound (Audio Settings Modal)

        GameObject soundModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Sound",

            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

            Vector2.zero, Vector2.zero,

            uiSprite, new Color(0f, 0f, 0f, 0.75f));



        GameObject soundCardGo = CreateUIPanel(soundModalGo.transform, "Card_Sound",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            Vector2.zero, new Vector2(620f, 420f),

            quizCardSprite, Color.white);



        CreateUIText(soundCardGo.transform, "Txt_Title",

            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -55f), new Vector2(500f, 45f),

            "CÀI ĐẶT ÂM THANH", 26, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



        CreateUIText(soundCardGo.transform, "Txt_Subtitle",

            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -125f), new Vector2(480f, 32f),

            "Điều chỉnh âm thanh trò chơi", 18, FontStyles.Normal, HexColor("#372016"), TextAlignmentOptions.Center, defaultFont);



        GameObject btnToggleSoundModal = CreateUIButton(soundCardGo.transform, "Btn_ToggleSound",

            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -185f), new Vector2(360f, 50f),

            btnWoodSprite, Color.white,

            "Âm thanh: ĐANG BẬT", 21, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);

        var txtSoundStatusModal = btnToggleSoundModal.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();



        CreateUIText(soundCardGo.transform, "Txt_Hint",

            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, -245f), new Vector2(480f, 30f),

            "Nhấn nút trên để bật hoặc tắt toàn bộ âm thanh", 16, FontStyles.Normal, HexColor("#5D4037"), TextAlignmentOptions.Center, defaultFont);



        GameObject btnCloseSoundBottom = CreateUIButton(soundCardGo.transform, "Btn_Close_Bottom",

            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, 55f), new Vector2(200f, 46f),

            btnWoodSprite, Color.white,

            "Đóng", 20, FontStyles.Bold, Color.white, defaultFont);



        GameObject btnCloseSoundX = CreateUIButton(soundCardGo.transform, "Btn_Close",

            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(-55f, -55f), new Vector2(44f, 44f),

            btnWoodSprite, Color.white,

            "X", 22, FontStyles.Bold, HexColor("#FFD54F"), defaultFont);



        soundModalGo.SetActive(false);



        // 6. Add or Wire L2_GameHUDController

        var hudCtrl = rootCanvas.GetComponent<L2_GameHUDController>();

        if (hudCtrl == null) hudCtrl = rootCanvas.gameObject.AddComponent<L2_GameHUDController>();



        SerializedObject soHud = new SerializedObject(hudCtrl);

        soHud.FindProperty("btnPause").objectReferenceValue = btnPauseGo.GetComponent<Button>();

        soHud.FindProperty("btnSound").objectReferenceValue = btnSoundGo.GetComponent<Button>();

        soHud.FindProperty("btnHome").objectReferenceValue = btnHomeGo.GetComponent<Button>();

        soHud.FindProperty("btnHelp").objectReferenceValue = btnHelpGo.GetComponent<Button>();



        soHud.FindProperty("imgSoundIcon").objectReferenceValue = imgSound;

        soHud.FindProperty("soundOnSprite").objectReferenceValue = iconSoundOn;

        soHud.FindProperty("soundOffSprite").objectReferenceValue = iconSoundOff;



        soHud.FindProperty("panelPause").objectReferenceValue = pauseModalGo;

        soHud.FindProperty("panelGuide").objectReferenceValue = guideModalGo;

        soHud.FindProperty("panelConfirmHome").objectReferenceValue = confirmHomeModalGo;

        soHud.FindProperty("panelSound").objectReferenceValue = soundModalGo;



        soHud.FindProperty("btnResumePause").objectReferenceValue = btnResumePause.GetComponent<Button>();

        soHud.FindProperty("btnRestartPause").objectReferenceValue = btnRestartPause.GetComponent<Button>();

        soHud.FindProperty("btnHomePause").objectReferenceValue = btnHomePause.GetComponent<Button>();

        soHud.FindProperty("btnClosePause").objectReferenceValue = btnClosePause.GetComponent<Button>();



        soHud.FindProperty("btnCloseGuide").objectReferenceValue = btnCloseGuide.GetComponent<Button>();

        soHud.FindProperty("btnCloseGuideAlt").objectReferenceValue = btnCloseGuideX.GetComponent<Button>();



        soHud.FindProperty("btnConfirmHomeYes").objectReferenceValue = btnYesHome.GetComponent<Button>();

        soHud.FindProperty("btnConfirmHomeNo").objectReferenceValue = btnNoHome.GetComponent<Button>();

        soHud.FindProperty("btnCloseConfirmHome").objectReferenceValue = btnCloseConfirmHome.GetComponent<Button>();



        soHud.FindProperty("btnToggleSoundModal").objectReferenceValue = btnToggleSoundModal.GetComponent<Button>();

        soHud.FindProperty("txtSoundStatusModal").objectReferenceValue = txtSoundStatusModal;

        soHud.FindProperty("btnCloseSound").objectReferenceValue = btnCloseSoundBottom.GetComponent<Button>();

        soHud.FindProperty("btnCloseSoundAlt").objectReferenceValue = btnCloseSoundX.GetComponent<Button>();



        soHud.ApplyModifiedPropertiesWithoutUndo();

    }



    private static GameObject CreateIconButton(Transform parent, string name, Sprite bgSprite, Sprite iconSprite, float size)

    {

        GameObject btnGo = new GameObject(name);

        btnGo.transform.SetParent(parent, false);

        var rt = btnGo.AddComponent<RectTransform>();

        rt.sizeDelta = new Vector2(size, size);



        var img = btnGo.AddComponent<Image>();

        img.sprite = bgSprite;

        img.color = Color.white;

        img.type = Image.Type.Sliced;



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

        iconRt.sizeDelta = new Vector2(size * 0.62f, size * 0.62f);



        var iconImg = iconGo.AddComponent<Image>();

        iconImg.sprite = iconSprite;

        iconImg.preserveAspect = true;

        iconImg.color = Color.white;

        iconImg.raycastTarget = false;



        return btnGo;

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

