using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Storefront.Modules.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.CreateTable(
                name: "AppSettings",
                schema: "content",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "BlogPosts",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Slug = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Body = table.Column<string>(type: "text", nullable: false),
                    FeaturedImage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Author = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SeoMetaTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SeoMetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SeoKeywords = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SeoOgImage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SeoOgType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SeoCanonicalUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeaturedBrands",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturedBrands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HeroSlides",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    LinkText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroSlides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HomeCategorySlides",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProductCount = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeCategorySlides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaticPages",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Slug = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SeoMetaTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SeoMetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SeoKeywords = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SeoOgImage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SeoOgType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SeoCanonicalUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaticPages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_Category",
                schema: "content",
                table: "AppSettings",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Category",
                schema: "content",
                table: "BlogPosts",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_IsPublished",
                schema: "content",
                table: "BlogPosts",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_PublishedAt",
                schema: "content",
                table: "BlogPosts",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_Slug",
                schema: "content",
                table: "BlogPosts",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeaturedBrands_DisplayOrder",
                schema: "content",
                table: "FeaturedBrands",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_HeroSlides_DisplayOrder",
                schema: "content",
                table: "HeroSlides",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_HomeCategorySlides_DisplayOrder",
                schema: "content",
                table: "HomeCategorySlides",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_StaticPages_DisplayOrder",
                schema: "content",
                table: "StaticPages",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_StaticPages_IsPublished",
                schema: "content",
                table: "StaticPages",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_StaticPages_Slug",
                schema: "content",
                table: "StaticPages",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings",
                schema: "content");

            migrationBuilder.DropTable(
                name: "BlogPosts",
                schema: "content");

            migrationBuilder.DropTable(
                name: "FeaturedBrands",
                schema: "content");

            migrationBuilder.DropTable(
                name: "HeroSlides",
                schema: "content");

            migrationBuilder.DropTable(
                name: "HomeCategorySlides",
                schema: "content");

            migrationBuilder.DropTable(
                name: "StaticPages",
                schema: "content");
        }
    }
}
