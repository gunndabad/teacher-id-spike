using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherIdentityServer.Models;
using TeacherIdentityServer.Models.Account;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace TeacherIdentityServer.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly TeacherIdentityServerDbContext _dbContext;

    public AccountController(TeacherIdentityServerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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
    public async Task<IActionResult> EmailConfirmation(EmailConfirmationModel model)
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

        var user = await _dbContext.Users.Where(u => u.EmailAddress == model.Email).SingleOrDefaultAsync();
        if (user is not null)
        {
            return await SignInUser(user);
        }
        else
        {
            return RedirectToAction(nameof(Name));
        }
    }

    [HttpGet("name")]
    public IActionResult Name()
    {
        var model = new NameModel();
        return View(model);
    }

    [HttpPost("name")]
    public async Task<IActionResult> Name(NameModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        HttpContext.Session.SetString(SessionKeys.FirstName, model.FirstName!);
        HttpContext.Session.SetString(SessionKeys.LastName, model.LastName!);

        var user = await RegisterUser();

        return await SignInUser(user);
    }

    private async Task<TeacherIdentityUser> RegisterUser()
    {
        var userId = Guid.NewGuid();
        var email = HttpContext.Session.GetString(SessionKeys.Email);
        var firstName = HttpContext.Session.GetString(SessionKeys.FirstName);
        var lastName = HttpContext.Session.GetString(SessionKeys.LastName);

        var user = new TeacherIdentityUser()
        {
            UserId = userId,
            EmailAddress = email,
            FirstName = firstName,
            LastName = lastName
        };

        _dbContext.Add(user);

        await _dbContext.SaveChangesAsync();

        return user;
    }

    private async Task<IActionResult> SignInUser(TeacherIdentityUser user)
    {
        var claims = new[]
        {
            new Claim(Claims.Subject, user.UserId.ToString()),
            new Claim(Claims.Email, user.EmailAddress!),
            new Claim(Claims.EmailVerified, "true"),
            new Claim(Claims.Name, user.FirstName + " " + user.LastName),
            new Claim(Claims.GivenName, user.FirstName!),
            new Claim(Claims.FamilyName, user.LastName!),
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "email", nameType: Claims.Name, roleType: null);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        var returnUrl = HttpContext.Session.GetString(SessionKeys.ReturnUrl) ?? "/";

        return Redirect(returnUrl);
    }
}
