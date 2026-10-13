using System.Diagnostics.CodeAnalysis;

namespace Partners.Application.Partners;

public static class VersionToken
{
    private const int Size = 8;

    public static string Encode(byte[] version) => Convert.ToBase64String(version);

    public static bool TryDecode(string? token, [NotNullWhen(true)] out byte[]? version)
    {
        version = null;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var buffer = new byte[Size];
        if (!Convert.TryFromBase64String(token, buffer, out var written) || written != Size)
        {
            return false;
        }

        version = buffer;
        return true;
    }
}
