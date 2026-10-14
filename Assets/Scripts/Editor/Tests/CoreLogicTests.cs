using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using JourneyOfMemories.Core.Data;
using JourneyOfMemories.Core.Managers;

namespace JourneyOfMemories.Tests
{
    [TestFixture]
    public class CoreLogicTests
    {
        private GameObject testRoot;
        private InventoryManager inventoryManager;
        private QuizManager quizManager;
        private PlacementManager placementManager;
        private CombineManager combineManager;
        private GameStateManager gameStateManager;
        private SaveLoadManager saveLoadManager;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("[UnitTestRoot]");
            inventoryManager = testRoot.AddComponent<InventoryManager>();
            quizManager = testRoot.AddComponent<QuizManager>();
            placementManager = testRoot.AddComponent<PlacementManager>();
            combineManager = testRoot.AddComponent<CombineManager>();
            gameStateManager = testRoot.AddComponent<GameStateManager>();
            saveLoadManager = testRoot.AddComponent<SaveLoadManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null)
            {
                Object.DestroyImmediate(testRoot);
            }
        }

        #region InventoryManager Tests

        [Test]
        public void InventoryManager_AddItems_RespectsMaxCapacity5()
        {
            inventoryManager.Clear();

            // Thêm 5 món hợp lệ
            Assert.IsTrue(inventoryManager.AddItem("Item1"));
            Assert.IsTrue(inventoryManager.AddItem("Item2"));
            Assert.IsTrue(inventoryManager.AddItem("Item3"));
            Assert.IsTrue(inventoryManager.AddItem("Item4"));
            Assert.IsTrue(inventoryManager.AddItem("Item5"));
            Assert.AreEqual(5, inventoryManager.ItemCount);

            // Món thứ 6 phải bị từ chối
            Assert.IsFalse(inventoryManager.AddItem("Item6"));
            Assert.AreEqual(5, inventoryManager.ItemCount);
        }

        [Test]
        public void InventoryManager_RemoveItem_SuccessAndFailCases()
        {
            inventoryManager.Clear();
            inventoryManager.AddItem("Food_ThitKhoTrung");

            Assert.IsTrue(inventoryManager.HasItem("Food_ThitKhoTrung"));
            Assert.IsTrue(inventoryManager.RemoveItem("Food_ThitKhoTrung"));
            Assert.IsFalse(inventoryManager.HasItem("Food_ThitKhoTrung"));

            // Xóa món không tồn tại phải trả về false
            Assert.IsFalse(inventoryManager.RemoveItem("Food_NonExistent"));
        }

        #endregion

        #region QuizManager Tests

        [Test]
        public void QuizManager_SubmitCorrectAnswer_AwardsItemToInventory()
        {
            inventoryManager.Clear();

            // Q01 đáp án đúng là B (Thịt kho trứng)
            var (isCorrect, feedback) = quizManager.SubmitQuizAnswer("Q01", QuizOption.B);

            Assert.IsTrue(isCorrect);
            Assert.IsTrue(quizManager.IsQuizSolved("Q01"));
            Assert.IsTrue(inventoryManager.HasItem("Food_ThitKhoTrung"));
            Assert.IsFalse(string.IsNullOrEmpty(feedback));
        }

        [Test]
        public void QuizManager_SubmitWrongAnswer_DoesNotAwardItem()
        {
            inventoryManager.Clear();

            // Q01 chọn sai (A)
            var (isCorrect, feedback) = quizManager.SubmitQuizAnswer("Q01", QuizOption.A);

            Assert.IsFalse(isCorrect);
            Assert.IsFalse(quizManager.IsQuizSolved("Q01"));
            Assert.IsFalse(inventoryManager.HasItem("Food_ThitKhoTrung"));
            Assert.IsFalse(string.IsNullOrEmpty(feedback));
        }

        #endregion

        #region PlacementManager Tests

        [Test]
        public void PlacementManager_TryPlaceMatchingItem_ConsumesItemAndFillsSlot()
        {
            inventoryManager.Clear();
            inventoryManager.AddItem("Food_ThitKhoTrung");

            var result = placementManager.TryPlaceItem("Food_ThitKhoTrung", "Slot_ThitKhoTrung");

            Assert.AreEqual(PlacementResult.Success, result);
            Assert.IsTrue(placementManager.IsSlotFilled("Slot_ThitKhoTrung"));
            Assert.IsFalse(inventoryManager.HasItem("Food_ThitKhoTrung"), "Vật phẩm phải được tiêu thụ khỏi túi đồ");
        }

        [Test]
        public void PlacementManager_TryPlaceMismatchItem_PreservesItemInInventory()
        {
            inventoryManager.Clear();
            inventoryManager.AddItem("Food_Kimchi");

            // Đặt Kim chi vào slot Thịt kho trứng -> Sai
            var result = placementManager.TryPlaceItem("Food_Kimchi", "Slot_ThitKhoTrung");

            Assert.AreEqual(PlacementResult.ItemMismatch, result);
            Assert.IsFalse(placementManager.IsSlotFilled("Slot_ThitKhoTrung"));
            Assert.IsTrue(inventoryManager.HasItem("Food_Kimchi"), "Vật phẩm phải được giữ nguyên trong túi");
        }

        [Test]
        public void PlacementManager_TryPlaceOnAlreadyFilledSlot_Fails()
        {
            inventoryManager.Clear();
            inventoryManager.AddItem("Food_ThitKhoTrung");
            placementManager.TryPlaceItem("Food_ThitKhoTrung", "Slot_ThitKhoTrung");

            inventoryManager.AddItem("Food_ThitKhoTrung");
            var result = placementManager.TryPlaceItem("Food_ThitKhoTrung", "Slot_ThitKhoTrung");

            Assert.AreEqual(PlacementResult.SlotAlreadyFilled, result);
        }

        #endregion

        #region CombineManager Tests

        [Test]
        public void CombineManager_WhenBothHalvesPresentAndQuizzesSolved_CombinesIntoWholeGem()
        {
            inventoryManager.Clear();

            // Giải Q06 và Q07 để nhận 2 nửa ngọc
            quizManager.SubmitQuizAnswer("Q06", QuizOption.B);
            quizManager.SubmitQuizAnswer("Q07", QuizOption.C);

            Assert.IsTrue(inventoryManager.HasItem("Gem_LotusHalf"));
            Assert.IsTrue(inventoryManager.HasItem("Gem_MugunghwaHalf"));
            Assert.AreEqual(2, inventoryManager.ItemCount);

            var (success, resultGemId, message) = combineManager.TryCombineGems();

            Assert.IsTrue(success);
            Assert.AreEqual("Gem_Whole", resultGemId);
            Assert.IsTrue(inventoryManager.HasItem("Gem_Whole"));
            Assert.IsFalse(inventoryManager.HasItem("Gem_LotusHalf"));
            Assert.IsFalse(inventoryManager.HasItem("Gem_MugunghwaHalf"));
            Assert.AreEqual(1, inventoryManager.ItemCount, "Số ô chiếm giảm từ 2 xuống 1");
        }

        #endregion

        #region GameStateManager Tests

        [Test]
        public void GameStateManager_Map1_AllConditionsMet_TriggersMemoryPending()
        {
            inventoryManager.Clear();

            // 1. Hoàn thành mini-game mèo
            gameStateManager.SetMiniGameDone(1, true);

            // 2. Giải 3 câu đố
            quizManager.SubmitQuizAnswer("Q01", QuizOption.B);
            quizManager.SubmitQuizAnswer("Q02", QuizOption.C);
            quizManager.SubmitQuizAnswer("Q03", QuizOption.B);

            // 3. Đặt 3 món vào mâm cơm
            placementManager.TryPlaceItem("Food_ThitKhoTrung", "Slot_ThitKhoTrung");
            placementManager.TryPlaceItem("Food_Kimchi", "Slot_Kimchi");
            placementManager.TryPlaceItem("Food_ComCuon", "Slot_ComCuon");

            Assert.AreEqual(MapState.MemoryPending, gameStateManager.CurrentMapState, "Trạng thái phải tự động đổi sang MemoryPending");
        }

        [Test]
        public void GameStateManager_Transition_AllowedOnlyWhenCurrentMapCompleted()
        {
            // Ban đầu chưa xong Map 1 -> Từ chối chuyển Map 2
            Assert.IsFalse(gameStateManager.RequestTransition(MapId.Map2_DressingRoom));

            // Xác nhận nhật ký Map 1 -> Map 1 Completed
            gameStateManager.ConfirmJournal(MapId.Map1_Kitchen);
            Assert.AreEqual(MapState.Completed, gameStateManager.CurrentMapState);

            // Giờ chuyển sang Map 2 phải thành công
            Assert.IsTrue(gameStateManager.RequestTransition(MapId.Map2_DressingRoom));
            Assert.AreEqual(MapId.Map2_DressingRoom, gameStateManager.CurrentMap);
            Assert.AreEqual(MapState.Active, gameStateManager.CurrentMapState);
        }

        #endregion

        #region SaveLoadManager Tests

        [Test]
        public void SaveLoadManager_AntiDuplicationCheck_Works()
        {
            inventoryManager.Clear();
            inventoryManager.AddItem("Food_ThitKhoTrung");

            Assert.IsTrue(saveLoadManager.IsItemAlreadyCollectedOrPlaced("Food_ThitKhoTrung"));
            Assert.IsFalse(saveLoadManager.IsItemAlreadyCollectedOrPlaced("Food_Kimchi"));

            placementManager.TryPlaceItem("Food_ThitKhoTrung", "Slot_ThitKhoTrung");
            // Món đã đặt lên slot vẫn phải được nhận diện là đã thu thập/đã đặt
            Assert.IsTrue(saveLoadManager.IsItemAlreadyCollectedOrPlaced("Food_ThitKhoTrung"));
        }

        #endregion
    }
}
