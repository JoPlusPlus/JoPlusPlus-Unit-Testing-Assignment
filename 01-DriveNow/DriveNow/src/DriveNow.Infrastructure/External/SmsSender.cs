namespace DriveNow.Infrastructure.External;

public class SmsSender
{
    readonly int _delayMilliseconds;

    public SmsSender(int delayMilliseconds = 500)
    {
        _delayMilliseconds = delayMilliseconds;
    }
    public void Send(string phone, string text)
    {
        Thread.Sleep(_delayMilliseconds);
        Console.WriteLine($"[SMS to {phone}] {text}");
    }
}
