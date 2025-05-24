using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace trustesseApp.Core.Entities;


public class AppUser : IdentityUser
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "First Name is required")]
    [StringLength(80, MinimumLength = 4, ErrorMessage = "First Name must be between 4 and 80 characters")]
    [CheckName(ErrorMessage = "Please enter a valid name")]
    public string FirstName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name is required")]
    [StringLength(80, MinimumLength = 4, ErrorMessage = "Last Name must be between 4 and 80 characters")]
    [CheckName(ErrorMessage = "Please enter a valid name")]
    public string LastName { get; set; }

    public DateTime DateCreated { get; set; }
    public DateTime LastActivityDateUtc { get; set; }

    public ICollection<AccessToken> AccessTokens { get; set; }

}