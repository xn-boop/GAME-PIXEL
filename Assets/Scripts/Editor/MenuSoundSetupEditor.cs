using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Script Editor tự động thiết lập và gắn Icon âm thanh góc trên bên phải (Top-Right Sound Button)
/// và bảng cài đặt âm thanh (Panel_Sound) vào 4 scenes menu theo chuẩn giao diện Map 1:
/// 1. Assets/Scenes/MainMenu.unity
/// 2. Assets/Scenes/LoginMenu.unity
/// 3. Assets/Scenes/RegisterMenu.unity
/// 4. Assets/Scenes/Chọn map.unity
/// </summary>
[InitializeOnLoad]
public static class MenuSoundSetupEditor
{
    private static readonly string[] MenuScenePaths = new string[]
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/LoginMenu.unity",
        "Assets/Scenes/RegisterMenu.unity",
        "Assets/Scenes/Chọn map.unity"
    };

    private static int updateTicks = 0;
    private static bool isProcessing = false;

    static MenuSoundSetupEditor()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    private static void OnEditorUpdate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        if (isProcessing) return;

        updateTicks++;
        // Kiểm tra định kỳ mỗi 15 ticks (~0.25 giây)
        if (updateTicks % 15 != 0) return;

        bool needsSetup = false;
        foreach (string scenePath in MenuScenePaths)
        {
            if (File.Exists(scenePath))
            {
                try
                {
                    string content = File.ReadAllText(scenePath);
                    if (!content.Contains("Btn_Sound"))
                    {
                        needsSetup = true;
                        break;
                    }
                }
                catch { }
            }
        }

        // Kiểm tra thêm active scene hiện tại nếu là 1 trong 4 scene
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.isLoaded && IsMenuScene(activeScene.path))
        {
            if (activeScene.GetRootGameObjects().Length > 0)
            {
                Canvas canvas = FindCanvasInScene(activeScene);
                if (canvas != null && canvas.transform.Find("Btn_Sound") == null)
                {
                    needsSetup = true;
                }
            }
        }

        if (needsSetup)
        {
            ExecuteFullSetup();
        }
    }

    private static bool IsMenuScene(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        foreach (string p in MenuScenePaths)
        {
            if (string.Equals(p, path, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    [MenuItem("Tools/Setup Top-Right Sound Button In All Menu Scenes")]
    public static void ForceSetupAllMenuScenes()
    {
        ExecuteFullSetup();
    }

    public static void ExecuteFullSetup()
    {
        if (isProcessing) return;
        isProcessing = true;

        try
        {
            Sprite soundOn = LoadSpriteSafe("Assets/UI/Icons/Icon_Sound_On.png");
            Sprite soundOff = LoadSpriteSafe("Assets/UI/Icons/Icon_Sound_Off.png");
            Sprite btnWood = LoadSpriteSafe("Assets/UI/Icons/Btn_Wood_Square.png");
            Sprite quizCard = LoadSpriteSafe("Assets/UI/Quiz.png");
            Font gameFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/SVN-Determination Sans.otf");

            Scene activeScene = SceneManager.GetActiveScene();
            string activePath = activeScene.path;

            // 1. Xử lý active scene trước nếu nó là scene menu
            if (activeScene.isLoaded && IsMenuScene(activePath))
            {
                SetupScene(activeScene, soundOn, soundOff, btnWood, quizCard, gameFont);
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                Debug.Log($"[MenuSoundSetupEditor] Đã gắn icon âm thanh vào active scene: {activeScene.name}");
            }

            // 2. Xử lý các scene còn lại chưa có Btn_Sound
            foreach (string scenePath in MenuScenePaths)
            {
                if (!File.Exists(scenePath)) continue;
                if (string.Equals(scenePath, activePath, StringComparison.OrdinalIgnoreCase)) continue;

                try
                {
                    string content = File.ReadAllText(scenePath);
                    if (content.Contains("Btn_Sound")) continue; // Đã có rồi
                }
                catch { }

                try
                {
                    Scene addScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                    if (addScene.isLoaded)
                    {
                        SetupScene(addScene, soundOn, soundOff, btnWood, quizCard, gameFont);
                        EditorSceneManager.MarkSceneDirty(addScene);
                        EditorSceneManager.SaveScene(addScene);
                        EditorSceneManager.CloseScene(addScene, true);
                        Debug.Log($"[MenuSoundSetupEditor] Đã thiết lập icon âm thanh và lưu thành công cho scene: {scenePath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MenuSoundSetupEditor] Lỗi khi xử lý scene {scenePath}: {ex.Message}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MenuSoundSetupEditor] HOÀN TẤT: Cả 4 scenes menu đã có icon âm thanh góc trên bên phải chuẩn Map 1!");
        }
        finally
        {
            isProcessing = false;
        }
    }

    private static Canvas FindCanvasInScene(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Canvas c = root.GetComponentInChildren<Canvas>(true);
            if (c != null) return c;
        }
        return null;
    }

    private static void SetupScene(Scene scene, Sprite soundOn, Sprite soundOff, Sprite btnWood, Sprite quizCard, Font gameFont)
    {
        Canvas canvas = FindCanvasInScene(scene);
        if (canvas == null)
        {
            Debug.LogError($"[MenuSoundSetupEditor] Không tìm thấy Canvas trong scene: {scene.name}");
            return;
        }

        // 1. Tạo hoặc lấy Btn_Sound
        Button btnSound = null;
        Image imgSoundIcon = null;
        Transform existingBtn = canvas.transform.Find("Btn_Sound");
        if (existingBtn != null)
        {
            btnSound = existingBtn.GetComponent<Button>();
            imgSoundIcon = existingBtn.Find("Icon")?.GetComponent<Image>();
        }
        else
        {
            GameObject btnObj = new GameObject("Btn_Sound", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(canvas.transform, false);

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-30f, -30f);
            rt.sizeDelta = new Vector2(54f, 54f);

            Image bgImage = btnObj.GetComponent<Image>();
            bgImage.sprite = btnWood;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Color.white;

            btnSound = btnObj.GetComponent<Button>();
            var colors = btnSound.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 0.85f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btnSound.colors = colors;

            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(btnObj.transform, false);

            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(34f, 34f);

            imgSoundIcon = iconObj.GetComponent<Image>();
            imgSoundIcon.sprite = soundOn;
            imgSoundIcon.preserveAspect = true;
            imgSoundIcon.raycastTarget = false;
            imgSoundIcon.color = Color.white;
        }

        // 2. Tạo hoặc lấy Panel_Sound
        GameObject panelObj = null;
        Transform existingPanel = canvas.transform.Find("Panel_Sound");
        if (existingPanel != null)
        {
            panelObj = existingPanel.gameObject;
        }
        else
        {
            panelObj = CreatePanelSound(canvas.transform, btnWood, quizCard, gameFont);
        }

        // Đảm bảo thứ tự hiển thị: Btn_Sound và Panel_Sound luôn render trên cùng
        btnSound.transform.SetAsLastSibling();
        panelObj.transform.SetAsLastSibling();

        // 3. Tìm Scene Manager để gắn MenuSoundController
        GameObject managerGo = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == "MainMenuManager" || root.name == "LoginMenuManager" ||
                root.name == "RegisterMenuManager" || root.name == "ChonMapMenuManager")
            {
                managerGo = root;
                break;
            }
        }
        if (managerGo == null)
        {
            managerGo = canvas.gameObject;
        }

        MenuSoundController controller = managerGo.GetComponent<MenuSoundController>();
        if (controller == null)
        {
            controller = managerGo.AddComponent<MenuSoundController>();
        }

        // 4. Gán SerializedProperties vào controller
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("btnSound").objectReferenceValue = btnSound;
        so.FindProperty("imgSoundIcon").objectReferenceValue = imgSoundIcon;
        so.FindProperty("soundOnSprite").objectReferenceValue = soundOn;
        so.FindProperty("soundOffSprite").objectReferenceValue = soundOff;
        so.FindProperty("btnWoodSprite").objectReferenceValue = btnWood;
        so.FindProperty("panelSound").objectReferenceValue = panelObj;

        Transform card = panelObj.transform.Find("Card_Sound");
        if (card != null)
        {
            Button muteBtn = card.Find("Btn_ToggleSound")?.GetComponent<Button>();
            Text muteTxt = card.Find("Btn_ToggleSound/Text")?.GetComponent<Text>();
            Slider musSld = card.Find("Volume_Group/Slider_Music")?.GetComponent<Slider>();
            Slider sfxSld = card.Find("Sfx_Group/Slider_Sfx")?.GetComponent<Slider>();
            Text musPct = card.Find("Volume_Group/Txt_MusicPercent")?.GetComponent<Text>();
            Text sfxPct = card.Find("Sfx_Group/Txt_SfxPercent")?.GetComponent<Text>();
            Button btnClose = card.Find("Btn_Close")?.GetComponent<Button>();
            Button btnCloseX = card.Find("Btn_Close_X")?.GetComponent<Button>();

            so.FindProperty("btnToggleMute").objectReferenceValue = muteBtn;
            so.FindProperty("txtMuteStatus").objectReferenceValue = muteTxt;
            so.FindProperty("musicVolumeSlider").objectReferenceValue = musSld;
            so.FindProperty("sfxVolumeSlider").objectReferenceValue = sfxSld;
            so.FindProperty("txtMusicPercent").objectReferenceValue = musPct;
            so.FindProperty("txtSfxPercent").objectReferenceValue = sfxPct;
            so.FindProperty("btnClose").objectReferenceValue = btnClose;
            so.FindProperty("btnCloseX").objectReferenceValue = btnCloseX;

            // Wire persistent listeners
            if (btnSound != null)
            {
                UnityEventTools.RemovePersistentListener(btnSound.onClick, controller.OnSoundButtonClicked);
                UnityEventTools.AddPersistentListener(btnSound.onClick, controller.OnSoundButtonClicked);
            }
            if (btnClose != null)
            {
                UnityEventTools.RemovePersistentListener(btnClose.onClick, controller.CloseSoundModal);
                UnityEventTools.AddPersistentListener(btnClose.onClick, controller.CloseSoundModal);
            }
            if (btnCloseX != null)
            {
                UnityEventTools.RemovePersistentListener(btnCloseX.onClick, controller.CloseSoundModal);
                UnityEventTools.AddPersistentListener(btnCloseX.onClick, controller.CloseSoundModal);
            }
            if (muteBtn != null)
            {
                UnityEventTools.RemovePersistentListener(muteBtn.onClick, controller.ToggleMute);
                UnityEventTools.AddPersistentListener(muteBtn.onClick, controller.ToggleMute);
            }
        }

        so.ApplyModifiedProperties();
    }

    private static GameObject CreatePanelSound(Transform canvasTr, Sprite btnWood, Sprite quizCard, Font gameFont)
    {
        if (gameFont == null)
        {
            Text sample = UnityEngine.Object.FindAnyObjectByType<Text>();
            if (sample != null) gameFont = sample.font;
            if (gameFont == null) gameFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        // Fullscreen Overlay
        GameObject panelObj = new GameObject("Panel_Sound", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        panelObj.transform.SetParent(canvasTr, false);

        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = Vector2.zero;
        panelRt.sizeDelta = Vector2.zero;

        Image panelImg = panelObj.GetComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.72f);
        panelImg.raycastTarget = true;

        // Card_Sound
        GameObject cardObj = new GameObject("Card_Sound", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        cardObj.transform.SetParent(panelObj.transform, false);

        RectTransform cardRt = cardObj.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.anchoredPosition = Vector2.zero;
        cardRt.sizeDelta = new Vector2(580f, 410f);

        Image cardImg = cardObj.GetComponent<Image>();
        if (quizCard != null)
        {
            cardImg.sprite = quizCard;
            cardImg.type = Image.Type.Sliced;
        }
        else
        {
            cardImg.color = new Color(0.18f, 0.11f, 0.08f, 0.98f);
        }

        // Txt_Title
        GameObject titleObj = new GameObject("Txt_Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        titleObj.transform.SetParent(cardObj.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -32f);
        titleRt.sizeDelta = new Vector2(460f, 48f);

        Text titleText = titleObj.GetComponent<Text>();
        titleText.font = gameFont;
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.98f, 0.88f, 0.65f, 1f);
        titleText.text = "CÀI ĐẶT ÂM THANH";

        // Btn_ToggleSound
        GameObject muteBtnObj = new GameObject("Btn_ToggleSound", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        muteBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform muteRt = muteBtnObj.GetComponent<RectTransform>();
        muteRt.anchorMin = new Vector2(0.5f, 1f);
        muteRt.anchorMax = new Vector2(0.5f, 1f);
        muteRt.pivot = new Vector2(0.5f, 1f);
        muteRt.anchoredPosition = new Vector2(0f, -92f);
        muteRt.sizeDelta = new Vector2(380f, 50f);

        Image muteImg = muteBtnObj.GetComponent<Image>();
        muteImg.sprite = btnWood;
        muteImg.type = Image.Type.Sliced;
        muteImg.color = Color.white;

        GameObject muteTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        muteTextObj.transform.SetParent(muteBtnObj.transform, false);
        RectTransform muteTextRt = muteTextObj.GetComponent<RectTransform>();
        muteTextRt.anchorMin = Vector2.zero;
        muteTextRt.anchorMax = Vector2.one;
        muteTextRt.sizeDelta = Vector2.zero;
        Text muteText = muteTextObj.GetComponent<Text>();
        muteText.font = gameFont;
        muteText.fontSize = 22;
        muteText.fontStyle = FontStyle.Bold;
        muteText.alignment = TextAnchor.MiddleCenter;
        muteText.text = "Âm thanh: ĐANG BẬT";
        muteText.color = new Color(0.35f, 0.85f, 0.45f, 1f);

        // Volume_Group
        CreateSliderRow(cardObj.transform, "Volume_Group", "Nhạc nền (BGM):", -160f, gameFont);

        // Sfx_Group
        CreateSliderRow(cardObj.transform, "Sfx_Group", "Hiệu ứng (SFX):", -240f, gameFont);

        // Btn_Close
        GameObject closeBtnObj = new GameObject("Btn_Close", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform closeRt = closeBtnObj.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(0.5f, 0f);
        closeRt.anchorMax = new Vector2(0.5f, 0f);
        closeRt.pivot = new Vector2(0.5f, 0f);
        closeRt.anchoredPosition = new Vector2(0f, 28f);
        closeRt.sizeDelta = new Vector2(200f, 48f);

        Image closeImg = closeBtnObj.GetComponent<Image>();
        closeImg.sprite = btnWood;
        closeImg.type = Image.Type.Sliced;
        closeImg.color = Color.white;

        GameObject closeTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTextRt = closeTextObj.GetComponent<RectTransform>();
        closeTextRt.anchorMin = Vector2.zero;
        closeTextRt.anchorMax = Vector2.one;
        closeTextRt.sizeDelta = Vector2.zero;
        Text closeText = closeTextObj.GetComponent<Text>();
        closeText.font = gameFont;
        closeText.fontSize = 22;
        closeText.fontStyle = FontStyle.Bold;
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.text = "ĐÓNG";
        closeText.color = Color.white;

        // Btn_Close_X
        GameObject xBtnObj = new GameObject("Btn_Close_X", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        xBtnObj.transform.SetParent(cardObj.transform, false);
        RectTransform xRt = xBtnObj.GetComponent<RectTransform>();
        xRt.anchorMin = new Vector2(1f, 1f);
        xRt.anchorMax = new Vector2(1f, 1f);
        xRt.pivot = new Vector2(1f, 1f);
        xRt.anchoredPosition = new Vector2(-20f, -20f);
        xRt.sizeDelta = new Vector2(40f, 40f);

        Image xImg = xBtnObj.GetComponent<Image>();
        xImg.sprite = btnWood;
        xImg.type = Image.Type.Sliced;
        xImg.color = Color.white;

        GameObject xTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        xTextObj.transform.SetParent(xBtnObj.transform, false);
        RectTransform xTextRt = xTextObj.GetComponent<RectTransform>();
        xTextRt.anchorMin = Vector2.zero;
        xTextRt.anchorMax = Vector2.one;
        xTextRt.sizeDelta = Vector2.zero;
        Text xText = xTextObj.GetComponent<Text>();
        xText.font = gameFont;
        xText.fontSize = 22;
        xText.fontStyle = FontStyle.Bold;
        xText.alignment = TextAnchor.MiddleCenter;
        xText.text = "✕";
        xText.color = new Color(0.95f, 0.85f, 0.75f, 1f);

        panelObj.SetActive(false);
        return panelObj;
    }

    private static void CreateSliderRow(Transform parent, string groupName, string label, float yPos, Font font)
    {
        GameObject group = new GameObject(groupName, typeof(RectTransform));
        group.transform.SetParent(parent, false);
        RectTransform grpRt = group.GetComponent<RectTransform>();
        grpRt.anchorMin = new Vector2(0.5f, 1f);
        grpRt.anchorMax = new Vector2(0.5f, 1f);
        grpRt.pivot = new Vector2(0.5f, 1f);
        grpRt.anchoredPosition = new Vector2(0f, yPos);
        grpRt.sizeDelta = new Vector2(460f, 65f);

        // Label
        GameObject lblObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        lblObj.transform.SetParent(group.transform, false);
        RectTransform lblRt = lblObj.GetComponent<RectTransform>();
        lblRt.anchorMin = new Vector2(0f, 1f);
        lblRt.anchorMax = new Vector2(0.7f, 1f);
        lblRt.pivot = new Vector2(0f, 1f);
        lblRt.anchoredPosition = Vector2.zero;
        lblRt.sizeDelta = new Vector2(0f, 26f);
        Text lbl = lblObj.GetComponent<Text>();
        lbl.font = font;
        lbl.fontSize = 20;
        lbl.fontStyle = FontStyle.Bold;
        lbl.alignment = TextAnchor.MiddleLeft;
        lbl.color = new Color(0.92f, 0.85f, 0.75f, 1f);
        lbl.text = label;

        // Percent
        GameObject pctObj = new GameObject(groupName == "Volume_Group" ? "Txt_MusicPercent" : "Txt_SfxPercent",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        pctObj.transform.SetParent(group.transform, false);
        RectTransform pctRt = pctObj.GetComponent<RectTransform>();
        pctRt.anchorMin = new Vector2(0.7f, 1f);
        pctRt.anchorMax = new Vector2(1f, 1f);
        pctRt.pivot = new Vector2(1f, 1f);
        pctRt.anchoredPosition = Vector2.zero;
        pctRt.sizeDelta = new Vector2(0f, 26f);
        Text percentText = pctObj.GetComponent<Text>();
        percentText.font = font;
        percentText.fontSize = 20;
        percentText.fontStyle = FontStyle.Bold;
        percentText.alignment = TextAnchor.MiddleRight;
        percentText.color = new Color(1f, 0.9f, 0.5f, 1f);
        percentText.text = "100%";

        // Slider
        string sliderName = groupName == "Volume_Group" ? "Slider_Music" : "Slider_Sfx";
        GameObject sliderObj = new GameObject(sliderName, typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(group.transform, false);
        RectTransform sliderRt = sliderObj.GetComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0f, 0f);
        sliderRt.anchorMax = new Vector2(1f, 0f);
        sliderRt.pivot = new Vector2(0.5f, 0f);
        sliderRt.anchoredPosition = new Vector2(0f, 4f);
        sliderRt.sizeDelta = new Vector2(0f, 26f);

        Slider slider = sliderObj.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;

        // Background
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = new Color(0.12f, 0.08f, 0.06f, 0.95f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRt.sizeDelta = new Vector2(-12f, 0f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.sizeDelta = Vector2.zero;
        Image fillImg = fill.GetComponent<Image>();
        fillImg.color = new Color(0.96f, 0.65f, 0.22f, 1f);
        slider.fillRect = fillRt;

        // Handle Area
        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRt = handleArea.GetComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.sizeDelta = new Vector2(-16f, 0f);

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(24f, 24f);
        Image handleImg = handle.GetComponent<Image>();
        handleImg.color = new Color(1f, 0.95f, 0.85f, 1f);
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
    }

    private static Sprite LoadSpriteSafe(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;

        UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
        if (allAssets != null)
        {
            foreach (var asset in allAssets)
            {
                if (asset is Sprite s) return s;
            }
        }

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex != null)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        return null;
    }
}
