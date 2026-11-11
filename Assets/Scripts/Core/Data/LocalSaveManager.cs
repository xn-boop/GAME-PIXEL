using System;
using System.IO;
using UnityEngine;

public static class LocalSaveManager
{
    private static string SaveFolder =>
        Path.Combine(Application.persistentDataPath, "saves");

    public static string GetSavePath(string accountId)
    {
        // Không cho phép ID chứa đường dẫn tùy ý
        string safeId = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes(accountId));

        safeId = safeId.Replace("/", "_").Replace("+", "-")
                       .Replace("=", "");

        return Path.Combine(SaveFolder, safeId + ".json");
    }

    public static bool HasSave(string accountId)
    {
        if (string.IsNullOrWhiteSpace(accountId)) return false;
        return File.Exists(GetSavePath(accountId));
    }

    public static bool Save(PlayerData data)
    {
        if (data == null || string.IsNullOrWhiteSpace(data.accountId))
            return false;

        string tempPath = null;

        try
        {
            Directory.CreateDirectory(SaveFolder);

            string path = GetSavePath(data.accountId);
            tempPath = path + ".tmp";

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(tempPath, json);

            // Giữ file cũ nếu có lỗi trong quá trình ghi
            if (File.Exists(path))
            {
                string backupPath = path + ".bak";
                File.Copy(path, backupPath, true);
                File.Delete(path);
            }

            File.Move(tempPath, path);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[LocalSaveManager] Lưu tiến trình thất bại: " + e.Message);
            return false;
        }
        finally
        {
            if (tempPath != null && File.Exists(tempPath))
            {
                try { File.Delete(tempPath); }
                catch { }
            }
        }
    }

    public static PlayerData Load(string accountId)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            return null;

        string path = GetSavePath(accountId);
        bool fileExisted = File.Exists(path);

        try
        {
            if (fileExisted)
            {
                string json = File.ReadAllText(path);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    PlayerData data = JsonUtility.FromJson<PlayerData>(json);
                    if (data != null && data.accountId == accountId)
                        return data;
                }
            }

            // Thử phục hồi bản sao lưu nếu file chính bị lỗi
            string backupPath = path + ".bak";
            if (File.Exists(backupPath))
            {
                string json = File.ReadAllText(backupPath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    PlayerData data = JsonUtility.FromJson<PlayerData>(json);
                    if (data != null && data.accountId == accountId)
                    {
                        Debug.LogWarning($"[LocalSaveManager] Đã phục hồi dữ liệu từ bản sao lưu: {backupPath}");
                        return data;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[LocalSaveManager] Đọc tiến trình thất bại: " + e.Message);
        }

        // Nếu file đã tồn tại nhưng không thể đọc (bị hỏng), KHÔNG tự tạo save trống đè lên
        if (fileExisted)
        {
            Debug.LogError($"[LocalSaveManager] File save '{path}' đã tồn tại nhưng bị hỏng hoặc không đúng accountId. Giữ nguyên file để bảo vệ dữ liệu.");
            return null;
        }

        // Chưa từng có save: khởi tạo tiến trình mới
        return CreateNewSave(accountId);
    }

    private static PlayerData CreateNewSave(string accountId)
    {
        return new PlayerData
        {
            accountId = accountId,
            currentLevel = 1
        };
    }
}
