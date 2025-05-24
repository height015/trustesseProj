using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using trustesseApp.Core;
using trustesseApp.Core.Entities;
using trustesseApp.Core.Factories;
using trustesseApp.Core.Infrastructure;
using trustesseApp.Core.Infrastructure.Helpers;
using trustesseApp.Models;
using trustesseApp.Service.Contracts;

namespace trustesseApp.Controllers.Accounts;


[Route($"api/{RouteHelper.Identity}")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly SignInManager<AppUser> _signInManager;
    public readonly ITokenService _tokenService;
    public readonly UserManager<AppUser> _userManager;
    private readonly IAccountFactory _accountFactory;
    private readonly ILogger<AccountController> _logger;



    public AccountController(UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
        ITokenService tokenService,
         IAccountFactory accountFactory,
        ILogger<AccountController> logger
        )
    {
        _signInManager = signInManager;
        _tokenService = tokenService;
        _userManager = userManager;
        _accountFactory = accountFactory;
        _logger = logger;

    }

    /// <summary>
    /// Users Registering
    /// </summary>
    /// <param object="UserRegVM">The user object</param>
    /// <returns>The response</returns>
    /// <response code="200">Returns success</response>
    /// <response code="400">If process fails</response>

    [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.BadRequest)]
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegVM userRegistrationVM)
    {
        try
        {

            var response = await _accountFactory.UserRegFac(userRegistrationVM);
            if (response.IsSucessful)
            {

                return Ok(ApiResponse<UserRegVM>.SuccessResponse(null, "Registration Successful"));
            }

            return BadRequest(ApiResponse<UserRegVM>.FailResponse(response.ErrorResponse.ErrorMessage));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.Source, ex.StackTrace);
            return BadRequest(ApiResponse<UserRegVM>.FailResponse("Process Error Occured"));
        }
    }


    /// <summary>
    /// Confirm Registration Account
    /// </summary>
    /// <param object="UserConfirmEmail">The object</param>
    /// <returns>The response </returns>
    /// <response code="200">Returns success message</response>
    /// <response code="400">Returns failure message</response>

    [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.BadRequest)]

    [HttpGet("confirm-email")]
    public async Task<ActionResult> ConfirmEmail([FromQuery] UserConfirmEmail userConfirmEmail)
    {

        try
        {
            var response = await _accountFactory.ConfirmAccountFactory(userConfirmEmail);
            if (response.IsSucessful)
            {
                return Ok(ApiResponse<string>.SuccessResponse("Email is Confirmed"));
            }
            return BadRequest(ApiResponse<string>.FailResponse(response.ErrorResponse.ErrorMessage));

        }
        catch (Exception ex)
        {
            var message = ex.InnerException;
            _logger.LogError(ex.Message, ex.Source, ex.StackTrace, message);
            return BadRequest(ApiResponse<string>.FailResponse($"Process Error"));
        }

    }

    /// <summary>
    /// User Login
    /// </summary>
    /// <param object="UserLoginVM">The object</param>
    /// <returns>The response </returns>
    /// <response code="200">Returns success message</response>
    /// <response code="400">Returns failure message</response>

    [ProducesResponseType(typeof(ApiResponse<UserResponseVM>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ApiResponse<UserLoginVM>), (int)HttpStatusCode.BadRequest)]
    [HttpPost("login")]
    public async Task<ActionResult<UserResponseVM>> Login(UserLoginVM userLoginVM)
    {
        try
        {
            var retVal = await _accountFactory.AccountLoginFactory(userLoginVM);

            if (retVal.IsSucessful)
            {
                return Ok(ApiResponse<UserResponseVM>.SuccessResponse(retVal.Entity, "Login Successful"));
            }

            return BadRequest(ApiResponse<UserResponseVM>.FailResponse(retVal.ErrorResponse.ErrorMessage));
        }

        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.Source, ex.StackTrace);
            return BadRequest(ApiResponse<string>.FailResponse("Process Error Occured"));
        }
    }


    /// <summary>
    /// User Login
    /// </summary>
    /// <param object="UserLoginVM">The object</param>
    /// <returns>The response </returns>
    /// <response code="200">Returns success message</response>
    /// <response code="400">Returns failure message</response>
    /// <response code="401">Unauthorized</response>
    /// 
    [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserVM>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), (int)HttpStatusCode.BadRequest)]
    [Authorize]
    [HttpGet("details/{userId}")]
    public async Task<ActionResult> Details(string userId)
    {
        try
        {
            var retVal = await _accountFactory.AccountDetailsFactory(userId);
            if (!retVal.IsSucessful)
            {
                return BadRequest(ApiResponse<UserVM>.FailResponse(retVal.ErrorResponse.ErrorMessage));
            }

            return Ok(ApiResponse<UserVM>.SuccessResponse(retVal.Entity, "Successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.Source, ex.StackTrace);
            return BadRequest(ApiResponse<string>.FailResponse("Process Error Occured"));
        }
    }



    /// <summary>
    /// Generates a new 6-digit alphanumeric token
    /// </summary>
    /// <param name="request">Token generation request</param>
    /// <returns>Generated token information</returns>
    /// <response code="401">Unauthorized</response>
    /// <response code="400">If the request is invalid or token generation fails</response>
    /// <response code="200">Returns success message</response>
    /// 
    [ProducesResponseType(typeof(ApiResponse<TokenGenerationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [Authorize]
    [HttpPost("generate")]

    public async Task<IActionResult> GenerateToken([FromBody] TokenGenerationRequest request)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        if (userEmail == null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await _tokenService.GenerateTokenAsync(
            user.Id,
            request.LifetimeInHours.HasValue ? TimeSpan.FromHours(request.LifetimeInHours.Value) : null);

        if (!result.IsSucessful)
        {
            return BadRequest(ApiResponse<string>.FailResponse(result.ErrorResponse.ErrorMessage));
        }

        var res = new TokenGenerationResponse
        {
            Token = result.Entity.Token,
            ExpiresAt = result.Entity.ExpiresAt
        };


        return Ok(ApiResponse<TokenGenerationResponse>.SuccessResponse(res, "Successful"));

    }

    /// <summary>
    /// Validates a token
    /// </summary>
    /// <param name="request">Token validation request</param>
    /// <response code="401">Unauthorized</response>
    /// <response code="200">Returns validation result</response>
    /// <response code="400">Returns failure message</response>

    /// <returns>Validation result</returns>

    [ProducesResponseType(typeof(ApiResponse<TokenValidationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [Authorize]
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateToken([FromBody] TokenValidationRequest request)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email);
        if (userEmail == null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await _tokenService.ValidateTokenAsync(request.Token, user.Id);

        if (!result.IsSucessful)
        {

            return BadRequest(ApiResponse<string>.FailResponse(result.ErrorResponse.ErrorMessage));
        }

        var res = new TokenValidationResponse
        {
            IsValid = result.IsSucessful,
            Message = "Token is valid"
        };


        return Ok(ApiResponse<TokenValidationResponse>.SuccessResponse(res, "Successful"));

    }



}
