using DanceApi.Model;
using Microsoft.AspNetCore.Identity;

namespace DanceApi.Interface;


public interface IUserRepository
{
    Task<User> GetUserByIdAsync(string userId);
    Task<User> GetUserByEmailAsync(string email);
    Task<IdentityResult> CreateUserAsync(User user, string password);
    Task<bool> AddUserToRoleAsync(User user, string roleName);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName); 
    
    void AddInstructorProfile(InstructorProfile profile); 
    Task SaveChangesAsync();
    
    void RemoveInstructorProfile(InstructorProfile profile); 
    Task<IdentityResult> DeleteUserAsync(User user); 
}