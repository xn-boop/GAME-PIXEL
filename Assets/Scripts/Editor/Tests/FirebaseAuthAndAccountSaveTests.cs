using System.Reflection;
using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using JourneyOfMemories.Core.Data;
using JourneyOfMemories.Core.Managers;

namespace JourneyOfMemories.Tests
{
    [TestFixture]
    public class FirebaseAuthAndAccountSaveTests
    {
        private GameObject testRoot;
        private LocalAccountManager accountManager;
        private SaveLoadManager saveLoadManager;
        private GameStateManager gameStateManager;
        private InventoryManager inventoryManager;

        private string persistentDir;
        private string accountsFile;
        private string legacySaveFile;
        private string savesDir;

                private static void InvokeAwake(MonoBehaviour behaviour)
        {
            if (behaviour == null) return;
            MethodInfo awake = behaviour.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            awake?.Invoke(behaviour, null);
        }

        [SetUp]
        public void SetUp()
        {
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            testRoot = new GameObject("[FirebaseAuthAndAccountSaveTestsRoot]");

            accountManager = testRoot.AddComponent<LocalAccountManager>();
            gameStateManager = testRoot.AddComponent<GameStateManager>();
            inventoryManager = testRoot.AddComponent<InventoryManager>();
            saveLoadManager = testRoot.AddComponent<SaveLoadManager>();
            SaveLoadManager.DisableDiskPersistenceForTests = false;

            InvokeAwake(accountManager);
            InvokeAwake(gameStateManager);
            InvokeAwake(inventoryManager);
            InvokeAwake(saveLoadManager);

            persistentDir = Application.persistentDataPath;
            accountsFile = Path.Combine(persistentDir, "accounts.json");
            legacySaveFile = Path.Combine(persistentDir, "savegame.json");
            savesDir = Path.Combine(persistentDir, "saves");

            CleanTestFiles();
            SaveLoadManager.DisableDiskPersistenceForTests = false;

            PlayerPrefs.DeleteKey("LoggedInUser_Email");
            PlayerPrefs.DeleteKey("SavedLogin_Email");
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
                if (File.Exists(accountsFile)) File.Delete(accountsFile);
                if (File.Exists(legacySaveFile)) File.Delete(legacySaveFile);
                if (File.Exists(legacySaveFile + ".migrated")) File.Delete(legacySaveFile + ".migrated");
                if (File.Exists(legacySaveFile + ".owner")) File.Delete(legacySaveFile + ".owner");
                if (Directory.Exists(savesDir))
                {
                    Directory.Delete(savesDir, true);
                }
            }
            catch { }
        }

        [Test]
        public void Test01_TryLinkFirebaseUser_CreatesAccountWithUid_AndPreservesAccountId()
        {
            string uid = "firebase_uid_12345";
            string email = "testuser@example.com";
            string displayName = "TestUser";

            bool linked = accountManager.TryLinkFirebaseUser(
                uid,
                email,
                displayName,
                emailVerified: true,
                passwordForOfflineHash: "Password123!",
                out PlayerAccount account,
                out string errorMessage
            );

            Assert.IsTrue(linked, $"Liên kết Firebase thất bại: {errorMessage}");
            Assert.IsNotNull(account);
            Assert.AreEqual(uid, account.firebaseUid);
            Assert.AreEqual(email, account.email);
            Assert.IsNotEmpty(account.accountId);
            Assert.IsTrue(account.isEmailVerified);
            Assert.IsTrue(account.isVerifiedOnDevice);

            // Mật khẩu không lưu dạng plaintext
            Assert.AreNotEqual("Password123!", account.passwordHash);
            Assert.IsNotEmpty(account.passwordSalt);
        }

        [Test]
        public void Test02_TryLinkFirebaseUser_LinksExistingLocalAccount_WhenEmailMatches()
        {
            // 1. Tạo tài khoản local trước đó
            bool regOk = accountManager.Register("OldPlayer", "oldplayer@example.com", "Old Player", "Secret123!", out string regMsg);
            Assert.IsTrue(regOk);
            string originalAccountId = accountManager.CurrentAccount.accountId;

            // 2. Người dùng sau đó đăng nhập bằng Firebase với cùng email
            string fbUid = "fb_uid_99999";
            bool linkOk = accountManager.TryLinkFirebaseUser(
                fbUid,
                "oldplayer@example.com",
                "Old Player",
                emailVerified: true,
                passwordForOfflineHash: "Secret123!",
                out PlayerAccount updatedAcc,
                out string linkErr
            );

            Assert.IsTrue(linkOk, linkErr);
            Assert.AreEqual(originalAccountId, updatedAcc.accountId, "accountId cũ phải được bảo toàn 100%!");
            Assert.AreEqual(fbUid, updatedAcc.firebaseUid, "Firebase UID phải được liên kết vào hồ sơ cũ.");
        }

