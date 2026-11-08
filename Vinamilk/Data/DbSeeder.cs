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

            // 1. Tự động làm mới cơ sở dữ liệu
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // 2. Khởi tạo các danh mục sản phẩm tương ứng dòng Dielac & Phô mai
            var categories = new List<Category>
            {
                new Category { Name = "Sữa bột Dielac" },
                new Category { Name = "Sữa uống pha sẵn Dielac" },
                new Category { Name = "Phô mai & Dinh dưỡng" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            var catBot = categories.First(c => c.Name == "Sữa bột Dielac").Id;
            var catUong = categories.First(c => c.Name == "Sữa uống pha sẵn Dielac").Id;
            var catPhoMai = categories.First(c => c.Name == "Phô mai & Dinh dưỡng").Id;

            // 3. Nạp danh mục sản phẩm Dielac & Phô mai chuẩn ảnh 360 độ kèm thành phần và chỉ số dinh dưỡng chi tiết
            context.Products.AddRange(
                new Product
                {
                    Name = "Sữa bột Vinamilk Dielac Gold 2 Lon 800g",
                    Price = 295000,
                    ImageUrl = "/images/dielac-gold-2-front.jpg",
                    ShortDescription = "natri citrat, kali clorid, magnesi oxyd, sắt sulfat, kẽm sulfat, đồng sulfat, mangan sulfat, kali iodid, natri selenit, DHA từ dầu cá, ARA từ dầu M. alpina, cholin clorid, các vitamin (natri ascorbat, E, niacin, calci D-pantothenat, B1, B2, A, B6, acid folic, K1, biotin, D3, B12), taurin, Bifidobacterium (BB-12™) (probiotics), myoinositol, L-Carnitin tartrat, chất chống oxy hóa (hỗn hợp tocopherol concentrate, ascorbyl palmitat), lutein. Có chứa sữa, đậu nành và dầu cá.",
                    NutritionInfo = "Năng lượng|496 kcal#Chất đạm|15 g#IgG (từ sữa non)|360 mg#Chất béo|23 g#Acid linoleic|3600 mg#Acid alpha-linolenic|400 mg#ARA (Arachidonic acid)|58 mg#DHA (Docosahexaenoic acid)|58 mg#Carbohydrat|51 g#Đường tổng số|29 g#Chất xơ hòa tan/FOS|2.0 g#Canxi / Calcium|510 mg#Phospho|390 mg#Magnesi|50 mg#Sắt / Iron|7.0 mg#Kẽm / Zinc|5.5 mg#Vitamin A|1650 IU#Vitamin D3|336 IU#Vitamin C|70 mg#Cholin|80 mg",
                    StockQuantity = 120,
                    IsFeatured = true,
                    CategoryId = catBot
                },
                new Product
                {
                    Name = "Sữa bột Vinamilk Dielac Grow Plus 2+ Lon 1.4kg",
                    Price = 435000,
                    ImageUrl = "/images/dielac-grow-plus-front.jpg",
                    ShortDescription = "Bột sữa, maltodextrin, đạm whey giàu alpha-lactalbumin, dầu thực vật, bột béo MCT, chất xơ hòa tan (FOS), sữa non (colostrum) (0,8%), DHA từ dầu cá, oligosaccharid (2'-FL), cholin, Bifidobacterium (BB-12™), hỗn hợp nucleotid, các vitamin (A, D3, E, K1, K2, C, B1, B2, B6, B12, acid folic) và khoáng chất (tricalci phosphat, kali iodid, kẽm sulfat, sắt pyrophosphat). Có chứa sữa, đậu nành và dầu cá.",
                    NutritionInfo = "Năng lượng|474 kcal#Chất đạm / Protein|17.5 g#Tryptophan|190 mg#IgG (từ sữa non)|160 mg#Chất béo / Total fat|22 g#MCT (Medium chain triglycerides)|2.3 g#Acid linoleic|2500 mg#DHA (Docosahexaenoic acid)|40 mg#Carbohydrat|50.4 g#2'-Fucosyllactose (2'-FL)|110 mg#Lysin|1300 mg#Taurin|44 mg#Nucleotid|20 mg#Canxi / Calcium|700 mg#Phospho|460 mg#Magnesi|48 mg#Kẽm / Zinc|5.9 mg#Vitamin A|1540 IU#Vitamin D3|400 IU#Vitamin K2|9 µg",
                    StockQuantity = 95,
                    IsFeatured = true,
                    CategoryId = catBot
                },
                new Product
                {
                    Name = "Sữa uống dinh dưỡng Dielac Grow 180ml",
                    Price = 12000,
                    ImageUrl = "/images/dielac-grow-front.jpg",
                    ShortDescription = "Sữa (93,7%) (sữa tươi, nước, sữa bột, chất béo sữa), đường, chất xơ hòa tan (inulin, oligofructose (FOS)), maltodextrin, lactoza, dầu thực vật, chất nhũ hóa (471, 466, 407), các khoáng chất (tricalci phosphat, natri citrat, magnesi citrat, sắt pyrophosphat, kẽm gluconat, mangan sulfat, đồng gluconat, kali iodid, natri selenit), bột sữa non (colostrum) (1 g/l), các vitamin (natri ascorbat, E, niacin, calci D-pantothenat, B6, B1, A, B2, acid folic, D3, K1, K2, biotin, B12), dầu cá giàu DHA, cholin clorid, hương liệu tổng hợp dùng cho thực phẩm, taurin, chất chống oxy hóa (hỗn hợp tocopherol concentrate, ascorbyl palmitat). Có chứa sữa, đậu nành và dầu cá.",
                    NutritionInfo = "Năng lượng / Energy|138.4 kcal#Chất đạm / Protein|5.67 g#Chất béo / Total fat|5.85 g#Acid linoleic|504 mg#Acid alpha-linolenic|39.6 mg#DHA|6.8 mg#Carbohydrat / Carbohydrate|15.1 g#Đường tổng số / Total sugars|13.2 g#Chất xơ hòa tan / Soluble dietary fiber|1.8 g#Lysin / Lysine|486 mg#Taurin / Taurine|13.1 mg#Natri / Sodium|90 mg#Kali / Potassium|259 mg#Clorid / Chloride|162 mg#Calci / Calcium|273 mg#Phospho / Phosphorus|187 mg#Magnesi / Magnesium|21.6 mg#Mangan / Manganese|234 µg#Sắt / Iron|1.44 mg#Iod / Iodine|25.2 µg#Kẽm / Zinc|1.08 mg#Đồng / Copper|0.08 mg#Selen / Selenium|3.6 µg#Vitamin A|414 IU#Vitamin D3|327 IU#Vitamin E|2.34 mg TE#Vitamin K1|9.5 µg#Vitamin K2|6.5 µg#Vitamin C|18 mg#Vitamin B1|0.18 mg#Vitamin B2|0.288 mg#Niacin|1.98 mg#Vitamin B6|0.36 mg#Acid folic|45 µg#Acid pantothenic|0.864 mg#Vitamin B12|0.6 µg#Biotin|7.2 µg#Cholin / Choline|27.0 mg",
                    StockQuantity = 240,
                    IsFeatured = true,
                    CategoryId = catUong
                },
                new Product
                {
                    Name = "Sữa uống Dielac Grow Plus Hương Chuối 110ml",
                    Price = 9000,
                    ImageUrl = "/images/dielac-grow-plus-banana-front.jpg",
                    ShortDescription = "Sữa (91,7%) (sữa tươi, nước, sữa bột, chất béo sữa), lactoza, đường, dầu thực vật, chất xơ hòa tan oligofructose (FOS), maltodextrin, nước chuối ép cô đặc, bột béo MCT, chất nhũ hóa (471, 460(i), 466, 407), các khoáng chất (tricalci phosphat, natri citrat, magnesi citrat, kẽm gluconat, sắt pyrophosphat, mangan sulfat, đồng gluconat, kali iodid, natri selenit), hương liệu tự nhiên và tổng hợp dùng cho thực phẩm, đạm whey giàu alpha-lactalbumin, bột sữa non (colostrum) (1 g/l), các vitamin (natri ascorbat, E, niacin, calci D-pantothenat, B6, B1, A, B2, acid folic, K1, K2, biotin, D3, B12), dầu cá giàu DHA, oligosaccharid (2'-Fucosyllactose (2'-FL)), cholin clorid, taurin, chất chống oxy hóa (hỗn hợp tocopherol concentrate, ascorbyl palmitat), hỗn hợp nucleotid. Có chứa sữa, đậu nành và dầu cá.",
                    NutritionInfo = "Năng lượng / Energy|97 kcal#Chất đạm / Protein|3.65 g#Chất béo / Total fat|4.1 g#MCT (Medium chain triglycerides)|0.5 g#Acid linoleic|650 mg#Acid alpha-linolenic|66 mg#DHA|6.7 mg#Carbohydrat / Carbohydrate|11 g#Đường tổng số / Total sugars|9.4 g#Chất xơ hòa tan / Soluble dietary fiber|0.88 g#2'-Fucosyllactose (2'-FL)|22.6 mg#Nucleotid / Nucleotides|4.2 mg#Lysin / Lysine|297 mg#Taurin / Taurine|9.4 mg#Arginin / Arginine|95 mg#Threonin / Threonine|105 mg#Natri / Sodium|55 mg#Kali / Potassium|158 mg#Clorid / Chloride|99 mg#Calci / Calcium|187 mg#Phospho / Phosphorus|132 mg#Magnesi / Magnesium|12.4 mg#Mangan / Manganese|150 µg#Sắt / Iron|0.98 mg#Iod / Iodine|17.6 µg#Kẽm / Zinc|0.74 mg#Đồng / Copper|0.05 mg#Selen / Selenium|4 µg#Vitamin A|319 IU#Vitamin D3|88 IU#Vitamin E|1.8 mg TE#Vitamin K1|5.8 µg#Vitamin K2|4 µg#Vitamin C|11 mg#Vitamin B1|0.12 mg#Vitamin B2|0.18 mg#Niacin|1.4 mg#Vitamin B6|0.22 mg#Acid folic|33 µg#Acid pantothenic|0.53 mg#Vitamin B12|0.41 µg#Biotin|4.4 µg#Cholin / Choline|24.2 mg",
                    StockQuantity = 300,
                    IsFeatured = true,
                    CategoryId = catUong
                },
                new Product
                {
                    Name = "Sữa uống Dielac Grow Plus Hương Dâu 110ml",
                    Price = 9000,
                    ImageUrl = "/images/dielac-grow-plus-strawberry-front.jpg",
                    ShortDescription = "Sữa (92%) (sữa tươi, nước, sữa bột, chất béo sữa), đường, lactoza, dầu thực vật, chất xơ hòa tan oligofructose (FOS), maltodextrin, bột béo MCT, chất nhũ hóa (471, 460(i), 466, 407), các khoáng chất (tricalci phosphat, natri citrat, magnesi citrat, kẽm gluconat, sắt pyrophosphat, mangan sulfat, đồng gluconat, kali iodid, natri selenit), hương dâu tự nhiên và tổng hợp, đạm whey giàu alpha-lactalbumin, bột sữa non (colostrum) (1 g/l), các vitamin (natri ascorbat, E, niacin, calci D-pantothenat, B6, B1, A, B2, acid folic, K1, K2, biotin, D3, B12), dầu cá giàu DHA, oligosaccharid (2'-Fucosyllactose (2'-FL)), cholin clorid, màu tự nhiên (120), taurin, chất chống oxy hóa (hỗn hợp tocopherol concentrate, ascorbyl palmitat), hỗn hợp nucleotid. Có chứa sữa, đậu nành và dầu cá.",
                    NutritionInfo = "Năng lượng / Energy|97 kcal#Chất đạm / Protein|3.65 g#Chất béo / Total fat|4.1 g#MCT (Medium chain triglycerides)|0.5 g#Acid linoleic|650 mg#Acid alpha-linolenic|66 mg#DHA|6.7 mg#Carbohydrat / Carbohydrate|11 g#Đường tổng số / Total sugars|9.4 g#Chất xơ hòa tan / Soluble dietary fiber|0.88 g#2'-Fucosyllactose (2'-FL)|22.6 mg#Nucleotid / Nucleotides|4.2 mg#Lysin / Lysine|297 mg#Taurin / Taurine|9.4 mg#Arginin / Arginine|95 mg#Threonin / Threonine|105 mg#Natri / Sodium|55 mg#Kali / Potassium|158 mg#Clorid / Chloride|99 mg#Calci / Calcium|187 mg#Phospho / Phosphorus|132 mg#Magnesi / Magnesium|12.4 mg#Mangan / Manganese|150 µg#Sắt / Iron|0.98 mg#Iod / Iodine|17.6 µg#Kẽm / Zinc|0.74 mg#Đồng / Copper|0.05 mg#Selen / Selenium|4 µg#Vitamin A|319 IU#Vitamin D3|88 IU#Vitamin E|1.8 mg TE#Vitamin K1|5.8 µg#Vitamin K2|4 µg#Vitamin C|11 mg#Vitamin B1|0.12 mg#Vitamin B2|0.18 mg#Niacin|1.4 mg#Vitamin B6|0.22 mg#Acid folic|33 µg#Acid pantothenic|0.53 mg#Vitamin B12|0.41 µg#Biotin|4.4 µg#Cholin / Choline|24.2 mg",
                    StockQuantity = 300,
                    IsFeatured = true,
                    CategoryId = catUong
                },
                new Product
                {
                    Name = "Phô mai miếng tam giác Vinamilk Hộp 8 miếng 120g",
                    Price = 36000,
                    ImageUrl = "/images/phomai-vinamilk-box-front.jpg",
                    ShortDescription = "Sữa tươi, bơ, đạm sữa, phô mai (21%), nước, sữa bột, chất điều chỉnh độ acid (330, 339(iii)), muối ăn, chất ổn định (452(i), 407a, 401, 508). Có chứa sữa.",
                    NutritionInfo = "Năng lượng / Energy|293.4 kcal#Chất đạm / Protein|9.0 g#Chất béo / Total Fat|26.2 g#Carbohydrat / Carbohydrate|5.4 g#Natri / Sodium|445 mg#Calci / Calcium|300 mg",
                    StockQuantity = 180,
                    IsFeatured = true,
                    CategoryId = catPhoMai
                }
            );

            // 4. Nạp danh sách điểm bán Cửa hàng O2O toàn quốc
            context.Stores.AddRange(
                new Store { Name = "Vinamilk Store Quận 1", Address = "10 Đinh Tiên Hoàng, P. Đa Kao, Quận 1", City = "Hồ Chí Minh", Phone = "0283910123", Latitude = 10.7876, Longitude = 106.6989 },
                new Store { Name = "Vinamilk Store Quận 3", Address = "220 Nguyễn Đình Chiểu, Phường 6, Quận 3", City = "Hồ Chí Minh", Phone = "0283930567", Latitude = 10.7769, Longitude = 106.6881 },
                new Store { Name = "Vinamilk Store Hoàn Kiếm", Address = "54 Tràng Tiền, Quận Hoàn Kiếm", City = "Hà Nội", Phone = "0243826111", Latitude = 21.0253, Longitude = 105.8540 },
                new Store { Name = "Vinamilk Store Cầu Giấy", Address = "122 Cầu Giấy, Quận Cầu Giấy", City = "Hà Nội", Phone = "0243767888", Latitude = 21.0333, Longitude = 105.7938 },
                new Store { Name = "Vinamilk Store Đà Nẵng", Address = "92 Bạch Đằng, P. Hải Châu 1, Hải Châu", City = "Đà Nẵng", Phone = "0236384999", Latitude = 16.0678, Longitude = 108.2208 },
                new Store { Name = "Vinamilk Store Cần Thơ", Address = "45 Đại Lộ Hòa Bình, Tân An, Ninh Kiều", City = "Cần Thơ", Phone = "0292381234", Latitude = 10.0342, Longitude = 105.7876 }
            );

            // 5. Nạp danh sách Lead
            var leads = new List<Lead>
            {
                new Lead { FullName = "Nguyễn Hoàng Nam", Email = "hoangnam.nguyen.demo@gmail.com", PhoneNumber = "0903112233", Status = "New", CreatedAt = DateTime.Now.AddDays(-1) },
                new Lead { FullName = "Trần Thị Mai Anh", Email = "maianh.tran.test@gmail.com", PhoneNumber = "0918223344", Status = "New", CreatedAt = DateTime.Now.AddDays(-2) },
                new Lead { FullName = "Lê Quốc Bảo", Email = "bao.lequoc.demo@gmail.com", PhoneNumber = "0934556677", Status = "New", CreatedAt = DateTime.Now.AddDays(-2) },
                new Lead { FullName = "Phạm Thu Thảo", Email = "thuthao.pham.test@gmail.com", PhoneNumber = "0978990011", Status = "New", CreatedAt = DateTime.Now.AddDays(-3) },
                new Lead { FullName = "Võ Minh Khôi", Email = "minhkhoi.vo.demo@gmail.com", PhoneNumber = "0981223355", Status = "New", CreatedAt = DateTime.Now.AddDays(-3) },
                new Lead { FullName = "Đỗ Hải Yến", Email = "haiyen.do.demo@gmail.com", PhoneNumber = "0909445566", Status = "New", CreatedAt = DateTime.Now.AddHours(-5) },
                new Lead { FullName = "Bùi Đức Thịnh", Email = "ducthinh.bui.demo@gmail.com", PhoneNumber = "0912334455", Status = "New", CreatedAt = DateTime.Now.AddHours(-2) },
                new Lead { FullName = "Hoàng Kim Ngân", Email = "kimngan.lead.demo@gmail.com", PhoneNumber = "0945667788", Status = "Contacted", CreatedAt = DateTime.Now.AddDays(-4) },
                new Lead { FullName = "Đặng Tuấn Kiệt", Email = "tuankiet.lead.demo@gmail.com", PhoneNumber = "0967889900", Status = "Contacted", CreatedAt = DateTime.Now.AddDays(-4) },
                new Lead { FullName = "Ngô Bích Phương", Email = "bichphuong.lead.demo@gmail.com", PhoneNumber = "0938112244", Status = "Qualified", CreatedAt = DateTime.Now.AddDays(-5) },
                new Lead { FullName = "Dương Văn Hậu", Email = "vanhau.lead.demo@gmail.com", PhoneNumber = "0977223344", Status = "Qualified", CreatedAt = DateTime.Now.AddDays(-5) },
                new Lead { FullName = "Lý Mỹ Duyên", Email = "myduyen.lead.demo@gmail.com", PhoneNumber = "0908332211", Status = "Contacted", CreatedAt = DateTime.Now.AddDays(-6) },
                new Lead { FullName = "Phan Trọng Hiếu", Email = "tronghieu.lead.demo@gmail.com", PhoneNumber = "0915664433", Status = "Qualified", CreatedAt = DateTime.Now.AddDays(-6) },
                new Lead { FullName = "Trịnh Cẩm Tú", Email = "camtu.lead.demo@gmail.com", PhoneNumber = "0988776655", Status = "New", CreatedAt = DateTime.Now.AddHours(-8) },
                new Lead { FullName = "Vũ Đình Trọng", Email = "trongvu.order@gmail.com", PhoneNumber = "0933119988", Status = "Converted", CreatedAt = DateTime.Now.AddDays(-7) },
                new Lead { FullName = "Nguyễn Ngọc Ánh", Email = "ngocanh.order@gmail.com", PhoneNumber = "0902887766", Status = "Converted", CreatedAt = DateTime.Now.AddDays(-7) },
                new Lead { FullName = "Hồ Gia Huy", Email = "giahuy.order@gmail.com", PhoneNumber = "0919445588", Status = "Converted", CreatedAt = DateTime.Now.AddDays(-8) },
                new Lead { FullName = "Trương Ánh Tuyết", Email = "anhtuyet.order@gmail.com", PhoneNumber = "0976332211", Status = "Converted", CreatedAt = DateTime.Now.AddDays(-9) },
                new Lead { FullName = "Đoàn Nhật Quang", Email = "nhatquang.order@gmail.com", PhoneNumber = "0982554433", Status = "Converted", CreatedAt = DateTime.Now.AddDays(-10) },
                new Lead { FullName = "Lâm Thanh Thảo", Email = "thanhthao.order@gmail.com", PhoneNumber = "0907114477", Status = "Converted", CreatedAt = DateTime.Now.AddDays(-10) }
            };
            context.Leads.AddRange(leads);

            // 6. Khởi tạo Role và User Identity (Bảo vệ phân quyền)
            if (!context.Roles.Any())
            {
                var adminRole = new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                };
                var customerRole = new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Customer",
                    NormalizedName = "CUSTOMER"
                };
                context.Roles.AddRange(adminRole, customerRole);

                var hasher = new PasswordHasher<IdentityUser>();

                // Tài khoản Admin duy nhất
                var adminUser = new IdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "admin@vinamilk.com.vn",
                    NormalizedUserName = "ADMIN@VINAMILK.COM.VN",
                    Email = "admin@vinamilk.com.vn",
                    NormalizedEmail = "ADMIN@VINAMILK.COM.VN",
                    EmailConfirmed = true,
                    PhoneNumber = "0901234567",
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

                // Tài khoản Khách hàng 1
                var customerUser1 = new IdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "khachhang1@gmail.com",
                    NormalizedUserName = "KHACHHANG1@GMAIL.COM",
                    Email = "khachhang1@gmail.com",
                    NormalizedEmail = "KHACHHANG1@GMAIL.COM",
                    EmailConfirmed = true,
                    PhoneNumber = "0912345678",
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                customerUser1.PasswordHash = hasher.HashPassword(customerUser1, "Customer@123");

                // Tài khoản Khách hàng 2
                var customerUser2 = new IdentityUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "khachhang2@gmail.com",
                    NormalizedUserName = "KHACHHANG2@GMAIL.COM",
                    Email = "khachhang2@gmail.com",
                    NormalizedEmail = "KHACHHANG2@GMAIL.COM",
                    EmailConfirmed = true,
                    PhoneNumber = "0987654321",
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                customerUser2.PasswordHash = hasher.HashPassword(customerUser2, "Customer@123");

                context.Users.AddRange(adminUser, customerUser1, customerUser2);

                // Gán quyền tương ứng vào bảng UserRoles
                context.UserRoles.AddRange(
                    new IdentityUserRole<string> { UserId = adminUser.Id, RoleId = adminRole.Id },
                    new IdentityUserRole<string> { UserId = customerUser1.Id, RoleId = customerRole.Id },
                    new IdentityUserRole<string> { UserId = customerUser2.Id, RoleId = customerRole.Id }
                );
            }

            // 7. Nạp đơn hàng mẫu (Chuẩn hóa đầy đủ các trường Required để không bị lỗi validate)
            context.Orders.AddRange(
                new Order
                {
                    CustomerName = "Trần Minh Quân",
                    PhoneNumber = "0912345678",
                    Address = "12 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
                    PaymentMethod = "COD",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    Status = "Đang xử lý",
                    TotalAmount = 385000
                },
                new Order
                {
                    CustomerName = "Nguyễn Hải Đăng",
                    PhoneNumber = "0908765432",
                    Address = "45 Lê Lợi, Phường Bến Thành, Quận 1, TP.HCM",
                    PaymentMethod = "Banking",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    Status = "Chờ thanh toán QR",
                    TotalAmount = 720000
                },
                new Order
                {
                    CustomerName = "Phạm Thị Lan",
                    PhoneNumber = "0934112233",
                    Address = "88 Hai Bà Trưng, Phường 6, Quận 3, TP.HCM",
                    PaymentMethod = "COD",
                    CreatedAt = DateTime.Now.AddDays(-3),
                    Status = "Đã giao hàng",
                    TotalAmount = 540000
                },
                new Order
                {
                    CustomerName = "Lê Hoàng Phúc",
                    PhoneNumber = "0978665544",
                    Address = "104 Nam Kỳ Khởi Nghĩa, Quận 1, TP.HCM",
                    PaymentMethod = "COD",
                    CreatedAt = DateTime.Now.AddDays(-4),
                    Status = "Đã giao hàng",
                    TotalAmount = 1250000
                },
                new Order
                {
                    CustomerName = "Hoàng Ngọc Mai",
                    PhoneNumber = "0989334455",
                    Address = "23 Pasteur, Phường Nguyễn Thái Bình, Quận 1, TP.HCM",
                    PaymentMethod = "Banking",
                    CreatedAt = DateTime.Now.AddDays(-5),
                    Status = "Đã thanh toán (Chờ xác nhận)",
                    TotalAmount = 185000
                },
                new Order
                {
                    CustomerName = "Đặng Vũ Linh",
                    PhoneNumber = "0965221199",
                    Address = "67 Võ Thị Sáu, Phường 7, Quận 3, TP.HCM",
                    PaymentMethod = "COD",
                    CreatedAt = DateTime.Now.AddDays(-6),
                    Status = "Đã hủy",
                    TotalAmount = 290000
                }
            );

            // 8. Nạp kịch bản trả lời mẫu cho Chatbot (ChatbotRules)
            if (!context.ChatbotRules.Any())
            {
                context.ChatbotRules.AddRange(
                    new ChatbotRule
                    {
                        Keywords = "chào, hello, hi, xin chào",
                        ResponseMessage = "Dạ chào bạn! Vinamilk có thể hỗ trợ gì cho bạn hôm nay ạ?",
                        IsActive = true
                    },
                    new ChatbotRule
                    {
                        Keywords = "ship, giao hàng, vận chuyển, phí ship",
                        ResponseMessage = "Vinamilk cam kết giao hàng nhanh bảo quản tiêu chuẩn trong vòng 2h tại khu vực nội thành!",
                        IsActive = true
                    },
                    new ChatbotRule
                    {
                        Keywords = "voucher, giảm giá, khuyến mãi, ưu đãi",
                        ResponseMessage = "Bạn có thể tham khảo mục Khuyến mãi hoặc đăng ký nhận mã ưu đãi tại Landing Page nhé!",
                        IsActive = true
                    },
                    new ChatbotRule
                    {
                        Keywords = "cửa hàng, địa chỉ, chi nhánh, mua ở đâu",
                        ResponseMessage = "Hệ thống Vinamilk Store có mặt tại TP.HCM, Hà Nội, Đà Nẵng, Cần Thơ. Bạn có thể xem bản đồ chi tiết ở mục Liên hệ / Cửa hàng ạ!",
                        IsActive = true
                    },
                    new ChatbotRule
                    {
                        Keywords = "sản phẩm, giá sữa, bao nhiêu tiền, dielac, phô mai",
                        ResponseMessage = "Vinamilk hiện phân phối các dòng dinh dưỡng cao cấp: Sữa bột Dielac Gold, Dielac Grow Plus, Sữa uống pha sẵn Dielac Grow (hương vani, dâu, chuối) và Phô mai miếng Vinamilk. Bạn bấm vào menu 'Sản phẩm' để xem giá và trải nghiệm xoay 360 độ nhé!",
                        IsActive = true
                    }
                );
            }

            context.SaveChanges();
        }
    }
}