using ReceiptParserAPI.Dto;

namespace ReceiptParserAPI.Services
{
    public interface IReceiptService
    {
        Task<(decimal TotalSpent, int Count)?> GetUserTotalSpentAsync(int userId);
        Task<IEnumerable<ReceiptListDto>> GetUserReceiptsListAsync(int userId, ReceiptFilterDto filter);
        Task<IEnumerable<object>> GetAllReceiptsAsync();
        Task<bool> DeleteReceiptAsync(int receiptId, int userId);
        Task<IEnumerable<object>> GetCategoryReportAsync(int userId, ReceiptFilterDto filter);
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    }
}
