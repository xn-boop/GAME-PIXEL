using System.Collections.Generic;

using System.IO;

using TMPro;

using UnityEditor;

using UnityEditor.SceneManagement;

using UnityEngine;

using UnityEngine.EventSystems;

using UnityEngine.InputSystem.UI;

using UnityEngine.UI;



[InitializeOnLoad]

public static class L1_SceneSetup

{

    private const string ScenePath = "Assets/Scenes/Level1_Prototype.unity";

    private const string TriggerFlagFile = "Temp/UpdateL1Scene.flag";

    private const string RevertFoodFlagFile = "Temp/RevertL1LivingRoomFood.flag";

    private const string TriggerHudFlagFile = "Temp/UpdateL1HUD.flag";



    static L1_SceneSetup()

    {

        EditorApplication.update -= CheckForTrigger;

        EditorApplication.update += CheckForTrigger;

    }



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



    private static void CheckForTrigger()

    {

        if (System.IO.File.Exists(TriggerFlagFile) || System.IO.File.Exists(RevertFoodFlagFile) || System.IO.File.Exists(TriggerHudFlagFile))

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

        if (System.IO.File.Exists(RevertFoodFlagFile))

        {

            try { System.IO.File.Delete(RevertFoodFlagFile); } catch { }

            RevertLivingRoomFoodAndRestoreKitchenCulturalItems();

        }

        if (System.IO.File.Exists(TriggerHudFlagFile))

        {

            try { System.IO.File.Delete(TriggerHudFlagFile); } catch { }

            SetupTopRightHUDInScene();

        }

    }



    [MenuItem("Tools/Map 1/Setup Top-Right HUD & Modals")]

    public static void SetupTopRightHUDInScene()

    {

        Debug.Log("[L1_SceneSetup] Setting up Top-Right HUD Buttons (Pause, Sound, Home, Guide) & Modals in Level1_Prototype...");

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

            Debug.LogError("[L1_SceneSetup] Could not find Canvas in Level1_Prototype!");

            return;

        }



        SetupTopRightHUDAndModals(canvas, defaultFont, uiSprite);



        EditorSceneManager.MarkSceneDirty(scene);

        EditorSceneManager.SaveScene(scene);

