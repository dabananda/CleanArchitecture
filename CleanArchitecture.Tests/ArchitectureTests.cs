using CleanArchitecture.Api.Controllers;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Infrastructure.Persistence;
using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace CleanArchitecture.Tests;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(ApplicationUser).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(CleanArchitecture.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;
    private static readonly Assembly ApiAssembly = typeof(AuthController).Assembly;

    [Fact]
    public void Domain_Should_Not_DependOn_OtherLayers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "CleanArchitecture.Application",
                "CleanArchitecture.Infrastructure",
                "CleanArchitecture.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result, "Domain must not depend on Application, Infrastructure, or Api."));
    }

    [Fact]
    public void Application_Should_Not_DependOn_InfrastructureOrApi()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "CleanArchitecture.Infrastructure",
                "CleanArchitecture.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result, "Application must not depend on Infrastructure or Api."));
    }

    [Fact]
    public void Application_Should_Not_DependOn_EntityFrameworkCore()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result, "Application must have zero EF Core dependency."));
    }

    [Fact]
    public void Infrastructure_Should_Not_DependOn_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn("CleanArchitecture.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result, "Infrastructure must not depend on Api."));
    }

    [Fact]
    public void Domain_Should_Not_DependOn_EntityFrameworkCore()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result, "Domain must have zero EF Core dependency."));
    }

    [Fact]
    public void Domain_Should_Not_DependOn_AspNetCore()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result, "Domain must have zero ASP.NET Core dependency."));
    }

    [Fact]
    public void Handlers_Should_Be_Sealed_Or_Internal_Implementation_Detail()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .ResideInNamespaceMatching(@"\.Features\.")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(BuildFailureMessage(result, "Handlers must live under a Features.* namespace."));
    }

    private static string BuildFailureMessage(TestResult result, string context)
    {
        var offenders = result.FailingTypeNames is null
            ? "(no type names reported)"
            : string.Join(Environment.NewLine, result.FailingTypeNames);

        return $"{context}{Environment.NewLine}Offending types:{Environment.NewLine}{offenders}";
    }
}
