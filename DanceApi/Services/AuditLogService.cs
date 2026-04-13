using System.Text.Json;
using DanceApi.Data;
using DanceApi.Interface;
using DanceApi.Model;
using Microsoft.EntityFrameworkCore;

namespace DanceApi.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(AppDbContext context, ILogger<AuditLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task WriteAsync(AuditWriteRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.TargetId))
            throw new ArgumentException("TargetId is required for audit log entries.", nameof(request));

        if (request.Changes.Count == 0)
            return;

        try
        {
            var currentVersion = await _context.AuditLogs
                .AsNoTracking()
                .Where(log => log.TargetType == request.TargetType && log.TargetId == request.TargetId)
                .MaxAsync(log => (int?)log.Version, cancellationToken) ?? 0;

            var auditLog = new AuditLog
            {
                TargetType = request.TargetType,
                TargetId = request.TargetId,
                Version = currentVersion + 1,
                ActionType = request.ActionType,
                SourceType = request.SourceType,
                ActorUserId = request.Actor?.UserId,
                ActorDisplayName = request.Actor?.DisplayName,
                ActorIdentifier = request.Actor?.Identifier,
                ChangedAtUtc = DateTime.UtcNow,
                ChangesJson = JsonSerializer.Serialize(request.Changes),
                Reason = request.Reason
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to persist audit log. TargetType: {TargetType}, TargetId: {TargetId}, ActionType: {ActionType}",
                request.TargetType,
                request.TargetId,
                request.ActionType);
        }
    }
}
