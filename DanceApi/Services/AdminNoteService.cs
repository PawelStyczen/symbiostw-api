using AutoMapper;
using DanceApi.Data;
using DanceApi.Dto;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Services;

public class AdminNoteService : IAdminNoteService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public AdminNoteService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AdminNoteReadDto>> GetNotesForTargetAsync(
        AdminNoteTargetType targetType,
        string targetId,
        string adminUserId)
    {
        var notes = await _context.AdminNotes
            .Include(note => note.CreatedBy)
            .Include(note => note.UpdatedBy)
            .Where(note =>
                note.CreatedById == adminUserId &&
                note.TargetType == targetType &&
                note.TargetId == targetId)
            .OrderByDescending(note => note.UpdatedDate ?? note.CreatedDate)
            .ToListAsync();

        return _mapper.Map<List<AdminNoteReadDto>>(notes);
    }
}
