using System.ComponentModel.DataAnnotations;

namespace DanceApi.Model;

public class RegisterModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public bool AllowNewsletter { get; set; } = true;

    public bool AllowSmsMarketing { get; set; } = false;

    public string? ImageUrl { get; set; }
}
