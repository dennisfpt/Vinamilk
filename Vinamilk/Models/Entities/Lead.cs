using System.ComponentModel.DataAnnotations;

namespace Vinamilk.Models.Entities
{
    public class Lead
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng số 0")]
        [StringLength(10, ErrorMessage = "Số điện thoại tối đa 10 số")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string Source { get; set; } = "LandingPage";

        // Tổng điểm tiềm năng Lead Scoring
        public int LeadScore { get; set; } = 0;

        // Trạng thái: New -> MQL (khi Score >= 50) -> SQL -> Customer
        public string Status { get; set; } = "New";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastInteractedAt { get; set; } = DateTime.Now;

        public ICollection<LeadActivity> Activities { get; set; } = new List<LeadActivity>();
    }
}