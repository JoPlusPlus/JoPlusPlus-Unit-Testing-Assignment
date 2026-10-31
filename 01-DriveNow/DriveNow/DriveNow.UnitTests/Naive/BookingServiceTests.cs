using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using FluentAssertions;

namespace DriveNow.UnitTests.Naive;

public class BookingServiceTests
{
    private const int GoldCustomerId = 1; 
    private const int RegularCustomerId = 2; 
    private const int SilverCustomerId = 3; 
    private const int PlatinumCustomerId = 5; 

    private const int EconomyVehicleId = 1;
    private const int EconomyVehicle2Id = 2; 
    private const int CompactVehicleId = 3; 

    private static BookingRequest Request(int customerId, int vehicleId, int daysFromNow) => new()
    {
        CustomerId = customerId,
        VehicleId = vehicleId,
        PickupAt = DateTime.Now.AddDays(daysFromNow),
        ReturnAt = DateTime.Now.AddDays(daysFromNow + 2),
        InsurancePlan = InsurancePlan.Standard,
        AdditionalDrivers = 0
    };

    [Fact]
    [Trait("Rule", "BR-24")] 
    [Trait("Rule", "BR-25")] 
    [Trait("Rule", "BR-26")] 
    public void CreateBooking_ValidRequest_ReturnsReservedBooking()
    {
        var service = new BookingService();

        var booking = service.CreateBooking(Request(GoldCustomerId, EconomyVehicleId, daysFromNow: 10));

        booking.Status.Should().Be(BookingStatus.Reserved);
        booking.Deposit.Should().Be(1000m);
        booking.Reference.Should().StartWith("DN-");
    }

    [Fact]
    [Trait("Rule", "BR-26")] 
    public void CreateBooking_PlatinumCustomer_ChargesNoDeposit()
    {
        var service = new BookingService();

        var booking = service.CreateBooking(Request(PlatinumCustomerId, CompactVehicleId, daysFromNow: 15));

        booking.Deposit.Should().Be(0m);
    }

    [Fact]
    [Trait("Rule", "BR-21")]
    public void CreateBooking_OverlappingDates_ThrowsBookingRejected()
    {
        var service = new BookingService();
        service.CreateBooking(Request(SilverCustomerId, EconomyVehicle2Id, daysFromNow: 20));

        var act = () => service.CreateBooking(Request(RegularCustomerId, EconomyVehicle2Id, daysFromNow: 20));

        act.Should().Throw<BookingRejectedException>()
           .WithMessage("Vehicle is already booked for these dates");
    }
}