using System.ComponentModel.DataAnnotations;

namespace TeacherIdentityServer.Models.Account;

public class EmailConfirmationModel
{
    public string? Email { get; set; }

    [Display(Name = "Enter your code")]
    [Required(ErrorMessage = "Enter your confirmation code")]
    public string? Code { get; set; }
}
