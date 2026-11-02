namespace DriveNow.UnitTests;

public class DummyEmailSender : IEmailSender
{
    public void Send(string to, string subject, string body) =>
        throw new InvalidOperationException("Dummy email sender should never be used");
}


public class StubClock : IClock
{
    public StubClock(DateTime now) => Now = now;
    public DateTime Now { get; }
}

public class StubPricingService : IPricingService
{
    public PriceBreakdown Price { get; set; } = new() { Total = 1000m };
    public Exception? ThrowOnCalculate { get; set; }

    public PriceBreakdown Calculate(Customer customer, Vehicle vehicle, BookingRequest request)
    {
        if (ThrowOnCalculate is not null)
            throw ThrowOnCalculate;
        return Price;
    }
}

public class StubReferenceGenerator : IReferenceGenerator
{
    public string Next(int length) => "A1B2";
}


public class SpySmsSender : ISmsSender
{
    public List<(string Phone, string Text)> Sent { get; } = [];
    public void Send(string phone, string text) => Sent.Add((phone, text));
}

public class SpyEmailSender : IEmailSender
{
    public List<(string To, string Subject, string Body)> Sent { get; } = [];
    public void Send(string to, string subject, string body) => Sent.Add((to, subject, body));
}

public class SpyPaymentGateway : IPaymentGateway
{
    public List<(int CustomerId, decimal Amount, string Description)> Charges { get; } = [];
    public bool Fail { get; set; }

    public string Charge(int customerId, decimal amount, string description)
    {
        Charges.Add((customerId, amount, description));
        if (Fail)
            throw new DriveNow.Infrastructure.External.PaymentFailedException("Gateway timeout");
        return "PAY-TEST";
    }
}


public class ManualAuditLog : IAuditLog
{
    readonly List<string> _expected;
    readonly List<string> _actual = [];

    public ManualAuditLog(params string[] expected) => _expected = expected.ToList();

    public void Write(string message) => _actual.Add(message);

    public void AssertExpected() => Assert.Equal(_expected, _actual);
}


public class FakeCustomerRepository : ICustomerRepository
{
    public List<Customer> Items { get; } = [];
    public Customer? GetById(int id) => Items.FirstOrDefault(c => c.Id == id);
}

public class FakeVehicleRepository : IVehicleRepository
{
    public List<Vehicle> Items { get; } = [];
    public Vehicle? GetById(int id) => Items.FirstOrDefault(v => v.Id == id);
}

public class FakePromoCodeRepository : IPromoCodeRepository
{
    public List<PromoCode> Items { get; } = [];

    public PromoCode? GetByCode(string code) =>
        Items.FirstOrDefault(p => string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase));

    public void Update(PromoCode promo)
    {
    }
}

public class FakeBookingRepository : IBookingRepository
{
    public List<Booking> Items { get; } = [];

    public Booking? GetById(int id) => Items.FirstOrDefault(b => b.Id == id);

    public void Add(Booking booking)
    {
        booking.Id = Items.Count + 1;
        Items.Add(booking);
    }

    public void Update(Booking booking)
    {
    }

    public List<Booking> GetOpenBookingsForVehicle(int vehicleId) =>
        Items.Where(b => b.VehicleId == vehicleId && IsOpen(b)).ToList();

    public int CountOpenBookingsForCustomer(int customerId) =>
        Items.Count(b => b.CustomerId == customerId && IsOpen(b));

    static bool IsOpen(Booking b) => b.Status is BookingStatus.Reserved or BookingStatus.Active;
}

public class BookingWorld
{
    public FakeCustomerRepository Customers { get; } = new();
    public FakeVehicleRepository Vehicles { get; } = new();
    public FakeBookingRepository Bookings { get; } = new();
    public FakePromoCodeRepository Promos { get; } = new();
    public StubPricingService Pricing { get; } = new();
    public SpyPaymentGateway Payments { get; } = new();
    public SpySmsSender Sms { get; } = new();
    public SpyEmailSender Email { get; } = new();

    public BookingService Build(IEmailSender? email = null, IAuditLog? audit = null) =>
        new(Customers, Vehicles, Bookings, Promos, Pricing, Payments, Sms,
            email ?? Email, new StubReferenceGenerator(), new StubClock(TestData.Now),
            audit ?? new ManualAuditLog());
}
