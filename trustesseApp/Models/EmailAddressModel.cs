using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace trustesseApp.Models;

public class EmailAddressModel
{
    public string Email { get; set; }
    public string Name { get; set; }
}

public class EmailAddress
{
    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
}

public class EmailRecipient
{
    [JsonProperty("email")]
    public string Email { get; set; }
}

public class MailtrapEmailRequest
{
    [JsonProperty("from")]
    public EmailAddress From { get; set; }

    [JsonProperty("to")]
    public List<EmailRecipient> To { get; set; }

    [JsonProperty("subject")]
    public string Subject { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("html")]
    public string Html { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; }
}

public class MailtrapResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("message_ids")]
    public List<string> MessageIds { get; set; }

    [JsonProperty("errors")]
    public List<string>? Errors { get; set; }

}

public class TokenGenerationRequest
{
    /// <summary>
    /// Optional lifetime in hours (max 72 hours/3 days)
    /// </summary>
    /// <example>24</example>
    public int? LifetimeInHours { get; set; }
}

public class TokenGenerationResponse
{
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class TokenValidationRequest
{
   
    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Token { get; set; }
}

public class TokenValidationResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; }
}