using System.Reflection;
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
        private SceneFlowManager sceneFlowManager;

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

        private LocalAccountManager localAccountManager;
        private string saveFilePath;

                private static void InvokeAwake(MonoBehaviour behaviour)
        {
            if (behaviour == null) return;
            MethodInfo awake = behaviour.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            awake?.Invoke(behaviour, null);
        }

        [SetUp]
        public void SetUp()
        {
            CleanTestFiles();
            SaveLoadManager.DisableDiskPersistenceForTests = false;

            testRoot = new GameObject("[LoginResetSaveFixTestsRoot]");

            // 1. Core Managers
            gameStateManager = testRoot.AddComponent<GameStateManager>();
            inventoryManager = testRoot.AddComponent<InventoryManager>();
            quizManager = testRoot.AddComponent<QuizManager>();
            placementManager = testRoot.AddComponent<PlacementManager>();
            saveLoadManager = testRoot.AddComponent<SaveLoadManager>();
            sceneFlowManager = testRoot.AddComponent<SceneFlowManager>();
            localAccountManager = testRoot.AddComponent<LocalAccountManager>();

            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");

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

            // 4. MainMenu Controller setup
            GameObject mmObj = new GameObject("MainMenuController", typeof(MainMenuController));
            mmObj.transform.SetParent(testRoot.transform);
            mainMenuController = mmObj.GetComponent<MainMenuController>();
            mainMenuStatusText = CreateText("MainMenuStatusText", mmObj.transform);
            mainMenuController.statusText = mainMenuStatusText;
            mainMenuController.fadeDuration = 0.01f;
            mainMenuController.firstLevelSceneName = "Map1";

            GameObject dlg = new GameObject("ConfirmDialog");
            dlg.transform.SetParent(mmObj.transform);
            mainMenuController.confirmNewGameDialog = dlg;
            mainMenuController.confirmDialogMessageText = CreateText("ConfirmText", dlg.transform);
            mainMenuController.confirmDialogMessageText.text = "Chơi mới sẽ thay thế tiến trình hiện tại. Bạn có chắc chắn không?";
            dlg.SetActive(false);

                        SaveLoadManager.DisableDiskPersistenceForTests = false;

            InvokeAwake(gameStateManager);
            InvokeAwake(inventoryManager);
            InvokeAwake(quizManager);
            InvokeAwake(placementManager);
            InvokeAwake(saveLoadManager);
            InvokeAwake(sceneFlowManager);
            InvokeAwake(localAccountManager);
            InvokeAwake(loginController);
            InvokeAwake(registerController);
            InvokeAwake(mainMenuController);
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(testRoot);
            }
            CleanTestFiles();
            SaveLoadManager.DisableDiskPersistenceForTests = false;
            PlayerPrefs.DeleteKey("LoggedInUser_Email");
            PlayerPrefs.DeleteKey("SavedLogin_Email");
        }

        private void CleanTestFiles()
        {
            try
            {
                string persistentDir = Application.persistentDataPath;
                string accountsFile = Path.Combine(persistentDir, "accounts.json");
                string legacySaveFile = Path.Combine(persistentDir, "savegame.json");
                string savesDir = Path.Combine(persistentDir, "saves");

                if (File.Exists(accountsFile)) File.Delete(accountsFile);
                if (File.Exists(legacySaveFile)) File.Delete(legacySaveFile);
                if (File.Exists(legacySaveFile + ".migrated")) File.Delete(legacySaveFile + ".migrated");
                if (File.Exists(legacySaveFile + ".owner")) File.Delete(legacySaveFile + ".owner");
                if (Directory.Exists(savesDir)) Directory.Delete(savesDir, true);
            }
            catch { }
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
            string targetPath = saveLoadManager.GetSaveFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            File.WriteAllText(targetPath, json);
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
            saveLoadManager.SetCurrentAccount("acc_existing_player");
            CreateCustomSaveFile(MapId.Map1_Kitchen, "Food_Kimchi", "Q02");
            string currentSavePath = saveLoadManager.GetSaveFilePath();

            var routine = registerController.SimulateRegistrationForTest("PlayerX", "playerx@gmail.com");
            StepCoroutine(routine);

            Assert.IsTrue(File.Exists(currentSavePath), "File save không được bị xóa sau khi đăng ký.");
            saveLoadManager.SetCurrentAccount("acc_existing_player");
            bool loadResult = saveLoadManager.LoadGame();
            Assert.IsTrue(loadResult, $"LoadGame() phải thành công nhưng thất bại: Status={saveLoadManager.LastLoadStatus}, Error='{saveLoadManager.LastErrorMessage}'");
            Assert.IsTrue(inventoryManager.HasItem("Food_Kimchi"), 
                $"Items count: {inventoryManager.Items.Count}, File content: '{File.ReadAllText(currentSavePath)}', InvInstance match: {InventoryManager.Instance == inventoryManager}, CurrentAccount: '{saveLoadManager.CurrentAccountId}'");
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

            saveLoadManager.SetCurrentAccount("acc_test_player");
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
            localAccountManager.TryLinkFirebaseUser("uid_test_user", "user@test.com", "UserTest", true, "123456", out PlayerAccount acc, out _);
            saveLoadManager.SetCurrentAccount(acc.accountId);
            string targetSavePath = saveLoadManager.GetSaveFilePath();

            string corruptJson = "{ \"Corrupted_Data\": true, INVALID JSON... }";
            File.WriteAllText(targetSavePath, corruptJson);

            // 1. Thử qua LoginMenu
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("SaveLoadManager.*JSON parse error|Lỗi khi nạp dữ liệu save|JSON parse error"));
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("LoginMenu.*Tải file save thất bại"));
            var loginRoutine = loginController.SimulateLoginForTest("user@test.com", "123456");
            StepCoroutine(loginRoutine);

            Assert.IsTrue(File.Exists(targetSavePath), "File save hỏng không được bị xóa.");
            Assert.AreEqual(corruptJson, File.ReadAllText(targetSavePath), "File save hỏng phải giữ nguyên nội dung trên đĩa.");
            Assert.IsTrue(loginStatusText.text.Contains("lỗi") || loginStatusText.text.Contains("corrupted"),
                "UI phải báo lỗi không thể nạp save.");
            Assert.IsFalse(loginController.IsTransitioning, "Controller phải mở lại tương tác.");

            // 2. Thử qua MainMenu Continue
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("SaveLoadManager.*JSON parse error|Lỗi khi nạp dữ liệu save|JSON parse error"));
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("MainMenuController.*Không thể tải dữ liệu save"));
            mainMenuController.ContinueGame();
            Assert.IsTrue(File.Exists(targetSavePath), "ContinueGame không được xóa file save hỏng.");
            Assert.AreEqual(corruptJson, File.ReadAllText(targetSavePath), "Nội dung file save hỏng vẫn phải nguyên vẹn.");
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

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("SceneFlowManager.*Build Settings"));
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("LoginMenu.*Build Settings"));
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
