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
}
