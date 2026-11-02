using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FluentAssertions;
using NetArchTest.Rules;

namespace ParkSmart.ArchitectureTests
{
    public class ArchitectureTests
    {
        private static readonly Assembly Domain = Assembly.Load("ParkSmart.Domain");
        private static readonly Assembly Application = Assembly.Load("ParkSmart.Application");
        private static readonly Assembly Infrastructure = Assembly.Load("ParkSmart.Infrastructure");
        private static readonly Assembly UnitTests = Assembly.Load("ParkSmart.UnitTests");

        // these 2 are by AI
        private static readonly Regex TestMethodPattern = new(@"^[A-Z][A-Za-z0-9]*_[A-Z][A-Za-z0-9]*_[A-Z][A-Za-z0-9]*$", RegexOptions.Compiled);


        public static bool IsValidTestMethodName(string name) => TestMethodPattern.IsMatch(name);
        // =================


        [Fact]
        public void Domain_Dependencies_FollowArchitectureRules()
        {
            var result = Types.InAssembly(Domain)
                .ShouldNot()
                .HaveDependencyOnAny(
                    "ParkSmart.Application",
                    "ParkSmart.Infrastructure",
                    "ParkSmart.ConsoleApp")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }
        [Fact]
        public void Application_Dependencies_FollowArchitectureRules()
        {
            var result = Types.InAssembly(Application)
                .ShouldNot()
                .HaveDependencyOnAny(
                    "ParkSmart.Infrastructure",
                    "ParkSmart.ConsoleApp")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }
        [Fact]
        public void Infrastructure_Dependencies_FollowArchitectureRules()
        {
            var result = Types.InAssembly(Infrastructure)
                .ShouldNot()
                .HaveDependencyOn("ParkSmart.ConsoleApp")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }
        [Fact]

        public void TestNaming_AllRules_AreFollowed()
        {
            var errors = new List<string>();

            // R1: test classes end with "Tests" (NetArchTest checks types)
            var classResult = Types.InAssembly(UnitTests)
                .That()
                .ResideInNamespaceMatching(@"^ParkSmart\.UnitTests$")
                .And().AreClasses()
                .And().AreNotNested()
                .Should()
                .HaveNameEndingWith("Tests")
                .GetResult();

            if (!classResult.IsSuccessful)
                errors.AddRange((classResult.FailingTypeNames ?? Array.Empty<string>())
                    .Select(n => $"Class must end with 'Tests': {n}"));

            // I USED AI HERE
            // R2: every [Fact]/[Theory] matches Method_Scenario_ExpectedResult (reflection + regex)
            var badMethods = UnitTests.GetTypes()
                .SelectMany(t => t.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.DeclaredOnly))
                .Where(m => m.GetCustomAttributes(typeof(FactAttribute), inherit: true).Any())
                .Where(m => !IsValidTestMethodName(m.Name))
                .Select(m => $"Method must match Method_Scenario_ExpectedResult: {m.DeclaringType!.Name}.{m.Name}");

            errors.AddRange(badMethods);

            Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
        }
    }
}
