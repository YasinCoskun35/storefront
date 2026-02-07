using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;

namespace Storefront.ArchitectureTests;

public class ArchitectureTests
{
    private static readonly Assembly IdentityAssembly = typeof(Modules.Identity.IdentityModuleExtensions).Assembly;
    private static readonly Assembly CatalogAssembly = typeof(Modules.Catalog.CatalogModuleExtensions).Assembly;
    private static readonly Assembly ContentAssembly = typeof(Modules.Content.ContentModuleExtensions).Assembly;
    private static readonly Assembly SharedKernelAssembly = typeof(SharedKernel.Result<>).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        // Domain layer (Core/Domain) should not reference Infrastructure
        var result = Types.InAssemblies(new[] { CatalogAssembly, ContentAssembly })
            .That()
            .ResideInNamespace("*.Core.Domain.*")
            .ShouldNot()
            .HaveDependencyOn("*.Infrastructure.*")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on Infrastructure layer");
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        // Domain should not depend on Application layer
        var result = Types.InAssemblies(new[] { CatalogAssembly, ContentAssembly })
            .That()
            .ResideInNamespace("*.Core.Domain.*")
            .ShouldNot()
            .HaveDependencyOn("*.Core.Application.*")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain layer should not depend on Application layer");
    }

    [Fact]
    public void Modules_Should_Not_Reference_Each_Other()
    {
        // Identity module should not reference Catalog or Content
        var identityResult = Types.InAssembly(IdentityAssembly)
            .Should()
            .NotHaveDependencyOn("Storefront.Modules.Catalog")
            .And()
            .NotHaveDependencyOn("Storefront.Modules.Content")
            .GetResult();

        identityResult.IsSuccessful.Should().BeTrue(
            "Identity module should not reference other modules");

        // Catalog module should not reference Identity or Content
        var catalogResult = Types.InAssembly(CatalogAssembly)
            .Should()
            .NotHaveDependencyOn("Storefront.Modules.Identity")
            .And()
            .NotHaveDependencyOn("Storefront.Modules.Content")
            .GetResult();

        catalogResult.IsSuccessful.Should().BeTrue(
            "Catalog module should not reference other modules");

        // Content module should not reference Identity or Catalog
        var contentResult = Types.InAssembly(ContentAssembly)
            .Should()
            .NotHaveDependencyOn("Storefront.Modules.Identity")
            .And()
            .NotHaveDependencyOn("Storefront.Modules.Catalog")
            .GetResult();

        contentResult.IsSuccessful.Should().BeTrue(
            "Content module should not reference other modules");
    }

    [Fact]
    public void Commands_Should_Implement_IRequest()
    {
        // All classes ending with "Command" should implement IRequest
        var result = Types.InAssemblies(new[] { IdentityAssembly, CatalogAssembly, ContentAssembly })
            .That()
            .HaveNameEndingWith("Command")
            .And()
            .ResideInNamespace("*.Application.Commands")
            .Should()
            .ImplementInterface(typeof(MediatR.IRequest<>))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "All Command classes should implement IRequest<T>");
    }

    [Fact]
    public void Queries_Should_Implement_IRequest()
    {
        // All classes ending with "Query" should implement IRequest
        var result = Types.InAssemblies(new[] { IdentityAssembly, CatalogAssembly, ContentAssembly })
            .That()
            .HaveNameEndingWith("Query")
            .And()
            .ResideInNamespace("*.Application.Queries")
            .Should()
            .ImplementInterface(typeof(MediatR.IRequest<>))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "All Query classes should implement IRequest<T>");
    }

    [Fact]
    public void Handlers_Should_Have_Correct_Naming_Convention()
    {
        // Command handlers should end with "CommandHandler"
        var commandHandlerResult = Types.InAssemblies(new[] { IdentityAssembly, CatalogAssembly, ContentAssembly })
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .And()
            .ResideInNamespace("*.Application.Commands")
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult();

        commandHandlerResult.IsSuccessful.Should().BeTrue(
            "Command handlers should end with 'CommandHandler'");

        // Query handlers should end with "QueryHandler"
        var queryHandlerResult = Types.InAssemblies(new[] { IdentityAssembly, CatalogAssembly, ContentAssembly })
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .And()
            .ResideInNamespace("*.Application.Queries")
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        queryHandlerResult.IsSuccessful.Should().BeTrue(
            "Query handlers should end with 'QueryHandler'");
    }

    [Fact]
    public void Validators_Should_Have_Correct_Naming_Convention()
    {
        // Validators should end with "Validator"
        var result = Types.InAssemblies(new[] { IdentityAssembly, CatalogAssembly, ContentAssembly })
            .That()
            .Inherit(typeof(FluentValidation.AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "FluentValidation validators should end with 'Validator'");
    }

    [Fact]
    public void DbContexts_Should_Be_In_Infrastructure_Persistence()
    {
        // All DbContext classes should be in Infrastructure.Persistence namespace
        var result = Types.InAssemblies(new[] { IdentityAssembly, CatalogAssembly, ContentAssembly })
            .That()
            .Inherit(typeof(Microsoft.EntityFrameworkCore.DbContext))
            .Should()
            .ResideInNamespace("*.Infrastructure.Persistence")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "DbContext classes should be in Infrastructure.Persistence namespace");
    }

    [Fact]
    public void Controllers_Should_Be_In_API_Namespace()
    {
        // All controllers should be in API.Controllers namespace
        var result = Types.InAssemblies(new[] { IdentityAssembly, CatalogAssembly, ContentAssembly })
            .That()
            .Inherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
            .Should()
            .ResideInNamespace("*.API.Controllers")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Controllers should be in API.Controllers namespace");
    }

    [Fact]
    public void Application_Layer_Should_Not_Depend_On_Infrastructure()
    {
        // Application layer should only depend on Domain and SharedKernel
        var result = Types.InAssemblies(new[] { CatalogAssembly, ContentAssembly })
            .That()
            .ResideInNamespace("*.Core.Application.*")
            .ShouldNot()
            .HaveDependencyOn("*.Infrastructure.*")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Application layer should not depend on Infrastructure layer");
    }
}

