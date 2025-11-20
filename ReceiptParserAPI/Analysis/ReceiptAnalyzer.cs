using System.Text.Json;
using System.Text;
using ReceiptParserAPI.Resilience;

namespace ReceiptParserAPI.Analysis
{
    public static class ReceiptAnalyzer
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public static async Task<ReceiptAnalysis> AnalyzeReceiptImage(byte[] imageBytes, string apiKey)
        {
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            // JSON formatı, artık LineItems listesini de içeriyor.
            string jsonFormat = "{\"StoreName\":\"\",\"ReceiptDate\":\"yyyy-MM-dd\",\"TotalAmount\":0.00, \"LineItems\": [{\"ItemName\":\"\",\"Quantity\":0.00,\"UnitPrice\":0.00,\"TotalLineAmount\":0.00}]}";

            string prompt =
                $"Bu fiş görselini analiz et. Fişten mağaza adını (StoreName), işlem tarihini (ReceiptDate), genel toplam tutarı (TotalAmount) ve tüm ürün kalemlerini (LineItems) bul. Çıktıyı kesinlikle bu formatta tek bir JSON nesnesi olarak ver: {jsonFormat}. Başka hiçbir metin veya açıklama EKLEME.";

            // Görseli Base64 string'e dönüştürme
            string base64Image = Convert.ToBase64String(imageBytes);

            // Gemini API'ye gönderilecek JSON istek gövdesi
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt },
                            new
                            {
                                inlineData = new
                                {
                                    mimeType = "image/jpeg",
                                    data = base64Image
                                }
                            } // Görsel
                        }
                    }
                }
            };

            string jsonPayload = JsonSerializer.Serialize(requestBody);

            try
            {

                // Polly
                var retryPolicy = GeminiRetryPolicy.GetPolicy();

                var response = await retryPolicy.ExecuteAsync(async () =>
                {
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    return await _httpClient.PostAsync(apiUrl, content);
                });

                // HTTP İsteğini Gönderme
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Cevabı Okuma ve Hata Kontrolü
                string responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ReceiptAnalysis
                    {
                        RawText = $"Gemini API Hatası: Durum Kodu {(int)response.StatusCode} ({response.StatusCode}). Cevap: {responseString}"
                    };
                }

                var geminiResponse = JsonSerializer.Deserialize<JsonDocument>(responseString);

                // Gömülü metin JSON'unu çıkar
                var rawJsonOutput = geminiResponse?
                                    .RootElement
                                    .GetProperty("candidates")[0]
                                    .GetProperty("content")
                                    .GetProperty("parts")[0]
                                    .GetProperty("text")
                                    .GetString() ?? "";

                // Gelen veriyi temizle ve modelimize dönüştür
                string cleanedJson = CleanJsonOutput(rawJsonOutput);

                var analysis = JsonSerializer.Deserialize<ReceiptAnalysis>(cleanedJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (analysis == null)
                    throw new JsonException("JSON Deserialization null sonuç döndürdü.");

                analysis.RawText = cleanedJson;
                return analysis;
            }
            catch (HttpRequestException ex)
            {
                // Ağ bağlantısı veya DNS hataları
                return new ReceiptAnalysis
                {
                    RawText = $"Ağ Bağlantısı Hatası: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                // Tüm diğer hataları (JSON ayrıştırma vb) 
                return new ReceiptAnalysis
                {
                    RawText = $"İç Uygulama Hatası: {ex.Message}"
                };
            }
        }
        private static string CleanJsonOutput(string rawOutput)
        {
            rawOutput = rawOutput.Trim();
            if (rawOutput.StartsWith("```"))
            {
                int start = rawOutput.IndexOf('{');
                int end = rawOutput.LastIndexOf('}');

                if (start > -1 && end > start)
                {
                    return rawOutput.Substring(start, end - start + 1);
                }
            }
            return rawOutput;
        }

    }
}