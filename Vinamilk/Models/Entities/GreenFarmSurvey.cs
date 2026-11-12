using System.ComponentModel.DataAnnotations;

namespace Vinamilk.Models.Entities
{
    public class GreenFarmSurvey
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "SĐT phải gồm 10 chữ số, bắt đầu bằng 0")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string AgeGroup { get; set; } = string.Empty; // Câu 1: Nhóm tuổi

        [Required]
        public string TargetUser { get; set; } = string.Empty; // Câu 2: Mua cho ai

        [Required]
        public string PreferredProduct { get; set; } = string.Empty; // Câu 3: Dòng sữa Green Farm

        [Required]
        public string Frequency { get; set; } = string.Empty; // Câu 4: Tần suất

        [Required]
        public string ReasonToChoose { get; set; } = string.Empty; // Câu 5: Yếu tố chọn mua

        [Required]
        public string Channel { get; set; } = string.Empty; // Câu 6: Kênh mua

        [Required]
        public string Packaging { get; set; } = string.Empty; // Câu 7: Quy cách đóng gói

        [Required]
        public string MonthlySpending { get; set; } = string.Empty; // Câu 9: Chi phí/tháng

        public string? Feedback { get; set; } // Câu 10: Góp ý

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}