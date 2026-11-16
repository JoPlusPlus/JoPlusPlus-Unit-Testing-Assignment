using DriveNow.Application.Models;
using DriveNow.Application.Services;
using DriveNow.Domain.Entities;
using DriveNow.Domain.Enums;
using DriveNow.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace DriveNow.UnitTests;

public class PricingServiceTests
{
    public static TheoryData<int,decimal> CalculateData = new()
    {
        {20,0m },
        {21,75m },
        {24,75m },
        {25,0m }
    };

    [Theory]
    [MemberData(nameof(CalculateData))]
    public void Calculate_YoungDriverAge_ExpectedYoungDriverSurcharge(int age,decimal expectedSurcharge)
    {
        var service = new PricingService();

        var customer = new Customer
        {
            DateOfBirth = DateTime.Today.AddYears(-age),
            LoyaltyTier = LoyaltyTier.None
        };

        var vehicle = new Vehicle
        {
            DailyRate = 100m,
            Category = VehicleCategory.Compact
        };

        var request = new BookingRequest
        {
            PickupAt = DateTime.Today,
            ReturnAt = DateTime.Today.AddDays(5),
            InsurancePlan = InsurancePlan.Basic
        };

        var result = service.Calculate(customer, vehicle, request);

        result.YoungDriverSurcharge.ShouldBe(expectedSurcharge);
    }

    public static TheoryData<int, decimal> DurationDiscountData = new()
    {
        { 6, 0m },
        { 7, 0m },
        { 13, 130m },
        { 14, 210m }
    };

    [Theory]
    [MemberData(nameof(DurationDiscountData))]
    public void Calculate_RentalDays_ExpectedDurationDiscount(int days,decimal expectedDiscount)
    {
        var service = new PricingService();

        var customer = new Customer
        {
            DateOfBirth = DateTime.Today.AddYears(-30)
        };

        var vehicle = new Vehicle
        {
            DailyRate = 100m,
            Category = VehicleCategory.Compact
        };

        var request = new BookingRequest
        {
            PickupAt = DateTime.Today,
            ReturnAt = DateTime.Today.AddDays(days),
            InsurancePlan = InsurancePlan.Basic
        };

        var result = service.Calculate(customer, vehicle, request);

        result.DurationDiscount.ShouldBe(expectedDiscount);
    }

    public static TheoryData<LoyaltyTier, decimal> LoyaltyDiscountData = new()
    {
        { LoyaltyTier.None, 0m },
        { LoyaltyTier.Silver, 50m },
        { LoyaltyTier.Gold, 100m },
        { LoyaltyTier.Platinum, 150m }
    };

    [Theory]
    [MemberData(nameof(LoyaltyDiscountData))]
    public void Calculate_LoyaltyTier_ExpectedLoyaltyDiscount(LoyaltyTier tier,decimal expectedDiscount)
    {
        var service = new PricingService();

        var customer = new Customer
        {
            LoyaltyTier = tier,
            DateOfBirth = DateTime.Today.AddYears(-30)
        };

        var vehicle = new Vehicle
        {
            DailyRate = 100m,
            Category = VehicleCategory.Compact
        };

        var request = new BookingRequest
        {
            PickupAt = DateTime.Today,
            ReturnAt = DateTime.Today.AddDays(10),
            InsurancePlan = InsurancePlan.Basic
        };

        var result = service.Calculate(customer, vehicle, request);

        result.LoyaltyDiscount.ShouldBe(expectedDiscount);
    }

    public static TheoryData<VehicleCategory, decimal> StandardInsuranceData = new()
    {
        { VehicleCategory.Compact, 1050m },
        { VehicleCategory.Luxury, 2100m }
    };

    [Theory]
    [MemberData(nameof(StandardInsuranceData))]
    public void Calculate_StandardInsurance_ExpectedInsuranceCost(VehicleCategory category,decimal expectedCost)
    {
        var service = new PricingService();

        var customer = new Customer
        {
            DateOfBirth = DateTime.Today.AddYears(-30)
        };

        var vehicle = new Vehicle
        {
            Category = category,
            DailyRate = 500m
        };

        var request = new BookingRequest
        {
            PickupAt = DateTime.Today,
            ReturnAt = DateTime.Today.AddDays(7),
            InsurancePlan = InsurancePlan.Standard
        };

        var result = service.Calculate(customer, vehicle, request);

        result.InsuranceCost.ShouldBe(expectedCost);
    }

