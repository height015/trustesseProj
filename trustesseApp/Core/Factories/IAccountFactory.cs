using System;
using trustesseApp.Infrastructure.Helpers;
using trustesseApp.Models;

namespace trustesseApp.Core.Factories;

public interface IAccountFactory{

    Task<DataResponseObj<UserRegVM>> UserRegFac(UserRegVM userRegVM);
    Task<DataResponseObj<string>> ConfirmAccountFactory(UserConfirmEmail userConfirmEmail);
    Task<DataResponseObj<UserResponseVM>> AccountLoginFactory(UserLoginVM userLoginVM);
    Task<DataResponseObj<UserVM>> AccountDetailsFactory(string userId);

}
