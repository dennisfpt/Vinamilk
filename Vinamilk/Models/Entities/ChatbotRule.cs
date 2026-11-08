using System.ComponentModel.DataAnnotations;

namespace Vinamilk.Models.Entities
{
    public class ChatbotRule
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập từ khóa nhận diện")]
        [Display(Name = "Từ khóa (phân cách bằng dấu phẩy)")]
        public string Keywords { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung câu trả lời")]
        [Display(Name = "Nội dung phản hồi")]
        public string ResponseMessage { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}