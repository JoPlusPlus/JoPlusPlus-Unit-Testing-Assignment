namespace DriveNow.UnitTests;

public static class TestData
{
    public static readonly DateTime Now = new(2026, 11, 2, 10, 0, 0);

    public static Customer MakeCustomer(int id = 1, int age = 30, int licenseYears = 5,
        bool blacklisted = false, LoyaltyTier tier = LoyaltyTier.None) => new()
    {
        Id = id,
        FullName = "Test Driver",
        Email = "driver@drivenow.eg",
        Phone = "0100000001",
        DateOfBirth = Now.AddYears(-age),
        LicenseIssueDate = Now.AddYears(-licenseYears),
        IsBlacklisted = blacklisted,
        LoyaltyTier = tier
    };

    public static Vehicle MakeCar(int id = 1, VehicleCategory category = VehicleCategory.Economy,
        bool available = true, int mileage = 12345) => new()
    {
        Id = id,
        PlateNumber = "ABC-1001",
        Model = "Test Car",
        Category = category,
        DailyRate = 600m,
        Mileage = mileage,
        IsAvailable = available
    };

    public static BookingRequest MakeRequest(int customerId = 1, int vehicleId = 1,
        DateTime? pickup = null, DateTime? returnAt = null, int drivers = 0,
        string? promo = null, InsurancePlan plan = InsurancePlan.Basic)
    {
        var start = pickup ?? Now.AddDays(3);
        return new BookingRequest
        {
            CustomerId = customerId,
            VehicleId = vehicleId,
            PickupAt = start,
            ReturnAt = returnAt ?? start.AddDays(2),
            InsurancePlan = plan,
            AdditionalDrivers = drivers,
            PromoCode = promo
        };
    }
}
