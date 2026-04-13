namespace DanceApi.Dto;

public class GuestUserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsDeleted { get; set; }
    public bool AllowNewsletter { get; set; }
    public bool AllowSmsMarketing { get; set; }
    public bool IsPendingApproval { get; set; }
    public string? Bio { get; set; }
}

public class GuestUserCreateDto
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool AllowNewsletter { get; set; } = true;
    public bool AllowSmsMarketing { get; set; } = false;
    public string? Bio { get; set; }
}

public class GuestUserUpdateDto
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool AllowNewsletter { get; set; }
    public bool AllowSmsMarketing { get; set; }
    public string? Bio { get; set; }
}
