using System;
using System.ComponentModel.DataAnnotations;
using trustesseApp.Core;

namespace trustesseApp.Models;


public class UserVM
{
    public string UserId { get; set; }
    public string FirstName { get; set; }
    public string Surname { get; set; }
    public string EmailAddress { get; set; }
    public string UserName { get; set; }
    public string JoinDate { get; set; }
}


public class UserResponseVM
{
    public string UserId { get; set; }
    public string EmailAddress { get; set; }
    public string UserName { get; set; }
    public string Token { get; set; }
}
public class UserLoginVM
{
    [Required(ErrorMessage = "Email Address or User name is Required", AllowEmptyStrings = false)]
    public string EmailAddress { get; set; }
    [Required(ErrorMessage = "Password is Required", AllowEmptyStrings = false)]
    public string Password { get; set; }
}

public class UserRegVM
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "First Name is required")]
    [StringLength(80, MinimumLength = 4, ErrorMessage = "First Name must be between 4 and 80 characters")]
    [CheckName(ErrorMessage = "Please enter a valid name")]
    public string FirstName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Las Name is required")]
    [StringLength(80, MinimumLength = 4, ErrorMessage = "Last Name  must be between 4 and 80 characters")]
    [CheckName(ErrorMessage = "Please enter a valid name")]
    public string LastName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email Address is required")]
    [CheckEmail(ErrorMessage = "Please Enter a valid Email")]
    public string EmailAddress { get; set; }



    [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
    [StringLength(80, MinimumLength = 4, ErrorMessage = "Password must be between 4 and 80 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; }


}

public class UserRegExtVM
{
    public string Token { get; set; }
    public string Provider { get; set; }

}

public class UserRegistrationVM
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "First Name is required")]
    [StringLength(80, MinimumLength = 4, ErrorMessage = "First Name must be between 4 and 80 characters")]
    [CheckName(ErrorMessage = "Please enter a valid name")]
    public string FirstName { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name is required")]
    [StringLength(80, MinimumLength = 4, ErrorMessage = "Last Name must be between 4 and 80 characters")]
    [CheckName(ErrorMessage = "Please enter a valid name")]
    public string LastName { get; set; }


    [Required(AllowEmptyStrings = false, ErrorMessage = "Email Address is required")]
    [CheckEmail(ErrorMessage = "Please enter a valid Email")]
    public string EmailAddress { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }



}
public class UserRegistrationResVM
{
    public string FirstName { get; set; }
    public string Surname { get; set; }
    public string MiddleName { get; set; }
    public string EmailAddress { get; set; }

}
public class UserConfirmEmail
{
    [Required]
    public string Token { get; set; }
    [Required]
    public string Email { get; set; }
}

