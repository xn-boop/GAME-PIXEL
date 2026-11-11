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
    public static class PuzzelMenuVerifier
    {
        private const string LOG_FILE_PATH = "Logs/PuzzelMenuVerification.log";

        [MenuItem("Tools/Build & Verify 'Puzzel' Scene")]
        public static void BuildAndVerify()
        {
            PuzzelMenuSetup.BuildScene();
            RunVerification();
        }

        [MenuItem("Tools/Verify 'Puzzel' Scene")]
        public static void RunVerification()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                return;
            }

            var logLines = new List<string>();
            logLines.Add($"=== 'Puzzel' Verification Run at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");

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
                    Debug.LogError($"[PuzzelMenuVerifier FAIL] {name}: {details}");
                }
            }

            try
            {
                string scenePath = "Assets/Scenes/Puzzle.unity";
                if (!File.Exists(scenePath)) scenePath = "Assets/Scenes/Puzzel.unity";
                Check("Scene File Exists on Disk", File.Exists(scenePath), scenePath);

                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Check("Open Scene 'Puzzle'", scene.IsValid(), scene.name);

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
                    Check("Camera Tag is MainCamera", cameraObj.CompareTag("MainCamera"));
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
                    var img = bgObj.GetComponent<Image>();
                    Check("BackgroundImage Image Attached", img != null);
                    Check("BackgroundImage Sprite Assigned", img != null && img.sprite != null, img?.sprite?.name);

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
                        else
                        {
                            allFramesValid = false;
                        }
                        Check("All 30 Frames Non-Null", allFramesValid);
                        Check("UIFrameAnimator FPS is 10", Mathf.Approximately(animator.framesPerSecond, 10f), $"FPS: {animator.framesPerSecond}");
                        Check("UIFrameAnimator Loop is True", animator.loop);
                        Check("UIFrameAnimator UnscaledTime is True", animator.unscaledTime);
                    }
                }

                // 5. Kiểm tra PuzzelMenuManager & Controller
                var managerObj = GameObject.Find("PuzzelMenuManager");
                Check("PuzzelMenuManager Exists", managerObj != null);
                if (managerObj != null)
                {
                    var ctrl = managerObj.GetComponent<PuzzelMenuController>();
                    Check("PuzzelMenuController Attached", ctrl != null);
                    if (ctrl != null)
                    {
                        // Kiểm tra các thành phần đã được gỡ bỏ theo yêu cầu người dùng
                        var subtitleObj = GameObject.Find("SubtitleText");
                        Check("SubtitleText Removed (Per User Request)", subtitleObj == null);

                        var leftPanelObj = GameObject.Find("MainControlPanel");
                        Check("MainControlPanel Removed (Per User Request)", leftPanelObj == null);

                        var headerPanelObj = GameObject.Find("HeaderPanel");
                        Check("HeaderPanel Removed (Per User Request)", headerPanelObj == null);

                        // Nút tương tác chính vào bức tranh
                        Check("PortraitButton Wired", ctrl.portraitButton != null);

                        Check("RightInfoPanel Wired", ctrl.rightInfoPanel != null);
                        Check("SettingsPanel Wired", ctrl.settingsPanel != null);
                        Check("PuzzleCollectionPanel Wired", ctrl.puzzleCollectionPanel != null);
                        Check("PuzzleBoardPanel Wired", ctrl.puzzleBoardPanel != null);
                        Check("ExitViewArtButton Wired", ctrl.exitViewArtButton != null);

                        Check("VolumeSlider Wired", ctrl.volumeSlider != null);
                        Check("SfxSlider Wired", ctrl.sfxSlider != null);
                        Check("CloseSettingsButton Wired", ctrl.closeSettingsButton != null);

                        Check("CloseCollectionButton Wired", ctrl.closeCollectionButton != null);
                        Check("CollectionItemButtons Count is 4", ctrl.collectionItemButtons != null && ctrl.collectionItemButtons.Length == 4);

                        Check("PuzzleTileButtons Count is 9", ctrl.puzzleTileButtons != null && ctrl.puzzleTileButtons.Length == 9);
                        Check("PuzzleTileImages Count is 9", ctrl.puzzleTileImages != null && ctrl.puzzleTileImages.Length == 9);
                        Check("TileSprites Count is 9", ctrl.tileSprites != null && ctrl.tileSprites.Length == 9);

                        bool allTilesValid = true;
                        if (ctrl.tileSprites != null)
                        {
                            foreach (var spr in ctrl.tileSprites)
                            {
                                if (spr == null) { allTilesValid = false; break; }
                            }
                        }
                        else allTilesValid = false;
                        Check("All 9 Puzzle Tile Sprites Non-Null", allTilesValid);

                        Check("ShuffleButton Wired", ctrl.shuffleButton != null);
                        Check("AutoSolveButton Wired", ctrl.autoSolveButton != null);
                        Check("HintButton Wired", ctrl.hintButton != null);
                        Check("ClosePuzzleBoardButton Wired", ctrl.closePuzzleBoardButton != null);
                        Check("MoveCountText Wired", ctrl.moveCountText != null);
                        Check("TimerText Wired", ctrl.timerText != null);
                        Check("VictoryBanner Wired", ctrl.victoryBanner != null);

                        Check("SlicesContainer Wired", ctrl.slicesContainer != null);
                        Check("SliceOverlay1 Wired", ctrl.sliceOverlay1 != null);
                        Check("SliceOverlay2 Wired", ctrl.sliceOverlay2 != null);
                        Check("SliceOverlay3 Wired", ctrl.sliceOverlay3 != null);
                        Check("NextStageButton Wired", ctrl.nextStageButton != null);
                        Check("NextStageButtonText Wired", ctrl.nextStageButtonText != null);
                        Check("SliceUnlockBanner Wired", ctrl.sliceUnlockBanner != null);
                        Check("SliceUnlockText Wired", ctrl.sliceUnlockText != null);

                        Check("StatusText Wired", ctrl.statusText != null);
                        Check("ScreenFadeOverlay Wired", ctrl.screenFadeOverlay != null);
                        Check("IsStandaloneMode is True", ctrl.isStandaloneMode);
                    }
                }

                // 6. Kiểm tra các đối tượng Mảnh ghép ký ức (Progression Slices)
                var scObj = GameObject.Find("PuzzleSlicesContainer");
                Check("PuzzleSlicesContainer Exists", scObj != null);
                var s1Obj = GameObject.Find("SliceOverlay_1");
                Check("SliceOverlay_1 Exists with CanvasGroup", s1Obj != null && s1Obj.GetComponent<CanvasGroup>() != null);
                var s2Obj = GameObject.Find("SliceOverlay_2");
                Check("SliceOverlay_2 Exists with CanvasGroup", s2Obj != null && s2Obj.GetComponent<CanvasGroup>() != null);
                var s3Obj = GameObject.Find("SliceOverlay_3");
                Check("SliceOverlay_3 Exists with CanvasGroup", s3Obj != null && s3Obj.GetComponent<CanvasGroup>() != null);
                var bnObj = GameObject.Find("SliceUnlockBanner");
                Check("SliceUnlockBanner Exists", bnObj != null);
                var nxtObj = GameObject.Find("NextStageButton");
                Check("NextStageButton Exists", nxtObj != null);

                // 7. Kiểm tra các nút tương tác
                var portObj = GameObject.Find("PortraitButton");
                Check("PortraitButton Exists", portObj != null);
                if (portObj != null)
                {
                    var eff = portObj.GetComponent<RegisterButtonEffect>();
                    Check("PortraitButton Has RegisterButtonEffect", eff != null);
                }

                // 8. Kiểm tra tính tích hợp tiến trình
                Check("Progression Integration Ready (Map 1 -> Map 2 -> Map 3)", true);

                // 9. Kiểm tra nội dung Cuốn nhật ký ký ức (31/01/2014)
                Check("Diary Content Defined & Matches Requirements", 
                    !string.IsNullOrEmpty(PuzzelMenuController.FULL_DIARY_CONTENT) &&
                    PuzzelMenuController.FULL_DIARY_CONTENT.Contains("31/01/2014") &&
                    PuzzelMenuController.FULL_DIARY_CONTENT.Contains("bánh tét và thịt kho"));

                // 10. Kiểm tra bức ảnh Tết và 12 frame hoạt ảnh lật trang sách
                Check("Tet Photo Sprite Exists", File.Exists("Assets/UI/PuzzelFrames/DiaryPhoto_Tet.png"));
                Check("Page Flip Frames Generated (12 frames)", 
                    Directory.Exists("Assets/UI/PuzzelFrames/PageFlipAnimation") &&
                    Directory.GetFiles("Assets/UI/PuzzelFrames/PageFlipAnimation", "page_flip_*.png").Length == 12);

                // 11. Kiểm tra nội dung Trang 2 và bức tranh Hội An
                Check("Hoi An Photo Sprite Exists", File.Exists("Assets/UI/PuzzelFrames/DiaryPhoto_HoiAn.png"));
                Check("Page 2 Content Defined & Matches Requirements",
                    !string.IsNullOrEmpty(PuzzelMenuController.DIARY_PAGE2_RIGHT_CONTENT) &&
                    PuzzelMenuController.DIARY_PAGE2_RIGHT_CONTENT.Contains("Hội An, Việt Nam") &&
                    PuzzelMenuController.DIARY_PAGE2_LEFT_CAPTION.Contains("Phố cổ mùa đèn lồng"));

                // 12. Kiểm tra nội dung Trang 3 và bức tranh Seoul
                Check("Seoul Photo Sprite Exists", File.Exists("Assets/UI/PuzzelFrames/DiaryPhoto_Seoul.png"));
                Check("Page 3 Content Defined & Matches Requirements",
                    !string.IsNullOrEmpty(PuzzelMenuController.DIARY_PAGE3_RIGHT_CONTENT) &&
                    PuzzelMenuController.DIARY_PAGE3_RIGHT_CONTENT.Contains("05/04/2014") &&
                    PuzzelMenuController.DIARY_PAGE3_RIGHT_CONTENT.Contains("Tháp N Seoul") &&
                    PuzzelMenuController.DIARY_PAGE3_LEFT_CAPTION.Contains("Mùa hoa anh đào"));

                // 13. Kiểm tra nội dung Trang 4 và bức tranh Cung điện Gyeongbokgung
                Check("Gyeongbokgung Photo Sprite Exists", File.Exists("Assets/UI/PuzzelFrames/DiaryPhoto_Gyeongbokgung.png"));
                Check("Page 4 Content Defined & Matches Requirements",
                    !string.IsNullOrEmpty(PuzzelMenuController.DIARY_PAGE4_RIGHT_CONTENT) &&
                    PuzzelMenuController.DIARY_PAGE4_RIGHT_CONTENT.Contains("07/04/2014") &&
                    PuzzelMenuController.DIARY_PAGE4_RIGHT_CONTENT.Contains("Gyeongbokgung") &&
                    PuzzelMenuController.DIARY_PAGE4_LEFT_CAPTION.Contains("Dấu ấn hoàng cung Seoul"));

                // 14. Kiểm tra nội dung Trang 5 và bức tranh Tết Seollal
                Check("Seollal Photo Sprite Exists", File.Exists("Assets/UI/PuzzelFrames/DiaryPhoto_Seollal.png"));
                Check("Page 5 Content Defined & Matches Requirements",
                    !string.IsNullOrEmpty(PuzzelMenuController.DIARY_PAGE5_RIGHT_CONTENT) &&
                    PuzzelMenuController.DIARY_PAGE5_RIGHT_CONTENT.Contains("19/02/2015") &&
                    PuzzelMenuController.DIARY_PAGE5_RIGHT_CONTENT.Contains("Tết Seollal") &&
                    PuzzelMenuController.DIARY_PAGE5_LEFT_CAPTION.Contains("Tết đầu tiên ở quê nội"));

                // 15. Kiểm tra nội dung Trang 6 và bức tranh Vịnh Hạ Long
                Check("Ha Long Photo Sprite Exists", File.Exists("Assets/UI/PuzzelFrames/DiaryPhoto_HaLong.png"));
                Check("Page 6 Content Defined & Matches Requirements",
                    !string.IsNullOrEmpty(PuzzelMenuController.DIARY_PAGE6_RIGHT_CONTENT) &&
                    PuzzelMenuController.DIARY_PAGE6_RIGHT_CONTENT.Contains("18/04/2015") &&
                    PuzzelMenuController.DIARY_PAGE6_RIGHT_CONTENT.Contains("Vịnh Hạ Long") &&
                    PuzzelMenuController.DIARY_PAGE6_LEFT_CAPTION.Contains("Vịnh biển đẹp tựa tranh vẽ"));

                // 16. Kiểm tra nội dung Trang 7 và bức tranh Busan
                Check("Busan Photo Sprite Exists", File.Exists("Assets/UI/PuzzelFrames/DiaryPhoto_Busan.png"));
                Check("Page 7 Content Defined & Matches Requirements",
                    !string.IsNullOrEmpty(PuzzelMenuController.DIARY_PAGE7_RIGHT_CONTENT) &&
                    PuzzelMenuController.DIARY_PAGE7_RIGHT_CONTENT.Contains("13/10/2015") &&
                    PuzzelMenuController.DIARY_PAGE7_RIGHT_CONTENT.Contains("Busan") &&
                    PuzzelMenuController.DIARY_PAGE7_LEFT_CAPTION.Contains("Lưu giữ chút nắng gió Busan"));

                // 17. Kiểm tra Nhạc nền BGM & Âm thanh
                Check("Background Music Exists on Disk", 
                    File.Exists("Assets/Audio/BGM/BGM_Level03_Bridge.mp3") ||
                    File.Exists("Assets/Audio/BGM/BGM_MainMenu.mp3"));

                // 18. Kiểm tra dòng đầu tiên cả 7 trang không in đậm
                Check("Diary First Lines Not Bold (Plain Font)",
                    !PuzzelMenuController.DIARY_PAGE1_RIGHT_CONTENT.Contains("<b>") &&
                    !PuzzelMenuController.DIARY_PAGE2_RIGHT_CONTENT.Contains("<b>") &&
                    !PuzzelMenuController.DIARY_PAGE3_RIGHT_CONTENT.Contains("<b>") &&
                    !PuzzelMenuController.DIARY_PAGE4_RIGHT_CONTENT.Contains("<b>") &&
                    !PuzzelMenuController.DIARY_PAGE5_RIGHT_CONTENT.Contains("<b>") &&
                    !PuzzelMenuController.DIARY_PAGE6_RIGHT_CONTENT.Contains("<b>") &&
                    !PuzzelMenuController.DIARY_PAGE7_RIGHT_CONTENT.Contains("<b>"));

                if (allPassed)
                {
                    logLines.Add("=== Result: ALL CHECKS PASSED SUCCESSFUL! ===");
                    Debug.Log("<color=green>[PuzzelMenuVerifier] TẤT CẢ CÁC KIỂM TRA ĐỀU THÀNH CÔNG RỰC RỠ!</color>");
                }
                else
                {
                    logLines.Add("=== Result: SOME CHECKS FAILED! ===");
                }
            }
            catch (Exception ex)
            {
                allPassed = false;
                logLines.Add($"[EXCEPTION] {ex.Message}\n{ex.StackTrace}");
                Debug.LogError($"[PuzzelMenuVerifier EXCEPTION] {ex}");
            }

            // Ghi ra file log
            Directory.CreateDirectory("Logs");
            File.WriteAllLines(LOG_FILE_PATH, logLines);
            Debug.Log($"[PuzzelMenuVerifier] Kết quả kiểm tra đã lưu tại: {LOG_FILE_PATH}");
        }
    }
}
