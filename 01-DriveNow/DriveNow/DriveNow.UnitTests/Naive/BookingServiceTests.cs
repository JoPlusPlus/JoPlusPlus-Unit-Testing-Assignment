using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Enums;
using DriveNow.Domain.Exceptions;
using DriveNow.Infrastructure.External;
using FluentAssertions;

namespace DriveNow.UnitTests.Naive;

public class BookingServiceTests
{
    // Seed data (README "Seed data" / InMemoryDatabase)
    private const int GoldCustomerId = 1; // Ahmed Fawzy, Gold, 1 open seed booking (vehicle 5)
    private const int RegularCustomerId = 2; // Mariam Adel, 23y, no tier, no open bookings
    private const int SilverCustomerId = 3; // Khaled Mostafa, 25y, Silver, no open bookings
    private const int PlatinumCustomerId = 5; // Tamer Nabil, Platinum, 1 open seed booking (vehicle 7)

    private const int EconomyVehicleId = 1; // Hyundai Accent, no seed bookings
    private const int EconomyVehicle2Id = 2; // Kia Rio, no seed bookings
    private const int CompactVehicleId = 3; // Toyota Corolla, no seed bookings

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
    [Trait("Rule", "BR-24")] // Reserved status, start mileage
    [Trait("Rule", "BR-25")] // reference format
    [Trait("Rule", "BR-26")] // Economy deposit 1,000
    public void CreateBooking_ValidRequest_ReturnsReservedBooking()
    {
        var service = new BookingService();

        var booking = service.CreateBooking(Request(GoldCustomerId, EconomyVehicleId, daysFromNow: 10));

        booking.Status.Should().Be(BookingStatus.Reserved);
        booking.Deposit.Should().Be(1000m);
        booking.Reference.Should().StartWith("DN-");
    }

    [Fact]
    [Trait("Rule", "BR-26")] // Platinum pays 0 and the gateway is not called
    public void CreateBooking_PlatinumCustomer_ChargesNoDeposit()
    {
        var service = new BookingService();

        var booking = service.CreateBooking(Request(PlatinumCustomerId, CompactVehicleId, daysFromNow: 15));

        booking.Deposit.Should().Be(0m);
        // "The gateway is not called" cannot be verified: PaymentGateway is created inside the service.
    }

    [Fact]
    [Trait("Rule", "BR-21")] // overlap with the vehicle's open bookings
    public void CreateBooking_OverlappingDates_ThrowsBookingRejected()
    {
        var service = new BookingService();
        service.CreateBooking(Request(SilverCustomerId, EconomyVehicle2Id, daysFromNow: 20));

        var act = () => service.CreateBooking(Request(RegularCustomerId, EconomyVehicle2Id, daysFromNow: 20));

        act.Should().Throw<BookingRejectedException>()
           .WithMessage("Vehicle is already booked for these dates");
    }
}