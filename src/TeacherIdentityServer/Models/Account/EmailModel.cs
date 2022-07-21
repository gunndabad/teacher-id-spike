using System.ComponentModel.DataAnnotations;

namespace TeacherIdentityServer.Models.Account;

public class EmailModel
{
    [Display(Name = "Your email address")]
    [Required(ErrorMessage = "Enter your email address")]
    public string? Email { get; set; }
}
