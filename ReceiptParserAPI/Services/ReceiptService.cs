using Microsoft.EntityFrameworkCore;
using ReceiptParserAPI.Data;
using ReceiptParserAPI.Dto;

namespace ReceiptParserAPI.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly ReceiptDbContext _context;

        public ReceiptService(ReceiptDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReceiptListDto>> GetUserReceiptsListAsync(int userId, ReceiptFilterDto filter)
        {
            // Temel Sorgu
            var query = _context.Receipts
                .Where(r => r.UserId == userId)
                .AsQueryable();

            // Tarih Filtreleri 
            if (filter.StartDate.HasValue)
            {
                query = query.Where(r => r.ReceiptDate >= filter.StartDate.Value.Date);
            }

            if (filter.EndDate.HasValue)
            {
                // Bitiş gününün SON saniyesini dahil et
                query = query.Where(r => r.ReceiptDate < filter.EndDate.Value.Date.AddDays(1));
            }

            //  Sıralama
            query = query.OrderByDescending(r => r.ReceiptDate);

            var result = await query
                .Select(r => new ReceiptListDto
                {
                    Id = r.Id,
                    StoreName = r.StoreName,
                    TotalAmount = r.TotalAmount,
                    ReceiptDate = r.ReceiptDate,
                    UploadedAt = r.UploadedAt,
                    LineItems = r.LineItems.Select(li => new LineItemDto
                    {
                        ItemName = li.ItemName,
                        Quantity = li.Quantity,
                        TotalLineAmount = li.TotalLineAmount,
                        Category = li.Category.Name
                    }).ToList()
                })
                .ToListAsync();

            return result;
        }
        public async Task<(decimal TotalSpent, int Count)?> GetUserTotalSpentAsync(int userId)
        {
            var totalSpentDouble = await _context.Receipts
                                                 .Where(r => r.UserId == userId)
                                                 .SumAsync(r => (double?)r.TotalAmount);

            if (!totalSpentDouble.HasValue)
            {
                return (0, 0);
            }

            var receiptCount = await _context.Receipts
                                             .CountAsync(r => r.UserId == userId);

            return ((decimal)totalSpentDouble.Value, receiptCount);
        }
        public async Task<IEnumerable<object>> GetAllReceiptsAsync()
        {
            var receipts = await _context.Receipts
                .Select(r => new ReceiptListDto
                {
                    Id = r.Id,
                    StoreName = r.StoreName,
                    TotalAmount = r.TotalAmount,
                    ReceiptDate = r.ReceiptDate,
                    UploadedAt = r.UploadedAt,
                    LineItems = r.LineItems.Select(li => new LineItemDto
                    {
                        ItemName = li.ItemName,
                        Quantity = li.Quantity,
                        TotalLineAmount = li.TotalLineAmount,
                        Category = li.Category.Name
                    }).ToList()
                })
                .ToListAsync();

            return receipts;
        }
        public async Task<bool> DeleteReceiptAsync(int receiptId, int userId)
        {
            var receipt = await _context.Receipts
                .FirstOrDefaultAsync(r => r.Id == receiptId);
            if (receipt == null) return false;

            if (receipt.UserId != userId) return false;

            _context.Receipts.Remove(receipt);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<object>> GetCategoryReportAsync(int userId, ReceiptFilterDto filter)
        {

            var query = _context.LineItems
                         .Where(li => li.Receipt.UserId == userId)
                         .AsQueryable();

            if (filter.StartDate.HasValue)
            {
                query = query.Where(li => li.Receipt.ReceiptDate >= filter.StartDate.Value.Date);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(li => li.Receipt.ReceiptDate < filter.EndDate.Value.Date.AddDays(1));
            }

            // Gruplama ve özetleme
            var report = await query
                .GroupBy(li => li.Category.Name) // Kategori adına göre grupla
                .Select(g => new
                {
                    CategoryName = g.Key,
                    TotalSpent = g.Sum(li => (double)li.TotalLineAmount), // Toplam harcama
                    ItemCount = g.Count() // Ürün sayısı
                })
                .OrderByDescending(r => r.TotalSpent) // En çok harcanandan en aza sırala
                .ToListAsync();

            return report.Select(r => new
            {
                r.CategoryName,
                r.ItemCount,
                TotalSpent = (decimal)r.TotalSpent,
                Currency = "TRY"
            });
        }
    }
}
