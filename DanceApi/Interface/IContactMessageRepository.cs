using DanceApi.Model;

namespace DanceApi.Interface;

public interface IContactMessageRepository
{
    Task<bool> CreateContactMessageAsync(ContactMessage message);
    Task<IEnumerable<ContactMessage>> GetAllMessagesAsync(bool includeRead = false);
    Task<ContactMessage?> GetMessageByIdAsync(int id);
    Task<bool> DeleteMessageAsync(int id);
    Task<bool> MarkMessageAsRepliedAsync(int id);
    Task<bool> MarkMessageAsReadAsync(int messageId);


}