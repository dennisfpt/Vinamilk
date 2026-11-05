using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Vinamilk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Thay chính xác API Key lấy từ Google AI Studio vào đây:
        private const string GEMINI_API_KEY = "";

        public ChatbotController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
                return Ok(new { reply = "Dạ, bạn đang quan tâm dòng sữa tươi, sữa hạt hay sữa chua của Vinamilk ạ?" });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={GEMINI_API_KEY}";

                var systemPrompt = "Bạn là trợ lý tư vấn sản phẩm Vinamilk. Hãy trả lời thân thiện, sinh động, ngắn gọn (tối đa 2-3 câu). " +
                                   "Tư vấn đúng trọng tâm theo câu hỏi của khách (sữa tươi 100%, sữa hạt Super Nut, sữa chua Probi, sữa đặc Ông Thọ...) và mời khách ghé trang Khuyến mãi nhận voucher.";

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
                // Bỏ qua lỗi mạng để chạy xuống bộ phản hồi dự phòng thông minh bên dưới
            }

            // BỘ PHẢN HỒI THÔNG MINH DỰ PHÒNG THEO TỪ KHÓA (KHÔNG BAO GIỜ BỊ LẶP 1 CÂU)
            string lower = userMsg.ToLower();
            string fallbackReply;

            if (lower.Contains("sữa tươi") || lower.Contains("tiệt trùng"))
            {
                fallbackReply = "Sữa tươi Vinamilk 100% từ trang trại chuẩn quốc tế, giàu canxi và vitamin tự nhiên. Bạn có thể chọn loại Có đường, Ít đường hoặc Không đường nhé!";
            }
            else if (lower.Contains("sữa chua") || lower.Contains("probi") || lower.Contains("men"))
            {
                fallbackReply = "Vinamilk có dòng sữa chua ăn nha đam/lựu đỏ và sữa chua uống Probi chứa hàng tỷ lợi khuẩn men sống chuẩn châu Âu, cực tốt cho đường ruột và làn da!";
            }
            else if (lower.Contains("sữa hạt") || lower.Contains("super nut") || lower.Contains("hạt"))
            {
                fallbackReply = "Sữa hạt 9 loại Super Nut là lựa chọn tuyệt vời cho vóc dáng, ít ngọt thanh bùi và giàu Omega-3 từ quả óc chó, hạnh nhân, yến mạch!";
            }
            else if (lower.Contains("sữa bột") || lower.Contains("dielac") || lower.Contains("bé"))
            {
                fallbackReply = "Dòng sữa bột Dielac Grow Plus và Alpha Gold của Vinamilk bổ sung DHA, Choline hỗ trợ bé phát triển toàn diện trí não và chiều cao.";
            }
            else if (lower.Contains("ưu đãi") || lower.Contains("giảm giá") || lower.Contains("khuyến mãi") || lower.Contains("mã"))
            {
                fallbackReply = "Bạn nhớ ghé trang Khuyến mãi điền thông tin để lấy mã giảm giá 30% áp dụng ngay khi đặt hàng nhé!";
            }
            else
            {
                fallbackReply = $"Vinamilk hân hạnh phục vụ bạn! Bạn muốn tìm hiểu kỹ hơn về dinh dưỡng hay chính sách giao hàng cho câu hỏi: '{userMsg}' ạ?";
            }

            return Ok(new { reply = fallbackReply });
        }
    }
}