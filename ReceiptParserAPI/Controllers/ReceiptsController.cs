using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReceiptParserAPI.Analysis;
using ReceiptParserAPI.Data;
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

                var receiptEntity = new Receipt
                {
                    UserId = userId,
                    StoreName = analysis.StoreName,
                    ReceiptDate = analysis.ReceiptDate,
                    TotalAmount = analysis.TotalAmount ?? 0,
                    RawJson = analysis.RawText,
                    UploadedAt = DateTime.UtcNow,

                    // Ürünleri Entity'ye Dönüştürme
                    LineItems = analysis.LineItems?
                     .Select(itemDto => new LineItem
                     {
                         ItemName = itemDto.ItemName,
                         Quantity = itemDto.Quantity,
                         UnitPrice = itemDto.UnitPrice,
                         TotalLineAmount = itemDto.TotalLineAmount
                     }).ToList()
                     ?? new List<LineItem>() // Null ise boş liste ata
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

        // --- 2. TOPLAM HARCAMA RAPORU (TEMİZLENMİŞ) ---
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
        [HttpGet("list")]
        public async Task<IActionResult> GetReceiptsList()
        {
            // Kullanıcı ID'sini Token'dan Al
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Kullanıcı kimliği alınamadı." });
            }

            var receipts = await _receiptService.GetUserReceiptsListAsync(userId);

            return Ok(receipts);
        }

        //  --- 4 FİŞ LİSTESİ ---
        [HttpGet("Getall")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var receipts = await _receiptService.GetAllReceiptsAsync();

            return Ok(receipts);
        }

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
    }
}