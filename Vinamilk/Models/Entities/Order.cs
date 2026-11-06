using System.ComponentModel.DataAnnotations;

namespace Vinamilk.Models.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng số 0")]
        [StringLength(10, ErrorMessage = "Số điện thoại tối đa 10 số")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;

        public string? Note { get; set; }

        public string PaymentMethod { get; set; } = "COD";

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Đang xử lý";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<OrderItem> OrderItems { get; set; } = new();
    }

    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}