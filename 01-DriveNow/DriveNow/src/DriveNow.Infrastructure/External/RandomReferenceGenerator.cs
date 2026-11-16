using DriveNow.Domain.Abstractions;

namespace DriveNow.Infrastructure.External;

public class RandomReferenceGenerator : IReferenceGenerator
{
    const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    readonly Random _random;

    public RandomReferenceGenerator(Random random)
    {
        _random = random;
    }

    public string Next(int length)
    {
        var chars = new char[length];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = Alphabet[_random.Next(Alphabet.Length)];

        return new string(chars);
    }
}
