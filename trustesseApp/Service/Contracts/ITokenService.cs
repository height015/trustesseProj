using System;
using trustesseApp.Core.Entities;
using trustesseApp.Infrastructure.Helpers;

namespace trustesseApp.Service.Contracts;

public interface ITokenService
{
    string GenerateToken(AppUser appUserObj, List<string> roles = null);
    void InvalidateJwtToken(string token);


    Task<DataResponseObj<AccessToken>>  GenerateTokenAsync(string userId, TimeSpan? lifetime = null);
    Task<DataResponseObj<AccessToken>> ValidateTokenAsync(string token, string userId);
}