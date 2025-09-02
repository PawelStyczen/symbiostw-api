using DanceApi.Data;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Repository;

public class ContactMessageRepository : IContactMessageRepository
{
    private readonly AppDbContext _context;

    public ContactMessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateContactMessageAsync(ContactMessage message)
    {
        await _context.ContactMessages.AddAsync(message);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<ContactMessage>> GetAllMessagesAsync(bool includeRead = false)
    {
        return await _context.ContactMessages
            .Where(m => includeRead || !m.IsRead) // 🔥 Filter unread messages
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync();
    }

    public async Task<ContactMessage?> GetMessageByIdAsync(int id)
    {
        return await _context.ContactMessages.FindAsync(id);
    }

    public async Task<bool> DeleteMessageAsync(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message == null) return false;

        _context.ContactMessages.Remove(message);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> MarkMessageAsRepliedAsync(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message == null) return false;

        message.IsReplied = true;
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> MarkMessageAsReadAsync(int messageId)
    {
        var message = await _context.ContactMessages.FindAsync(messageId);
        if (message == null) return false;

        message.IsRead = true; 
        return await _context.SaveChangesAsync() > 0;
    }
}