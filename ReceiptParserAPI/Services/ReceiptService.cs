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

        public async Task<IEnumerable<object>> GetUserReceiptsListAsync(int userId)
        {
            var receipts = await _context.Receipts
                .Where(r => r.UserId == userId)
                .Select(r => new
                {
                    r.Id,
                    r.StoreName,
                    r.TotalAmount,
                    r.ReceiptDate,
                    r.UploadedAt
                })
                .ToListAsync();

            return receipts;
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
                        TotalLineAmount = li.TotalLineAmount
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
    }
}
