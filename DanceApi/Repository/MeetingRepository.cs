using DanceApi.Data;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DanceApi.Repository
{
    public class MeetingRepository : IMeetingRepository
    {
        private readonly AppDbContext _context;

        public MeetingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Meeting>> GetAllMeetingsAsync()
        {
            return await _context.Meetings
                .Include(m => m.Location)
                .Include(m => m.TypeOfMeeting)
                .Include(m => m.Instructor)
                .ToListAsync();
        }
        public async Task<List<Meeting>> GetMeetingsByInstructorIdAsync(string instructorId)
        {
            return await _context.Meetings
                .Where(m => m.InstructorId == instructorId)
                .Include(m => m.Location)  
                .Include(m => m.TypeOfMeeting)  
                .Include(m => m.Instructor)  
                .ToListAsync();
        }
        public async Task<Meeting?> GetMeetingByIdAsync(int id)
        {
            return await _context.Meetings
                .Include(m => m.Location)
                .Include(m => m.TypeOfMeeting)
                .Include(m => m.Instructor)
                .Include(m => m.MeetingParticipants)
                .ThenInclude(mp => mp.User)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
        
        public async Task<IEnumerable<Meeting>> GetUpcomingMeetingsByUserIdAsync(string userId)
        {
            var currentDate = DateTime.UtcNow;

            return await _context.Meetings
                .Where(m => m.MeetingParticipants.Any(p => p.UserId == userId) && m.Date > currentDate) 
                .OrderByDescending(m => m.Date) 
                .Include(m => m.Instructor)  
                .Include(m => m.Location)    
                .Include(m => m.TypeOfMeeting)
                .ToListAsync();
        }

        public async Task<Meeting> CreateMeetingAsync(Meeting meeting, string userId)
        {
            meeting.CreatedById = userId;
            meeting.CreatedDate = DateTime.Now;
            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();
            return meeting;
        }
        
        public async Task<bool> UpdateMeetingAsync(Meeting meeting, string userId)
        {
            try
            {
                meeting.UpdatedById = userId;
                meeting.UpdatedDate = DateTime.UtcNow;
                _context.Meetings.Update(meeting);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> SoftDeleteMeetingAsync(int id, string userId)
        {
            var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.Id == id);

            if (meeting == null)
            {
                return false; // Meeting not found
            }

            meeting.IsDeleted = true; // Soft delete flag
            meeting.UpdatedById = userId;
            meeting.UpdatedDate = DateTime.UtcNow;

            _context.Meetings.Update(meeting);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> AddParticipantToMeetingAsync(int meetingId, string userId)
        {
            var meeting = await _context.Meetings.Include(m => m.MeetingParticipants).FirstOrDefaultAsync(m => m.Id == meetingId);

            if (meeting == null || meeting.MeetingParticipants.Any(mp => mp.UserId == userId))
            {
                return false; 
            }

            meeting.MeetingParticipants.Add(new MeetingParticipant
            {
                MeetingId = meetingId,
                UserId = userId
            });
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> RemoveParticipantFromMeetingAsync(int meetingId, string userId)
        {
            var participant = await _context.MeetingParticipants
                .FirstOrDefaultAsync(mp => mp.MeetingId == meetingId && mp.UserId == userId);

            if (participant == null)
            {
                return false; // Participant not found
            }

            _context.MeetingParticipants.Remove(participant);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<IEnumerable<Meeting>> GetMeetingsByUserIdAsync(string userId)
        {
            return await _context.Meetings
                .Where(m => m.MeetingParticipants.Any(p => p.UserId == userId))
                .Include(m => m.Instructor)  // Eager load Instructor
                .Include(m => m.Location)    // Eager load Location if needed
                .Include(m => m.TypeOfMeeting)
                .ToListAsync();
        }
    }
}