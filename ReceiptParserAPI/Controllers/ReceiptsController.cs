using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReceiptParserAPI.Analysis;
using ReceiptParserAPI.Data;
using ReceiptParserAPI.Dto;
using ReceiptParserAPI.Models;
using ReceiptParserAPI.Services;
using System.Security.Claims;

namespace ReceiptParserAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Bu Controller'daki tüm metotlar için JWT yetkilendirmesi zorunludur.
    public class ReceiptController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ReceiptDbContext _context;
        private readonly IReceiptService _receiptService;

        public ReceiptController(IConfiguration configuration, ReceiptDbContext context, IReceiptService receiptService)
        {
            _configuration = configuration;
            _context = context;
            _receiptService = receiptService;
        }

        // --- 1. FİŞ ANALİZİ VE KAYIT ---
        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeReceipt(IFormFile file)
        {
            // JWT'den gelen kullanıcı ID'sini al
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Kullanıcı kimliği token'dan alınamadı." });
            }

            // Dosya doğrulaması
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi");

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound($"Token'daki kullanıcı ID:{userId} veritabanında bulunamadı.");
            }

            // Api key kontrolü
            var apiKey = _configuration["GeminiApi:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return StatusCode(500, new { error = "API Anahtarı yapılandırma dosyasında bulunamadı." });
            }

            // Gemini API ile fişi analiz et ve analiz sonucunu veritabanı entitysine dönüştür ve kaydet
            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();


                var analysis = await ReceiptAnalyzer.AnalyzeReceiptImage(imageBytes, apiKey);

                if (analysis.StoreName.Contains("Hata"))
                {
                    return StatusCode(500, new { success = false, error = analysis.RawText });
                }

                // --- KATEGORİ İŞLEMLERİ ---

                // 1. Fişteki unique kategorileri bul
                var distinctCategories = analysis.LineItems?
                    .Select(i => i.Category ?? "Diğer")
                    .Distinct()
                    .ToList() ?? new List<string>();

                // 2. Bu kategorileri veritabanında bul eğer veritabanında yoksa oluştur 
                var categoryMap = new Dictionary<string, Category>();

                foreach (var catName in distinctCategories)
                {
                    // Veritabanında var mı?
                    var existingCat = await _context.Categories.FirstOrDefaultAsync(c => c.Name.ToLower() == catName.ToLower());

                    if (existingCat != null)
                    {
                        categoryMap[catName] = existingCat;
                    }
                    else
                    {
                        // Yoksa yeni oluştur
                        var newCat = new Category { Name = catName };
                        _context.Categories.Add(newCat);
                        await _context.SaveChangesAsync();
                        categoryMap[catName] = newCat;
                    }
                }

                var receiptEntity = new Receipt
                {
                    UserId = userId,
                    StoreName = analysis.StoreName,
                    ReceiptDate = analysis.ReceiptDate,
                    TotalAmount = analysis.TotalAmount ?? 0,
                    RawJson = analysis.RawText,
                    UploadedAt = DateTime.UtcNow,
                    LineItems = analysis.LineItems?
                     .Select(itemDto => new LineItem
                     {
                         ItemName = itemDto.ItemName,
                         Quantity = itemDto.Quantity,
                         UnitPrice = itemDto.UnitPrice,
                         TotalLineAmount = itemDto.TotalLineAmount,
                         Category = categoryMap[itemDto.Category]
                     }).ToList()
                     ?? new List<LineItem>()
                };

                _context.Receipts.Add(receiptEntity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Fiş başarıyla analiz edildi ve kaydedildi.",
                    receiptId = receiptEntity.Id,
                    userId = userId,
                    analysis = analysis,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = $"Genel Sunucu Hatası: {ex.Message}"
                });
            }
        }

        // --- 2. TOPLAM HARCAMA RAPORU ---
        [HttpGet("totalspent")]
        public async Task<IActionResult> GetTotalSpent()
        {
            // Kullanıcı ID'sini Token'dan Al
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Kullanıcı kimliği alınamadı." });
            }

            try
            {
                var result = await _receiptService.GetUserTotalSpentAsync(userId);

                if (result == null)
                {
                    // Fiş yoktur (0 döner) ya da DB hatası olmuştur.
                    return NotFound($"Kullanıcı ID {userId} veritabanında bulunamadı.");
                }

                return Ok(new
                {
                    userId = userId,
                    receiptCount = result.Value.Count,
                    totalSpent = result.Value.TotalSpent,
                    currency = "TRY"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Harcama raporu alınırken hata oluştu: {ex.Message}" });
            }
        }

        // --- 3 KULLANICIYA ÖZEL FİŞ LİSTESİ ---
        // (api/receipt/list?startDate=...&enddate=...)
        [HttpGet("list")]
        public async Task<IActionResult> GetReceiptsList([FromQuery] ReceiptFilterDto filter)
        {
            // Kullanıcı ID'sini Token'dan Al
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Kullanıcı kimliği alınamadı." });
            }

            var receipts = await _receiptService.GetUserReceiptsListAsync(userId, filter);

            return Ok(receipts);
        }

        //  --- 4 TÜM FİŞ LİSTESİ ---
        [HttpGet("Getall")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var receipts = await _receiptService.GetAllReceiptsAsync();

            return Ok(receipts);
        }

        //  --- 5 FİŞ SİL ---
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteReceipt(int id)
        {
            // Kullanıcı ID'sini Token'dan Al
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Kullanıcı kimliği alınamadı." });
            }

            var result = await _receiptService.DeleteReceiptAsync(id, userId);

            if (!result)
            {
                return NotFound(new { message = "Fiş bulunamadı veya silme yetkiniz yok." });
            }

            return Ok(new { success = true, message = "Fiş başarıyla silindi." });
        }

        // --- 6. KATEGORİ BAZLI HARCAMA ÖZETİ RAPORU ---
        //(api/receipt/summary/categories?startDate=...)
        [HttpGet("summary/categories")]
        public async Task<IActionResult> GetCategorySummary([FromQuery] ReceiptFilterDto filter)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Kullanıcı kimliği alınamadı." });
            }

            var report = await _receiptService.GetCategoryReportAsync(userId, filter);

            if (report == null || !report.Any())
            {
                return NotFound(new { message = "Belirtilen tarihlerde harcama kaydı bulunamadı." });
            }

            return Ok(report);
        }

        // --- 7. KATEGORİLERİ LİSTELEME ---
        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _receiptService.GetAllCategoriesAsync();
            return Ok(categories);
        }
    }
}