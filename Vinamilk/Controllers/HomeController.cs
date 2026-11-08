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
        public async Task<IActionResult> Products(int? categoryId, string? keyword, string? sort)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.Name.ToLower().Contains(keyword.ToLower()));
            }

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Id)
            };

            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;
            ViewBag.Categories = categories;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentKeyword = keyword;
            ViewBag.CurrentSort = sort;

            return View(await query.ToListAsync());
        }

        // ==========================================
        // 3. CHI TIẾT SẢN PHẨM & CẤU HÌNH XOAY 360 ĐỘ
        // ==========================================
        public async Task<IActionResult> ProductDetail(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            ViewBag.RelatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(4)
                .ToListAsync();

            // Khởi tạo danh sách các góc ảnh xoay 360 độ dựa trên ảnh đại diện của sản phẩm
            var images360 = new List<string>();
            var img = (product.ImageUrl ?? string.Empty).ToLower();

            if (img.Contains("banana"))
            {
                images360 = new List<string>
                {
                    "/images/dielac-grow-plus-banana-front.jpg",
                    "/images/dielac-grow-plus-banana-side-left.jpg",
                    "/images/dielac-grow-plus-banana-back.jpg",
                    "/images/dielac-grow-plus-banana-side-right.jpg"
                };
            }
            else if (img.Contains("strawberry"))
            {
                images360 = new List<string>
                {
                    "/images/dielac-grow-plus-strawberry-front.jpg",
                    "/images/dielac-grow-plus-strawberry-side-left.jpg",
                    "/images/dielac-grow-plus-strawberry-back.jpg",
                    "/images/dielac-grow-plus-strawberry-side-right.jpg"
                };
            }
            else if (img.Contains("gold-2"))
            {
                images360 = new List<string>
                {
                    "/images/dielac-gold-2-front.jpg",
                    "/images/dielac-gold-2-side.jpg",
                    "/images/dielac-gold-2-back.jpg",
                    "/images/dielac-gold-2-side.jpg"
                };
            }
            else if (img.Contains("grow-plus"))
            {
                images360 = new List<string>
                {
                    "/images/dielac-grow-plus-front.jpg",
                    "/images/dielac-grow-plus-side.jpg",
                    "/images/dielac-grow-plus-back.jpg",
                    "/images/dielac-grow-plus-side.jpg"
                };
            }
            else if (img.Contains("dielac-grow"))
            {
                images360 = new List<string>
                {
                    "/images/dielac-grow-front.jpg",
                    "/images/dielac-grow-side-left.jpg",
                    "/images/dielac-grow-back.jpg",
                    "/images/dielac-grow-side-right.jpg"
                };
            }
            else if (img.Contains("phomai"))
            {
                images360 = new List<string>
                {
                    "/images/phomai-vinamilk-box-front.jpg",
                    "/images/phomai-vinamilk-inside-open.jpg",
                    "/images/phomai-vinamilk-single-piece.jpg",
                    "/images/phomai-vinamilk-box-back.jpg"
                };
            }
            else
            {
                // Fallback nếu là sản phẩm thông thường
                images360.Add(product.ImageUrl ?? "/images/hero-banner.jpg");
            }

            ViewBag.Product360Images = images360;

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
        // 7. THANH TOÁN (CHECKOUT)
        // ==========================================
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCartItems();
            if (!cart.Any())
            {
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Cart = cart;
            ViewBag.SubTotal = cart.Sum(c => c.Total);
            ViewBag.ShippingFee = 15000m;
            ViewBag.GrandTotal = ViewBag.SubTotal + ViewBag.ShippingFee;

            return View();
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
    }
}