using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    // Liên kết tiến trình với tài khoản
    public string accountId;
    public string displayName;
    public string firebaseUid;

    // Màn chơi
    public int currentLevel = 1;

    // Danh sách màn đã mở khóa
    public List<int> unlockedLevels = new List<int> { 1 };

    // Các câu đố đã giải
    public List<string> solvedPuzzles = new List<string>();

    // Các đồ vật đã tìm và đặt đúng vị trí
    public List<string> collectedItems = new List<string>();

    // Phiên bản dữ liệu lưu
    public int saveVersion = 1;
}
