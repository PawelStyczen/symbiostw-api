using DanceApi.Model;

namespace DanceApi.Interface;

public interface IPayURepository
{
    Task<string> GetAccessTokenAsync();
    Task<string> CreateOrderAsync(PayUOrder order);
}