using DriveNow.Domain.Abstractions;

namespace DriveNow.Infrastructure.External;

public class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
}
