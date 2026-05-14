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
            await SeedCardBackgroundsAsync(db, logger);
            await SeedCardDecorationsAsync(db, logger);
            await SeedSimulatedUsersAsync(db, logger);
            await SeedHistoricalDataAsync(db, logger);
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
        // ── Upsert: always sync Name/Description to ensure DB matches seed ──

        var categories = new[]
        {
            new Category
            {
                Id = new Guid("11111111-0000-0000-0000-000000000001"),
                Name = "Birthday",
                Slug = "sinh-nhat",
                Description = "Birthday greeting cards",
                IconUrl = "🎂",
                Color = "#FF6B6B",
                DisplayOrder = 1,
                IsSystem = true,
                Status = CategoryStatus.Active
            },
            new Category
            {
                Id = new Guid("11111111-0000-0000-0000-000000000002"),
                Name = "Wedding",
                Slug = "dam-cuoi",
                Description = "Wedding congratulations cards",
                IconUrl = "💍",
                Color = "#C9A96E",
                DisplayOrder = 2,
                IsSystem = true,
                Status = CategoryStatus.Active
            },
            new Category
            {
                Id = new Guid("11111111-0000-0000-0000-000000000003"),
                Name = "New Year",
                Slug = "nam-moi",
                Description = "New Year greeting cards",
                IconUrl = "🎉",
                Color = "#FFD700",
                DisplayOrder = 3,
                IsSystem = true,
                Status = CategoryStatus.Active
            },
            new Category
            {
                Id = new Guid("11111111-0000-0000-0000-000000000004"),
                Name = "Festival",
                Slug = "le-hoi",
                Description = "Special occasion and festival cards",
                IconUrl = "🎊",
                Color = "#7C3AED",
                DisplayOrder = 4,
                IsSystem = true,
                Status = CategoryStatus.Active
            },
            new Category
            {
                Id = new Guid("11111111-0000-0000-0000-000000000005"),
                Name = "Graduation",
                Slug = "tot-nghiep",
                Description = "Graduation congratulations cards",
                IconUrl = "🎓",
                Color = "#059669",
                DisplayOrder = 5,
                IsSystem = false,
                Status = CategoryStatus.Active
            }
        };

        // Upsert each category: update Name/Description if exists, insert if not
        foreach (var cat in categories)
        {
            var existing = await db.Categories.FindAsync(cat.Id);
            if (existing != null)
            {
                existing.Name        = cat.Name;
                existing.Description = cat.Description;
                existing.IconUrl     = cat.IconUrl;
                existing.Color       = cat.Color;
            }
            else
            {
                await db.Categories.AddAsync(cat);
            }
        }

        await db.SaveChangesAsync();
        logger.LogInformation("[SEED] ✅ Upserted {Count} categories", categories.Length);
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
        // Skip if cards already exist — preserve transactions
        if (await db.GreetingCards.AnyAsync())
        {
            logger.LogInformation("[SEED] Cards already seeded, skipping.");
            return;
        }

        // Guids for categories
        var catBirthday = new Guid("11111111-0000-0000-0000-000000000001");
        var catWedding = new Guid("11111111-0000-0000-0000-000000000002");
        var catNewYear = new Guid("11111111-0000-0000-0000-000000000003");
        var catFestival = new Guid("11111111-0000-0000-0000-000000000004");
        var catGraduation = new Guid("11111111-0000-0000-0000-000000000005");

        var cards = new[]
        {
            // ── Birthday (1 Free, 4 Premium) ───────────────────────────
            new GreetingCard { CategoryId = catBirthday, Name = "Golden Birthday Celebration", Slug = "golden-birthday", Description = "A luxurious birthday card with golden bokeh lights.", Tags = "birthday,gold,celebration", ThumbnailUrl = "https://picsum.photos/id/237/800/600", FileUrl = "https://picsum.photos/id/237/800/600", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"bday-gold\", \"fontFamily\":\"'Palatino Linotype', serif\", \"textColor\":\"#3d2900\", \"heading\":\"Happy Birthday!\", \"message\":\"Wishing you a golden year filled with joy and success.\"}" },

            new GreetingCard { CategoryId = catBirthday, Name = "Sweet Birthday Cake", Slug = "sweet-birthday-cake", Description = "A delightful birthday card featuring a beautiful cake.", Tags = "birthday,cake,sweet,pink", ThumbnailUrl = "https://images.unsplash.com/photo-1535141192574-5d4897c12636?w=800", FileUrl = "https://images.unsplash.com/photo-1535141192574-5d4897c12636?w=800", IsFeatured = false, IsPremium = false, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-bday-cake\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#9d174d\", \"heading\":\"Happy Birthday!\", \"message\":\"May your special day be as sweet as you are.\"}" },

            new GreetingCard { CategoryId = catBirthday, Name = "Birthday Sparklers", Slug = "birthday-sparklers", Description = "Celebrate with bright sparkling lights on your birthday.", Tags = "birthday,sparklers,party,night", ThumbnailUrl = "https://images.unsplash.com/photo-1516280440614-37939bbacd81?w=800", FileUrl = "https://images.unsplash.com/photo-1516280440614-37939bbacd81?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-bday-sparklers\", \"fontFamily\":\"Georgia, serif\", \"textColor\":\"#facc15\", \"heading\":\"Time to Sparkle!\", \"message\":\"Shine bright on your special day!\"}" },

            new GreetingCard { CategoryId = catBirthday, Name = "Wrapped With Love", Slug = "wrapped-with-love", Description = "Send beautifully wrapped birthday wishes with balloons.", Tags = "birthday,gift,love,balloons", ThumbnailUrl = "https://images.unsplash.com/photo-1574271143515-5cddf8da19be?w=800", FileUrl = "https://images.unsplash.com/photo-1574271143515-5cddf8da19be?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-bday-balloons\", \"fontFamily\":\"'Noto Serif', serif\", \"textColor\":\"#1e3a8a\", \"heading\":\"Sent with Love\", \"message\":\"Wishing you the happiest of birthdays!\"}" },

            new GreetingCard { CategoryId = catBirthday, Name = "Midnight Birthday Party", Slug = "midnight-birthday-party", Description = "An exciting midnight birthday bash with confetti.", Tags = "birthday,night,party,confetti", ThumbnailUrl = "https://images.unsplash.com/photo-1492684223066-81342ee5ff30?w=800", FileUrl = "https://images.unsplash.com/photo-1492684223066-81342ee5ff30?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-bday-confetti\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#ffffff\", \"heading\":\"Let's Celebrate!\", \"message\":\"Time to party and have fun!\"}" },

            // ── Wedding (1 Free, 4 Premium) ────────────────────────────
            new GreetingCard { CategoryId = catWedding, Name = "Classic White Wedding", Slug = "classic-white-wedding", Description = "An elegant white-themed wedding card with white silk.", Tags = "wedding,white,classic,silk", ThumbnailUrl = "https://images.unsplash.com/photo-1519225421980-715cb0215aed?w=800", FileUrl = "https://images.unsplash.com/photo-1519225421980-715cb0215aed?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-wed-silk\", \"fontFamily\":\"'Palatino Linotype', serif\", \"textColor\":\"#1a1a1a\", \"heading\":\"To the Happy Couple\", \"message\":\"Wishing you a lifetime of love and happiness.\"}" },

            new GreetingCard { CategoryId = catWedding, Name = "Eternal Rings", Slug = "eternal-rings", Description = "A symbol of eternal love featuring elegant wedding rings.", Tags = "wedding,rings,love,eternal", ThumbnailUrl = "https://images.unsplash.com/photo-1515934751635-c81c6bc9a2d8?w=800", FileUrl = "https://images.unsplash.com/photo-1515934751635-c81c6bc9a2d8?w=800", IsFeatured = false, IsPremium = false, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-wed-rings\", \"fontFamily\":\"Georgia, serif\", \"textColor\":\"#5c4000\", \"heading\":\"Eternal Love\", \"message\":\"Congratulations on your beautiful union!\"}" },

            new GreetingCard { CategoryId = catWedding, Name = "Floral Romance", Slug = "floral-romance", Description = "A romantic wedding card adorned with beautiful florals.", Tags = "wedding,floral,romance", ThumbnailUrl = "https://images.unsplash.com/photo-1518531933037-91b2f5f229cc?w=800", FileUrl = "https://images.unsplash.com/photo-1518531933037-91b2f5f229cc?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-wed-floral\", \"fontFamily\":\"'Noto Serif', serif\", \"textColor\":\"#ffffff\", \"heading\":\"Forever Romance\", \"message\":\"May your love continue to blossom.\"}" },

            new GreetingCard { CategoryId = catWedding, Name = "With Love, Always", Slug = "with-love-always", Description = "Celebrate the beautiful union of a loving couple.", Tags = "wedding,couple,love,marriage", ThumbnailUrl = "https://images.unsplash.com/photo-1511285560929-80b456fea0bc?w=800", FileUrl = "https://images.unsplash.com/photo-1511285560929-80b456fea0bc?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-wed-couple\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#ffffff\", \"heading\":\"With Love, Always\", \"message\":\"May your love grow stronger each and every day.\"}" },

            new GreetingCard { CategoryId = catWedding, Name = "Garden Wedding Wishes", Slug = "garden-wedding-wishes", Description = "A warm rustic garden wedding card.", Tags = "wedding,rustic,garden,nature", ThumbnailUrl = "https://images.unsplash.com/photo-1520854221256-17451cc331bf?w=800", FileUrl = "https://images.unsplash.com/photo-1520854221256-17451cc331bf?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-wed-rustic\", \"fontFamily\":\"'Courier New', monospace\", \"textColor\":\"#166534\", \"heading\":\"Best Wishes\", \"message\":\"Wishing you a beautiful life together.\"}" },

            // ── New Year (1 Free, 4 Premium) ───────────────────────────
            new GreetingCard { CategoryId = catNewYear, Name = "Spectacular Fireworks", Slug = "spectacular-fireworks", Description = "Welcome the new year with spectacular fireworks.", Tags = "newyear,fireworks,colorful", ThumbnailUrl = "https://images.unsplash.com/photo-1516912481808-3406841bd33c?w=800", FileUrl = "https://images.unsplash.com/photo-1516912481808-3406841bd33c?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-ny-fireworks\", \"fontFamily\":\"'Palatino Linotype', serif\", \"textColor\":\"#ffffff\", \"heading\":\"Happy New Year!\", \"message\":\"May this year bring new happiness and wonderful goals.\"}" },

            new GreetingCard { CategoryId = catNewYear, Name = "Midnight Countdown", Slug = "midnight-countdown", Description = "The final exciting seconds before a brand new year.", Tags = "newyear,countdown,midnight", ThumbnailUrl = "https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800", FileUrl = "https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800", IsFeatured = false, IsPremium = false, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-ny-midnight\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#f5e8c7\", \"heading\":\"Countdown Begins!\", \"message\":\"A toast to the wonderful future ahead!\"}" },

            new GreetingCard { CategoryId = catNewYear, Name = "Sparkling New Year", Slug = "sparkling-new-year", Description = "A glittering new year greeting with golden confetti.", Tags = "newyear,sparkling,confetti,gold", ThumbnailUrl = "https://images.unsplash.com/photo-1467810563316-b5476525c0f9?w=800", FileUrl = "https://images.unsplash.com/photo-1467810563316-b5476525c0f9?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-ny-sparkle\", \"fontFamily\":\"Georgia, serif\", \"textColor\":\"#3d2900\", \"heading\":\"Happy New Year!\", \"message\":\"Let the celebration begin!\"}" },

            new GreetingCard { CategoryId = catNewYear, Name = "Toast to the New Year", Slug = "toast-to-new-year", Description = "Raise a glass and toast to a wonderful new year.", Tags = "newyear,champagne,toast", ThumbnailUrl = "https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?w=800", FileUrl = "https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-ny-champagne\", \"fontFamily\":\"'Noto Serif', serif\", \"textColor\":\"#ffffff\", \"heading\":\"Cheers to the New Year!\", \"message\":\"Wishing you joy and prosperity ahead.\"}" },

            new GreetingCard { CategoryId = catNewYear, Name = "Season's Greetings", Slug = "seasons-greetings", Description = "A festive holiday card to share warm seasonal wishes.", Tags = "newyear,holiday,festive,season", ThumbnailUrl = "https://images.unsplash.com/photo-1513151233558-d860c5398176?w=800", FileUrl = "https://images.unsplash.com/photo-1513151233558-d860c5398176?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-ny-gold\", \"fontFamily\":\"'Courier New', monospace\", \"textColor\":\"#fdf0ee\", \"heading\":\"Season's Greetings!\", \"message\":\"Embrace the joy of this festive season.\"}" },

            // ── Festival (1 Free, 4 Premium) ───────────────────────────
            new GreetingCard { CategoryId = catFestival, Name = "Merry Christmas", Slug = "merry-christmas", Description = "A heartwarming Christmas greeting with holiday magic.", Tags = "festival,christmas,magic,tree", ThumbnailUrl = "https://images.unsplash.com/photo-1482517967863-00e15c9b44be?w=800", FileUrl = "https://images.unsplash.com/photo-1482517967863-00e15c9b44be?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-fest-xmas\", \"fontFamily\":\"Georgia, serif\", \"textColor\":\"#ffffff\", \"heading\":\"Merry Christmas!\", \"message\":\"Warm wishes to you and your loved ones this holiday season.\"}" },

            new GreetingCard { CategoryId = catFestival, Name = "Spooky Halloween", Slug = "spooky-halloween", Description = "Send some spooky and fun Halloween wishes.", Tags = "festival,halloween,spooky", ThumbnailUrl = "https://images.unsplash.com/photo-1508361001413-7a9dca21d08a?w=800", FileUrl = "https://images.unsplash.com/photo-1508361001413-7a9dca21d08a?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-fest-halloween\", \"fontFamily\":\"'Courier New', monospace\", \"textColor\":\"#facc15\", \"heading\":\"Happy Halloween!\", \"message\":\"Have a spooktacular and magical night!\"}" },

            new GreetingCard { CategoryId = catFestival, Name = "Festival of Lights", Slug = "festival-of-lights", Description = "A magical night glowing with beautiful lanterns.", Tags = "festival,lanterns,lights,magical", ThumbnailUrl = "https://picsum.photos/seed/lantern-night/800/600", FileUrl = "https://picsum.photos/seed/lantern-night/800/600", IsFeatured = false, IsPremium = false, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-fest-lantern\", \"fontFamily\":\"'Noto Serif', serif\", \"textColor\":\"#ffffff\", \"heading\":\"Festival of Lights!\", \"message\":\"May your life be illuminated with joy and warmth.\"}" },

            new GreetingCard { CategoryId = catFestival, Name = "Thanksgiving Harvest", Slug = "thanksgiving-harvest", Description = "Give thanks with this beautiful autumn harvest card.", Tags = "festival,thanksgiving,autumn,harvest", ThumbnailUrl = "https://images.unsplash.com/photo-1505253758473-96b7015fcd40?w=800", FileUrl = "https://images.unsplash.com/photo-1505253758473-96b7015fcd40?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-fest-thanksgiving\", \"fontFamily\":\"'Palatino Linotype', serif\", \"textColor\":\"#3d2900\", \"heading\":\"Happy Thanksgiving!\", \"message\":\"Grateful for you today and always.\"}" },

            new GreetingCard { CategoryId = catFestival, Name = "Celebrate Together", Slug = "celebrate-together", Description = "Join the vibrant and colorful festival celebration.", Tags = "festival,celebrate,vibrant,colorful", ThumbnailUrl = "https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?w=800", FileUrl = "https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-fest-carnival\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#ffffff\", \"heading\":\"Let's Celebrate!\", \"message\":\"Enjoy every moment of this joyful celebration!\"}" },

            // ── Graduation (1 Free, 4 Premium) ─────────────────────────
            new GreetingCard { CategoryId = catGraduation, Name = "Congratulations, Graduate!", Slug = "congratulations-graduate", Description = "A proud congratulations for your graduation milestone.", Tags = "graduation,congratulations,achievement", ThumbnailUrl = "https://images.unsplash.com/photo-1498243691581-b145c3f54a5a?w=800", FileUrl = "https://images.unsplash.com/photo-1498243691581-b145c3f54a5a?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-grad-cap\", \"fontFamily\":\"'Palatino Linotype', serif\", \"textColor\":\"#ffffff\", \"heading\":\"Congratulations!\", \"message\":\"Your hard work and dedication have paid off!\"}" },

            new GreetingCard { CategoryId = catGraduation, Name = "Tossing the Hats", Slug = "tossing-the-hats", Description = "Celebrate the joy of graduation day with cap tossing.", Tags = "graduation,hats,toss,ceremony", ThumbnailUrl = "https://images.unsplash.com/photo-1541339907198-e08756dedf3f?w=800", FileUrl = "https://images.unsplash.com/photo-1541339907198-e08756dedf3f?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-grad-day\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#ffffff\", \"heading\":\"You Did It!\", \"message\":\"The future belongs to you — congratulations!\"}" },

            new GreetingCard { CategoryId = catGraduation, Name = "You Did It!", Slug = "you-did-it", Description = "A heartfelt card celebrating your graduation diploma.", Tags = "graduation,diploma,achievement,proud", ThumbnailUrl = "https://images.unsplash.com/photo-1627556704290-2b1f5853ff78?w=800", FileUrl = "https://images.unsplash.com/photo-1627556704290-2b1f5853ff78?w=800", IsFeatured = false, IsPremium = false, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-grad-diploma\", \"fontFamily\":\"Georgia, serif\", \"textColor\":\"#0a0a0a\", \"heading\":\"You Did It!\", \"message\":\"Best wishes for your next great adventure!\"}" },

            new GreetingCard { CategoryId = catGraduation, Name = "Here's to Your Future", Slug = "heres-to-your-future", Description = "Wishing you an exciting and bright future ahead.", Tags = "graduation,future,bright,inspire", ThumbnailUrl = "https://images.unsplash.com/photo-1580582932707-520aed937b7b?w=800", FileUrl = "https://images.unsplash.com/photo-1580582932707-520aed937b7b?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-grad-celebrate\", \"fontFamily\":\"'Noto Serif', serif\", \"textColor\":\"#ffffff\", \"heading\":\"Here's to Your Future!\", \"message\":\"Well done — the world is waiting for you!\"}" },

            new GreetingCard { CategoryId = catGraduation, Name = "So Proud of You", Slug = "so-proud-of-you", Description = "Express your pride for this remarkable achievement.", Tags = "graduation,proud,success,milestone", ThumbnailUrl = "https://images.unsplash.com/photo-1498243691581-b145c3f54a5a?w=800", FileUrl = "https://images.unsplash.com/photo-1498243691581-b145c3f54a5a?w=800", IsFeatured = false, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"prm-velvet\", \"fontFamily\":\"Georgia, serif\", \"textColor\":\"#d4af37\", \"heading\":\"So Proud of You!\", \"message\":\"Class of 2026 — you have a bright future ahead!\"}" },

            // ══════════════════════════════════════════════════════════
            // ADDITIONAL FEATURED CARDS — Total featured target: 18
            // Distribution: Birthday×4, Wedding×4, NewYear×4, Festival×3, Graduation×3
            // ══════════════════════════════════════════════════════════

            // ── Birthday Featured (2 more → total 4) ──────────────────
            new GreetingCard { CategoryId = catBirthday, Name = "Confetti Rain Birthday", Slug = "confetti-rain-birthday", Description = "A joyful birthday surrounded by colourful confetti.", Tags = "birthday,confetti,colour,party", ThumbnailUrl = "https://images.unsplash.com/photo-1514525253161-7a46d19cd819?w=800", FileUrl = "https://images.unsplash.com/photo-1514525253161-7a46d19cd819?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-bday-confetti2\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#ffffff\", \"heading\":\"Happy Birthday!\", \"message\":\"May your day be filled with colour and laughter!\"}" },

            new GreetingCard { CategoryId = catBirthday, Name = "Bloom & Celebrate", Slug = "bloom-and-celebrate", Description = "Fresh flowers to brighten up someone's special day.", Tags = "birthday,flowers,bloom,fresh", ThumbnailUrl = "https://picsum.photos/seed/birthday-bloom/800/600", FileUrl = "https://picsum.photos/seed/birthday-bloom/800/600", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-bday-bloom\", \"fontFamily\":\"'Noto Serif', serif\", \"textColor\":\"#4a1942\", \"heading\":\"Bloom On Your Birthday!\", \"message\":\"Sending you bouquets of happiness on your special day.\"}" },

            // ── Wedding Featured (2 more → total 4) ───────────────────
            new GreetingCard { CategoryId = catWedding, Name = "Golden Hour Vows", Slug = "golden-hour-vows", Description = "A warm golden-hour wedding scene full of promise.", Tags = "wedding,golden,vows,sunset", ThumbnailUrl = "https://images.unsplash.com/photo-1537633552985-df8429e8048b?w=800", FileUrl = "https://images.unsplash.com/photo-1537633552985-df8429e8048b?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-wed-golden\", \"fontFamily\":\"'Palatino Linotype', serif\", \"textColor\":\"#3d1a00\", \"heading\":\"Forever Begins Today\", \"message\":\"May the golden light of this day follow you always.\"}" },

            new GreetingCard { CategoryId = catWedding, Name = "Petals & Promises", Slug = "petals-and-promises", Description = "Delicate petals framing a timeless wedding promise.", Tags = "wedding,petals,promise,roses", ThumbnailUrl = "https://images.unsplash.com/photo-1519741497674-611481863552?w=800", FileUrl = "https://images.unsplash.com/photo-1519741497674-611481863552?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-wed-petals\", \"fontFamily\":\"Georgia, serif\", \"textColor\":\"#ffffff\", \"heading\":\"With All My Love\", \"message\":\"Every petal is a promise — congratulations!\"}" },

            // ── New Year Featured (2 more → total 4) ──────────────────
            new GreetingCard { CategoryId = catNewYear, Name = "Golden New Year Glow", Slug = "golden-new-year-glow", Description = "Ring in the new year with a dazzling golden glow.", Tags = "newyear,gold,glow,luxury", ThumbnailUrl = "https://images.unsplash.com/photo-1467810563316-b5476525c0f9?w=800", FileUrl = "https://images.unsplash.com/photo-1467810563316-b5476525c0f9?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-ny-gold-glow\", \"fontFamily\":\"'Palatino Linotype', serif\", \"textColor\":\"#d4af37\", \"heading\":\"Happy New Year!\", \"message\":\"May this golden year bring prosperity and joy.\"}" },

            new GreetingCard { CategoryId = catNewYear, Name = "Night Sky Celebration", Slug = "night-sky-celebration", Description = "Celebrate beneath a glittering night sky.", Tags = "newyear,night,sky,stars", ThumbnailUrl = "https://images.unsplash.com/photo-1516912481808-3406841bd33c?w=800", FileUrl = "https://images.unsplash.com/photo-1516912481808-3406841bd33c?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-ny-stars\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#f0f4ff\", \"heading\":\"Under a Lucky Star\", \"message\":\"The best is yet to come — Happy New Year!\"}" },

            // ── Festival Featured (2 more → total 3) ──────────────────
            new GreetingCard { CategoryId = catFestival, Name = "Sparkling New Year Concert", Slug = "sparkling-concert", Description = "Music and lights make this festival night unforgettable.", Tags = "festival,concert,music,lights", ThumbnailUrl = "https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?w=800", FileUrl = "https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-fest-concert\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#ffffff\", \"heading\":\"Feel the Music!\", \"message\":\"Wishing you a festival season full of joy and rhythm.\"}" },

            new GreetingCard { CategoryId = catFestival, Name = "Cherry Blossom Festival", Slug = "cherry-blossom-festival", Description = "Celebrate spring's arrival with stunning cherry blossoms.", Tags = "festival,spring,blossom,pink", ThumbnailUrl = "https://images.unsplash.com/photo-1522383225653-ed111181a951?w=800", FileUrl = "https://images.unsplash.com/photo-1522383225653-ed111181a951?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-fest-blossom\", \"fontFamily\":\"'Noto Serif', serif\", \"textColor\":\"#7c2d55\", \"heading\":\"Season of Bloom\", \"message\":\"May this season bring you beauty and new beginnings.\"}" },

            // ── Graduation Featured (2 more → total 3) ────────────────
            new GreetingCard { CategoryId = catGraduation, Name = "Cap & Scroll of Honor", Slug = "cap-and-scroll", Description = "A prestigious card celebrating academic excellence.", Tags = "graduation,cap,scroll,honor", ThumbnailUrl = "https://images.unsplash.com/photo-1541339907198-e08756dedf3f?w=800", FileUrl = "https://images.unsplash.com/photo-1541339907198-e08756dedf3f?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-grad-scroll\", \"fontFamily\":\"'Palatino Linotype', serif\", \"textColor\":\"#1a3a1a\", \"heading\":\"A Scholar's Achievement\", \"message\":\"Your dedication to learning has brought you here — well done!\"}" },

            new GreetingCard { CategoryId = catGraduation, Name = "Step Into Greatness", Slug = "step-into-greatness", Description = "Every step you've taken has led you to this great moment.", Tags = "graduation,success,journey,milestone", ThumbnailUrl = "https://images.unsplash.com/photo-1501386761578-eac5c94b800a?w=800", FileUrl = "https://images.unsplash.com/photo-1501386761578-eac5c94b800a?w=800", IsFeatured = true, IsPremium = true, Status = CardStatus.Active,
                CustomJsonContent = "{\"bgId\":\"img-grad-step\", \"fontFamily\":\"'Manrope', sans-serif\", \"textColor\":\"#ffffff\", \"heading\":\"Step Into Greatness!\", \"message\":\"Your journey is just beginning — the best is yet to come.\"}" }
        };

        await db.GreetingCards.AddRangeAsync(cards);
        await db.SaveChangesAsync();
        logger.LogInformation("[SEED] ✅ Seeded {Count} sample greeting cards", cards.Length);
    }

    // ── 4. Card Backgrounds ───────────────────────────────────────────
    private static async Task SeedCardBackgroundsAsync(AppDbContext db, ILogger logger)
    {
        if (await db.CardBackgrounds.AnyAsync()) return;

        var items = new List<CardBackground>
        {
            // Universal
            new() { Id = "cream",      Label = "Classic Cream",    Categories = "[\"all\"]",                    BgStyle = "#fdf8f0",                                                                                                                          SortOrder = 1 },
            new() { Id = "white",      Label = "Pure White",        Categories = "[\"all\"]",                    BgStyle = "#ffffff",                                                                                                                           SortOrder = 2 },
            new() { Id = "linen",      Label = "Linen",             Categories = "[\"all\"]",                    BgStyle = "linear-gradient(160deg,#f5f0e8 0%,#ede8e0 100%)",                                                                                   SortOrder = 3 },
            // Birthday
            new() { Id = "bday-pink",    Label = "Vibrant Pink",    Categories = "[\"birthday\"]",               BgStyle = "linear-gradient(135deg,#fce4ec 0%,#f8bbd0 50%,#fce4ec 100%)",                                                                       SortOrder = 10 },
            new() { Id = "bday-blue",    Label = "Joyful Blue",     Categories = "[\"birthday\"]",               BgStyle = "linear-gradient(135deg,#e3f2fd 0%,#bbdefb 50%,#e8f5e9 100%)",                                                                       SortOrder = 11 },
            new() { Id = "bday-gold",    Label = "Golden Sparkle",  Categories = "[\"birthday\"]",               BgStyle = "linear-gradient(135deg,#fff8e1 0%,#ffecb3 40%,#ffe082 100%)",                                                                        SortOrder = 12 },
            new() { Id = "bday-rainbow", Label = "Pastel Rainbow",  Categories = "[\"birthday\"]",               BgStyle = "linear-gradient(180deg,#fce4ec 0%,#e8eaf6 25%,#e3f2fd 50%,#e8f5e9 75%,#fffde7 100%)",                                               SortOrder = 13 },
            new() { Id = "bday-purple",  Label = "Dreamy Purple",   Categories = "[\"birthday\"]",               BgStyle = "linear-gradient(135deg,#f3e5f5 0%,#e1bee7 50%,#ce93d8 100%)",                                                                        SortOrder = 14 },
            new() { Id = "bday-mint",    Label = "Mint Green",      Categories = "[\"birthday\"]",               BgStyle = "linear-gradient(135deg,#e0f2f1 0%,#b2dfdb 100%)",                                                                                    SortOrder = 15 },
            // Wedding
            new() { Id = "wed-ivory",    Label = "Elegant Ivory",   Categories = "[\"wedding\"]",                BgStyle = "linear-gradient(160deg,#fdf6e3 0%,#f5e6c8 100%)",                                                                                    SortOrder = 20 },
            new() { Id = "wed-rose",     Label = "Rose Gold",        Categories = "[\"wedding\"]",                BgStyle = "linear-gradient(135deg,#fff0f3 0%,#ffe4e1 40%,#ffd7cc 100%)",                                                                        SortOrder = 21 },
            new() { Id = "wed-sage",     Label = "Sage Green",       Categories = "[\"wedding\"]",                BgStyle = "linear-gradient(160deg,#f0f4f0 0%,#dce8dc 100%)",                                                                                    SortOrder = 22 },
            new() { Id = "wed-blush",    Label = "Peach Blush",      Categories = "[\"wedding\"]",                BgStyle = "linear-gradient(160deg,#fff5f5 0%,#ffe0e0 100%)",                                                                                    SortOrder = 23 },
            new() { Id = "wed-sky",      Label = "Sky Blue",         Categories = "[\"wedding\"]",                BgStyle = "linear-gradient(135deg,#e8f4f8 0%,#d0e8f2 100%)",                                                                                    SortOrder = 24 },
            new() { Id = "wed-lavender", Label = "Lavender",         Categories = "[\"wedding\"]",                BgStyle = "linear-gradient(160deg,#f5f0ff 0%,#e8d8ff 100%)",                                                                                    SortOrder = 25 },
            // New Year
            new() { Id = "ny-midnight",  Label = "New Year Eve",     Categories = "[\"newyear\"]",                BgStyle = "linear-gradient(160deg,#0d1b2a 0%,#1b2a4a 50%,#0d1b2a 100%)",                                                                       SortOrder = 30 },
            new() { Id = "ny-gold",      Label = "Champagne Gold",   Categories = "[\"newyear\"]",                BgStyle = "linear-gradient(135deg,#2c1810 0%,#8b6914 40%,#d4af37 100%)",                                                                        SortOrder = 31 },
            new() { Id = "ny-red",       Label = "Festive Red",      Categories = "[\"newyear\"]",                BgStyle = "linear-gradient(160deg,#7f0000 0%,#c62828 50%,#7f0000 100%)",                                                                        SortOrder = 32 },
            new() { Id = "ny-crystal",   Label = "Blue Crystal",     Categories = "[\"newyear\"]",                BgStyle = "linear-gradient(135deg,#e0f7fa 0%,#b2ebf2 50%,#80deea 100%)",                                                                        SortOrder = 33 },
            new() { Id = "ny-snow",      Label = "White Snow",       Categories = "[\"newyear\"]",                BgStyle = "linear-gradient(160deg,#ffffff 0%,#e8f4f8 50%,#dce8f0 100%)",                                                                        SortOrder = 34 },
            // Festival
            new() { Id = "fest-autumn",  Label = "Autumn",           Categories = "[\"festival\"]",               BgStyle = "linear-gradient(135deg,#fff3e0 0%,#ffe0b2 40%,#ffcc80 100%)",                                                                        SortOrder = 40 },
            new() { Id = "fest-spring",  Label = "Spring",           Categories = "[\"festival\"]",               BgStyle = "linear-gradient(160deg,#fce4ec 0%,#f8bbd0 30%,#e8f5e9 100%)",                                                                        SortOrder = 41 },
            new() { Id = "fest-summer",  Label = "Summer",           Categories = "[\"festival\"]",               BgStyle = "linear-gradient(135deg,#fff9c4 0%,#b3e5fc 100%)",                                                                                    SortOrder = 42 },
            new() { Id = "fest-winter",  Label = "Christmas",        Categories = "[\"festival\"]",               BgStyle = "linear-gradient(160deg,#1a3c2a 0%,#2d5a40 50%,#1a3c2a 100%)",                                                                        SortOrder = 43 },
            // Premium
            new() { Id = "prm-marble",   Label = "Marble",           Categories = "[\"all\"]",                    BgStyle = "linear-gradient(135deg,#f5f5f5 0%,#e0e0e0 25%,#f8f8f8 50%,#e8e8e8 75%,#f5f5f5 100%)",                                               SortOrder = 50, IsPremium = true },
            new() { Id = "prm-velvet",   Label = "Purple Velvet",    Categories = "[\"all\"]",                    BgStyle = "linear-gradient(160deg,#2d1b4e 0%,#4a2980 100%)",                                                                                    SortOrder = 51, IsPremium = true },
            new() { Id = "prm-noir",     Label = "Elegant Noir",     Categories = "[\"all\"]",                    BgStyle = "linear-gradient(160deg,#1a1a1a 0%,#2d2d2d 100%)",                                                                                    SortOrder = 52, IsPremium = true },
        };

        await db.CardBackgrounds.AddRangeAsync(items);
        await db.SaveChangesAsync();
        logger.LogInformation("[SEED] ✅ Seeded {Count} card backgrounds", items.Count);
    }

    // ── 5. Card Decorations ───────────────────────────────────────────
    private static async Task SeedCardDecorationsAsync(AppDbContext db, ILogger logger)
    {
        if (await db.CardDecorations.AnyAsync()) return;

        var items = new List<CardDecoration>
        {
            new() { Id = "none", Label = "None", Preview = "✕", Categories = "[\"all\"]", Elements = "[]", SortOrder = 0 },

            // Birthday
            new() { Id = "bday-balloons", Label = "Balloons", Preview = "🎈", Categories = "[\"birthday\"]", SortOrder = 10,
                Elements = """[{"emoji":"🎈","top":"4%","left":"4%","rotate":"-15deg","size":"2rem"},{"emoji":"🎈","top":"6%","left":"78%","rotate":"10deg","size":"1.6rem"},{"emoji":"🎉","top":"83%","left":"8%","rotate":"20deg","size":"1.8rem"},{"emoji":"✨","top":"14%","left":"48%","rotate":"0deg","size":"1.2rem","opacity":"0.7"},{"emoji":"🎊","top":"82%","left":"72%","rotate":"-10deg","size":"1.6rem"}]""" },
            new() { Id = "bday-cake",    Label = "Cake",     Preview = "🎂", Categories = "[\"birthday\"]", SortOrder = 11,
                Elements = """[{"emoji":"🎂","top":"78%","left":"44%","rotate":"0deg","size":"2.4rem"},{"emoji":"🎁","top":"4%","left":"8%","rotate":"-10deg","size":"1.8rem"},{"emoji":"🌟","top":"5%","left":"76%","rotate":"20deg","size":"1.6rem"},{"emoji":"🎀","top":"80%","left":"10%","rotate":"-5deg","size":"1.6rem"},{"emoji":"🍭","top":"82%","left":"75%","rotate":"10deg","size":"1.6rem"}]""" },
            new() { Id = "bday-stars",   Label = "Stars",    Preview = "⭐", Categories = "[\"birthday\"]", SortOrder = 12,
                Elements = """[{"emoji":"⭐","top":"4%","left":"12%","rotate":"10deg","size":"1.6rem"},{"emoji":"✨","top":"8%","left":"72%","rotate":"-5deg","size":"1.2rem","opacity":"0.8"},{"emoji":"🌟","top":"80%","left":"15%","rotate":"15deg","size":"1.8rem"},{"emoji":"💫","top":"76%","left":"74%","rotate":"-10deg","size":"1.6rem"},{"emoji":"⭐","top":"45%","left":"4%","rotate":"5deg","size":"1.2rem","opacity":"0.5"}]""" },

            // Wedding
            new() { Id = "wed-flowers",     Label = "Flowers",     Preview = "💐", Categories = "[\"wedding\"]", SortOrder = 20,
                Elements = """[{"emoji":"🌸","top":"4%","left":"4%","rotate":"-20deg","size":"2rem"},{"emoji":"🌺","top":"6%","left":"74%","rotate":"15deg","size":"1.8rem"},{"emoji":"🌹","top":"78%","left":"6%","rotate":"-10deg","size":"1.8rem"},{"emoji":"💐","top":"80%","left":"70%","rotate":"10deg","size":"2rem"},{"emoji":"🌷","top":"44%","left":"2%","rotate":"-5deg","size":"1.4rem","opacity":"0.6"}]""" },
            new() { Id = "wed-hearts",      Label = "Hearts",      Preview = "💕", Categories = "[\"wedding\"]", SortOrder = 21,
                Elements = """[{"emoji":"❤️","top":"4%","left":"44%","rotate":"0deg","size":"2rem"},{"emoji":"💕","top":"80%","left":"40%","rotate":"0deg","size":"1.8rem"},{"emoji":"💍","top":"8%","left":"8%","rotate":"-10deg","size":"1.6rem"},{"emoji":"🕊️","top":"10%","left":"74%","rotate":"15deg","size":"1.6rem"},{"emoji":"💖","top":"44%","left":"78%","rotate":"5deg","size":"1.2rem","opacity":"0.6"}]""" },
            new() { Id = "wed-butterflies", Label = "Butterflies", Preview = "🦋", Categories = "[\"wedding\"]", SortOrder = 22,
                Elements = """[{"emoji":"🦋","top":"5%","left":"8%","rotate":"-15deg","size":"1.8rem"},{"emoji":"🦋","top":"10%","left":"68%","rotate":"20deg","size":"1.4rem"},{"emoji":"🌸","top":"78%","left":"12%","rotate":"-5deg","size":"1.8rem"},{"emoji":"🌷","top":"80%","left":"68%","rotate":"10deg","size":"1.8rem"},{"emoji":"🦋","top":"48%","left":"76%","rotate":"-10deg","size":"1.2rem","opacity":"0.5"}]""" },

            // New Year
            new() { Id = "ny-fireworks", Label = "Fireworks", Preview = "🎆", Categories = "[\"newyear\"]", SortOrder = 30,
                Elements = """[{"emoji":"🎆","top":"4%","left":"8%","rotate":"0deg","size":"2.2rem"},{"emoji":"🎇","top":"6%","left":"68%","rotate":"0deg","size":"2rem"},{"emoji":"✨","top":"80%","left":"18%","rotate":"0deg","size":"1.6rem","opacity":"0.8"},{"emoji":"⭐","top":"76%","left":"74%","rotate":"20deg","size":"1.6rem"},{"emoji":"🌟","top":"44%","left":"76%","rotate":"-5deg","size":"1.2rem","opacity":"0.6"}]""" },
            new() { Id = "ny-champagne", Label = "Party",     Preview = "🥂", Categories = "[\"newyear\"]", SortOrder = 31,
                Elements = """[{"emoji":"🥂","top":"76%","left":"40%","rotate":"0deg","size":"2.2rem"},{"emoji":"🎊","top":"4%","left":"8%","rotate":"-10deg","size":"2rem"},{"emoji":"🎉","top":"6%","left":"70%","rotate":"10deg","size":"1.8rem"},{"emoji":"✨","top":"44%","left":"4%","rotate":"0deg","size":"1.2rem","opacity":"0.6"},{"emoji":"🎈","top":"78%","left":"72%","rotate":"5deg","size":"1.6rem"}]""" },
            new() { Id = "ny-lantern",   Label = "Lanterns",  Preview = "🏮", Categories = "[\"newyear\"]", SortOrder = 32,
                Elements = """[{"emoji":"🏮","top":"4%","left":"12%","rotate":"-5deg","size":"2rem"},{"emoji":"🏮","top":"6%","left":"64%","rotate":"5deg","size":"1.8rem"},{"emoji":"🌙","top":"78%","left":"8%","rotate":"-10deg","size":"1.8rem"},{"emoji":"⭐","top":"80%","left":"70%","rotate":"15deg","size":"1.6rem"},{"emoji":"✨","top":"44%","left":"76%","rotate":"0deg","size":"1.2rem","opacity":"0.5"}]""" },

            // Festival
            new() { Id = "fest-autumn",   Label = "Autumn Leaves", Preview = "🍂", Categories = "[\"festival\"]", SortOrder = 40,
                Elements = """[{"emoji":"🍂","top":"4%","left":"8%","rotate":"-20deg","size":"2rem"},{"emoji":"🍁","top":"8%","left":"72%","rotate":"15deg","size":"1.8rem"},{"emoji":"🍂","top":"78%","left":"12%","rotate":"10deg","size":"1.6rem"},{"emoji":"🍁","top":"80%","left":"68%","rotate":"-15deg","size":"1.8rem"},{"emoji":"🍄","top":"44%","left":"4%","rotate":"5deg","size":"1.2rem","opacity":"0.5"}]""" },
            new() { Id = "fest-spring",   Label = "Spring Bloom",  Preview = "🌸", Categories = "[\"festival\"]", SortOrder = 41,
                Elements = """[{"emoji":"🌸","top":"4%","left":"6%","rotate":"-15deg","size":"1.8rem"},{"emoji":"🌷","top":"6%","left":"70%","rotate":"10deg","size":"1.8rem"},{"emoji":"🦋","top":"78%","left":"8%","rotate":"-10deg","size":"1.8rem"},{"emoji":"🌺","top":"80%","left":"70%","rotate":"5deg","size":"1.8rem"},{"emoji":"🌼","top":"44%","left":"76%","rotate":"-5deg","size":"1.2rem","opacity":"0.6"}]""" },
            new() { Id = "fest-christmas",Label = "Christmas",     Preview = "🎄", Categories = "[\"festival\"]", SortOrder = 42,
                Elements = """[{"emoji":"🎄","top":"4%","left":"8%","rotate":"-5deg","size":"2.2rem"},{"emoji":"⭐","top":"3%","left":"70%","rotate":"10deg","size":"1.6rem"},{"emoji":"🦌","top":"80%","left":"8%","rotate":"-10deg","size":"1.8rem"},{"emoji":"🎁","top":"78%","left":"68%","rotate":"5deg","size":"1.8rem"},{"emoji":"❄️","top":"44%","left":"76%","rotate":"0deg","size":"1.4rem","opacity":"0.6"}]""" },

            // Universal
            new() { Id = "universal-sparkle", Label = "Sparkles",       Preview = "✨", Categories = "[\"all\"]", SortOrder = 50,
                Elements = """[{"emoji":"✨","top":"4%","left":"8%","rotate":"0deg","size":"1.4rem","opacity":"0.7"},{"emoji":"✨","top":"6%","left":"76%","rotate":"0deg","size":"1rem","opacity":"0.5"},{"emoji":"✨","top":"78%","left":"12%","rotate":"0deg","size":"1.4rem","opacity":"0.7"},{"emoji":"✨","top":"80%","left":"74%","rotate":"0deg","size":"1rem","opacity":"0.5"},{"emoji":"💫","top":"44%","left":"4%","rotate":"0deg","size":"1.2rem","opacity":"0.4"}]""" },
            new() { Id = "universal-corner", Label = "Corner Flowers", Preview = "🌸", Categories = "[\"all\"]", SortOrder = 51,
                Elements = """[{"emoji":"🌸","top":"2%","left":"2%","rotate":"-20deg","size":"1.8rem"},{"emoji":"🌸","top":"3%","left":"76%","rotate":"20deg","size":"1.5rem"},{"emoji":"🌸","top":"80%","left":"2%","rotate":"-10deg","size":"1.5rem"},{"emoji":"🌸","top":"82%","left":"76%","rotate":"10deg","size":"1.8rem"}]""" },
            new() { Id = "universal-hearts", Label = "Hearts",         Preview = "❤️", Categories = "[\"all\"]", SortOrder = 52,
                Elements = """[{"emoji":"❤️","top":"4%","left":"44%","rotate":"0deg","size":"1.8rem"},{"emoji":"💕","top":"80%","left":"40%","rotate":"0deg","size":"1.4rem"},{"emoji":"❤️","top":"44%","left":"4%","rotate":"-10deg","size":"1rem","opacity":"0.4"},{"emoji":"❤️","top":"44%","left":"76%","rotate":"10deg","size":"1rem","opacity":"0.4"}]""" },
        };

        await db.CardDecorations.AddRangeAsync(items);
        await db.SaveChangesAsync();
        logger.LogInformation("[SEED] ✅ Seeded {Count} card decorations", items.Count);
    }

    // ── 6. Simulated Users (5 Free + 4 Monthly + 2 Annual) ────────────
    private static async Task SeedSimulatedUsersAsync(AppDbContext db, ILogger logger)
    {
        if (await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email.EndsWith("@simtest.local")))
        {
            logger.LogInformation("[SEED] Simulated users already exist, skipping.");
            return;
        }

        const string password = "User@123456!";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, 12);

        var profiles = new[]
        {
            ("user1@simtest.local",  "Nguyễn Văn An",      SubscriptionPlan.Free,    (int?)null),
            ("user2@simtest.local",  "Trần Thị Bình",      SubscriptionPlan.Free,    null),
            ("user3@simtest.local",  "Lê Quốc Cường",      SubscriptionPlan.Free,    null),
            ("user4@simtest.local",  "Phạm Diệu Dung",     SubscriptionPlan.Free,    null),
            ("user5@simtest.local",  "Hoàng Mạnh Em",      SubscriptionPlan.Free,    null),
            ("user6@simtest.local",  "Đỗ Thanh Phong",     SubscriptionPlan.Monthly, (int?)30),
            ("user7@simtest.local",  "Vũ Hồng Giang",      SubscriptionPlan.Monthly, 30),
            ("user8@simtest.local",  "Bùi Trí Hùng",       SubscriptionPlan.Monthly, 30),
            ("user9@simtest.local",  "Đặng Minh Khôi",     SubscriptionPlan.Monthly, 30),
            ("user10@simtest.local", "Trương Hà Linh",     SubscriptionPlan.Annual,  365),
            ("user11@simtest.local", "Ngô Thái Minh",      SubscriptionPlan.Annual,  365)
        };

        var users = new List<User>();
        foreach (var (email, fullName, _, _) in profiles)
        {
            users.Add(new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                Email = email,
                PasswordHash = passwordHash,
                Role = UserRole.User,
                Status = UserStatus.Active,
                EmailVerificationToken = null,
                EmailVerificationTokenExpiry = null,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            });
        }
        await db.Users.AddRangeAsync(users);
        await db.SaveChangesAsync();

        // Create subscriptions + payment transactions for Monthly/Annual users
        var now = DateTime.UtcNow;
        for (int i = 0; i < profiles.Length; i++)
        {
            var (_, _, plan, days) = profiles[i];
            if (plan == SubscriptionPlan.Free) continue;

            var startDate = now.AddDays(-15);
            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = users[i].Id,
                Plan = plan,
                Status = SubscriptionStatus.Active,
                StartDate = startDate,
                ExpiryDate = startDate.AddDays(days!.Value),
                PaymentMethod = PaymentMethod.CardPayment,
                CreatedAt = startDate,
                UpdatedAt = startDate
            };
            await db.Subscriptions.AddAsync(subscription);

            var payment = new PaymentTransaction
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscription.Id,
                Amount = plan == SubscriptionPlan.Monthly ? 9.99m : 89.99m,
                Currency = "USD",
                PaymentMethod = PaymentMethod.CardPayment,
                GatewayProvider = "VNPay",
                GatewayTransactionId = $"VNPAY-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                Status = PaymentStatus.Paid,
                PaidAt = startDate,
                CreatedAt = startDate
            };
            await db.PaymentTransactions.AddAsync(payment);
        }
        await db.SaveChangesAsync();

        logger.LogInformation("[SEED] ✅ Seeded {Count} simulated users (5 Free + 4 Monthly + 2 Annual) + 6 paid subscriptions", users.Count);
    }

    // ── 7. Historical Greeting Transactions (~707 over 30 days, 2 peaks) ─
    private static async Task SeedHistoricalDataAsync(AppDbContext db, ILogger logger)
    {
        // Idempotent guard: skip if there are already transactions older than 20 days
        var oldThreshold = DateTime.UtcNow.AddDays(-20);
        if (await db.GreetingTransactions.AnyAsync(t => t.CreatedAt < oldThreshold))
        {
            logger.LogInformation("[SEED] Historical transactions already exist, skipping.");
            return;
        }

        var senders = await db.Users.IgnoreQueryFilters()
            .Where(u => u.Email.EndsWith("@simtest.local"))
            .ToListAsync();
        var cards = await db.GreetingCards.ToListAsync();
        if (senders.Count == 0 || cards.Count == 0)
        {
            logger.LogWarning("[SEED] Cannot seed historical data: missing simulated users or cards.");
            return;
        }

        var random = new Random(42); // Deterministic seed for reproducible data
        var transactions = new List<GreetingTransaction>();
        var today = DateTime.UtcNow.Date;
        var recipientDomains = new[] { "gmail.com", "yahoo.com", "outlook.com", "hotmail.com" };

        for (int day = 0; day < 30; day++)
        {
            var date = today.AddDays(-(29 - day));

            // 2 peaks: around day 9 and day 23 (counting from start)
            int count;
            if (day == 9 || day == 23) count = 55;
            else if (Math.Abs(day - 9) <= 3 || Math.Abs(day - 23) <= 3) count = 30;
            else count = random.Next(10, 18);

            for (int i = 0; i < count; i++)
            {
                var sender = senders[random.Next(senders.Count)];
                var card = cards[random.Next(cards.Count)];
                var sentAt = date.AddHours(random.Next(8, 22)).AddMinutes(random.Next(0, 60));
                transactions.Add(new GreetingTransaction
                {
                    Id = Guid.NewGuid(),
                    SenderId = sender.Id,
                    CardId = card.Id,
                    RecipientEmail = $"recipient{random.Next(1, 999)}@{recipientDomains[random.Next(recipientDomains.Length)]}",
                    Subject = $"A greeting card for you",
                    PersonalMessage = "Wishing you all the best!",
                    Status = TransactionStatus.Sent,
                    SentAt = sentAt,
                    CreatedAt = sentAt
                });
            }
        }

        await db.GreetingTransactions.AddRangeAsync(transactions);
        await db.SaveChangesAsync();
        logger.LogInformation("[SEED] ✅ Seeded {Count} historical greeting transactions over 30 days (2 peaks)", transactions.Count);
    }
}
