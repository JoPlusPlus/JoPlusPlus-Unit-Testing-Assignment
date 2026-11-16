namespace DriveNow.UnitTests;

public class BookingServiceTests
{
    const string Ref = "DN-20261105-A1B2"; 

    readonly BookingWorld _w = new();

    void SeedHappyPath(VehicleCategory category = VehicleCategory.Economy)
    {
        _w.Customers.Items.Add(TestData.MakeCustomer());
        _w.Vehicles.Items.Add(TestData.MakeCar(category: category));
    }

    [Fact]
    public void BR10_missing_customer_is_rejected()
    {
        var sut = _w.Build(email: new DummyEmailSender());

        var ex = Assert.Throws<CustomerNotFoundException>(() => sut.CreateBooking(TestData.MakeRequest(customerId: 99)));
        Assert.Equal("Customer #99 not found", ex.Message);
    }

    [Fact]
    public void BR11_blacklisted_customer_is_rejected()
    {
        _w.Customers.Items.Add(TestData.MakeCustomer(blacklisted: true));
        _w.Vehicles.Items.Add(TestData.MakeCar());

        var ex = Assert.Throws<BookingRejectedException>(() => _w.Build().CreateBooking(TestData.MakeRequest()));
        Assert.Equal("Customer is blacklisted", ex.Message);
    }

    [Fact]
    public void BR12_driver_under_21_is_rejected()
    {
        _w.Customers.Items.Add(TestData.MakeCustomer(age: 20));
        _w.Vehicles.Items.Add(TestData.MakeCar());

        var ex = Assert.Throws<BookingRejectedException>(() => _w.Build().CreateBooking(TestData.MakeRequest()));
        Assert.Equal("Driver must be at least 21", ex.Message);
    }

    [Fact]
    public void BR13_license_under_one_year_is_rejected()
    {
        _w.Customers.Items.Add(TestData.MakeCustomer(licenseYears: 0));
        _w.Vehicles.Items.Add(TestData.MakeCar());

        var ex = Assert.Throws<BookingRejectedException>(() => _w.Build().CreateBooking(TestData.MakeRequest()));
        Assert.Equal("License must be held for at least 1 year", ex.Message);
    }

    [Fact]
    public void BR14_missing_vehicle_is_rejected()
    {
        _w.Customers.Items.Add(TestData.MakeCustomer());

        var ex = Assert.Throws<VehicleNotFoundException>(() => _w.Build().CreateBooking(TestData.MakeRequest(vehicleId: 1)));
        Assert.Equal("Vehicle #1 not found", ex.Message);
    }

    [Fact]
    public void BR15_unavailable_vehicle_is_rejected()
    {
        _w.Customers.Items.Add(TestData.MakeCustomer());
        _w.Vehicles.Items.Add(TestData.MakeCar(available: false));

        var ex = Assert.Throws<BookingRejectedException>(() => _w.Build().CreateBooking(TestData.MakeRequest()));
        Assert.Equal("Vehicle is not available", ex.Message);
    }

    [Fact]
    public void BR16_luxury_under_25_is_rejected()
    {
        _w.Customers.Items.Add(TestData.MakeCustomer(age: 24, licenseYears: 5));
        _w.Vehicles.Items.Add(TestData.MakeCar(category: VehicleCategory.Luxury));

        var ex = Assert.Throws<BookingRejectedException>(() => _w.Build().CreateBooking(TestData.MakeRequest()));
        Assert.Equal("Luxury vehicles require age 25+ and 3 years of license", ex.Message);
    }

    [Fact]
    public void BR16_luxury_at_exactly_25_with_3_years_is_accepted()
    {
        _w.Customers.Items.Add(TestData.MakeCustomer(age: 25, licenseYears: 3));
        _w.Vehicles.Items.Add(TestData.MakeCar(category: VehicleCategory.Luxury));

        var booking = _w.Build().CreateBooking(TestData.MakeRequest());
        Assert.Equal(5000m, booking.Deposit);
    }

    [Fact]
    public void BR17_van_with_one_year_of_license_is_rejected()
    {
        _w.Customers.Items.Add(TestData.MakeCustomer(licenseYears: 1));
        _w.Vehicles.Items.Add(TestData.MakeCar(category: VehicleCategory.Van));

        var ex = Assert.Throws<BookingRejectedException>(() => _w.Build().CreateBooking(TestData.MakeRequest()));
        Assert.Equal("Vans require 2 years of license", ex.Message);
    }

