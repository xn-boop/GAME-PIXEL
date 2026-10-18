using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using JourneyOfMemories.Core.Data;
using JourneyOfMemories.Core.Managers;

namespace JourneyOfMemories.Tests
{
    [TestFixture]
    public class LoginResetSaveFixTests
    {
        private GameObject testRoot;
        private SaveLoadManager saveLoadManager;
        private GameStateManager gameStateManager;
        private InventoryManager inventoryManager;
        private QuizManager quizManager;
        private PlacementManager placementManager;

        private LoginMenuController loginController;
        private InputField loginEmailInput;
        private InputField loginPassInput;
        private Toggle loginRememberToggle;
        private Button loginBtn;
        private Text loginStatusText;

        private RegisterMenuController registerController;
        private InputField regUserInput;
        private InputField regEmailInput;
        private InputField regPassInput;
        private Toggle regTermsToggle;
        private Button regBtn;
        private Text regStatusText;

        private MainMenuController mainMenuController;
        private Text mainMenuStatusText;

        private string saveFilePath;
        private string testSaveDirectory;

        [SetUp]
        public void SetUp()
        {
            // Mỗi test dùng file save ngẫu nhiên trong Unity temporary cache, tuyệt đối không chạm savegame.json thật.
            testSaveDirectory = Path.Combine(
                Application.temporaryCachePath,
                "JourneyOfMemoriesTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(testSaveDirectory);
            SaveLoadManager.TestSaveFilePathOverride = Path.Combine(testSaveDirectory, "savegame.json");
            SaveLoadManager.DisableDiskPersistenceForTests = true;

            testRoot = new GameObject("[LoginResetSaveFixTestsRoot]");

            // Canvas là dependency của dialog Chơi mới được tạo động.
            GameObject canvasObj = new GameObject("TestCanvas", typeof(RectTransform), typeof(Canvas));
            canvasObj.transform.SetParent(testRoot.transform);

            // 1. Core Managers
            gameStateManager = testRoot.AddComponent<GameStateManager>();
            inventoryManager = testRoot.AddComponent<InventoryManager>();
            quizManager = testRoot.AddComponent<QuizManager>();
            placementManager = testRoot.AddComponent<PlacementManager>();
            saveLoadManager = testRoot.AddComponent<SaveLoadManager>();

            // EditMode không bảo đảm Unity gọi Awake(). Khởi tạo singleton đúng thứ tự
            // để SaveLoadManager.LoadGame() nạp dữ liệu vào các manager thật của test.
            InvokeAwake(gameStateManager);
            InvokeAwake(inventoryManager);
            InvokeAwake(quizManager);
            InvokeAwake(placementManager);
            InvokeAwake(saveLoadManager);

            saveFilePath = saveLoadManager.GetSaveFilePath();
            SaveLoadManager.DisableDiskPersistenceForTests = false;

            // 2. LoginMenu Controller setup
            GameObject loginObj = new GameObject("LoginController", typeof(LoginMenuController));
            loginObj.transform.SetParent(testRoot.transform);
            loginController = loginObj.GetComponent<LoginMenuController>();

            loginEmailInput = CreateInputField("LoginEmail", loginObj.transform);
            loginPassInput = CreateInputField("LoginPass", loginObj.transform);
            loginRememberToggle = CreateToggle("RememberToggle", loginObj.transform);
            loginBtn = CreateButton("LoginBtn", loginObj.transform);
            loginStatusText = CreateText("LoginStatusText", loginObj.transform);

            loginController.emailInput = loginEmailInput;
            loginController.passwordInput = loginPassInput;
            loginController.rememberToggle = loginRememberToggle;
            loginController.loginButton = loginBtn;
            loginController.statusText = loginStatusText;
            loginController.fadeDuration = 0.01f;
            // Unit test chỉ kiểm tra state/save; không được load scene thật và phá test fixture.
            loginController.gameSceneName = "__TestSceneMissing__";

            // 3. RegisterMenu Controller setup
            GameObject regObj = new GameObject("RegisterController", typeof(RegisterMenuController));
            regObj.transform.SetParent(testRoot.transform);
            registerController = regObj.GetComponent<RegisterMenuController>();

            regUserInput = CreateInputField("RegUser", regObj.transform);
            regEmailInput = CreateInputField("RegEmail", regObj.transform);
            regPassInput = CreateInputField("RegPass", regObj.transform);
            regTermsToggle = CreateToggle("RegTerms", regObj.transform);
            regTermsToggle.isOn = true;
            regBtn = CreateButton("RegBtn", regObj.transform);
            regStatusText = CreateText("RegStatusText", regObj.transform);

            registerController.usernameInput = regUserInput;
            registerController.emailInput = regEmailInput;
            registerController.passwordInput = regPassInput;
            registerController.termsToggle = regTermsToggle;
            registerController.registerButton = regBtn;
            registerController.statusText = regStatusText;
            registerController.fadeDuration = 0.01f;
            registerController.gameSceneName = "__TestSceneMissing__";

            // 4. MainMenu Controller setup
            GameObject mmObj = new GameObject("MainMenuController", typeof(MainMenuController));
            mmObj.transform.SetParent(testRoot.transform);
            mainMenuController = mmObj.GetComponent<MainMenuController>();
            mainMenuStatusText = CreateText("MainMenuStatusText", mmObj.transform);
            mainMenuController.statusText = mainMenuStatusText;
            mainMenuController.fadeDuration = 0.01f;
            mainMenuController.firstLevelSceneName = "__TestSceneMissing__";
        }

        private static void InvokeAwake(MonoBehaviour behaviour)
        {
            MethodInfo awake = behaviour.GetType().GetMethod(
                "Awake",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(awake, $"{behaviour.GetType().Name} must define Awake().");
            awake.Invoke(behaviour, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null)
            {
                Object.DestroyImmediate(testRoot);
            }

            // File test nằm trong temporary cache và được hệ điều hành dọn; không xóa file nào trong TearDown.
            SaveLoadManager.DisableDiskPersistenceForTests = false;
            SaveLoadManager.TestSaveFilePathOverride = null;
        }

        private InputField CreateInputField(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(InputField));
            obj.transform.SetParent(parent);
            var inf = obj.GetComponent<InputField>();
            var txtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtObj.transform.SetParent(obj.transform);
            inf.textComponent = txtObj.GetComponent<Text>();
            return inf;
        }

        private Toggle CreateToggle(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            obj.transform.SetParent(parent);
            return obj.GetComponent<Toggle>();
        }

        private Button CreateButton(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Button));
            obj.transform.SetParent(parent);
            return obj.GetComponent<Button>();
        }

