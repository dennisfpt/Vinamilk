using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vinamilk.Data;

namespace Vinamilk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;

        // Điền API Key từ Google AI Studio nếu muốn dùng thêm AI tự do (để trống vẫn chạy mượt theo luật Admin)
        private const string GEMINI_API_KEY = "";

        public ChatbotController(IHttpClientFactory httpClientFactory, ApplicationDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            string userMsg = request?.Message?.Trim() ?? "";
            if (string.IsNullOrEmpty(userMsg))
            {
                return Ok(new { reply = "Dạ, bạn đang quan tâm dòng sữa tươi Green Farm, sữa hạt hay ưu đãi tích điểm của Vinamilk ạ?" });
            }

            string lowerMsg = userMsg.ToLower();

            // =========================================================================
            // BƯỚC 1: ĐỌC KỊCH BẢN TỪ DATABASE (LIÊN KẾT TRỰC TIẾP VỚI ADMIN AUTOMATION)
            // =========================================================================
            var activeRules = await _context.ChatbotRules
                .Where(r => r.IsActive)
                .ToListAsync();

            foreach (var rule in activeRules)
            {
                if (string.IsNullOrWhiteSpace(rule.Keywords)) continue;

                // Tách các từ khóa cách nhau bằng dấu phẩy và so khớp với tin nhắn khách
                var keywords = rule.Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (keywords.Any(k => lowerMsg.Contains(k.Trim().ToLower())))
                {
                    return Ok(new { reply = rule.ResponseMessage });
                }
            }

            // =========================================================================
            // BƯỚC 2: NẾU CÓ GEMINI API KEY THÌ GỌI AI KÈM NGỮ CẢNH TỪ ADMIN
            // =========================================================================
            if (!string.IsNullOrWhiteSpace(GEMINI_API_KEY))
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(10);

                    string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={GEMINI_API_KEY}";

                    var adminKnowledge = activeRules.Any()
                        ? "Thông tin khuyến mãi & kịch bản hiện tại từ Admin: " +
                          string.Join(" | ", activeRules.Select(r => $"{r.Keywords}: {r.ResponseMessage}"))
                        : "";

                    var systemPrompt =
                        $"Bạn là trợ lý tư vấn sản phẩm Vinamilk. Trả lời thân thiện, ngắn gọn (2-3 câu). " +
                        $"Mã khuyến mãi hiện tại là {CampaignConfig.VoucherCode} giảm {CampaignConfig.DiscountPercent}%. " +
                        $"{adminKnowledge}";

                    var payload = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = $"{systemPrompt}\n\nKhách hỏi: {userMsg}" }
                                }
                            }
                        }
                    };

                    var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(responseString);

                        var candidates = doc.RootElement.GetProperty("candidates");
                        if (candidates.GetArrayLength() > 0)
                        {
                            string aiText = candidates[0]
                                .GetProperty("content")
                                .GetProperty("parts")[0]
                                .GetProperty("text")
                                .GetString() ?? "";

                            if (!string.IsNullOrWhiteSpace(aiText))
                            {
                                return Ok(new { reply = aiText.Trim() });
                            }
                        }
                    }
                }
                catch
                {
                    // Rơi xuống bộ phản hồi dự phòng bên dưới nếu lỗi mạng
                }
            }

            // =========================================================================
            // BƯỚC 3: PHẢN HỒI DỰ PHÒNG ĐỒNG BỘ VỚI CẤU HÌNH CHIẾN DỊCH (CampaignConfig)
            // =========================================================================
            string fallbackReply;

            if (lowerMsg.Contains("green farm") || lowerMsg.Contains("sữa tươi") || lowerMsg.Contains("tiệt trùng"))
            {
                fallbackReply = "Sữa tươi Vinamilk Green Farm 100% từ trang trại sinh thái đạt chuẩn quốc tế, vị thanh nhẹ thuần khiết. Bạn có thể chọn loại Rất ít đường, Ít đường hoặc Cao đạm ít béo nhé!";
            }
            else if (lowerMsg.Contains("sữa chua") || lowerMsg.Contains("probi") || lowerMsg.Contains("hy lạp"))
            {
                fallbackReply = "Vinamilk có dòng sữa chua ăn kiểu Hy Lạp cao đạm và sữa chua uống Probi chứa hàng tỷ lợi khuẩn men sống, cực tốt cho tiêu hóa và vóc dáng!";
            }
            else if (lowerMsg.Contains("sữa hạt") || lowerMsg.Contains("super nut") || lowerMsg.Contains("hạt"))
            {
                fallbackReply = "Sữa hạt 9 loại hạt Vinamilk kết hợp óc chó, hạnh nhân, yến mạch... ít ngọt thanh bùi và giàu Omega-3 tốt cho tim mạch!";
            }
            else if (lowerMsg.Contains("optimum") || lowerMsg.Contains("sữa bột") || lowerMsg.Contains("dielac") || lowerMsg.Contains("bé"))
            {
                fallbackReply = "Dòng Optimum A2 Pro+ và Dielac bổ sung đạm quý A2, 6 HMO cùng DHA hỗ trợ bé tiêu hóa khỏe và phát triển trí não vượt trội.";
            }
            else if (lowerMsg.Contains("voucher") || lowerMsg.Contains("ưu đãi") || lowerMsg.Contains("giảm giá") || lowerMsg.Contains("khuyến mãi") || lowerMsg.Contains("mã") || lowerMsg.Contains("điểm"))
            {
                fallbackReply = $"Bạn có thể đăng ký tài khoản nhận ngay 5.000 điểm Vinamilk Rewards để đổi voucher 35K - 200K, hoặc nhập mã {CampaignConfig.VoucherCode} giảm {CampaignConfig.DiscountPercent}% tại trang Khuyến mãi nhé!";
            }
            else if (lowerMsg.Contains("giao hàng") || lowerMsg.Contains("ship") || lowerMsg.Contains("đổi trả"))
            {
                fallbackReply = "Vinamilk hỗ trợ giao lạnh siêu tốc trong 2 giờ và đổi trả miễn phí 100% nếu có bất kỳ vấn đề gì về chất lượng ạ!";
            }
            else
            {
                fallbackReply = "Vinamilk xin chào! Bạn đang cần tư vấn về sữa tươi Green Farm, sữa bột Optimum A2, đổi điểm nhận voucher hay kiểm tra đơn hàng ạ?";
            }

            return Ok(new { reply = fallbackReply });
        }
    }
}