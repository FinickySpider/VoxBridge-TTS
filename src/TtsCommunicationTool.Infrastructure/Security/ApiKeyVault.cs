using System.Security.Cryptography;
using System.Text;

namespace TtsCommunicationTool.Infrastructure.Security;

/// <summary>
/// DPAPI-backed helpers for encrypting/decrypting secrets (e.g. the ElevenLabs API key).
/// Uses <see cref="DataProtectionScope.CurrentUser"/> so the encrypted blob is only
/// usable by the same Windows user account that created it.
/// </summary>
internal static class ApiKeyVault
{
    /// <summary>Encrypts <paramref name="plaintext"/> with DPAPI and returns a Base64 string safe for JSON storage.</summary>
    public static string Encrypt(string plaintext)
    {
        var bytes     = Encoding.UTF8.GetBytes(plaintext);
        var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// Decrypts a Base64 DPAPI blob back to plaintext.
    /// Returns <c>null</c> if <paramref name="encryptedBase64"/> is null/empty or decryption fails.
    /// </summary>
    public static string? Decrypt(string? encryptedBase64)
    {
        if (string.IsNullOrEmpty(encryptedBase64)) return null;
        try
        {
            var bytes = Convert.FromBase64String(encryptedBase64);
            var plain = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plain);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Returns true if a stored encrypted key is present and can be decrypted.</summary>
    public static bool HasKey(string? encryptedBase64) => Decrypt(encryptedBase64) is not null;

    /// <summary>
    /// Returns the last <paramref name="count"/> characters of <paramref name="plaintext"/>
    /// as a masked tail for display (e.g. "a1b2").  Short keys are fully masked with asterisks.
    /// </summary>
    public static string ComputeTail(string plaintext, int count = 4)
        => plaintext.Length <= count
            ? new string('*', plaintext.Length)
            : plaintext[^count..];
}
