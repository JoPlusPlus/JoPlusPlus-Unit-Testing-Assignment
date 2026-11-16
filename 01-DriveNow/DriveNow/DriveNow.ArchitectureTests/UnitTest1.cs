using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FluentAssertions;
using NetArchTest.Rules;

namespace DriveNow.ArchitectureTests
{
    public class ArchitectureTests
    {
        private static readonly Assembly Domain = Assembly.Load("DriveNow.Domain");
        private static readonly Assembly Application = Assembly.Load("DriveNow.Application");
        private static readonly Assembly Infrastructure = Assembly.Load("DriveNow.Infrastructure");
        private static readonly Assembly UnitTests = Assembly.Load("DriveNow.UnitTests");

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
                    "DriveNow.Application",
                    "DriveNow.Infrastructure",
                    "DriveNow.ConsoleApp")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }
        [Fact]
        public void Application_Dependencies_FollowArchitectureRules()
        {
            var result = Types.InAssembly(Application)
                .ShouldNot()
                .HaveDependencyOnAny(
                    "DriveNow.Infrastructure",
                    "DriveNow.ConsoleApp")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }
        [Fact]
        public void Infrastructure_Dependencies_FollowArchitectureRules()
        {
            var result = Types.InAssembly(Infrastructure)
                .ShouldNot()
                .HaveDependencyOn("DriveNow.ConsoleApp")
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void ProductionTypes_Naming_FollowConventions()
        {
            var productionTypes = new[] { Domain, Application, Infrastructure }

            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass || type.IsInterface)
            .ToList();

            // Interfaces must start with I
            productionTypes
                .Where(type => type.IsInterface)
                .Should()
                .OnlyContain(type => type.Name.StartsWith("I"));

            // Repositories must end with Repository
            // and live in a Repositories namespace
            productionTypes
                .Where(type => type.Name.EndsWith("Repository") ||
                               (type.Namespace != null &&
                                type.Namespace.Contains(".Repositories")))
                .Should()
                .OnlyContain(type =>
                    type.Name.EndsWith("Repository") &&
                    type.Namespace != null &&
                    type.Namespace.Contains(".Repositories"));

            // Services must end with Service
            // and live in a Services namespace
            productionTypes
                .Where(type => type.Name.EndsWith("Services") ||
                               (type.Namespace != null &&
                                type.Namespace.Contains(".Services")))
                .Should()
                .OnlyContain(type =>
                    type.Name.EndsWith("Services") &&
                    type.Namespace != null &&
                    type.Namespace.Contains(".Services"));

            // Exceptions must end with Exception
            // and live in DriveNow.Domain.Exceptions
            productionTypes
                .Where(type =>
                    type.IsClass &&
                    typeof(Exception).IsAssignableFrom(type) &&
                    type != typeof(Exception))
                .Should()
                .OnlyContain(type =>
                    type.Name.EndsWith("Exception") &&
                    type.Namespace == "DriveNow.Domain.Exceptions");
        }
        [Fact]

        public void TestNaming_AllRules_AreFollowed()
        {
            var errors = new List<string>();

            // R1: test classes end with "Tests" (NetArchTest checks types)
            var classResult = Types.InAssembly(UnitTests)
                .That()
                .ResideInNamespaceMatching(@"^DriveNow\.UnitTests$")
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
