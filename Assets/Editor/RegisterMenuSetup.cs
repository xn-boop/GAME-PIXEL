using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RegisterMenuSetup
{
    [MenuItem("Tools/Build RegisterMenu Scene")]
    public static void BuildScene()
    {
        Debug.Log("[RegisterMenuSetup] Bắt đầu khởi tạo Scene RegisterMenu...");

        // 1. Tạo Scene mới
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        string scenePath = "Assets/Scenes/RegisterMenu.unity";

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

        // 3. Tạo EventSystem
        GameObject eventObj = new GameObject("EventSystem");
        EventSystem es = eventObj.AddComponent<EventSystem>();
        InputSystemUIInputModule inputModule = eventObj.AddComponent<InputSystemUIInputModule>();

        // 4. Tạo Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1844, 853);
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

        // Tải 31 frames từ RegisterMenuFrames
        List<Sprite> framesList = new List<Sprite>();
        for (int i = 0; i < 31; i++)
        {
            string framePath = $"Assets/UI/RegisterMenuFrames/frame_{i:02d}.png";
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
        GameObject formObj = new GameObject("RegisterForm", typeof(RectTransform));
        formObj.transform.SetParent(canvasObj.transform, false);
        RectTransform formRect = formObj.GetComponent<RectTransform>();
        formRect.anchorMin = new Vector2(0.5f, 0.5f);
        formRect.anchorMax = new Vector2(0.5f, 0.5f);
        formRect.sizeDelta = new Vector2(1844, 853);

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

            // Text GameObject
            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObj.transform.SetParent(inputRoot.transform, false);
            RectTransform tRect = textObj.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.sizeDelta = Vector2.zero;

            Text txt = textObj.GetComponent<Text>();
            txt.font = defaultFont;
            txt.fontSize = 24;
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
            phRect.sizeDelta = Vector2.zero;

            Text phTxt = phObj.GetComponent<Text>();
            phTxt.font = defaultFont;
            phTxt.fontSize = 24;
            phTxt.fontStyle = FontStyle.Normal;
            phTxt.color = placeholderColor;
            phTxt.alignment = TextAnchor.MiddleLeft;
            phTxt.text = placeholderText;

            inf.textComponent = txt;
            inf.placeholder = phTxt;

            return inf;
        }

        // Tọa độ input boxes (đã tính toán pixel-perfect từ ảnh gốc)
        InputField userInf = CreateInputField("UsernameInput", new Vector2(30.5f, 116.5f), new Vector2(470f, 54f), "Nhập tên người chơi...", InputField.ContentType.Standard, 25);
        InputField emailInf = CreateInputField("EmailInput", new Vector2(30.5f, 36.5f), new Vector2(470f, 54f), "Nhập địa chỉ email...", InputField.ContentType.EmailAddress, 45);
        InputField passInf = CreateInputField("PasswordInput", new Vector2(30.5f, -38.5f), new Vector2(470f, 54f), "Nhập mật khẩu (tối thiểu 6 ký tự)...", InputField.ContentType.Password, 30);

        // 7. Checkbox (Toggle)
        // Vùng tương tác trong suốt bao phủ toàn bộ ô checkbox và dòng chữ điều khoản.
        // Chỉ chứa 1 Checkmark và 1 Label Text rõ nét, cùng hàng, không trùng lặp.
        GameObject toggleRoot = new GameObject("TermsToggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Toggle));
        toggleRoot.transform.SetParent(formObj.transform, false);
        RectTransform togRect = toggleRoot.GetComponent<RectTransform>();
        togRect.anchorMin = new Vector2(0.5f, 0.5f);
        togRect.anchorMax = new Vector2(0.5f, 0.5f);
        togRect.anchoredPosition = new Vector2(30.5f, -102.5f);
        togRect.sizeDelta = new Vector2(470f, 48f);

        Image togTargetImg = toggleRoot.GetComponent<Image>();
        togTargetImg.color = new Color(1f, 1f, 1f, 0.001f);
        togTargetImg.raycastTarget = true;

        // Checkmark Image (Dấu tích xanh khớp chuẩn xác từng pixel vào ô checkbox)
        GameObject checkObj = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        checkObj.transform.SetParent(toggleRoot.transform, false);
        RectTransform checkRect = checkObj.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkRect.pivot = new Vector2(0.5f, 0.5f);
        checkRect.anchoredPosition = new Vector2(-213f, 0f);
        checkRect.sizeDelta = new Vector2(39f, 38f);

        Image checkImg = checkObj.GetComponent<Image>();
        Sprite checkSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/checkbox_checked.png");
        checkImg.sprite = checkSprite;
        checkImg.raycastTarget = false;
        checkObj.SetActive(false);

        // Label Text rõ nét, nằm cạnh checkbox trên cùng một hàng
        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObj.transform.SetParent(toggleRoot.transform, false);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = new Vector2(24f, 0f);
        labelRect.sizeDelta = new Vector2(410f, 40f);

        Text labelText = labelObj.GetComponent<Text>();
        labelText.font = defaultFont;
        labelText.fontSize = 22;
        labelText.fontStyle = FontStyle.Bold;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = darkTextColor;
        labelText.text = "Tôi đồng ý với Điều khoản sử dụng";
        labelText.raycastTarget = false;

        Toggle toggle = toggleRoot.GetComponent<Toggle>();
        toggle.isOn = false; // Mặc định chưa tích chọn
        toggle.targetGraphic = togTargetImg;
        toggle.graphic = checkImg;

        // 8. Status Text (Hiển thị lỗi hoặc thành công)
        GameObject statusObj = new GameObject("StatusText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        statusObj.transform.SetParent(formObj.transform, false);
        RectTransform stRect = statusObj.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(0.5f, 0.5f);
        stRect.anchorMax = new Vector2(0.5f, 0.5f);
        stRect.anchoredPosition = new Vector2(0f, -140f);
        stRect.sizeDelta = new Vector2(750f, 32f);

        Text stText = statusObj.GetComponent<Text>();
        stText.font = defaultFont;
        stText.fontSize = 18;
        stText.fontStyle = FontStyle.Bold;
        stText.alignment = TextAnchor.MiddleCenter;
        stText.text = "";
        stText.raycastTarget = false;

        // 9. Nút ĐĂNG KÝ
        GameObject regBtnObj = new GameObject("RegisterButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
        regBtnObj.transform.SetParent(formObj.transform, false);
        RectTransform regRect = regBtnObj.GetComponent<RectTransform>();
        regRect.anchorMin = new Vector2(0.5f, 0.5f);
        regRect.anchorMax = new Vector2(0.5f, 0.5f);
        regRect.anchoredPosition = new Vector2(0f, -178.5f);
        regRect.sizeDelta = new Vector2(520f, 72f);

        Image regImg = regBtnObj.GetComponent<Image>();
        regImg.color = new Color(1f, 1f, 1f, 0.001f);
        regImg.raycastTarget = true;

        Button regBtn = regBtnObj.GetComponent<Button>();
        RegisterButtonEffect regEffect = regBtnObj.GetComponent<RegisterButtonEffect>();
        regEffect.chimeFrequency = 1050f;

        // 10. Nút QUAY LẠI
        GameObject backBtnObj = new GameObject("BackButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
        backBtnObj.transform.SetParent(formObj.transform, false);
        RectTransform backRect = backBtnObj.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.5f, 0.5f);
        backRect.anchorMax = new Vector2(0.5f, 0.5f);
        backRect.anchoredPosition = new Vector2(0f, -248.5f);
        backRect.sizeDelta = new Vector2(400f, 54f);

        Image backImg = backBtnObj.GetComponent<Image>();
        backImg.color = new Color(1f, 1f, 1f, 0.001f);
        backImg.raycastTarget = true;

        Button backBtn = backBtnObj.GetComponent<Button>();
        RegisterButtonEffect backEffect = backBtnObj.GetComponent<RegisterButtonEffect>();
        backEffect.chimeFrequency = 650f;

        // 11. Dòng chữ "Đã có tài khoản? ĐĂNG NHẬP" bên dưới nút QUAY LẠI
        GameObject loginLinkObj = new GameObject("LoginPrompt", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
        loginLinkObj.transform.SetParent(formObj.transform, false);
        RectTransform loginRect = loginLinkObj.GetComponent<RectTransform>();
        loginRect.anchorMin = new Vector2(0.5f, 0.5f);
        loginRect.anchorMax = new Vector2(0.5f, 0.5f);
        loginRect.pivot = new Vector2(0.5f, 0.5f);
        loginRect.anchoredPosition = new Vector2(0f, -315f);
        loginRect.sizeDelta = new Vector2(380f, 36f);

        Image loginImg = loginLinkObj.GetComponent<Image>();
        loginImg.color = new Color(1f, 1f, 1f, 0.001f);
        loginImg.raycastTarget = true;

        Button loginBtn = loginLinkObj.GetComponent<Button>();
        RegisterButtonEffect loginEffect = loginLinkObj.GetComponent<RegisterButtonEffect>();
        loginEffect.chimeFrequency = 1150f;
        loginEffect.punchDuration = 0.18f;
        loginEffect.pressedScale = 0.94f;

        // Text hiển thị
        GameObject loginTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        loginTextObj.transform.SetParent(loginLinkObj.transform, false);
        RectTransform ltRect = loginTextObj.GetComponent<RectTransform>();
        ltRect.anchorMin = Vector2.zero;
        ltRect.anchorMax = Vector2.one;
        ltRect.sizeDelta = Vector2.zero;
        ltRect.anchoredPosition = Vector2.zero;

        Text loginTxt = loginTextObj.GetComponent<Text>();
        loginTxt.font = defaultFont;
        loginTxt.fontSize = 20;
        loginTxt.fontStyle = FontStyle.Bold;
        loginTxt.alignment = TextAnchor.MiddleCenter;
        loginTxt.supportRichText = true;
        loginTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
        loginTxt.verticalOverflow = VerticalWrapMode.Overflow;
        loginTxt.color = darkTextColor;
        loginTxt.text = "Đã có tài khoản? <color=#D23C3C>ĐĂNG NHẬP</color>";
        loginTxt.raycastTarget = false;

        // Underline dưới chữ ĐĂNG NHẬP
        GameObject underObj = new GameObject("Underline", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        underObj.transform.SetParent(loginLinkObj.transform, false);
        RectTransform uRect = underObj.GetComponent<RectTransform>();
        uRect.anchorMin = new Vector2(0.5f, 0.5f);
        uRect.anchorMax = new Vector2(0.5f, 0.5f);
        uRect.pivot = new Vector2(0.5f, 0.5f);
        uRect.anchoredPosition = new Vector2(83.5f, -11f);
        uRect.sizeDelta = new Vector2(124f, 2f);

        Image underImg = underObj.GetComponent<Image>();
        underImg.color = new Color(0.824f, 0.235f, 0.235f, 1f);
        underImg.raycastTarget = false;

        // 12. ScreenFadeOverlay
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

        // 13. RegisterMenuManager
        GameObject managerObj = new GameObject("RegisterMenuManager");
        RegisterMenuController controller = managerObj.AddComponent<RegisterMenuController>();
        controller.usernameInput = userInf;
        controller.emailInput = emailInf;
        controller.passwordInput = passInf;
        controller.termsToggle = toggle;
        controller.registerButton = regBtn;
        controller.backButton = backBtn;
        controller.loginLinkButton = loginBtn;
        controller.statusText = stText;
        controller.screenFadeOverlay = fadeGroup;
        controller.fadeDuration = 0.35f;
        controller.mainMenuSceneName = "MainMenu";
        controller.gameSceneName = "Level1_Prototype";

        // Gán sự kiện cho các nút
        regBtn.onClick.AddListener(controller.OnRegisterClicked);
        backBtn.onClick.AddListener(controller.OnBackClicked);

        // 13. Lưu Scene
        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log($"[RegisterMenuSetup] Đã lưu thành công scene tại: {scenePath}");

        // 14. Thêm Scene vào Build Settings
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
            buildScenes.Insert(1, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
            Debug.Log("[RegisterMenuSetup] Đã thêm RegisterMenu vào Scenes In Build (index 1).");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[RegisterMenuSetup] Hoàn tất toàn bộ quy trình thiết lập Scene RegisterMenu!");
    }
}
