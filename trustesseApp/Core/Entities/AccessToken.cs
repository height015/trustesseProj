using System;
using System.ComponentModel.DataAnnotations;

namespace trustesseApp.Core.Entities;

public class AccessToken
{
    public Guid Id { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Token is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Token must be 6 characters")]
    public string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public AppUser User { get; set; }
    public string UserId { get; set; }

}