namespace DriveNow.Domain.Abstractions;

public interface IReferenceGenerator
{
    string Next(int length);
}
