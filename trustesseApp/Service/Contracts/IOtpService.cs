using System;
using trustesseApp.Core.Entities;
using trustesseApp.Infrastructure.Helpers;

namespace trustesseApp.Service.Contracts;

public interface IOtpService
{
    Task<DataResponseObj<OtpRecord>> GenerateOtp(string? userEmail = null, string? phoneNumber = null);
    Task<DataResponseObj<bool>> ValidateOtp(string userEmail, string? phoneNumber = null, string? otp = null);
}

