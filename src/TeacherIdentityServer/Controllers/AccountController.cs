using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherIdentityServer.Models;
using TeacherIdentityServer.Models.Account;

namespace TeacherIdentityServer.Controllers;

[Route("account")]
public class AccountController : Controller
{
    [HttpGet("")]
    public IActionResult Index([FromQuery] string? returnUrl = null)
    {
        // TODO Check returnUrl
        HttpContext.Session.SetString(SessionKeys.ReturnUrl, returnUrl ?? string.Empty);

        return RedirectToAction("Email");
    }

    [HttpGet("email")]
    public IActionResult Email() => View();

    [HttpPost("email")]
    public IActionResult Email(EmailModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        HttpContext.Session.SetString(SessionKeys.Email, model.Email!);

        return RedirectToAction(nameof(EmailConfirmation));
    }

    [HttpGet("email-confirmation")]
    public IActionResult EmailConfirmation()
    {
        var model = new EmailConfirmationModel()
        {
            Email = HttpContext.Session.GetString(SessionKeys.Email)
        };

        return View(model);
    }

    [HttpPost("email-confirmation")]
    public async Task<IActionResult> EmailConfirmation(EmailConfirmationModel model, [FromServices] TeacherIdentityServerDbContext dbContext)
    {
        var email = HttpContext.Session.GetString(SessionKeys.Email);

        if (email is null)
        {
            return RedirectToAction(nameof(Email));
        }

        model.Email = email;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await dbContext.Users.Where(u => u.EmailAddress == model.Email).SingleOrDefaultAsync();
        if (user is null)
        {
            var userId = Guid.NewGuid();

            user = new TeacherIdentityUser()
            {
                UserId = userId,
                EmailAddress = model.Email
            };

            dbContext.Add(user);

            await dbContext.SaveChangesAsync();
        }

        var claims = new[]
        {
            new Claim("sub", user.UserId.ToString()),
            new Claim("email", user.EmailAddress!),
            new Claim("email_verified", "true")
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "email", nameType: "sub", roleType: null);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        var returnUrl = HttpContext.Session.GetString(SessionKeys.ReturnUrl) ?? "/";

        return Redirect(returnUrl);
    }
}
