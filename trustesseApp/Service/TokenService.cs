using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using trustesseApp.Core.Entities;
using trustesseApp.Core.Repository;
using trustesseApp.Infrastructure.Configuration;
using trustesseApp.Infrastructure.Helpers;
using trustesseApp.Service.Contracts;

namespace trustesseApp.Service;



public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _symmetricSecurityKey;
    private readonly TokenSettings _settings;

    private readonly IRepository<AccessToken> _accessTokenRepository;
    private readonly ILogger<TokenService> _logger;
    private readonly ResponseObj _errorObj;

    public TokenService(IOptions<TokenSettings> settings,
        IRepository<AccessToken> accessTokenRepository,
        ILogger<TokenService> logger)
    {
        _accessTokenRepository = accessTokenRepository;
        _logger = logger;

        _settings = settings.Value;
        _symmetricSecurityKey =
            new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(
            _settings.Key
            ));
        _errorObj = new ResponseObj();


    }
    public string GenerateToken(AppUser appUserObj, List<string> roles = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, appUserObj.Email)
        };

       if (roles != null && roles.Count > 0)
       {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var creds = new SigningCredentials(_symmetricSecurityKey, SecurityAlgorithms.HmacSha512Signature);
        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(_settings.ExpiresInDays),
            SigningCredentials = creds,
            Issuer = _settings.Issuer,
            Audience = _settings.JwtIssuer
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescription);

        var tokenStr = tokenHandler.WriteToken(token);
        return tokenStr;
    }

    public void InvalidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _settings.Issuer
            ));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidateAudience = true
        };
        SecurityToken validatedToken;
        var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
    }


    // public async Task<DataResponseObj<AccessToken>> GenerateTokenAsync(string userId, TimeSpan? lifetime = null)
    // {

    //     var response = new DataResponseObj<AccessToken>
    //     {
    //         IsSucessful = false,
    //         Entity = new AccessToken(),
    //         ErrorResponse = new ResponseObj()
    //     };
    //     try
    //     {
    //         // Validate lifetime
    //         var tokenLifetime = lifetime ?? TimeSpan.FromHours(1);

    //         if (tokenLifetime <= TimeSpan.Zero)
    //         {

    //             response.IsSucessful = false;
    //             _errorObj.ErrorMessage = "Token lifetime must be in the future";
    //             _errorObj.TechnicalMessage = $"Token lifetime not in the future {tokenLifetime} now {DateTime.UtcNow.TimeOfDay}";
    //             response.ErrorResponse = _errorObj;
    //             _logger.LogError(_errorObj.TechnicalMessage);
    //             _logger.LogError(_errorObj.ErrorMessage);
    //             return response;

    //         }

    //         if (tokenLifetime > TimeSpan.FromDays(_settings.MaxTokenLifetimeDays))
    //         {
    //             response.IsSucessful = false;
    //             _errorObj.ErrorMessage = "Token lifetime cannot exceed max token lifetime";
    //             _errorObj.TechnicalMessage = $"Token lifetime {tokenLifetime} exceeds max token lifetime {_settings.MaxTokenLifetimeDays}";
    //             response.ErrorResponse = _errorObj;
    //             _logger.LogError(_errorObj.TechnicalMessage);
    //             _logger.LogError(_errorObj.ErrorMessage);
    //             return response;
    //         }

    //         // Generate unique token
    //         string token;
    //         do
    //         {
    //             token = GenerateRandomToken();
    //         }
    //         while (await _accessTokenRepository.TableNoTracking.AnyAsync(t => t.Token == token));

    //         var tokenRecord = new AccessToken
    //         {
    //             Token = token,
    //             CreatedAt = DateTime.UtcNow,
    //             ExpiresAt = DateTime.UtcNow.Add(tokenLifetime),
    //             UserId = userId,
    //             IsUsed = false
    //         };

    //         var retVal = await _accessTokenRepository.Insert(tokenRecord);

    //         if (retVal == null || retVal.Id == Guid.Empty)
    //         {
    //             response.IsSucessful = false;
    //             _errorObj.ErrorMessage = "Failed to create token record";
    //             _errorObj.TechnicalMessage = $"Failed to create token record in database {retVal} ";
    //             response.ErrorResponse = _errorObj;
    //             _logger.LogError(_errorObj.TechnicalMessage);
    //             _logger.LogError(_errorObj.ErrorMessage);
    //             return response;
    //         }

    //         response.IsSucessful = true;
    //         response.Entity = tokenRecord;
    //         _logger.LogInformation($"Token generated successfully for user {userId} .. Token: {token}");
    //         return response;

    //     }
    //     catch (Exception ex)
    //     {
    //         _errorObj.ErrorMessage = "Error generating token";
    //         _errorObj.TechnicalMessage = $"Failed to create token record in database {ex.Message} ";
    //         _logger.LogError(ex, _errorObj.TechnicalMessage);
    //         _logger.LogError(ex, "Error generating token");
    //         _logger.LogError(ex.Message, ex.Source, ex.StackTrace, ex.InnerException);
    //         response.ErrorResponse = _errorObj;
    //         response.IsSucessful = false;
    //         return response;
    //     }
    // }

