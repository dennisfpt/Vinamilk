using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vinamilk.Data;
using Vinamilk.Models;
using Vinamilk.Models.Entities;

namespace Vinamilk.Controllers
{
    // Model dữ liệu thông báo cho khách hàng
    public class UserNotificationItem
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-bell";
    }

    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CART_KEY = "VinamilkCartSession";
        private const string NOTIF_KEY = "VinamilkUserNotifications";

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper: Lấy danh sách sản phẩm trong Session
        private List<CartItem> GetCartItems()
        {
            var sessionData = HttpContext.Session.GetString(CART_KEY);
            return string.IsNullOrEmpty(sessionData)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(sessionData) ?? new List<CartItem>();
        }

        // Helper: Lưu giỏ hàng vào Session
        private void SaveCartItems(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CART_KEY, JsonSerializer.Serialize(cart));
        }

        // Helper: Lấy thông báo chuông từ Session
        private List<UserNotificationItem> GetUserNotifications()
        {
            var data = HttpContext.Session.GetString(NOTIF_KEY);
            if (string.IsNullOrEmpty(data))
            {
                return new List<UserNotificationItem>();
            }
            return JsonSerializer.Deserialize<List<UserNotificationItem>>(data) ?? new List<UserNotificationItem>();
        }

        // Helper: Bắn thông báo mới vào chuông
        private void AddUserNotification(string title, string message, string icon = "bi-bell-fill")
        {
            var list = GetUserNotifications();
            list.Insert(0, new UserNotificationItem
            {
                Title = title,
                Message = message,
                TimeAgo = "Vừa xong",
                Icon = icon
            });
            HttpContext.Session.SetString(NOTIF_KEY, JsonSerializer.Serialize(list));
        }

        // Helper: Kiểm tra định dạng số điện thoại Việt Nam (10 chữ số, bắt đầu bằng 0)
        private bool IsValidVietnamesePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return Regex.IsMatch(phone.Trim(), @"^0\d{9}$");
        }

        // ==========================================
        // 1. TRANG CHỦ
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // ==========================================
        // 2. DANH MỤC SẢN PHẨM
        // ==========================================
        public IActionResult Products(string? keyword, int? categoryId)
        {
            var query = _context.Products.AsQueryable();

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = query.ToList();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kw = RemoveDiacritics(keyword.Trim().ToLower());
                products = products
                    .Where(p => RemoveDiacritics((p.Name ?? "").ToLower()).Contains(kw))
                    .ToList();
                ViewBag.SearchKeyword = keyword.Trim();
            }

            return View(products);
        }

        // Hàm phụ hỗ trợ tìm kiếm cả có dấu lẫn không dấu (ví dụ gõ "dielac" hoặc "sua bot" đều ra)
        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in normalized)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC).Replace('đ', 'd');
        }

        // ==========================================
        // 3. CHI TIẾT SẢN PHẨM & CẤU HÌNH XOAY 360 ĐỘ + DINH DƯỠNG TOÀN DIỆN
        // ==========================================
        public async Task<IActionResult> ProductDetail(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            ViewBag.RelatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(4)
                .ToListAsync();

            var nameLower = product.Name.ToLower();
            var imgLower = (product.ImageUrl ?? string.Empty).ToLower();

            // 1. Sữa uống Dielac Grow Plus Hương Chuối 110ml
            if (nameLower.Contains("chuối") || imgLower.Contains("banana"))
            {
                ViewBag.Product360Images = new List<string>
                {
                    "/images/dielac-grow-plus-banana-front.jpg",
                    "/images/dielac-grow-plus-banana-side-left.jpg",
                    "/images/dielac-grow-plus-banana-back.jpg",
                    "/images/dielac-grow-plus-banana-side-right.jpg"
                };
                product.ShortDescription = "Sữa (91,7%) (sữa tươi, nước, sữa bột, chất béo sữa), lactoza, đường, dầu thực vật, chất xơ hòa tan oligofructose (FOS), maltodextrin, nước chuối ép cô đặc, bột béo MCT, chất nhũ hóa (471, 460(i), 466, 407), các khoáng chất (tricalci phosphat, natri citrat, magnesi citrat, kẽm gluconat, sắt pyrophosphat, mangan sulfat, đồng gluconat, kali iodid, natri selenit), hương liệu tự nhiên và tổng hợp dùng cho thực phẩm, đạm whey giàu alpha-lactalbumin, bột sữa non (colostrum) (1 g/l), các vitamin (natri ascorbat, E, niacin, calci D-pantothenat, B6, B1, A, B2, acid folic, K1, K2, biotin, D3, B12), dầu cá giàu DHA, oligosaccharid (2'-Fucosyllactose (2'-FL)), cholin clorid, taurin, chất chống oxy hóa (hỗn hợp tocopherol concentrate, ascorbyl palmitat), hỗn hợp nucleotid. Có chứa sữa, đậu nành và dầu cá.";
                product.NutritionInfo = "Năng lượng / Energy|97 kcal#Chất đạm / Protein|3.65 g#Chất béo / Total fat|4.1 g#MCT (Medium chain triglycerides)|0.5 g#Acid linoleic|650 mg#Acid alpha-linolenic|66 mg#DHA|6.7 mg#Carbohydrat / Carbohydrate|11 g#Đường tổng số / Total sugars|9.4 g#Chất xơ hòa tan / Soluble dietary fiber|0.88 g#2'-Fucosyllactose (2'-FL)|22.6 mg#Nucleotid / Nucleotides|4.2 mg#Lysin / Lysine|297 mg#Taurin / Taurine|9.4 mg#Arginin / Arginine|95 mg#Threonin / Threonine|105 mg#Natri / Sodium|55 mg#Kali / Potassium|158 mg#Clorid / Chloride|99 mg#Calci / Calcium|187 mg#Phospho / Phosphorus|132 mg#Magnesi / Magnesium|12.4 mg#Mangan / Manganese|150 µg#Sắt / Iron|0.98 mg#Iod / Iodine|17.6 µg#Kẽm / Zinc|0.74 mg#Đồng / Copper|0.05 mg#Selen / Selenium|4 µg#Vitamin A|319 IU#Vitamin D3|88 IU#Vitamin E|1.8 mg TE#Vitamin K1|5.8 µg#Vitamin K2|4 µg#Vitamin C|11 mg#Vitamin B1|0.12 mg#Vitamin B2|0.18 mg#Niacin|1.4 mg#Vitamin B6|0.22 mg#Acid folic|33 µg#Acid pantothenic|0.53 mg#Vitamin B12|0.41 µg#Biotin|4.4 µg#Cholin / Choline|24.2 mg";
            }
            // 2. Sữa uống Dielac Grow Plus Hương Dâu 110ml
            else if (nameLower.Contains("dâu") || imgLower.Contains("strawberry"))
            {
                ViewBag.Product360Images = new List<string>
                {
                    "/images/dielac-grow-plus-strawberry-front.jpg",
                    "/images/dielac-grow-plus-strawberry-side-left.jpg",
                    "/images/dielac-grow-plus-strawberry-back.jpg",
                    "/images/dielac-grow-plus-strawberry-side-right.jpg"
                };
                product.ShortDescription = "Sữa (92%) (sữa tươi, nước, sữa bột, chất béo sữa), đường, lactoza, dầu thực vật, chất xơ hòa tan oligofructose (FOS), maltodextrin, bột béo MCT, chất nhũ hóa (471, 460(i), 466, 407), các khoáng chất (tricalci phosphat, natri citrat, magnesi citrat, kẽm gluconat, sắt pyrophosphat, mangan sulfat, đồng gluconat, kali iodid, natri selenit), hương dâu tự nhiên và tổng hợp, đạm whey giàu alpha-lactalbumin, bột sữa non (colostrum) (1 g/l), các vitamin (natri ascorbat, E, niacin, calci D-pantothenat, B6, B1, A, B2, acid folic, K1, K2, biotin, D3, B12), dầu cá giàu DHA, oligosaccharid (2'-Fucosyllactose (2'-FL)), cholin clorid, màu tự nhiên (120), taurin, chất chống oxy hóa (hỗn hợp tocopherol concentrate, ascorbyl palmitat), hỗn hợp nucleotid. Có chứa sữa, đậu nành và dầu cá.";
                product.NutritionInfo = "Năng lượng / Energy|97 kcal#Chất đạm / Protein|3.65 g#Chất béo / Total fat|4.1 g#MCT (Medium chain triglycerides)|0.5 g#Acid linoleic|650 mg#Acid alpha-linolenic|66 mg#DHA|6.7 mg#Carbohydrat / Carbohydrate|11 g#Đường tổng số / Total sugars|9.4 g#Chất xơ hòa tan / Soluble dietary fiber|0.88 g#2'-Fucosyllactose (2'-FL)|22.6 mg#Nucleotid / Nucleotides|4.2 mg#Lysin / Lysine|297 mg#Taurin / Taurine|9.4 mg#Arginin / Arginine|95 mg#Threonin / Threonine|105 mg#Natri / Sodium|55 mg#Kali / Potassium|158 mg#Clorid / Chloride|99 mg#Calci / Calcium|187 mg#Phospho / Phosphorus|132 mg#Magnesi / Magnesium|12.4 mg#Mangan / Manganese|150 µg#Sắt / Iron|0.98 mg#Iod / Iodine|17.6 µg#Kẽm / Zinc|0.74 mg#Đồng / Copper|0.05 mg#Selen / Selenium|4 µg#Vitamin A|319 IU#Vitamin D3|88 IU#Vitamin E|1.8 mg TE#Vitamin K1|5.8 µg#Vitamin K2|4 µg#Vitamin C|11 mg#Vitamin B1|0.12 mg#Vitamin B2|0.18 mg#Niacin|1.4 mg#Vitamin B6|0.22 mg#Acid folic|33 µg#Acid pantothenic|0.53 mg#Vitamin B12|0.41 µg#Biotin|4.4 µg#Cholin / Choline|24.2 mg";
            }
            // 3. Sữa bột Vinamilk Dielac Gold 2 Lon 800g
            else if (nameLower.Contains("gold 2") || imgLower.Contains("gold-2"))
            {
                ViewBag.Product360Images = new List<string>
                {
                    "/images/dielac-gold-2-front.jpg",
                    "/images/dielac-gold-2-side.jpg",
                    "/images/dielac-gold-2-back.jpg",
                    "/images/dielac-gold-2-side.jpg"
                };
                product.ShortDescription = "natri citrat, kali clorid, magnesi oxyd, sắt sulfat, kẽm sulfat, đồng sulfat, mangan sulfat, kali iodid, natri selenit, DHA từ dầu cá, ARA từ dầu M. alpina, cholin clorid, các vitamin (natri ascorbat, E, niacin, calci D-pantothenat, B1, B2, A, B6, acid folic, K1, biotin, D3, B12), taurin, Bifidobacterium (BB-12™) (probiotics), myoinositol, L-Carnitin tartrat, chất chống oxy hóa (hỗn hợp tocopherol concentrate, ascorbyl palmitat), lutein. Có chứa sữa, đậu nành và dầu cá.";
                product.NutritionInfo = "Năng lượng|496 kcal#Chất đạm|15 g#IgG (từ sữa non)|360 mg#Chất béo|23 g#Acid linoleic|3600 mg#Acid alpha-linolenic|400 mg#ARA (Arachidonic acid)|58 mg#DHA (Docosahexaenoic acid)|58 mg#Carbohydrat|51 g#Đường tổng số|29 g#Chất xơ hòa tan/FOS|2.0 g#Canxi / Calcium|510 mg#Phospho|390 mg#Magnesi|50 mg#Sắt / Iron|7.0 mg#Kẽm / Zinc|5.5 mg#Vitamin A|1650 IU#Vitamin D3|336 IU#Vitamin C|70 mg#Cholin|80 mg";
            }
            // 4. Sữa bột Vinamilk Dielac Grow Plus 2+ Lon 1.4kg
            else if (nameLower.Contains("grow plus 2+") || nameLower.Contains("grow-plus") || imgLower.Contains("grow-plus-front"))
            {
                ViewBag.Product360Images = new List<string>
                {
                    "/images/dielac-grow-plus-front.jpg",
                    "/images/dielac-grow-plus-side.jpg",
                    "/images/dielac-grow-plus-back.jpg",
                    "/images/dielac-grow-plus-side.jpg"
                };
                product.ShortDescription = "Bột sữa, maltodextrin, đạm whey giàu alpha-lactalbumin, dầu thực vật, bột béo MCT, chất xơ hòa tan (FOS), sữa non (colostrum) (0,8%), DHA từ dầu cá, oligosaccharid (2'-FL), cholin, Bifidobacterium (BB-12™), hỗn hợp nucleotid, các vitamin (A, D3, E, K1, K2, C, B1, B2, B6, B12, acid folic) và khoáng chất (tricalci phosphat, kali iodid, kẽm sulfat, sắt pyrophosphat). Có chứa sữa, đậu nành và dầu cá.";
                product.NutritionInfo = "Năng lượng|474 kcal#Chất đạm / Protein|17.5 g#Tryptophan|190 mg#IgG (từ sữa non)|160 mg#Chất béo / Total fat|22 g#MCT (Medium chain triglycerides)|2.3 g#Acid linoleic|2500 mg#DHA (Docosahexaenoic acid)|40 mg#Carbohydrat|50.4 g#2'-Fucosyllactose (2'-FL)|110 mg#Lysin|1300 mg#Taurin|44 mg#Nucleotid|20 mg#Canxi / Calcium|700 mg#Phospho|460 mg#Magnesi|48 mg#Kẽm / Zinc|5.9 mg#Vitamin A|1540 IU#Vitamin D3|400 IU#Vitamin K2|9 µg";
            }
            // 5. Sữa uống dinh dưỡng Dielac Grow 180ml
            else if (nameLower.Contains("180ml") || imgLower.Contains("dielac-grow-front"))
            {
                ViewBag.Product360Images = new List<string>
                {
                    "/images/dielac-grow-front.jpg",
                    "/images/dielac-grow-side-left.jpg",
                    "/images/dielac-grow-back.jpg",
                    "/images/dielac-grow-side-right.jpg"
                };
                product.ShortDescription = "Sữa (93,7%) (sữa tươi, nước, sữa bột, chất béo sữa), đường, chất xơ hòa tan (inulin, oligofructose (FOS)), maltodextrin, lactoza, dầu thực vật, chất nhũ hóa (471, 466, 407), các khoáng chất (tricalci phosphat, natri citrat, magnesi citrat, sắt pyrophosphat, kẽm gluconat, mangan sulfat, đồng gluconat, kali iodid, natri selenit), bột sữa non (colostrum) (1 g/l), các vitamin (natri ascorbat, E, niacin, calci D-pantothenat, B6, B1, A, B2, acid folic, D3, K1, K2, biotin, B12), dầu cá giàu DHA, cholin clorid, taurin, chất chống oxy hóa. Có chứa sữa, đậu nành và dầu cá.";
                product.NutritionInfo = "Năng lượng / Energy|138.4 kcal#Chất đạm / Protein|5.67 g#Chất béo / Total fat|5.85 g#Acid linoleic|504 mg#Acid alpha-linolenic|39.6 mg#DHA|6.8 mg#Carbohydrat / Carbohydrate|15.1 g#Đường tổng số / Total sugars|13.2 g#Chất xơ hòa tan / Soluble dietary fiber|1.8 g#Lysin / Lysine|486 mg#Taurin / Taurine|13.1 mg#Natri / Sodium|90 mg#Kali / Potassium|259 mg#Calci / Calcium|273 mg#Phospho / Phosphorus|187 mg#Magnesi / Magnesium|21.6 mg#Sắt / Iron|1.44 mg#Kẽm / Zinc|1.08 mg#Vitamin A|414 IU#Vitamin D3|327 IU#Vitamin K1|9.5 µg#Vitamin K2|6.5 µg#Vitamin C|18 mg#Cholin / Choline|27.0 mg";
            }
            // 6. Phô mai miếng tam giác Vinamilk Hộp 8 miếng 120g
            else if (nameLower.Contains("phô mai") || imgLower.Contains("phomai"))
            {
                ViewBag.Product360Images = new List<string>
                {
                    "/images/phomai-vinamilk-box-front.jpg",
                    "/images/phomai-vinamilk-inside-open.jpg",
                    "/images/phomai-vinamilk-single-piece.jpg",
                    "/images/phomai-vinamilk-box-back.jpg"
                };
                product.ShortDescription = "Sữa tươi, bơ, đạm sữa, phô mai (21%), nước, sữa bột, chất điều chỉnh độ acid (330, 339(iii)), muối ăn, chất ổn định (452(i), 407a, 401, 508). Có chứa sữa.";
                product.NutritionInfo = "Năng lượng / Energy|293.4 kcal#Chất đạm / Protein|9.0 g#Chất béo / Total Fat|26.2 g#Carbohydrat / Carbohydrate|5.4 g#Natri / Sodium|445 mg#Calci / Calcium|300 mg";
            }
            else
            {
                ViewBag.Product360Images = new List<string> { product.ImageUrl ?? "/images/hero-banner.jpg" };
            }

            return View(product);
        }

        // ==========================================
        // 4. TRANG ĐÍCH (LANDING PAGE) & ĐĂNG KÝ LEAD
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> LandingPage()
        {
            ViewBag.IsCampaignActive = CampaignConfig.IsActive;
            ViewBag.DiscountPercent = CampaignConfig.DiscountPercent;
            ViewBag.CampaignTitle = CampaignConfig.CampaignTitle;
            ViewBag.CampaignSubtitle = CampaignConfig.CampaignSubtitle;
            ViewBag.BannerImageUrl = CampaignConfig.BannerImageUrl;

            var featuredProducts = await _context.Products
                .Where(p => CampaignConfig.FeaturedProductIds.Contains(p.Id) || p.IsFeatured)
                .Take(8)
                .ToListAsync();

            return View(featuredProducts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitLead(string fullName, string email, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập địa chỉ Email!";
                return RedirectToAction(nameof(LandingPage));
            }

            // Kiểm tra ràng buộc tối đa đúng 10 số bắt đầu bằng 0
            if (!IsValidVietnamesePhone(phoneNumber))
            {
                TempData["ErrorMessage"] = "Số điện thoại không hợp lệ! Vui lòng nhập đúng 10 chữ số (bắt đầu bằng số 0).";
                return RedirectToAction(nameof(LandingPage));
            }

            try
            {
                var cleanName = string.IsNullOrWhiteSpace(fullName) ? "Khách hàng" : fullName.Trim();
                var cleanEmail = email.Trim();
                var cleanPhone = phoneNumber.Trim();

                var lead = await _context.Leads
                    .Include(l => l.Activities)
                    .FirstOrDefaultAsync(l => l.Email == cleanEmail);

                int discountPercent = CampaignConfig.DiscountPercent;

                if (lead == null)
                {
                    lead = new Lead
                    {
                        FullName = cleanName,
                        Email = cleanEmail,
                        PhoneNumber = cleanPhone,
                        Source = "LandingPage",
                        LeadScore = 20,
                        Status = "New",
                        CreatedAt = DateTime.Now,
                        Activities = new List<LeadActivity>()
                    };

                    lead.Activities.Add(new LeadActivity
                    {
                        ActivityType = "FormSubmit",
                        PointsAwarded = 20,
                        Description = $"Đăng ký nhận ưu đãi ({discountPercent}%) từ Landing Page (+20đ)",
                        CreatedAt = DateTime.Now
                    });

                    _context.Leads.Add(lead);
                }
                else
                {
                    lead.FullName = cleanName;
                    lead.PhoneNumber = cleanPhone;
                    lead.LeadScore += 10;

                    if (lead.Activities == null)
                    {
                        lead.Activities = new List<LeadActivity>();
                    }

                    lead.Activities.Add(new LeadActivity
                    {
                        ActivityType = "ReEngage",
                        PointsAwarded = 10,
                        Description = "Đăng ký lại thông tin từ Landing Page (+10đ)",
                        CreatedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();

                try
                {
                    AdminController.AutomationStore.TriggerFlow(1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi TriggerFlow: " + ex.Message);
                }

                HttpContext.Session.SetString("UserFullName", cleanName);
                string voucher = CampaignConfig.VoucherCode ?? "VNM30";
                HttpContext.Session.SetString("UnlockedVoucher", voucher);

                AddUserNotification(
                    "Đăng Ký Ưu Đãi Thành Công",
                    $"Xin chào {cleanName}, yêu cầu nhận voucher {discountPercent}% của bạn đã được ghi nhận. Mã ưu đãi kích hoạt đang được gửi vào hòm thư {cleanEmail}. Vui lòng kiểm tra email!",
                    "bi-envelope-check-fill"
                );

                TempData["SuccessMessage"] = $"Đăng ký thành công! Hệ thống đã ghi nhận thông tin và gửi hướng dẫn kích hoạt vào hòm thư {cleanEmail}.";
                return RedirectToAction(nameof(LandingPage));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi SubmitLead: " + ex.ToString());
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thông tin, vui lòng thử lại!";
                return RedirectToAction(nameof(LandingPage));
            }
        }

        // ==========================================
        // 5. GIỚI THIỆU & TIN TỨC
        // ==========================================
        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        public IActionResult News()
        {
            return View();
        }

        // ==========================================
        // 6. GIỎ HÀNG
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            SaveCartItems(cart);
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        public async Task<IActionResult> BuyNow(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            SaveCartItems(cart);
            return RedirectToAction(nameof(Checkout));
        }

        [HttpGet]
        public IActionResult Cart()
        {
            var cart = GetCartItems();
            ViewBag.TotalAmount = cart.Sum(c => c.Total);
            return View(cart);
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }
            SaveCartItems(cart);
            return RedirectToAction(nameof(Cart));
        }

        [HttpGet]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCartItems();
            cart.RemoveAll(c => c.ProductId == productId);
            SaveCartItems(cart);
            return RedirectToAction(nameof(Cart));
        }

        // ==========================================
        // 7. THANH TOÁN (CHECKOUT) - BẮT BUỘC ĐĂNG NHẬP / ĐĂNG KÝ
        // ==========================================
        [HttpGet]
        public IActionResult Checkout()
        {
            // 1. Kiểm tra trạng thái đăng nhập của khách hàng
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập hoặc đăng ký tài khoản để tiếp tục thanh toán và nhận ưu đãi!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Home/Checkout" });
            }

            // 2. Kiểm tra giỏ hàng
            var cart = GetCartItems();
            if (cart == null || !cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống! Vui lòng chọn sản phẩm trước khi thanh toán.";
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Cart = cart;
            ViewBag.SubTotal = cart.Sum(c => c.Total);
            ViewBag.ShippingFee = 15000m;
            ViewBag.GrandTotal = ViewBag.SubTotal + ViewBag.ShippingFee;

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(string fullName, string phoneNumber, string address, string paymentMethod, string? note)
        {
            // Kiểm tra số điện thoại tối đa đúng 10 số
            if (!IsValidVietnamesePhone(phoneNumber))
            {
                TempData["ErrorMessage"] = "Số điện thoại người nhận phải gồm đúng 10 chữ số và bắt đầu bằng số 0!";
                return RedirectToAction(nameof(Checkout));
            }

            var cart = GetCartItems();
            if (!cart.Any())
            {
                return RedirectToAction(nameof(Products));
            }

            string paymentCode = "VNM" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            decimal totalAmount = cart.Sum(c => c.Total) + 15000m;

            var order = new Order
            {
                CustomerName = fullName.Trim(),
                PhoneNumber = phoneNumber.Trim(),
                Address = address.Trim(),
                Note = string.IsNullOrEmpty(note) ? $"Mã CK: {paymentCode}" : $"{note.Trim()} | Mã CK: {paymentCode}",
                PaymentMethod = paymentMethod ?? "COD",
                TotalAmount = totalAmount,
                Status = (paymentMethod == "Banking") ? "Chờ thanh toán QR" : "Đang xử lý",
                CreatedAt = DateTime.Now
            };

            foreach (var item in cart)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            AddUserNotification(
                $"Đặt hàng thành công #{order.Id}",
                $"Đơn hàng {totalAmount:N0}đ của bạn đã được ghi nhận. Trạng thái: {order.Status}",
                "bi-bag-check-fill"
            );

            HttpContext.Session.Remove(CART_KEY);

            if (paymentMethod == "Banking")
            {
                string bankAccount = "0865156242";
                string bankCode = "MB";
                long amount = Convert.ToInt64(totalAmount);
                string accountName = Uri.EscapeDataString("VINAMILK STORE");
                string memo = Uri.EscapeDataString(paymentCode);

                string qrUrl = $"https://img.vietqr.io/image/{bankCode}-{bankAccount}-compact2.png?amount={amount}&addInfo={memo}&accountName={accountName}";

                ViewBag.QrUrl = qrUrl;
                ViewBag.PaymentCode = paymentCode;
                ViewBag.BankAccount = bankAccount;

                return View("PaymentWaiting", order);
            }

            return View("OrderSuccess", order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = "Đã thanh toán (Chờ xác nhận)";
                await _context.SaveChangesAsync();

                AddUserNotification(
                    $"Xác nhận thanh toán #{order.Id}",
                    "Hệ thống đã nhận thông tin chuyển khoản và đang duyệt.",
                    "bi-qr-code-scan"
                );
            }
            return View("OrderSuccess", order);
        }

        // ==========================================
        // 8. TÀI KHOẢN & HỆ THỐNG CỬA HÀNG
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Redirect("/Account/Login");
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.UserEmail = User.Identity?.Name;
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Stores()
        {
            var stores = await _context.Stores.ToListAsync();
            return View(stores);
        }

        [HttpGet]
        public IActionResult GreenFarmSurvey()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GreenFarmSurvey(GreenFarmSurvey model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CreatedAt = DateTime.Now;
            _context.GreenFarmSurveys.Add(model);

            // Tự động cộng 15 điểm LeadScore nếu khách đã có trong bảng Leads
            var existingLead = _context.Leads.FirstOrDefault(l => l.PhoneNumber == model.PhoneNumber);
            if (existingLead != null)
            {
                existingLead.LeadScore += 15;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cảm ơn Anh/Chị đã hoàn thành khảo sát sữa tươi Vinamilk Green Farm!";
            return RedirectToAction("GreenFarmSurvey");
        }
    }
}