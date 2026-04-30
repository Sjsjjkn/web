using System.Security.Cryptography;
using System.Text;

namespace Backend.Security
{
    /// <summary>
    /// 密码哈希工具（PBKDF2）。
    /// 说明：这里不依赖第三方包，保证在当前项目中可直接使用。
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128-bit
        private const int KeySize = 32;  // 256-bit
        private const int Iterations = 100_000;

        /// <summary>
        /// 生成哈希后的密码字符串，格式：pbkdf2$iterations$saltBase64$hashBase64
        /// </summary>
        public static string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("密码不能为空", nameof(password));

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize
            );

            return $"pbkdf2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// 验证密码。兼容旧的明文密码：当 storedPassword 不是 pbkdf2 格式时，按明文比较。
        /// </summary>
        public static bool Verify(string password, string storedPassword)
        {
            if (storedPassword == null) return false;

            if (!storedPassword.StartsWith("pbkdf2$", StringComparison.Ordinal))
            {
                // 兼容旧数据：原系统为明文存储
                return storedPassword == password;
            }

            var parts = storedPassword.Split('$');
            if (parts.Length != 4) return false;

            if (!int.TryParse(parts[1], out var iterations)) return false;
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length
            );

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}

