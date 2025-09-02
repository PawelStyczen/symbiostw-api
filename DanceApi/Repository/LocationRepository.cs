using DanceApi.Data;
using DanceApi.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DanceApi.Interface;



public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _context;

    public LocationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Location>> GetAllLocationsAsync()
    {
        return await _context.Locations
            .Where(l => !l.IsDeleted)
            .Include(l => l.CreatedBy)
            .Include(l => l.UpdatedBy) 
            .ToListAsync();
    }

    public async Task<Location?> GetLocationByIdAsync(int id)
    {
        return await _context.Locations
            .Include(l => l.CreatedBy) 
            .Include(l => l.UpdatedBy) 
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
    }

    public async Task<Location> CreateLocationAsync(Location location, string userId)
    {
        location.CreatedById = userId;
        location.CreatedDate = DateTime.UtcNow;

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return await _context.Locations
            .Include(l => l.CreatedBy) 
            .FirstOrDefaultAsync(l => l.Id == location.Id);
    }

    public async Task<bool> UpdateLocationAsync(Location location, string userId)
    {
        var existingLocation = await _context.Locations.FindAsync(location.Id);
        if (existingLocation == null || existingLocation.IsDeleted)
        {
            return false;
        }

        existingLocation.Name = location.Name;
        existingLocation.City = location.City;
        existingLocation.Street = location.Street;
        existingLocation.Description = location.Description;
        existingLocation.ImageUrl = location.ImageUrl;
        existingLocation.UpdatedDate = DateTime.UtcNow;
        existingLocation.UpdatedById = userId;

        _context.Locations.Update(existingLocation);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteLocationAsync(int id, string userId)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null || location.IsDeleted)
        {
            return false;
        }

        location.IsDeleted = true;
        location.DeletedDate = DateTime.UtcNow;
        location.DeletedById = userId;

        _context.Locations.Update(location);
        await _context.SaveChangesAsync();
        return true;
    }
}