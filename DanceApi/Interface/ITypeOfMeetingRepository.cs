using System.Collections.Generic;
using System.Threading.Tasks;
using DanceApi.Model;

namespace DanceApi.Interface
{
    public interface ITypeOfMeetingRepository
    {
        Task<IEnumerable<TypeOfMeeting>> GetAllTypeOfMeetingsAsync();
        Task<IEnumerable<TypeOfMeeting>> GetAllTypeOfMeetingsVisibleAsync();
        Task<TypeOfMeeting> GetTypeOfMeetingByIdAsync(int id);
        Task<TypeOfMeeting> CreateTypeOfMeetingAsync(TypeOfMeeting typeOfMeeting, string userId);
        Task<bool> UpdateTypeOfMeetingAsync(TypeOfMeeting typeOfMeeting, string userId);
        Task<bool> SoftDeleteTypeOfMeetingAsync(int id, string userId);
    }
}