        [Test]
        public void Test03_TryLinkFirebaseUser_DetectsInconsistentData_WhenUidLinkedToDifferentEmail()
        {
            // Tài khoản 1
            accountManager.TryLinkFirebaseUser(
                "uid_unique_abc",
                "first@example.com",
                "First",
                true,
                "pass123",
                out _,
                out _
            );

            // Thử gán UID đó cho email khác
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("không nhất quán"));
            bool secondLink = accountManager.TryLinkFirebaseUser(
                "uid_unique_abc",
                "second@example.com",
                "Second",
                true,
                "pass123",
                out _,
                out string errMsg
            );

            Assert.IsFalse(secondLink, "Phải từ chối liên kết khi phát hiện dữ liệu không nhất quán.");
            StringAssert.Contains("không nhất quán", errMsg);
        }

        [Test]
        public void Test04_AccountBasedSave_IsolatesSaveFiles_BetweenTwoAccounts()
        {
            // Tài khoản A
            string accIdA = "account_user_A";
            saveLoadManager.SetCurrentAccount(accIdA);
            string savePathA = saveLoadManager.GetSaveFilePath();
            Assert.IsTrue(savePathA.Contains("saves"), "Save file phải nằm trong thư mục 'saves'.");

            inventoryManager.Clear();
            inventoryManager.AddItem("Food_ThitKhoTrung");
            saveLoadManager.SaveGame();
            Assert.IsTrue(File.Exists(savePathA), "Save file của tài khoản A phải được tạo.");

            // Tài khoản B
            string accIdB = "account_user_B";
            saveLoadManager.SetCurrentAccount(accIdB);
            string savePathB = saveLoadManager.GetSaveFilePath();
            Assert.AreNotEqual(savePathA, savePathB, "Mỗi tài khoản phải có file save hoàn toàn riêng biệt.");
            Assert.IsFalse(saveLoadManager.HasSaveData(), "Tài khoản B ban đầu chưa có save.");

            inventoryManager.Clear();
            inventoryManager.AddItem("Food_Kimchi");
            saveLoadManager.SaveGame();
            Assert.IsTrue(File.Exists(savePathB), "Save file của tài khoản B phải được tạo.");

            // Chuyển lại tài khoản A và tải lại
            saveLoadManager.SetCurrentAccount(accIdA);
            bool loadA = saveLoadManager.LoadGame();
            Assert.IsTrue(loadA);
            Assert.IsTrue(inventoryManager.HasItem("Food_ThitKhoTrung"), "Tài khoản A phải giữ đúng vật phẩm của mình.");
            Assert.IsFalse(inventoryManager.HasItem("Food_Kimchi"), "Tiến trình của tài khoản B không được lẫn vào tài khoản A!");
        }

        [Test]
        public void Test05_DeleteSave_OnlyDeletesCurrentAccountSave_DoesNotAffectOtherAccount()
        {
            // Tạo save cho A
            string accIdA = "account_A";
            saveLoadManager.SetCurrentAccount(accIdA);
            inventoryManager.AddItem("Food_ThitKhoTrung");
            saveLoadManager.SaveGame();
            string savePathA = saveLoadManager.GetSaveFilePath();

            // Tạo save cho B
            string accIdB = "account_B";
            saveLoadManager.SetCurrentAccount(accIdB);
            inventoryManager.AddItem("Food_Kimchi");
            saveLoadManager.SaveGame();
            string savePathB = saveLoadManager.GetSaveFilePath();

            Assert.IsTrue(File.Exists(savePathA));
            Assert.IsTrue(File.Exists(savePathB));

            // Bắt đầu game mới trên tài khoản A (xóa save của A)
            saveLoadManager.SetCurrentAccount(accIdA);
            saveLoadManager.DeleteSave();

            Assert.IsFalse(File.Exists(savePathA), "Save của tài khoản A phải bị xóa.");
            Assert.IsTrue(File.Exists(savePathB), "Save của tài khoản B PHẢI NGUYÊN VẸN, không được bị xóa.");
        }

        [Test]
        public void Test06_OfflineLogin_Succeeds_ForVerifiedOnDeviceAccount()
        {
            accountManager.TryLinkFirebaseUser(
                "uid_verified_1",
                "verified@game.com",
                "VerifiedPlayer",
                emailVerified: true,
                passwordForOfflineHash: "MySecurePass123",
                out PlayerAccount acc,
                out _
            );

            // Đăng nhập offline đúng mật khẩu
            bool loginOk = accountManager.TryOfflineLogin("verified@game.com", "MySecurePass123", out PlayerAccount logged, out string msg);
            Assert.IsTrue(loginOk, msg);
            Assert.IsNotNull(logged);
            Assert.AreEqual("verified@game.com", logged.email);

            // Đăng nhập offline sai mật khẩu
            bool wrongPass = accountManager.TryOfflineLogin("verified@game.com", "WrongPassword", out _, out string wrongMsg);
            Assert.IsFalse(wrongPass);
            StringAssert.Contains("không chính xác", wrongMsg);
        }

        [Test]
        public void Test07_OfflineLogin_Blocks_UnverifiedOrNewAccounts()
        {
            // Tài khoản chưa xác minh email
            accountManager.TryLinkFirebaseUser(
                "uid_unverified_2",
                "unverified@game.com",
                "UnverifiedPlayer",
                emailVerified: false,
                passwordForOfflineHash: "Pass123",
                out _,
                out _
            );

            bool loginUnverified = accountManager.TryOfflineLogin("unverified@game.com", "Pass123", out _, out string msg);
            Assert.IsFalse(loginUnverified, "Tài khoản chưa xác minh không được phép chơi offline.");
            StringAssert.Contains("chưa từng xác minh", msg);

            // Tài khoản chưa từng tồn tại trên thiết bị
            bool loginNonExistent = accountManager.TryOfflineLogin("unknown@game.com", "Pass123", out _, out string nonExistentMsg);
            Assert.IsFalse(loginNonExistent);
            StringAssert.Contains("Không tìm thấy", nonExistentMsg);
        }

        [Test]
        public void Test08_CorruptedSave_IsNotOverwrittenByEmptySave()
        {
            string accId = "acc_corrupt_test";
            saveLoadManager.SetCurrentAccount(accId);
            string savePath = saveLoadManager.GetSaveFilePath();

            string corruptContent = "{ INVALID_JSON_DATA_12345: true... ";
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            File.WriteAllText(savePath, corruptContent);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Lỗi khi nạp dữ liệu save|JSON parse error"));
            bool loadResult = saveLoadManager.LoadGame();
            Assert.IsFalse(loadResult, "Tải file hỏng phải trả về false.");
            Assert.AreEqual(SaveLoadStatus.CorruptedData, saveLoadManager.LastLoadStatus);

            // Nội dung trên đĩa phải giữ nguyên vẹn
            Assert.AreEqual(corruptContent, File.ReadAllText(savePath), "File save hỏng không được bị xóa hoặc thay bằng dữ liệu trống.");
        }

        [Test]
        public void Test09_LegacySaveMigration_CreatesBackup_AndMigratesToOwnerAccount()
        {
            // 1. Chuẩn bị file savegame.json cũ
            SaveData legacyData = new SaveData
            {
                SaveVersion = 2,
                CurrentMap = MapId.Map1_Kitchen.ToString(),
                CurrentMapState = MapState.Active.ToString()
            };
            legacyData.InventoryItems.Add("Food_ThitKhoTrung");
            File.WriteAllText(legacySaveFile, JsonUtility.ToJson(legacyData, true));

            // 2. Tạo tài khoản với email trùng khớp phiên đăng nhập
            string userEmail = "legacyowner@example.com";
            PlayerPrefs.SetString("LoggedInUser_Email", userEmail);

            accountManager.TryLinkFirebaseUser(
                "uid_legacy_owner",
                userEmail,
                "LegacyOwner",
                true,
                "pass123",
                out PlayerAccount acc,
                out _
            );

            // 3. Kích hoạt tài khoản -> SaveLoadManager kiểm tra và chuyển đổi
            saveLoadManager.SetCurrentAccount(acc.accountId);
            string accountSavePath = saveLoadManager.GetSaveFilePath();

            Assert.IsTrue(File.Exists(accountSavePath), "Save mới theo tài khoản phải được tạo.");
            Assert.IsTrue(File.Exists(legacySaveFile + ".migrated"), "Marker .migrated phải được tạo.");

            // Kiểm tra thư mục backup
            string backupDir = Path.Combine(persistentDir, "saves", "backups");
            Assert.IsTrue(Directory.Exists(backupDir), "Thư mục backup phải tồn tại.");
            var backups = Directory.GetFiles(backupDir, "savegame_legacy_backup_*.json");
            Assert.GreaterOrEqual(backups.Length, 1, "Phải có ít nhất một bản sao lưu an toàn.");

            // Kiểm tra nội dung file mới nạp được dữ liệu cũ
            bool loaded = saveLoadManager.LoadGame();
            Assert.IsTrue(loaded);
            Assert.IsTrue(inventoryManager.HasItem("Food_ThitKhoTrung"), "Vật phẩm từ save cũ phải được khôi phục nguyên vẹn.");
        }

