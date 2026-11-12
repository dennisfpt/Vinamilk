using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vinamilk.Data;
using Vinamilk.Models;
using Vinamilk.Models.Entities;

namespace Vinamilk.Controllers
{
    // Cấu hình chiến dịch khuyến mãi dùng chung
    public static class CampaignConfig
    {
        public static bool IsActive { get; set; } = true;
        public static int DiscountPercent { get; set; } = 30;
        public static string VoucherCode { get; set; } = "VNM30";
        public static string CampaignTitle { get; set; } = "Tặng Ngay Voucher Giảm";
        public static string CampaignSubtitle { get; set; } = "Khám phá nguồn dinh dưỡng thuần khiết chuẩn quốc tế từ hệ thống trang trại sinh thái Vinamilk Green Farm. Đăng ký nhận mã ưu đãi áp dụng cho sản phẩm ưu đãi!";
        public static string BannerImageUrl { get; set; } = "/images/hero-banner.jpg";
        public static List<int> FeaturedProductIds { get; set; } = new List<int>();
    }

    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper: Kiểm tra quyền Admin (chỉ email admin được vào hệ thống)
        private IActionResult? CheckAdminRole()
        {
            if (User.Identity?.Name != "admin@vinamilk.com.vn")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            return null;
        }

        // Helper: Tính số thông báo chưa xử lý
        private async Task SetNotificationBadge()
        {
            var pendingOrders = await _context.Orders
                .CountAsync(o => o.Status == "Chờ thanh toán QR" || o.Status == "Đã thanh toán (Chờ xác nhận)" || o.Status == "Đang xử lý");
            var newLeads = await _context.Leads.CountAsync(l => l.Status == "New");
            ViewBag.NotificationCount = pendingOrders + newLeads;
        }

        // ==========================================
        // 1. DASHBOARD CHÍNH
        // ==========================================
        public async Task<IActionResult> Index(string timeRange = "7days")
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();

            DateTime now = DateTime.Now;
            DateTime? startDate = null;
            DateTime? endDate = now;

            switch (timeRange.ToLower())
            {
                case "today":
                    startDate = now.Date;
                    break;
                case "7days":
                    startDate = now.Date.AddDays(-7);
                    break;
                case "30days":
                    startDate = now.Date.AddDays(-30);
                    break;
                case "thisyear":
                    startDate = new DateTime(now.Year, 1, 1);
                    break;
                default:
                    if (int.TryParse(timeRange, out int selectedYear))
                    {
                        startDate = new DateTime(selectedYear, 1, 1);
                        endDate = new DateTime(selectedYear, 12, 31, 23, 59, 59);
                    }
                    else
                    {
                        startDate = now.Date.AddDays(-7);
                        timeRange = "7days";
                    }
                    break;
            }

