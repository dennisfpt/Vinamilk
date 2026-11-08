using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vinamilk.Models.Entities;

namespace Vinamilk.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        // Constructor bắt buộc để nhận cấu hình kết nối DB từ Program.cs
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public ApplicationDbContext()
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Lead> Leads { get; set; }
        public DbSet<LeadActivity> LeadActivities { get; set; }
        public DbSet<Store> Stores { get; set; }

        public DbSet<Vinamilk.Models.Entities.ChatbotRule> ChatbotRules { get; set; }

        // 2 bảng Đơn hàng
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}