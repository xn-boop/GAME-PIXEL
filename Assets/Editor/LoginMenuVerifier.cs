using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JourneyOfMemories.Editor
{
    [InitializeOnLoad]
    public static class LoginMenuVerifier
    {
        private const string LOG_FILE_PATH = "Logs/LoginMenuVerification.log";

        static LoginMenuVerifier()
        {
            EditorApplication.delayCall += RunVerification;
        }

        [MenuItem("Tools/Build & Verify LoginMenu Scene")]
        public static void BuildAndVerify()
        {
            LoginMenuSetup.BuildScene();
            RunVerification();
        }

        [MenuItem("Tools/Verify LoginMenu Scene")]
        public static void RunVerification()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                return;
            }

            var logLines = new List<string>();
            logLines.Add($"=== LoginMenu Verification Run at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");

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
                    Debug.LogError($"[LoginMenuVerifier FAIL] {name}: {details}");
                }
            }

            try
            {
                Scene scene;
                string scenePath = "Assets/Scenes/LoginMenu.unity";

                if (EditorApplication.isPlaying)
                {
                    scene = SceneManager.GetActiveScene();
                    if (!scene.IsValid() || !scene.name.Contains("LoginMenu"))
                    {
                        return;
                    }
                    Check("Active Scene in Play Mode", true, scene.name);
                }
                else
                {
                    if (!File.Exists(scenePath))
                    {
                        Debug.Log("[LoginMenuVerifier] Scene chưa tồn tại, đang tự động build...");
                        LoginMenuSetup.BuildScene();
                    }
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    Check("Open Scene", scene.IsValid(), scenePath);
                }

                // 1. Kiểm tra EventSystem: Chỉ có đúng 1 EventSystem trong Scene
                var eventSystems = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include);
                Check("Single EventSystem in Scene (No duplicates)", eventSystems.Length == 1, $"Found {eventSystems.Length}");

                // 2. Kiểm tra LoginMenuController
                var controller = UnityEngine.Object.FindAnyObjectByType<LoginMenuController>();
                Check("LoginMenuController Exists", controller != null);

                if (controller != null)
                {
                    Check("EmailInput Wired", controller.emailInput != null);
                    Check("PasswordInput Wired", controller.passwordInput != null);
                    Check("RememberToggle Wired", controller.rememberToggle != null);
                    Check("LoginButton Wired", controller.loginButton != null);
                    Check("BackButton Wired", controller.backButton != null);
                    Check("ForgotPasswordButton Wired", controller.forgotPasswordButton != null);
                    Check("RegisterLinkButton Wired", controller.registerLinkButton != null);
                    Check("StatusText Wired", controller.statusText != null);
                    Check("ScreenFadeOverlay Wired", controller.screenFadeOverlay != null);
                }

                // 3. Kiểm tra InputFields cấu hình
                if (controller != null && controller.passwordInput != null)
                {
                    Check("Password Input ContentType is Password (hidden asterisks)", controller.passwordInput.contentType == InputField.ContentType.Password);
                }

                if (controller != null && controller.emailInput != null)
                {
                    Check("Email Input ContentType is EmailAddress", controller.emailInput.contentType == InputField.ContentType.EmailAddress);
                }

                // 4. Kiểm tra RememberToggle
                var toggleObj = GameObject.Find("RememberToggle");
                Check("RememberToggle Exists", toggleObj != null);

                if (toggleObj != null)
                {
                    var toggle = toggleObj.GetComponent<Toggle>();
                    Check("Toggle Component Attached", toggle != null);

                    Transform checkmarkTransform = toggleObj.transform.Find("Checkmark");
                    Check("Checkmark Exists Under RememberToggle", checkmarkTransform != null);

                    Transform labelTransform = toggleObj.transform.Find("Label");
                    Check("Label Exists Under RememberToggle", labelTransform != null);

                    if (labelTransform != null)
                    {
                        var labelText = labelTransform.GetComponent<Text>();
                        Check("Label Text Matches 'Ghi nhớ đăng nhập'", labelText != null && labelText.text.Contains("Ghi nhớ đăng nhập"),
                            $"Text: '{labelText?.text}'");
                    }

                    // Test toggle ON / OFF functionality
                    if (toggle != null && controller != null)
                    {
                        toggle.isOn = false;
                        controller.OnRememberToggleChanged(false);
                        Check("Toggle OFF -> Checkmark Hidden", checkmarkTransform != null && !checkmarkTransform.gameObject.activeSelf);

                        toggle.isOn = true;
                        controller.OnRememberToggleChanged(true);
                        Check("Toggle ON -> Checkmark Visible", checkmarkTransform != null && checkmarkTransform.gameObject.activeSelf);

                        toggle.isOn = false;
                        controller.OnRememberToggleChanged(false);
                        Check("Toggle Reset to OFF -> Checkmark Hidden", checkmarkTransform != null && !checkmarkTransform.gameObject.activeSelf);
                    }
                }

                // 5. Kiểm tra Quên mật khẩu?
                var forgotObj = GameObject.Find("ForgotPasswordButton");
                Check("ForgotPasswordButton Exists", forgotObj != null);
                if (forgotObj != null)
                {
                    var btn = forgotObj.GetComponent<Button>();
                    Check("ForgotPasswordButton Has Button Component", btn != null);
                    Transform txtChild = forgotObj.transform.Find("Text");
                    Check("ForgotPasswordButton Has Text Child", txtChild != null && txtChild.GetComponent<Text>()?.text.Contains("Quên mật khẩu?") == true);
                }

                // 6. Kiểm tra Dòng "Chưa có tài khoản? ĐĂNG KÝ"
                var regPromptObj = GameObject.Find("RegisterPrompt");
                Check("RegisterPrompt Exists", regPromptObj != null);
                if (regPromptObj != null)
                {
                    var btn = regPromptObj.GetComponent<Button>();
                    Check("RegisterPrompt Has Button Component", btn != null);
                    Transform txtChild = regPromptObj.transform.Find("Text");
                    Check("RegisterPrompt Has Text Child", txtChild != null);
                    if (txtChild != null)
                    {
                        var tComp = txtChild.GetComponent<Text>();
                        Check("RegisterPrompt Text Content Matches", tComp != null && tComp.text.Contains("Chưa có tài khoản?") && tComp.text.Contains("ĐĂNG KÝ"));
                        Check("RegisterPrompt Has Red Color Tag", tComp != null && tComp.text.Contains("#D23C3C"));
                    }
                    Transform underChild = regPromptObj.transform.Find("Underline");
                    Check("RegisterPrompt Has Underline Child", underChild != null);
                }

                // 7. Kiểm tra Scenes In Build
                bool inBuild = false;
                foreach (var s in EditorBuildSettings.scenes)
                {
                    if (s.path.Contains("LoginMenu.unity") && s.enabled)
                    {
                        inBuild = true;
                        break;
                    }
                }
                Check("LoginMenu In Build Settings & Enabled", inBuild);

                // 8. Render Scene View ra ảnh để kiểm tra chất lượng hiển thị trực quan
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

                        var fade = GameObject.Find("ScreenFadeOverlay");
                        CanvasGroup fg = fade != null ? fade.GetComponent<CanvasGroup>() : null;
                        float prevAlpha = fg != null ? fg.alpha : 0f;
                        if (fg != null) fg.alpha = 0f;

                        RenderTexture rt = new RenderTexture(2340, 1080, 24);
                        cam.targetTexture = rt;
                        cam.Render();

                        RenderTexture.active = rt;
                        Texture2D tex = new Texture2D(2340, 1080, TextureFormat.RGB24, false);
                        tex.ReadPixels(new Rect(0, 0, 2340, 1080), 0, 0);
                        tex.Apply();

                        string outPng = "D:/game/picture/login/actual_login_render.png";
                        File.WriteAllBytes(outPng, tex.EncodeToPNG());
                        logLines.Add($"[INFO] Rendered scene to {outPng}");

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
                Debug.Log($"[LoginMenuVerifier] Hoàn tất kiểm tra: {(allPassed ? "TẤT CẢ ĐIỀU KIỆN ĐÃ ĐẠT CHUẨN XUẤT SẮC!" : "CÓ LỖI CẦN XỬ LÝ!")}");
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
                Debug.LogWarning($"[LoginMenuVerifier] Không thể ghi log ra file: {fileEx.Message}");
            }
        }
    }
}
