namespace DriveNow.UnitTests;

using VerifyXunit;
using static VerifyXunit.Verifier;

public class BookingSnapshotTests
{
    [Fact]
    public Task Created_booking_snapshot()
    {
        var world = new BookingWorld();
        world.Customers.Items.Add(TestData.MakeCustomer());
        world.Vehicles.Items.Add(TestData.MakeCar());

        var booking = world.Build().CreateBooking(TestData.MakeRequest());

        return Verify(booking);
    }
}
