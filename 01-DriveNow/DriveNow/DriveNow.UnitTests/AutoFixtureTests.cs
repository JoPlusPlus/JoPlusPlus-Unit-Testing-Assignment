namespace DriveNow.UnitTests;

using AutoFixture.Xunit2;
using DriveNow.Domain.Services;

public class AutoFixtureTests
{
    static void Arrange(
        Mock<ICustomerRepository> customers, Mock<IVehicleRepository> vehicles,
        Mock<IBookingRepository> bookings, Mock<IClock> clock,
        Customer customer, Vehicle vehicle)
    {
        clock.Setup(c => c.Now).Returns(TestData.Now);
        customers.Setup(c => c.GetById(customer.Id)).Returns(customer);
        vehicles.Setup(v => v.GetById(vehicle.Id)).Returns(vehicle);
        bookings.Setup(b => b.GetOpenBookingsForVehicle(It.IsAny<int>())).Returns(new List<Booking>());
    }

    [Theory, AutoMoqData]
    public void R3_frozen_repository_reports_two_open_bookings(
        [Frozen] Mock<ICustomerRepository> customers,
        [Frozen] Mock<IVehicleRepository> vehicles,
        [Frozen] Mock<IBookingRepository> bookings,
        [Frozen] Mock<IClock> clock,
        BookingService sut)
    {
        Arrange(customers, vehicles, bookings, clock, TestData.MakeCustomer(), TestData.MakeCar());
        bookings.Setup(b => b.CountOpenBookingsForCustomer(1)).Returns(2);

        var ex = Assert.Throws<BookingRejectedException>(() => sut.CreateBooking(TestData.MakeRequest()));

        Assert.Equal("Customer already has 2 active bookings", ex.Message);
        bookings.Verify(b => b.Add(It.IsAny<Booking>()), Times.Never);
    }

    [Theory, AutoMoqData]
    public void R4_FAILS_RANDOMLY_anonymous_customer_books_a_car(
        [Frozen] Mock<ICustomerRepository> customers,
        [Frozen] Mock<IVehicleRepository> vehicles,
        [Frozen] Mock<IBookingRepository> bookings,
        [Frozen] Mock<IClock> clock,
        Customer customer,
        BookingService sut)
    {
        Arrange(customers, vehicles, bookings, clock, customer, TestData.MakeCar());

        sut.CreateBooking(TestData.MakeRequest(customerId: customer.Id));
    }

    public void R4_fixed_with_customization_adult_customer_books_a_car(
        [Frozen] Mock<ICustomerRepository> customers,
        [Frozen] Mock<IVehicleRepository> vehicles,
        [Frozen] Mock<IBookingRepository> bookings,
        [Frozen] Mock<IClock> clock,
        Customer customer,
        BookingService sut)
    {
        Arrange(customers, vehicles, bookings, clock, customer, TestData.MakeCar());

        var booking = sut.CreateBooking(TestData.MakeRequest(customerId: customer.Id));

        Assert.Equal(customer.Id, booking.CustomerId);
    }

    [Theory]
    [InlineAutoMoqData(20, false)]
    [InlineAutoMoqData(21, true)]
    [InlineAutoMoqData(25, true)]
    public void R5_age_under_test(
        int age, bool accepted,
        [Frozen] Mock<ICustomerRepository> customers,
        [Frozen] Mock<IVehicleRepository> vehicles,
        [Frozen] Mock<IBookingRepository> bookings,
        [Frozen] Mock<IClock> clock,
        Customer customer,
        Vehicle vehicle,
        BookingService sut)
    {
        customer.DateOfBirth = TestData.Now.AddYears(-age);
        Arrange(customers, vehicles, bookings, clock, customer, vehicle);
        var request = TestData.MakeRequest(customerId: customer.Id, vehicleId: vehicle.Id);

        if (accepted)
        {
            sut.CreateBooking(request);
        }
        else
        {
            var ex = Assert.Throws<BookingRejectedException>(() => sut.CreateBooking(request));
            Assert.Equal("Driver must be at least 21", ex.Message);
        }
    }
}
