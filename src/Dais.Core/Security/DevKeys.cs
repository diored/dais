using System.Security.Cryptography;

using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Dais.Core.Security;

public sealed class DevKeys
{
    private const string KeyFileName = "crypto_key";

    public RSA RsaKey { get; }
    public RsaSecurityKey RsaSecurityKey { get; }
    public string KeyId { get; }

    public DevKeys(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, KeyFileName);

        RSA rsa;
        if (File.Exists(path))
        {
            // load existing private key
            rsa = RSA.Create();
            var privateBytes = File.ReadAllBytes(path);
            rsa.ImportRSAPrivateKey(privateBytes, out _);
        }
        else
        {
            // generate new key and save it
            rsa = RSA.Create(2048);
            var privateBytes = rsa.ExportRSAPrivateKey();
            File.WriteAllBytes(path, privateBytes);
        }

        RsaKey = rsa;
        RsaSecurityKey = new RsaSecurityKey(rsa);

        // kid must be stable, so derived from public key
        KeyId = ComputeKeyId(rsa);
        RsaSecurityKey.KeyId = KeyId;
    }

    private static string ComputeKeyId(RSA rsa)
    {
        // Use SHA256(public key) → Base64Url
        var parameters = rsa.ExportParameters(false);

        // Combine modulus + exponent as recommended
        var data = parameters.Modulus!.Concat(parameters.Exponent!).ToArray();
        var hash = SHA256.HashData(data);

        return hash.AsSpan()[..8].ToBase64Url(); // 8 bytes more than enough
    }
}