    [Fact]
    public void BR17_van_with_two_years_of_license_is_accepted()
    {
        _w.Customers.Items.Add(TestData.MakeCustomer(licenseYears: 2));
        _w.Vehicles.Items.Add(TestData.MakeCar(category: VehicleCategory.Van));

        var booking = _w.Build().CreateBooking(TestData.MakeRequest());
        Assert.Equal(2500m, booking.Deposit);
    }

    [Fact]
    public void BR18_pickup_within_two_hours_is_rejected()
    {
        SeedHappyPath();
        var request = TestData.MakeRequest(pickup: TestData.Now.AddHours(1));

        var ex = Assert.Throws<BookingRejectedException>(() => _w.Build().CreateBooking(request));
        Assert.Equal("Pickup must be at least 2 hours from now", ex.Message);
    }

    [Fact]
    public void BR18_pickup_exactly_two_hours_from_now_is_accepted()
    {
        SeedHappyPath();
        var request = TestData.MakeRequest(pickup: TestData.Now.AddHours(2));

        var booking = _w.Build().CreateBooking(request);
        Assert.Equal(request.PickupAt, booking.PickupAt);
    }

    [Fact]
    public void BR19_return_not_after_pickup_is_rejected()
    {
        SeedHappyPath();
        var pickup = TestData.Now.AddDays(3);
        var request = TestData.MakeRequest(pickup: pickup, returnAt: pickup);

        var ex = Assert.Throws<BookingRejectedException>(() => _w.Build().CreateBooking(request));
        Assert.Equal("Rental period is invalid", ex.Message);
    }

    [Fact]
    public void BR19_rental_longer_than_30_days_is_rejected()
    {
        SeedHappyPath();
        var pickup = TestData.Now.AddDays(3);
        var request = TestData.MakeRequest(pickup: pickup, returnAt: pickup.AddDays(31));

        var ex = Assert.Throws<BookingRejectedException>(() => _w.Build().CreateBooking(request));
        Assert.Equal("Rental period is invalid", ex.Message);
    }

    [Fact]
    public void BR20_more_than_three_additional_drivers_is_rejected()
    {
        SeedHappyPath();

        var ex = Assert.Throws<BookingRejectedException>(() => _w.Build().CreateBooking(TestData.MakeRequest(drivers: 4)));
        Assert.Equal("Up to 3 additional drivers allowed", ex.Message);
    }

    [Fact]
    public void BR21_overlapping_booking_on_same_car_is_rejected()
    {
        SeedHappyPath();
        _w.Bookings.Items.Add(new Booking
        {
            CustomerId = 2, VehicleId = 1, Status = BookingStatus.Reserved,
            PickupAt = TestData.Now.AddDays(3), ReturnAt = TestData.Now.AddDays(5)
        });
        var request = TestData.MakeRequest(pickup: TestData.Now.AddDays(4), returnAt: TestData.Now.AddDays(6));

        var ex = Assert.Throws<BookingRejectedException>(() => _w.Build().CreateBooking(request));
        Assert.Equal("Vehicle is already booked for these dates", ex.Message);
    }

    [Fact]
    public void BR21_back_to_back_booking_on_same_car_is_allowed()
    {
        SeedHappyPath();
        _w.Bookings.Items.Add(new Booking
        {
            CustomerId = 2, VehicleId = 1, Status = BookingStatus.Reserved,
            PickupAt = TestData.Now.AddDays(3), ReturnAt = TestData.Now.AddDays(5)
        });
        var request = TestData.MakeRequest(pickup: TestData.Now.AddDays(5), returnAt: TestData.Now.AddDays(7));

        var booking = _w.Build().CreateBooking(request);
        Assert.Equal(TestData.Now.AddDays(5), booking.PickupAt);
    }

    [Fact]
    public void BR22_customer_with_two_open_bookings_is_rejected()
    {
        SeedHappyPath();
        _w.Bookings.Items.Add(new Booking { CustomerId = 1, VehicleId = 2, Status = BookingStatus.Reserved });
        _w.Bookings.Items.Add(new Booking { CustomerId = 1, VehicleId = 3, Status = BookingStatus.Active });

        var ex = Assert.Throws<BookingRejectedException>(() => _w.Build().CreateBooking(TestData.MakeRequest()));
        Assert.Equal("Customer already has 2 active bookings", ex.Message);
    }

    [Fact]
    public void BR22_customer_with_one_open_booking_is_allowed()
    {
        SeedHappyPath();
        _w.Bookings.Items.Add(new Booking { CustomerId = 1, VehicleId = 2, Status = BookingStatus.Reserved });

        var booking = _w.Build().CreateBooking(TestData.MakeRequest());
        Assert.Equal(1, booking.CustomerId);
    }

