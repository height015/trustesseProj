using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using trustesseApp.Core.Entities;
using trustesseApp.Core.Infrastructure;
using trustesseApp.Core.Infrastructure.Helpers;
using trustesseApp.Core.Infrastructure.Mappers;
using trustesseApp.Infrastructure.Helpers;
using trustesseApp.Models;
using trustesseApp.Service.Contracts;

namespace trustesseApp.Core.Factories;


public class AccountFactory : IAccountFactory
{
    private readonly SignInManager<AppUser> _signInManager;
    public readonly ITokenService _tokenService;
    public readonly UserManager<AppUser> _userManager;
    private readonly ResponseObj _errorObj;
    private readonly IEmailApiClientService _emailApiClientService;
    private readonly AppMailSettings _apiSettings;
    private readonly IOtpService _otpService;
    private readonly ITemplateReader _templateReader;

    public AccountFactory(SignInManager<AppUser> signInManager,
    ITokenService tokenService,
    UserManager<AppUser> userManager, IEmailApiClientService emailApiClientService,
     IOptions<AppMailSettings> apiOptions, IOtpService otpService, ITemplateReader templateReader)
    {
        _signInManager = signInManager;
        _tokenService = tokenService;
        _userManager = userManager;
        _emailApiClientService = emailApiClientService;
        _apiSettings = apiOptions.Value;
        _otpService = otpService;
        _templateReader = templateReader;
        _errorObj = new ResponseObj();
    }

    public async Task<DataResponseObj<UserRegVM>> UserRegFac(UserRegVM userRegVM)
    {
      
        var response = InitResponse<UserRegVM>();

        // Validate user input
        response = GenericObjValidator(userRegVM, response);
        if (!response.IsSucessful) return response;

        // Check if email already exists
        response = await EmailExists(userRegVM.EmailAddress, response);
        if (!response.IsSucessful) return response;


        // Create user entity
        var userObj = CreateUserEntity(userRegVM);


        // Attempt to create user in database
       response = await CreateUser(userObj, userRegVM.Password, response);
        if (!response.IsSucessful) return response;

        // Handle OTP/email confirmation if enabled
        await HandleUserVerification(userRegVM, userObj, response);

       
        response.IsSucessful = true;
        response.Entity = userRegVM;
        return response;
      
    }

    public async Task<DataResponseObj<string>> ConfirmAccountFactory(UserConfirmEmail userConfirmEmail)
    {
         var response = new DataResponseObj<string>
        {
            Entity = string.Empty,
            IsSucessful = false,
            ErrorResponse = new ResponseObj()
        };

        response = GenericObjValidator(userConfirmEmail, response);
        if (!response.IsSucessful) return response;

        var userResult = await _userManager.FindByEmailAsync(userConfirmEmail.Email);
            if (userResult == null){
                _errorObj.ErrorMessage = "User not found";
                response.IsSucessful = false;
                response.ErrorResponse = _errorObj;
                return response;
            }
            if (userResult.EmailConfirmed)
            {
                _errorObj.ErrorMessage = "Email already confirmed";
                response.IsSucessful = false;
                response.ErrorResponse = _errorObj;
                return response;
            }

            var confirmResult = await _otpService.ValidateOtp(userConfirmEmail.Email, null, userConfirmEmail.Token);

            if (!confirmResult.IsSucessful)
            {
                _errorObj.ErrorMessage = confirmResult.ErrorResponse.ErrorMessage;
                response.IsSucessful = false;
                _errorObj.TechnicalMessage = confirmResult.ErrorResponse.TechnicalMessage;
                response.ErrorResponse = _errorObj;
                return response;
            }

            userResult.EmailConfirmed = true;
            await _userManager.UpdateAsync(userResult);

            response.IsSucessful = true;
            response.Entity = "Email confirmed successfully";
            return response;
           
    }

    public async Task<DataResponseObj<UserResponseVM>> AccountLoginFactory(UserLoginVM userLoginVM)
    {
        bool lockout = false;
        var response = new DataResponseObj<UserResponseVM>
        {
            Entity = new UserResponseVM(),
            IsSucessful = false,
            ErrorResponse = new ResponseObj()
        };

        response = GenericObjValidator(userLoginVM, response);

        if (!response.IsSucessful) 
            return response;

         var userCheck = await _userManager.FindByEmailAsync(userLoginVM.EmailAddress.Trim()) 
                    ?? await _userManager.FindByNameAsync(userLoginVM.EmailAddress.Trim());

        if (userCheck == null)
        {
            _errorObj.ErrorMessage = "User does not exist";
            response.IsSucessful = false;
            response.ErrorResponse = _errorObj;
            return response;
        }
        
       

        if (await _userManager.IsLockedOutAsync(userCheck))
        {
            _errorObj.ErrorMessage = $"Your account is locked. Please wait till {userCheck.LockoutEnd.Value} (UTC Time) to login.";
            response.IsSucessful = false;
            response.ErrorResponse = _errorObj;
            return response;
        }
        

        var signInResult = await _signInManager.CheckPasswordSignInAsync(userCheck, userLoginVM.Password, lockout);

        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut)
            {
                _errorObj.ErrorMessage = "Your account is locked. Please try again later.";
                response.IsSucessful = false;
                response.ErrorResponse = _errorObj;
                return response;
            }


