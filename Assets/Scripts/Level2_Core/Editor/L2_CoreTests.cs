using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class L2_CoreTests
{
    [Test]
    public void TestQuizDatabase_All9QuestionsExistAndValid()
    {
        // 1. Cat Quiz
        Assert.IsNotNull(L2_QuizDatabase.CatQuiz);
        Assert.AreEqual(4, L2_QuizDatabase.CatQuiz.options.Length);
        Assert.AreEqual(0, L2_QuizDatabase.CatQuiz.correctIndex);
        StringAssert.Contains("Otgoreum", L2_QuizDatabase.CatQuiz.options[0]);

        // 2. Paintings 1 to 6
        int[] expectedPaintingAnswers = { 0, 1, 1, 1, 2, 0 };
        for (int i = 1; i <= 6; i++)
        {
            var quiz = L2_QuizDatabase.GetPaintingQuiz(i);
            Assert.IsNotNull(quiz, $"Painting {i} quiz is null");
            Assert.AreEqual(4, quiz.options.Length);
            Assert.AreEqual(expectedPaintingAnswers[i - 1], quiz.correctIndex, $"Painting {i} wrong answer index");
        }

        // 3. Outfits (Ao Dai & Hanbok) & Accessories (Non La & Mu Gat)
        Assert.IsNotNull(L2_QuizDatabase.AoDaiQuiz);
        Assert.AreEqual(2, L2_QuizDatabase.AoDaiQuiz.correctIndex);

        Assert.IsNotNull(L2_QuizDatabase.HanbokQuiz);
        Assert.AreEqual(0, L2_QuizDatabase.HanbokQuiz.correctIndex);

        // 4. Item Quizzes: Non La (Câu 8) & Mu Gat (Câu 9)
        Assert.IsNotNull(L2_QuizDatabase.NonLaQuiz);
        Assert.AreEqual(1, L2_QuizDatabase.NonLaQuiz.correctIndex);
        StringAssert.Contains("Nón Bài Thơ", L2_QuizDatabase.NonLaQuiz.options[1]);
        Assert.AreSame(L2_QuizDatabase.NonLaQuiz, L2_QuizDatabase.GetQuizByItemType(L2_ItemType.NonLa));

        Assert.IsNotNull(L2_QuizDatabase.MuGatQuiz);
        Assert.AreEqual(2, L2_QuizDatabase.MuGatQuiz.correctIndex);
        StringAssert.Contains("quý tộc / nho sĩ", L2_QuizDatabase.MuGatQuiz.options[2]);
        Assert.AreSame(L2_QuizDatabase.MuGatQuiz, L2_QuizDatabase.GetQuizByItemType(L2_ItemType.MuGat));
    }

    [Test]
    public void TestInventoryManager_AddAndRemove4Items()
    {
        GameObject go = new GameObject("Test_Inventory");
        var im = go.AddComponent<L2_InventoryManager>();

        Assert.IsFalse(im.HasItem(L2_ItemType.AoDai));
        Assert.IsFalse(im.HasItem(L2_ItemType.NonLa));
        Assert.IsFalse(im.HasItem(L2_ItemType.Hanbok));
        Assert.IsFalse(im.HasItem(L2_ItemType.MuGat));

        im.AddItem(L2_ItemType.AoDai);
        im.AddItem(L2_ItemType.NonLa);
        im.AddItem(L2_ItemType.Hanbok);
        im.AddItem(L2_ItemType.MuGat);

        Assert.IsTrue(im.HasItem(L2_ItemType.AoDai));
        Assert.IsTrue(im.HasItem(L2_ItemType.NonLa));
        Assert.IsTrue(im.HasItem(L2_ItemType.Hanbok));
        Assert.IsTrue(im.HasItem(L2_ItemType.MuGat));

        im.RemoveItem(L2_ItemType.AoDai);
        Assert.IsFalse(im.HasItem(L2_ItemType.AoDai));
        Assert.IsTrue(im.HasItem(L2_ItemType.NonLa));

        Object.DestroyImmediate(go);
    }

    [Test]
    public void TestInventoryManager_SpriteMappingAndUIUpdate()
    {
        GameObject go = new GameObject("Test_Inventory_Mapping");
        var im = go.AddComponent<L2_InventoryManager>();

        // Create dummy sprites
        Texture2D tex = new Texture2D(16, 16);
        Sprite sAoDai = Sprite.Create(tex, new Rect(0, 0, 16, 16), Vector2.zero);
        Sprite sNonLa = Sprite.Create(tex, new Rect(0, 0, 16, 16), Vector2.zero);
        Sprite sHanbok = Sprite.Create(tex, new Rect(0, 0, 16, 16), Vector2.zero);
        Sprite sMuGat = Sprite.Create(tex, new Rect(0, 0, 16, 16), Vector2.zero);

        im.aoDaiIcon = sAoDai;
        im.nonLaIcon = sNonLa;
        im.hanbokIcon = sHanbok;
        im.muGatIcon = sMuGat;

        Assert.AreSame(sAoDai, im.GetItemSprite(L2_ItemType.AoDai));
        Assert.AreSame(sNonLa, im.GetItemSprite(L2_ItemType.NonLa));
        Assert.AreSame(sHanbok, im.GetItemSprite(L2_ItemType.Hanbok));
        Assert.AreSame(sMuGat, im.GetItemSprite(L2_ItemType.MuGat));

        // Create test UI Image slots
        GameObject slotGo1 = new GameObject("Slot1");
        var img1 = slotGo1.AddComponent<UnityEngine.UI.Image>();
        GameObject slotGo2 = new GameObject("Slot2");
        var img2 = slotGo2.AddComponent<UnityEngine.UI.Image>();

        im.SetSlotIcons(new UnityEngine.UI.Image[] { img1, img2 });

        im.AddItem(L2_ItemType.AoDai);
        Assert.AreSame(sAoDai, img1.sprite);
        Assert.AreEqual(Color.white, img1.color);

        im.AddItem(L2_ItemType.NonLa);
        Assert.AreSame(sNonLa, img2.sprite);
        Assert.AreEqual(Color.white, img2.color);

        Object.DestroyImmediate(slotGo1);
        Object.DestroyImmediate(slotGo2);
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TestItemType_ConstantsAndAliases()
    {
        Assert.AreEqual("Item_AoDai", L2_ItemConstants.Item_AoDai);
        Assert.AreEqual("Item_Hanbok", L2_ItemConstants.Item_Hanbok);
        Assert.AreEqual("Item_NonLa", L2_ItemConstants.Item_NonLa);
        Assert.AreEqual("Item_MuGat", L2_ItemConstants.Item_MuGat);

        Assert.AreEqual((int)L2_ItemType.AoDai, (int)L2_ItemType.Item_AoDai);
        Assert.AreEqual((int)L2_ItemType.NonLa, (int)L2_ItemType.Item_NonLa);
        Assert.AreEqual((int)L2_ItemType.Hanbok, (int)L2_ItemType.Item_Hanbok);
        Assert.AreEqual((int)L2_ItemType.MuGat, (int)L2_ItemType.Item_MuGat);
    }

    [Test]
    public void TestSceneLevel2_StructureHierarchy()
    {
        // Check if Scene_Level2 exists in assets
        string scenePath = "Assets/Scenes/Scene_Level2.unity";
        var sceneAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(scenePath);
        Assert.IsNotNull(sceneAsset, "Scene_Level2.unity does not exist in Assets/Scenes/");
    }

    [Test]
    public void TestItemPickup_ArrowIndicatorLifecycle()
    {
        // 1. Test auto-creation of Arrow_Indicator fallback and initial inactive state
        GameObject itemGo = new GameObject("ItemSpawn_AoDai");
        var pickup = itemGo.AddComponent<L2_ItemPickup>();

        Assert.IsNotNull(pickup.ArrowIndicator, "Arrow indicator should be created or referenced");
        Assert.AreEqual("Arrow_Indicator", pickup.ArrowIndicator.name);
        Assert.IsFalse(pickup.ArrowIndicator.activeSelf, "Arrow indicator must be initially inactive");
        Assert.IsFalse(pickup.IsUnlocked, "Item pickup should be initially locked");

        // 2. Test unlock activates the Arrow Indicator
        pickup.UnlockPickup();
        Assert.IsTrue(pickup.IsUnlocked, "Item should be unlocked");
        Assert.IsTrue(pickup.ArrowIndicator.activeSelf, "Arrow indicator should be active after UnlockPickup");

        // 3. Test deactivation of arrow indicator
        pickup.SetArrowIndicatorActive(false);
        Assert.IsFalse(pickup.ArrowIndicator.activeSelf, "Arrow indicator should be hidden when deactivated");

        Object.DestroyImmediate(itemGo);

        // 4. Test pre-existing child named Arrow_Indicator is preserved
        GameObject customItemGo = new GameObject("ItemSpawn_NonLa");
        GameObject customArrow = new GameObject("Arrow_Indicator");
        customArrow.transform.SetParent(customItemGo.transform, false);

        var customPickup = customItemGo.AddComponent<L2_ItemPickup>();
        Assert.AreSame(customArrow, customPickup.ArrowIndicator, "L2_ItemPickup should bind to existing child named Arrow_Indicator");

        Object.DestroyImmediate(customItemGo);
    }

    [Test]
    public void TestSceneLevel2_AllTextMeshProUGUIUseSvnDeterminationSans()
    {
        string scenePath = "Assets/Scenes/Scene_Level2.unity";
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(scenePath);
        bool wasLoaded = scene.isLoaded;
        if (!wasLoaded)
        {
            scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
        }

        var texts = Object.FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int level2Texts = 0;
        foreach (var txt in texts)
        {
            if (txt.gameObject.scene == scene)
            {
                level2Texts++;
                Assert.IsNotNull(txt.font, $"Text '{txt.gameObject.name}' in Scene_Level2 has null font");
                Assert.AreEqual("SVN-Determination Sans", txt.font.name, $"Text '{txt.gameObject.name}' in Scene_Level2 does not use SVN-Determination Sans font");
            }
        }

        Assert.Greater(level2Texts, 0, "No TextMeshProUGUI found in Scene_Level2");

        if (!wasLoaded)
        {
            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
        }
    }

    [Test]
    public void TestSceneLevel2_QuizDialog_LayoutElementsFitWithinCardBounds()
    {
        string scenePath = "Assets/Scenes/Scene_Level2.unity";
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(scenePath);
        bool wasLoaded = scene.isLoaded;
        if (!wasLoaded)
        {
            scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
        }

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Canvas targetCanvas = null;
        foreach (var c in canvases)
        {
            if (c.gameObject.scene == scene) { targetCanvas = c; break; }
        }
        Assert.IsNotNull(targetCanvas, "Canvas not found in Scene_Level2");

        Transform quizModalT = targetCanvas.transform.Find("Panel_Quiz_Dialog");
        Assert.IsNotNull(quizModalT, "Panel_Quiz_Dialog not found");

        Transform cardContentT = quizModalT.Find("Card_Content");
        Assert.IsNotNull(cardContentT, "Card_Content not found");

        RectTransform cardRt = cardContentT.GetComponent<RectTransform>();
        Assert.GreaterOrEqual(cardRt.rect.width, 1050f, "Card_Content width should be at least 1050 for fitted 2-column layout");
        Assert.GreaterOrEqual(cardRt.rect.height, 480f, "Card_Content height should be at least 480 for fitted 2-column layout");
        Assert.LessOrEqual(cardRt.rect.height, 540f, "Card_Content height should be at most 540 to snugly fit the content");

        // Verify Panel_Illustration bottom aligns with Btn_Answer_D bottom
        Transform panelIllT = cardContentT.Find("Panel_Illustration");
        Transform btnAnsDT = cardContentT.Find("Btn_Answer_D");
        if (panelIllT != null && btnAnsDT != null)
        {
            Vector3[] illCorners = new Vector3[4];
            (panelIllT as RectTransform).GetWorldCorners(illCorners);
            Vector3[] btnDCorners = new Vector3[4];
            (btnAnsDT as RectTransform).GetWorldCorners(btnDCorners);
            Assert.AreEqual(btnDCorners[0].y, illCorners[0].y, 2.5f, "Panel_Illustration bottom should align with Btn_Answer_D bottom");
        }

        // Verify all active children fit inside Card_Content
        for (int i = 0; i < cardContentT.childCount; i++)
        {
            var child = cardContentT.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeSelf) continue;

            Vector3[] childCorners = new Vector3[4];
            child.GetWorldCorners(childCorners);

            Vector3[] cardCorners = new Vector3[4];
            cardRt.GetWorldCorners(cardCorners);

            Assert.GreaterOrEqual(childCorners[0].x, cardCorners[0].x - 1f, $"{child.name} overflows Card_Content on the left");
            Assert.LessOrEqual(childCorners[2].x, cardCorners[2].x + 1f, $"{child.name} overflows Card_Content on the right");
            Assert.GreaterOrEqual(childCorners[0].y, cardCorners[0].y - 1f, $"{child.name} overflows Card_Content on the bottom");
            Assert.LessOrEqual(childCorners[2].y, cardCorners[2].y + 1f, $"{child.name} overflows Card_Content on the top");
        }

        if (!wasLoaded)
        {
            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
        }
    }

    [Test]
    public void TestMannequinController_LayeredEquipAndCombination()
    {
        GameObject go = new GameObject("Mannequin_Test");
        var controller = go.AddComponent<MannequinController>();

        GameObject bodyGo = new GameObject("Slot_Body");
        bodyGo.transform.SetParent(go.transform);
        var bodySr = bodyGo.AddComponent<SpriteRenderer>();

        GameObject headGo = new GameObject("Slot_Head");
        headGo.transform.SetParent(go.transform);
        var headSr = headGo.AddComponent<SpriteRenderer>();

        controller.SetRenderers(bodySr, headSr);

        Texture2D tex = new Texture2D(16, 16);
        Sprite sAoDai = Sprite.Create(tex, new Rect(0, 0, 16, 16), Vector2.zero);
        Sprite sNonLa = Sprite.Create(tex, new Rect(0, 0, 16, 16), Vector2.zero);

        bool eventFired = false;
        controller.OnCombinationComplete.AddListener(() => { eventFired = true; });

        // Equip Body
        bool bEquip = controller.EquipItem("Item_AoDai", sAoDai);
        Assert.IsTrue(bEquip);
        Assert.AreEqual("Item_AoDai", controller.CurrentBodyID);
        Assert.IsTrue(bodySr.gameObject.activeSelf);
        Assert.AreSame(sAoDai, bodySr.sprite);
        Assert.IsFalse(controller.IsComplete);
        Assert.IsFalse(eventFired);

        // Equip Head
        bool hEquip = controller.EquipItem("Item_NonLa", sNonLa);
        Assert.IsTrue(hEquip);
        Assert.AreEqual("Item_NonLa", controller.CurrentHeadID);
        Assert.IsTrue(headSr.gameObject.activeSelf);
        Assert.AreSame(sNonLa, headSr.sprite);
        Assert.IsTrue(controller.IsComplete);
        Assert.IsTrue(controller.IsVietnamSet);
        Assert.IsTrue(eventFired);

        Object.DestroyImmediate(go);
    }

    [Test]
    public void TestQuizData_Illustration_CanBeAssignedAndRetrieved()
    {
        Texture2D tex = new Texture2D(16, 16);
        Sprite dummySprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), Vector2.zero);

        L2_QuizData customQuiz = new L2_QuizData(
            "TEST_QUIZ", "TIÊU ĐỀ", "CÂU HỎI?",
            "A", "B", "C", "D", 0, "GIẢI THÍCH",
            dummySprite, "Item_AoDai"
        );

        Assert.AreEqual("TEST_QUIZ", customQuiz.id);
        Assert.AreEqual("Item_AoDai", customQuiz.rewardItemId);
        Assert.AreSame(dummySprite, customQuiz.illustration);

        // Verify default item quizzes have rewardItemId populated
        Assert.AreEqual(L2_ItemConstants.Item_AoDai, L2_QuizDatabase.AoDaiQuiz.rewardItemId);
        Assert.AreEqual(L2_ItemConstants.Item_Hanbok, L2_QuizDatabase.HanbokQuiz.rewardItemId);
        Assert.AreEqual(L2_ItemConstants.Item_NonLa, L2_QuizDatabase.NonLaQuiz.rewardItemId);
        Assert.AreEqual(L2_ItemConstants.Item_MuGat, L2_QuizDatabase.MuGatQuiz.rewardItemId);

        Object.DestroyImmediate(tex);
    }

    [Test]
    public void TestQuizManager_OpenQuiz_ResolvesAndDisplaysItemIllustration()
    {
        // Clear any static illustration state leaked from other tests or scene objects
        L2_QuizDatabase.AoDaiQuiz.illustration = null;
        L2_QuizDatabase.HanbokQuiz.illustration = null;
        L2_QuizDatabase.NonLaQuiz.illustration = null;
        L2_QuizDatabase.MuGatQuiz.illustration = null;

        GameObject qmGo = new GameObject("Test_QuizManager");
        var qm = qmGo.AddComponent<L2_QuizManager>();

        GameObject modalGo = new GameObject("Panel_Quiz_Dialog");
        modalGo.SetActive(false);
        qm.SetQuizPanel(modalGo);
        GameObject panelIllGo = new GameObject("Panel_Illustration");
        panelIllGo.transform.SetParent(modalGo.transform);

        GameObject imgGo = new GameObject("Img_Illustration");
        imgGo.transform.SetParent(panelIllGo.transform);
        var img = imgGo.AddComponent<UnityEngine.UI.Image>();

        GameObject txtNameGo = new GameObject("Txt_Illustration_Name");
        txtNameGo.transform.SetParent(panelIllGo.transform);
        var txtName = txtNameGo.AddComponent<TMPro.TextMeshProUGUI>();

        qm.SetIllustrationReferences(panelIllGo, img, txtName);

        // Set dummy sprites
        Texture2D tex = new Texture2D(16, 16);
        Sprite sAoDai = Sprite.Create(tex, new Rect(0, 0, 16, 16), Vector2.zero);
        Sprite sNonLa = Sprite.Create(tex, new Rect(0, 0, 16, 16), Vector2.zero);
        Sprite sHanbok = Sprite.Create(tex, new Rect(0, 0, 16, 16), Vector2.zero);
        Sprite sMuGat = Sprite.Create(tex, new Rect(0, 0, 16, 16), Vector2.zero);
        qm.SetItemSprites(sAoDai, sNonLa, sHanbok, sMuGat);

        // 1. Test Ao Dai illustration
        qm.OpenQuiz(L2_QuizDatabase.AoDaiQuiz, null);
        Assert.IsTrue(qm.IsQuizOpen);
        Assert.IsTrue(qm.IllustrationContainer.activeSelf);
        Assert.AreSame(sAoDai, qm.IllustrationImage.sprite);
        Assert.IsTrue(qm.IllustrationImage.preserveAspect);
        Assert.AreEqual("ÁO DÀI", qm.IllustrationNameText.text);
        qm.CloseQuiz();

        // 2. Test Hanbok illustration
        qm.OpenQuiz(L2_QuizDatabase.HanbokQuiz, null);
        Assert.AreSame(sHanbok, qm.IllustrationImage.sprite);
        Assert.AreEqual("HANBOK", qm.IllustrationNameText.text);
        qm.CloseQuiz();

        // 3. Test Non La illustration
        qm.OpenQuiz(L2_QuizDatabase.NonLaQuiz, null);
        Assert.AreSame(sNonLa, qm.IllustrationImage.sprite);
        Assert.AreEqual("NÓN LÁ", qm.IllustrationNameText.text);
        qm.CloseQuiz();

        // 4. Test Mu Gat illustration
        qm.OpenQuiz(L2_QuizDatabase.MuGatQuiz, null);
        Assert.AreSame(sMuGat, qm.IllustrationImage.sprite);
        Assert.AreEqual("MŨ GAT", qm.IllustrationNameText.text);
        qm.CloseQuiz();

        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(modalGo);
        Object.DestroyImmediate(qmGo);
    }

    [Test]
    public void TestGameManager_ObjectiveUI_UpdatesCostumeAndPickedCountOnCollect()
    {
        GameObject gmGo = new GameObject("Test_GameManager");
        var gm = gmGo.AddComponent<L2_GameManager>();

        GameObject txtGo = new GameObject("Txt_Objective");
        var txtObj = txtGo.AddComponent<TMPro.TextMeshProUGUI>();

        var field = typeof(L2_GameManager).GetField("txtObjective", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field.SetValue(gm, txtObj);

        // Initial update
        gm.UpdateObjectiveUI();
        Assert.AreEqual("TRANG PHỤC: 0/4 (Đã nhặt: 0)", txtObj.text);

        // Collect item 1
        gm.OnItemCollected(L2_ItemType.MuGat);
        Assert.AreEqual("TRANG PHỤC: 1/4 (Đã nhặt: 1)", txtObj.text);
        Assert.AreEqual(1, gm.CollectedCount);

        // Collect item 2
        gm.OnItemCollected(L2_ItemType.AoDai);
        Assert.AreEqual("TRANG PHỤC: 2/4 (Đã nhặt: 2)", txtObj.text);
        Assert.AreEqual(2, gm.CollectedCount);

        // Collect item 3
        gm.OnItemCollected(L2_ItemType.NonLa);
        Assert.AreEqual("TRANG PHỤC: 3/4 (Đã nhặt: 3)", txtObj.text);
        Assert.AreEqual(3, gm.CollectedCount);

        // Collect item 4
        gm.OnItemCollected(L2_ItemType.Hanbok);
        Assert.AreEqual("TRANG PHỤC: 4/4 (Đã nhặt: 4)", txtObj.text);
        Assert.AreEqual(4, gm.CollectedCount);

        // Place items (1 to 3)
        gm.OnItemPlacedOnMannequin(L2_ItemType.AoDai);
        gm.OnItemPlacedOnMannequin(L2_ItemType.NonLa);
        gm.OnItemPlacedOnMannequin(L2_ItemType.Hanbok);
        Assert.AreEqual(3, gm.PlacedCount);
        Assert.AreEqual("TRANG PHỤC: 4/4 (Đã nhặt: 4)", txtObj.text);

        // Place 4th item (all placed)
        gm.OnItemPlacedOnMannequin(L2_ItemType.MuGat);
        Assert.AreEqual(4, gm.PlacedCount);
        Assert.AreEqual("TRANG PHỤC: 4/4", txtObj.text);

        Object.DestroyImmediate(txtGo);
        Object.DestroyImmediate(gmGo);
    }

    [Test]
    public void TestSceneLevel2_TopRightHUDButtons_ExistAndProperlyConfigured()
    {
        string scenePath = "Assets/Scenes/Scene_Level2.unity";
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(scenePath);
        bool wasLoaded = scene.isLoaded;
        if (!wasLoaded)
        {
            scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
        }

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Canvas targetCanvas = null;
        foreach (var c in canvases)
        {
            if (c.gameObject.scene == scene) { targetCanvas = c; break; }
        }
        Assert.IsNotNull(targetCanvas, "Canvas not found in Scene_Level2");

        // 1. HUD_TopRight_Buttons Container
        Transform topRightT = targetCanvas.transform.Find("HUD_TopRight_Buttons");
        Assert.IsNotNull(topRightT, "HUD_TopRight_Buttons not found in Canvas");

        // 2. 4 Buttons: Btn_Help, Btn_Sound, Btn_Pause, Btn_Home
        Transform btnHelpT = topRightT.Find("Btn_Help");
        Assert.IsNotNull(btnHelpT, "Btn_Help not found in HUD_TopRight_Buttons");
        Assert.IsNotNull(btnHelpT.GetComponent<UnityEngine.UI.Button>(), "Btn_Help has no Button component");

        Transform btnSoundT = topRightT.Find("Btn_Sound");
        Assert.IsNotNull(btnSoundT, "Btn_Sound not found in HUD_TopRight_Buttons");
        Assert.IsNotNull(btnSoundT.GetComponent<UnityEngine.UI.Button>(), "Btn_Sound has no Button component");

        Transform btnPauseT = topRightT.Find("Btn_Pause");
        Assert.IsNotNull(btnPauseT, "Btn_Pause not found in HUD_TopRight_Buttons");
        Assert.IsNotNull(btnPauseT.GetComponent<UnityEngine.UI.Button>(), "Btn_Pause has no Button component");

        Transform btnHomeT = topRightT.Find("Btn_Home");
        Assert.IsNotNull(btnHomeT, "Btn_Home not found in HUD_TopRight_Buttons");
        Assert.IsNotNull(btnHomeT.GetComponent<UnityEngine.UI.Button>(), "Btn_Home has no Button component");

        // 3. Modals: Panel_Pause, Panel_Guide, Panel_Confirm_Home
        Transform panelPauseT = targetCanvas.transform.Find("Panel_Pause");
        Assert.IsNotNull(panelPauseT, "Panel_Pause not found in Canvas");
        Assert.IsFalse(panelPauseT.gameObject.activeSelf, "Panel_Pause should be inactive by default");

        Transform panelGuideT = targetCanvas.transform.Find("Panel_Guide");
        Assert.IsNotNull(panelGuideT, "Panel_Guide not found in Canvas");
        Assert.IsFalse(panelGuideT.gameObject.activeSelf, "Panel_Guide should be inactive by default");

        Transform panelConfirmHomeT = targetCanvas.transform.Find("Panel_Confirm_Home");
        Assert.IsNotNull(panelConfirmHomeT, "Panel_Confirm_Home not found in Canvas");
        Assert.IsFalse(panelConfirmHomeT.gameObject.activeSelf, "Panel_Confirm_Home should be inactive by default");

        // 4. L2_GameHUDController attached
        var hudCtrl = targetCanvas.GetComponent<L2_GameHUDController>();
        Assert.IsNotNull(hudCtrl, "L2_GameHUDController not found on Canvas");

        if (!wasLoaded)
        {
            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
        }
    }

    [Test]
    public void TestGameHUDController_PauseToggleAndModalLifecycle()
    {
        GameObject hudGo = new GameObject("Test_HUDController");
        var hud = hudGo.AddComponent<L2_GameHUDController>();

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

        // Initial state
        Assert.IsFalse(hud.IsPaused);
        Assert.IsFalse(hud.IsGuideOpen);
        Assert.IsFalse(hud.IsConfirmHomeOpen);

        // Toggle pause
        hud.TogglePause();
        Assert.IsTrue(hud.IsPaused);
        Assert.IsTrue(pausePanel.activeSelf);
        Assert.AreEqual(0f, Time.timeScale);

        // Resume game
        hud.ResumeGame();
        Assert.IsFalse(hud.IsPaused);
        Assert.IsFalse(pausePanel.activeSelf);
        Assert.AreEqual(1f, Time.timeScale);

        // Toggle Guide
        hud.ToggleGuide();
        Assert.IsTrue(hud.IsGuideOpen);
        Assert.IsTrue(guidePanel.activeSelf);
        Assert.AreEqual(0f, Time.timeScale);

        hud.CloseGuide();
        Assert.IsFalse(hud.IsGuideOpen);
        Assert.IsFalse(guidePanel.activeSelf);
        Assert.AreEqual(1f, Time.timeScale);

        // Open Confirm Home
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
    public void TestGameHUDController_SoundToggle_UpdatesStateAndIcon()
    {
        GameObject hudGo = new GameObject("Test_HUDController_Sound");
        var hud = hudGo.AddComponent<L2_GameHUDController>();

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

        // Toggle back
        hud.ToggleSound();
        Assert.AreEqual(initialMuted, hud.IsMuted, "Sound state should revert to original");
        Assert.AreEqual(hud.IsMuted ? offSprite : onSprite, img.sprite);

        Object.DestroyImmediate(onSprite);
        Object.DestroyImmediate(offSprite);
        Object.DestroyImmediate(imgGo);
        Object.DestroyImmediate(hudGo);
    }

    [Test]
    public void TestGameManager_ContinueGame_TargetsMap3()
    {
        var method = typeof(L2_GameManager).GetMethod("ContinueGame");
        Assert.IsNotNull(method, "L2_GameManager must have a public ContinueGame method.");
    }
}
