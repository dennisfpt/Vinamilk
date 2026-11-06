using Microsoft.AspNetCore.Identity;
using Vinamilk.Models.Entities;

namespace Vinamilk.Data
{
    public static class DbSeeder
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Tự động làm mới cơ sở dữ liệu để nạp đường dẫn ảnh nội bộ
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // 2. Khởi tạo 6 danh mục sản phẩm chính
            var categories = new List<Category>
            {
                new Category { Name = "Sữa tươi" },
                new Category { Name = "Sữa chua" },
                new Category { Name = "Sữa hạt & Dinh dưỡng" },
                new Category { Name = "Sữa đặc" },
                new Category { Name = "Sữa bột" },
                new Category { Name = "Kem & Phô mai" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            var catTuoi = categories.First(c => c.Name == "Sữa tươi").Id;
            var catChua = categories.First(c => c.Name == "Sữa chua").Id;
            var catHat = categories.First(c => c.Name == "Sữa hạt & Dinh dưỡng").Id;
            var catDac = categories.First(c => c.Name == "Sữa đặc").Id;
            var catBot = categories.First(c => c.Name == "Sữa bột").Id;
            var catKem = categories.First(c => c.Name == "Kem & Phô mai").Id;

            // 3. Nạp danh mục sản phẩm mẫu
            context.Products.AddRange(
                new Product
                {
                    Name = "Sữa tươi tiệt trùng Vinamilk 100% Có đường 180ml",
                    Price = 34000,
                    ImageUrl = "/images/sua-tuoi-co-duong.jpg",
                    ShortDescription = "Làm từ 100% sữa bò tươi nguyên chất từ trang trại chuẩn quốc tế.",
                    NutritionInfo = "Năng lượng: 73.2 kcal, Canxi: 110mg, Đạm: 2.9g/100ml.",
                    StockQuantity = 200,
                    IsFeatured = true,
                    CategoryId = catTuoi
                },
                new Product
                {
                    Name = "Sữa tươi tiệt trùng Vinamilk 100% Không đường 180ml",
                    Price = 34000,
                    ImageUrl = "/images/sua-tuoi-khong-duong.jpg",
                    ShortDescription = "Hương vị sữa tự nhiên thanh khiết, không bổ sung đường.",
                    NutritionInfo = "Năng lượng: 61.2 kcal, Canxi: 115mg/100ml.",
                    StockQuantity = 180,
                    IsFeatured = true,
                    CategoryId = catTuoi
                },
                new Product
                {
                    Name = "Sữa chua ăn Vinamilk Có đường 100g (Lốc 4 hộp)",
                    Price = 28000,
                    ImageUrl = "/images/sua-chua-an.jpg",
                    ShortDescription = "Lên men tự nhiên với chủng men thuần Bulgaricus thơm ngon sánh mịn.",
                    NutritionInfo = "Hàng triệu lợi khuẩn Probiotics cùng Vitamin D3 hỗ trợ tiêu hóa.",
                    StockQuantity = 250,
                    IsFeatured = true,
                    CategoryId = catChua
                },
                new Product
                {
                    Name = "Sữa dinh dưỡng 9 loại hạt Vinamilk Super Nut 180ml",
                    Price = 44000,
                    ImageUrl = "/images/sua-super-nut.jpg",
                    ShortDescription = "Tuyển chọn 9 loại hạt cao cấp: Óc chó, Hạnh nhân, Yến mạch, Macca...",
                    NutritionInfo = "Giàu Omega-3 tự nhiên, chất chống oxy hóa và chất xơ thực vật.",
                    StockQuantity = 150,
                    IsFeatured = true,
                    CategoryId = catHat
                },
                new Product
                {
                    Name = "Sữa đặc có đường Ông Thọ nhãn trắng Lon 380g",
                    Price = 36000,
                    ImageUrl = "/images/sua-ong-tho.jpg",
                    ShortDescription = "Sữa đặc truyền thống sánh đặc, thơm béo đậm đà pha cà phê sữa.",
                    NutritionInfo = "Độ đạm cao 7g/100g, sánh mịn đặc trưng.",
                    StockQuantity = 300,
                    IsFeatured = true,
                    CategoryId = catDac
                },
                new Product
                {
                    Name = "Sữa bột Vinamilk Dielac Alpha Gold IQ Step 4 Lon 850g",
                    Price = 295000,
                    ImageUrl = "/images/sua-bot-dielac.jpg",
                    ShortDescription = "Công thức bổ sung gấp 3 lần DHA, Cholin và Lutein hỗ trợ phát triển trí não.",
                    NutritionInfo = "DHA, ARA, Lutein, Canxi, Vitamin D3.",
                    StockQuantity = 90,
                    IsFeatured = true,
                    CategoryId = catBot
                }
            );

            // 4. Nạp danh sách điểm bán Cửa hàng O2O toàn quốc
            context.Stores.AddRange(
                new Store { Name = "Vinamilk Store Quận 1", Address = "10 Đinh Tiên Hoàng, P. Đa Kao, Quận 1", City = "Hồ Chí Minh", Phone = "028 3910 1234", Latitude = 10.7876, Longitude = 106.6989 },
                new Store { Name = "Vinamilk Store Quận 3", Address = "220 Nguyễn Đình Chiểu, Phường 6, Quận 3", City = "Hồ Chí Minh", Phone = "028 3930 5678", Latitude = 10.7769, Longitude = 106.6881 },
                new Store { Name = "Vinamilk Store Hoàn Kiếm", Address = "54 Tràng Tiền, Quận Hoàn Kiếm", City = "Hà Nội", Phone = "024 3826 1111", Latitude = 21.0253, Longitude = 105.8540 },
                new Store { Name = "Vinamilk Store Cầu Giấy", Address = "122 Cầu Giấy, Quận Cầu Giấy", City = "Hà Nội", Phone = "024 3767 8888", Latitude = 21.0333, Longitude = 105.7938 },
                new Store { Name = "Vinamilk Store Đà Nẵng", Address = "92 Bạch Đằng, P. Hải Châu 1, Hải Châu", City = "Đà Nẵng", Phone = "0236 384 9999", Latitude = 16.0678, Longitude = 108.2208 },
                new Store { Name = "Vinamilk Store Cần Thơ", Address = "45 Đại Lộ Hòa Bình, Tân An, Ninh Kiều", City = "Cần Thơ", Phone = "0292 381 2345", Latitude = 10.0342, Longitude = 105.7876 }
            );

            // 5. Nạp 20 khách hàng tiềm năng vào DbSet<Lead>
            var leads = new List<Lead>
            {
                // Nhóm 1: Lead mới (Trạng thái "New" -> kích hoạt badge chuông thông báo)
                new Lead { FullName = "Nguyễn Hoàng Nam", Email = "hoangnam.nguyen.demo@gmail.com", PhoneNumber = "0903112233", Status = "New", CreatedAt = DateTime.Now.AddDays(-1) },
                new Lead { FullName = "Trần Thị Mai Anh", Email = "maianh.tran.test@gmail.com", PhoneNumber = "0918223344", Status = "New", CreatedAt = DateTime.Now.AddDays(-2) },
                new Lead { FullName = "Lê Quốc Bảo", Email = "bao.lequoc.demo@gmail.com", PhoneNumber = "0934556677", Status = "New", CreatedAt = DateTime.Now.AddDays(-2) },
                new Lead { FullName = "Phạm Thu Thảo", Email = "thuthao.pham.test@gmail.com", PhoneNumber = "0978990011", Status = "New", CreatedAt = DateTime.Now.AddDays(-3) },
                new Lead { FullName = "Võ Minh Khôi", Email = "minhkhoi.vo.demo@gmail.com", PhoneNumber = "0981223355", Status = "New", CreatedAt = DateTime.Now.AddDays(-3) },
                new Lead { FullName = "Đỗ Hải Yến", Email = "haiyen.do.demo@gmail.com", PhoneNumber = "0909445566", Status = "New", CreatedAt = DateTime.Now.AddHours(-5) },
                new Lead { FullName = "Bùi Đức Thịnh", Email = "ducthinh.bui.demo@gmail.com", PhoneNumber = "0912334455", Status = "New", CreatedAt = DateTime.Now.AddHours(-2) },

                // Nhóm 2: Lead đang liên hệ / Tư vấn
                new Lead { FullName = "Hoàng Kim Ngân", Email = "kimngan.lead.demo@gmail.com", PhoneNumber = "0945667788", Status = "Contacted", CreatedAt = DateTime.Now.AddDays(-4) },
                new Lead { FullName = "Đặng Tuấn Kiệt", Email = "tuankiet.lead.demo@gmail.com", PhoneNumber = "0967889900", Status = "Contacted", CreatedAt = DateTime.Now.AddDays(-4) },
                new Lead { FullName = "Ngô Bích Phương", Email = "bichphuong.lead.demo@gmail.com", PhoneNumber = "0938112244", Status = "Qualified", CreatedAt = DateTime.Now.AddDays(-5) },
                new Lead { FullName = "Dương Văn Hậu", Email = "vanhau.lead.demo@gmail.com", PhoneNumber = "0977223344", Status = "Qualified", CreatedAt = DateTime.Now.AddDays(-5) },
                new Lead { FullName = "Lý Mỹ Duyên", Email = "myduyen.lead.demo@gmail.com", PhoneNumber = "0908332211", Status = "Contacted", CreatedAt = DateTime.Now.AddDays(-6) },
                new Lead { FullName = "Phan Trọng Hiếu", Email = "tronghieu.lead.demo@gmail.com", PhoneNumber = "0915664433", Status = "Qualified", CreatedAt = DateTime.Now.AddDays(-6) },
                new Lead { FullName = "Trịnh Cẩm Tú", Email = "camtu.lead.demo@gmail.com", PhoneNumber = "0988776655", Status = "New", CreatedAt = DateTime.Now.AddHours(-8) },

                // Nhóm 3: Lead đã chuyển đổi mua hàng (Converted)
                new Lead { FullName = "Vũ Đình Trọng", Email = "trongvu.order@gmail.com", PhoneNumber = "0933119988", Status = "Converted", CreatedAt = DateTime.Now.AddDays(-7) },
                new Lead { FullName = "Nguyễn Ngọc Ánh", Email = "ngocanh.order@gmail.com", PhoneNumber = "0902887766", Status = "Converted", CreatedAt = DateTime.Now.AddDays(-7) },
                new Lead { FullName = "Hồ Gia Huy", Email = "giahuy.order@gmail.com", PhoneNumber = "0919445588", Status = "Converted", CreatedAt = DateTime.Now.AddDays(-8) },
                new Lead { FullName = "Trương Ánh Tuyết", Email = "anhtuyet.order@gmail.com", PhoneNumber = "0976332211", Status = "Converted", CreatedAt = DateTime.Now.AddDays(-9) },
                new Lead { FullName = "Đoàn Nhật Quang", Email = "nhatquang.order@gmail.com", PhoneNumber = "0982554433", Status = "Converted", CreatedAt = DateTime.Now.AddDays(-10) },
                new Lead { FullName = "Lâm Thanh Thảo", Email = "thanhthao.order@gmail.com", PhoneNumber = "0907114477", Status = "Converted", CreatedAt = DateTime.Now.AddDays(-10) }
            };
            context.Leads.AddRange(leads);

            // 6. Nạp tài khoản thành viên Identity (Đếm tổng khách hàng)
            if (!context.Users.Any())
            {
                var hasher = new PasswordHasher<IdentityUser>();
                var adminUser = new IdentityUser
                {
                    UserName = "admin@vinamilk.com.vn",
                    NormalizedUserName = "ADMIN@VINAMILK.COM.VN",
                    Email = "admin@vinamilk.com.vn",
                    NormalizedEmail = "ADMIN@VINAMILK.COM.VN",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

                var customerUser1 = new IdentityUser
                {
                    UserName = "khachhang1@gmail.com",
                    NormalizedUserName = "KHACHHANG1@GMAIL.COM",
                    Email = "khachhang1@gmail.com",
                    NormalizedEmail = "KHACHHANG1@GMAIL.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                customerUser1.PasswordHash = hasher.HashPassword(customerUser1, "Customer@123");

                var customerUser2 = new IdentityUser
                {
                    UserName = "khachhang2@gmail.com",
                    NormalizedUserName = "KHACHHANG2@GMAIL.COM",
                    Email = "khachhang2@gmail.com",
                    NormalizedEmail = "KHACHHANG2@GMAIL.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                customerUser2.PasswordHash = hasher.HashPassword(customerUser2, "Customer@123");

                context.Users.AddRange(adminUser, customerUser1, customerUser2);
            }

            // 7. Nạp đơn hàng mẫu (Khớp với các trạng thái xử lý trong Controller)
            context.Orders.AddRange(
                new Order { CreatedAt = DateTime.Now.AddDays(-1), Status = "Đang xử lý", TotalAmount = 385000 },
                new Order { CreatedAt = DateTime.Now.AddDays(-2), Status = "Chờ thanh toán QR", TotalAmount = 720000 },
                new Order { CreatedAt = DateTime.Now.AddDays(-3), Status = "Đã giao hàng", TotalAmount = 540000 },
                new Order { CreatedAt = DateTime.Now.AddDays(-4), Status = "Đã giao hàng", TotalAmount = 1250000 },
                new Order { CreatedAt = DateTime.Now.AddDays(-5), Status = "Đã thanh toán (Chờ xác nhận)", TotalAmount = 185000 },
                new Order { CreatedAt = DateTime.Now.AddDays(-6), Status = "Đã hủy", TotalAmount = 290000 }
            );

            context.SaveChanges();
        }
    }
}