using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using JourneyOfMemories.Core.Managers;

public class LocalAccountManager : MonoBehaviour
{
    public static LocalAccountManager Instance { get; private set; }

    public PlayerAccount CurrentAccount { get; private set; }
    public PlayerData CurrentPlayerData { get; private set; }

    [Serializable]
    public class AccountList
    {
        public List<PlayerAccount> accounts = new List<PlayerAccount>();
    }

    private string AccountFile =>
        Path.Combine(Application.persistentDataPath, "accounts.json");

    private AccountList accountList = new AccountList();

    public IReadOnlyList<PlayerAccount> AllAccounts => accountList.accounts.AsReadOnly();

    private void Awake()
        {
#if UNITY_EDITOR
            Instance = this;
#else
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
#endif
        if (Application.isPlaying) DontDestroyOnLoad(gameObject);
        LoadAccounts();
    }

    public void LoadAccounts()
    {
        try
        {
            if (!File.Exists(AccountFile))
                return;

            string json = File.ReadAllText(AccountFile);
            AccountList loaded = JsonUtility.FromJson<AccountList>(json);

            if (loaded != null && loaded.accounts != null)
                accountList = loaded;
        }
        catch (Exception e)
        {
            Debug.LogError("[LocalAccountManager] Không đọc được dữ liệu tài khoản: " + e.Message);
        }
    }