            _errorObj.ErrorMessage = "Email or Password does not match our records";
            response.IsSucessful = false;
            response.ErrorResponse = _errorObj;
            return response;

        }

        var principal = await _signInManager.CreateUserPrincipalAsync(userCheck);
        if (principal?.Identity?.IsAuthenticated ?? false)
        {
            var userResponse = new UserResponseVM
            {
                UserId = userCheck.Id,
                EmailAddress = userCheck.Email,
                UserName = userCheck.UserName,
                Token = _tokenService.GenerateToken(userCheck)
            };

            response.IsSucessful = true;
            response.Entity = userResponse;
        }
        else
        {
            _errorObj.ErrorMessage = "Authentication failed. Please try again.";
            response.IsSucessful = false;
            response.ErrorResponse = _errorObj; 
        }

        return response;
    }

    public async Task<DataResponseObj<UserVM>> AccountDetailsFactory(string userId)
    {
        var response = InitResponse<UserVM>();

        var model = await GetUser(userId);
        if (!model.IsSucessful)
        {
            response.ErrorResponse.ErrorMessage = model.ErrorResponse.ErrorMessage;
            return response;
        }

        var gg = model.Entity.ToDTO();
        

        response.IsSucessful = true;
        response.Entity = gg;
        return response;
    }


#region Utilities
    private string ReplaceTemplateValues(string template, Dictionary<string, string> replacements)
    {
        foreach (var placeholder in replacements)
        {
            template = template.Replace($"%{placeholder.Key}%", placeholder.Value);
        }
        return template;
    }

    private DataResponseObj<TResponse> GenericObjValidator<TModel, TResponse>(TModel model, DataResponseObj<TResponse> response)
    {
       
        if (!model.TryValidate(out var msg))
        {
            response.ErrorResponse.ErrorMessage = msg;
            response.IsSucessful = false;
            response.ErrorResponse.TechnicalMessage = $"Validation error occurred: {msg}";
            return response;
        }
    response.IsSucessful = true;
    return response;
    }

#endregion

#region Users Registration Methodsw

private async Task<DataResponseObj<UserRegVM>> EmailExists(string email, DataResponseObj<UserRegVM> response)
{
    var emailCheck = await _userManager.FindByEmailAsync(email.ToLower().Trim());
    if (emailCheck != null)
    {
        response.ErrorResponse.ErrorMessage = "Email already exists";
        response.IsSucessful = false;
        response.ErrorResponse.TechnicalMessage = "Email already exists";
        return response;
    }
    response.IsSucessful = true;
    return response;
}

private AppUser CreateUserEntity(UserRegVM userRegVM)
{
    var userObj = userRegVM.ToEntity();

    return userObj;
}

private async Task<DataResponseObj<UserRegVM>>CreateUser(AppUser userObj, string password, DataResponseObj<UserRegVM> response)
{
    var result = await _userManager.CreateAsync(userObj, password);
    if (!result.Succeeded)
    {
        response.IsSucessful = false;
        response.ErrorResponse.ErrorMessage = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description));
        response.ErrorResponse.TechnicalMessage = string.Join(Environment.NewLine, $"Error occurred: {result.Errors.Select(e => e.Description)}");
        return response;
    }
    response.IsSucessful = true;
    return response;
}

private async Task<DataResponseObj<UserRegVM>> HandleUserVerification(UserRegVM userRegVM, AppUser userObj, DataResponseObj<UserRegVM> response)
{
        var opt = await _otpService.GenerateOtp(userObj.Email);

        if (!opt.IsSucessful)
        {
            response.ErrorResponse.ErrorMessage = opt.ErrorResponse.ErrorMessage;
            response.ErrorResponse.TechnicalMessage = opt.ErrorResponse.TechnicalMessage;
            response.IsSucessful = false;
            return response;
        }

        var mesTep =  EmailTemplates.GetOtpEmailTemplate();

        var replacements = new Dictionary<string, string>
        {
            { "%EMAIL%", userRegVM.EmailAddress },
            { "%OtpCode%", opt.Entity.OtpCode },
            { "%APP_NAME%", "Trustesse Accessment 😂 " },
            { "%Year%", DateTime.UtcNow.Year.ToString() },
            { "%COMPANY_NAME%", "Trustesse Limited." },
            { "%FACEBOOK_LINK%", "https://facebook.com/page__" },
            { "%TWITTER_LINK%", "https://twitter.com/page__" },
            { "%LINKEDIN_LINK%", "https://linkedin.com/company/page__" },
            { "%INSTAGRAM_LINK%", "https://instagram.com/page__" }
        };


        string emailContent = ReplaceTemplateValues(mesTep, replacements);

        await SendEmailVerification(userRegVM.EmailAddress, emailContent);
    

     response.IsSucessful = true;
     return response;
}


private async Task SendEmailVerification(string email, string emailContent)
{
            var request = new MailtrapEmailRequest
            {
                From = new EmailAddress
                {
                    Email = "2height@gmail.com",
                    Name = "Trustesse Accessment"
                },
                To = new List<EmailRecipient>
                {
                    new EmailRecipient { Email = email }
                },
                Subject = "Email Confirmation",
                Text = emailContent,
                Category = "Integration Test"
            };

    var result =  await _emailApiClientService.SendEmailAsync<MailtrapEmailRequest, MailtrapResponse>("send",request);

}

private async Task<DataResponseObj<AppUser>> GetUser(string userId)
{
    var response = InitResponse<AppUser>();
    response.Entity = null;

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
    {
        _errorObj.ErrorMessage = "User not found";
        response.IsSucessful = false;
        
        response.ErrorResponse = _errorObj;
        return response;
    }
    if (userId == null || userId.Length < 5)
    {
        _errorObj.ErrorMessage = "Invalid User Id";
        response.IsSucessful = false;
        response.ErrorResponse = _errorObj;
        return response;
    }

    response.IsSucessful = true;
    response.Entity = user;
    return response;
}

private DataResponseObj<T> InitResponse<T>() where T : class, new()
{
    return new DataResponseObj<T>
    {
        Entity = new T(), 
        IsSucessful = false,
        ErrorResponse = new ResponseObj()
    };
}



}
#endregion




