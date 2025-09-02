using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DanceApi.Data;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Repository
{
public class TypeOfMeetingRepository : ITypeOfMeetingRepository
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper; 

    public TypeOfMeetingRepository(AppDbContext context, IMapper mapper) 
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TypeOfMeeting>> GetAllTypeOfMeetingsAsync()
    {
        return await _context.TypeOfMeetings
            .Where(t => !t.IsDeleted)
            .Include(t => t.CreatedBy) 
            .Include(t => t.UpdatedBy) 
            .ToListAsync();
    }

    public async Task<IEnumerable<TypeOfMeeting>> GetAllTypeOfMeetingsVisibleAsync()
    {
        return await _context.TypeOfMeetings
            .Where(t => !t.IsDeleted && t.IsVisible) 
            .Include(t => t.CreatedBy) 
            .Include(t => t.UpdatedBy) 
            .ToListAsync();
    }

    public async Task<TypeOfMeeting?> GetTypeOfMeetingByIdAsync(int id)
    {
        return await _context.TypeOfMeetings
            .Include(t => t.CreatedBy) 
            .Include(t => t.UpdatedBy) 
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<TypeOfMeeting> CreateTypeOfMeetingAsync(TypeOfMeeting typeOfMeeting, string userId)
    {
        typeOfMeeting.CreatedById = userId;
        typeOfMeeting.CreatedDate = DateTime.UtcNow;

        _context.TypeOfMeetings.Add(typeOfMeeting);
        await _context.SaveChangesAsync();

        return await _context.TypeOfMeetings
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == typeOfMeeting.Id);
    }

    public async Task<bool> UpdateTypeOfMeetingAsync(TypeOfMeeting typeOfMeeting, string userId)
    {
        var existingTypeOfMeeting = await _context.TypeOfMeetings.FindAsync(typeOfMeeting.Id);
        if (existingTypeOfMeeting == null || existingTypeOfMeeting.IsDeleted)
        {
            return false;
        }

        _mapper.Map(typeOfMeeting, existingTypeOfMeeting); 
        existingTypeOfMeeting.UpdatedById = userId;
        existingTypeOfMeeting.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteTypeOfMeetingAsync(int id, string userId)
    {
        var typeOfMeeting = await _context.TypeOfMeetings.FindAsync(id);
        if (typeOfMeeting == null || typeOfMeeting.IsDeleted)
        {
            return false;
        }

        typeOfMeeting.IsDeleted = true;
        typeOfMeeting.DeletedById = userId;
        typeOfMeeting.DeletedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}
}