            var ordersQuery = _context.Orders.AsQueryable();
            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate.Value && o.CreatedAt <= endDate.Value);
            }

            ViewBag.TotalOrders = await ordersQuery.CountAsync();
            ViewBag.TotalRevenue = await ordersQuery
                .Where(o => o.Status != "Đã hủy")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            ViewBag.TotalCustomers = await _context.Users.CountAsync();
            ViewBag.TotalLeads = await _context.Leads.CountAsync();
            ViewBag.CurrentTimeRange = timeRange;

            var availableYears = await _context.Orders
                .Select(o => o.CreatedAt.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            if (!availableYears.Contains(now.Year))
            {
                availableYears.Insert(0, now.Year);
            }
            ViewBag.AvailableYears = availableYears;

            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(recentOrders);
        }

        // ==========================================
        // 2. QUẢN LÝ SẢN PHẨM
        // ==========================================
        public async Task<IActionResult> Products()
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(string name, int categoryId, decimal price, int stockQuantity, string? imageUrl, string? shortDescription, bool isFeatured = true)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Tên sản phẩm không được để trống!";
                return RedirectToAction(nameof(Products));
            }

            try
            {
                var product = new Product
                {
                    Name = name.Trim(),
                    CategoryId = categoryId,
                    Price = price,
                    StockQuantity = stockQuantity > 0 ? stockQuantity : 100,
                    ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? "/images/sua-tuoi-co-duong.jpg" : imageUrl.Trim(),
                    ShortDescription = shortDescription ?? string.Empty,
                    IsFeatured = isFeatured
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã thêm sản phẩm '{product.Name}' thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi thêm: {ex.Message}";
            }

            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, string name, int categoryId, decimal price, int stockQuantity, string? imageUrl, string? shortDescription)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            var existing = await _context.Products.FindAsync(id);
            if (existing != null)
            {
                existing.Name = name.Trim();
                existing.CategoryId = categoryId;
                existing.Price = price;
                existing.StockQuantity = stockQuantity;
                if (!string.IsNullOrWhiteSpace(imageUrl)) existing.ImageUrl = imageUrl.Trim();
                if (!string.IsNullOrWhiteSpace(shortDescription)) existing.ShortDescription = shortDescription;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật sản phẩm '{existing.Name}'!";
            }
            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa sản phẩm '{product.Name}'!";
            }
            return RedirectToAction(nameof(Products));
        }

        // ==========================================
        // 3. QUẢN LÝ DANH MỤC
        // ==========================================
        public async Task<IActionResult> Categories()
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            var categories = await _context.Categories.Include(c => c.Products).ToListAsync();
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string name)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            if (!string.IsNullOrWhiteSpace(name))
            {
                _context.Categories.Add(new Category { Name = name.Trim() });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm danh mục mới thành công!";
            }
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
            if (category != null)
            {
                if (category.Products != null && category.Products.Any())
                {
                    TempData["ErrorMessage"] = "Không thể xóa danh mục đang có sản phẩm!";
                }
                else
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa danh mục thành công!";
                }
            }
            return RedirectToAction(nameof(Categories));
        }

        // ==========================================
        // 4. QUẢN LÝ ĐƠN HÀNG
        // ==========================================
        public async Task<IActionResult> Orders(string? status)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            ViewBag.CurrentStatus = status;
            var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = newStatus;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn #{orderId} thành: {newStatus}";
            }
            return RedirectToAction(nameof(Orders), new { status = ViewBag.CurrentStatus });
        }

        // ==========================================
        // 5. QUẢN LÝ KHÁCH HÀNG
        // ==========================================
        public async Task<IActionResult> Customers()
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // ==========================================
        // 6. CẤU HÌNH KHUYẾN MÃI (MARKETING ĐỘNG)
        // ==========================================
        public async Task<IActionResult> Promotions()
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            ViewBag.DiscountPercent = CampaignConfig.DiscountPercent;
            ViewBag.VoucherCode = CampaignConfig.VoucherCode;
            ViewBag.CampaignTitle = CampaignConfig.CampaignTitle;
            ViewBag.CampaignSubtitle = CampaignConfig.CampaignSubtitle;
            ViewBag.BannerImageUrl = CampaignConfig.BannerImageUrl;
            ViewBag.SelectedProductIds = CampaignConfig.FeaturedProductIds;
            ViewBag.AllProducts = await _context.Products.ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCampaign(bool isActive, int discountPercent, string voucherCode, string campaignTitle, string campaignSubtitle, string? bannerImageUrl, List<int>? productIds)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            CampaignConfig.IsActive = isActive;
            CampaignConfig.DiscountPercent = discountPercent;
            CampaignConfig.VoucherCode = voucherCode.ToUpper().Trim();
            CampaignConfig.CampaignTitle = campaignTitle.Trim();
            CampaignConfig.CampaignSubtitle = campaignSubtitle.Trim();
            CampaignConfig.BannerImageUrl = string.IsNullOrWhiteSpace(bannerImageUrl) ? "/images/hero-banner.jpg" : bannerImageUrl.Trim();
            CampaignConfig.FeaturedProductIds = productIds ?? new List<int>();

            TempData["SuccessMessage"] = isActive
                ? "Đã lưu chiến dịch đang hoạt động!"
                : "Đã tạm dừng/kết thúc chiến dịch khuyến mãi!";
            return RedirectToAction(nameof(Promotions));
        }

        // ==========================================
        // 7. BÀI VIẾT & TIN TỨC ĐỘNG
        // ==========================================
        public class ArticleItem
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Summary { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string PublishDate { get; set; } = string.Empty;
            public string Status { get; set; } = "Đã xuất bản";
        }

        public static class ArticleStore
        {
            public static List<ArticleItem> Articles { get; set; } = new List<ArticleItem>
            {
                new ArticleItem
                {
                    Id = 1,
                    Title = "Vinamilk ra mắt dòng sản phẩm sữa tươi ít đường bổ sung vi chất",
                    Category = "Dinh dưỡng",
                    Summary = "Đáp ứng xu hướng tiêu dùng lành mạnh, bảo vệ sức khỏe cho thế hệ trẻ năng động.",
                    ImageUrl = "/images/sua-tuoi-co-duong.jpg",
                    Author = "Admin",
                    PublishDate = "Hôm nay",
                    Status = "Đã xuất bản"
                },
                new ArticleItem
                {
                    Id = 2,
                    Title = "Hành trình Net Zero 2050: Bước tiến mới của các trang trại sinh thái",
                    Category = "Bền vững",
                    Summary = "Ứng dụng 100% năng lượng tái tạo và hệ thống xử lý sinh học hiện đại.",
                    ImageUrl = "/images/hero-banner.jpg",
                    Author = "Admin",
                    PublishDate = "02/11/2026",
                    Status = "Đã xuất bản"
                }
            };
        }

        public async Task<IActionResult> Content()
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            return View(ArticleStore.Articles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateArticle(string title, string category, string summary, string? imageUrl)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            if (!string.IsNullOrWhiteSpace(title))
            {
                int newId = ArticleStore.Articles.Any() ? ArticleStore.Articles.Max(a => a.Id) + 1 : 1;
                ArticleStore.Articles.Insert(0, new ArticleItem
                {
                    Id = newId,
                    Title = title.Trim(),
                    Category = string.IsNullOrWhiteSpace(category) ? "Tin tức" : category,
                    Summary = summary ?? string.Empty,
                    ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? "/images/sua-tuoi-co-duong.jpg" : imageUrl.Trim(),
                    Author = User.Identity?.Name ?? "Admin",
                    PublishDate = DateTime.Now.ToString("dd/MM/yyyy"),
                    Status = "Đã xuất bản"
                });
                TempData["SuccessMessage"] = $"Đã xuất bản bài viết '{title}' thành công!";
            }
            return RedirectToAction(nameof(Content));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteArticle(int id)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            ArticleStore.Articles.RemoveAll(a => a.Id == id);
            TempData["SuccessMessage"] = "Đã xóa bài viết thành công!";
            return RedirectToAction(nameof(Content));
        }

        // ==========================================
        // 8. QUẢN LÝ LEAD
        // ==========================================
        public async Task<IActionResult> Leads()
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            var leads = await _context.Leads
                .Include(l => l.Activities)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
            return View(leads);
        }

        // ==========================================
        // 9. EMAIL MARKETING & LỊCH SỬ GỬI ĐỘNG
        // ==========================================
        public class CampaignHistoryItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Subject { get; set; } = string.Empty;
            public int RecipientsCount { get; set; }
            public string SentAt { get; set; } = string.Empty;
            public string Status { get; set; } = "Hoàn thành";
        }

        public static class EmailCampaignStore
        {
            public static List<CampaignHistoryItem> Histories { get; set; } = new List<CampaignHistoryItem>
            {
                new CampaignHistoryItem
                {
                    Id = 1,
                    Name = "Voucher chào mừng Lead mới",
                    Subject = "[Vinamilk] Quà tặng chào mừng",
                    RecipientsCount = 12,
                    SentAt = "Hôm qua",
                    Status = "Tự động"
                }
            };
        }

        public async Task<IActionResult> EmailMarketing()
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            var leadEmails = await _context.Leads.Where(l => !string.IsNullOrEmpty(l.Email)).Select(l => l.Email).ToListAsync();
            var userEmails = await _context.Users.Where(u => !string.IsNullOrEmpty(u.Email)).Select(u => u.Email).ToListAsync();

            ViewBag.TotalRecipients = leadEmails.Concat(userEmails).Distinct().Count();
            ViewBag.Histories = EmailCampaignStore.Histories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCampaign(string campaignName, string subject, string content)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            if (string.IsNullOrWhiteSpace(campaignName) || string.IsNullOrWhiteSpace(subject))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập tên chiến dịch và tiêu đề email!";
                return RedirectToAction(nameof(EmailMarketing));
            }

            var leadEmails = await _context.Leads.Where(l => !string.IsNullOrEmpty(l.Email)).Select(l => l.Email).ToListAsync();
            var userEmails = await _context.Users.Where(u => !string.IsNullOrEmpty(u.Email)).Select(u => u.Email).ToListAsync();
            var totalRecipients = leadEmails.Concat(userEmails).Distinct().Count();

            EmailCampaignStore.Histories.Insert(0, new CampaignHistoryItem
            {
                Id = EmailCampaignStore.Histories.Count + 1,
                Name = campaignName.Trim(),
                Subject = subject.Trim(),
                RecipientsCount = totalRecipients > 0 ? totalRecipients : 1,
                SentAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                Status = "Hoàn thành"
            });

            TempData["SuccessMessage"] = $"Đã gửi thành công chiến dịch '{campaignName}' tới {totalRecipients} người nhận!";
            return RedirectToAction(nameof(EmailMarketing));
        }

        // ==========================================
        // 10. CHATBOT AI (QUẢN LÝ KỊCH BẢN TRẢ LỜI)
        // ==========================================
        public async Task<IActionResult> Chatbot()
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            var rules = await _context.ChatbotRules.OrderByDescending(r => r.Id).ToListAsync();
            return View(rules);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChatbotRule(ChatbotRule model)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            if (ModelState.IsValid)
            {
                _context.ChatbotRules.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã lưu kịch bản phản hồi mới thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ từ khóa và nội dung câu trả lời!";
            }
            return RedirectToAction(nameof(Chatbot));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteChatbotRule(int id)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            var rule = await _context.ChatbotRules.FindAsync(id);
            if (rule != null)
            {
                _context.ChatbotRules.Remove(rule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa quy tắc phản hồi thành công!";
            }
            return RedirectToAction(nameof(Chatbot));
        }

        // ==========================================
        // 11. MARKETING AUTOMATION
        // ==========================================
        public class AutomationFlow
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string TriggerDescription { get; set; } = string.Empty;
            public string ActionDescription { get; set; } = string.Empty;
            public bool IsActive { get; set; } = true;
            public int ExecutionCount { get; set; } = 0;
            public DateTime? LastExecutedAt { get; set; }
        }

        public static class AutomationStore
        {
            public static List<AutomationFlow> Flows { get; set; } = new List<AutomationFlow>
            {
                new AutomationFlow
                {
                    Id = 1,
                    Name = "Khách điền Form Landing Page → Tự động tặng Voucher",
                    TriggerDescription = "Kích hoạt: Khi Lead bấm gửi Form từ trang Landing Page.",
                    ActionDescription = "Ghi nhận Lead, gửi email thông báo mã giảm giá theo chiến dịch.",
                    IsActive = true,
                    ExecutionCount = 3,
                    LastExecutedAt = DateTime.Now.AddHours(-1)
                },
                new AutomationFlow
                {
                    Id = 2,
                    Name = "Nhắc nhở khách bỏ quên giỏ hàng sau 24h",
                    TriggerDescription = "Kích hoạt: Khi phiên giỏ hàng (Cart Session) lưu > 24h chưa checkout.",
                    ActionDescription = "Gửi thông báo email tự động nhắc nhở kèm ưu đãi giỏ hàng.",
                    IsActive = true,
                    ExecutionCount = 0,
                    LastExecutedAt = null
                }
            };

            public static void TriggerFlow(int flowId)
            {
                var flow = Flows.FirstOrDefault(f => f.Id == flowId && f.IsActive);
                if (flow != null)
                {
                    flow.ExecutionCount++;
                    flow.LastExecutedAt = DateTime.Now;
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Automation()
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            return View(AutomationStore.Flows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFlow(int id)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            var flow = AutomationStore.Flows.FirstOrDefault(f => f.Id == id);
            if (flow != null)
            {
                flow.IsActive = !flow.IsActive;
                TempData["SuccessMessage"] = $"Đã {(flow.IsActive ? "BẬT" : "TẮT")} quy trình '{flow.Name}'!";
            }
            return RedirectToAction(nameof(Automation));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TestRunFlow(int id)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            AutomationStore.TriggerFlow(id);
            TempData["SuccessMessage"] = $"Đã kích hoạt chạy thử nghiệm thành công quy trình #{id}!";
            return RedirectToAction(nameof(Automation));
        }

        // ==========================================
        // 12. BÁO CÁO & THỐNG KÊ
        // ==========================================
        public async Task<IActionResult> Reports()
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.Status != "Đã hủy")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalLeads = await _context.Leads.CountAsync();
            return View();
        }

        // ==========================================
        // 13. CÀI ĐẶT HỆ THỐNG
        // ==========================================
        public async Task<IActionResult> Settings()
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            return View();
        }

        // ==========================================
        // 14. TRÌNH THIẾT KẾ LANDING PAGE
        // ==========================================
        public async Task<IActionResult> PageBuilder()
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            return View();
        }

        [HttpPost]
        public IActionResult SaveLandingPage([FromBody] PageData data)
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            System.IO.File.WriteAllText("wwwroot/landing-preview.html", data.Html + "<style>" + data.Css + "</style>");
            return Json(new { success = true });
        }

        // ==========================================
        // 15. QUẢN LÝ KHẢO SÁT GREEN FARM
        // ==========================================
        public async Task<IActionResult> Surveys()
        {
            var check = CheckAdminRole();
            if (check != null) return check;

            await SetNotificationBadge();
            return View();
        }

        public class PageData
        {
            public string Html { get; set; } = string.Empty;
            public string Css { get; set; } = string.Empty;
        }
    }
}