    [Fact]
    public void BR23_price_comes_from_pricing_service()
    {
        SeedHappyPath();
        _w.Pricing.Price = new PriceBreakdown { Total = 777m };

        var booking = _w.Build().CreateBooking(TestData.MakeRequest());
        Assert.Equal(777m, booking.Price.Total);
    }

    [Fact]
    public void BR23_invalid_promo_from_pricing_saves_nothing()
    {
        SeedHappyPath();
        _w.Pricing.ThrowOnCalculate = new InvalidPromoCodeException("Promo code 'NOPE' does not exist");

        Assert.Throws<InvalidPromoCodeException>(() => _w.Build().CreateBooking(TestData.MakeRequest(promo: "NOPE")));
        Assert.Empty(_w.Bookings.Items);
        Assert.Empty(_w.Payments.Charges);
    }

    [Fact]
    public void BR24_new_booking_is_reserved_with_created_at_now_and_start_mileage()
    {
        SeedHappyPath();

        var booking = _w.Build().CreateBooking(TestData.MakeRequest());
        Assert.Equal(BookingStatus.Reserved, booking.Status);
        Assert.Equal(TestData.Now, booking.CreatedAt);
        Assert.Equal(12345, booking.StartMileage);
    }

    [Fact]
    public void BR25_reference_is_DN_pickup_date_and_four_characters()
    {
        SeedHappyPath();

        var booking = _w.Build().CreateBooking(TestData.MakeRequest());
        Assert.Equal(Ref, booking.Reference);
    }

    [Theory]
    [InlineData(VehicleCategory.Economy, InsurancePlan.Basic, 1000)]
    [InlineData(VehicleCategory.Compact, InsurancePlan.Standard, 1000)]
    [InlineData(VehicleCategory.SUV, InsurancePlan.Basic, 2500)]
    [InlineData(VehicleCategory.Van, InsurancePlan.Premium, 1250)]
    [InlineData(VehicleCategory.Luxury, InsurancePlan.Basic, 5000)]
    [InlineData(VehicleCategory.Economy, InsurancePlan.Premium, 500)]
    [InlineData(VehicleCategory.Luxury, InsurancePlan.Premium, 2500)]
    public void BR26_deposit_by_category_and_insurance(VehicleCategory category, InsurancePlan plan, int expected)
    {
        SeedHappyPath(category);

        var booking = _w.Build().CreateBooking(TestData.MakeRequest(plan: plan));

        Assert.Equal((decimal)expected, booking.Deposit);
        var charge = Assert.Single(_w.Payments.Charges);
        Assert.Equal((decimal)expected, charge.Amount);
        Assert.Equal($"Deposit {Ref}", charge.Description);
    }

    [Fact]
    public void BR26_platinum_pays_no_deposit_and_gateway_is_not_called()
    {
        _w.Customers.Items.Add(TestData.MakeCustomer(tier: LoyaltyTier.Platinum));
        _w.Vehicles.Items.Add(TestData.MakeCar());

        var booking = _w.Build().CreateBooking(TestData.MakeRequest());

        Assert.Equal(0m, booking.Deposit);
        Assert.Empty(_w.Payments.Charges);
    }

    [Fact]
    public void BR26_gateway_failure_saves_nothing()
    {
        SeedHappyPath();
        _w.Payments.Fail = true;

        Assert.Throws<DriveNow.Infrastructure.External.PaymentFailedException>(() =>
            _w.Build().CreateBooking(TestData.MakeRequest()));
        Assert.Empty(_w.Bookings.Items);
        Assert.Empty(_w.Sms.Sent);
    }
    [Fact]
    public void BR27_promo_count_booking_saved_and_confirmations_sent()
    {
        SeedHappyPath();
        _w.Promos.Items.Add(new PromoCode
        {
            Code = "SAVE15", DiscountPercent = 15, MinDays = 1, MaxUses = 100, UsedCount = 3,
            ValidFrom = TestData.Now.AddDays(-10), ValidUntil = TestData.Now.AddDays(60)
        });
        var audit = new ManualAuditLog($"Booking {Ref} created");

        _w.Build(audit: audit).CreateBooking(TestData.MakeRequest(promo: "save15")); 

        Assert.Equal(4, _w.Promos.Items.Single().UsedCount);
        Assert.Single(_w.Bookings.Items);
        Assert.Equal(("0100000001", $"Booking {Ref} confirmed"), _w.Sms.Sent.Single());
        var mail = _w.Email.Sent.Single();
        Assert.Equal("driver@drivenow.eg", mail.To);
        Assert.Equal($"Booking {Ref} confirmed", mail.Subject);
        Assert.Equal($"Booking {Ref} confirmed", mail.Body);
        audit.AssertExpected();
    }
}
