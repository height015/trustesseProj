using System;

namespace trustesseApp.Infrastructure.Configuration;

public class TokenSettings
{
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string JwtIssuer { get; set; }
    public int ExpiresInDays { get; set; }
    public string AllowedChars { get; set; }
    public int TokenLength { get; set; }
    public int MaxTokenLifetimeDays { get; set; }


}