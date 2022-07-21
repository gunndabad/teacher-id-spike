﻿using Client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers;

public class HomeController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View();

    [Authorize]
    [HttpGet("profile")]
    public IActionResult Profile()
    {
        var model = new ProfileModel()
        {
            Email = User.FindFirst("email")?.Value,
            UserId = User.FindFirst("sub")?.Value
        };

        return View(model);
    }
}