        private Text CreateText(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent);
            return obj.GetComponent<Text>();
        }

        private void StepCoroutine(IEnumerator routine, int maxSteps = 50)
        {
            int steps = 0;
            while (routine != null && routine.MoveNext() && steps < maxSteps)
            {
                steps++;
            }
        }

        private void CreateCustomSaveFile(MapId mapId, string customItem, string solvedQuiz)
        {
            SaveData data = new SaveData();
            data.SaveVersion = 2;
            data.CurrentMap = mapId.ToString();
            data.CurrentMapState = MapState.Active.ToString();
            if (!string.IsNullOrEmpty(customItem)) data.InventoryItems.Add(customItem);
            if (!string.IsNullOrEmpty(solvedQuiz)) data.SolvedQuizzes.Add(solvedQuiz);

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
        }

        #region AC-01: Đăng nhập có save hợp lệ không bị reset save

        [Test]
        public void BR01_AC01_Login_WhenSaveExists_DoesNotCallStartNewGame_AndLoadsSavedState()
        {
            // 1. Chuẩn bị save có tiến trình
            CreateCustomSaveFile(MapId.Map1_Kitchen, "Food_ThitKhoTrung", "Q01");
            Assert.IsTrue(saveLoadManager.HasSaveData(), "File save phải tồn tại.");

            loginEmailInput.text = "testuser@gmail.com";
            loginPassInput.text = "123456";

            // 2. Thực hiện đăng nhập
            var routine = loginController.SimulateLoginForTest("testuser@gmail.com", "123456");
            StepCoroutine(routine);

            // 3. Kiểm tra: Save file vẫn tồn tại, dữ liệu được tải vào inventory
            Assert.IsTrue(File.Exists(saveFilePath), "File save không được bị xóa sau khi đăng nhập.");
            Assert.IsTrue(inventoryManager.HasItem("Food_ThitKhoTrung"), "Vật phẩm lưu trong save phải được khôi phục vào túi đồ.");
            Assert.IsTrue(quizManager.IsQuizSolved("Q01"), "Tiến trình câu đố đã giải phải được khôi phục.");
            Assert.AreEqual(SaveLoadStatus.Success, saveLoadManager.LastLoadStatus, "SaveLoadStatus phải là Success.");
        }

        #endregion

        #region AC-02: Đăng nhập tại Map khác Map 1 không bị ép về Map 1

        [Test]
        public void BR01_AC02_Login_WhenSaveOnMap2_PreservesMap2Destination()
        {
            // Save ở Map 2
            CreateCustomSaveFile(MapId.Map2_DressingRoom, "Hanbok_Jeogori", "Q04");

            var routine = loginController.SimulateLoginForTest("player@gmail.com", "123456");
            StepCoroutine(routine);

            Assert.AreEqual(MapId.Map2_DressingRoom, gameStateManager.CurrentMap, "Đích đến phải giữ nguyên là Map 2, không được ép về Map 1.");
            Assert.IsTrue(inventoryManager.HasItem("Hanbok_Jeogori"), "Vật phẩm của Map 2 phải được khôi phục.");
        }

        #endregion

        #region AC-03: Thiết bị chưa có save, đăng nhập lần đầu tạo lượt mới tại Map 1

        [Test]
        public void BR02_AC03_Login_WhenNoSaveExists_StartsNewGameAtMap1()
        {
            Assert.IsFalse(saveLoadManager.HasSaveData(), "Ban đầu chưa có file save.");

            var routine = loginController.SimulateLoginForTest("newuser@gmail.com", "123456");
            StepCoroutine(routine);

            Assert.IsTrue(saveLoadManager.HasSaveData(), "Sau khi đăng nhập lần đầu, phải tạo save mới.");
            Assert.AreEqual(MapId.Map1_Kitchen, gameStateManager.CurrentMap, "Lượt chơi mới phải bắt đầu ở Map 1.");
        }

        #endregion

        #region AC-04: Nhập sai định dạng không đổi file save hoặc chuyển cảnh

        [Test]
        public void BR01_AC04_Login_WhenInvalidCredentials_DoesNotModifySaveOrTransition()
        {
            CreateCustomSaveFile(MapId.Map1_Kitchen, "Food_ThitKhoTrung", "Q01");
            string originalJson = File.ReadAllText(saveFilePath);

            loginEmailInput.text = "invalid_email_no_domain";
            loginPassInput.text = "123"; // Quá ngắn

            loginController.OnLoginClicked();

            Assert.IsFalse(loginController.IsTransitioning, "Không được bắt đầu chuyển cảnh.");
            Assert.AreEqual(originalJson, File.ReadAllText(saveFilePath), "File save phải giữ nguyên 100%.");
            Assert.IsTrue(loginStatusText.text.Length > 0, "Phải hiển thị lỗi trên UI.");
        }

        #endregion

        #region AC-05: Đăng ký mô phỏng khi đã có save không reset save chung

        [Test]
        public void BR04_AC05_Register_WhenSaveExists_PreservesExistingProgress()
        {
            CreateCustomSaveFile(MapId.Map1_Kitchen, "Food_Kimchi", "Q02");

            var routine = registerController.SimulateRegistrationForTest("PlayerX", "playerx@gmail.com");
            StepCoroutine(routine);

            Assert.IsTrue(File.Exists(saveFilePath), "File save không được bị xóa sau khi đăng ký.");
            Assert.IsTrue(inventoryManager.HasItem("Food_Kimchi"), "Vật phẩm hiện có phải được bảo toàn.");
            Assert.IsTrue(quizManager.IsQuizSolved("Q02"), "Quiz đã giải phải được bảo toàn.");
        }

        #endregion

        #region AC-06: Chọn Chơi mới rồi Hủy thì giữ nguyên tiến trình

        [Test]
        public void BR06_AC06_MainMenu_NewGame_WhenCancelled_PreservesSave()
        {
            CreateCustomSaveFile(MapId.Map1_Kitchen, "Food_ComCuon", "Q03");
            string originalJson = File.ReadAllText(saveFilePath);

            // Bấm Chơi mới
            mainMenuController.OnNewGameClicked();

            // Dialog xác nhận phải được hiển thị
            Assert.IsNotNull(mainMenuController.confirmNewGameDialog, "Phải có dialog xác nhận.");
            Assert.IsTrue(mainMenuController.confirmNewGameDialog.activeSelf, "Dialog xác nhận phải đang mở.");
            Assert.IsTrue(mainMenuController.confirmDialogMessageText.text.Contains("Chơi mới sẽ thay thế tiến trình hiện tại"),
                "Nội dung dialog phải cảnh báo ghi đè.");

            // Chọn HỦY
            mainMenuController.CancelStartNewGame();

            Assert.IsFalse(mainMenuController.confirmNewGameDialog.activeSelf, "Dialog phải đóng sau khi Hủy.");
            Assert.AreEqual(originalJson, File.ReadAllText(saveFilePath), "File save không được bị thay đổi sau khi Hủy.");
        }

        #endregion

        #region AC-07: Chọn Chơi mới và Xác nhận thì reset có chủ đích và tạo save mới

        [Test]
        public void BR06_AC07_MainMenu_NewGame_WhenConfirmed_ResetsAndStartsMap1()
        {
            CreateCustomSaveFile(MapId.Map1_Kitchen, "Food_ComCuon", "Q03");

            mainMenuController.ConfirmStartNewGame();

            Assert.AreEqual(MapId.Map1_Kitchen, gameStateManager.CurrentMap, "Game mới phải ở Map 1.");
            Assert.IsFalse(inventoryManager.HasItem("Food_ComCuon"), "Vật phẩm cũ phải bị xóa khi Chơi mới.");
            Assert.IsFalse(quizManager.IsQuizSolved("Q03"), "Quiz cũ phải được reset khi Chơi mới.");
        }

        #endregion

        #region AC-08: File save hỏng không bị xóa hoặc ghi đè, báo lỗi rõ ràng

        [Test]
        public void BR03_AC08_WhenSaveCorrupted_DoesNotDeleteOrOverwrite_ReportsError()
        {
            string corruptJson = "{ \"Corrupted_Data\": true, INVALID JSON... }";
            File.WriteAllText(saveFilePath, corruptJson);

            // 1. Thử qua LoginMenu
            var loginRoutine = loginController.SimulateLoginForTest("user@test.com", "123456");
            StepCoroutine(loginRoutine);

            Assert.IsTrue(File.Exists(saveFilePath), "File save hỏng không được bị xóa.");
            Assert.AreEqual(corruptJson, File.ReadAllText(saveFilePath), "File save hỏng phải giữ nguyên nội dung trên đĩa.");
            Assert.IsTrue(loginStatusText.text.Contains("lỗi") || loginStatusText.text.Contains("corrupted"),
                "UI phải báo lỗi không thể nạp save.");
            Assert.IsFalse(loginController.IsTransitioning, "Controller phải mở lại tương tác.");

            // 2. Thử qua MainMenu Continue
            mainMenuController.ContinueGame();
            Assert.IsTrue(File.Exists(saveFilePath), "ContinueGame không được xóa file save hỏng.");
            Assert.AreEqual(corruptJson, File.ReadAllText(saveFilePath), "Nội dung file save hỏng vẫn phải nguyên vẹn.");
            Assert.IsTrue(mainMenuStatusText.text.Contains("lỗi") || mainMenuStatusText.text.Contains("corrupted"),
                "MainMenu phải báo lỗi khi Tiếp tục với file save hỏng.");
        }

        #endregion

        #region AC-09: Scene chưa có trong Build Settings không reset save

        [Test]
        public void AC09_MissingSceneInBuildSettings_DoesNotResetSave()
        {
            // Map3_Garden chưa có trong Build Settings
            CreateCustomSaveFile(MapId.Map3_Garden, "Gem_Whole", "Q08");
            string originalJson = File.ReadAllText(saveFilePath);

            var routine = loginController.SimulateLoginForTest("user@test.com", "123456");
            StepCoroutine(routine);

            Assert.AreEqual(originalJson, File.ReadAllText(saveFilePath), "Save không được bị reset khi scene thiếu.");
            Assert.IsTrue(loginStatusText.text.Contains("Build Settings"), "Phải báo lỗi về Build Settings.");
            Assert.IsFalse(loginController.IsTransitioning, "Phải cho phép người chơi tương tác trở lại để quay về menu.");
        }

        #endregion

        #region AC-10: Nhấn nút liên tiếp khi đang chuyển cảnh chỉ xử lý một lần

        [Test]
        public void BR07_AC10_RepeatedClickDuringTransition_IsIgnored()
        {
            loginEmailInput.text = "player@test.com";
            loginPassInput.text = "123456";

            loginController.OnLoginClicked();
            Assert.IsTrue(loginController.IsTransitioning, "Lần nhấn đầu tiên bắt đầu chuyển cảnh.");

            // Nhấn lần 2 khi đang chuyển cảnh
            loginController.OnLoginClicked();
            Assert.IsTrue(loginController.IsTransitioning, "Trạng thái vẫn giữ nguyên, không khởi động coroutine thứ 2.");
        }

        #endregion
    }
}
