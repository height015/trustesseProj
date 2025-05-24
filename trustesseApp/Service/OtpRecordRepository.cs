using System;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using trustesseApp.Core.Entities;
using trustesseApp.Core.Repository;
using trustesseApp.Infrastructure.Helpers;
using trustesseApp.Service.Contracts;

namespace trustesseApp.Service;




public class OtpRecordRepository : IOtpService
{
    private readonly IRepository<OtpRecord> _otpRecordRepository;
    private readonly ResponseObj _errorObj;
    private readonly ILogger<OtpRecordRepository> _logger;

    public OtpRecordRepository(IRepository<OtpRecord> otpRecordRepository, ILogger<OtpRecordRepository> logger)
    {
        _otpRecordRepository = otpRecordRepository;
        _errorObj = new ResponseObj();
        _logger = logger;
    }

    public async Task<DataResponseObj<OtpRecord>> GenerateOtp(string userEmail = null, string phoneNumber = null)
    {
        var response = new DataResponseObj<OtpRecord>
        {
            IsSucessful = false,
            Entity = new OtpRecord(),
            ErrorResponse = new ResponseObj()
        };
        try
        {
            var otpCode = new Random().Next(100000, 999999).ToString();
            var expiryTime = DateTime.UtcNow.AddMinutes(5);

            var query = _otpRecordRepository.Table;
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                query = query.Where(u => u.PhoneNumber == phoneNumber &&
            !u.IsUsed);
            }
            if (!string.IsNullOrEmpty(userEmail))
            {
                query = query.Where(u => u.Email == userEmail &&
            !u.IsUsed);
            }

            var existingOtp = await query.ToListAsync();
            if (existingOtp.Any())
            {
                await _otpRecordRepository.Delete(existingOtp);
            }

            var otpRecord = new OtpRecord
            {
                Email = userEmail,
                PhoneNumber = !string.IsNullOrEmpty(phoneNumber) ? phoneNumber : string.Empty,
                OtpCode = otpCode,
                ExpiryTime = expiryTime,
                IsUsed = false
            };

            var retVal = await _otpRecordRepository.Insert(otpRecord);

            if (retVal.Id > 0)
            {
                response.IsSucessful = true;
                response.Entity = retVal;
                return await Task.FromResult(response);
            }

            response.IsSucessful = false;
            _errorObj.ErrorMessage = "Error Occurred! Could not generate OTP";
            _errorObj.TechnicalMessage = "Otp was not generated because " +
                "Identity response is less than or equals zero";
            response.Entity = null;
            response.ErrorResponse = _errorObj;
            return await Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _errorObj.ErrorMessage = "Unknown Error Occurred!";
            _errorObj.TechnicalMessage = ex.GetBaseException().Message;
            response.ErrorResponse = _errorObj;
            _logger.LogError(JsonConvert.SerializeObject(response.ErrorResponse));
            return await Task.FromResult(response);
        }

    }
    public async Task<DataResponseObj<bool>> ValidateOtp(string userEmail, string phoneNumber = null, string otp = null)
    {
        var response = new DataResponseObj<bool>
        {
            IsSucessful = false,
            Entity = new bool(),
            ErrorResponse = new ResponseObj()
        };
        var query = _otpRecordRepository.Table;
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            query = query.Where(u => u.PhoneNumber == phoneNumber);
        }
        query = query.Where(o => o.Email == userEmail && o.OtpCode == otp
        && !o.IsUsed);

        var otpRecords = await query.ToListAsync();
        var otpRecord = new OtpRecord();
        if (otpRecords.Any())
        {
            otpRecord = otpRecords[0];
        }
        else
        {
            response.IsSucessful = false;
            _errorObj.ErrorMessage = "Error Occurred! Could not fetch OTP";
            _errorObj.TechnicalMessage = "Otp was not found in the database";
            response.Entity = false;
            response.ErrorResponse = _errorObj;
            return response;
        }
        if (otpRecord.ExpiryTime < DateTime.UtcNow)
        {
            response.IsSucessful = false;
            _errorObj.ErrorMessage = "Error Occurred! OTP Expired, Generate a new OTP";
            _errorObj.TechnicalMessage = "Otp has expired, Please generate a new OTP";
            response.Entity = false;
            response.ErrorResponse = _errorObj;
            return response;
        }

        otpRecord.IsUsed = true;
        var retVal = await _otpRecordRepository.Update(otpRecord);
        if (retVal.Id <= 0)
        {
            response.IsSucessful = false;
            _errorObj.ErrorMessage = "Error Occurred! OTP Could not be updated";
            _errorObj.TechnicalMessage = "OTP record could not be updated";
            response.Entity = false;
            response.ErrorResponse = _errorObj;
            return response;
        }
        response.IsSucessful = true;
        response.Entity = true;
        return response;
    }
}