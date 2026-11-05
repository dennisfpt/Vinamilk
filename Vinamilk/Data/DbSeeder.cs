using Vinamilk.Models.Entities;

namespace Vinamilk.Data
{
    public static class DbSeeder
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Tự động làm mới cơ sở dữ liệu để nạp đường dẫn ảnh nội bộ
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();


            // 1. Khởi tạo 6 danh mục sản phẩm chính
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

            // 2. Nạp sản phẩm trỏ trực tiếp vào thư mục wwwroot/images
            context.Products.AddRange(
                // Sữa tươi có đường
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
                // Sữa tươi không đường
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
                // Sữa chua ăn
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
                // Sữa hạt Super Nut
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
                // Sữa đặc Ông Thọ
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
                // Sữa bột Dielac
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

            // 3. Nạp danh sách điểm bán Cửa hàng O2O toàn quốc
            context.Stores.AddRange(
                new Store { Name = "Vinamilk Store Quận 1", Address = "10 Đinh Tiên Hoàng, P. Đa Kao, Quận 1", City = "Hồ Chí Minh", Phone = "028 3910 1234", Latitude = 10.7876, Longitude = 106.6989 },
                new Store { Name = "Vinamilk Store Quận 3", Address = "220 Nguyễn Đình Chiểu, Phường 6, Quận 3", City = "Hồ Chí Minh", Phone = "028 3930 5678", Latitude = 10.7769, Longitude = 106.6881 },
                new Store { Name = "Vinamilk Store Hoàn Kiếm", Address = "54 Tràng Tiền, Quận Hoàn Kiếm", City = "Hà Nội", Phone = "024 3826 1111", Latitude = 21.0253, Longitude = 105.8540 },
                new Store { Name = "Vinamilk Store Cầu Giấy", Address = "122 Cầu Giấy, Quận Cầu Giấy", City = "Hà Nội", Phone = "024 3767 8888", Latitude = 21.0333, Longitude = 105.7938 },
                new Store { Name = "Vinamilk Store Đà Nẵng", Address = "92 Bạch Đằng, P. Hải Châu 1, Hải Châu", City = "Đà Nẵng", Phone = "0236 384 9999", Latitude = 16.0678, Longitude = 108.2208 },
                new Store { Name = "Vinamilk Store Cần Thơ", Address = "45 Đại Lộ Hòa Bình, Tân An, Ninh Kiều", City = "Cần Thơ", Phone = "0292 381 2345", Latitude = 10.0342, Longitude = 105.7876 }
            );

            context.SaveChanges();
        }
    }
}