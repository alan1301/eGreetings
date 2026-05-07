using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EGreetings.Infrastructure.Persistence;

/// <summary>
/// Seeds mandatory default data on first run:
/// - System categories (BR-31: cannot delete)
/// - Default Admin account
/// - Sample greeting cards
/// </summary>
public static class AppDbContextSeed
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger)
    {
        try
        {
            await SeedCategoriesAsync(db, logger);
            await SeedAdminUserAsync(db, logger);
            await SeedSampleCardsAsync(db, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[SEED] Error seeding database: {Message}", ex.Message);
            throw;
        }
    }

    // ── 1. System Categories (BR-31: cannot delete) ──────────────────
    private static async Task SeedCategoriesAsync(AppDbContext db, ILogger logger)
    {
        if (await db.Categories.AnyAsync()) return;

        var categories = new[]
        {
            new Category
            {
                Id = new Guid("11111111-0000-0000-0000-000000000001"),
                Name = "Sinh Nhật",
                Slug = "sinh-nhat",
                Description = "Thiệp chúc mừng sinh nhật",
                IconUrl = "🎂",
                Color = "#FF6B6B",
                DisplayOrder = 1,
                IsSystem = true,
                Status = CategoryStatus.Active
            },
            new Category
            {
                Id = new Guid("11111111-0000-0000-0000-000000000002"),
                Name = "Đám Cưới",
                Slug = "dam-cuoi",
                Description = "Thiệp chúc mừng hôn lễ",
                IconUrl = "💍",
                Color = "#C9A96E",
                DisplayOrder = 2,
                IsSystem = true,
                Status = CategoryStatus.Active
            },
            new Category
            {
                Id = new Guid("11111111-0000-0000-0000-000000000003"),
                Name = "Năm Mới",
                Slug = "nam-moi",
                Description = "Thiệp chào đón năm mới",
                IconUrl = "🎉",
                Color = "#FFD700",
                DisplayOrder = 3,
                IsSystem = true,
                Status = CategoryStatus.Active
            },
            new Category
            {
                Id = new Guid("11111111-0000-0000-0000-000000000004"),
                Name = "Lễ Hội",
                Slug = "le-hoi",
                Description = "Thiệp các dịp lễ hội đặc biệt",
                IconUrl = "🎊",
                Color = "#7C3AED",
                DisplayOrder = 4,
                IsSystem = true,
                Status = CategoryStatus.Active
            },
            new Category
            {
                Id = new Guid("11111111-0000-0000-0000-000000000005"),
                Name = "Tốt Nghiệp",
                Slug = "tot-nghiep",
                Description = "Thiệp chúc mừng tốt nghiệp",
                IconUrl = "🎓",
                Color = "#059669",
                DisplayOrder = 5,
                IsSystem = false,
                Status = CategoryStatus.Active
            }
        };

        await db.Categories.AddRangeAsync(categories);
        await db.SaveChangesAsync();
        logger.LogInformation("[SEED] ✅ Seeded {Count} categories", categories.Length);
    }

    // ── 2. Default Admin Accounts ─────────────────────────────────────
    private static async Task SeedAdminUserAsync(AppDbContext db, ILogger logger)
    {
        // ── Admin 1: system account ──────────────────────────────────
        var adminEmail = "admin@e-greetings.com";
        if (!await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == adminEmail))
        {
            var admin = new User
            {
                Id = new Guid("AAAAAAAA-0000-0000-0000-000000000001"),
                FullName = "System Administrator",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456!", 12),
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                EmailVerificationToken = null,
                EmailVerificationTokenExpiry = null
            };
            await db.Users.AddAsync(admin);
            await db.SaveChangesAsync();
            logger.LogWarning("[SEED] ✅ Admin account created. Email: {Email}", adminEmail);
        }

        // ── Admin 2: secondary admin account ─────────────────────────
        var admin2Email = "admin@gmail.com";
        if (!await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == admin2Email))
        {
            var admin2 = new User
            {
                Id = new Guid("BBBBBBBB-0000-0000-0000-000000000002"),
                FullName = "Admin User",
                Email = admin2Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123@", 12),
                Role = UserRole.Admin,
                Status = UserStatus.Active,   // Active = email confirmed
                EmailVerificationToken = null,
                EmailVerificationTokenExpiry = null
            };
            await db.Users.AddAsync(admin2);
            await db.SaveChangesAsync();
            logger.LogWarning("[SEED] ✅ Admin2 account created. Email: {Email}", admin2Email);
        }
        else
        {
            // Ensure existing account has Admin role and is Active
            var existing = await db.Users.IgnoreQueryFilters().FirstAsync(u => u.Email == admin2Email);
            if (existing.Role != UserRole.Admin || existing.Status != UserStatus.Active)
            {
                existing.Role = UserRole.Admin;
                existing.Status = UserStatus.Active;
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123@", 12);
                await db.SaveChangesAsync();
                logger.LogWarning("[SEED] ✅ admin@gmail.com promoted to Admin role.");
            }
        }
    }

    // ── 3. Sample Greeting Cards ──────────────────────────────────────
    private static async Task SeedSampleCardsAsync(AppDbContext db, ILogger logger)
    {
        if (await db.GreetingCards.AnyAsync()) return;

        var cards = new[]
        {
            new GreetingCard
            {
                CategoryId = new Guid("11111111-0000-0000-0000-000000000001"),
                Name = "Happy Birthday Classic",
                Slug = "happy-birthday-classic",
                Description = "Mẫu thiệp sinh nhật cổ điển với nền hồng pastel",
                Tags = "sinh nhật,classic,hồng,bánh kem",
                ThumbnailUrl = "https://placehold.co/400x300/FF6B6B/white?text=Happy+Birthday",
                FileUrl = "https://placehold.co/800x600/FF6B6B/white?text=Happy+Birthday",
                IsFeatured = true,
                Status = CardStatus.Active
            },
            new GreetingCard
            {
                CategoryId = new Guid("11111111-0000-0000-0000-000000000001"),
                Name = "Birthday Fun Balloons",
                Slug = "birthday-fun-balloons",
                Description = "Thiệp sinh nhật vui tươi với nhiều bóng bay",
                Tags = "sinh nhật,bóng bay,vui tươi,màu sắc",
                ThumbnailUrl = "https://placehold.co/400x300/FFD700/333?text=Birthday+Balloons",
                FileUrl = "https://placehold.co/800x600/FFD700/333?text=Birthday+Balloons",
                IsFeatured = true,
                Status = CardStatus.Active
            },
            new GreetingCard
            {
                CategoryId = new Guid("11111111-0000-0000-0000-000000000002"),
                Name = "Elegant Wedding",
                Slug = "elegant-wedding",
                Description = "Thiệp đám cưới sang trọng với hoa hồng trắng",
                Tags = "đám cưới,sang trọng,hoa hồng,trắng",
                ThumbnailUrl = "https://placehold.co/400x300/C9A96E/white?text=Wedding",
                FileUrl = "https://placehold.co/800x600/C9A96E/white?text=Wedding",
                IsFeatured = true,
                Status = CardStatus.Active
            },
            new GreetingCard
            {
                CategoryId = new Guid("11111111-0000-0000-0000-000000000003"),
                Name = "Happy New Year 2025",
                Slug = "happy-new-year-2025",
                Description = "Thiệp chúc mừng năm mới với pháo hoa rực rỡ",
                Tags = "năm mới,pháo hoa,2025,chúc mừng",
                ThumbnailUrl = "https://placehold.co/400x300/7C3AED/white?text=Happy+New+Year",
                FileUrl = "https://placehold.co/800x600/7C3AED/white?text=Happy+New+Year",
                IsFeatured = true,
                Status = CardStatus.Active
            },
            new GreetingCard
            {
                CategoryId = new Guid("11111111-0000-0000-0000-000000000004"),
                Name = "Tết Nguyên Đán 2025",
                Slug = "tet-nguyen-dan-2025",
                Description = "Thiệp Tết truyền thống với hoa mai vàng rực rỡ",
                Tags = "tết,xuân,mai vàng,truyền thống",
                ThumbnailUrl = "https://placehold.co/400x300/ef4444/FFD700?text=Chuc+Mung+Nam+Moi",
                FileUrl = "https://placehold.co/800x600/ef4444/FFD700?text=Chuc+Mung+Nam+Moi",
                IsFeatured = true,
                Status = CardStatus.Active
            },
            new GreetingCard
            {
                CategoryId = new Guid("11111111-0000-0000-0000-000000000005"),
                Name = "Congratulations Graduate",
                Slug = "congratulations-graduate",
                Description = "Thiệp chúc mừng tốt nghiệp trang trọng",
                Tags = "tốt nghiệp,chúc mừng,học vị,mũ cử nhân",
                ThumbnailUrl = "https://placehold.co/400x300/059669/white?text=Congratulations",
                FileUrl = "https://placehold.co/800x600/059669/white?text=Congratulations",
                IsFeatured = false,
                Status = CardStatus.Active
            }
        };

        await db.GreetingCards.AddRangeAsync(cards);
        await db.SaveChangesAsync();
        logger.LogInformation("[SEED] ✅ Seeded {Count} sample greeting cards", cards.Length);
    }
}
