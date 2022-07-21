using System.ComponentModel.DataAnnotations;

namespace TeacherIdentityServer.Models.Account;

public class NameModel
{
    [Display(Name = "Your first name")]
    [Required(ErrorMessage = "Enter your first name")]
    public string? FirstName { get; set; }

    [Display(Name = "Your last name")]
    [Required(ErrorMessage = "Enter your last name")]
    public string? LastName { get; set; }
}
