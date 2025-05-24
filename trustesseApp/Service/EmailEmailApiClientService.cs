using System;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using trustesseApp.Core.Infrastructure;
using trustesseApp.Service.Contracts;

namespace trustesseApp.Service;


public class EmailEmailApiClientService : IEmailApiClientService
{
    private readonly HttpClient _httpClient;
    private readonly AppMailSettings _apiSettings;

    public EmailEmailApiClientService(HttpClient httpClient, IOptions<AppMailSettings> apiOptions)
    {
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        _httpClient = httpClient;
        _apiSettings = apiOptions.Value;
        _httpClient.BaseAddress = new Uri(_apiSettings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

          _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiSettings.SendMailToken);
  
    }

    public async Task<TResponse> SendEmailAsync<TRequest, TResponse>(string endpoint, TRequest payload)
    {
        
        string jsonContent = JsonConvert.SerializeObject(payload);

        StringContent obj = new StringContent(jsonContent, 
            Encoding.UTF8,
            "application/json"
            );

        var response = await _httpClient.PostAsync(endpoint, obj);

        var json = await response.Content.ReadAsStringAsync();



        return JsonConvert.DeserializeObject<TResponse>(json);
    }
    public async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"API call failed: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<T>(json);

        //return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