    public bool SaveAccounts()
    {
        string tempPath = AccountFile + ".tmp";

        try
        {
            string folder = Path.GetDirectoryName(AccountFile);
            if (!string.IsNullOrEmpty(folder))
                Directory.CreateDirectory(folder);

            string json = JsonUtility.ToJson(accountList, true);
            File.WriteAllText(tempPath, json);

            if (File.Exists(AccountFile))
                File.Delete(AccountFile);

            File.Move(tempPath, AccountFile);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[LocalAccountManager] Không lưu được tài khoản: " + e.Message);
            return false;
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }
        }
    }

    public PlayerAccount FindAccountByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        return accountList.accounts.FirstOrDefault(a =>
            string.Equals(a.email, email.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public PlayerAccount FindAccountByFirebaseUid(string firebaseUid)
    {
        if (string.IsNullOrWhiteSpace(firebaseUid)) return null;
        return accountList.accounts.FirstOrDefault(a =>
            string.Equals(a.firebaseUid, firebaseUid.Trim(), StringComparison.Ordinal));
    }

    public PlayerAccount FindAccountById(string accountId)
    {
        if (string.IsNullOrWhiteSpace(accountId)) return null;
        return accountList.accounts.FirstOrDefault(a =>
            string.Equals(a.accountId, accountId.Trim(), StringComparison.Ordinal));
    }

    /// <summary>
    /// Liên kết hồ sơ local với Firebase UID một cách an toàn.
    /// Giữ nguyên accountId cũ, không tạo hồ sơ trùng lặp, bảo toàn tiến trình chơi.
    /// Kiểm tra tính nhất quán dữ liệu: Báo lỗi nếu trùng email hoặc UID không khớp.
    /// </summary>
    public bool TryLinkFirebaseUser(
        string firebaseUid,
        string email,
        string displayName,
        bool emailVerified,
        string passwordForOfflineHash,
        out PlayerAccount account,
        out string message)
    {
        account = null;
        message = string.Empty;

        if (string.IsNullOrWhiteSpace(firebaseUid) || string.IsNullOrWhiteSpace(email))
        {
            message = "Thông tin xác thực Firebase không hợp lệ.";
            return false;
        }

        firebaseUid = firebaseUid.Trim();
        email = email.Trim();
        displayName = displayName?.Trim();

        PlayerAccount existingByUid = FindAccountByFirebaseUid(firebaseUid);
        PlayerAccount existingByEmail = FindAccountByEmail(email);

        // 1. Kiểm tra tính nhất quán dữ liệu (Data consistency check)
        if (existingByUid != null && existingByEmail != null && existingByUid != existingByEmail)
        {
            message = "Phát hiện dữ liệu không nhất quán: UID và Email thuộc về hai tài khoản local khác nhau.";
            Debug.LogError($"[LocalAccountManager] {message} UID: {firebaseUid}, Email: {email}");
            return false;
        }

        if (existingByUid != null && !string.IsNullOrEmpty(existingByUid.email) &&
            !string.Equals(existingByUid.email, email, StringComparison.OrdinalIgnoreCase))
        {
            message = "Phát hiện dữ liệu không nhất quán: Firebase UID này đã gắn với email khác.";
            Debug.LogError($"[LocalAccountManager] {message} (Đã có: {existingByUid.email}, Mới: {email})");
            return false;
        }

        if (existingByEmail != null && !string.IsNullOrEmpty(existingByEmail.firebaseUid) &&
            !string.Equals(existingByEmail.firebaseUid, firebaseUid, StringComparison.Ordinal))
        {
            message = "Phát hiện dữ liệu không nhất quán: Email này đã liên kết với Firebase UID khác.";
            Debug.LogError($"[LocalAccountManager] {message} (Đã có: {existingByEmail.firebaseUid}, Mới: {firebaseUid})");
            return false;
        }

        PlayerAccount targetAccount = existingByUid ?? existingByEmail;

        if (targetAccount == null)
        {
            // Tài khoản hoàn toàn mới trên thiết bị này: Tạo hồ sơ với accountId duy nhất ổn định
            string newAccountId = Guid.NewGuid().ToString("N");
            string salt = !string.IsNullOrEmpty(passwordForOfflineHash) ? PasswordHasher.GenerateSalt() : string.Empty;
            string hash = !string.IsNullOrEmpty(passwordForOfflineHash) ? PasswordHasher.HashPassword(passwordForOfflineHash, salt) : string.Empty;

            targetAccount = new PlayerAccount
            {
                accountId = newAccountId,
                firebaseUid = firebaseUid,
                email = email,
                username = !string.IsNullOrEmpty(displayName) ? displayName : email.Split('@')[0],
                displayName = !string.IsNullOrEmpty(displayName) ? displayName : email.Split('@')[0],
                passwordSalt = salt,
                passwordHash = hash,
                isEmailVerified = emailVerified,
                isVerifiedOnDevice = emailVerified,
                lastLoginTime = DateTime.UtcNow.ToString("o"),
                lastOnlineVerificationTimestamp = emailVerified ? DateTime.UtcNow.ToString("o") : string.Empty
            };

            if (emailVerified)
            {
                targetAccount.deviceAuthSignature = GenerateDeviceAuthSignature(targetAccount);
            }

            accountList.accounts.Add(targetAccount);
            Debug.Log($"[LocalAccountManager] Đã tạo hồ sơ local mới '{targetAccount.accountId}' cho Firebase UID '{firebaseUid}'.");
        }
        else
        {
            // Hồ sơ đã tồn tại: Cập nhật thông tin, GIỮ NGUYÊN accountId CŨ
            if (string.IsNullOrEmpty(targetAccount.firebaseUid))
            {
                targetAccount.firebaseUid = firebaseUid;
                Debug.Log($"[LocalAccountManager] Đã liên kết Firebase UID '{firebaseUid}' vào hồ sơ cũ '{targetAccount.accountId}'.");
            }

            if (!string.IsNullOrEmpty(displayName) && string.IsNullOrEmpty(targetAccount.displayName))
            {
                targetAccount.displayName = displayName;
            }

            targetAccount.isEmailVerified = emailVerified;
            targetAccount.lastLoginTime = DateTime.UtcNow.ToString("o");

            // Cập nhật salt/hash mật khẩu nếu có để hỗ trợ đăng nhập ngoại tuyến an toàn (không lưu text thô)
            if (!string.IsNullOrEmpty(passwordForOfflineHash))
            {
                targetAccount.passwordSalt = PasswordHasher.GenerateSalt();
                targetAccount.passwordHash = PasswordHasher.HashPassword(passwordForOfflineHash, targetAccount.passwordSalt);
            }

            if (emailVerified)
            {
                targetAccount.isVerifiedOnDevice = true;
                targetAccount.lastOnlineVerificationTimestamp = DateTime.UtcNow.ToString("o");
                targetAccount.deviceAuthSignature = GenerateDeviceAuthSignature(targetAccount);
            }
        }

        if (!SaveAccounts())
        {
            message = "Không thể lưu hồ sơ tài khoản.";
            return false;
        }

        CurrentAccount = targetAccount;
        SaveLoadManager.Instance?.SetCurrentAccount(targetAccount.accountId);
        account = targetAccount;
        return true;
    }

    /// <summary>
    /// Cho phép chơi offline trên cùng thiết bị nếu hồ sơ đã từng được xác minh trực tuyến qua Firebase.
    /// Không cho phép tài khoản mới hoặc chưa từng xác minh chơi offline.
    /// </summary>
    public bool TryOfflineLogin(
        string email,
        string password,
        out PlayerAccount account,
        out string message)
    {
        account = null;
        email = email?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            message = "Vui lòng nhập email và mật khẩu.";
            return false;
        }

        PlayerAccount target = FindAccountByEmail(email);
        if (target == null)
        {
            message = "Không tìm thấy tài khoản trên thiết bị này. Bạn cần có kết nối mạng để đăng nhập lần đầu.";
            return false;
        }

        if (!target.isVerifiedOnDevice || !target.isEmailVerified || string.IsNullOrEmpty(target.firebaseUid))
        {
            message = "Tài khoản chưa từng xác minh trực tuyến trên thiết bị này. Vui lòng kết nối mạng để đăng nhập.";
            return false;
        }

        // Kiểm tra chữ ký bảo mật thiết bị để chống việc sửa đổi thô sơ PlayerPrefs
        if (!VerifyDeviceAuthSignature(target))
        {
            message = "Dữ liệu xác thực thiết bị không khớp hoặc đã bị chỉnh sửa. Vui lòng kết nối mạng để đăng nhập lại.";
            return false;
        }

        // Kiểm tra mật khẩu bằng salted hash
        if (string.IsNullOrEmpty(target.passwordHash) ||
            !PasswordHasher.VerifyPassword(password, target.passwordSalt, target.passwordHash))
        {
            message = "Email hoặc mật khẩu không chính xác.";
            return false;
        }

        CurrentAccount = target;
        SaveLoadManager.Instance?.SetCurrentAccount(target.accountId);
        account = target;
        message = "Đăng nhập ngoại tuyến thành công.";
        Debug.Log($"[LocalAccountManager] Đăng nhập Offline thành công cho tài khoản '{target.email}'.");
        return true;
    }

    private string GenerateDeviceAuthSignature(PlayerAccount acc)
    {
        if (acc == null) return string.Empty;
        string raw = $"{acc.accountId}:{acc.firebaseUid}:{acc.email}:{SystemInfo.deviceUniqueIdentifier}";
        using (SHA256 sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes);
        }
    }

    private bool VerifyDeviceAuthSignature(PlayerAccount acc)
    {
        if (acc == null) return false;
        if (string.IsNullOrEmpty(acc.deviceAuthSignature))
        {
            // Nếu là tài khoản vừa tạo hoặc nâng cấp chưa có signature, chấp nhận nếu có UID và đã xác minh
            return !string.IsNullOrEmpty(acc.firebaseUid) && acc.isVerifiedOnDevice;
        }
        string expected = GenerateDeviceAuthSignature(acc);
        return string.Equals(expected, acc.deviceAuthSignature, StringComparison.Ordinal);
    }

    public bool Register(
        string username,
        string email,
        string displayName,
        string password,
        out string message)
    {
        username = username?.Trim();
        email = email?.Trim();
        displayName = displayName?.Trim();

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(displayName) ||
            string.IsNullOrEmpty(password))
        {
            message = "Vui lòng nhập đầy đủ thông tin.";
            return false;
        }

        if (username.Length < 3)
        {
            message = "Tên đăng nhập cần ít nhất 3 ký tự.";
            return false;
        }

        if (!IsValidEmail(email))
        {
            message = "Địa chỉ email không hợp lệ.";
            return false;
        }

        if (password.Length < 6)
        {
            message = "Mật khẩu cần ít nhất 6 ký tự.";
            return false;
        }

        if (accountList.accounts.Any(a =>
            string.Equals(a.username, username, StringComparison.OrdinalIgnoreCase)))
        {
            message = "Tên đăng nhập đã tồn tại.";
            return false;
        }

        if (accountList.accounts.Any(a =>
            string.Equals(a.email, email, StringComparison.OrdinalIgnoreCase)))
        {
            message = "Email này đã được sử dụng.";
            return false;
        }

        string accountId = Guid.NewGuid().ToString("N");
        string salt = PasswordHasher.GenerateSalt();
        string hash = PasswordHasher.HashPassword(password, salt);

        PlayerAccount account = new PlayerAccount
        {
            accountId = accountId,
            username = username,
            email = email,
            displayName = displayName,
            passwordSalt = salt,
            passwordHash = hash
        };

        accountList.accounts.Add(account);

        if (!SaveAccounts())
        {
            accountList.accounts.Remove(account);
            message = "Không thể lưu tài khoản. Vui lòng thử lại.";
            return false;
        }

        PlayerData newData = new PlayerData
        {
            accountId = accountId,
            displayName = displayName,
            currentLevel = 1
        };

        if (!LocalSaveManager.Save(newData))
        {
            accountList.accounts.Remove(account);
            SaveAccounts();
            message = "Không thể tạo dữ liệu người chơi.";
            return false;
        }

        CurrentAccount = account;
        CurrentPlayerData = newData;
        SaveLoadManager.Instance?.SetCurrentAccount(accountId);
        message = "Đăng ký thành công!";
        return true;
    }

    public bool Login(
        string email,
        string password,
        out string message)
    {
        email = email?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            message = "Vui lòng nhập email và mật khẩu.";
            return false;
        }

        PlayerAccount account = accountList.accounts.FirstOrDefault(a =>
            string.Equals(a.email, email, StringComparison.OrdinalIgnoreCase));

        if (account == null ||
            !PasswordHasher.VerifyPassword(password, account.passwordSalt, account.passwordHash))
        {
            message = "Email hoặc mật khẩu không đúng.";
            return false;
        }

        PlayerData data = LocalSaveManager.Load(account.accountId);

        if (data == null)
        {
            message = "Không thể tải dữ liệu người chơi.";
            return false;
        }

        data.accountId = account.accountId;
        data.displayName = account.displayName;

        CurrentAccount = account;
        CurrentPlayerData = data;
        SaveLoadManager.Instance?.SetCurrentAccount(account.accountId);

        message = "Đăng nhập thành công!";
        return true;
    }

    public bool SaveCurrentProgress()
    {
        if (CurrentAccount == null || CurrentPlayerData == null)
            return false;

        return LocalSaveManager.Save(CurrentPlayerData);
    }

    public void Logout()
    {
        SaveCurrentProgress();
        CurrentAccount = null;
        CurrentPlayerData = null;
        SaveLoadManager.Instance?.SetCurrentAccount(null);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var address = new System.Net.Mail.MailAddress(email);
            return address.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
