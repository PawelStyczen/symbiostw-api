using DanceApi.Model;

namespace DanceApi.Interface;

public interface IAuditLogService
{
    Task WriteAsync(AuditWriteRequest request, CancellationToken cancellationToken = default);
}
