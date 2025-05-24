using System;
using trustesseApp.Core.Entities;
using trustesseApp.Models;

namespace trustesseApp.Core.Infrastructure.Mappers;


public static class MappingExtensions 
    {


    public static UserVM ToDTO(this AppUser appUser)
    {
        return new UserVM
        {
            UserId = appUser.Id,
            FirstName = appUser.FirstName,
            Surname = appUser.LastName,
            EmailAddress = appUser.Email ?? "",
            UserName = appUser.UserName,
            JoinDate = appUser.DateCreated.ToString("yyyy-MM-dd"),
        };
    }

    public static AppUser ToEntity(this UserRegVM appUser)
    {
        return new AppUser
        {
            FirstName = appUser.FirstName,
            LastName = appUser.LastName,
            Email = appUser.EmailAddress,
            UserName = appUser.EmailAddress,
            DateCreated = DateTime.UtcNow,
            LastActivityDateUtc = DateTime.UtcNow,
            NormalizedEmail = appUser.EmailAddress.ToUpper().Trim(),
            NormalizedUserName = appUser.EmailAddress.ToUpper().Trim(),

        };
    }

    public static UserRegVM AsDTO(this AppUser appUser)
    {
       return new UserRegVM
       {
           FirstName = appUser.FirstName,
           LastName = appUser.LastName,
           EmailAddress = appUser.Email,
           
       };
    }

}
