﻿namespace TeacherIdentityServer.Models;

public class TeacherIdentityUser
{
    public Guid UserId { get; set; }
    public string? EmailAddress { get; set; }
    public string? Trn { get; set; }
}
