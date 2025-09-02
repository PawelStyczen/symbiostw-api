using DanceApi.Data;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Repository;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    public UserRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<User> GetUserByIdAsync(string userId)
    {
        return await _userManager.Users
            .Include(u => u.InstructorProfile)  
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<IdentityResult> CreateUserAsync(User user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<bool> AddUserToRoleAsync(User user, string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }
        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }
    
    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
    {
        var usersInRole = await _context.Users
            .Where(u => _context.UserRoles
                .Any(ur => ur.UserId == u.Id && _context.Roles
                    .Any(r => r.Id == ur.RoleId && r.Name == roleName)))
            .Include(u => u.InstructorProfile)
            .ToListAsync();

        return usersInRole;
    }
    
    
    public void AddInstructorProfile(InstructorProfile profile)
    {
        _context.InstructorProfiles.Add(profile);
    }
    
    public void RemoveInstructorProfile(InstructorProfile profile)
    {
        _context.InstructorProfiles.Remove(profile);
    }

    public async Task<IdentityResult> DeleteUserAsync(User user)
    {
        return await _userManager.DeleteAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    
}