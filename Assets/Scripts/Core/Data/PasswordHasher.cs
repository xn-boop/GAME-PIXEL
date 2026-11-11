
using System;
using System.Security.Cryptography;

public static class PasswordHasher
{
    // Tạo salt ngẫu nhiên cho mỗi tài khoản
    public static string GenerateSalt()
    {
        byte[] salt = new byte[16];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        return Convert.ToBase64String(salt);
    }

    // Tạo password hash từ mật khẩu và salt
    public static string HashPassword(
        string password,
        string saltBase64)
    {
        byte[] salt = Convert.FromBase64String(saltBase64);
        byte[] passwordBytes =
            System.Text.Encoding.UTF8.GetBytes(password);

        using (var pbkdf2 = new Rfc2898DeriveBytes(
            passwordBytes,
            salt,
            600000,
            HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(32);
            return Convert.ToBase64String(hash);
        }
    }

    // Kiểm tra mật khẩu khi đăng nhập
    public static bool VerifyPassword(
        string password,
        string saltBase64,
        string expectedHash)
    {
        try
        {
            byte[] actualHash = Convert.FromBase64String(
                HashPassword(password, saltBase64));

            byte[] storedHash =
                Convert.FromBase64String(expectedHash);

            return CryptographicOperations.FixedTimeEquals(
                actualHash,
                storedHash);
        }
        catch
        {
            return false;
        }
    }
}
