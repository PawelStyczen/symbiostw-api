using DanceApi.Data;
using DanceApi.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IWebHostEnvironment _env;

    public DatabaseSeeder(
        AppDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _env = env;
    }

    public async Task SeedAsync()
    {
        // Always seed roles
        await SeedRolesAsync();

        // Only seed users in non-production
        if (!_env.IsProduction())
        {
            Console.WriteLine("Running development seeding...");
            var instructorId = await SeedUsersAsync();
            await _context.SaveChangesAsync();
            Console.WriteLine("Development seeding completed.");
        }
        else
        {
            Console.WriteLine("Production environment detected. Skipping user seeding.");
        }
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = { "User", "Instructor", "Admin" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private async Task<string> SeedUsersAsync()
    {
        string instructorId = null;

        // Seed regular user
        if (await _userManager.FindByEmailAsync("user@example.com") == null)
        {
            var user = new User
            {
                UserName = "user@example.com",
                Email = "user@example.com",
                Name = "Test",
                Surname = "User"
            };
            await _userManager.CreateAsync(user, "Password123!");
            await _userManager.AddToRoleAsync(user, "User");
        }

        // Seed instructor
        var instructor = await _userManager.FindByEmailAsync("instructor@example.com");
        if (instructor == null)
        {
            instructor = new User
            {
                UserName = "instructor@example.com",
                Email = "instructor@example.com",
                Name = "Instructor",
                Surname = "Example"
            };
            await _userManager.CreateAsync(instructor, "Password123!");
            await _userManager.AddToRoleAsync(instructor, "Instructor");
        }

        instructorId = instructor.Id;

        // Seed admin
        if (await _userManager.FindByEmailAsync("admin@example.com") == null)
        {
            var admin = new User
            {
                UserName = "admin@symbiostw.pl",
                Email = "admin@symbiostw.pl",
                Name = "Admin",
                Surname = "Master"
            };
            await _userManager.CreateAsync(admin, "AdminPassword123!");
            await _userManager.AddToRoleAsync(admin, "Admin");
        }

        return instructorId;
    }
    
    
}