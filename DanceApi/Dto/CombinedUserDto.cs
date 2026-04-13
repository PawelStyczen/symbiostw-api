namespace DanceApi.Dto;

public class CombinedUserDto
{
    public string Id { get; set; }
    public bool IsGuest { get; set; }
    public string UserType { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsDeleted { get; set; }
    public bool? AllowNewsletter { get; set; }
    public bool? AllowSmsMarketing { get; set; }
    public string? Bio { get; set; }
}
