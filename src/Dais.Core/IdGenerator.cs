using System.Security.Cryptography;

namespace DioRed.Dais.Core;

internal static class IdGenerator
{
    public static string Generate()
    {
        Span<byte> buffer = stackalloc byte[12];
        RandomNumberGenerator.Fill(buffer);

        return Convert.ToBase64String(buffer)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}