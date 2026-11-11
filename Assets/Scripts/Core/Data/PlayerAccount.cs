using System;

[Serializable]
public class PlayerAccount
{
    // ID tài khoản local cố định
    public string accountId;
    public string username;
    public string email;
    public string displayName;

    // Salt và password hash, tuyệt đối không lưu mật khẩu gốc
    public string passwordSalt;
    public string passwordHash;

    // Liên kết Firebase Authentication
    public string firebaseUid;
    public bool isEmailVerified;
    public string lastLoginTime;

    // Đánh dấu thiết bị đã xác minh trực tuyến thành công để cho phép chơi offline
    public bool isVerifiedOnDevice;
    public string lastOnlineVerificationTimestamp;
    public string deviceAuthSignature;
}
