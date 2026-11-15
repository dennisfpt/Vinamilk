namespace Vinamilk.Models
{
    public class BotRule
    {
        public int Id { get; set; }
        public string FlowName { get; set; } = "";      // Tên luồng Automation
        public string Keywords { get; set; } = "";      // Từ khóa kích hoạt (cách nhau dấu phẩy)
        public string ReplyMessage { get; set; } = "";  // Câu trả lời gửi cho khách
        public bool IsActive { get; set; } = true;      // Trạng thái Bật/Tắt trong Automation
    }

    public static class AutomationBotStore
    {
        public static List<BotRule> Rules { get; set; } = new List<BotRule>
        {
            new BotRule
            {
                Id = 1,
                FlowName = "Tư vấn Sữa Tươi Green Farm",
                Keywords = "green farm,sữa tươi,ít đường,không đường,sinh thái",
                ReplyMessage = "Chào bạn! Sữa tươi Vinamilk Green Farm được làm từ 100% sữa tươi trang trại sinh thái, vị thanh nhẹ thuần khiết. Bạn có thể đăng ký tài khoản ngay hôm nay để nhận 5.000 điểm đổi voucher nhé!",
                IsActive = true
            },
            new BotRule
            {
                Id = 2,
                FlowName = "Kịch bản Săn Voucher & Khuyến Mãi",
                Keywords = "voucher,khuyến mãi,giảm giá,mã,quà,tích điểm,rewards",
                ReplyMessage = "Hiện tại Vinamilk Rewards đang tặng 5.000 điểm khi đăng ký mới! Bạn có thể dùng điểm đổi Voucher Trà Atisô 50K, Nước Ép 100K - 200K hoặc Kem Gelato ngay tại mục Khuyến mãi.",
                IsActive = true
            },
            new BotRule
            {
                Id = 3,
                FlowName = "Hỗ trợ Giao hàng & Đổi trả",
                Keywords = "giao hàng,ship,vận chuyển,đổi trả,bao lâu,mấy tiếng",
                ReplyMessage = "Vinamilk hỗ trợ giao lạnh siêu tốc trong 2 giờ tại nội thành và miễn phí đổi trả 100% nếu sản phẩm có lỗi từ nhà sản xuất ạ.",
                IsActive = true
            },
            new BotRule
            {
                Id = 4,
                FlowName = "Tư vấn Sữa Bột & Dinh Dưỡng",
                Keywords = "optimum,a2,sữa bột,trẻ em,dielac,sữa hạt,sữa chua,hy lạp",
                ReplyMessage = "Các dòng Optimum A2 Pro+, Sữa Hạt 9 Loại Hạt và Sữa chua Hy Lạp Cao Đạm hiện đều có sẵn hàng. Bạn vào mục Sản phẩm để xem chi tiết thành phần dinh dưỡng nhé!",
                IsActive = true
            }
        };

        public static string GetAutoReply(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return "Xin chào! Bạn cần Vinamilk hỗ trợ thông tin gì ạ?";

            string msg = userMessage.Trim().ToLower();

            // Chỉ khớp với các luồng Automation đang BẬT (IsActive == true)
            foreach (var rule in Rules.Where(r => r.IsActive))
            {
                var keys = rule.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (keys.Any(k => msg.Contains(k.Trim().ToLower())))
                {
                    return rule.ReplyMessage;
                }
            }

            return "Cảm ơn bạn đã nhắn tin cho Vinamilk! Bạn có thể hỏi về: Green Farm, Voucher khuyến mãi, Tích điểm Rewards hoặc Giao hàng để được hỗ trợ ngay nhé.";
        }
    }
}