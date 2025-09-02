using System.ComponentModel.DataAnnotations;

namespace DanceApi.Dto;

public class UserDto
{
    public class UpdateUserDto
    {

        public string? Name { get; set; }

    
        public string? Surname { get; set; }
        
    }
}