using DanceApi.Data;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DanceApi.Helper;

namespace DanceApi.Repository
{
    public class MeetingRepository : IMeetingRepository
    {
        private readonly AppDbContext _context;

        public MeetingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CopyMeetingsMonthAsync(
            int sourceYear,
            int sourceMonth,
            int targetYear,
            int targetMonth,
            string userId)
        {
            // Jeśli w DB trzymasz UTC — użyj DateTimeKind.Utc konsekwentnie:
            var sourceStart = new DateTime(sourceYear, sourceMonth, 1, 0, 0, 0, DateTimeKind.Utc);
            var sourceEnd = sourceStart.AddMonths(1);

            var targetStart = new DateTime(targetYear, targetMonth, 1, 0, 0, 0, DateTimeKind.Utc);
            var targetEnd = targetStart.AddMonths(1);

            // "Grid start" = Monday tygodnia, w którym jest 1 dzień miesiąca
            var sourceGridStart = DateHelpers.GetMondayOfWeek(sourceStart);
            var targetGridStart = DateHelpers.GetMondayOfWeek(targetStart);

            var meetings = await _context.Meetings
                .Where(m => !m.IsDeleted && m.Date >= sourceStart && m.Date < sourceEnd)
                .ToListAsync();

            int copied = 0;

            foreach (var m in meetings)
            {
                // Offset w minutach od gridStart, żeby zachować też godziny
                var offset = m.Date - sourceGridStart;
                var targetDate = targetGridStart.Add(offset);

                // ✅ WAŻNE: kopiuj tylko te, które lądują w target miesiącu
                if (targetDate < targetStart || targetDate >= targetEnd)
                    continue;

                var clone = new Meeting
                {
                    Date = targetDate,
                    Duration = m.Duration,
                    LocationId = m.LocationId,
                    InstructorId = m.InstructorId,
                    TypeOfMeetingId = m.TypeOfMeetingId,
                    Price = m.Price,
                    Level = m.Level,
                    IsVisible = m.IsVisible,
                    IsHighlighted = false,
                    CreatedById = userId,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Meetings.Add(clone);
                copied++;
            }

            await _context.SaveChangesAsync();
            return copied;
        }
        
       

        public async Task<int> CopyWeekAsync(int sourceYear, int sourceWeek, int targetYear, int targetWeek, string userId)
        {
            var sourceStart = DateHelpers.GetMondayOfIsoWeek(sourceYear, sourceWeek);
            var sourceEnd = sourceStart.AddDays(7);

            var targetStart = DateHelpers.GetMondayOfIsoWeek(targetYear, targetWeek);

            // delta tygodni (najczęściej +7 dni, ale działa też na dowolny tydzień)
            var delta = targetStart - sourceStart;

            var sourceMeetings = await _context.Meetings
                .Where(m => !m.IsDeleted && m.Date >= sourceStart && m.Date < sourceEnd)
                .ToListAsync();

            if (!sourceMeetings.Any())
                return 0;

            var copies = sourceMeetings.Select(m => new Meeting
            {
                Date = m.Date.Add(delta),              // ✅ zachowuje godzinę 1:1
                Duration = m.Duration,
                LocationId = m.LocationId,
                InstructorId = m.InstructorId,
                TypeOfMeetingId = m.TypeOfMeetingId,
                Price = m.Price,
                Level = m.Level,
                IsHighlighted = false,
                IsVisible = m.IsVisible,
                CreatedById = userId,
                CreatedDate = DateTime.UtcNow
            }).ToList();

            _context.Meetings.AddRange(copies);
            await _context.SaveChangesAsync();

            return copies.Count;
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
        
        
        
        public async Task<bool> SoftDeleteEventAsync(int meetingId, string userId)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            var meeting = await _context.Meetings
                .Include(m => m.TypeOfMeeting)
                .FirstOrDefaultAsync(m => m.Id == meetingId && !m.IsDeleted);

            if (meeting == null || meeting.TypeOfMeeting == null)
                return false;

            if (!meeting.TypeOfMeeting.IsEvent)
                return false;

            // 1) soft delete meeting
            meeting.IsDeleted = true;
            meeting.DeletedById = userId;
            meeting.DeletedDate = DateTime.UtcNow;

            // 2) soft delete type
            var type = meeting.TypeOfMeeting;
            if (!type.IsDeleted)
            {
                type.IsDeleted = true;
                type.DeletedById = userId;
                type.DeletedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await tx.CommitAsync();
            return true;
        }
        
        
    }
    
}