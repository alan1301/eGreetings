using EGreetings.Domain.Entities;
using EGreetings.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EGreetings.Infrastructure.Persistence.Seed;

public static class AppDbContextSeed
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            await context.Database.MigrateAsync();

            if (!await context.Users.AnyAsync())
                await SeedUsersAsync(context, logger);

            if (!await context.Categories.AnyAsync())
                await SeedCategoriesAsync(context, logger);

            if (!await context.GreetingTemplates.AnyAsync())
                await SeedTemplatesAsync(context, logger);

            if (!await context.Subscriptions.AnyAsync())
                await SeedSubscriptionsAsync(context, logger);

            if (!await context.Greetings.AnyAsync())
                await SeedGreetingsAsync(context, logger);

            if (!await context.WebContents.AnyAsync())
                await SeedWebContentsAsync(context, logger);

            if (!await context.Contacts.AnyAsync())
                await SeedContactsAsync(context, logger);

            if (!await context.Feedbacks.AnyAsync())
                await SeedFeedbacksAsync(context, logger);

            logger.LogInformation("Seed dữ liệu mẫu hoàn tất.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi seed dữ liệu: {Message}", ex.Message);
            throw;
        }
    }

    private static async Task SeedUsersAsync(AppDbContext context, ILogger logger)
    {
        var users = new List<User>
        {
            // Admin
            new()
            {
                FullName = "Quản trị viên hệ thống",
                Email = "admin@egreetings.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
                Phone = "0901234567",
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                EmailVerifiedAt = DateTime.UtcNow.AddDays(-30),
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            // Users
            new()
            {
                FullName = "Nguyễn Thị Hoa",
                Email = "hoanguyen@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123456"),
                Phone = "0912345678",
                Role = UserRole.User,
                Status = UserStatus.Active,
                EmailVerifiedAt = DateTime.UtcNow.AddDays(-20),
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new()
            {
                FullName = "Trần Văn Minh",
                Email = "minhtvn@yahoo.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123456"),
                Phone = "0923456789",
                Role = UserRole.User,
                Status = UserStatus.Active,
                EmailVerifiedAt = DateTime.UtcNow.AddDays(-15),
                CreatedAt = DateTime.UtcNow.AddDays(-18)
            },
            new()
            {
                FullName = "Lê Thị Bích Ngọc",
                Email = "ngocle@outlook.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123456"),
                Phone = "0934567890",
                Role = UserRole.User,
                Status = UserStatus.Active,
                EmailVerifiedAt = DateTime.UtcNow.AddDays(-10),
                CreatedAt = DateTime.UtcNow.AddDays(-12)
            },
            new()
            {
                FullName = "Phạm Quốc Hùng",
                Email = "hungpq@company.vn",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123456"),
                Phone = "0945678901",
                Role = UserRole.User,
                Status = UserStatus.Active,
                EmailVerifiedAt = DateTime.UtcNow.AddDays(-5),
                CreatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new()
            {
                FullName = "Võ Thị Kim Anh",
                Email = "kimanhvo@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123456"),
                Phone = "0956789012",
                Role = UserRole.User,
                Status = UserStatus.Inactive,  // Chưa xác thực email
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                FullName = "Đặng Văn Toàn",
                Email = "toandv.test@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123456"),
                Phone = "0967890123",
                Role = UserRole.User,
                Status = UserStatus.Banned,  // Tài khoản bị ban
                EmailVerifiedAt = DateTime.UtcNow.AddDays(-30),
                CreatedAt = DateTime.UtcNow.AddDays(-40)
            }
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
        logger.LogInformation("Đã seed {Count} người dùng.", users.Count);
    }

    private static async Task SeedCategoriesAsync(AppDbContext context, ILogger logger)
    {
        var categories = new List<Category>
        {
            new() { Name = "Sinh nhật", Description = "Thiệp chúc mừng sinh nhật", IconUrl = "/icons/birthday.svg", IsSystem = true, IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow.AddDays(-60) },
            new() { Name = "Tết Nguyên Đán", Description = "Thiệp chúc mừng năm mới", IconUrl = "/icons/tet.svg", IsSystem = true, IsActive = true, DisplayOrder = 2, CreatedAt = DateTime.UtcNow.AddDays(-60) },
            new() { Name = "Đám cưới", Description = "Thiệp chúc mừng đám cưới và kỷ niệm ngày cưới", IconUrl = "/icons/wedding.svg", IsSystem = true, IsActive = true, DisplayOrder = 3, CreatedAt = DateTime.UtcNow.AddDays(-60) },
            new() { Name = "Kỷ niệm", Description = "Thiệp kỷ niệm các dịp đặc biệt", IconUrl = "/icons/anniversary.svg", IsSystem = false, IsActive = true, DisplayOrder = 4, CreatedAt = DateTime.UtcNow.AddDays(-50) },
            new() { Name = "Tốt nghiệp", Description = "Thiệp chúc mừng tốt nghiệp và thành tích học tập", IconUrl = "/icons/graduation.svg", IsSystem = false, IsActive = true, DisplayOrder = 5, CreatedAt = DateTime.UtcNow.AddDays(-45) },
            new() { Name = "Lễ Giáng Sinh", Description = "Thiệp chúc mừng Giáng sinh", IconUrl = "/icons/christmas.svg", IsSystem = false, IsActive = true, DisplayOrder = 6, CreatedAt = DateTime.UtcNow.AddDays(-40) },
            new() { Name = "Cảm ơn", Description = "Thiệp cảm ơn và tri ân", IconUrl = "/icons/thanks.svg", IsSystem = false, IsActive = true, DisplayOrder = 7, CreatedAt = DateTime.UtcNow.AddDays(-35) },
            new() { Name = "Chia buồn", Description = "Thiệp chia buồn và an ủi", IconUrl = "/icons/sympathy.svg", IsSystem = false, IsActive = true, DisplayOrder = 8, CreatedAt = DateTime.UtcNow.AddDays(-30) },
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
        logger.LogInformation("Đã seed {Count} danh mục.", categories.Count);
    }

    private static async Task SeedTemplatesAsync(AppDbContext context, ILogger logger)
    {
        var catIds = await context.Categories.OrderBy(c => c.DisplayOrder).Select(c => c.Id).ToListAsync();

        var templates = new List<GreetingTemplate>
        {
            // Sinh nhật (Category 0)
            new()
            {
                CategoryId = catIds[0], Name = "Sinh nhật rực rỡ", IsFree = true, IsActive = true,
                Description = "Mẫu thiệp sinh nhật rực rỡ với hoa và bóng bay",
                ThumbnailUrl = "/thumbnails/birthday-1.jpg",
                HtmlContent = "<div class='greeting birthday-1'><h1>Chúc mừng sinh nhật!</h1><p>Chúc {{recipient_name}} một ngày sinh nhật thật vui vẻ và hạnh phúc!</p><p class='message'>{{sender_message}}</p></div>",
                CssStyle = ".birthday-1 { background: linear-gradient(135deg, #ff9a9e, #fecfef); padding: 2rem; border-radius: 16px; text-align: center; }",
                UsageCount = 245, CreatedAt = DateTime.UtcNow.AddDays(-55)
            },
            new()
            {
                CategoryId = catIds[0], Name = "Sinh nhật sang trọng", IsFree = false, IsActive = true,
                Description = "Mẫu thiệp sinh nhật phong cách sang trọng với nền vàng gold",
                ThumbnailUrl = "/thumbnails/birthday-2.jpg",
                HtmlContent = "<div class='greeting birthday-2'><h1>Happy Birthday!</h1><p>Kính chúc {{recipient_name}} sinh nhật vui vẻ!</p><p class='message'>{{sender_message}}</p></div>",
                CssStyle = ".birthday-2 { background: linear-gradient(135deg, #f6d365, #fda085); padding: 2rem; border-radius: 16px; text-align: center; color: #fff; }",
                UsageCount = 89, CreatedAt = DateTime.UtcNow.AddDays(-50)
            },
            new()
            {
                CategoryId = catIds[0], Name = "Sinh nhật dễ thương", IsFree = true, IsActive = true,
                Description = "Mẫu thiệp sinh nhật dễ thương với phong cách cartoon",
                ThumbnailUrl = "/thumbnails/birthday-3.jpg",
                HtmlContent = "<div class='greeting birthday-3'><h1>🎂 Happy Birthday 🎂</h1><p>{{recipient_name}} ơi!</p><p class='message'>{{sender_message}}</p></div>",
                CssStyle = ".birthday-3 { background: #fffde7; padding: 2rem; border-radius: 16px; text-align: center; }",
                UsageCount = 178, CreatedAt = DateTime.UtcNow.AddDays(-48)
            },
            // Tết (Category 1)
            new()
            {
                CategoryId = catIds[1], Name = "Tết vạn sự như ý", IsFree = true, IsActive = true,
                Description = "Mẫu thiệp Tết truyền thống với hoa mai và câu đối đỏ",
                ThumbnailUrl = "/thumbnails/tet-1.jpg",
                HtmlContent = "<div class='greeting tet-1'><h1>Chúc Mừng Năm Mới</h1><p>Kính chúc {{recipient_name}} năm mới vạn sự như ý!</p><p class='message'>{{sender_message}}</p></div>",
                CssStyle = ".tet-1 { background: linear-gradient(135deg, #ff0000, #ff6b35); padding: 2rem; border-radius: 16px; text-align: center; color: #ffd700; }",
                UsageCount = 512, CreatedAt = DateTime.UtcNow.AddDays(-55)
            },
            new()
            {
                CategoryId = catIds[1], Name = "Tết hiện đại", IsFree = false, IsActive = true,
                Description = "Mẫu thiệp Tết phong cách hiện đại, tối giản",
                ThumbnailUrl = "/thumbnails/tet-2.jpg",
                HtmlContent = "<div class='greeting tet-2'><h1>新年快乐 · Happy New Year</h1><p>{{recipient_name}}</p><p class='message'>{{sender_message}}</p></div>",
                CssStyle = ".tet-2 { background: #1a1a2e; padding: 2rem; border-radius: 16px; text-align: center; color: #e2b04a; }",
                UsageCount = 143, CreatedAt = DateTime.UtcNow.AddDays(-50)
            },
            // Đám cưới (Category 2)
            new()
            {
                CategoryId = catIds[2], Name = "Chúc mừng hôn lễ", IsFree = false, IsActive = true,
                Description = "Mẫu thiệp chúc mừng đám cưới lãng mạn với hoa hồng",
                ThumbnailUrl = "/thumbnails/wedding-1.jpg",
                HtmlContent = "<div class='greeting wedding-1'><h1>💒 Chúc Mừng Hôn Lễ</h1><p>Kính chúc đôi uyên ương {{recipient_name}} trăm năm hạnh phúc!</p><p class='message'>{{sender_message}}</p></div>",
                CssStyle = ".wedding-1 { background: linear-gradient(135deg, #ffecd2, #fcb69f); padding: 2rem; border-radius: 16px; text-align: center; }",
                UsageCount = 67, CreatedAt = DateTime.UtcNow.AddDays(-45)
            },
            // Tốt nghiệp (Category 4)
            new()
            {
                CategoryId = catIds[4], Name = "Chúc mừng tốt nghiệp", IsFree = true, IsActive = true,
                Description = "Mẫu thiệp chúc mừng tốt nghiệp sôi động",
                ThumbnailUrl = "/thumbnails/graduation-1.jpg",
                HtmlContent = "<div class='greeting grad-1'><h1>🎓 Chúc Mừng Tốt Nghiệp!</h1><p>Chúc {{recipient_name}} tiếp tục thành công trên con đường sự nghiệp!</p><p class='message'>{{sender_message}}</p></div>",
                CssStyle = ".grad-1 { background: linear-gradient(135deg, #43e97b, #38f9d7); padding: 2rem; border-radius: 16px; text-align: center; }",
                UsageCount = 134, CreatedAt = DateTime.UtcNow.AddDays(-40)
            },
            // Giáng sinh (Category 5)
            new()
            {
                CategoryId = catIds[5], Name = "Noel vui vẻ", IsFree = true, IsActive = true,
                Description = "Mẫu thiệp Giáng sinh ấm áp với tuyết rơi và cây thông",
                ThumbnailUrl = "/thumbnails/christmas-1.jpg",
                HtmlContent = "<div class='greeting xmas-1'><h1>🎄 Merry Christmas!</h1><p>Kính chúc {{recipient_name}} mùa Giáng sinh an lành và hạnh phúc!</p><p class='message'>{{sender_message}}</p></div>",
                CssStyle = ".xmas-1 { background: linear-gradient(135deg, #1d4e1a, #2d8653); padding: 2rem; border-radius: 16px; text-align: center; color: #fff; }",
                UsageCount = 289, CreatedAt = DateTime.UtcNow.AddDays(-35)
            },
        };

        await context.GreetingTemplates.AddRangeAsync(templates);
        await context.SaveChangesAsync();
        logger.LogInformation("Đã seed {Count} mẫu thiệp.", templates.Count);
    }

    private static async Task SeedSubscriptionsAsync(AppDbContext context, ILogger logger)
    {
        var userIds = await context.Users
            .Where(u => u.Role == UserRole.User && u.Status == UserStatus.Active)
            .Select(u => u.Id)
            .ToListAsync();

        if (userIds.Count < 3) return;

        var subscriptions = new List<Subscription>
        {
            // Active subscription - User 1 (Nguyễn Thị Hoa)
            new()
            {
                UserId = userIds[0],
                Status = SubscriptionStatus.Active,
                Price = 99000,
                StartDate = DateTime.UtcNow.AddDays(-20),
                ExpiredAt = DateTime.UtcNow.AddDays(10),
                MaxRecipients = 20,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            // Active subscription - User 2 (Trần Văn Minh)
            new()
            {
                UserId = userIds[1],
                Status = SubscriptionStatus.Active,
                Price = 199000,
                StartDate = DateTime.UtcNow.AddDays(-15),
                ExpiredAt = DateTime.UtcNow.AddDays(15),
                MaxRecipients = 50,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            // Expired subscription - User 3 (Lê Thị Bích Ngọc)
            new()
            {
                UserId = userIds[2],
                Status = SubscriptionStatus.Expired,
                Price = 99000,
                StartDate = DateTime.UtcNow.AddDays(-40),
                ExpiredAt = DateTime.UtcNow.AddDays(-10),
                MaxRecipients = 20,
                CreatedAt = DateTime.UtcNow.AddDays(-40)
            },
        };

        await context.Subscriptions.AddRangeAsync(subscriptions);
        await context.SaveChangesAsync();

        // Seed recipients cho subscription đầu tiên
        var firstSubId = subscriptions[0].Id;
        var recipients = new List<SubscriptionRecipient>
        {
            new() { SubscriptionId = firstSubId, Email = "meviet@gmail.com", Name = "Mẹ", Birthday = new DateTime(1960, 5, 15), Occasion = "Birthday", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new() { SubscriptionId = firstSubId, Email = "chaviet@gmail.com", Name = "Bố", Birthday = new DateTime(1958, 10, 20), Occasion = "Birthday", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new() { SubscriptionId = firstSubId, Email = "anhcaiviet@gmail.com", Name = "Anh trai", Birthday = new DateTime(1985, 3, 8), Occasion = "Birthday", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-15) },
        };

        await context.SubscriptionRecipients.AddRangeAsync(recipients);

        // Seed payments
        var payments = new List<Payment>
        {
            new() { UserId = userIds[0], SubscriptionId = subscriptions[0].Id, Amount = 99000, PaymentMethod = "VNPay", Status = PaymentStatus.Paid, PaidAt = DateTime.UtcNow.AddDays(-20), CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new() { UserId = userIds[1], SubscriptionId = subscriptions[1].Id, Amount = 199000, PaymentMethod = "Momo", Status = PaymentStatus.Paid, PaidAt = DateTime.UtcNow.AddDays(-15), CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new() { UserId = userIds[2], SubscriptionId = subscriptions[2].Id, Amount = 99000, PaymentMethod = "Card", Status = PaymentStatus.Paid, PaidAt = DateTime.UtcNow.AddDays(-40), CreatedAt = DateTime.UtcNow.AddDays(-40) },
        };

        await context.Payments.AddRangeAsync(payments);
        await context.SaveChangesAsync();
        logger.LogInformation("Đã seed {Count} subscriptions và {Count2} payments.", subscriptions.Count, payments.Count);
    }

    private static async Task SeedGreetingsAsync(AppDbContext context, ILogger logger)
    {
        var userIds = await context.Users
            .Where(u => u.Role == UserRole.User && u.Status == UserStatus.Active)
            .Select(u => u.Id)
            .ToListAsync();

        var templateIds = await context.GreetingTemplates
            .Where(t => t.IsActive)
            .Select(t => t.Id)
            .ToListAsync();

        if (userIds.Count == 0 || templateIds.Count == 0) return;

        var greetings = new List<Greeting>
        {
            new()
            {
                UserId = userIds[0], TemplateId = templateIds[0],
                RecipientEmail = "banbe1@gmail.com", RecipientName = "Thùy Linh",
                SenderMessage = "Chúc bạn thêm một tuổi mới thật nhiều niềm vui và may mắn! 🎂",
                CustomHtml = "<div>...</div>",
                ReplyToEmail = "hoanguyen@gmail.com",
                Status = GreetingStatus.Sent, SentAt = DateTime.UtcNow.AddDays(-5),
                ViewCount = 3, UniqueToken = "abc123def456ghi789jkl012",
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new()
            {
                UserId = userIds[0], TemplateId = templateIds[3],
                RecipientEmail = "ongba@gmail.com", RecipientName = "Ông nội",
                SenderMessage = "Kính chúc ông nội năm mới sức khỏe dồi dào, sống lâu trăm tuổi!",
                CustomHtml = "<div>...</div>",
                ReplyToEmail = "hoanguyen@gmail.com",
                Status = GreetingStatus.Scheduled, ScheduledAt = DateTime.UtcNow.AddDays(3),
                UniqueToken = "mno345pqr678stu901vwx234",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                UserId = userIds[1], TemplateId = templateIds[6],
                RecipientEmail = "congty@gmail.com", RecipientName = "Toàn thể công ty",
                SenderMessage = "Chúc mừng các bạn đồng nghiệp thân mến!",
                CustomHtml = "<div>...</div>",
                ReplyToEmail = "minhtvn@yahoo.com",
                Status = GreetingStatus.Sent, SentAt = DateTime.UtcNow.AddDays(-2),
                ViewCount = 15, UniqueToken = "yz5678ab1234cd5678ef9012",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            // Guest greeting (UC08)
            new()
            {
                UserId = null,
                GuestSenderEmail = "guest@example.com", GuestSenderName = "Khách vãng lai",
                TemplateId = templateIds[0],
                RecipientEmail = "recipient@gmail.com", RecipientName = "Người nhận",
                SenderMessage = "Chúc mừng từ khách!",
                CustomHtml = "<div>...</div>",
                ReplyToEmail = "guest@example.com",
                Status = GreetingStatus.Sent, SentAt = DateTime.UtcNow.AddDays(-1),
                UniqueToken = "guest001token002sample003",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
        };

        await context.Greetings.AddRangeAsync(greetings);
        await context.SaveChangesAsync();
        logger.LogInformation("Đã seed {Count} thiệp.", greetings.Count);
    }

    private static async Task SeedWebContentsAsync(AppDbContext context, ILogger logger)
    {
        var adminId = await context.Users
            .Where(u => u.Role == UserRole.Admin)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        var contents = new List<WebContent>
        {
            new()
            {
                Key = "banner_home", Title = "Banner Trang Chủ",
                Content = "<div class='hero-banner'><h1>Gửi yêu thương qua từng tấm thiệp</h1><p>Hơn 500 mẫu thiệp đẹp cho mọi dịp</p></div>",
                ImageUrl = "/banners/home-banner.jpg",
                IsActive = true, Version = 1, UpdatedByUserId = adminId,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Key = "footer", Title = "Footer Website",
                Content = "<footer><p>© 2026 E-Greetings Vietnam. Gửi yêu thương qua từng tấm thiệp.</p><p>Hotline: 1900-xxxx | Email: support@egreetings.vn</p></footer>",
                IsActive = true, Version = 1, UpdatedByUserId = adminId,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Key = "about", Title = "Giới thiệu về E-Greetings",
                Content = "<section><h2>Về chúng tôi</h2><p>E-Greetings là nền tảng gửi thiệp điện tử hàng đầu Việt Nam. Chúng tôi giúp bạn chia sẻ yêu thương và kết nối với những người thân yêu qua những tấm thiệp điện tử đẹp và ý nghĩa.</p></section>",
                IsActive = true, Version = 1, UpdatedByUserId = adminId,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
        };

        await context.WebContents.AddRangeAsync(contents);
        await context.SaveChangesAsync();
        logger.LogInformation("Đã seed {Count} web contents.", contents.Count);
    }

    private static async Task SeedContactsAsync(AppDbContext context, ILogger logger)
    {
        var userId = await context.Users
            .Where(u => u.Email == "hoanguyen@gmail.com")
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        if (userId == 0) return;

        var contacts = new List<Contact>
        {
            new() { UserId = userId, Name = "Nguyễn Thị Mai", Email = "mainguyen@gmail.com", Phone = "0987654321", Birthday = new DateTime(1990, 7, 22), Note = "Bạn thân thời đại học", CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new() { UserId = userId, Name = "Phạm Anh Tuấn", Email = "tuanpa@company.vn", Phone = "0976543210", Birthday = new DateTime(1988, 11, 5), Note = "Đồng nghiệp cũ", CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new() { UserId = userId, Name = "Hoàng Thị Thu", Email = "thuhth@hotmail.com", Birthday = new DateTime(1992, 2, 14), Note = "Bạn cùng lớp cấp 3", CreatedAt = DateTime.UtcNow.AddDays(-15) },
        };

        await context.Contacts.AddRangeAsync(contacts);
        await context.SaveChangesAsync();
        logger.LogInformation("Đã seed {Count} contacts.", contacts.Count);
    }

    private static async Task SeedFeedbacksAsync(AppDbContext context, ILogger logger)
    {
        var userId = await context.Users
            .Where(u => u.Email == "minhtvn@yahoo.com")
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        var feedbacks = new List<Feedback>
        {
            new()
            {
                UserId = userId,
                Subject = "Trang web rất đẹp và dễ dùng",
                Content = "Mình đã dùng E-Greetings được 1 tháng và rất hài lòng. Các mẫu thiệp rất đa dạng và đẹp. Mong có thêm nhiều mẫu thiệp Tết hơn!",
                ContactEmail = "minhtvn@yahoo.com",
                IsRead = true,
                AdminReply = "Cảm ơn bạn đã phản hồi tích cực! Chúng tôi sẽ bổ sung thêm mẫu thiệp Tết trong thời gian tới.",
                RepliedAt = DateTime.UtcNow.AddDays(-3),
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                UserId = null,  // Anonymous
                Subject = "Báo lỗi: không gửi được thiệp",
                Content = "Khi mình nhấn gửi thiệp thì màn hình bị trắng, không thấy thông báo gì. Mình dùng Chrome trên máy tính.",
                ContactEmail = "khach@gmail.com",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
        };

        await context.Feedbacks.AddRangeAsync(feedbacks);
        await context.SaveChangesAsync();
        logger.LogInformation("Đã seed {Count} feedbacks.", feedbacks.Count);
    }
}
