namespace DriveNow.UnitTests;


public class BookingServiceMoqTests
{
    static BookingService Build(
        Mock<ICustomerRepository> customers,
        Mock<IVehicleRepository> vehicles,
        Mock<IBookingRepository> bookings,
        Mock<IPaymentGateway> payments,
        Mock<ISmsSender> sms) =>
        new(customers.Object, vehicles.Object, bookings.Object, new FakePromoCodeRepository(),
            new StubPricingService(), payments.Object, sms.Object, new SpyEmailSender(),
            new StubReferenceGenerator(), new StubClock(TestData.Now), new ManualAuditLog());


    [Fact]
    public void BR15_unavailable_vehicle_is_rejected_moq()
    {
        var customers = new Mock<ICustomerRepository>();
        customers.Setup(c => c.GetById(1)).Returns(TestData.MakeCustomer());

        var vehicles = new Mock<IVehicleRepository>();
        vehicles.Setup(v => v.GetById(1)).Returns(TestData.MakeCar(available: false));

        var sut = Build(customers, vehicles, new Mock<IBookingRepository>(),
            new Mock<IPaymentGateway>(), new Mock<ISmsSender>());

        var ex = Assert.Throws<BookingRejectedException>(() => sut.CreateBooking(TestData.MakeRequest()));
        Assert.Equal("Vehicle is not available", ex.Message);
    }

    [Fact]
    public void BR26_platinum_gateway_never_charged_moq()
    {
        var customers = new Mock<ICustomerRepository>();
        customers.Setup(c => c.GetById(1)).Returns(TestData.MakeCustomer(tier: LoyaltyTier.Platinum));

        var vehicles = new Mock<IVehicleRepository>();
        vehicles.Setup(v => v.GetById(1)).Returns(TestData.MakeCar());

        var bookings = new Mock<IBookingRepository>();
        bookings.Setup(b => b.GetOpenBookingsForVehicle(1)).Returns(new List<Booking>());

        var payments = new Mock<IPaymentGateway>();
        var sut = Build(customers, vehicles, bookings, payments, new Mock<ISmsSender>());

        sut.CreateBooking(TestData.MakeRequest());

        payments.Verify(p => p.Charge(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void BR27_confirmation_sms_sent_once_moq()
    {
        var customers = new Mock<ICustomerRepository>();
        customers.Setup(c => c.GetById(1)).Returns(TestData.MakeCustomer());

        var vehicles = new Mock<IVehicleRepository>();
        vehicles.Setup(v => v.GetById(1)).Returns(TestData.MakeCar());

        var bookings = new Mock<IBookingRepository>();
        bookings.Setup(b => b.GetOpenBookingsForVehicle(1)).Returns(new List<Booking>());

        var sms = new Mock<ISmsSender>();
        var sut = Build(customers, vehicles, bookings, new Mock<IPaymentGateway>(), sms);

        sut.CreateBooking(TestData.MakeRequest());

        sms.Verify(s => s.Send("0100000001", "Booking DN-20261105-A1B2 confirmed"), Times.Once);
    }
}
