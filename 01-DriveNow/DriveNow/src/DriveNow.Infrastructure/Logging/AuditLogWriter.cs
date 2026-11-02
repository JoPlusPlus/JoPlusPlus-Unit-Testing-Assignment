using DriveNow.Domain.Abstractions;

namespace DriveNow.Infrastructure.Logging;

public class AuditLogWriter : IAuditLog
{
    public void Write(string message) => AuditLog.Write(message);
}
