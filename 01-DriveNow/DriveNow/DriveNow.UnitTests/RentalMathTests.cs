using System.Xml.Serialization;
using DriveNow.Domain.Services;
using FluentAssertions;
using Shouldly;

namespace DriveNow.UnitTests
{
    public class RentalMathTests
    {
        [Theory]
        [Trait("Rule","BR-01")]

        // happy case
        [InlineData("2000-06-15", "2020-06-14", 19)]
        [InlineData("2000-06-15", "2020-06-15", 20)]
        [InlineData("2000-06-15", "2020-06-16", 20)]
        // boundaries
        [InlineData("1999-12-31", "2020-12-30", 20)]
        [InlineData("1999-12-31", "2020-12-31", 21)]
        [InlineData("2000-01-01", "2020-12-31", 20)]
        [InlineData("2000-01-01", "2021-01-01", 21)]
        // ZERO Afge
        [InlineData("2020-03-10", "2020-03-10", 0)]
        [InlineData("2020-03-10", "2021-03-09", 0)]
        // Born on Leap Year and current year is leap
        [InlineData("2000-02-29", "2024-02-28", 23)]
        [InlineData("2000-02-29", "2024-02-29", 24)]
        // Born on Leap Year and current year is not leap
        [InlineData("2000-02-29", "2025-02-28", 25)]


        public void CalculateAge_HappyAndSadSenario_ReturnsExpectedAge(string birth, string today, int expected)
        {
            var birthDate = DateTime.Parse(birth);
            var todayhDate = DateTime.Parse(today);

            RentalMath.CalculateAge(birthDate,todayhDate).ShouldBe(expected);
        }
        [Theory]
        [Trait("Rule", "BR-02")]
        [InlineData("2015-05-10", "2020-05-09", 4)]
        [InlineData("2015-05-10", "2020-05-10", 5)]
        [InlineData("2015-05-10", "2020-05-11", 5)]
        [InlineData("2020-05-10", "2020-05-10", 0)]
        [InlineData("2020-05-10", "2021-05-09", 0)]
        [InlineData("2020-05-10", "2021-05-10", 1)]
        [InlineData("2016-02-29", "2021-02-27", 4)]
        [InlineData("2016-02-29", "2021-02-28", 5)]
        [InlineData("2016-02-29", "2021-03-01", 5)]
        public void CalculateLicenseYears_AroundAnniversary_ReturnsFullYears(string issued, string onDate, int expected)
        {

            var years = RentalMath.CalculateLicenseYears(DateTime.Parse(issued), DateTime.Parse(onDate));

            years.ShouldBe(expected);
        }

        [Theory]
        [Trait("Rule", "BR-03")]
                                                // ASSUME CURRENT TIME IS 2025-03-10 10:00
        // less than one day: minimum is 1
        [InlineData("2025-03-10 10:01", 1)]
        [InlineData("2025-03-10 11:00", 1)]
        [InlineData("2025-03-11 09:59", 1)]
        // exactly one day, and inside the grace hour
        [InlineData("2025-03-11 10:00", 1)]
        [InlineData("2025-03-11 10:59", 1)]
        // grace hour edge
        [InlineData("2025-03-11 11:00", 1)] // exactly 60 min doesnt count
        [InlineData("2025-03-11 11:01", 2)]
        //more than one day
        [InlineData("2025-03-12 10:00", 2)]
        [InlineData("2025-03-12 10:59", 2)]
        [InlineData("2025-03-12 11:00", 2)]
        [InlineData("2025-03-12 11:01", 3)]
        public void CalculateRentalDays_GraceHourBoundaries_ReturnsBilledDays(string returnAt, int expected)
        {
            var days = RentalMath.CalculateRentalDays(DateTime.Parse("2025-03-10 10:00"), DateTime.Parse(returnAt));

            days.ShouldBe(expected);
        }

        [Theory]
        [Trait("Rule", "BR-03")]
        [InlineData("2025-03-10 10:00")] // equal
        [InlineData("2025-03-10 09:59")] // one minute before
        [InlineData("2025-03-09 10:00")] // a day before
        public void CalculateRentalDays_ReturnNotAfterPickup_ThrowsArgumentException(string returnAt)
        {
            Action act = () => RentalMath.CalculateRentalDays(DateTime.Parse("2025-03-10 10:00"),DateTime.Parse(returnAt));

            act.Should()
               .Throw<ArgumentException>()
               .WithMessage("Return must be after pickup");
        }
    }

}

