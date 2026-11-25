using System.Text.Json.Serialization;
using Bugtrack.Api.Enums;
using Microsoft.AspNetCore.Identity;

namespace Bugtrack.Api.Classes;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";

    [JsonIgnore]
    public string PasswordHash { get; set; } = "";

    public SystemRole SystemRole { get; set; } = SystemRole.User;

    public void ChangeName(string name)
    {
        Name = name;
    }

    public void ChangePassword(string password)
    {
        var passwordHasher = new PasswordHasher<User>();
        PasswordHash = passwordHasher.HashPassword(this, password);
    }

    public void ChangeSystemRole(SystemRole role)
    {
        SystemRole = role;
    }
}