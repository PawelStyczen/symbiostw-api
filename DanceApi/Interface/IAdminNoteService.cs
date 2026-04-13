using DanceApi.Dto;
using DanceApi.Model;

namespace DanceApi.Interface;

public interface IAdminNoteService
{
    Task<List<AdminNoteReadDto>> GetNotesForTargetAsync(
        AdminNoteTargetType targetType,
        string targetId,
        string adminUserId);
}
