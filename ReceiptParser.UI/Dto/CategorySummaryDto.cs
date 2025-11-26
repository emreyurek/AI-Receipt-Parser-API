namespace ReceiptParser.UI.Dto
{
    public class CategorySummaryDto
    {
        public string CategoryName { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalSpent { get; set; }
        public string Currency { get; set; }
    }
}