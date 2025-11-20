namespace ReceiptParserAPI.Services
{
    public interface IReceiptService
    {
        Task<(decimal TotalSpent, int Count)?> GetUserTotalSpentAsync(int userId);
        Task<IEnumerable<object>> GetUserReceiptsListAsync(int userId);
        Task<IEnumerable<object>> GetAllReceiptsAsync();
        Task<bool> DeleteReceiptAsync(int receiptId, int userId);
    }
}
