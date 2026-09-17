using CleanArchitecture.Application.Interfaces;
using System.Security.Cryptography;

namespace CleanArchitecture.Infrastructure.Security;

public class SecureTokenGenerator : ISecureTokenGenerator
{
    public string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
