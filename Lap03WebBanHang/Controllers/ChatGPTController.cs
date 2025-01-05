using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Tesseract;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace Lap03WebBanHang.Controllers
{
    public class ChatGPTController : Controller
    {
        private static List<(string Sender, string Message)> ChatHistory = new List<(string Sender, string Message)>();
        private static List<string> LearnedData = new List<string>(); // Lưu các phần dữ liệu đã học
        private readonly HttpClient _httpClient;

        // Cấu hình ngôn ngữ và ngữ cảnh
        private string CurrentLanguage = "vi"; // Mặc định là tiếng Việt
        private string AssistantIntroduction = "Chào mừng bạn đến với trợ lý ảo của Điện Gia Dụng Trần Duy! Tôi ở đây để hỗ trợ bạn tìm hiểu về các sản phẩm điện gia dụng như máy giặt, máy lạnh, bếp điện, và các thiết bị nhà bếp hiện đại." +
            " Nếu bạn cần tư vấn hoặc có bất kỳ câu hỏi nào, đừng ngần ngại, hãy hỏi tôi!"; // cấu hình Chat

        public ChatGPTController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer sk-Qq3iwRRI09ZsVdhWYFfjNS18OOKC9oTe7NkZC5sdpvvrFRil");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View(ChatHistory);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> SendMessage(string userMessage, string language = "vi")
        {
            if (!string.IsNullOrEmpty(userMessage))
            {
                try
                {
                    // Lưu tin nhắn của người dùng
                    ChatHistory.Add(("User", userMessage));

                    // Chuẩn bị payload
                    var payload = new
                    {
                        model = "gpt-4o",
                        messages = GetEnhancedContextMessages(userMessage, language),
                        max_tokens = 800
                    };

                    // Gửi yêu cầu
                    var response = await _httpClient.PostAsync(
                        "https://api.yescale.io/v1/chat/completions",
                        new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                    );

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ChatHistory.Add(("System", $"API lỗi: {response.StatusCode}. Nội dung: {errorContent}"));
                        return RedirectToAction("Index");
                    }

                    // Xử lý phản hồi từ API
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(responseBody);

                    if (jsonDocument.RootElement.TryGetProperty("choices", out JsonElement choices) &&
                        choices.GetArrayLength() > 0 &&
                        choices[0].TryGetProperty("message", out JsonElement message) &&
                        message.TryGetProperty("content", out JsonElement content))
                    {
                        var gptResponse = content.GetString();
                        ChatHistory.Add(("GPT", gptResponse));
                    }
                    else
                    {
                        ChatHistory.Add(("System", "Phản hồi từ API không hợp lệ hoặc thiếu dữ liệu."));
                    }
                }
                catch (Exception ex)
                {
                    ChatHistory.Add(("System", $"Lỗi xảy ra: {ex.Message}"));
                }
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                try
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        var fileContent = reader.ReadToEnd();
                        var dataChunks = SplitDataIntoChunks(fileContent);
                        LearnedData.AddRange(dataChunks);
                    }

                    ChatHistory.Add(("System", $"File '{file.FileName}' đã được tải lên thành công và GPT đã học dữ liệu."));
                }
                catch (Exception ex)
                {
                    ChatHistory.Add(("System", $"Lỗi xảy ra khi tải file: {ex.Message}"));
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SendMessageAjax(string userMessage, string language = "vi")
        {
            if (string.IsNullOrEmpty(userMessage))
            {
                return Content("Tin nhắn không hợp lệ!");
            }

            try
            {
                // Thêm tin nhắn của người dùng vào lịch sử
                ChatHistory.Add(("User", userMessage));

                // Tạo payload gửi đến GPT API
                var payload = new
                {
                    model = "gpt-4o",
                    messages = GetEnhancedContextMessages(userMessage, language),
                    max_tokens = 800
                };

                var response = await _httpClient.PostAsync(
                    "https://api.yescale.io/v1/chat/completions",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Content($"API lỗi: {response.StatusCode}. Nội dung: {errorContent}");
                }

                // Xử lý phản hồi từ API
                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseBody);

                if (jsonDocument.RootElement.TryGetProperty("choices", out JsonElement choices) &&
                    choices.GetArrayLength() > 0 &&
                    choices[0].TryGetProperty("message", out JsonElement message) &&
                    message.TryGetProperty("content", out JsonElement content))
                {
                    var gptResponse = content.GetString();
                    ChatHistory.Add(("GPT", gptResponse));
                    return Content(gptResponse);
                }

                return Content("Phản hồi từ API không hợp lệ.");
            }
            catch (Exception ex)
            {
                return Content($"Lỗi xảy ra: {ex.Message}");
            }
        }


        [HttpPost]
        public IActionResult ConfigureSettings(string language, string introduction)
        {
            if (!string.IsNullOrEmpty(language))
            {
                CurrentLanguage = language; // Cập nhật ngôn ngữ
            }

            if (!string.IsNullOrEmpty(introduction))
            {
                AssistantIntroduction = introduction; // Cập nhật giới thiệu
            }

            ChatHistory.Add(("System", "Cấu hình đã được cập nhật thành công."));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFileAjax(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                try
                {
                    // Tạo đường dẫn tạm thời để lưu hình ảnh
                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    // Đảm bảo thư mục tồn tại
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images"));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var fileUrl = $"/uploads/{fileName}";

                    // Thêm thông báo vào khung chat
                    ChatHistory.Add(("System", $"File '{file.FileName}' đã được tải lên thành công."));
                    return Json(new { success = true, url = fileUrl });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Lỗi xảy ra khi tải file: {ex.Message}" });
                }
            }

            return Json(new { success = false, message = "Không có file nào được tải lên!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult SaveHistory()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "chats", $"ChatHistory_{DateTime.Now:yyyyMMddHHmmss}.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                var chatContent = string.Join("\n", ChatHistory.Select(c => $"{c.Sender}: {c.Message}"));
                System.IO.File.WriteAllText(filePath, chatContent);

                ChatHistory.Add(("System", $"Lịch sử đã được lưu tại: {filePath}"));
            }
            catch (Exception ex)
            {
                ChatHistory.Add(("System", $"Lỗi xảy ra khi lưu lịch sử: {ex.Message}"));
            }

            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult ClearLearnedDataAndResetChat()
        {
            try
            {
                // Xóa toàn bộ dữ liệu đã học
                LearnedData.Clear();

                // Xóa lịch sử chat
                ChatHistory.Clear();

                // Thêm một thông điệp trống (hoặc chào mừng) để khung chat không bị trống hoàn toàn
                ChatHistory.Add(("System", "Chào cậu, tui có thể giúp gì cho cậu"));
            }
            catch (Exception ex)
            {
                ChatHistory.Add(("System", $"Lỗi xảy ra khi reset dữ liệu: {ex.Message}"));
            }

            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> GenerateImageFromChat(string userMessage, string size = "1024x1024")
        {
            if (string.IsNullOrEmpty(userMessage))
            {
                ChatHistory.Add(("System", "Vui lòng nhập mô tả hình ảnh để tạo ảnh."));
                return RedirectToAction("Index");
            }

            var validSizes = new List<string> { "256x256", "512x512", "1024x1024", "1024x1792", "1792x1024" };
            if (!validSizes.Contains(size))
            {
                ChatHistory.Add(("System", $"Kích thước '{size}' không hợp lệ. Vui lòng chọn từ: {string.Join(", ", validSizes)}."));
                return RedirectToAction("Index");
            }

            try
            {
                // Payload cho OpenAI DALL-E 3
                var payload = new
                {
                    prompt = userMessage,
                    n = 1, // Số lượng ảnh muốn tạo
                    size = size, // Kích thước ảnh
                    model = "dall-e-3" // Model DALL-E 3
                };

                // Gửi yêu cầu đến API OpenAI
                var response = await _httpClient.PostAsync(
                    "https://api.yescale.io/v1/images/generations",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ChatHistory.Add(("System", $"API DALL-E lỗi: {response.StatusCode}. Nội dung: {errorContent}"));
                    return RedirectToAction("Index");
                }

                // Xử lý phản hồi từ API
                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseBody);

                if (jsonDocument.RootElement.TryGetProperty("data", out JsonElement dataArray) &&
                    dataArray.GetArrayLength() > 0 &&
                    dataArray[0].TryGetProperty("url", out JsonElement imageUrl))
                {
                    var imageLink = imageUrl.GetString();

                    // Thêm thẻ <img> để hiển thị hình ảnh
                    ChatHistory.Add(("GPT", $"Hình ảnh đã được tạo:<br><img src='{imageLink}' alt='Hình ảnh tạo từ mô tả' style='max-width: 100%; height: auto;'>"));
                }
                else
                {
                    ChatHistory.Add(("System", "Không nhận được URL hình ảnh từ API."));
                }
            }
            catch (Exception ex)
            {
                ChatHistory.Add(("System", $"Lỗi xảy ra khi tạo ảnh: {ex.Message}"));
            }

            return RedirectToAction("Index");
        }



        private List<object> GetEnhancedContextMessages(string userMessage, string language)
        {
            var contextMessages = new List<object>
            {
                new
                {
                    role = "system",
                    content = $"{AssistantIntroduction} Hỗ trợ ngôn ngữ: {language}"
                }
            };

            if (LearnedData.Any())
            {
                contextMessages.Add(new
                {
                    role = "system",
                    content = $"Dữ liệu GPT đã học:\n{string.Join("\n", LearnedData.Take(100))}"
                });
            }

            contextMessages.AddRange(ChatHistory.Select(c => new
            {
                role = c.Sender == "User" ? "user" : "assistant",
                content = c.Message
            }));

            contextMessages.Add(new
            {
                role = "user",
                content = userMessage
            });

            return contextMessages;
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AnalyzeImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ChatHistory.Add(("System", "Vui lòng tải lên một tệp hình ảnh hợp lệ."));
                return RedirectToAction("Index");
            }

            try
            {
                // Lưu tệp ảnh tạm thời
                var tempFilePath = Path.GetTempFileName();
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Sử dụng Tesseract OCR để phân tích văn bản từ ảnh
                var ocrEngine = new Tesseract.TesseractEngine("./tessdata", "vie", Tesseract.EngineMode.Default);
                var img = Tesseract.Pix.LoadFromFile(tempFilePath);
                var result = ocrEngine.Process(img);
                var extractedText = result.GetText().Trim();

                if (string.IsNullOrEmpty(extractedText))
                {
                    ChatHistory.Add(("System", "Không thể trích xuất văn bản từ ảnh."));
                    return RedirectToAction("Index");
                }

                // Thêm nội dung OCR vào lịch sử chat
                ChatHistory.Add(("User", $"[Ảnh được phân tích]:\n{extractedText}"));

                // Gửi nội dung đến ChatGPT
                var payload = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new List<object>
            {
                new { role = "user", content = extractedText }
            },
                    max_tokens = 500
                };

                var response = await _httpClient.PostAsync(
                    "https://api.yescale.io/v1/chat/completions",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ChatHistory.Add(("System", $"API lỗi: {response.StatusCode}. Nội dung: {errorContent}"));
                    return RedirectToAction("Index");
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(responseBody);

                if (jsonDocument.RootElement.TryGetProperty("choices", out JsonElement choices) &&
                    choices.GetArrayLength() > 0 &&
                    choices[0].TryGetProperty("message", out JsonElement message) &&
                    message.TryGetProperty("content", out JsonElement content))
                {
                    var gptResponse = content.GetString();
                    ChatHistory.Add(("GPT", gptResponse));
                }
                else
                {
                    ChatHistory.Add(("System", "Phản hồi từ API không hợp lệ hoặc thiếu dữ liệu."));
                }
            }
            catch (Exception ex)
            {
                ChatHistory.Add(("System", $"Lỗi xảy ra khi phân tích ảnh: {ex.Message}"));
            }

            return RedirectToAction("Index");
        }


        private List<string> SplitDataIntoChunks(string data, int chunkSize = 500)
        {
            var chunks = new List<string>();
            for (int i = 0; i < data.Length; i += chunkSize)
            {
                chunks.Add(data.Substring(i, Math.Min(chunkSize, data.Length - i)));
            }
            return chunks;
        }
    }
}
