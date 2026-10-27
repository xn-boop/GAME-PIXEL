using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class L1_CoreTests
{
    [Test]
    public void TestQuizDatabase_All8QuestionsExistAndValid()
    {
        // 1. Cat Quiz
        Assert.IsNotNull(L1_QuizDatabase.CatQuiz);
        Assert.AreEqual(4, L1_QuizDatabase.CatQuiz.options.Length);
        Assert.AreEqual(1, L1_QuizDatabase.CatQuiz.correctIndex); // Answer B
        Assert.IsFalse(L1_QuizDatabase.CatQuiz.title.Contains("(CÂU"), "Quiz title should not contain question numbering");

        // 2. Main Food 1: Thit Kho
        Assert.IsNotNull(L1_QuizDatabase.ThitKhoQuiz);
        Assert.AreEqual(4, L1_QuizDatabase.ThitKhoQuiz.options.Length);
        Assert.AreEqual(1, L1_QuizDatabase.ThitKhoQuiz.correctIndex); // Answer B
        Assert.IsFalse(L1_QuizDatabase.ThitKhoQuiz.title.Contains("(CÂU"));
        Assert.AreSame(L1_QuizDatabase.ThitKhoQuiz, L1_QuizDatabase.GetQuizByItemType(L1_ItemType.ThitKho));

        // 3. Main Food 2: Kimchi
        Assert.IsNotNull(L1_QuizDatabase.KimchiQuiz);
        Assert.AreEqual(4, L1_QuizDatabase.KimchiQuiz.options.Length);
        Assert.AreEqual(2, L1_QuizDatabase.KimchiQuiz.correctIndex); // Answer C
        Assert.IsFalse(L1_QuizDatabase.KimchiQuiz.title.Contains("(CÂU"));
        Assert.AreSame(L1_QuizDatabase.KimchiQuiz, L1_QuizDatabase.GetQuizByItemType(L1_ItemType.Kimchi));

        // 4. Main Food 3: Kimbap
        Assert.IsNotNull(L1_QuizDatabase.KimbapQuiz);
        Assert.AreEqual(4, L1_QuizDatabase.KimbapQuiz.options.Length);
        Assert.AreEqual(1, L1_QuizDatabase.KimbapQuiz.correctIndex); // Answer B
        Assert.IsFalse(L1_QuizDatabase.KimbapQuiz.title.Contains("(CÂU"));
        Assert.AreSame(L1_QuizDatabase.KimbapQuiz, L1_QuizDatabase.GetQuizByItemType(L1_ItemType.Kimbap));

        // 5. Cultural Item 1: Flag VN
        Assert.IsNotNull(L1_QuizDatabase.FlagVnQuiz);
        Assert.AreEqual(4, L1_QuizDatabase.FlagVnQuiz.options.Length);
        Assert.AreEqual(1, L1_QuizDatabase.FlagVnQuiz.correctIndex); // Answer B
        Assert.IsFalse(L1_QuizDatabase.FlagVnQuiz.title.Contains("(CÂU"));

        // 6. Cultural Item 2: Flag KR
        Assert.IsNotNull(L1_QuizDatabase.FlagKrQuiz);
        Assert.AreEqual(4, L1_QuizDatabase.FlagKrQuiz.options.Length);
        Assert.AreEqual(1, L1_QuizDatabase.FlagKrQuiz.correctIndex); // Answer B
        Assert.IsFalse(L1_QuizDatabase.FlagKrQuiz.title.Contains("(CÂU"));

        // 7. Cultural Item 3: Banh Mi
        Assert.IsNotNull(L1_QuizDatabase.BanhMiQuiz);
        Assert.AreEqual(4, L1_QuizDatabase.BanhMiQuiz.options.Length);
        Assert.AreEqual(1, L1_QuizDatabase.BanhMiQuiz.correctIndex); // Answer B
        Assert.IsFalse(L1_QuizDatabase.BanhMiQuiz.title.Contains("(CÂU"));

        // 8. Cultural Item 4: Tteokbokki
        Assert.IsNotNull(L1_QuizDatabase.TteokbokkiQuiz);
        Assert.AreEqual(4, L1_QuizDatabase.TteokbokkiQuiz.options.Length);
        Assert.AreEqual(0, L1_QuizDatabase.TteokbokkiQuiz.correctIndex); // Answer A
        Assert.IsFalse(L1_QuizDatabase.TteokbokkiQuiz.title.Contains("(CÂU"));
    }

    [Test]
    public void TestInventoryManager_AddAndRemoveItems()
    {
        GameObject go = new GameObject("Test_Inventory_L1");
        var im = go.AddComponent<L1_InventoryManager>();

        Assert.IsFalse(im.HasItem(L1_ItemType.ThitKho));
        Assert.IsFalse(im.HasItem(L1_ItemType.Kimchi));
        Assert.IsFalse(im.HasItem(L1_ItemType.Kimbap));

        im.AddItem(L1_ItemType.ThitKho);
        im.AddItem(L1_ItemType.Kimchi);
        im.AddItem(L1_ItemType.Kimbap);

        Assert.IsTrue(im.HasItem(L1_ItemType.ThitKho));
        Assert.IsTrue(im.HasItem(L1_ItemType.Kimchi));
        Assert.IsTrue(im.HasItem(L1_ItemType.Kimbap));
        Assert.AreEqual(3, im.Items.Count);

        im.RemoveItem(L1_ItemType.ThitKho);
        Assert.IsFalse(im.HasItem(L1_ItemType.ThitKho));
        Assert.IsTrue(im.HasItem(L1_ItemType.Kimchi));
        Assert.AreEqual(2, im.Items.Count);

        UnityEngine.Object.DestroyImmediate(go);
    }

    [Test]
    public void TestDiningTable_PlacementProgress()
    {
        GameObject tableGo = new GameObject("Test_DiningTable");
        var tableCtrl = tableGo.AddComponent<L1_DiningTableController>();

        GameObject p1Go = new GameObject("Plate_1");
        p1Go.transform.SetParent(tableGo.transform);
        var p1 = p1Go.AddComponent<L1_Plate>();

        GameObject p2Go = new GameObject("Plate_2");
        p2Go.transform.SetParent(tableGo.transform);
        var p2 = p2Go.AddComponent<L1_Plate>();

        GameObject p3Go = new GameObject("Plate_3");
        p3Go.transform.SetParent(tableGo.transform);
        var p3 = p3Go.AddComponent<L1_Plate>();

        tableCtrl.SetPlates(p1, p2, p3);

        GameObject invGo = new GameObject("Test_Inv");
        var im = invGo.AddComponent<L1_InventoryManager>();

        // Initially 0
        Assert.AreEqual(0, tableCtrl.PlacedDishesCount);
        Assert.IsFalse(tableCtrl.IsTableComplete);

        // Add Thit Kho & interact
        im.AddItem(L1_ItemType.ThitKho);
        tableCtrl.Interact();
        Assert.AreEqual(1, tableCtrl.PlacedDishesCount);
        Assert.IsTrue(p1.IsPlaced);
        Assert.IsFalse(im.HasItem(L1_ItemType.ThitKho));

        // Add Kimchi & Kimbap & interact
        im.AddItem(L1_ItemType.Kimchi);
        im.AddItem(L1_ItemType.Kimbap);

        tableCtrl.Interact();
        Assert.AreEqual(2, tableCtrl.PlacedDishesCount);
        Assert.IsTrue(p2.IsPlaced);

        tableCtrl.Interact();
        Assert.AreEqual(3, tableCtrl.PlacedDishesCount);
        Assert.IsTrue(p3.IsPlaced);
        Assert.IsTrue(tableCtrl.IsTableComplete);

        UnityEngine.Object.DestroyImmediate(invGo);
        UnityEngine.Object.DestroyImmediate(tableGo);
    }

    [Test]
    public void TestGameManager_StateProgressionAndVictory()
    {
        GameObject gmGo = new GameObject("Test_GameManager");
        var gm = gmGo.AddComponent<L1_GameManager>();

        Assert.IsFalse(gm.CatQuizSolved);
        Assert.AreEqual(0, gm.CollectedCount);
        Assert.AreEqual(0, gm.PlacedCount);
        Assert.IsFalse(gm.IsVictory);

        // Cat solved
        gm.OnCatQuizSolved();
        Assert.IsTrue(gm.CatQuizSolved);

        // Food collected
        gm.OnItemCollected(L1_ItemType.ThitKho);
        Assert.AreEqual(1, gm.CollectedCount);
        gm.OnItemCollected(L1_ItemType.Kimchi);
        Assert.AreEqual(2, gm.CollectedCount);
        gm.OnItemCollected(L1_ItemType.Kimbap);
        Assert.AreEqual(3, gm.CollectedCount);

        // Food placed on table
        gm.OnFoodPlacedOnTable(L1_ItemType.ThitKho);
        Assert.AreEqual(1, gm.PlacedCount);
        Assert.IsFalse(gm.IsVictory);

        gm.OnFoodPlacedOnTable(L1_ItemType.Kimchi);
        Assert.AreEqual(2, gm.PlacedCount);
        Assert.IsFalse(gm.IsVictory);

        gm.OnFoodPlacedOnTable(L1_ItemType.Kimbap);
        Assert.AreEqual(3, gm.PlacedCount);
        // Under dual win condition, 3/3 dishes is NOT enough yet without Banh Mi and Tteokbokki
        Assert.IsFalse(gm.IsVictory, "Victory should not trigger if BanhMi and Tteokbokki are not solved yet");

        // Solve Banh Mi (1/2 explore)
        gm.OnCulturalItemSolved(L1_CulturalType.BanhMi);
        Assert.IsTrue(gm.IsBanhMiSolved);
        Assert.IsFalse(gm.IsVictory, "Victory should not trigger if only BanhMi is solved");

        // Flags do not affect win condition
        gm.OnCulturalItemSolved(L1_CulturalType.FlagVN);
        gm.OnCulturalItemSolved(L1_CulturalType.FlagKR);
        Assert.IsFalse(gm.IsVictory, "Flags are optional cultural items and should not satisfy explore condition");

        // Solve Tteokbokki (2/2 explore + 3/3 dishes => Victory!)
        gm.OnCulturalItemSolved(L1_CulturalType.Tteokbokki);
        Assert.IsTrue(gm.IsTteokbokkiSolved);
        Assert.IsTrue(gm.IsVictory, "Victory should trigger when both 3/3 dishes and 2/2 explore are complete!");

        UnityEngine.Object.DestroyImmediate(gmGo);
    }

    [Test]
    public void TestGameManager_DualWinCondition_ExploreFirstThenDishes()
    {
        GameObject gmGo = new GameObject("Test_GameManager_ExploreFirst");
        var gm = gmGo.AddComponent<L1_GameManager>();

        // 1. Solve Cat quiz
        gm.OnCatQuizSolved();

        // 2. Solve Banh Mi and Tteokbokki first
        gm.OnCulturalItemSolved(L1_CulturalType.BanhMi);
        Assert.IsTrue(gm.IsBanhMiSolved);
        Assert.IsFalse(gm.IsVictory);

        gm.OnCulturalItemSolved(L1_CulturalType.Tteokbokki);
        Assert.IsTrue(gm.IsTteokbokkiSolved);
        Assert.IsFalse(gm.IsVictory, "Victory should not trigger when explore is 2/2 but dishes are 0/3");

        // 3. Now collect and place the 3 main dishes
        gm.OnItemCollected(L1_ItemType.ThitKho);
        gm.OnItemCollected(L1_ItemType.Kimchi);
        gm.OnItemCollected(L1_ItemType.Kimbap);

        gm.OnFoodPlacedOnTable(L1_ItemType.ThitKho);
        Assert.AreEqual(1, gm.PlacedCount);
        Assert.IsFalse(gm.IsVictory);

        gm.OnFoodPlacedOnTable(L1_ItemType.Kimchi);
        Assert.AreEqual(2, gm.PlacedCount);
        Assert.IsFalse(gm.IsVictory);

        gm.OnFoodPlacedOnTable(L1_ItemType.Kimbap);
        Assert.AreEqual(3, gm.PlacedCount);
        // Now both 2/2 explore and 3/3 dishes are complete => VICTORY!
        Assert.IsTrue(gm.IsVictory, "Victory should trigger when the 3rd dish is placed after BanhMi and Tteokbokki are solved");

        UnityEngine.Object.DestroyImmediate(gmGo);
    }

    [Test]
    public void TestCulturalItem_LockedAfterSolved()
    {
        GameObject itemGo = new GameObject("Interact_BanhMi");
        var cultural = itemGo.AddComponent<L1_CulturalItem>();

        Assert.IsTrue(cultural.CanInteract);
        Assert.IsFalse(cultural.IsSolved);

        // Interact opens quiz
        cultural.Interact();

        // Simulate solve
        var method = typeof(L1_CulturalItem).GetMethod("OnQuizSolved", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(method);
        method.Invoke(cultural, null);

        Assert.IsTrue(cultural.IsSolved);
        Assert.IsFalse(cultural.CanInteract, "CanInteract must be false after being solved to prevent repeat interaction");

        UnityEngine.Object.DestroyImmediate(itemGo);
    }

    [Test]
    public void TestGameManager_ArrowIndicators_HiddenUntilCatQuizSolved_AndHiddenWhenCollected()
    {
        GameObject gmGo = new GameObject("Test_GameManager_Arrows");
        var gm = gmGo.AddComponent<L1_GameManager>();

        GameObject arrowThitKho = new GameObject("Arrow_ThitKho");
        GameObject arrowKimchi = new GameObject("Arrow_Kimchi");
        GameObject arrowKimbap = new GameObject("Arrow_Kimbap");

        // Initially active in hierarchy
        arrowThitKho.SetActive(true);
        arrowKimchi.SetActive(true);
        arrowKimbap.SetActive(true);

        gm.SetArrowReferences(arrowThitKho, arrowKimchi, arrowKimbap);

        // HideAllArrows should make them inactive
        gm.HideAllArrows();
        Assert.IsFalse(arrowThitKho.activeSelf, "arrowThitKho should be inactive initially");
        Assert.IsFalse(arrowKimchi.activeSelf, "arrowKimchi should be inactive initially");
        Assert.IsFalse(arrowKimbap.activeSelf, "arrowKimbap should be inactive initially");

        // Solving Cat Quiz should activate all 3 arrows
        gm.OnCatQuizSolved();
        Assert.IsTrue(arrowThitKho.activeSelf, "arrowThitKho should become active after cat quiz");
        Assert.IsTrue(arrowKimchi.activeSelf, "arrowKimchi should become active after cat quiz");
        Assert.IsTrue(arrowKimbap.activeSelf, "arrowKimbap should become active after cat quiz");

        // Collecting ThitKho should deactivate its arrow
        gm.OnItemCollected(L1_ItemType.ThitKho);
        Assert.IsFalse(arrowThitKho.activeSelf, "arrowThitKho should be inactive after collection");
        Assert.IsTrue(arrowKimchi.activeSelf, "arrowKimchi should remain active");
        Assert.IsTrue(arrowKimbap.activeSelf, "arrowKimbap should remain active");

        // Collecting Kimchi should deactivate its arrow
        gm.OnItemCollected(L1_ItemType.Kimchi);
        Assert.IsFalse(arrowKimchi.activeSelf, "arrowKimchi should be inactive after collection");
        Assert.IsTrue(arrowKimbap.activeSelf, "arrowKimbap should remain active");

        // Collecting Kimbap should deactivate its arrow
        gm.OnItemCollected(L1_ItemType.Kimbap);
        Assert.IsFalse(arrowKimbap.activeSelf, "arrowKimbap should be inactive after collection");

        UnityEngine.Object.DestroyImmediate(arrowThitKho);
        UnityEngine.Object.DestroyImmediate(arrowKimchi);
        UnityEngine.Object.DestroyImmediate(arrowKimbap);
        UnityEngine.Object.DestroyImmediate(gmGo);
    }
}