[Test]
        public void Test10_Unauthenticated_Operations_AreRefused_AndDoNotModifyLegacySave()
        {
            // Chuẩn bị file savegame.json cũ với nội dung gốc
            string legacyContent = "{\"SaveVersion\":2,\"CurrentMap\":\"Map1_Kitchen\",\"CurrentMapState\":\"Active\",\"InventoryItems\":[\"Food_ThitKhoTrung\"]}";
            File.WriteAllText(legacySaveFile, legacyContent);

            // Đảm bảo không có tài khoản nào được kích hoạt
            saveLoadManager.SetCurrentAccount(null);
            Assert.IsNull(saveLoadManager.CurrentAccountId);

            // 1. HasSaveData khi chưa có tài khoản:
            // Trả về true nếu file legacy tồn tại (để giao diện/test nhận biết thiết bị có dữ liệu cũ cần migrate)
            Assert.IsTrue(saveLoadManager.HasLegacySaveData());

            // 2. LoadGame() khi chưa đăng nhập -> Từ chối và gán status NoAccount
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Chưa xác định tài khoản đăng nhập.*LoadGame"));
            bool loaded = saveLoadManager.LoadGame();
            Assert.IsFalse(loaded, "LoadGame() khi chưa đăng nhập phải trả về false.");
            Assert.AreEqual(SaveLoadStatus.NoAccount, saveLoadManager.LastLoadStatus);
            Assert.IsFalse(inventoryManager.HasItem("Food_ThitKhoTrung"), "Túi đồ không được tự ý nạp từ savegame.json cũ khi chưa xác định tài khoản.");

            // 3. SaveGame() khi chưa đăng nhập -> Từ chối và không ghi đè savegame.json cũ
            inventoryManager.AddItem("Food_Kimchi");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Chưa xác định tài khoản đăng nhập.*SaveGame"));
            saveLoadManager.SaveGame();
            Assert.AreEqual(legacyContent, File.ReadAllText(legacySaveFile), "File savegame.json cũ không được bị ghi đè khi chưa đăng nhập.");

            // 4. DeleteSave() khi chưa đăng nhập -> Từ chối và không xóa file cũ
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Chưa xác định tài khoản đăng nhập.*DeleteSave"));
            saveLoadManager.DeleteSave();
            Assert.IsTrue(File.Exists(legacySaveFile), "File savegame.json cũ không được bị xóa khi chưa đăng nhập.");

            // 5. StartNewGame() khi chưa đăng nhập -> Từ chối
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("Chưa xác định tài khoản đăng nhập.*StartNewGame"));
            saveLoadManager.StartNewGame();
            Assert.IsTrue(File.Exists(legacySaveFile), "StartNewGame() khi chưa đăng nhập không được xóa hoặc can thiệp save cũ.");
            Assert.AreEqual(legacyContent, File.ReadAllText(legacySaveFile), "Nội dung savegame.json cũ phải nguyên vẹn 100%.");
        }

        [Test]
        public void Test11_LegacySaveMigration_RefusesMigration_WhenOwnerUnidentifiable()
        {
            // 1. Tạo 2 tài khoản trên cùng thiết bị TRƯỚC KHI tạo file legacy save
            // Để đảm bảo thiết bị có nhiều hơn 1 tài khoản (AllAccounts.Count = 2)
            accountManager.TryLinkFirebaseUser("uid_user_1", "user1@example.com", "UserOne", true, "pass1", out PlayerAccount acc1, out _);
            accountManager.TryLinkFirebaseUser("uid_user_2", "user2@example.com", "UserTwo", true, "pass2", out PlayerAccount acc2, out _);

            Assert.AreEqual(2, accountManager.AllAccounts.Count);

            // 2. Chuẩn bị file savegame.json cũ và đảm bảo không có marker
            string legacyContent = "{\"SaveVersion\":2,\"CurrentMap\":\"Map1_Kitchen\",\"CurrentMapState\":\"Active\",\"InventoryItems\":[\"Hanbok_Jeogori\"]}";
            File.WriteAllText(legacySaveFile, legacyContent);
            if (File.Exists(legacySaveFile + ".migrated")) File.Delete(legacySaveFile + ".migrated");
            if (File.Exists(legacySaveFile + ".owner")) File.Delete(legacySaveFile + ".owner");

            // 3. Tài khoản 2 đăng nhập -> Có 2 tài khoản, không có marker .owner -> Không xác định được ai sở hữu savegame.json
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("chưa xác định được chủ sở hữu đủ tin cậy"));
            saveLoadManager.SetCurrentAccount(acc2.accountId);

            string acc2SavePath = saveLoadManager.GetSaveFilePath();

            // Kì vọng:
            // - File save riêng của acc2 CHƯA được tạo từ savegame.json
            // - Marker .migrated KHÔNG được tạo
            // - File savegame.json cũ giữ nguyên 100%
            Assert.IsFalse(File.Exists(acc2SavePath), "Không được tự ý gán save cũ cho acc2 khi chưa xác định chủ sở hữu.");
            Assert.IsFalse(File.Exists(legacySaveFile + ".migrated"), "Marker .migrated không được tạo khi chưa có bằng chứng sở hữu.");
            Assert.IsTrue(File.Exists(legacySaveFile), "File savegame.json cũ phải được giữ nguyên vẹn trên đĩa.");
            Assert.AreEqual(legacyContent, File.ReadAllText(legacySaveFile));
        }

        [Test]
        public void Test12_StartNewGame_OnlyResetsAndSavesForCurrentAccount_DoesNotTouchOtherAccounts()
        {
            string accIdA = "acc_player_alpha";
            string accIdB = "acc_player_beta";

            // Tài khoản A chơi và lưu tiến trình
            saveLoadManager.SetCurrentAccount(accIdA);
            inventoryManager.Clear();
            inventoryManager.AddItem("Food_ThitKhoTrung");
            saveLoadManager.SaveGame();
            string savePathA = saveLoadManager.GetSaveFilePath();
            Assert.IsTrue(File.Exists(savePathA));

            // Tài khoản B đăng nhập, chơi và lưu tiến trình
            saveLoadManager.SetCurrentAccount(accIdB);
            inventoryManager.Clear();
            inventoryManager.AddItem("Food_Kimchi");
            saveLoadManager.SaveGame();
            string savePathB = saveLoadManager.GetSaveFilePath();
            Assert.IsTrue(File.Exists(savePathB));

            // Tài khoản B gọi StartNewGame()
            saveLoadManager.StartNewGame();

            // Kiểm tra: Bị xóa và tạo save mới ở Map1, inventory trống
            Assert.IsTrue(File.Exists(savePathB), "Save của tài khoản B phải được tạo mới sau StartNewGame.");
            Assert.AreEqual(0, inventoryManager.Items.Count, "Inventory của tài khoản B phải trống.");

            // Chuyển lại tài khoản A: Save của A vẫn nguyên vẹn
            saveLoadManager.SetCurrentAccount(accIdA);
            bool loadA = saveLoadManager.LoadGame();
            Assert.IsTrue(loadA);
            Assert.IsTrue(inventoryManager.HasItem("Food_ThitKhoTrung"), "Tiến trình của tài khoản A không được bị ảnh hưởng bởi StartNewGame của tài khoản B.");
        }

        [Test]
        public void Test13_CorruptedSave_LoadGame_PreservesCorruptedFileOnDisk_WithoutEmptyOverwrite()
        {
            string accId = "acc_corrupt_verify";
            saveLoadManager.SetCurrentAccount(accId);
            string savePath = saveLoadManager.GetSaveFilePath();

            string corruptData = "{ \"corrupted_json_stream_999\": true, INVALID SYNTAX...";
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            File.WriteAllText(savePath, corruptData);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Lỗi khi nạp dữ liệu save|JSON parse error"));
            bool loadResult = saveLoadManager.LoadGame();
            Assert.IsFalse(loadResult, "LoadGame() file hỏng phải trả về false.");
            Assert.AreEqual(SaveLoadStatus.CorruptedData, saveLoadManager.LastLoadStatus);

            // Kiểm tra: Nội dung file hỏng trên đĩa không bị thay thế bằng file save trắng
            Assert.AreEqual(corruptData, File.ReadAllText(savePath), "Tuyệt đối không được ghi đè file rỗng hoặc xóa file khi load thất bại.");
        }
    }
}
