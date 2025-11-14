using System.Security.Cryptography;
using System.Text;

namespace Dais.Core.OAuth;

public static class Pkce
{
    public static bool IsValidVerifierFormat(string v)
    {
        return v.Length is >= 43 and <= 128 && v.All(IsBase64UrlChar);
    }

    public static string ComputeS256(string verifier)
    {
        return SHA256.HashData(Encoding.ASCII.GetBytes(verifier)).ToBase64Url();
    }

    private static bool IsBase64UrlChar(char c)
    {
        return c is
            >= 'a' and <= 'z' or
            >= 'A' and <= 'Z' or
            >= '0' and <= '9' or
            '-' or
            '_';
    }
}