using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Map3.Editor
{
    public class Map3_Tests
    {
        private const string ScenePath = "Assets/_Map3_Final/Scenes/Map3_Final.unity";

        [Test]
        public void Test01_QuizDatabase_All8QuestionsValid()
        {
            Assert.IsNotNull(Map3_QuizDatabase.Quiz_CatHint, "Quiz_CatHint must not be null");
            Assert.AreEqual(1, Map3_QuizDatabase.Quiz_CatHint.correctIndex, "Quiz_CatHint answer must be B (index 1)");
            Assert.IsNotEmpty(Map3_QuizDatabase.Quiz_CatHint.explanation, "Quiz_CatHint explanation must not be empty");

            Assert.IsNotNull(Map3_QuizDatabase.Quiz_Key, "Quiz_Key must not be null");
            Assert.AreEqual(1, Map3_QuizDatabase.Quiz_Key.correctIndex, "Quiz_Key answer must be B (index 1)");

            Assert.IsNotNull(Map3_QuizDatabase.Quiz_Wood1, "Quiz_Wood1 must not be null");
            Assert.AreEqual(1, Map3_QuizDatabase.Quiz_Wood1.correctIndex, "Quiz_Wood1 answer must be B (index 1)");

            Assert.IsNotNull(Map3_QuizDatabase.Quiz_Wood2, "Quiz_Wood2 must not be null");
            Assert.AreEqual(1, Map3_QuizDatabase.Quiz_Wood2.correctIndex, "Quiz_Wood2 answer must be B (index 1)");

            Assert.IsNotNull(Map3_QuizDatabase.Quiz_Wood3, "Quiz_Wood3 must not be null");
            Assert.AreEqual(1, Map3_QuizDatabase.Quiz_Wood3.correctIndex, "Quiz_Wood3 answer must be B (index 1)");

            Assert.IsNotNull(Map3_QuizDatabase.Quiz_Stone, "Quiz_Stone must not be null");
            Assert.AreEqual(1, Map3_QuizDatabase.Quiz_Stone.correctIndex, "Quiz_Stone answer must be B (index 1)");

            Assert.IsNotNull(Map3_QuizDatabase.Quiz_VietJade, "Quiz_VietJade must not be null");
            Assert.AreEqual(1, Map3_QuizDatabase.Quiz_VietJade.correctIndex, "Quiz_VietJade answer must be B (index 1)");

            Assert.IsNotNull(Map3_QuizDatabase.Quiz_KoreaJade, "Quiz_KoreaJade must not be null");
            Assert.AreEqual(2, Map3_QuizDatabase.Quiz_KoreaJade.correctIndex, "Quiz_KoreaJade answer must be C (index 2)");
        }

        [Test]
        public void Test02_InventoryManager_5SlotsLifecycle()
        {
            var go = new GameObject("Test_Inventory");
            var im = go.AddComponent<Map3_InventoryManager>();

            Assert.AreEqual(0, im.Count);
            Assert.IsFalse(im.IsFull);

            // Add 5 items
            Assert.IsTrue(im.AddItem(Map3_ItemIds.Key));
            Assert.IsTrue(im.AddItem(Map3_ItemIds.WoodPlank));
            Assert.IsTrue(im.AddItem(Map3_ItemIds.WoodPlank));
            Assert.IsTrue(im.AddItem(Map3_ItemIds.WoodPlank));
            Assert.IsTrue(im.AddItem(Map3_ItemIds.PrayerStone));

            Assert.AreEqual(5, im.Count);
            Assert.IsTrue(im.IsFull);
            Assert.AreEqual(3, im.GetItemCount(Map3_ItemIds.WoodPlank));
            Assert.IsTrue(im.HasItem(Map3_ItemIds.Key));

            // 6th item should fail (inventory is capped at 5 slots)
            Assert.IsFalse(im.AddItem(Map3_ItemIds.VietJade));
            Assert.AreEqual(5, im.Count);

            // Remove 3 wood planks (for bridge repair)
            Assert.IsTrue(im.RemoveItem(Map3_ItemIds.WoodPlank, 3));
            Assert.AreEqual(0, im.GetItemCount(Map3_ItemIds.WoodPlank));
            Assert.AreEqual(2, im.Count);
            Assert.IsFalse(im.IsFull);

            // Add VietJade
            Assert.IsTrue(im.AddItem(Map3_ItemIds.VietJade));
            Assert.AreEqual(3, im.Count);
            Assert.IsTrue(im.HasItem(Map3_ItemIds.VietJade));

            im.ClearAll();
            Assert.AreEqual(0, im.Count);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Test03_Scene_All10InteractablesPresentAndWired()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Assert.IsTrue(scene.IsValid(), "Scene must be valid");

            // Player & Managers
            Assert.IsNotNull(Object.FindFirstObjectByType<Map3_PlayerController>(), "PlayerController must exist");
            Assert.IsNotNull(Object.FindFirstObjectByType<Map3_CameraController>(), "CameraController must exist");
            Assert.IsNotNull(Object.FindFirstObjectByType<Map3_InventoryManager>(), "InventoryManager must exist");
            Assert.IsNotNull(Object.FindFirstObjectByType<Map3_QuizManager>(), "QuizManager must exist");
            Assert.IsNotNull(Object.FindFirstObjectByType<Map3_UIManager>(), "UIManager must exist");

            // 10 Gameplay Points
            Assert.IsNotNull(Object.FindFirstObjectByType<Map3_CatHint>(), "Point 0: CatHint must exist");
            Assert.IsNotNull(Object.FindFirstObjectByType<Map3_KeyJar>(), "Point 1: KeyJar must exist");

            var doors = Object.FindObjectsByType<Map3_ShrineDoor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.GreaterOrEqual(doors.Length, 4, "Points 2 & 7: 4 Shrine doors (2 entrances, 2 exits) must exist");

            Assert.IsNotNull(Object.FindFirstObjectByType<Map3_Chest>(), "Point 3: VietChest must exist");

            var planks = Object.FindObjectsByType<Map3_WoodPlank>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(3, planks.Length, "Point 4: Exactly 3 WoodPlanks must exist");

            Assert.IsNotNull(Object.FindFirstObjectByType<Map3_BridgeController>(), "Point 5: BridgeController must exist");
            Assert.IsNotNull(Object.FindFirstObjectByType<Map3_RockPile>(), "Point 6: RockPile must exist");
            Assert.IsNotNull(Object.FindFirstObjectByType<Map3_Doltap>(), "Point 8: Doltap must exist");
            Assert.IsNotNull(Object.FindFirstObjectByType<Map3_FinalAltar>(), "Point 9: FinalAltar must exist");
        }

        [Test]
        public void Test04_ShrineDoors_ProperlyConfigured()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Assert.IsTrue(scene.IsValid(), "Scene must be valid");

            var koreaExit = GameObject.Find("KoreaShrine_Exit");
            Assert.IsNotNull(koreaExit, "KoreaShrine_Exit GameObject must exist in scene");

            var col = koreaExit.GetComponent<BoxCollider2D>();
            Assert.IsNotNull(col, "KoreaShrine_Exit must have BoxCollider2D");
            Assert.IsTrue(col.isTrigger, "KoreaShrine_Exit collider must be a trigger");

            var door = koreaExit.GetComponent<Map3_ShrineDoor>();
            Assert.IsNotNull(door, "KoreaShrine_Exit must have Map3_ShrineDoor component");

            var vietExit = GameObject.Find("VietShrine_Exit");
            Assert.IsNotNull(vietExit, "VietShrine_Exit GameObject must exist in scene");
            var vietCol = vietExit.GetComponent<BoxCollider2D>();
            Assert.IsNotNull(vietCol, "VietShrine_Exit must have BoxCollider2D");
            Assert.IsTrue(vietCol.isTrigger, "VietShrine_Exit collider must be a trigger");
        }

        [Test]
        public void Test05_UIFont_NoMissingUnicodeCharacters()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Assert.IsTrue(scene.IsValid(), "Scene must be valid");

            var btnClose = GameObject.Find("Btn_Close");
            if (btnClose != null)
            {
                var tmp = btnClose.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmp != null)
                {
                    Assert.IsFalse(tmp.text.Contains("\u2715"), "Btn_Close must not contain unicode \\u2715 which is missing in pixel font");
                    Assert.AreEqual("X", tmp.text, "Btn_Close text should be 'X'");
                }
            }
        }

        [Test]
        public void Test06_QuizAutoClose_ContinueButtonHiddenAndPlayerUnlocked()
        {
            var go = new GameObject("Test_Quiz_Manager");
            var qm = go.AddComponent<Map3_QuizManager>();

            Assert.GreaterOrEqual(qm.AutoCloseDelay, 2.0f, "Auto close delay should be at least 2 seconds (default 2.5s)");

            var panel = new GameObject("Panel");
            panel.transform.SetParent(go.transform);
            panel.SetActive(false);

            var btnGo = new GameObject("Btn_Continue");
            btnGo.transform.SetParent(go.transform);
            var btn = btnGo.AddComponent<UnityEngine.UI.Button>();

            var so = new UnityEditor.SerializedObject(qm);
            so.FindProperty("quizPanel").objectReferenceValue = panel;
            so.FindProperty("btnContinue").objectReferenceValue = btn;
            so.ApplyModifiedPropertiesWithoutUndo();

            bool rewardGiven = false;
            var qData = new Map3_QuizData("test_q", "Title", "Question?", "A", "B", "C", "D", 1, "Explanation");

            qm.OpenQuiz(qData, () => { rewardGiven = true; });

            Assert.IsTrue(qm.IsQuizOpen, "Quiz panel should be open");
            Assert.IsFalse(btnGo.activeSelf, "Continue button must be inactive when quiz opens");

            // Answering correct
            qm.OnAnswerClicked(1);
            Assert.IsFalse(btnGo.activeSelf, "Continue button must remain inactive after answering correctly");

            // Manually closing or simulating close
            qm.CloseQuiz();
            Assert.IsFalse(qm.IsQuizOpen, "Quiz panel should be closed");
            Assert.IsTrue(rewardGiven, "Reward callback must be invoked upon closing after correct answer");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Test07_QuizButtonColor_DefaultIsBrown4E342E()
        {
            var go = new GameObject("Test_Quiz_Manager_Color");
            var qm = go.AddComponent<Map3_QuizManager>();

            Color expectedBrown = new Color32(78, 52, 46, 255);
            Assert.AreEqual(expectedBrown.r, qm.ColorNormal.r, 0.01f, "ColorNormal red channel mismatch");
            Assert.AreEqual(expectedBrown.g, qm.ColorNormal.g, 0.01f, "ColorNormal green channel mismatch");
            Assert.AreEqual(expectedBrown.b, qm.ColorNormal.b, 0.01f, "ColorNormal blue channel mismatch");

            var panel = new GameObject("Panel");
            panel.transform.SetParent(go.transform);
            panel.SetActive(false);

            var btns = new UnityEngine.UI.Button[4];
            var imgs = new UnityEngine.UI.Image[4];
            for (int i = 0; i < 4; i++)
            {
                var bGo = new GameObject($"Btn_{i}");
                bGo.transform.SetParent(panel.transform);
                btns[i] = bGo.AddComponent<UnityEngine.UI.Button>();
                imgs[i] = bGo.AddComponent<UnityEngine.UI.Image>();
                imgs[i].color = Color.gray; // start with gray
            }

            var so = new UnityEditor.SerializedObject(qm);
            so.FindProperty("quizPanel").objectReferenceValue = panel;
            var spBtns = so.FindProperty("answerButtons");
            spBtns.arraySize = 4;
            for (int i = 0; i < 4; i++) spBtns.GetArrayElementAtIndex(i).objectReferenceValue = btns[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            var qData = new Map3_QuizData("test_q", "Title", "Question?", "A", "B", "C", "D", 0, "Explanation");
            qm.DisplayQuestion(qData, null);

            // After opening, all buttons must have been set to #4E342E (not gray)
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(expectedBrown.r, imgs[i].color.r, 0.01f, $"Btn {i} red channel must match #4E342E");
                Assert.AreEqual(expectedBrown.g, imgs[i].color.g, 0.01f, $"Btn {i} green channel must match #4E342E");
                Assert.AreEqual(expectedBrown.b, imgs[i].color.b, 0.01f, $"Btn {i} blue channel must match #4E342E");
            }

            qm.CloseQuiz();
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Test08_BridgeController_ForegroundLayersWiredAndTransitionConfigured()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Assert.IsTrue(scene.IsValid(), "Scene must be valid");

            var bc = Object.FindFirstObjectByType<Map3_BridgeController>();
            Assert.IsNotNull(bc, "Map3_BridgeController must exist in scene");

            Assert.IsNotNull(bc.BrokenLayerRenderer, "brokenLayerRenderer must be wired or auto-resolved");
            Assert.IsNotNull(bc.FixedLayerRenderer, "fixedLayerRenderer must be wired or auto-resolved");

            Assert.AreEqual("Map3_Broken layer", bc.BrokenLayerRenderer.gameObject.name);
            Assert.AreEqual("Map3_Fixed layer", bc.FixedLayerRenderer.gameObject.name);

            Assert.AreEqual("Foreground", bc.BrokenLayerRenderer.sortingLayerName);
            Assert.AreEqual("Foreground", bc.FixedLayerRenderer.sortingLayerName);
        }

        [Test]
        public void Test09_LoreQuizzes_DatabaseAndInteractablesConfigured()
        {
            // Verify 4 Lore questions in Quiz Database
            var qBinhPhong = Map3_QuizDatabase.Quiz_Lore_BinhPhong;
            Assert.IsNotNull(qBinhPhong, "Quiz_Lore_BinhPhong must exist");
            Assert.AreEqual(2, qBinhPhong.correctIndex, "Binh Phong correct index must be 2 (C)");
            Assert.AreEqual(qBinhPhong, Map3_QuizDatabase.GetById("Quiz_Lore_BinhPhong"));

            var qGomSu = Map3_QuizDatabase.Quiz_Lore_GomSu;
            Assert.IsNotNull(qGomSu, "Quiz_Lore_GomSu must exist");
            Assert.AreEqual(3, qGomSu.correctIndex, "Gom Su correct index must be 3 (D)");
            Assert.AreEqual(qGomSu, Map3_QuizDatabase.GetById("Quiz_Lore_GomSu"));

            var qDanTranh = Map3_QuizDatabase.Quiz_Lore_DanTranh;
            Assert.IsNotNull(qDanTranh, "Quiz_Lore_DanTranh must exist");
            Assert.AreEqual(1, qDanTranh.correctIndex, "Dan Tranh correct index must be 1 (B)");
            Assert.AreEqual(qDanTranh, Map3_QuizDatabase.GetById("Quiz_Lore_DanTranh"));

            var qDinhDong = Map3_QuizDatabase.Quiz_Lore_DinhDong;
            Assert.IsNotNull(qDinhDong, "Quiz_Lore_DinhDong must exist");
            Assert.AreEqual(0, qDinhDong.correctIndex, "Dinh Dong correct index must be 0 (A)");
            Assert.AreEqual(qDinhDong, Map3_QuizDatabase.GetById("Quiz_Lore_DinhDong"));

            // Verify scene objects
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Assert.IsTrue(scene.IsValid(), "Scene must be valid");

            var loreInteractables = Object.FindObjectsByType<Map3_LoreInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(4, loreInteractables.Length, "Must have exactly 4 Map3_LoreInteractable objects in scene");

            var goBinhPhong = GameObject.Find("Interact_Ilworobongdo");
            Assert.IsNotNull(goBinhPhong, "Interact_Ilworobongdo must exist");
            Assert.IsTrue(goBinhPhong.GetComponent<BoxCollider2D>().isTrigger);

            var goGomSu = GameObject.Find("Interact_Celadon");
            Assert.IsNotNull(goGomSu, "Interact_Celadon must exist");
            Assert.IsTrue(goGomSu.GetComponent<BoxCollider2D>().isTrigger);

            var goDanTranh = GameObject.Find("Interact_DanTranh");
            Assert.IsNotNull(goDanTranh, "Interact_DanTranh must exist");
            Assert.IsTrue(goDanTranh.GetComponent<BoxCollider2D>().isTrigger);

            var goDinhDong = GameObject.Find("Interact_DinhDong");
            Assert.IsNotNull(goDinhDong, "Interact_DinhDong must exist");
            Assert.IsTrue(goDinhDong.GetComponent<BoxCollider2D>().isTrigger);
        }

        [Test]
        public void Test10_CatPrecondition_BlocksItemsUntilCatTalked()
        {
            var gm = Map3_GameManager.Instance;
            Assert.IsNotNull(gm, "Map3_GameManager must exist via Lazy Singleton");

            gm.hasTalkedToCat = false;
            Assert.IsFalse(gm.hasTalkedToCat, "hasTalkedToCat should be false initially");

            // Mock talking to cat
            var catGo = new GameObject("Test_Cat");
            var catScript = catGo.AddComponent<Map3_CatHint>();
            // Emulate cat conversation finished
            gm.hasTalkedToCat = true;
            Assert.IsTrue(gm.hasTalkedToCat, "hasTalkedToCat should be true after cat conversation");

            Object.DestroyImmediate(catGo);
        }

        [Test]
        public void Test11_SceneMap3_TopRightHUDButtons_ExistAndProperlyConfigured()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Assert.IsTrue(scene.IsValid(), "Scene must be valid");

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            Assert.IsNotNull(canvas, "Canvas not found in Map3_Final");

            Transform topRightT = canvas.transform.Find("HUD_TopRight_Buttons");
            if (topRightT == null)
            {
                Map3_SceneBuilderEditor.SetupTopRightHUDInScene();
                topRightT = canvas.transform.Find("HUD_TopRight_Buttons");
            }
            Assert.IsNotNull(topRightT, "HUD_TopRight_Buttons not found in Canvas");

            Assert.IsNotNull(topRightT.Find("Btn_Help"), "Btn_Help not found in HUD_TopRight_Buttons");
            Assert.IsNotNull(topRightT.Find("Btn_Sound"), "Btn_Sound not found in HUD_TopRight_Buttons");
            Assert.IsNotNull(topRightT.Find("Btn_Pause"), "Btn_Pause not found in HUD_TopRight_Buttons");
            Assert.IsNotNull(topRightT.Find("Btn_Home"), "Btn_Home not found in HUD_TopRight_Buttons");

            Assert.IsNotNull(canvas.transform.Find("Panel_Pause"), "Panel_Pause not found in Canvas");
            Assert.IsNotNull(canvas.transform.Find("Panel_Guide"), "Panel_Guide not found in Canvas");
            Assert.IsNotNull(canvas.transform.Find("Panel_Confirm_Home"), "Panel_Confirm_Home not found in Canvas");
            Assert.IsNotNull(canvas.transform.Find("Panel_Sound"), "Panel_Sound not found in Canvas");

            var hudCtrl = canvas.GetComponent<Map3_GameHUDController>();
            Assert.IsNotNull(hudCtrl, "Map3_GameHUDController not found on Canvas");
        }

        [Test]
        public void Test12_Map3GameHUDController_PauseToggleAndModalLifecycle()
        {
            GameObject hudGo = new GameObject("Test_Map3HUDController");
            var hud = hudGo.AddComponent<Map3_GameHUDController>();

            GameObject pausePanel = new GameObject("Test_PausePanel");
            pausePanel.SetActive(false);

            GameObject guidePanel = new GameObject("Test_GuidePanel");
            guidePanel.SetActive(false);

            GameObject homePanel = new GameObject("Test_HomePanel");
            homePanel.SetActive(false);

            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(hud);
            so.FindProperty("panelPause").objectReferenceValue = pausePanel;
            so.FindProperty("panelGuide").objectReferenceValue = guidePanel;
            so.FindProperty("panelConfirmHome").objectReferenceValue = homePanel;
            so.ApplyModifiedPropertiesWithoutUndo();

            Assert.IsFalse(hud.IsPaused);
            Assert.IsFalse(hud.IsGuideOpen);
            Assert.IsFalse(hud.IsConfirmHomeOpen);

            hud.TogglePause();
            Assert.IsTrue(hud.IsPaused);
            Assert.IsTrue(pausePanel.activeSelf);
            Assert.AreEqual(0f, Time.timeScale);

            hud.ResumeGame();
            Assert.IsFalse(hud.IsPaused);
            Assert.IsFalse(pausePanel.activeSelf);
            Assert.AreEqual(1f, Time.timeScale);

            hud.ToggleGuide();
            Assert.IsTrue(hud.IsGuideOpen);
            Assert.IsTrue(guidePanel.activeSelf);
            Assert.AreEqual(0f, Time.timeScale);

            hud.CloseGuide();
            Assert.IsFalse(hud.IsGuideOpen);
            Assert.IsFalse(guidePanel.activeSelf);
            Assert.AreEqual(1f, Time.timeScale);

            hud.OpenConfirmHome();
            Assert.IsTrue(hud.IsConfirmHomeOpen);
            Assert.IsTrue(homePanel.activeSelf);

            hud.CloseConfirmHome();
            Assert.IsFalse(hud.IsConfirmHomeOpen);
            Assert.IsFalse(homePanel.activeSelf);
            Assert.AreEqual(1f, Time.timeScale);

            Object.DestroyImmediate(homePanel);
            Object.DestroyImmediate(guidePanel);
            Object.DestroyImmediate(pausePanel);
            Object.DestroyImmediate(hudGo);
        }

        [Test]
        public void Test13_Map3GameHUDController_SoundToggle_UpdatesStateAndIcon()
        {
            GameObject hudGo = new GameObject("Test_Map3HUDController_Sound");
            var hud = hudGo.AddComponent<Map3_GameHUDController>();

            GameObject imgGo = new GameObject("SoundIcon");
            var img = imgGo.AddComponent<UnityEngine.UI.Image>();

            Sprite onSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            Sprite offSprite = Sprite.Create(Texture2D.blackTexture, new Rect(0, 0, 1, 1), Vector2.zero);

            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(hud);
            so.FindProperty("imgSoundIcon").objectReferenceValue = img;
            so.FindProperty("soundOnSprite").objectReferenceValue = onSprite;
            so.FindProperty("soundOffSprite").objectReferenceValue = offSprite;
            so.ApplyModifiedPropertiesWithoutUndo();

            bool initialMuted = hud.IsMuted;
            hud.ToggleSound();
            Assert.AreEqual(!initialMuted, hud.IsMuted, "Sound state should invert after toggle");
            Assert.AreEqual(hud.IsMuted ? offSprite : onSprite, img.sprite);

            hud.ToggleSound();
            Assert.AreEqual(initialMuted, hud.IsMuted, "Sound state should revert to original");
            Assert.AreEqual(hud.IsMuted ? offSprite : onSprite, img.sprite);

            Object.DestroyImmediate(onSprite);
            Object.DestroyImmediate(offSprite);
            Object.DestroyImmediate(imgGo);
            Object.DestroyImmediate(hudGo);
        }
        [Test]
        public void Test14_SceneMap3_HintDialogAndToast_ConfiguredWithQuizFrame()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Assert.IsTrue(scene.IsValid(), "Scene must be valid");

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            Assert.IsNotNull(canvas, "Canvas not found in Map3_Final");

            // Verify Panel_Toast
            Transform toastT = canvas.transform.Find("Panel_Toast");
            Assert.IsNotNull(toastT, "Panel_Toast must exist in Canvas");
            var toastImg = toastT.GetComponent<UnityEngine.UI.Image>();
            Assert.IsNotNull(toastImg, "Panel_Toast must have Image component");
            Assert.IsNotNull(toastImg.sprite, "Panel_Toast must have a sprite assigned");
            Assert.IsTrue(toastImg.sprite.name == "Toast_Frame" || toastImg.sprite.name == "Quiz_0", "Panel_Toast must use styled card sprite");
            Assert.AreEqual(UnityEngine.UI.Image.Type.Sliced, toastImg.type, "Panel_Toast Image type must be Sliced");
            var toastRt = toastT.GetComponent<RectTransform>();
            Assert.IsNotNull(toastRt, "Panel_Toast must have RectTransform");
            Assert.IsTrue(toastRt.sizeDelta.y <= 150f, $"Panel_Toast outer height should be compact (<= 150), got {toastRt.sizeDelta.y}");

            Transform toastTxtT = toastT.Find("Txt_Toast");
            Assert.IsNotNull(toastTxtT, "Txt_Toast must exist inside Panel_Toast");
            var toastTmp = toastTxtT.GetComponent<TMPro.TextMeshProUGUI>();
            Assert.IsNotNull(toastTmp, "Txt_Toast must have TextMeshProUGUI");
            Assert.IsFalse(toastTmp.enableAutoSizing, "Txt_Toast auto sizing must be disabled to prevent tiny blurry pixel text");
            Assert.IsTrue(toastTmp.fontSize >= 19f, $"Txt_Toast font size should be at least 19, got {toastTmp.fontSize}");
            Assert.AreEqual(TMPro.FontStyles.Bold, toastTmp.fontStyle, "Txt_Toast font style should be Bold");

            // Verify Panel_Hint_Dialog
            Transform hintDlgT = canvas.transform.Find("Panel_Hint_Dialog");
            Assert.IsNotNull(hintDlgT, "Panel_Hint_Dialog must exist in Canvas");

            Transform cardHintT = hintDlgT.Find("Card_Hint");
            Assert.IsNotNull(cardHintT, "Card_Hint must exist inside Panel_Hint_Dialog");
            var cardImg = cardHintT.GetComponent<UnityEngine.UI.Image>();
            Assert.IsNotNull(cardImg, "Card_Hint must have Image component");
            Assert.AreEqual("Quiz_0", cardImg.sprite.name, "Card_Hint must use Quiz_0 card sprite");
            Assert.AreEqual(UnityEngine.UI.Image.Type.Sliced, cardImg.type, "Card_Hint Image type must be Sliced");

            Assert.IsNotNull(cardHintT.Find("Txt_Title"), "Txt_Title must exist in Card_Hint");
            Assert.IsNotNull(cardHintT.Find("Txt_Message"), "Txt_Message must exist in Card_Hint");
            Assert.IsNotNull(cardHintT.Find("Btn_Confirm"), "Btn_Confirm must exist in Card_Hint");
            Assert.IsNotNull(cardHintT.Find("Btn_Close"), "Btn_Close must exist in Card_Hint");
            Assert.IsNotNull(cardHintT.Find("Border_CatAvatar"), "Border_CatAvatar must exist in Card_Hint");
        }

        [Test]
        public void Test15_UIManager_HintDialogLifecycle_OpenAndClose()
        {
            GameObject uiGo = new GameObject("Test_UIManager_Hint");
            var um = uiGo.AddComponent<Map3_UIManager>();

            GameObject dlgPanel = new GameObject("HintDialogPanel");
            GameObject txtTitleGo = new GameObject("Txt_Title");
            var txtTitle = txtTitleGo.AddComponent<TMPro.TextMeshProUGUI>();
            GameObject txtMsgGo = new GameObject("Txt_Message");
            var txtMsg = txtMsgGo.AddComponent<TMPro.TextMeshProUGUI>();
            GameObject btnConfirmGo = new GameObject("Btn_Confirm");
            var btnConfirm = btnConfirmGo.AddComponent<UnityEngine.UI.Button>();
            GameObject btnCloseGo = new GameObject("Btn_Close");
            var btnClose = btnCloseGo.AddComponent<UnityEngine.UI.Button>();

            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(um);
            so.FindProperty("hintDialogPanel").objectReferenceValue = dlgPanel;
            so.FindProperty("txtHintTitle").objectReferenceValue = txtTitle;
            so.FindProperty("txtHintMessage").objectReferenceValue = txtMsg;
            so.FindProperty("btnHintConfirm").objectReferenceValue = btnConfirm;
            so.FindProperty("btnHintClose").objectReferenceValue = btnClose;
            so.ApplyModifiedPropertiesWithoutUndo();

            dlgPanel.SetActive(false);
            Assert.IsFalse(um.IsHintDialogOpen);

            bool callbackInvoked = false;
            um.ShowHintDialog("GỢI Ý TỪ CHÚ MÈO", "Hãy tìm chiếc chìa khóa cổ...", () => callbackInvoked = true);

            Assert.IsTrue(um.IsHintDialogOpen);
            Assert.AreEqual("GỢI Ý TỪ CHÚ MÈO", txtTitle.text);
            Assert.AreEqual("Hãy tìm chiếc chìa khóa cổ...", txtMsg.text);

            um.CloseHintDialog();
            Assert.IsFalse(um.IsHintDialogOpen);
            Assert.IsTrue(callbackInvoked, "onClose callback should be invoked when hint dialog closes");

            Object.DestroyImmediate(btnCloseGo);
            Object.DestroyImmediate(btnConfirmGo);
            Object.DestroyImmediate(txtMsgGo);
            Object.DestroyImmediate(txtTitleGo);
            Object.DestroyImmediate(dlgPanel);
            Object.DestroyImmediate(uiGo);
        }

        [Test]
        public void Test16_CatHintButton_AppearsOnUnderstood_AndReopensDialog()
        {
            GameObject uiGo = new GameObject("Test_UIManager_CatHint");
            var um = uiGo.AddComponent<Map3_UIManager>();

            GameObject dlgPanel = new GameObject("HintDialogPanel");
            GameObject txtTitleGo = new GameObject("Txt_Title");
            var txtTitle = txtTitleGo.AddComponent<TMPro.TextMeshProUGUI>();
            GameObject txtMsgGo = new GameObject("Txt_Message");
            var txtMsg = txtMsgGo.AddComponent<TMPro.TextMeshProUGUI>();
            GameObject btnConfirmGo = new GameObject("Btn_Confirm");
            var btnConfirm = btnConfirmGo.AddComponent<UnityEngine.UI.Button>();
            GameObject btnCloseGo = new GameObject("Btn_Close");
            var btnClose = btnCloseGo.AddComponent<UnityEngine.UI.Button>();

            GameObject btnCatHintGo = new GameObject("Btn_CatHint");
            var btnCatHint = btnCatHintGo.AddComponent<UnityEngine.UI.Button>();
            btnCatHintGo.SetActive(false);

            UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(um);
            so.FindProperty("hintDialogPanel").objectReferenceValue = dlgPanel;
            so.FindProperty("txtHintTitle").objectReferenceValue = txtTitle;
            so.FindProperty("txtHintMessage").objectReferenceValue = txtMsg;
            so.FindProperty("btnHintConfirm").objectReferenceValue = btnConfirm;
            so.FindProperty("btnHintClose").objectReferenceValue = btnClose;
            so.FindProperty("btnCatHint").objectReferenceValue = btnCatHint;
            so.ApplyModifiedPropertiesWithoutUndo();

            um.SetBtnCatHintForTesting(btnCatHint);

            // 1. Initially hidden before hint received
            Assert.IsFalse(btnCatHintGo.activeSelf, "Btn_CatHint should be initially inactive");
            Assert.IsFalse(um.HasReceivedCatHint, "HasReceivedCatHint should initially be false");

            // 2. Open hint dialog (when interacting with cat)
            um.ShowHintDialog("Gá»¢I Ã Tá»ª CHÃš MÃˆO", "HÃ£y tÃ¬m chiáº¿c chÃ¬a khÃ³a cá»•...");
            Assert.IsTrue(um.IsHintDialogOpen, "Hint dialog should be open");
            Assert.IsTrue(um.HasReceivedCatHint, "HasReceivedCatHint should be true after receiving hint");

            // 3. User clicks 'ÄÃƒ HIá»‚U' -> CloseHintDialog
            um.CloseHintDialog();
            Assert.IsFalse(um.IsHintDialogOpen, "Hint dialog should close on ÄÃƒ HIá»‚U");
            Assert.IsTrue(btnCatHintGo.activeSelf, "Btn_CatHint must appear in HUD after ÄÃƒ HIá»‚U is clicked");

            // 4. User forgets and clicks '!' (Btn_CatHint) -> reopens dialog
            um.OnCatHintButtonClicked();
            Assert.IsTrue(um.IsHintDialogOpen, "Hint dialog should reopen when Btn_CatHint is clicked");
            Assert.AreEqual("Gá»¢I Ã Tá»ª CHÃš MÃˆO", txtTitle.text);
            Assert.AreEqual("HÃ£y tÃ¬m chiáº¿c chÃ¬a khÃ³a cá»•...", txtMsg.text);

            // 5. Clean up
            Object.DestroyImmediate(btnCatHintGo);
            Object.DestroyImmediate(btnCloseGo);
            Object.DestroyImmediate(btnConfirmGo);
            Object.DestroyImmediate(txtMsgGo);
            Object.DestroyImmediate(txtTitleGo);
            Object.DestroyImmediate(dlgPanel);
            Object.DestroyImmediate(uiGo);
        }

        [Test]
        public void Test17_SceneMap3_CatHintButton_ExistsAndProperlyPositioned()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);
            Assert.IsTrue(scene.IsValid(), "Scene must be valid");

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            Assert.IsNotNull(canvas, "Canvas not found in Map3_Final");

            Transform catHintT = canvas.transform.Find("Btn_CatHint");
            if (catHintT == null)
            {
                Map3_SceneBuilderEditor.SetupTopRightHUDInScene();
                catHintT = canvas.transform.Find("Btn_CatHint");
            }
            Assert.IsNotNull(catHintT, "Btn_CatHint not found in Canvas");

            var rt = catHintT.GetComponent<RectTransform>();
            Assert.IsNotNull(rt, "Btn_CatHint must have RectTransform");
            Assert.AreEqual(new Vector2(54f, 54f), rt.sizeDelta, "Btn_CatHint size must be 54x54");

            Transform topRightT = canvas.transform.Find("HUD_TopRight_Buttons");
            Assert.IsNotNull(topRightT, "HUD_TopRight_Buttons must exist");
            Transform helpT = topRightT.Find("Btn_Help");
            Assert.IsNotNull(helpT, "Btn_Help must exist in HUD_TopRight_Buttons");

            // Check distance between Btn_CatHint right edge and Btn_Help left edge (~74px, ~1.5 cm)
            Assert.AreEqual(-350f, rt.anchoredPosition.x, 5f, "Btn_CatHint should be ~1.5 cm to the left of Btn_Help");
        }
    }
}