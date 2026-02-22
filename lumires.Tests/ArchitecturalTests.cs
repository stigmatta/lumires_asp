using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using FastEndpoints;
using FluentAssertions;
using Infrastructure;
using lumires.Api;
using lumires.Domain;
using NetArchTest.Rules;

namespace Tests;

internal sealed class ArchitecturalTests
{
    private static string InfraNamespace => typeof(InfraRegistration).Namespace!;
    private static string TestsNamespace => typeof(ArchitecturalTests).Namespace!;
    private static string ApiNamespace => typeof(ApiRegistration).Namespace!;
    private static string DomainNamespace => typeof(NamespaceMarker).Namespace!;
    private static string CoreNamespace => typeof(lumires.Core.NamespaceMarker).Namespace!;


    private static string AnchoredNamespace(string ns)
    {
        return $"^{Regex.Escape(ns)}(\\.|$)";
    }

    [Test]
    public void ApiTests_Should_Not_Be_Dependant_On_Infrastructure()
    {
        var result = Types.InAssembly(typeof(ApiTests.NamespaceMarker).Assembly)
            .That()
            .ResideInNamespaceMatching(AnchoredNamespace(typeof(ApiTests.NamespaceMarker).Namespace!))
            .ShouldNot()
            .HaveDependencyOn(InfraNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Failed types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Test]
    public void Api_Should_Not_Be_Dependant_On_Infrastructure_And_Tests()
    {
        var result = Types.InAssembly(typeof(ApiRegistration).Assembly)
            .That()
            .ResideInNamespaceMatching(AnchoredNamespace(ApiNamespace))
            .ShouldNot()
            .HaveDependencyOnAny(InfraNamespace, TestsNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"API layer should not reference Infrastructure or Tests. " +
            $"Failed types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Test]
    public void Infrastructure_Should_Not_Be_Dependant_On_Other_Than_Core_And_Domain()
    {
        var result = Types.InAssembly(typeof(InfraRegistration).Assembly)
            .That()
            .ResideInNamespaceMatching(AnchoredNamespace(InfraNamespace))
            .ShouldNot()
            .HaveDependencyOnAny(ApiNamespace, TestsNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Infra layer should not reference Api or Tests. " +
            $"Failed types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }


    [Test]
    public void Domain_Should_Not_Be_Dependant_On_Any()
    {
        var result = Types.InAssembly(typeof(NamespaceMarker).Assembly)
            .That()
            .ResideInNamespaceMatching(AnchoredNamespace(DomainNamespace))
            .ShouldNot()
            .HaveDependencyOnAny(ApiNamespace, InfraNamespace, TestsNamespace, CoreNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not reference anything. " +
            $"Failed types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Test]
    public void Core_Should_Not_Depend_On_Outer_Layers()
    {
        var result = Types.InAssembly(typeof(lumires.Core.NamespaceMarker).Assembly)
            .That()
            .ResideInNamespaceMatching(AnchoredNamespace(CoreNamespace))
            .ShouldNot()
            .HaveDependencyOnAny(ApiNamespace, InfraNamespace, TestsNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Core layer should not reference Api, Infra or Tests. " +
            $"Failed types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Test]
    public void Api_Endpoints_Should_Be_Sealed()
    {
        var result = Types.InAssembly(typeof(ApiRegistration).Assembly)
            .That()
            .Inherit(typeof(EndpointWithoutRequest))
            .Or()
            .Inherit(typeof(EndpointWithoutRequest<>))
            .Or()
            .Inherit(typeof(Endpoint<>))
            .Or()
            .Inherit(typeof(Endpoint<,>))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"All endpoints should be sealed. " +
            $"Failed types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Test]
    public void Api_Endpoints_Should_Not_Be_Public()
    {
        var result = Types.InAssembly(typeof(ApiRegistration).Assembly)
            .That()
            .ImplementInterface(typeof(IEndpoint))
            .Should()
            .NotBePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Endpoints should not be public. " +
            $"Failed types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Test]
    [RequiresUnreferencedCode("Test code, trimming not applicable")]
    public void Api_Endpoints_Should_Be_Named_EndpointOrProperlyNamed()
    {
        var assembly = typeof(ApiRegistration).Assembly;

        var endpointTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(IEndpoint).IsAssignableFrom(t))
            .ToList();

        endpointTypes.Should().OnlyContain(t => t.Name == "Endpoint",
            $"All endpoint classes should be named 'Endpoint'. Found: {string.Join(", ", endpointTypes.Select(t => t.Name))}");
    }

    [Test]
    [RequiresUnreferencedCode("Test code, trimming not applicable")]
    public void Api_Endpoints_Should_Be_Located_In_Features()
    {
        var assembly = typeof(ApiRegistration).Assembly;

        var endpointTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(IEndpoint).IsAssignableFrom(t))
            .ToList();

        endpointTypes.Should().OnlyContain(
            t => t.Namespace != null && t.Namespace.Contains(".Features."),
            $"All endpoint classes should reside in a Features namespace. " +
            $"Found: {string.Join(", ", endpointTypes.Select(t => t.FullName))}");
    }


    [Test]
    public void Core_Common_Should_Be_Public()
    {
        var result = Types.InAssembly(typeof(lumires.Core.NamespaceMarker).Assembly)
            .That()
            .AreClasses()
            .And()
            .DoNotResideInNamespace(CoreNamespace + ".Resources")
            .Should()
            .BePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Core common models/abstractions should  be public. " +
            $"Failed types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Test]
    public void Core_Interfaces_Should_Reside_In_Abstractions()
    {
        var result = Types.InAssembly(typeof(lumires.Core.NamespaceMarker).Assembly)
            .That()
            .AreInterfaces()
            .Should()
            .ResideInNamespace(CoreNamespace + ".Abstractions")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Core interfaces should live in Abstractions namespace. " +
            $"Failed types: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }

    [Test]
    [RequiresUnreferencedCode("Test code, trimming not applicable")]
    public void Api_Endpoints_Should_Have_Summary_Class()
    {
        var assembly = typeof(ApiRegistration).Assembly;

        var endpointTypes = assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IEndpoint)) && t is { IsAbstract: false, IsInterface: false })
            .ToList();

        var summaryTypes = assembly.GetTypes()
            .Where(t => t.BaseType is { IsGenericType: true } &&
                        t.BaseType.GetGenericTypeDefinition() == typeof(Summary<>))
            .Select(t => t.BaseType!.GetGenericArguments()[0])
            .ToHashSet();

        var endpointsWithoutSummary = endpointTypes
            .Where(t => !summaryTypes.Contains(t))
            .ToList();

        endpointsWithoutSummary.Should().BeEmpty(
            $"Every endpoint should have a Summary class. " +
            $"Missing: {string.Join(", ", endpointsWithoutSummary.Select(t => t.FullName))}");
    }

    [Test]
    [RequiresUnreferencedCode("Test code, trimming not applicable")]
    public void Core_Service_Interfaces_Should_Be_Implemented_In_Infrastructure()
    {
        var coreAssembly = typeof(lumires.Core.NamespaceMarker).Assembly;
        var infraAssembly = typeof(InfraRegistration).Assembly;

        var serviceInterfaces = coreAssembly.GetTypes()
            .Where(t => t.IsInterface &&
                        t.Namespace!.StartsWith(CoreNamespace + ".Abstractions.Services"))
            .ToList();

        var infraTypes = infraAssembly.GetTypes()
            .Where(t => t.Namespace?.StartsWith(InfraNamespace + ".Services") == true)
            .ToList();

        var unimplemented = serviceInterfaces
            .Where(iface =>
                !infraTypes.Any(t => iface.IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false }))
            .ToList();

        unimplemented.Should().BeEmpty(
            $"All Core service interfaces should be implemented in Infrastructure.Services. " +
            $"Missing implementations: {string.Join(", ", unimplemented.Select(t => t.FullName))}");
    }
}