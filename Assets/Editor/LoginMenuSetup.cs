using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LoginMenuSetup
{
    [MenuItem("Tools/Build LoginMenu Scene")]
    public static void BuildScene()
    {
        Debug.Log("[LoginMenuSetup] Bắt đầu khởi tạo Scene LoginMenu...");

        // 1. Tạo Scene mới
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        string scenePath = "Assets/Scenes/LoginMenu.unity";

        // 2. Tạo Main Camera
        GameObject cameraObj = new GameObject("Main Camera");
        Camera cam = cameraObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 1000f;
        cameraObj.AddComponent<AudioListener>();
        cameraObj.tag = "MainCamera";

        // 3. Tạo EventSystem duy nhất (tránh tạo nhiều EventSystem)
        GameObject eventObj = new GameObject("EventSystem");
        EventSystem es = eventObj.AddComponent<EventSystem>();
        InputSystemUIInputModule inputModule = eventObj.AddComponent<InputSystemUIInputModule>();

        // 4. Tạo Canvas (Reference Resolution 2340 x 1080 pixel art chuẩn 19.5:9)
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2340, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 5. Background Image & Frame Animator
        GameObject bgObj = new GameObject("BackgroundImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        Image bgImage = bgObj.GetComponent<Image>();
        bgImage.raycastTarget = false;

        // Tải 31 frames từ LoginMenuFrames
        List<Sprite> framesList = new List<Sprite>();
        for (int i = 0; i < 31; i++)
        {
            string framePath = $"Assets/UI/LoginMenuFrames/frame_{i:02d}.png";
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
            if (sp != null)
            {
                framesList.Add(sp);
            }
        }
        if (framesList.Count > 0)
        {
            bgImage.sprite = framesList[0];
        }

        UIFrameAnimator animator = bgObj.AddComponent<UIFrameAnimator>();
        animator.frames = framesList.ToArray();
        animator.framesPerSecond = 10f;
        animator.loop = true;
        animator.unscaledTime = true;

        // Font chữ
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null)
        {
            try { defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
        }

        Color darkTextColor = new Color(0.28f, 0.15f, 0.10f, 1f);     // #47261A
        Color placeholderColor = new Color(0.58f, 0.42f, 0.35f, 0.85f); // #946B59

        // 6. Form Container
        GameObject formObj = new GameObject("LoginForm", typeof(RectTransform));
        formObj.transform.SetParent(canvasObj.transform, false);
        RectTransform formRect = formObj.GetComponent<RectTransform>();
        formRect.anchorMin = new Vector2(0.5f, 0.5f);
        formRect.anchorMax = new Vector2(0.5f, 0.5f);
        formRect.sizeDelta = new Vector2(2340, 1080);

        // Helper tạo InputField
        InputField CreateInputField(string name, Vector2 pos, Vector2 size, string placeholderText, InputField.ContentType contentType, int charLimit)
        {
            GameObject inputRoot = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField));
            inputRoot.transform.SetParent(formObj.transform, false);
            RectTransform rootRect = inputRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = pos;
            rootRect.sizeDelta = size;

            Image rootImg = inputRoot.GetComponent<Image>();
            rootImg.color = new Color(1f, 1f, 1f, 0.001f); // Vùng tương tác trong suốt
            rootImg.raycastTarget = true;

            InputField inf = inputRoot.GetComponent<InputField>();
            inf.contentType = contentType;
            inf.characterLimit = charLimit;
            inf.caretWidth = 2;
            inf.caretColor = darkTextColor;
            inf.selectionColor = new Color(0.85f, 0.6f, 0.4f, 0.5f);

            // Text GameObject: offsetMin.x = 135 để không đè lên icon hòm thư/ổ khóa
            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObj.transform.SetParent(inputRoot.transform, false);
            RectTransform tRect = textObj.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = new Vector2(135f, 0f);
            tRect.offsetMax = new Vector2(-20f, 0f);

            Text txt = textObj.GetComponent<Text>();
            txt.font = defaultFont;
            txt.fontSize = 26;
            txt.fontStyle = FontStyle.Normal;
            txt.color = darkTextColor;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.supportRichText = false;

            // Placeholder GameObject
            GameObject phObj = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            phObj.transform.SetParent(inputRoot.transform, false);
            RectTransform phRect = phObj.GetComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = new Vector2(135f, 0f);
            phRect.offsetMax = new Vector2(-20f, 0f);

            Text phTxt = phObj.GetComponent<Text>();
            phTxt.font = defaultFont;
            phTxt.fontSize = 26;
            phTxt.fontStyle = FontStyle.Normal;
            phTxt.color = placeholderColor;
            phTxt.alignment = TextAnchor.MiddleLeft;
            phTxt.text = placeholderText;

            inf.textComponent = txt;
            inf.placeholder = phTxt;

            return inf;
        }

        // Tọa độ Input Boxes (tính toán pixel-perfect từ ảnh 2340x1080)
        InputField emailInf = CreateInputField("EmailInput", new Vector2(0f, 130f), new Vector2(690f, 82f), "Email", InputField.ContentType.EmailAddress, 50);
        InputField passInf = CreateInputField("PasswordInput", new Vector2(0f, 32.5f), new Vector2(690f, 82f), "Mật khẩu", InputField.ContentType.Password, 35);

        // 7. Checkbox (Toggle) Ghi nhớ đăng nhập
        GameObject toggleRoot = new GameObject("RememberToggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
        toggleRoot.transform.SetParent(formObj.transform, false);
        RectTransform togRect = toggleRoot.GetComponent<RectTransform>();
        togRect.anchorMin = new Vector2(0.5f, 0.5f);
        togRect.anchorMax = new Vector2(0.5f, 0.5f);
        togRect.anchoredPosition = new Vector2(-155f, -72.5f);
        togRect.sizeDelta = new Vector2(370f, 48f);

        Image togTargetImg = toggleRoot.GetComponent<Image>();
        togTargetImg.color = new Color(1f, 1f, 1f, 0.001f);
        togTargetImg.raycastTarget = true;

        // Checkmark Image
        GameObject checkObj = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        checkObj.transform.SetParent(toggleRoot.transform, false);
        RectTransform checkRect = checkObj.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkRect.pivot = new Vector2(0.5f, 0.5f);
        checkRect.anchoredPosition = new Vector2(-177f, 0f);
        checkRect.sizeDelta = new Vector2(48f, 47f);

        Image checkImg = checkObj.GetComponent<Image>();
        Sprite checkSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/checkbox_checked.png");
        checkImg.sprite = checkSprite;
        checkImg.raycastTarget = false;
        checkObj.SetActive(false);

        // Label Text
        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObj.transform.SetParent(toggleRoot.transform, false);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = new Vector2(25f, 0f);
        labelRect.sizeDelta = new Vector2(310f, 40f);

        Text labelText = labelObj.GetComponent<Text>();
        labelText.font = defaultFont;
        labelText.fontSize = 26;
        labelText.fontStyle = FontStyle.Bold;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = darkTextColor;
        labelText.text = "Ghi nhớ đăng nhập";
        labelText.raycastTarget = false;

        Toggle toggle = toggleRoot.GetComponent<Toggle>();
        toggle.isOn = false;
        toggle.targetGraphic = togTargetImg;
        toggle.graphic = checkImg;

        // 8. Quên mật khẩu? (Link Button)
        GameObject forgotObj = new GameObject("ForgotPasswordButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
        forgotObj.transform.SetParent(formObj.transform, false);
        RectTransform forgotRect = forgotObj.GetComponent<RectTransform>();
        forgotRect.anchorMin = new Vector2(0.5f, 0.5f);
        forgotRect.anchorMax = new Vector2(0.5f, 0.5f);
        forgotRect.anchoredPosition = new Vector2(248f, -72.5f);
        forgotRect.sizeDelta = new Vector2(240f, 48f);

        Image forgotImg = forgotObj.GetComponent<Image>();
        forgotImg.color = new Color(1f, 1f, 1f, 0.001f);
        forgotImg.raycastTarget = true;

        Button forgotBtn = forgotObj.GetComponent<Button>();
        RegisterButtonEffect forgotEffect = forgotObj.GetComponent<RegisterButtonEffect>();
        forgotEffect.chimeFrequency = 900f;

        GameObject forgotTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        forgotTextObj.transform.SetParent(forgotObj.transform, false);
        RectTransform ftRect = forgotTextObj.GetComponent<RectTransform>();
        ftRect.anchorMin = Vector2.zero;
        ftRect.anchorMax = Vector2.one;
        ftRect.sizeDelta = Vector2.zero;

        Text forgotTxt = forgotTextObj.GetComponent<Text>();
        forgotTxt.font = defaultFont;
        forgotTxt.fontSize = 26;
        forgotTxt.fontStyle = FontStyle.Bold;
        forgotTxt.alignment = TextAnchor.MiddleRight;
        forgotTxt.color = darkTextColor;
        forgotTxt.text = "Quên mật khẩu?";
        forgotTxt.raycastTarget = false;

        GameObject fUnderObj = new GameObject("Underline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fUnderObj.transform.SetParent(forgotObj.transform, false);
        RectTransform fuRect = fUnderObj.GetComponent<RectTransform>();
        fuRect.anchorMin = new Vector2(0.5f, 0.5f);
        fuRect.anchorMax = new Vector2(0.5f, 0.5f);
        fuRect.anchoredPosition = new Vector2(10f, -14f);
        fuRect.sizeDelta = new Vector2(200f, 2f);

        Image fuImg = fUnderObj.GetComponent<Image>();
        fuImg.color = darkTextColor;
        fuImg.raycastTarget = false;

        // 9. Status Text (Hiển thị lỗi hoặc thành công)
        GameObject statusObj = new GameObject("StatusText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        statusObj.transform.SetParent(formObj.transform, false);
        RectTransform stRect = statusObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0.5f, 0.5f);
        stRect.anchorMax = new Vector2(0.5f, 0.5f);
        stRect.anchoredPosition = new Vector2(0f, -130f);
        stRect.sizeDelta = new Vector2(900f, 36f);

        Text stText = statusObj.GetComponent<Text>();
        stText.font = defaultFont;
        stText.fontSize = 22;
        stText.fontStyle = FontStyle.Bold;
        stText.alignment = TextAnchor.MiddleCenter;
        stText.text = "";
        stText.raycastTarget = false;

        // 10. Nút ĐĂNG NHẬP
        GameObject loginBtnObj = new GameObject("LoginButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
        loginBtnObj.transform.SetParent(formObj.transform, false);
        RectTransform loginRect = loginBtnObj.GetComponent<RectTransform>();
        loginRect.anchorMin = new Vector2(0.5f, 0.5f);
        loginRect.anchorMax = new Vector2(0.5f, 0.5f);
        loginRect.anchoredPosition = new Vector2(0f, -187f);
        loginRect.sizeDelta = new Vector2(650f, 88f);

        Image loginImg = loginBtnObj.GetComponent<Image>();
        loginImg.color = new Color(1f, 1f, 1f, 0.001f);
        loginImg.raycastTarget = true;

        Button loginBtn = loginBtnObj.GetComponent<Button>();
        RegisterButtonEffect loginEffect = loginBtnObj.GetComponent<RegisterButtonEffect>();
        loginEffect.chimeFrequency = 1050f;

        // 11. Nút QUAY LẠI
        GameObject backBtnObj = new GameObject("BackButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
        backBtnObj.transform.SetParent(formObj.transform, false);
        RectTransform backRect = backBtnObj.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.5f, 0.5f);
        backRect.anchorMax = new Vector2(0.5f, 0.5f);
        backRect.anchoredPosition = new Vector2(0f, -296f);
        backRect.sizeDelta = new Vector2(510f, 76f);

        Image backImg = backBtnObj.GetComponent<Image>();
        backImg.color = new Color(1f, 1f, 1f, 0.001f);
        backImg.raycastTarget = true;

        Button backBtn = backBtnObj.GetComponent<Button>();
        RegisterButtonEffect backEffect = backBtnObj.GetComponent<RegisterButtonEffect>();
        backEffect.chimeFrequency = 650f;

        // 12. Dòng "Chưa có tài khoản? ĐĂNG KÝ"
        GameObject regLinkObj = new GameObject("RegisterPrompt", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
        regLinkObj.transform.SetParent(formObj.transform, false);
        RectTransform regLinkRect = regLinkObj.GetComponent<RectTransform>();
        regLinkRect.anchorMin = new Vector2(0.5f, 0.5f);
        regLinkRect.anchorMax = new Vector2(0.5f, 0.5f);
        regLinkRect.pivot = new Vector2(0.5f, 0.5f);
        regLinkRect.anchoredPosition = new Vector2(0f, -403f);
        regLinkRect.sizeDelta = new Vector2(460f, 40f);

        Image regLinkImg = regLinkObj.GetComponent<Image>();
        regLinkImg.color = new Color(1f, 1f, 1f, 0.001f);
        regLinkImg.raycastTarget = true;

        Button regLinkBtn = regLinkObj.GetComponent<Button>();
        RegisterButtonEffect regLinkEffect = regLinkObj.GetComponent<RegisterButtonEffect>();
        regLinkEffect.chimeFrequency = 1150f;
        regLinkEffect.punchDuration = 0.18f;
        regLinkEffect.pressedScale = 0.94f;

        GameObject regTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        regTextObj.transform.SetParent(regLinkObj.transform, false);
        RectTransform rtRect = regTextObj.GetComponent<RectTransform>();
        rtRect.anchorMin = Vector2.zero;
        rtRect.anchorMax = Vector2.one;
        rtRect.sizeDelta = Vector2.zero;

        Text regTxt = regTextObj.GetComponent<Text>();
        regTxt.font = defaultFont;
        regTxt.fontSize = 24;
        regTxt.fontStyle = FontStyle.Bold;
        regTxt.alignment = TextAnchor.MiddleCenter;
        regTxt.supportRichText = true;
        regTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
        regTxt.verticalOverflow = VerticalWrapMode.Overflow;
        regTxt.color = darkTextColor;
        regTxt.text = "Chưa có tài khoản? <color=#D23C3C>ĐĂNG KÝ</color>";
        regTxt.raycastTarget = false;

        GameObject rUnderObj = new GameObject("Underline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        rUnderObj.transform.SetParent(regLinkObj.transform, false);
        RectTransform ruRect = rUnderObj.GetComponent<RectTransform>();
        ruRect.anchorMin = new Vector2(0.5f, 0.5f);
        ruRect.anchorMax = new Vector2(0.5f, 0.5f);
        ruRect.pivot = new Vector2(0.5f, 0.5f);
        ruRect.anchoredPosition = new Vector2(115f, -14f);
        ruRect.sizeDelta = new Vector2(110f, 2f);

        Image ruImg = rUnderObj.GetComponent<Image>();
        ruImg.color = new Color(0.824f, 0.235f, 0.235f, 1f); // #D23C3C
        ruImg.raycastTarget = false;

        // 13. ScreenFadeOverlay
        GameObject fadeObj = new GameObject("ScreenFadeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        fadeObj.transform.SetParent(canvasObj.transform, false);
        fadeObj.transform.SetAsLastSibling();

        RectTransform fRect = fadeObj.GetComponent<RectTransform>();
        fRect.anchorMin = Vector2.zero;
        fRect.anchorMax = Vector2.one;
        fRect.sizeDelta = Vector2.zero;

        Image fImg = fadeObj.GetComponent<Image>();
        fImg.color = Color.black;
        fImg.raycastTarget = true;

        CanvasGroup fadeGroup = fadeObj.GetComponent<CanvasGroup>();
        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = true;

        // 14. LoginMenuManager
        GameObject managerObj = new GameObject("LoginMenuManager");
        LoginMenuController controller = managerObj.AddComponent<LoginMenuController>();
        controller.emailInput = emailInf;
        controller.passwordInput = passInf;
        controller.rememberToggle = toggle;
        controller.loginButton = loginBtn;
        controller.backButton = backBtn;
        controller.forgotPasswordButton = forgotBtn;
        controller.registerLinkButton = regLinkBtn;
        controller.statusText = stText;
        controller.screenFadeOverlay = fadeGroup;
        controller.fadeDuration = 0.35f;
        controller.mainMenuSceneName = "MainMenu";
        controller.registerMenuSceneName = "RegisterMenu";
        controller.gameSceneName = "Level1_Prototype";

        // Gán sự kiện
        loginBtn.onClick.AddListener(controller.OnLoginClicked);
        backBtn.onClick.AddListener(controller.OnBackClicked);
        forgotBtn.onClick.AddListener(controller.OnForgotPasswordClicked);
        regLinkBtn.onClick.AddListener(controller.OnRegisterClicked);

        // 15. Lưu Scene
        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log($"[LoginMenuSetup] Đã lưu thành công scene tại: {scenePath}");

        // 16. Thêm Scene vào Build Settings (Scenes in Build)
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool exists = false;
        foreach (var s in buildScenes)
        {
            if (s.path == scenePath)
            {
                exists = true;
                s.enabled = true;
                break;
            }
        }
        if (!exists)
        {
            // Chèn sau MainMenu (index 1) hoặc vào danh sách
            int insertIndex = 1;
            if (buildScenes.Count > 0 && buildScenes[0].path.Contains("MainMenu"))
            {
                insertIndex = 1;
            }
            buildScenes.Insert(Mathf.Min(insertIndex, buildScenes.Count), new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log($"[LoginMenuSetup] Đã thêm LoginMenu vào Scenes In Build tại index {insertIndex}.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[LoginMenuSetup] Hoàn tất toàn bộ quy trình thiết lập Scene LoginMenu!");
    }
}
