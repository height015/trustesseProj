using System;

namespace trustesseApp.Core.Infrastructure;

public class AppMailSettings
{
    public string BaseUrl { get; set; }
    public string ContentType { get; set; }
    public string From { get; set; } 
    public string SendMailToken { get; set; }
}
