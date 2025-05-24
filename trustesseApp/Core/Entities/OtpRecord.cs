using System;
using System.ComponentModel.DataAnnotations;

namespace trustesseApp.Core.Entities;

public class OtpRecord 
{
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required")]
    [CheckEmail(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; }
    public string PhoneNumber { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Otp Code is required")]
    [StringLength(6, MinimumLength = 5, ErrorMessage = "Otp Code must be between 5 and 6 characters")]
    public string OtpCode { get; set; }
    public DateTime ExpiryTime { get; set; }
    public bool IsUsed { get; set; }
}