        Debug.Log("[L1_SceneSetup] Top-Right HUD Buttons and Modals successfully configured and saved in Level1_Prototype!");

    }



    [MenuItem("Tools/Map 1/Update Quiz UI in Level1_Prototype")]

    public static void UpdateQuizUIInScene()

    {

        Debug.Log("[L1_SceneSetup] Updating Quiz UI with Illustration in Level1_Prototype...");

        if (EditorApplication.isPlaying || EditorApplication.isCompiling)

        {

            Debug.LogWarning("[L1_SceneSetup] Cannot update Level1_Prototype while playing or compiling.");

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



        Sprite quizCardSprite = null;

        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath("Assets/UI/Quiz.png"))

        {

            if (obj is Sprite s)

            {

                quizCardSprite = s;

                break;

            }

        }



        var qm = Object.FindFirstObjectByType<L1_QuizManager>();

        if (qm == null)

        {

            Debug.LogError("[L1_SceneSetup] Could not find L1_QuizManager in Level1_Prototype!");

            return;

        }



        Canvas canvas = Object.FindFirstObjectByType<Canvas>();

        if (canvas == null)

        {

            Debug.LogError("[L1_SceneSetup] Could not find Canvas in Level1_Prototype!");

            return;

        }



        Transform quizModalT = canvas.transform.Find("Panel_Quiz_Dialog");

        if (quizModalT == null)

        {

            Debug.LogError("[L1_SceneSetup] Could not find Panel_Quiz_Dialog in Canvas!");

            return;

        }



        Transform cardContentT = quizModalT.Find("Card_Content");

        if (cardContentT == null)

        {

            Debug.LogError("[L1_SceneSetup] Could not find Card_Content in Panel_Quiz_Dialog!");

            return;

        }



        RectTransform cardRt = cardContentT.GetComponent<RectTransform>();

        cardRt.sizeDelta = new Vector2(1100f, 500f);

        var cardImg = cardContentT.GetComponent<Image>();

        if (cardImg != null)

        {

            if (quizCardSprite != null) cardImg.sprite = quizCardSprite;

            cardImg.type = Image.Type.Sliced;

            cardImg.preserveAspect = false;

            cardImg.color = Color.white;

        }

        cardRt.anchoredPosition = Vector2.zero;



        // 1. Setup Panel_Illustration on Left

        Transform panelIllT = cardContentT.Find("Panel_Illustration");

        GameObject panelIllGo;

        if (panelIllT == null)

        {

            panelIllGo = CreateUIPanel(cardContentT, "Panel_Illustration",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                new Vector2(-330f, -30f), new Vector2(310f, 308f),

                uiSprite, HexColor("#141820"));

        }

        else

        {

            panelIllGo = panelIllT.gameObject;

            var rt = panelIllGo.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0.5f);

            rt.anchorMax = new Vector2(0.5f, 0.5f);

            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = new Vector2(-330f, -30f);

            rt.sizeDelta = new Vector2(310f, 308f);

        }



        // Img_Illustration

        Transform imgIllT = panelIllGo.transform.Find("Img_Illustration");

        GameObject imgIllGo;

        if (imgIllT == null)

        {

            imgIllGo = CreateUIPanel(panelIllGo.transform, "Img_Illustration",

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                new Vector2(0f, 18f), new Vector2(240f, 240f),

                null, Color.white);

        }

        else

        {

            imgIllGo = imgIllT.gameObject;

            var rt = imgIllGo.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0.5f);

            rt.anchorMax = new Vector2(0.5f, 0.5f);

            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = new Vector2(0f, 18f);

            rt.sizeDelta = new Vector2(240f, 240f);

        }

        var imgIll = imgIllGo.GetComponent<Image>();

        imgIll.type = Image.Type.Simple;

        imgIll.preserveAspect = true;



        // Txt_Illustration_Name

        Transform txtNameT = panelIllGo.transform.Find("Txt_Illustration_Name");

        GameObject txtNameGo;

        if (txtNameT == null)

        {

            txtNameGo = CreateUIText(panelIllGo.transform, "Txt_Illustration_Name",

                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),

                new Vector2(0f, 10f), new Vector2(280f, 28f),

                "", 19, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);

        }

        else

        {

            txtNameGo = txtNameT.gameObject;

            var rt = txtNameGo.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0f);

            rt.anchorMax = new Vector2(0.5f, 0f);

            rt.pivot = new Vector2(0.5f, 0f);

            rt.anchoredPosition = new Vector2(0f, 10f);

            rt.sizeDelta = new Vector2(280f, 28f);

            var tmp = txtNameGo.GetComponent<TextMeshProUGUI>();

            tmp.fontSize = 19f;

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

            rt.anchoredPosition = new Vector2(0f, 195f);

            rt.sizeDelta = new Vector2(800f, 42f);

            var tmp = txtTitleT.GetComponent<TextMeshProUGUI>();

            tmp.alignment = TextAlignmentOptions.Center;

            tmp.fontSize = 24f;

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

            rt.anchoredPosition = new Vector2(165f, 87f);

            rt.sizeDelta = new Vector2(640f, 74f);

            var tmp = txtQuestT.GetComponent<TextMeshProUGUI>();

            tmp.alignment = TextAlignmentOptions.TopLeft;

            tmp.fontSize = 18f;

            tmp.enableAutoSizing = true;

            tmp.fontSizeMin = 13f;

            tmp.fontSizeMax = 18f;

            tmp.margin = new Vector4(6f, 2f, 6f, 2f);

            tmp.color = HexColor("#372016");

            tmp.textWrappingMode = TextWrappingModes.Normal;

            tmp.overflowMode = TextOverflowModes.Ellipsis;

            if (defaultFont != null) tmp.font = defaultFont;

        }



        string[] btnNames = { "Btn_Answer_A", "Btn_Answer_B", "Btn_Answer_C", "Btn_Answer_D" };

        float[] btnYs = { 7f, -49f, -105f, -161f };

        Button[] ansBtns = new Button[4];

        TextMeshProUGUI[] ansTexts = new TextMeshProUGUI[4];



        for (int i = 0; i < 4; i++)

        {

            Transform bT = cardContentT.Find(btnNames[i]);

            if (bT != null)

            {

                var rt = bT.GetComponent<RectTransform>();

                rt.anchorMin = new Vector2(0.5f, 0.5f);

                rt.anchorMax = new Vector2(0.5f, 0.5f);

                rt.pivot = new Vector2(0.5f, 0.5f);

                rt.anchoredPosition = new Vector2(165f, btnYs[i]);

                rt.sizeDelta = new Vector2(640f, 46f);

                var bImg = bT.GetComponent<Image>();

                if (bImg != null)

                {

                    bImg.color = HexColor("#4E342E");

                }

                var txt = bT.GetComponentInChildren<TextMeshProUGUI>();

                if (txt != null)

                {

                    txt.fontSize = 17f;

                    txt.enableAutoSizing = true;

                    txt.fontSizeMin = 10f;

                    txt.fontSizeMax = 17f;

                    txt.alignment = TextAlignmentOptions.MidlineLeft;

                    txt.margin = new Vector4(16f, 3f, 16f, 3f);

                    txt.textWrappingMode = TextWrappingModes.Normal;

                    txt.overflowMode = TextOverflowModes.Ellipsis;

                    if (defaultFont != null) txt.font = defaultFont;

                    ansTexts[i] = txt;



                    var txtRt = txt.GetComponent<RectTransform>();

                    if (txtRt != null)

                    {

                        txtRt.anchorMin = Vector2.zero;

                        txtRt.anchorMax = Vector2.one;

                        txtRt.pivot = new Vector2(0.5f, 0.5f);

                        txtRt.anchoredPosition = Vector2.zero;

                        txtRt.sizeDelta = Vector2.zero;

                    }

                }

                ansBtns[i] = bT.GetComponent<Button>();

            }

        }



        Transform txtFeedT = cardContentT.Find("Txt_Feedback");

        if (txtFeedT != null)

        {

            var rt = txtFeedT.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0.5f, 0.5f);

            rt.anchorMax = new Vector2(0.5f, 0.5f);

            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = new Vector2(165f, 40f);

            rt.sizeDelta = new Vector2(640f, 20f);

            var tmp = txtFeedT.GetComponent<TextMeshProUGUI>();

            tmp.alignment = TextAlignmentOptions.Center;

            tmp.fontSize = 17f;

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



        // 4. Load item sprites and wire references in L1_QuizManager

        Sprite sThitKho = LoadSpriteSubAsset("Assets/Sprites/Level1/Thit.png");

        Sprite sKimchi = LoadSpriteSubAsset("Assets/Sprites/Level1/Kimchi.png");

        Sprite sKimbap = LoadSpriteSubAsset("Assets/Sprites/Level1/Kimbap.png");

        Sprite sFlagVn = LoadSpriteSubAsset("Assets/Sprites/Level1/Flag_VN.png");

        Sprite sFlagKr = LoadSpriteSubAsset("Assets/Sprites/Level1/Flag_KR.png");

        Sprite sBanhMi = LoadSpriteSubAsset("Assets/Sprites/Level1/Icon_BanhMi.png");

        Sprite sTteokbokki = LoadSpriteSubAsset("Assets/Sprites/Level1/Icon_Tteokbokki.png");

        Sprite sCat = LoadSpriteSubAsset("Assets/Sprites/cat_question_mark.png");



        if (imgIll != null && imgIll.sprite == null)

        {

            imgIll.sprite = sThitKho;

        }



        SerializedObject soQm = new SerializedObject(qm);

        soQm.FindProperty("quizPanel").objectReferenceValue = quizModalT.gameObject;

        if (txtTitleT != null) soQm.FindProperty("txtTitle").objectReferenceValue = txtTitleT.GetComponent<TextMeshProUGUI>();

        if (txtQuestT != null) soQm.FindProperty("txtQuestion").objectReferenceValue = txtQuestT.GetComponent<TextMeshProUGUI>();

        if (txtFeedT != null) soQm.FindProperty("txtFeedback").objectReferenceValue = txtFeedT.GetComponent<TextMeshProUGUI>();

        if (btnCloseT != null) soQm.FindProperty("btnClose").objectReferenceValue = btnCloseT.GetComponent<Button>();



        SerializedProperty spAnsBtns = soQm.FindProperty("answerButtons");

        spAnsBtns.arraySize = 4;

        for (int i = 0; i < 4; i++) spAnsBtns.GetArrayElementAtIndex(i).objectReferenceValue = ansBtns[i];



        SerializedProperty spAnsTxts = soQm.FindProperty("answerTexts");

        spAnsTxts.arraySize = 4;

        for (int i = 0; i < 4; i++) spAnsTxts.GetArrayElementAtIndex(i).objectReferenceValue = ansTexts[i];



        soQm.FindProperty("illustrationContainer").objectReferenceValue = panelIllGo;

        soQm.FindProperty("imgIllustration").objectReferenceValue = imgIll;

        soQm.FindProperty("txtIllustrationName").objectReferenceValue = txtNameGo.GetComponent<TextMeshProUGUI>();



        soQm.FindProperty("thitKhoSprite").objectReferenceValue = sThitKho;

        soQm.FindProperty("kimchiSprite").objectReferenceValue = sKimchi;

        soQm.FindProperty("kimbapSprite").objectReferenceValue = sKimbap;

        soQm.FindProperty("flagVnSprite").objectReferenceValue = sFlagVn;

        soQm.FindProperty("flagKrSprite").objectReferenceValue = sFlagKr;

        soQm.FindProperty("banhMiSprite").objectReferenceValue = sBanhMi;

        soQm.FindProperty("tteokbokkiSprite").objectReferenceValue = sTteokbokki;

        soQm.FindProperty("catSprite").objectReferenceValue = sCat;

        soQm.ApplyModifiedPropertiesWithoutUndo();



        var invMgr = Object.FindFirstObjectByType<L1_InventoryManager>();

        if (invMgr != null)

        {

            SerializedObject soInv = new SerializedObject(invMgr);

            soInv.FindProperty("thitKhoIcon").objectReferenceValue = sThitKho;

            soInv.FindProperty("kimchiIcon").objectReferenceValue = sKimchi;

            soInv.FindProperty("kimbapIcon").objectReferenceValue = sKimbap;

            soInv.ApplyModifiedPropertiesWithoutUndo();

        }



        EditorSceneManager.MarkSceneDirty(scene);

        EditorSceneManager.SaveScene(scene);

        Debug.Log("[L1_SceneSetup] Successfully updated Quiz UI with Illustration in Level1_Prototype!");

    }



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



        Sprite quizCardSprite = null;

        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath("Assets/UI/Quiz.png"))

        {

            if (obj is Sprite s)

            {

                quizCardSprite = s;

                break;

            }

        }



        Sprite map1Bg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Map_1.png");

        Sprite plateSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Plate.png");

        Sprite iconThitKho = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Thit.png");

        Sprite iconKimchi = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Kimchi.png");

        Sprite iconKimbap = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Kimbap.png");

        Sprite iconFlagVn = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Flag_VN.png");

        Sprite iconFlagKr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Flag_KR.png");

        Sprite iconBanhMi = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Icon_BanhMi.png");

        Sprite iconTbk = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Level1/Icon_Tteokbokki.png");

        Sprite iconCat = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/cat_question_mark.png");



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

        // GROUP 4: GAMEPLAY OBJECTS

        // -------------------------------------------------------------

        // 1. Player (Hạ Ân)

        GameObject playerGo = CreateGameplayRoot(rootGameplay.transform, "Player", new Vector3(-2.2f, -1.0f, 0f),

            defaultFont, isPlayer: true, playerSprite: capsuleSprite);

        var playerCtrl = playerGo.AddComponent<L1_PlayerController>();



        Transform pVisual = playerGo.transform.Find("Visual_Sprite");

        if (pVisual != null && playerAnimCtrl != null)

        {

            var anim = pVisual.gameObject.AddComponent<Animator>();

            anim.runtimeAnimatorController = playerAnimCtrl;

        }



        // 2. NPC_Cat

        GameObject catGo = CreateGameplayRoot(rootGameplay.transform, "NPC_Cat", new Vector3(-1.8f, 1.8f, 0f), defaultFont);

        var cat = catGo.AddComponent<L1_NPC_Cat>();

        WireInteractableFields(cat, catGo);



        // 3. Dining Table

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



        // 4. Main Food Items

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



        // 5. Cultural Items

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



        // Bánh Mì on Kitchen Table

        GameObject banhMiGo = CreateGameplayRoot(rootGameplay.transform, "Interact_BanhMi", new Vector3(4.58f, -0.72f, 0f), defaultFont);

        var cultBm = banhMiGo.AddComponent<L1_CulturalItem>();

        SetSerializedEnum(cultBm, "culturalType", (int)L1_CulturalType.BanhMi);

        WireInteractableFields(cultBm, banhMiGo);



        // Tteokbokki on Kitchen Table

        GameObject tbkGo = CreateGameplayRoot(rootGameplay.transform, "Interact_Tteokbokki", new Vector3(8.59f, -0.68f, 0f), defaultFont);

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



        // Btn_Interact

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



        // 5. Panel_Quiz_Dialog (Fitted 2-Column Layout matching Map 2)

        GameObject quizModalGo = CreateUIPanel(rootCanvas.transform, "Panel_Quiz_Dialog",

            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),

            Vector2.zero, Vector2.zero,

            uiSprite, new Color(0f, 0f, 0f, 0.75f));



        GameObject quizCardGo = CreateUIPanel(quizModalGo.transform, "Card_Content",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            Vector2.zero, new Vector2(1100f, 500f),

            quizCardSprite != null ? quizCardSprite : uiSprite, Color.white);

        var cardImg = quizCardGo.GetComponent<Image>();

        if (cardImg != null)

        {

            cardImg.type = Image.Type.Sliced;

            cardImg.preserveAspect = false;

        }



        // Close Button

        GameObject btnCloseGo = CreateUIButton(quizCardGo.transform, "Btn_Close",

            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),

            new Vector2(-55f, -55f), new Vector2(44f, 44f),

            uiSprite, HexColor("#D32F2F"),

            "X", 24, FontStyles.Bold, Color.white, defaultFont);



        // --- LEFT COLUMN: ILLUSTRATION PANEL ---

        GameObject panelIllustrationGo = CreateUIPanel(quizCardGo.transform, "Panel_Illustration",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            new Vector2(-330f, -30f), new Vector2(310f, 308f),

            uiSprite, HexColor("#141820"));



        GameObject imgIllustrationGo = CreateUIPanel(panelIllustrationGo.transform, "Img_Illustration",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, 18f), new Vector2(240f, 240f),

            null, Color.white);

        var imgIllustration = imgIllustrationGo.GetComponent<Image>();

        imgIllustration.preserveAspect = true;



        GameObject txtNameGo = CreateUIText(panelIllustrationGo.transform, "Txt_Illustration_Name",

            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),

            new Vector2(0f, 10f), new Vector2(280f, 28f),

            "", 19, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



        // --- RIGHT COLUMN: QUESTION & CHOICES ---

        // Title

        GameObject txtTitleGo = CreateUIText(quizCardGo.transform, "Txt_Title",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            new Vector2(0f, 195f), new Vector2(800f, 42f),

            "CÂU HỎI", 24, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



        // Question

        GameObject txtQuestContentGo = CreateUIText(quizCardGo.transform, "Txt_Question",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            new Vector2(165f, 87f), new Vector2(640f, 74f),

            "Nội dung câu hỏi...", 18, FontStyles.Normal, HexColor("#372016"), TextAlignmentOptions.TopLeft, defaultFont);

        var txtQuestionTmp = txtQuestContentGo.GetComponent<TextMeshProUGUI>();

        txtQuestionTmp.margin = new Vector4(6f, 2f, 6f, 2f);

        txtQuestionTmp.enableAutoSizing = true;

        txtQuestionTmp.fontSizeMin = 13f;

        txtQuestionTmp.fontSizeMax = 18f;

        txtQuestionTmp.textWrappingMode = TextWrappingModes.Normal;

        txtQuestionTmp.overflowMode = TextOverflowModes.Ellipsis;



        // Feedback Text

        GameObject txtFeedGo = CreateUIText(quizCardGo.transform, "Txt_Feedback",

            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

            new Vector2(165f, 40f), new Vector2(640f, 20f),

            "", 17, FontStyles.Bold, HexColor("#4CAF50"), TextAlignmentOptions.Center, defaultFont);



        // 4 Answer Buttons

        Button[] ansBtns = new Button[4];

        TextMeshProUGUI[] ansTexts = new TextMeshProUGUI[4];

        string[] btnNames = { "Btn_Answer_A", "Btn_Answer_B", "Btn_Answer_C", "Btn_Answer_D" };

        float[] btnYs = { 7f, -49f, -105f, -161f };



        for (int i = 0; i < 4; i++)

        {

            GameObject aBtnGo = CreateUIButton(quizCardGo.transform, btnNames[i],

                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),

                new Vector2(165f, btnYs[i]), new Vector2(640f, 46f),

                uiSprite, HexColor("#4E342E"),

                $"Phương án {i + 1}", 17, FontStyles.Normal, Color.white, defaultFont);



            ansBtns[i] = aBtnGo.GetComponent<Button>();

            ansTexts[i] = aBtnGo.GetComponentInChildren<TextMeshProUGUI>();

            ansTexts[i].alignment = TextAlignmentOptions.MidlineLeft;

            ansTexts[i].margin = new Vector4(16f, 3f, 16f, 3f);

            ansTexts[i].enableAutoSizing = true;

            ansTexts[i].fontSizeMin = 10f;

            ansTexts[i].fontSizeMax = 17f;

            ansTexts[i].textWrappingMode = TextWrappingModes.Normal;

            ansTexts[i].overflowMode = TextOverflowModes.Ellipsis;

        }



        quizModalGo.SetActive(false);



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
            "HOÀN THÀNH MÀN 1: BỮA CƠM GIA ĐÌNH VIỆT - HÀN!", 26, FontStyles.Bold, HexColor("#372016"), TextAlignmentOptions.Center, defaultFont);

        CreateUIText(victoryCardGo.transform, "Txt_Victory_Desc",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -32f), new Vector2(680f, 85f),
            "Bữa cơm gia đình Việt - Hàn đã sẵn sàng! Bạn đã hoàn thành Màn 1!", 21, FontStyles.Bold, HexColor("#4E342E"), TextAlignmentOptions.Center, defaultFont);

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

        soGm.FindProperty("arrowThitKho").objectReferenceValue = thitKhoGo.transform.Find("Arrow_Indicator")?.gameObject;

        soGm.FindProperty("arrowKimchi").objectReferenceValue = kimchiGo.transform.Find("Arrow_Indicator")?.gameObject;

        soGm.FindProperty("arrowKimbap").objectReferenceValue = kimbapGo.transform.Find("Arrow_Indicator")?.gameObject;

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



        soQm.FindProperty("illustrationContainer").objectReferenceValue = panelIllustrationGo;

        soQm.FindProperty("imgIllustration").objectReferenceValue = imgIllustration;

        soQm.FindProperty("txtIllustrationName").objectReferenceValue = txtNameGo.GetComponent<TextMeshProUGUI>();



        soQm.FindProperty("thitKhoSprite").objectReferenceValue = iconThitKho;

        soQm.FindProperty("kimchiSprite").objectReferenceValue = iconKimchi;

        soQm.FindProperty("kimbapSprite").objectReferenceValue = iconKimbap;

        soQm.FindProperty("flagVnSprite").objectReferenceValue = iconFlagVn;

        soQm.FindProperty("flagKrSprite").objectReferenceValue = iconFlagKr;

        soQm.FindProperty("banhMiSprite").objectReferenceValue = iconBanhMi;

        soQm.FindProperty("tteokbokkiSprite").objectReferenceValue = iconTbk;

        soQm.FindProperty("catSprite").objectReferenceValue = iconCat;

        soQm.ApplyModifiedPropertiesWithoutUndo();



        // Wire L1_InventoryManager

        SerializedObject soIm = new SerializedObject(im);

        SerializedProperty spSlots = soIm.FindProperty("slotIcons");

        spSlots.arraySize = 5;

        for (int i = 0; i < 5; i++) spSlots.GetArrayElementAtIndex(i).objectReferenceValue = invSlotIcons[i];

        soIm.FindProperty("thitKhoIcon").objectReferenceValue = iconThitKho;

        soIm.FindProperty("kimchiIcon").objectReferenceValue = iconKimchi;

        soIm.FindProperty("kimbapIcon").objectReferenceValue = iconKimbap;

        soIm.ApplyModifiedPropertiesWithoutUndo();



        // Ensure all Text components use the Determination font

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



        Transform visual = root.transform.Find("Visual_Sprite");

        if (visual != null && interactable is L1_ItemPickup itemPickup)

        {

            SetSerializedObjectReference(itemPickup, "visualSprite", visual.GetComponent<SpriteRenderer>());

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



    [MenuItem("Tools/Map 1/Revert Living Room Food Pickups And Restore Kitchen Cultural Items")]

    public static void RevertLivingRoomFoodAndRestoreKitchenCulturalItems()

    {

        Debug.Log("[L1_SceneSetup] Reverting Living Room Food Pickups & Restoring Kitchen Cultural Items...");

        if (EditorApplication.isPlaying || EditorApplication.isCompiling)

        {

            Debug.LogWarning("[L1_SceneSetup] Cannot update scene while playing or compiling.");

            return;

        }



        string scenePath = ScenePath;

        var scene = EditorSceneManager.GetActiveScene();

        if (scene.path != scenePath)

        {

            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        }



        TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.asset");

        if (defaultFont == null) defaultFont = TMP_Settings.defaultFontAsset;



        GameObject rootGameplay = GameObject.Find("--- GAMEPLAY OBJECTS ---");

        Transform parentTransform = rootGameplay != null ? rootGameplay.transform : null;



        // 1. Delete Kitchen_Display_Table if present

        GameObject kitchenTableGo = GameObject.Find("Kitchen_Display_Table");

        if (kitchenTableGo != null)

        {

            Object.DestroyImmediate(kitchenTableGo);

            Debug.Log("[L1_SceneSetup] Destroyed Kitchen_Display_Table");

        }



        // 2. Restore Interact_BanhMi on the kitchen table (x: 4.58, y: -0.72)

        GameObject banhMiGo = GameObject.Find("Interact_BanhMi");

        if (banhMiGo == null)

        {

            banhMiGo = new GameObject("Interact_BanhMi");

            if (parentTransform != null) banhMiGo.transform.SetParent(parentTransform, false);

        }

        banhMiGo.transform.position = new Vector3(4.58f, -0.72f, 0f);

        banhMiGo.transform.localScale = Vector3.one;



        // Remove L1_ItemPickup if present

        var pickupBm = banhMiGo.GetComponent<L1_ItemPickup>();

        if (pickupBm != null) Object.DestroyImmediate(pickupBm);



        // Remove Visual_Sprite & Arrow_Indicator children

        Transform bmVisual = banhMiGo.transform.Find("Visual_Sprite");

        if (bmVisual != null) Object.DestroyImmediate(bmVisual.gameObject);

        Transform bmArrow = banhMiGo.transform.Find("Arrow_Indicator");

        if (bmArrow != null) Object.DestroyImmediate(bmArrow.gameObject);



        // Ensure BoxCollider2D

        var colBm = banhMiGo.GetComponent<BoxCollider2D>();

        if (colBm == null) colBm = banhMiGo.AddComponent<BoxCollider2D>();

        colBm.isTrigger = true;

        colBm.size = new Vector2(1.8f, 1.8f);

        colBm.offset = Vector2.zero;



        // Ensure Interact_Indicator

        Transform bmIndT = banhMiGo.transform.Find("Interact_Indicator");

        GameObject bmIndGo;

        if (bmIndT == null)

        {

            bmIndGo = new GameObject("Interact_Indicator");

            bmIndGo.transform.SetParent(banhMiGo.transform, false);

            bmIndGo.transform.localPosition = new Vector3(0f, 1.35f, 0f);

            var tmp = bmIndGo.AddComponent<TextMeshPro>();

            if (defaultFont != null) tmp.font = defaultFont;

            tmp.text = "[!]";

            tmp.fontSize = 3.6f;

            tmp.fontStyle = FontStyles.Bold;

            tmp.alignment = TextAlignmentOptions.Center;

            tmp.color = HexColor("#FFD54F");

            tmp.sortingOrder = 12;

            bmIndGo.SetActive(false);

        }

        else

        {

            bmIndGo = bmIndT.gameObject;

        }



        // Ensure L1_CulturalItem

        var cultBm = banhMiGo.GetComponent<L1_CulturalItem>();

        if (cultBm == null) cultBm = banhMiGo.AddComponent<L1_CulturalItem>();

        SetSerializedEnum(cultBm, "culturalType", (int)L1_CulturalType.BanhMi);

        SetSerializedObjectReference(cultBm, "interactIndicator", bmIndGo);



        // 3. Restore Interact_Tteokbokki on the kitchen table (x: 8.59, y: -0.68)

        GameObject tbkGo = GameObject.Find("Interact_Tteokbokki");

        if (tbkGo == null)

        {

            tbkGo = new GameObject("Interact_Tteokbokki");

            if (parentTransform != null) tbkGo.transform.SetParent(parentTransform, false);

        }

        tbkGo.transform.position = new Vector3(8.59f, -0.68f, 0f);

        tbkGo.transform.localScale = Vector3.one;



        // Remove L1_ItemPickup if present

        var pickupTbk = tbkGo.GetComponent<L1_ItemPickup>();

        if (pickupTbk != null) Object.DestroyImmediate(pickupTbk);



        // Remove Visual_Sprite & Arrow_Indicator children

        Transform tbkVisual = tbkGo.transform.Find("Visual_Sprite");

        if (tbkVisual != null) Object.DestroyImmediate(tbkVisual.gameObject);

        Transform tbkArrow = tbkGo.transform.Find("Arrow_Indicator");

        if (tbkArrow != null) Object.DestroyImmediate(tbkArrow.gameObject);



        // Ensure BoxCollider2D

        var colTbk = tbkGo.GetComponent<BoxCollider2D>();

        if (colTbk == null) colTbk = tbkGo.AddComponent<BoxCollider2D>();

        colTbk.isTrigger = true;

        colTbk.size = new Vector2(1.8f, 1.8f);

        colTbk.offset = Vector2.zero;



        // Ensure Interact_Indicator

        Transform tbkIndT = tbkGo.transform.Find("Interact_Indicator");

        GameObject tbkIndGo;

        if (tbkIndT == null)

        {

            tbkIndGo = new GameObject("Interact_Indicator");

            tbkIndGo.transform.SetParent(tbkGo.transform, false);

            tbkIndGo.transform.localPosition = new Vector3(0f, 1.35f, 0f);

            var tmp = tbkIndGo.AddComponent<TextMeshPro>();

            if (defaultFont != null) tmp.font = defaultFont;

            tmp.text = "[!]";

            tmp.fontSize = 3.6f;

            tmp.fontStyle = FontStyles.Bold;

            tmp.alignment = TextAlignmentOptions.Center;

            tmp.color = HexColor("#FFD54F");

            tmp.sortingOrder = 12;

            tbkIndGo.SetActive(false);

        }

        else

        {

            tbkIndGo = tbkIndT.gameObject;

        }



        // Ensure L1_CulturalItem

        var cultTbk = tbkGo.GetComponent<L1_CulturalItem>();

        if (cultTbk == null) cultTbk = tbkGo.AddComponent<L1_CulturalItem>();

        SetSerializedEnum(cultTbk, "culturalType", (int)L1_CulturalType.Tteokbokki);

        SetSerializedObjectReference(cultTbk, "interactIndicator", tbkIndGo);



        EditorSceneManager.MarkSceneDirty(scene);

        EditorSceneManager.SaveScene(scene);

        Debug.Log("[L1_SceneSetup] Successfully reverted Living Room Food Pickups & Restored Kitchen Cultural Items!");

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

            "Chơi lại màn 1", 22, FontStyles.Bold, HexColor("#FFE082"), defaultFont);



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

            "HƯỚNG DẪN CHƠI - BẢN ĐỒ 1", 28, FontStyles.Bold, HexColor("#FFD54F"), TextAlignmentOptions.Center, defaultFont);



        string guideText =

            "<color=#7C2D12><b>1. HỎI GỢI Ý TỪ MÈO:</b></color>\n" +

            "Gặp và trò chuyện với <b>Chú Mèo</b> để nhận gợi ý chuẩn bị bữa cơm gia đình và mở khóa các mũi tên chỉ dẫn.\n\n" +

            "<color=#7C2D12><b>2. THU THẬP ĐỦ 3 MÓN ĂN CHÍNH:</b></color>\n" +

            "• <b>Thịt kho trứng</b> (Việt Nam)    • <b>Kim chi</b> (Hàn Quốc)    • <b>Kimbap</b> (Hàn Quốc)\n\n" +

            "<color=#7C2D12><b>3. BÀY LÊN BÀN ĂN:</b></color>\n" +

            "Mang các món ăn đến bàn ăn gia đình, bấm <b>TƯƠNG TÁC</b> để bày đủ 3 món lên 3 chiếc đĩa.\n\n" +

            "<color=#7C2D12><b>4. KHÁM PHÁ VĂN HÓA ẨM THỰC:</b></color>\n" +

            "Tương tác với <b>Bánh mì</b> và <b>Tteokbokki</b> trên bàn bếp để hoàn thành nét đẹp giao lưu ẩm thực Việt - Hàn.\n\n" +

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



        // 6. Add or Wire L1_GameHUDController

        var hudCtrl = rootCanvas.GetComponent<L1_GameHUDController>();

        if (hudCtrl == null) hudCtrl = rootCanvas.gameObject.AddComponent<L1_GameHUDController>();



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





}
