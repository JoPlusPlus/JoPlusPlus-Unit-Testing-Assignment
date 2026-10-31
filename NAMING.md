# Naming Convention

Name: <your name>
Group / ID: <your id>

# Task 0 - Part A

## R1 - Test projects, classes and files
1. Test projects: `ProjectName.UnitTests` -> `DriveNow.UnitTests`, `ParkSmart.UnitTests`
2. Test classes: `ClassNameTests` -> `RentalMathTests`, `PricingServiceTests`
3. Test files: same name as the class -> `RentalMathTests.cs`

## R2 - Test methods

### Styles
1. `Method_Scenario_ExpectedResult` -> the basic one
2. `Should_X_When_Y` -> i think it is used with the Shouldly framework more
3. `Given_When_Then` -> long, it is more for BDD

I picked: `Method_Scenario_ExpectedResult`

Why:
- i see the method name first, so i know what is tested

### Examples from code
1. `CalculateAge_BeforeBirthday_ReturnsLowerAge`
2. `CalculateLicenseYears_InvalidDate_ThrowsArgumentException`
3. `CalculateRentalDays_GraceHourExceeded_ReturnsExpectedDays`

## R3 - Test helpers

Hand-written test doubles: `KindName` and they live in `DriveNow.UnitTests.TestDoubles`
- Fake -> `FakeBookingRepository`
- Stub -> `StubPaymentGateway`
- Spy -> `SpySmsSender`
- Mock -> `MockPaymentGateway`
- Dummy -> `DummyEmailSender`

Bogus fakers: `EntityFaker` -> `CustomerFaker`, `VehicleFaker`
- methods start with `With` -> `WithAge(23)`
- always use a fixed seed

AutoFixture:
- customizations end with `Customization` -> `ValidCustomerCustomization`
- attributes end with `DataAttribute` -> `AutoMoqDataAttribute`

## R4 - Production code

| Kind | Rule | Example | Namespace |
|------|------|---------|-----------|
| Interface | starts with `I` | `IBookingRepository` | - |
| Repository | ends with `Repository` | `BookingRepository` | `Repositories` |
| Service | ends with `Service` | `BookingService` | `Services` |
| Exception | ends with `Exception` | `BookingConflictException` | `DriveNow.Domain.Exceptions` |

Dependency rules:
- Domain depends on nothing
- Application depends only on Domain
- Infrastructure does not depend on ConsoleApp
