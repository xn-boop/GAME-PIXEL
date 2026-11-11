using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JourneyOfMemories.Editor
{
    [InitializeOnLoad]
    public static class ChonMapMenuSetup
    {
        private const string PREF_KEY = "ChonMapMenu_Scene_Created_v1";

        static ChonMapMenuSetup()
        {
            EditorApplication.delayCall += InitializeIfNeeded;
        }

        private static void InitializeIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            string scenePath = "Assets/Scenes/Chọn map.unity";
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.Log("[ChonMapMenuSetup] Scene 'Chọn map' chưa tồn tại. Tự động khởi tạo...");
                BuildScene();
            }
        }

        [MenuItem("Tools/Build 'Chọn map' Scene")]
        public static void BuildScene()
        {
            Debug.Log("[ChonMapMenuSetup] Bắt đầu khởi tạo Scene 'Chọn map'...");

            // 1. Tạo Scene mới
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            string scenePath = "Assets/Scenes/Chọn map.unity";

            // 2. Tạo Main Camera (chuẩn 2D Pixel Art)
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

            // 3. Tạo EventSystem duy nhất (tránh duplicate)
            GameObject eventObj = new GameObject("EventSystem");
            EventSystem es = eventObj.AddComponent<EventSystem>();
            InputSystemUIInputModule inputModule = eventObj.AddComponent<InputSystemUIInputModule>();

            // 4. Tạo Canvas (Reference Resolution 1896 x 830 pixel art chuẩn theo các frame)
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1896, 830);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // 5. Background Image & Frame Animator (30 frames)
            GameObject bgObj = new GameObject("BackgroundImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgObj.transform.SetParent(canvasObj.transform, false);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            Image bgImage = bgObj.GetComponent<Image>();
            bgImage.raycastTarget = false;

            // Tải 30 frames từ MenuManChoiFrames
            List<Sprite> framesList = new List<Sprite>();
            for (int i = 0; i < 30; i++)
            {
                string framePath = $"Assets/UI/MenuManChoiFrames/frame_{i:D2}.png";
                AssetDatabase.ImportAsset(framePath, ImportAssetOptions.ForceUpdate);
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
                if (sp == null)
                {
                    var allAssets = AssetDatabase.LoadAllAssetsAtPath(framePath);
                    foreach (var a in allAssets)
                    {
                        if (a is Sprite spr)
                        {
                            sp = spr;
                            break;
                        }
                    }
                }

                if (sp != null)
                {
                    framesList.Add(sp);
                }
            }
            if (framesList.Count > 0)
            {
                bgImage.sprite = framesList[0];
            }
            else
            {
                Debug.LogWarning("[ChonMapMenuSetup] Chưa load được frames từ Assets/UI/MenuManChoiFrames!");
            }

            UIFrameAnimator animator = bgObj.AddComponent<UIFrameAnimator>();
            animator.frames = framesList.ToArray();
            animator.framesPerSecond = 10f;
            animator.loop = true;
            animator.unscaledTime = true;

            // Font chữ hệ thống chuẩn Pixel
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
            {
                try { defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
            }

            Color darkTextColor = new Color(0.28f, 0.15f, 0.10f, 1f);     // #47261A
            Color goldTitleColor = new Color(0.96f, 0.85f, 0.60f, 1f);    // #F5D999
            Color cardBgColor = new Color(0.18f, 0.11f, 0.08f, 0.96f);    // #2E1C14

            // 6. Form Container chính
            GameObject formObj = new GameObject("ChonMapForm", typeof(RectTransform));
            formObj.transform.SetParent(canvasObj.transform, false);
            RectTransform formRect = formObj.GetComponent<RectTransform>();
            formRect.anchorMin = new Vector2(0.5f, 0.5f);
            formRect.anchorMax = new Vector2(0.5f, 0.5f);
            formRect.sizeDelta = new Vector2(1896, 830);

            // Helper tạo button trên biển gỗ
            Button CreateBoardButton(string name, Vector2 pos, Vector2 size, float chimeFreq)
            {
                GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
                btnObj.transform.SetParent(formObj.transform, false);
                RectTransform bRect = btnObj.GetComponent<RectTransform>();
                bRect.anchorMin = new Vector2(0.5f, 0.5f);
                bRect.anchorMax = new Vector2(0.5f, 0.5f);
                bRect.anchoredPosition = pos;
                bRect.sizeDelta = size;

                Image img = btnObj.GetComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0.001f); // Vùng tương tác trong suốt khớp nút vẽ sẵn
                img.raycastTarget = true;

                Button btn = btnObj.GetComponent<Button>();
                RegisterButtonEffect effect = btnObj.GetComponent<RegisterButtonEffect>();
                effect.chimeFrequency = chimeFreq;
                effect.punchDuration = 0.2f;
                effect.pressedScale = 0.94f;

                return btn;
            }

            // Tọa độ 3 nút pixel-perfect trên biển gỗ 1896x830
            // Nút "CHƠI"
            Button playBtn = CreateBoardButton("PlayButton", new Vector2(0f, -76.5f), new Vector2(350f, 82f), 1050f);

            // Nút "CÀI ĐẶT"
            Button settingsBtn = CreateBoardButton("SettingsButton", new Vector2(0f, -172f), new Vector2(350f, 82f), 880f);

            // Nút "THOÁT"
            Button quitBtn = CreateBoardButton("QuitButton", new Vector2(0f, -266f), new Vector2(350f, 82f), 650f);

            // 7. Status Text (Thông báo trạng thái)
            GameObject statusObj = new GameObject("StatusText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            statusObj.transform.SetParent(formObj.transform, false);
            RectTransform stRect = statusObj.GetComponent<RectTransform>();
            stRect.anchorMin = new Vector2(0.5f, 0.5f);
            stRect.anchorMax = new Vector2(0.5f, 0.5f);
            stRect.anchoredPosition = new Vector2(0f, -345f);
            stRect.sizeDelta = new Vector2(900f, 40f);

            Text stText = statusObj.GetComponent<Text>();
            stText.font = defaultFont;
            stText.fontSize = 24;
            stText.fontStyle = FontStyle.Bold;
            stText.alignment = TextAnchor.MiddleCenter;
            stText.color = darkTextColor;
            stText.text = "";
            stText.raycastTarget = false;

            // 8. Settings Panel (Bảng Cài Đặt)
            GameObject settingsPanelObj = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            settingsPanelObj.transform.SetParent(canvasObj.transform, false);
            RectTransform spRect = settingsPanelObj.GetComponent<RectTransform>();
            spRect.anchorMin = new Vector2(0.5f, 0.5f);
            spRect.anchorMax = new Vector2(0.5f, 0.5f);
            spRect.anchoredPosition = Vector2.zero;
            spRect.sizeDelta = new Vector2(640f, 440f);

            Image spImg = settingsPanelObj.GetComponent<Image>();
            spImg.color = cardBgColor;

            // Tiêu đề Cài Đặt
            GameObject stTitleObj = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            stTitleObj.transform.SetParent(settingsPanelObj.transform, false);
            RectTransform sttRect = stTitleObj.GetComponent<RectTransform>();
            sttRect.anchorMin = new Vector2(0.5f, 0.5f);
            sttRect.anchorMax = new Vector2(0.5f, 0.5f);
            sttRect.anchoredPosition = new Vector2(0f, 165f);
            sttRect.sizeDelta = new Vector2(500f, 50f);

            Text sttText = stTitleObj.GetComponent<Text>();
            sttText.font = defaultFont;
            sttText.fontSize = 30;
            sttText.fontStyle = FontStyle.Bold;
            sttText.alignment = TextAnchor.MiddleCenter;
            sttText.color = goldTitleColor;
            sttText.text = "CÀI ĐẶT";

            // Helper tạo Slider âm lượng
            Slider CreateVolumeSlider(string name, Vector2 pos, string labelText, float defaultValue)
            {
                // Nhãn mô tả
                GameObject lblObj = new GameObject(name + "_Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                lblObj.transform.SetParent(settingsPanelObj.transform, false);
                RectTransform lblRect = lblObj.GetComponent<RectTransform>();
                lblRect.anchorMin = new Vector2(0.5f, 0.5f);
                lblRect.anchorMax = new Vector2(0.5f, 0.5f);
                lblRect.anchoredPosition = new Vector2(pos.x - 70f, pos.y + 30f);
                lblRect.sizeDelta = new Vector2(380f, 30f);

                Text lbl = lblObj.GetComponent<Text>();
                lbl.font = defaultFont;
                lbl.fontSize = 20;
                lbl.fontStyle = FontStyle.Bold;
                lbl.color = new Color(0.9f, 0.85f, 0.8f, 1f);
                lbl.alignment = TextAnchor.MiddleLeft;
                lbl.text = labelText;

                // Slider Root
                GameObject sliderObj = new GameObject(name, typeof(RectTransform), typeof(Slider));
                sliderObj.transform.SetParent(settingsPanelObj.transform, false);
                RectTransform sRect = sliderObj.GetComponent<RectTransform>();
                sRect.anchorMin = new Vector2(0.5f, 0.5f);
                sRect.anchorMax = new Vector2(0.5f, 0.5f);
                sRect.anchoredPosition = pos;
                sRect.sizeDelta = new Vector2(460f, 24f);

                // Background
                GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                bg.transform.SetParent(sliderObj.transform, false);
                RectTransform bgR = bg.GetComponent<RectTransform>();
                bgR.anchorMin = new Vector2(0f, 0.25f);
                bgR.anchorMax = new Vector2(1f, 0.75f);
                bgR.sizeDelta = Vector2.zero;
                Image bgI = bg.GetComponent<Image>();
                bgI.color = new Color(0.10f, 0.06f, 0.04f, 0.9f);

                // Fill Area
                GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
                fillArea.transform.SetParent(sliderObj.transform, false);
                RectTransform faR = fillArea.GetComponent<RectTransform>();
                faR.anchorMin = new Vector2(0f, 0.25f);
                faR.anchorMax = new Vector2(1f, 0.75f);
                faR.sizeDelta = new Vector2(-20f, 0f);

                GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fill.transform.SetParent(fillArea.transform, false);
                RectTransform fillR = fill.GetComponent<RectTransform>();
                fillR.sizeDelta = Vector2.zero;
                Image fillI = fill.GetComponent<Image>();
                fillI.color = new Color(0.85f, 0.55f, 0.22f, 1f);

                // Handle Area
                GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
                handleArea.transform.SetParent(sliderObj.transform, false);
                RectTransform haR = handleArea.GetComponent<RectTransform>();
                haR.anchorMin = Vector2.zero;
                haR.anchorMax = Vector2.one;
                haR.sizeDelta = new Vector2(-20f, 0f);

                GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                handle.transform.SetParent(handleArea.transform, false);
                RectTransform hR = handle.GetComponent<RectTransform>();
                hR.sizeDelta = new Vector2(24f, 28f);
                Image handleI = handle.GetComponent<Image>();
                handleI.color = goldTitleColor;

                Slider s = sliderObj.GetComponent<Slider>();
                s.fillRect = fillR;
                s.handleRect = hR;
                s.targetGraphic = handleI;
                s.direction = Slider.Direction.LeftToRight;
                s.minValue = 0f;
                s.maxValue = 1f;
                s.value = defaultValue;

                return s;
            }

            Slider musicSlider = CreateVolumeSlider("VolumeSlider", new Vector2(0f, 60f), "Âm lượng Nhạc nền (BGM)", 0.75f);
            Slider sfxSlider = CreateVolumeSlider("SfxSlider", new Vector2(0f, -30f), "Âm thanh Hiệu ứng (SFX)", 0.85f);

            // Nút Đóng Cài Đặt (Quay Lại)
            GameObject closeSetObj = new GameObject("CloseSettingsButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(RegisterButtonEffect));
            closeSetObj.transform.SetParent(settingsPanelObj.transform, false);
            RectTransform csRect = closeSetObj.GetComponent<RectTransform>();
            csRect.anchorMin = new Vector2(0.5f, 0.5f);
            csRect.anchorMax = new Vector2(0.5f, 0.5f);
            csRect.anchoredPosition = new Vector2(0f, -145f);
            csRect.sizeDelta = new Vector2(240f, 52f);

            Image csImg = closeSetObj.GetComponent<Image>();
            csImg.color = new Color(0.35f, 0.20f, 0.14f, 1f);

            Button closeSetBtn = closeSetObj.GetComponent<Button>();
            RegisterButtonEffect csEffect = closeSetObj.GetComponent<RegisterButtonEffect>();
            csEffect.chimeFrequency = 750f;

            GameObject csTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            csTextObj.transform.SetParent(closeSetObj.transform, false);
            RectTransform cstRect = csTextObj.GetComponent<RectTransform>();
            cstRect.anchorMin = Vector2.zero;
            cstRect.anchorMax = Vector2.one;
            cstRect.sizeDelta = Vector2.zero;

            Text csTxt = csTextObj.GetComponent<Text>();
            csTxt.font = defaultFont;
            csTxt.fontSize = 22;
            csTxt.fontStyle = FontStyle.Bold;
            csTxt.alignment = TextAnchor.MiddleCenter;
            csTxt.color = goldTitleColor;
            csTxt.text = "QUAY LẠI";

            settingsPanelObj.SetActive(false);

            // 9. Map Selection Panel đã được loại bỏ theo yêu cầu: Bấm Chơi vào thẳng Map 0 (Map0_Prologue)

            // 10. ScreenFadeOverlay
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

            // 11. Controller Manager
            GameObject managerObj = new GameObject("ChonMapMenuManager");
            ChonMapMenuController controller = managerObj.AddComponent<ChonMapMenuController>();
            controller.playButton = playBtn;
            controller.settingsButton = settingsBtn;
            controller.quitButton = quitBtn;
            controller.settingsPanel = settingsPanelObj;
            controller.volumeSlider = musicSlider;
            controller.sfxSlider = sfxSlider;
            controller.closeSettingsButton = closeSetBtn;
            controller.mapSelectionPanel = null;
            controller.map0Button = null;
            controller.map1Button = null;
            controller.map2Button = null;
            controller.map3Button = null;
            controller.closeMapSelectButton = null;
            controller.statusText = stText;
            controller.screenFadeOverlay = fadeGroup;
            controller.fadeDuration = 0.35f;
            controller.isStandaloneMode = false;
            controller.allowSceneTransition = true;
            controller.targetPlaySceneName = "Map0_Prologue";

            // 12. Lưu Scene tại: Assets/Scenes/Chọn map.unity
            EditorSceneManager.SaveScene(newScene, scenePath);
            Debug.Log($"[ChonMapMenuSetup] Đã lưu thành công scene tại: {scenePath} (Bấm Chơi -> Vào thẳng Map 0, Đã bỏ menu màu nâu)");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ChonMapMenuSetup] Hoàn tất toàn bộ quy trình thiết lập Scene 'Chọn map'!");
        }

        [InitializeOnLoadMethod]
        private static void AutoRebuildOnCompile()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            string key = "ChonMapMenu_DirectPlayMap0_Done_v2";
            if (!SessionState.GetBool(key, false))
            {
                SessionState.SetBool(key, true);
                EditorApplication.delayCall += () =>
                {
                    try
                    {
                        BuildScene();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ChonMapMenuSetup AutoRebuild] {ex}");
                    }
                };
            }
        }
    }
}
