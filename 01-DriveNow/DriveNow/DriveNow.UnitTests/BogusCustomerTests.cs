namespace DriveNow.UnitTests;

using Bogus;

public class BogusCustomerTests
{
    static readonly Faker<Customer> CustomerFaker = new Faker<Customer>()
        .RuleFor(c => c.Id, f => f.IndexFaker + 1)
        .RuleFor(c => c.FullName, f => f.Name.FullName())
        .RuleFor(c => c.Email, f => f.Internet.Email())
        .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber("0100#######"))
        .RuleFor(c => c.DateOfBirth, f => f.Date.Between(TestData.Now.AddYears(-60), TestData.Now.AddYears(-25)))
        .RuleFor(c => c.LicenseIssueDate, f => f.Date.Between(TestData.Now.AddYears(-8), TestData.Now.AddYears(-2)))
        .RuleFor(c => c.LoyaltyTier, f => LoyaltyTier.None)
        .RuleFor(c => c.IsBlacklisted, f => false);

    [Fact]
    public void Bogus_generated_adult_driver_books_a_car()
    {
        var world = new BookingWorld();
        var customer = CustomerFaker.Generate();
        world.Customers.Items.Add(customer);
        world.Vehicles.Items.Add(TestData.MakeCar());

        var booking = world.Build().CreateBooking(TestData.MakeRequest(customerId: customer.Id));

        Assert.Equal(customer.Id, booking.CustomerId);
    }
}
