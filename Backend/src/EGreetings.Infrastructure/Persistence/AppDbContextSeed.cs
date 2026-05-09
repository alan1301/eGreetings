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
        // 1. Wipe existing data to allow fresh seed
        if (await db.GreetingCards.AnyAsync())
        {
            db.EmailRetryQueues.RemoveRange(db.EmailRetryQueues);
            db.Drafts.RemoveRange(db.Drafts);
            db.GreetingTransactions.RemoveRange(db.GreetingTransactions);
            db.GreetingCards.RemoveRange(db.GreetingCards);
            await db.SaveChangesAsync();
            logger.LogInformation("[SEED] Wiped existing cards and transactions for a clean slate.");
        }

        // Guids for categories
        var catBirthday = new Guid("11111111-0000-0000-0000-000000000001");
        var catWedding = new Guid("11111111-0000-0000-0000-000000000002");
        var catNewYear = new Guid("11111111-0000-0000-0000-000000000003");
        var catFestival = new Guid("11111111-0000-0000-0000-000000000004");
        var catGraduation = new Guid("11111111-0000-0000-0000-000000000005");

        var cards = new[]
        {
            // Birthday
            new GreetingCard { CategoryId = catBirthday, Name = "Golden Birthday Celebration", Slug = "golden-birthday", Description = "A luxurious birthday card with a golden background.", Tags = "birthday,gold,celebration", ThumbnailUrl = "https://images.unsplash.com/photo-1618501258602-0e3a6aef912e?w=800", FileUrl = "https://images.unsplash.com/photo-1618501258602-0e3a6aef912e?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-ny-gold\", \"fontFamily\":\"'Palatino Linotype', serif\", \"textColor\":\"#3d2900\", \"heading\":\"Happy Birthday!\", \"message\":\"Wishing you a golden year filled with joy and success.\"}" },
            new GreetingCard { CategoryId = catBirthday, Name = "Sweet Strawberry Cake", Slug = "sweet-cake", Description = "A sweet birthday wish featuring a vibrant pink gradient.", Tags = "birthday,sweet,pink", ThumbnailUrl = null, FileUrl = null, IsFeatured = false, IsPremium = false, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"bday-pink\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#9d174d\", \"heading\":\"Sweet Birthday\", \"message\":\"May your special day be as sweet as you are.\"}" },
            new GreetingCard { CategoryId = catBirthday, Name = "Birthday Sparklers", Slug = "bday-sparklers", Description = "Celebrate your special day with bright and cheerful sparklers.", Tags = "birthday,sparklers,party", ThumbnailUrl = null, FileUrl = null, IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"prm-noir\", \"fontFamily\":\"Georgia, serif\", \"textColor\":\"#facc15\", \"heading\":\"Time to Sparkle\", \"message\":\"Shine bright on your birthday!\"}" },
            new GreetingCard { CategoryId = catBirthday, Name = "Wrapped With Love", Slug = "wrapped-with-love", Description = "Send beautifully wrapped gifts digitally with balloons.", Tags = "birthday,gift,love,balloons", ThumbnailUrl = "https://images.unsplash.com/photo-1530103862676-de8892bf309c?w=800", FileUrl = "https://images.unsplash.com/photo-1530103862676-de8892bf309c?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-bday-balloons\", \"fontFamily\":\"'Noto Serif', serif\", \"textColor\":\"#1e3a8a\", \"heading\":\"Sent with Love\", \"message\":\"Wishing you the happiest of birthdays.\"}" },
            new GreetingCard { CategoryId = catBirthday, Name = "Midnight Party", Slug = "midnight-party", Description = "An exciting midnight birthday celebration card with confetti.", Tags = "birthday,night,party,confetti", ThumbnailUrl = "https://images.unsplash.com/photo-1492684223066-81342ee5ff30?w=800", FileUrl = "https://images.unsplash.com/photo-1492684223066-81342ee5ff30?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-bday-confetti\", \"fontFamily\":\"'Courier New', monospace\", \"textColor\":\"#1f1b15\", \"heading\":\"Let's Celebrate!\", \"message\":\"Time to party and have fun!\"}" },

            // Wedding
            new GreetingCard { CategoryId = catWedding, Name = "Classic White Wedding", Slug = "classic-white-wedding", Description = "An elegant white-themed wedding invitation featuring white silk.", Tags = "wedding,white,classic,silk", ThumbnailUrl = "https://images.unsplash.com/photo-1601662528567-526cd06f6582?w=800", FileUrl = "https://images.unsplash.com/photo-1601662528567-526cd06f6582?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-wed-silk\", \"fontFamily\":\"'Palatino Linotype', serif\", \"textColor\":\"#1a1a1a\", \"heading\":\"To the Happy Couple\", \"message\":\"Wishing you a lifetime of love and happiness.\"}" },
            new GreetingCard { CategoryId = catWedding, Name = "Eternal Rings", Slug = "eternal-rings", Description = "A symbol of eternal love with an elegant ivory background.", Tags = "wedding,rings,love,ivory", ThumbnailUrl = null, FileUrl = null, IsFeatured = false, IsPremium = false, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"wed-ivory\", \"fontFamily\":\"Georgia, serif\", \"textColor\":\"#5c4000\", \"heading\":\"Eternal Love\", \"message\":\"Congratulations on your beautiful union!\"}" },
            new GreetingCard { CategoryId = catWedding, Name = "Floral Romance", Slug = "floral-romance", Description = "A romantic wedding card adorned with abstract flowers.", Tags = "wedding,floral,romance", ThumbnailUrl = "https://images.unsplash.com/photo-1518531933037-91b2f5f229cc?w=800", FileUrl = "https://images.unsplash.com/photo-1518531933037-91b2f5f229cc?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-wed-floral\", \"fontFamily\":\"'Noto Serif', serif\", \"textColor\":\"#ffffff\", \"heading\":\"Forever Romance\", \"message\":\"May your love continue to blossom.\"}" },
            new GreetingCard { CategoryId = catWedding, Name = "The Happy Couple", Slug = "happy-couple", Description = "Celebrate the beautiful union of the happy couple.", Tags = "wedding,couple,marriage", ThumbnailUrl = null, FileUrl = null, IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"wed-rose\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#852d18\", \"heading\":\"Best Wishes\", \"message\":\"May your love grow stronger each day.\"}" },
            new GreetingCard { CategoryId = catWedding, Name = "Rustic Wedding Banquet", Slug = "rustic-wedding", Description = "A rustic and warm themed wedding card.", Tags = "wedding,rustic,banquet", ThumbnailUrl = null, FileUrl = null, IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"wed-sage\", \"fontFamily\":\"'Courier New', monospace\", \"textColor\":\"#166534\", \"heading\":\"Cheers to You\", \"message\":\"Wishing you the best on your special day.\"}" },

            // New Year
            new GreetingCard { CategoryId = catNewYear, Name = "Spectacular Fireworks", Slug = "spectacular-fireworks", Description = "Welcome the new year with spectacular and colorful fireworks.", Tags = "newyear,fireworks,colorful", ThumbnailUrl = "https://images.unsplash.com/photo-1543007630-9710e4a00a20?w=800", FileUrl = "https://images.unsplash.com/photo-1543007630-9710e4a00a20?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-ny-fireworks\", \"fontFamily\":\"'Palatino Linotype', serif\", \"textColor\":\"#ffffff\", \"heading\":\"Happy New Year\", \"message\":\"May this year bring new happiness and goals.\"}" },
            new GreetingCard { CategoryId = catNewYear, Name = "Midnight Countdown", Slug = "midnight-countdown", Description = "The final seconds before a brand new chapter begins.", Tags = "newyear,countdown,midnight", ThumbnailUrl = null, FileUrl = null, IsFeatured = false, IsPremium = false, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"ny-midnight\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#f5e8c7\", \"heading\":\"Midnight Countdown\", \"message\":\"A toast to the future!\"}" },
            new GreetingCard { CategoryId = catNewYear, Name = "Sparkling Celebration", Slug = "sparkling-celebration", Description = "A sparkling new year greeting with golden lights.", Tags = "newyear,sparkling,gold", ThumbnailUrl = "https://images.unsplash.com/photo-1618501258602-0e3a6aef912e?w=800", FileUrl = "https://images.unsplash.com/photo-1618501258602-0e3a6aef912e?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-ny-gold\", \"fontFamily\":\"Georgia, serif\", \"textColor\":\"#3d2900\", \"heading\":\"Celebrate 2026\", \"message\":\"Let the celebration begin!\"}" },
            new GreetingCard { CategoryId = catNewYear, Name = "Cheers to the Future", Slug = "cheers-future", Description = "Raise a glass and toast to a wonderful future.", Tags = "newyear,cheers,champagne", ThumbnailUrl = null, FileUrl = null, IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"ny-gold\", \"fontFamily\":\"'Noto Serif', serif\", \"textColor\":\"#ffffff\", \"heading\":\"Cheers to the Future\", \"message\":\"Wishing you joy and prosperity.\"}" },
            new GreetingCard { CategoryId = catNewYear, Name = "Festive Decorations", Slug = "festive-decor", Description = "Beautiful decorations to ring in the new year.", Tags = "newyear,decorations,festive", ThumbnailUrl = null, FileUrl = null, IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"ny-red\", \"fontFamily\":\"'Courier New', monospace\", \"textColor\":\"#fdf0ee\", \"heading\":\"Festive Greetings\", \"message\":\"Embrace the new beginnings.\"}" },

            // Festival
            new GreetingCard { CategoryId = catFestival, Name = "Merry Christmas Tree", Slug = "merry-christmas", Description = "A heartwarming Christmas greeting with holiday magic.", Tags = "festival,christmas,magic", ThumbnailUrl = "https://images.unsplash.com/photo-1543364195-bfe6e4932397?w=800", FileUrl = "https://images.unsplash.com/photo-1543364195-bfe6e4932397?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-fest-xmas\", \"fontFamily\":\"Georgia, serif\", \"textColor\":\"#ffffff\", \"heading\":\"Merry Christmas\", \"message\":\"Warm wishes for the holidays.\"}" },
            new GreetingCard { CategoryId = catFestival, Name = "Spooky Halloween", Slug = "spooky-halloween", Description = "Send some spooky and fun Halloween wishes.", Tags = "festival,halloween,spooky", ThumbnailUrl = "https://images.unsplash.com/photo-1508361001413-7a9dca21d08a?w=800", FileUrl = "https://images.unsplash.com/photo-1508361001413-7a9dca21d08a?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-fest-halloween\", \"fontFamily\":\"'Courier New', monospace\", \"textColor\":\"#facc15\", \"heading\":\"Happy Halloween\", \"message\":\"Have a spooktacular night!\"}" },
            new GreetingCard { CategoryId = catFestival, Name = "Lantern Festival", Slug = "lantern-festival", Description = "A magical night of glowing lanterns.", Tags = "festival,lanterns,magical", ThumbnailUrl = null, FileUrl = null, IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"fest-autumn\", \"fontFamily\":\"'Noto Serif', serif\", \"textColor\":\"#623e20\", \"heading\":\"Festival of Lights\", \"message\":\"May your life be illuminated with joy.\"}" },
            new GreetingCard { CategoryId = catFestival, Name = "Thanksgiving Harvest", Slug = "thanksgiving", Description = "Give thanks with this beautiful autumn harvest card.", Tags = "festival,thanksgiving,autumn", ThumbnailUrl = "https://images.unsplash.com/photo-1507369512168-9b7e7a57fa87?w=800", FileUrl = "https://images.unsplash.com/photo-1507369512168-9b7e7a57fa87?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-fest-thanksgiving\", \"fontFamily\":\"'Palatino Linotype', serif\", \"textColor\":\"#3d2900\", \"heading\":\"Happy Thanksgiving\", \"message\":\"Grateful for you today and always.\"}" },
            new GreetingCard { CategoryId = catFestival, Name = "Colorful Carnival", Slug = "colorful-carnival", Description = "Join the vibrant and colorful carnival celebration.", Tags = "festival,carnival,vibrant", ThumbnailUrl = null, FileUrl = null, IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"fest-spring\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#9d174d\", \"heading\":\"Carnival Joy\", \"message\":\"Enjoy the vibrant celebration.\"}" },

            // Graduation
            new GreetingCard { CategoryId = catGraduation, Name = "Graduation Cap & Scroll", Slug = "graduation-cap", Description = "A classic graduation congratulation card with academic wood.", Tags = "graduation,cap,achievement,wood", ThumbnailUrl = "https://images.unsplash.com/photo-1517646287270-a5a9ca602e5c?w=800", FileUrl = "https://images.unsplash.com/photo-1517646287270-a5a9ca602e5c?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-grad-wood\", \"fontFamily\":\"'Courier New', monospace\", \"textColor\":\"#ffffff\", \"heading\":\"You Did It!\", \"message\":\"So proud of your achievement.\"}" },
            new GreetingCard { CategoryId = catGraduation, Name = "Tossing the Hats", Slug = "tossing-hats", Description = "Celebrate the joy of tossing graduation hats.", Tags = "graduation,hats,toss,blue", ThumbnailUrl = "https://images.unsplash.com/photo-1557683316-973673baf926?w=800", FileUrl = "https://images.unsplash.com/photo-1557683316-973673baf926?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-grad-blue\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#ffffff\", \"heading\":\"Congratulations\", \"message\":\"The future belongs to you.\"}" },
            new GreetingCard { CategoryId = catGraduation, Name = "Academic Success", Slug = "academic-success", Description = "Acknowledge the hard work and academic success.", Tags = "graduation,success,study", ThumbnailUrl = null, FileUrl = null, IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"white\", \"fontFamily\":\"'Palatino Linotype', serif\", \"textColor\":\"#0a0a0a\", \"heading\":\"Graduation Day\", \"message\":\"Best wishes for your next adventure.\"}" },
            new GreetingCard { CategoryId = catGraduation, Name = "Future Pathways", Slug = "future-pathways", Description = "Wishing the best for the exciting future pathways.", Tags = "graduation,future,path", ThumbnailUrl = null, FileUrl = null, IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"linen\", \"fontFamily\":\"'Noto Serif', serif\", \"textColor\":\"#3d2900\", \"heading\":\"Success\", \"message\":\"Well done on your hard work.\"}" },
            new GreetingCard { CategoryId = catGraduation, Name = "Proud Graduate", Slug = "proud-graduate", Description = "A proud moment for every graduate.", Tags = "graduation,proud,diploma", ThumbnailUrl = null, FileUrl = null, IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"prm-noir\", \"fontFamily\":\"Georgia, serif\", \"textColor\":\"#d4af37\", \"heading\":\"Class of 2026\", \"message\":\"You have a bright future ahead.\"}" }
        };

        await db.GreetingCards.AddRangeAsync(cards);
        await db.SaveChangesAsync();
        logger.LogInformation("[SEED] ✅ Seeded {Count} sample greeting cards", cards.Length);
    }
}
