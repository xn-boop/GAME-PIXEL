using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JourneyOfMemories.Editor
{
    [InitializeOnLoad]
    public static class RegisterMenuVerifier
    {
        private const string LOG_FILE_PATH = "Logs/RegisterMenuVerification.log";

        static RegisterMenuVerifier()
        {
            EditorApplication.delayCall += RunVerification;
        }

        [MenuItem("Tools/Verify RegisterMenu Scene & Play Mode")]
        public static void RunVerification()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                return;
            }

            var logLines = new System.Collections.Generic.List<string>();
            logLines.Add($"=== RegisterMenu Verification Run at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");

            bool allPassed = true;
            void Check(string name, bool condition, string details = "")
            {
                if (condition)
                {
                    logLines.Add($"[PASS] {name}" + (string.IsNullOrEmpty(details) ? "" : $" ({details})"));
                }
                else
                {
                    allPassed = false;
                    logLines.Add($"[FAIL] {name} - {details}");
                    Debug.LogError($"[RegisterMenuVerifier FAIL] {name}: {details}");
                }
            }

            try
            {
                Scene scene;
                if (EditorApplication.isPlaying)
                {
                    scene = SceneManager.GetActiveScene();
                    if (!scene.IsValid() || !scene.name.Contains("RegisterMenu"))
                    {
                        return;
                    }
                    Check("Active Scene in Play Mode", true, scene.name);
                }
                else
                {
                    string scenePath = "Assets/Scenes/RegisterMenu.unity";
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    Check("Open Scene", scene.IsValid(), scenePath);
                }

                // 1. Kiểm tra RegisterMenuController
                var controller = UnityEngine.Object.FindAnyObjectByType<RegisterMenuController>();
                Check("RegisterMenuController Exists", controller != null);

                if (controller != null)
                {
                    controller.EnsureLoginPromptExists();
                    Check("UsernameInput Wired", controller.usernameInput != null);
                    Check("EmailInput Wired", controller.emailInput != null);
                    Check("PasswordInput Wired", controller.passwordInput != null);
                    Check("TermsToggle Wired", controller.termsToggle != null);
                    Check("RegisterButton Wired", controller.registerButton != null);
                    Check("BackButton Wired", controller.backButton != null);
                    Check("LoginLinkButton Wired", controller.loginLinkButton != null);
                    Check("StatusText Wired", controller.statusText != null);
                }

                // 2. Kiểm tra TermsToggle Hierarchy & Cleanliness
                var toggleObj = GameObject.Find("TermsToggle");
                Check("TermsToggle Exists", toggleObj != null);

                if (toggleObj != null)
                {
                    var toggle = toggleObj.GetComponent<Toggle>();
                    Check("Toggle Component Attached", toggle != null);

                    var togImg = toggleObj.GetComponent<Image>();
                    Check("Invisible Raycast Target Image Attached", togImg != null && togImg.raycastTarget);

                    // Check child count: Must be EXACTLY 2 (Checkmark and Label, NO duplicate Background)
                    Check("TermsToggle Child Count == 2 (Checkmark and Label)", toggleObj.transform.childCount == 2,
                        $"Found {toggleObj.transform.childCount} children");

                    Transform checkmarkTransform = toggleObj.transform.Find("Checkmark");
                    Check("Checkmark Exists Under TermsToggle", checkmarkTransform != null);

                    Transform labelTransform = toggleObj.transform.Find("Label");
                    Check("Label Exists Under TermsToggle", labelTransform != null);

                    Transform duplicateBg = toggleObj.transform.Find("Background");
                    Check("Duplicate Background Image Removed", duplicateBg == null,
                        duplicateBg != null ? "Found duplicate Background!" : "Clean");

                    if (checkmarkTransform != null)
                    {
                        var checkImg = checkmarkTransform.GetComponent<Image>();
                        Check("Checkmark Has Image Component", checkImg != null);
                        Check("Checkmark Sprite Assigned", checkImg != null && checkImg.sprite != null && checkImg.sprite.name.Contains("checkbox_checked"));

                        var checkRect = checkmarkTransform.GetComponent<RectTransform>();
                        Check("Checkmark RectTransform Position Aligned", checkRect != null &&
                            Mathf.Approximately(checkRect.anchoredPosition.x, -213f) &&
                            Mathf.Approximately(checkRect.anchoredPosition.y, 0f),
                            $"Pos: {checkRect?.anchoredPosition}");

                        Check("Checkmark RectTransform Size Aligned", checkRect != null &&
                            Mathf.Approximately(checkRect.sizeDelta.x, 39f) &&
                            Mathf.Approximately(checkRect.sizeDelta.y, 38f),
                            $"Size: {checkRect?.sizeDelta}");
                    }

                    if (labelTransform != null)
                    {
                        var labelText = labelTransform.GetComponent<Text>();
                        Check("Label Has Text Component", labelText != null);
                        Check("Label Text Content Matches", labelText != null && labelText.text == "Tôi đồng ý với Điều khoản sử dụng",
                            $"Text: '{labelText?.text}'");
                        Check("Label Text Alignment MiddleLeft", labelText != null && labelText.alignment == TextAnchor.MiddleLeft);

                        var labelRect = labelTransform.GetComponent<RectTransform>();
                        Check("Label RectTransform Position Aligned", labelRect != null &&
                            Mathf.Approximately(labelRect.anchoredPosition.x, 24f) &&
                            Mathf.Approximately(labelRect.anchoredPosition.y, 0f),
                            $"Pos: {labelRect?.anchoredPosition}");
                    }

                    // 3. Kiểm tra chức năng Bật/Tắt Checkbox
                    if (toggle != null && controller != null)
                    {
                        // Khi Toggle = OFF: Checkmark phải ẩn
                        toggle.isOn = false;
                        controller.OnTermsToggleChanged(false);
                        Check("Toggle OFF -> Checkmark Hidden", checkmarkTransform != null && !checkmarkTransform.gameObject.activeSelf);

                        // Khi Toggle = ON: Checkmark phải hiện
                        toggle.isOn = true;
                        controller.OnTermsToggleChanged(true);
                        Check("Toggle ON -> Checkmark Visible", checkmarkTransform != null && checkmarkTransform.gameObject.activeSelf);

                        // Đặt lại Toggle về mặc định ban đầu là OFF
                        toggle.isOn = false;
                        controller.OnTermsToggleChanged(false);
                        Check("Toggle Reset to OFF -> Checkmark Hidden", checkmarkTransform != null && !checkmarkTransform.gameObject.activeSelf);
                    }
                }

                // 4. Kiểm tra toàn bộ Scene có đúng 1 Text 'Điều khoản' rõ nét, không bị trùng lặp
                var allTexts = UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include);
                int termsTextCount = 0;
                foreach (var t in allTexts)
                {
                    if (t.text.Contains("Điều khoản") || t.text.Contains("Tôi đồng ý"))
                    {
                        termsTextCount++;
                    }
                }
                Check("Single Sharp Text Component For Terms (Zero duplicate text in scene)", termsTextCount == 1,
                    $"Found {termsTextCount} terms Text components");

                // 5. Kiểm tra Scenes In Build
                bool inBuild = false;
                foreach (var s in EditorBuildSettings.scenes)
                {
                    if (s.path.Contains("RegisterMenu.unity") && s.enabled)
                    {
                        inBuild = true;
                        break;
                    }
                }
                Check("RegisterMenu In Build Settings & Enabled", inBuild);

                // 6. Kiểm tra dòng chữ "Đã có tài khoản? ĐĂNG NHẬP" bên dưới nút QUAY LẠI
                var loginObj = GameObject.Find("LoginPrompt");
                Check("LoginPrompt Exists", loginObj != null);

                if (loginObj != null)
                {
                    var loginBtn = loginObj.GetComponent<Button>();
                    Check("LoginPrompt Has Button Component", loginBtn != null);

                    var loginRect = loginObj.GetComponent<RectTransform>();
                    Check("LoginPrompt Positioned Below BackButton (Y < -248.5)", loginRect != null && loginRect.anchoredPosition.y < -248.5f,
                        $"Pos: {loginRect?.anchoredPosition}, Size: {loginRect?.sizeDelta}");

                    var backObj = GameObject.Find("BackButton");
                    if (backObj != null)
                    {
                        var br = backObj.GetComponent<RectTransform>();
                        logLines.Add($"[DEBUG] BackButton Pos: {br?.anchoredPosition}, WorldPos: {backObj.transform.position}, LossyScale: {backObj.transform.lossyScale}, LocalScale: {backObj.transform.localScale}");
                        if (Camera.main != null)
                        {
                            logLines.Add($"[DEBUG] BackButton ScreenPoint: {Camera.main.WorldToScreenPoint(backObj.transform.position)}");
                        }
                    }

                    logLines.Add($"[DEBUG] LoginPrompt Parent: {loginObj.transform.parent?.name}, LocalPos: {loginObj.transform.localPosition}, AnchoredPos: {loginRect.anchoredPosition}, AnchorMin: {loginRect.anchorMin}, AnchorMax: {loginRect.anchorMax}, Pivot: {loginRect.pivot}, LocalScale: {loginObj.transform.localScale}, LossyScale: {loginObj.transform.lossyScale}, WorldPos: {loginObj.transform.position}");
                    if (Camera.main != null)
                    {
                        Vector3 sp = Camera.main.WorldToScreenPoint(loginObj.transform.position);
                        logLines.Add($"[DEBUG] LoginPrompt ScreenPoint: {sp}");
                    }

                    Transform textChild = loginObj.transform.Find("Text");
                    Check("LoginPrompt Has Text Child", textChild != null);
                    if (textChild != null)
                    {
                        var tComp = textChild.GetComponent<Text>();
                        Check("LoginPrompt Text Has Content", tComp != null && tComp.text.Contains("Đã có tài khoản?") && tComp.text.Contains("ĐĂNG NHẬP"),
                            $"Text: '{tComp?.text}', Font: {tComp?.font?.name}, Size: {tComp?.fontSize}");
                        Check("LoginPrompt Text Has Red Color Tag", tComp != null && tComp.text.Contains("#D23C3C"));

                        logLines.Add($"[DEBUG] TextObj ActiveInHierarchy: {textChild.gameObject.activeInHierarchy}, WorldPos: {textChild.position}, Scale: {textChild.lossyScale}");
                        if (Camera.main != null)
                        {
                            Vector3 tsp = Camera.main.WorldToScreenPoint(textChild.position);
                            logLines.Add($"[DEBUG] Text ScreenPoint: {tsp}");
                        }

                        if (tComp != null)
                        {
                            // Ensure it NEVER truncates or clips due to font metrics
                            tComp.horizontalOverflow = HorizontalWrapMode.Overflow;
                            tComp.verticalOverflow = VerticalWrapMode.Overflow;
                            logLines.Add($"[INFO] Text Preferred Size: ({tComp.preferredWidth}, {tComp.preferredHeight}), Rect: {tComp.rectTransform.rect}, Color: {tComp.color}");
                        }
                    }

                    Transform underChild = loginObj.transform.Find("Underline");
                    Check("LoginPrompt Has Underline Child", underChild != null);
                    if (underChild != null)
                    {
                        var uImg = underChild.GetComponent<Image>();
                        Check("LoginPrompt Underline Color Red", uImg != null && Mathf.Approximately(uImg.color.r, 0.824f));
                        logLines.Add($"[DEBUG] Underline ActiveInHierarchy: {underChild.gameObject.activeInHierarchy}, WorldPos: {underChild.position}, Scale: {underChild.lossyScale}, Rect: {underChild.GetComponent<RectTransform>()?.rect}, Color: {uImg?.color}");
                    }
                }

                // Render actual scene to image file for visual verification
                try
                {
                    Camera cam = Camera.main;
                    Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
                    if (cam != null && canvas != null)
                    {
                        RenderMode prevMode = canvas.renderMode;
                        Camera prevCam = canvas.worldCamera;

                        canvas.renderMode = RenderMode.ScreenSpaceCamera;
                        canvas.worldCamera = cam;

                        // Temporarily disable fade overlay for screenshot
                        var fade = GameObject.Find("ScreenFadeOverlay");
                        CanvasGroup fg = fade != null ? fade.GetComponent<CanvasGroup>() : null;
                        float prevAlpha = fg != null ? fg.alpha : 0f;
                        if (fg != null) fg.alpha = 0f;

                        RenderTexture rt = new RenderTexture(1844, 853, 24);
                        cam.targetTexture = rt;
                        cam.Render();

                        RenderTexture.active = rt;
                        Texture2D tex = new Texture2D(1844, 853, TextureFormat.RGB24, false);
                        tex.ReadPixels(new Rect(0, 0, 1844, 853), 0, 0);
                        tex.Apply();

                        string outPng = "D:/game/picture/reggister/actual_unity_render.png";
                        File.WriteAllBytes(outPng, tex.EncodeToPNG());
                        logLines.Add($"[INFO] Rendered scene to {outPng}");

                        // Restore
                        if (fg != null) fg.alpha = prevAlpha;
                        canvas.renderMode = prevMode;
                        canvas.worldCamera = prevCam;
                        cam.targetTexture = null;
                        RenderTexture.active = null;
                        UnityEngine.Object.DestroyImmediate(rt);
                        UnityEngine.Object.DestroyImmediate(tex);
                    }
                }
                catch (Exception renderEx)
                {
                    logLines.Add($"[WARN] Could not render screenshot: {renderEx.Message}");
                }

                if (!EditorApplication.isPlaying && scene.IsValid())
                {
                    EditorSceneManager.SaveScene(scene);
                }

                logLines.Add($"=== Result: {(allPassed ? "ALL CHECKS PASSED SUCCESSFUL!" : "SOME CHECKS FAILED")} ===");
                Debug.Log($"[RegisterMenuVerifier] Hoàn tất kiểm tra: {(allPassed ? "TẤT CẢ ĐIỀU KIỆN ĐÃ ĐẠT CHUẨN XUẤT SẮC!" : "CÓ LỖI CẦN XỬ LÝ!")}");
            }
            catch (Exception ex)
            {
                allPassed = false;
                logLines.Add($"[EXCEPTION] {ex.Message}\n{ex.StackTrace}");
                Debug.LogException(ex);
            }

            try
            {
                File.WriteAllLines(LOG_FILE_PATH, logLines);
            }
            catch (Exception fileEx)
            {
                Debug.LogWarning($"[RegisterMenuVerifier] Không thể ghi log ra file: {fileEx.Message}");
            }
        }
    }
}
