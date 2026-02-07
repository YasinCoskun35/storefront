using FluentAssertions;
using NSubstitute;
using Storefront.Modules.Content.Core.Application.Interfaces;
using Storefront.Modules.Content.Core.Domain.Entities;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.Modules.Content.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Storefront.UnitTests.Content.Application;

public class SlugServiceTests
{
    private readonly ContentDbContext _context;
    private readonly SlugService _slugService;

    public SlugServiceTests()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _slugService = new SlugService(_context);
    }

    [Fact]
    public async Task GenerateSlug_Should_Convert_Title_To_Lowercase_With_Dashes()
    {
        // Arrange
        var title = "My New Drill Guide";

        // Act
        var slug = await _slugService.GenerateUniqueSlugAsync(title);

        // Assert
        slug.Should().Be("my-new-drill-guide");
    }

    [Fact]
    public async Task GenerateSlug_Should_Remove_Special_Characters()
    {
        // Arrange
        var title = "Top 10 Drills! (2024 Edition)";

        // Act
        var slug = await _slugService.GenerateUniqueSlugAsync(title);

        // Assert
        slug.Should().Be("top-10-drills-2024-edition");
    }

    [Fact]
    public async Task GenerateSlug_Should_Replace_Multiple_Spaces_With_Single_Dash()
    {
        // Arrange
        var title = "Best    Drill    Bits";

        // Act
        var slug = await _slugService.GenerateUniqueSlugAsync(title);

        // Assert
        slug.Should().Be("best-drill-bits");
    }

    [Fact]
    public async Task GenerateSlug_Should_Append_Number_If_Slug_Exists()
    {
        // Arrange
        var existingPost = new BlogPost
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Drill Guide",
            Slug = "drill-guide",
            Body = "Content",
            IsPublished = true
        };
        _context.BlogPosts.Add(existingPost);
        await _context.SaveChangesAsync();

        // Act
        var slug = await _slugService.GenerateUniqueSlugAsync("Drill Guide");

        // Assert
        slug.Should().Be("drill-guide-1");
    }

    [Fact]
    public async Task GenerateSlug_Should_Increment_Number_For_Multiple_Duplicates()
    {
        // Arrange
        var post1 = new BlogPost
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Drill Guide",
            Slug = "drill-guide",
            Body = "Content",
            IsPublished = true
        };
        var post2 = new BlogPost
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Drill Guide 1",
            Slug = "drill-guide-1",
            Body = "Content",
            IsPublished = true
        };
        _context.BlogPosts.AddRange(post1, post2);
        await _context.SaveChangesAsync();

        // Act
        var slug = await _slugService.GenerateUniqueSlugAsync("Drill Guide");

        // Assert
        slug.Should().Be("drill-guide-2");
    }

    [Theory]
    [InlineData("", "untitled")]
    [InlineData("   ", "untitled")]
    [InlineData(null, "untitled")]
    public async Task GenerateSlug_Should_Handle_Empty_Or_Null_Titles(string? title, string expected)
    {
        // Act
        var slug = await _slugService.GenerateUniqueSlugAsync(title!);

        // Assert
        slug.Should().Be(expected);
    }
}

