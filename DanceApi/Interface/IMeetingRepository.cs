using DanceApi.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using DanceApi.Dto;

namespace DanceApi.Interface
{
    public interface IMeetingRepository
    {
        Task<IEnumerable<Meeting>> GetAllMeetingsAsync();
        Task<List<Meeting>> GetMeetingsByInstructorIdAsync(string instructorId);
        Task<Meeting?> GetMeetingByIdAsync(int id);
        
        Task<Meeting> CreateMeetingAsync(Meeting meeting, string userId);
        
        Task<bool> UpdateMeetingAsync(Meeting meeting, string userId);
        
        Task<bool> SoftDeleteMeetingAsync(int id, string userId);
        Task<bool> AddParticipantToMeetingAsync(int meetingId, string userId);
        Task<bool> RemoveParticipantFromMeetingAsync(int meetingId, string userId);
        Task<IEnumerable<Meeting>> GetMeetingsByUserIdAsync(string userId); 
        
        Task<IEnumerable<Meeting>> GetUpcomingMeetingsByUserIdAsync(string userId);

        Task<bool> SoftDeleteEventAsync(int meetingId, string userId);
        
        Task<int> CopyMeetingsMonthAsync(
            int sourceYear,
            int sourceMonth,
            int targetYear,
            int targetMonth,
            string userId
        );
        
        Task<int> CopyWeekAsync(int sourceYear, int sourceWeek, int targetYear, int targetWeek, string userId);
    }
}