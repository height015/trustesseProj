using System;

namespace trustesseApp.Service.Contracts;

public interface IEmailApiClientService
{

    Task<TResponse> SendEmailAsync<TRequest, TResponse>(string endpoint, TRequest payload);

}
