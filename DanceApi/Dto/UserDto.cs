using System.ComponentModel.DataAnnotations;

namespace DanceApi.Dto;

public class UserDto
{
    public class UpdateUserDto
    {

        public string? Name { get; set; }

    
        public string? Surname { get; set; }

        public string? PhoneNumber { get; set; }

        public bool? AllowNewsletter { get; set; }

        public bool? AllowSmsMarketing { get; set; }
        
    }
}

public class AdminUserDetailsDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool AllowNewsletter { get; set; }
    public bool AllowSmsMarketing { get; set; }
    public List<AdminNoteReadDto> Notes { get; set; } = new();
}

public class GuestUserDetailsDto : GuestUserDto
{
    public List<AdminNoteReadDto> Notes { get; set; } = new();
}
