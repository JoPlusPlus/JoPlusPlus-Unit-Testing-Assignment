namespace DriveNow.UnitTests;

using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;

public static class FixtureFactory
{
    public static IFixture Create(params ICustomization[] extras)
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });

        fixture.Customize(new ConstructorCustomization(typeof(BookingService), new GreedyConstructorQuery()));

        foreach (var extra in extras)
            fixture.Customize(extra);
        return fixture;
    }
}

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute() : base(() => FixtureFactory.Create()) { }
}

public class ValidBookingWorldCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customize<Customer>(c => c
            .With(x => x.DateOfBirth, TestData.Now.AddYears(-30))
            .With(x => x.LicenseIssueDate, TestData.Now.AddYears(-5))
            .With(x => x.IsBlacklisted, false)
            .With(x => x.LoyaltyTier, LoyaltyTier.None));

        fixture.Customize<Vehicle>(c => c
            .With(x => x.Category, VehicleCategory.Economy)
            .With(x => x.IsAvailable, true));
    }
}

public class AdultAutoMoqDataAttribute : AutoDataAttribute
{
    public AdultAutoMoqDataAttribute() : base(() => FixtureFactory.Create(new ValidBookingWorldCustomization())) { }
}

public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] values)
        : base(new AdultAutoMoqDataAttribute(), values) { }
}
