using DanceApi.Model;

namespace DanceApi.Interface;

public interface ILocationRepository
{
    Task<IEnumerable<Location>> GetAllLocationsAsync();
    Task<Location> GetLocationByIdAsync(int id);
    Task<Location> CreateLocationAsync(Location location, string userId);
    Task<bool> UpdateLocationAsync(Location location, string userId);
    Task<bool> SoftDeleteLocationAsync(int id, string userId);
}