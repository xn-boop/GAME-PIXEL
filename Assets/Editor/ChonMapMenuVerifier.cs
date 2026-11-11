using System;
using System.IO;
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
    public static class ChonMapMenuVerifier
    {
        private const string LOG_FILE_PATH = "Logs/ChonMapMenuVerification.log";

        [MenuItem("Tools/Build & Verify 'Chọn map' Scene")]
        public static void BuildAndVerify()
        {
            ChonMapMenuSetup.BuildScene();
            RunVerification();
        }

        [MenuItem("Tools/Verify 'Chọn map' Scene")]
        public static void RunVerification()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                return;
            }

            var logLines = new List<string>();
            logLines.Add($"=== 'Chọn map' Verification Run at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");

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
                    Debug.LogError($"[ChonMapMenuVerifier FAIL] {name}: {details}");
                }
            }

            try
            {
                string scenePath = "Assets/Scenes/Chọn map.unity";
                Check("Scene File Exists on Disk", File.Exists(scenePath), scenePath);

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Check("Open Scene 'Chọn map'", scene.IsValid(), scene.name);

                // 1. Kiểm tra Main Camera
                var cameraObj = GameObject.Find("Main Camera");
                Check("Main Camera Exists", cameraObj != null);
                if (cameraObj != null)
                {
                    var cam = cameraObj.GetComponent<Camera>();
                    Check("Camera Component Attached", cam != null);
                    if (cam != null)
                    {
                        Check("Camera Orthographic is True", cam.orthographic);
                        Check("Camera Size is 5.0", Mathf.Approximately(cam.orthographicSize, 5f));
                    }
                    var audioListener = cameraObj.GetComponent<AudioListener>();
                    Check("AudioListener Attached", audioListener != null);
                }

                // 2. Kiểm tra EventSystem duy nhất
                var eventSystems = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
                Check("Single EventSystem in Scene (No duplicates)", eventSystems != null && eventSystems.Length == 1, $"Found {eventSystems?.Length ?? 0}");
                if (eventSystems != null && eventSystems.Length > 0)
                {
                    var inputMod = eventSystems[0].GetComponent<InputSystemUIInputModule>();
                    Check("InputSystemUIInputModule Attached", inputMod != null);
                }

                // 3. Kiểm tra Canvas & CanvasScaler
                var canvasObj = GameObject.Find("Canvas");
                Check("Canvas Exists", canvasObj != null);
                if (canvasObj != null)
                {
                    var canvas = canvasObj.GetComponent<Canvas>();
                    Check("Canvas RenderMode is ScreenSpaceOverlay", canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay);
                    Check("Canvas PixelPerfect is True", canvas != null && canvas.pixelPerfect);

                    var scaler = canvasObj.GetComponent<CanvasScaler>();
                    Check("CanvasScaler Attached", scaler != null);
                    if (scaler != null)
                    {
                        Check("CanvasScaler ScaleMode is ScaleWithScreenSize", scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize);
                        Check("CanvasScaler RefResolution is 1896x830", scaler.referenceResolution == new Vector2(1896, 830), $"Got {scaler.referenceResolution}");
                    }
                }

                // 4. Kiểm tra BackgroundImage & UIFrameAnimator (30 frames)
                var bgObj = GameObject.Find("BackgroundImage");
                Check("BackgroundImage Exists", bgObj != null);
                if (bgObj != null)
                {
                    var bgImg = bgObj.GetComponent<Image>();
                    Check("BackgroundImage Image Attached", bgImg != null);
                    Check("BackgroundImage Sprite Assigned", bgImg != null && bgImg.sprite != null, bgImg != null && bgImg.sprite != null ? bgImg.sprite.name : "null");

                    var animator = bgObj.GetComponent<UIFrameAnimator>();
                    Check("UIFrameAnimator Attached", animator != null);
                    if (animator != null)
                    {
                        Check("UIFrameAnimator Frames Count is 30", animator.frames != null && animator.frames.Length == 30, $"Found {animator.frames?.Length ?? 0}");
                        bool allFramesValid = true;
                        if (animator.frames != null)
                        {
                            for (int i = 0; i < animator.frames.Length; i++)
                            {
                                if (animator.frames[i] == null)
                                {
                                    allFramesValid = false;
                                    break;
                                }
                            }
                        }
                        Check("All 30 Frames Non-Null", allFramesValid);
                        Check("UIFrameAnimator FPS is 10", Mathf.Approximately(animator.framesPerSecond, 10f));
                        Check("UIFrameAnimator Loop is True", animator.loop);
                        Check("UIFrameAnimator UnscaledTime is True", animator.unscaledTime);
                    }
                }

                // 5. Kiểm tra ChonMapMenuController
                var managerObj = GameObject.Find("ChonMapMenuManager");
                Check("ChonMapMenuManager Exists", managerObj != null);
                ChonMapMenuController controller = null;
                if (managerObj != null)
                {
                    controller = managerObj.GetComponent<ChonMapMenuController>();
                    Check("ChonMapMenuController Attached", controller != null);
                }

                // 6. Kiểm tra các nút trên biển gỗ
                if (controller != null)
                {
                    Check("PlayButton Wired", controller.playButton != null);
                    if (controller.playButton != null)
                    {
                        var rt = controller.playButton.GetComponent<RectTransform>();
                        Check("PlayButton Anchored Pos Y is -76.5", Mathf.Approximately(rt.anchoredPosition.y, -76.5f), $"Got {rt.anchoredPosition.y}");
                        Check("PlayButton Size is 350x82", rt.sizeDelta == new Vector2(350f, 82f), $"Got {rt.sizeDelta}");
                        Check("PlayButton Has RegisterButtonEffect", controller.playButton.GetComponent<RegisterButtonEffect>() != null);
                    }

                    Check("SettingsButton Wired", controller.settingsButton != null);
                    if (controller.settingsButton != null)
                    {
                        var rt = controller.settingsButton.GetComponent<RectTransform>();
                        Check("SettingsButton Anchored Pos Y is -172", Mathf.Approximately(rt.anchoredPosition.y, -172f), $"Got {rt.anchoredPosition.y}");
                        Check("SettingsButton Size is 350x82", rt.sizeDelta == new Vector2(350f, 82f), $"Got {rt.sizeDelta}");
                        Check("SettingsButton Has RegisterButtonEffect", controller.settingsButton.GetComponent<RegisterButtonEffect>() != null);
                    }

                    Check("QuitButton Wired", controller.quitButton != null);
                    if (controller.quitButton != null)
                    {
                        var rt = controller.quitButton.GetComponent<RectTransform>();
                        Check("QuitButton Anchored Pos Y is -266", Mathf.Approximately(rt.anchoredPosition.y, -266f), $"Got {rt.anchoredPosition.y}");
                        Check("QuitButton Size is 350x82", rt.sizeDelta == new Vector2(350f, 82f), $"Got {rt.sizeDelta}");
                        Check("QuitButton Has RegisterButtonEffect", controller.quitButton.GetComponent<RegisterButtonEffect>() != null);
                    }

                    // 7. Kiểm tra Settings Panel
                    Check("SettingsPanel Wired", controller.settingsPanel != null);
                    if (controller.settingsPanel != null)
                    {
                        Check("VolumeSlider Wired", controller.volumeSlider != null);
                        Check("SfxSlider Wired", controller.sfxSlider != null);
                        Check("CloseSettingsButton Wired", controller.closeSettingsButton != null);
                        if (controller.closeSettingsButton != null)
                        {
                            Check("CloseSettingsButton Has RegisterButtonEffect", controller.closeSettingsButton.GetComponent<RegisterButtonEffect>() != null);
                        }
                    }

                    // 8. Kiểm tra Map Selection Panel đã được loại bỏ theo yêu cầu người dùng
                    var mapPanelInScene = GameObject.Find("MapSelectionPanel");
                    Check("MapSelectionPanel Removed From Scene", mapPanelInScene == null);
                    Check("PlayButton Target Scene is Map0_Prologue", controller.targetPlaySceneName == "Map0_Prologue");
                    Check("Scene Transition Allowed", controller.allowSceneTransition);

                    // 9. Kiểm tra StatusText & Overlay
                    Check("StatusText Wired", controller.statusText != null);
                    Check("ScreenFadeOverlay Wired", controller.screenFadeOverlay != null);
                }

                // 10. Kiểm tra scene Map0_Prologue tồn tại và hợp lệ
                Check("Map0_Prologue Scene Exists on Disk", File.Exists("Assets/Scenes/Map0_Prologue.unity"));

                if (allPassed)
                {
                    logLines.Add("=== Result: ALL CHECKS PASSED SUCCESSFUL! ===");
                    Debug.Log("[ChonMapMenuVerifier] TẤT CẢ CÁC BƯỚC KIỂM TRA ĐỀU THÀNH CÔNG RỰC RỠ (PASS 100%)!");
                }
                else
                {
                    logLines.Add("=== Result: SOME CHECKS FAILED ===");
                    Debug.LogError("[ChonMapMenuVerifier] MỘT SỐ BƯỚC KIỂM TRA BỊ THẤT BẠI. HÃY KIỂM TRA LOG.");
                }
            }
            catch (Exception ex)
            {
                allPassed = false;
                logLines.Add($"[EXCEPTION] {ex.Message}\n{ex.StackTrace}");
                Debug.LogException(ex);
            }
            finally
            {
                Directory.CreateDirectory("Logs");
                File.WriteAllLines(LOG_FILE_PATH, logLines);
            }
        }
    }
}
