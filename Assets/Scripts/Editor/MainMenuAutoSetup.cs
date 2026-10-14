using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace JourneyOfMemories.Editor
{
    [InitializeOnLoad]
    public static class MainMenuAutoSetup
    {
        private const string PREF_KEY = "MainMenu_V3_Setup_Completed_v1";

        static MainMenuAutoSetup()
        {
            EditorApplication.delayCall += ExecuteSetupIfNeeded;
        }

        [MenuItem("Tools/Re-Setup Main Menu V3")]
        public static void ForceExecuteSetup()
        {
            EditorPrefs.DeleteKey(PREF_KEY);
            ExecuteSetup();
        }

        private static void ExecuteSetupIfNeeded()
        {
            if (!EditorPrefs.GetBool(PREF_KEY, false))
            {
                ExecuteSetup();
            }
        }

        public static void ExecuteSetup()
        {
            string scenePath = "Assets/Scenes/MainMenu.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[MainMenuAutoSetup] Không thể mở scene tại: {scenePath}");
                return;
            }

            Debug.Log("[MainMenuAutoSetup] Bắt đầu tự động cấu hình MainMenu theo chuẩn 31 frame...");

            // 1. Tìm Canvas và cấu hình CanvasScaler
            var canvasObj = GameObject.Find("Canvas");
            if (canvasObj == null)
            {
                Debug.LogError("[MainMenuAutoSetup] Không tìm thấy Canvas!");
                return;
            }

            var canvasScaler = canvasObj.GetComponent<CanvasScaler>();
            if (canvasScaler != null)
            {
                Undo.RecordObject(canvasScaler, "Update CanvasScaler");
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1844, 853);
                canvasScaler.matchWidthOrHeight = 0.5f;
            }

            // 2. Cấu hình BackgroundImage và UIFrameAnimator với 31 frame
            var bgObj = GameObject.Find("BackgroundImage");
            if (bgObj != null)
            {
                Undo.RecordObject(bgObj, "Configure Background");
                var bgImage = bgObj.GetComponent<Image>();
                var animator = bgObj.GetComponent<UIFrameAnimator>();
                if (animator == null)
                {
                    animator = Undo.AddComponent<UIFrameAnimator>(bgObj);
                }

                List<Sprite> frameList = new List<Sprite>();
                for (int i = 0; i <= 30; i++)
                {
                    string path = $"Assets/UI/MainMenuFrames/frame_{i:D2}.png";
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite != null)
                    {
                        frameList.Add(sprite);
                    }
                    else
                    {
                        Debug.LogWarning($"[MainMenuAutoSetup] Chưa load được sprite: {path}");
                    }
                }

                if (frameList.Count > 0)
                {
                    Undo.RecordObject(animator, "Set Frames");
                    animator.frames = frameList.ToArray();
                    animator.framesPerSecond = 10f;
                    animator.loop = true;
                    animator.unscaledTime = true;

                    if (bgImage != null)
                    {
                        Undo.RecordObject(bgImage, "Set First Frame");
                        bgImage.sprite = frameList[0];
                    }
                    Debug.Log($"[MainMenuAutoSetup] Đã nạp thành công {frameList.Count} frame vào UIFrameAnimator (10 FPS)!");
                }
            }

            // 3. Cấu hình nút TAP TO START
            var tapBtnObj = GameObject.Find("TapToStartButton");
            TapToStartEffect tapEffect = null;
            if (tapBtnObj != null)
            {
                Undo.RecordObject(tapBtnObj, "Setup TapToStartButton");
                var rect = tapBtnObj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.208f);
                    rect.anchorMax = new Vector2(0.5f, 0.208f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = Vector2.zero;
                    rect.sizeDelta = new Vector2(440f, 90f);
                }

                tapEffect = tapBtnObj.GetComponent<TapToStartEffect>();
                if (tapEffect == null)
                {
                    tapEffect = Undo.AddComponent<TapToStartEffect>(tapBtnObj);
                }

                // Xóa MenuBlinkEffect cũ nếu có trên PulseHighlight để tránh xung đột
                var pulseObj = tapBtnObj.transform.Find("PulseHighlight");
                if (pulseObj != null)
                {
                    var oldBlink = pulseObj.GetComponent<MenuBlinkEffect>();
                    if (oldBlink != null)
                    {
                        Undo.DestroyObjectImmediate(oldBlink);
                    }
                }

                // Tạo Flash Overlay nếu chưa có
                Transform flashTrans = tapBtnObj.transform.Find("FlashOverlay");
                Image flashImg = null;
                if (flashTrans == null)
                {
                    var flashObj = new GameObject("FlashOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    Undo.RegisterCreatedObjectUndo(flashObj, "Create FlashOverlay");
                    flashTrans = flashObj.transform;
                    flashTrans.SetParent(tapBtnObj.transform, false);

                    var fRect = flashObj.GetComponent<RectTransform>();
                    fRect.anchorMin = Vector2.zero;
                    fRect.anchorMax = Vector2.one;
                    fRect.sizeDelta = Vector2.zero;

                    flashImg = flashObj.GetComponent<Image>();
                    flashImg.color = new Color(1f, 0.95f, 0.8f, 0f);
                    flashImg.raycastTarget = false;
                    flashObj.SetActive(false);
                }
                else
                {
                    flashImg = flashTrans.GetComponent<Image>();
                }

                // Tạo Ripple Ring nếu chưa có
                Transform rippleTrans = tapBtnObj.transform.Find("RippleRing");
                if (rippleTrans == null)
                {
                    var rippleObj = new GameObject("RippleRing", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    Undo.RegisterCreatedObjectUndo(rippleObj, "Create RippleRing");
                    rippleTrans = rippleObj.transform;
                    rippleTrans.SetParent(tapBtnObj.transform, false);

                    var rRect = rippleObj.GetComponent<RectTransform>();
                    rRect.anchorMin = new Vector2(0.5f, 0.5f);
                    rRect.anchorMax = new Vector2(0.5f, 0.5f);
                    rRect.sizeDelta = new Vector2(460f, 100f);

                    var rImg = rippleObj.GetComponent<Image>();
                    rImg.color = new Color(1f, 0.9f, 0.6f, 0.7f);
                    rImg.raycastTarget = false;
                    rippleObj.SetActive(false);
                }

                tapEffect.flashOverlay = flashImg;
                tapEffect.rippleRing = rippleTrans.GetComponent<RectTransform>();
                tapEffect.tapDuration = 0.45f;
            }

            // 4. Cấu hình Nút Ngôn ngữ (LanguageButton) ở góc phải dưới
            var langBtnObj = GameObject.Find("LanguageButton");
            LanguageSelectorUI langSelector = null;
            if (langBtnObj != null)
            {
                Undo.RecordObject(langBtnObj, "Setup LanguageButton");
                var rect = langBtnObj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.906f, 0.096f);
                    rect.anchorMax = new Vector2(0.906f, 0.096f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = Vector2.zero;
                    rect.sizeDelta = new Vector2(240f, 65f);
                }

                langSelector = langBtnObj.GetComponent<LanguageSelectorUI>();
                if (langSelector == null)
                {
                    langSelector = Undo.AddComponent<LanguageSelectorUI>(langBtnObj);
                }

                var btn = langBtnObj.GetComponent<Button>();
                if (btn != null)
                {
                    Undo.RecordObject(btn, "Hookup Language Click");
                    btn.onClick.RemoveAllListeners();
                    UnityEventTools.AddPersistentListener(btn.onClick, langSelector.OnLanguageButtonClicked);
                }

                // Tạo Overlay Text cho VN và EN để hiển thị làm nổi bật ngôn ngữ đang chọn
                Font legacyFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

                Transform indicatorTrans = langBtnObj.transform.Find("ActiveIndicator");
                if (indicatorTrans == null)
                {
                    var indObj = new GameObject("ActiveIndicator", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    Undo.RegisterCreatedObjectUndo(indObj, "Create ActiveIndicator");
                    indicatorTrans = indObj.transform;
                    indicatorTrans.SetParent(langBtnObj.transform, false);

                    var indRect = indObj.GetComponent<RectTransform>();
                    indRect.anchorMin = new Vector2(0.5f, 0.5f);
                    indRect.anchorMax = new Vector2(0.5f, 0.5f);
                    indRect.sizeDelta = new Vector2(65f, 32f);
                    indRect.anchoredPosition = new Vector2(40f, 0f);

                    var indImg = indObj.GetComponent<Image>();
                    indImg.color = new Color(1f, 0.85f, 0.35f, 0.25f);
                    indImg.raycastTarget = false;
                }

                Transform enTrans = langBtnObj.transform.Find("EN_Overlay");
                Text enText = null;
                if (enTrans == null)
                {
                    var enObj = new GameObject("EN_Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                    Undo.RegisterCreatedObjectUndo(enObj, "Create EN_Overlay");
                    enTrans = enObj.transform;
                    enTrans.SetParent(langBtnObj.transform, false);

                    var enRect = enObj.GetComponent<RectTransform>();
                    enRect.anchorMin = new Vector2(0.5f, 0.5f);
                    enRect.anchorMax = new Vector2(0.5f, 0.5f);
                    enRect.sizeDelta = new Vector2(60f, 40f);
                    enRect.anchoredPosition = new Vector2(-42f, 0f);

                    enText = enObj.GetComponent<Text>();
                    enText.font = legacyFont;
                    enText.fontSize = 24;
                    enText.fontStyle = FontStyle.Bold;
                    enText.alignment = TextAnchor.MiddleCenter;
                    enText.text = "EN";
                    enText.color = new Color(0.8f, 0.85f, 0.9f, 0.6f);
                    enText.raycastTarget = false;
                }
                else
                {
                    enText = enTrans.GetComponent<Text>();
                }

                Transform vnTrans = langBtnObj.transform.Find("VN_Overlay");
                Text vnText = null;
                if (vnTrans == null)
                {
                    var vnObj = new GameObject("VN_Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                    Undo.RegisterCreatedObjectUndo(vnObj, "Create VN_Overlay");
                    vnTrans = vnObj.transform;
                    vnTrans.SetParent(langBtnObj.transform, false);

                    var vnRect = vnObj.GetComponent<RectTransform>();
                    vnRect.anchorMin = new Vector2(0.5f, 0.5f);
                    vnRect.anchorMax = new Vector2(0.5f, 0.5f);
                    vnRect.sizeDelta = new Vector2(60f, 40f);
                    vnRect.anchoredPosition = new Vector2(42f, 0f);

                    vnText = vnObj.GetComponent<Text>();
                    vnText.font = legacyFont;
                    vnText.fontSize = 24;
                    vnText.fontStyle = FontStyle.Bold;
                    vnText.alignment = TextAnchor.MiddleCenter;
                    vnText.text = "VN";
                    vnText.color = new Color(1f, 0.95f, 0.65f, 1f);
                    vnText.raycastTarget = false;
                }
                else
                {
                    vnText = vnTrans.GetComponent<Text>();
                }

                langSelector.enText = enText;
                langSelector.vnText = vnText;
                langSelector.activeIndicator = indicatorTrans.GetComponent<RectTransform>();

                // Tạo Toast Notification Panel
                Transform toastTrans = canvasObj.transform.Find("ToastNotification");
                if (toastTrans == null)
                {
                    var toastObj = new GameObject("ToastNotification", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                    Undo.RegisterCreatedObjectUndo(toastObj, "Create ToastNotification");
                    toastTrans = toastObj.transform;
                    toastTrans.SetParent(canvasObj.transform, false);

                    var tRect = toastObj.GetComponent<RectTransform>();
                    tRect.anchorMin = new Vector2(0.5f, 0.05f);
                    tRect.anchorMax = new Vector2(0.5f, 0.05f);
                    tRect.pivot = new Vector2(0.5f, 0.5f);
                    tRect.sizeDelta = new Vector2(380f, 44f);
                    tRect.anchoredPosition = Vector2.zero;

                    var tImg = toastObj.GetComponent<Image>();
                    tImg.color = new Color(0.08f, 0.1f, 0.15f, 0.88f);
                    tImg.raycastTarget = false;

                    var cg = toastObj.GetComponent<CanvasGroup>();
                    cg.alpha = 0f;
                    cg.blocksRaycasts = false;

                    var textChild = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                    textChild.transform.SetParent(toastObj.transform, false);
                    var childRect = textChild.GetComponent<RectTransform>();
                    childRect.anchorMin = Vector2.zero;
                    childRect.anchorMax = Vector2.one;
                    childRect.sizeDelta = Vector2.zero;

                    var tt = textChild.GetComponent<Text>();
                    tt.font = legacyFont;
                    tt.fontSize = 18;
                    tt.alignment = TextAnchor.MiddleCenter;
                    tt.color = new Color(1f, 0.92f, 0.7f, 1f);
                    tt.text = "Ngôn ngữ: Tiếng Việt (VN)";
                    tt.raycastTarget = false;

                    langSelector.toastPanel = toastObj;
                    langSelector.toastText = tt;
                    langSelector.toastCanvasGroup = cg;
                    toastObj.SetActive(false);
                }
            }

            // 5. Tạo ScreenFadeOverlay để chuyển cảnh mượt mà
            Transform fadeTrans = canvasObj.transform.Find("ScreenFadeOverlay");
            CanvasGroup fadeCg = null;
            if (fadeTrans == null)
            {
                var fadeObj = new GameObject("ScreenFadeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                Undo.RegisterCreatedObjectUndo(fadeObj, "Create ScreenFadeOverlay");
                fadeTrans = fadeObj.transform;
                fadeTrans.SetParent(canvasObj.transform, false);
                fadeTrans.SetAsLastSibling();

                var fRect = fadeObj.GetComponent<RectTransform>();
                fRect.anchorMin = Vector2.zero;
                fRect.anchorMax = Vector2.one;
                fRect.sizeDelta = Vector2.zero;

                var fImg = fadeObj.GetComponent<Image>();
                fImg.color = Color.black;
                fImg.raycastTarget = true;

                fadeCg = fadeObj.GetComponent<CanvasGroup>();
                fadeCg.alpha = 0f;
                fadeCg.blocksRaycasts = false;
                fadeObj.SetActive(false);
            }
            else
            {
                fadeCg = fadeTrans.GetComponent<CanvasGroup>();
            }

            // 6. Cấu hình MainMenuManager / MainMenuController
            var managerObj = GameObject.Find("MainMenuManager");
            if (managerObj != null)
            {
                var controller = managerObj.GetComponent<MainMenuController>();
                if (controller != null)
                {
                    Undo.RecordObject(controller, "Setup MainMenuController");
                    controller.firstLevelSceneName = "Level1_Prototype";
                    controller.tapToStartEffect = tapEffect;
                    controller.screenFadeOverlay = fadeCg;
                    controller.fadeDuration = 0.35f;
                }
            }

            // 7. Gán LocalizedText cho SettingsTitle và CloseSettingsButton
            var settingsTitleObj = GameObject.Find("SettingsTitle");
            if (settingsTitleObj != null)
            {
                var loc = settingsTitleObj.GetComponent<LocalizedText>();
                if (loc == null) loc = Undo.AddComponent<LocalizedText>(settingsTitleObj);
                loc.vietnameseText = "CÀI ĐẶT";
                loc.englishText = "SETTINGS";
            }

            var closeBtnTextObj = GameObject.Find("CloseSettingsButton/Text");
            if (closeBtnTextObj != null)
            {
                var loc = closeBtnTextObj.GetComponent<LocalizedText>();
                if (loc == null) loc = Undo.AddComponent<LocalizedText>(closeBtnTextObj);
                loc.vietnameseText = "QUAY LẠI";
                loc.englishText = "BACK";
            }

            // 8. Lưu scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            EditorPrefs.SetBool(PREF_KEY, true);

            Debug.Log("[MainMenuAutoSetup] Chúc mừng! Cấu hình Main Menu V3 hoàn tất 100%!");
        }
    }
}