    public static TheoryData<LoyaltyTier, decimal> LoyaltyTierData = new()
    {
        { LoyaltyTier.None, 700m },
        { LoyaltyTier.Silver, 0m },
        { LoyaltyTier.Gold, 0m },
        { LoyaltyTier.Platinum, 0m }
    };

    [Theory]
    [MemberData(nameof(LoyaltyTierData))]
    public void Calculate_OneAdditionalDriver_ExpectedDriverCost(LoyaltyTier tier,decimal expectedCost)
    {
        var service = new PricingService();

        var customer = new Customer
        {
            LoyaltyTier = tier,
            DateOfBirth = DateTime.Today.AddYears(-30)
        };

        var vehicle = new Vehicle
        {
            DailyRate = 500m,
            Category = VehicleCategory.Compact
        };

        var request = new BookingRequest
        {
            PickupAt = DateTime.Today,
            ReturnAt = DateTime.Today.AddDays(7),
            AdditionalDrivers = 1,
            InsurancePlan = InsurancePlan.Basic
        };

        var result = service.Calculate(customer, vehicle, request);

        result.AdditionalDriversCost.ShouldBe(expectedCost);
    }

    [Fact]
    public void Calculate_WorkedExampleA_ExpectedPriceBreakdown()
    {
        var service = new PricingService();

        var customer = new Customer
        {
            LoyaltyTier = LoyaltyTier.Gold,
            DateOfBirth = DateTime.Today.AddYears(-34)
        };

        var vehicle = new Vehicle
        {
            DailyRate = 600m,
            Category = VehicleCategory.Economy
        };

        var pickup = new DateTime(2026, 11, 1, 10, 0, 0);

        var request = new BookingRequest
        {
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(3),
            InsurancePlan = InsurancePlan.Standard,
            AdditionalDrivers = 0
        };

        var result = service.Calculate(customer, vehicle, request);

        result.RentalDays.ShouldBe(3);
        result.BaseAmount.ShouldBe(1800m);
        result.WeekendSurcharge.ShouldBe(0m);
        result.YoungDriverSurcharge.ShouldBe(0m);
        result.DurationDiscount.ShouldBe(0m);
        result.LoyaltyDiscount.ShouldBe(180m);
        result.PromoDiscount.ShouldBe(0m);
        result.InsuranceCost.ShouldBe(450m);
        result.AdditionalDriversCost.ShouldBe(0m);
        result.Total.ShouldBe(2070m);
    }

    [Fact]
    public void Calculate_WorkedExampleB_ExpectedPriceBreakdown()
    {
        var service = new PricingService();

        var customer = new Customer
        {
            LoyaltyTier = LoyaltyTier.Silver,
            DateOfBirth = DateTime.Today.AddYears(-23)
        };

        var vehicle = new Vehicle
        {
            DailyRate = 1500m,
            Category = VehicleCategory.SUV
        };

        var pickup = new DateTime(2026, 11, 5, 9, 0, 0);

        var request = new BookingRequest
        {
            PickupAt = pickup,
            ReturnAt = pickup.AddDays(7).AddMinutes(30),
            InsurancePlan = InsurancePlan.Premium,
            PromoCode = "SAVE15",
            AdditionalDrivers = 1
        };

        var result = service.Calculate(customer, vehicle, request);

        result.RentalDays.ShouldBe(7);
        result.BaseAmount.ShouldBe(10500m);
        result.WeekendSurcharge.ShouldBe(600m);
        result.YoungDriverSurcharge.ShouldBe(1575m);
        result.DurationDiscount.ShouldBe(0m);
        result.LoyaltyDiscount.ShouldBe(525m);
        result.PromoDiscount.ShouldBe(1575m);
        result.InsuranceCost.ShouldBe(2100m);
        result.AdditionalDriversCost.ShouldBe(0m);
    }
}