public async Task<DataResponseObj<AccessToken>> GenerateTokenAsync(string userId, TimeSpan? lifetime = null)
{
    var response = new DataResponseObj<AccessToken>
    {
        IsSucessful = false,
        Entity = new AccessToken(),
        ErrorResponse = new ResponseObj()
    };

    try
    {
        var tokenLifetime = lifetime ?? TimeSpan.FromHours(1);
        var maxLifetime = TimeSpan.FromDays(_settings.MaxTokenLifetimeDays);
        var capped = false;

        if (tokenLifetime <= TimeSpan.Zero)
        {
            response.IsSucessful = false;
            _errorObj.ErrorMessage = "Token lifetime must be in the future";
            _errorObj.TechnicalMessage = $"Token lifetime not in the future {tokenLifetime} now {DateTime.UtcNow.TimeOfDay}";
            response.ErrorResponse = _errorObj;
            _logger.LogError(_errorObj.TechnicalMessage);
            _logger.LogError(_errorObj.ErrorMessage);
            return response;
        }

        if (tokenLifetime > maxLifetime)
            {
                capped = true;
                tokenLifetime = maxLifetime;
                _logger.LogInformation($"Token lifetime capped to maximum {maxLifetime.TotalDays} days");
            }

        // Generate unique token
        string token;
        do
        {
            token = GenerateRandomToken();
        } 
        while (await _accessTokenRepository.TableNoTracking.AnyAsync(t => t.Token == token));

        // Create token record
        var tokenRecord = new AccessToken
        {
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(tokenLifetime),
            UserId = userId,
            IsUsed = false
        };

        var retVal = await _accessTokenRepository.Insert(tokenRecord);

        if (retVal == null || retVal.Id == Guid.Empty)
        {
            response.IsSucessful = false;
            _errorObj.ErrorMessage = "Failed to create token record";
            _errorObj.TechnicalMessage = $"Failed to create token record in database {retVal}";
            response.ErrorResponse = _errorObj;
            _logger.LogError(_errorObj.TechnicalMessage);
            _logger.LogError(_errorObj.ErrorMessage);
            return response;
        }

        response.IsSucessful = true;
        response.Entity = tokenRecord;
        
        if (capped)
        {
            _errorObj.FriendlyMessage = $"Token lifetime was capped to maximum {maxLifetime.TotalDays} days";
            _logger.LogInformation( _errorObj.FriendlyMessage);
        }
        response.ErrorResponse = _errorObj;


        _logger.LogInformation($"Token generated successfully for user {userId}. Token: {token}");
        return response;
    }
    catch (Exception ex)
    {
        _errorObj.ErrorMessage = "Error generating token";
        _errorObj.TechnicalMessage = $"Failed to create token record in database: {ex.Message}";
        _logger.LogError(ex, _errorObj.TechnicalMessage);
        response.ErrorResponse = _errorObj;
        response.IsSucessful = false;
        return response;
    }
}
    public async Task<DataResponseObj<AccessToken>> ValidateTokenAsync(string token, string userId)
    {
        var response = new DataResponseObj<AccessToken>
        {
            IsSucessful = false,
            Entity = new AccessToken(),
            ErrorResponse = new ResponseObj()
        };
        try
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(token) || token.Length != _settings.TokenLength)
            {
                response.IsSucessful = false;
                _errorObj.ErrorMessage = "Invalid token format";
                _errorObj.TechnicalMessage = $"Invalid token format {token} shoud be of length {_settings.TokenLength}";
                response.ErrorResponse = _errorObj;
                _logger.LogError(_errorObj.TechnicalMessage);
                _logger.LogError(_errorObj.ErrorMessage);
                return response;

            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                response.IsSucessful = false;
                _errorObj.ErrorMessage = "Invalid user ID";
                _errorObj.TechnicalMessage = $"Invalid user ID {userId} ";
                response.ErrorResponse = _errorObj;
                _logger.LogError(_errorObj.TechnicalMessage);
                _logger.LogError(_errorObj.ErrorMessage);
                return response;

            }

            // Find token in database
            var tokenRecord = await _accessTokenRepository.Table
                .FirstOrDefaultAsync(t => t.Token == token && t.UserId == userId);

            if (tokenRecord == null)
            {
                response.IsSucessful = false;
                _errorObj.ErrorMessage = "Token not found";
                _errorObj.TechnicalMessage = $"Token not found in database {token} ";
                response.ErrorResponse = _errorObj;
                _logger.LogError(_errorObj.TechnicalMessage);
                _logger.LogError(_errorObj.ErrorMessage);
                return response;

            }
            // Check if token is already used
            if (tokenRecord.IsUsed)
            {
                response.IsSucessful = false;
                _errorObj.ErrorMessage = "Token has already been used";
                _errorObj.TechnicalMessage = $"Token has already been used {token} ";
                response.ErrorResponse = _errorObj;
                _logger.LogError(_errorObj.TechnicalMessage);
                _logger.LogError(_errorObj.ErrorMessage);
                return response;

            }

            // Check expiration
            if (tokenRecord.ExpiresAt < DateTime.UtcNow)
            {
                response.IsSucessful = false;
                _errorObj.ErrorMessage = "Token has expired";
                _errorObj.TechnicalMessage = $"Token has expired {token} ";
                response.ErrorResponse = _errorObj;
                _logger.LogError(_errorObj.TechnicalMessage);
                _logger.LogError(_errorObj.ErrorMessage);
                return response;

            }

            tokenRecord.IsUsed = true;
            var retVal = await _accessTokenRepository.Update(tokenRecord);

            if (retVal == null || retVal.Id == Guid.Empty)
            {
                response.IsSucessful = false;
                _errorObj.ErrorMessage = "Failed to update token record";
                _errorObj.TechnicalMessage = $"Failed to update token record in database {retVal} ";
                response.ErrorResponse = _errorObj;
                _logger.LogError(_errorObj.TechnicalMessage);
                _logger.LogError(_errorObj.ErrorMessage);
                return response;
            }

            response.IsSucessful = true;
            response.Entity = tokenRecord;
            _logger.LogInformation($"Token validated successfully for user {userId} .. Token: {token}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            _errorObj.ErrorMessage = "Error validating token";
            _errorObj.TechnicalMessage = $"Error validating token {ex.Message} ";
            response.ErrorResponse = _errorObj;
            _logger.LogError(_errorObj.TechnicalMessage);
            _logger.LogError(_errorObj.ErrorMessage);
            response.IsSucessful = false;
            return response;
        }
    }


    private string GenerateRandomToken()
    {
        var random = new Random();
        return new string(Enumerable.Repeat(_settings.AllowedChars, _settings.TokenLength)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

}
