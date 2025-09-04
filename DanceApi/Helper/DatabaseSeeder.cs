using DanceApi.Data;
using DanceApi.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public DatabaseSeeder(
        AppDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment env,
        IConfiguration config)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _env = env;
        _config = config;
    }

    public async Task SeedAsync()
    {
        // Always seed roles
        await SeedRolesAsync();

        Console.WriteLine("Running database seeding...");
        await SeedAdminAsync();
        await _context.SaveChangesAsync();
        Console.WriteLine("Seeding completed.");
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

    private async Task SeedAdminAsync()
    {
        // Read from config first, fallback to env vars (especially in production)
        var adminEmail = _config["Admin:Email"] ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = _config["Admin:Password"] ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            Console.WriteLine("Admin email or password not configured. Skipping admin creation.");
            return;
        }

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = "Admin",
                Surname = "Master",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                throw new Exception("Admin creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("✅ Admin user created.");
        }
        else
        {
            Console.WriteLine("ℹ️ Admin user already exists.");
        }
    }
}