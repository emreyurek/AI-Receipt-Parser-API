using ReceiptParserAPI.Dto;

namespace ReceiptParserAPI.Analysis
{
    public class ReceiptAnalysis
    {
        public string StoreName { get; set; }
        public DateTime ReceiptDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string RawText { get; set; }
        public List<ItemDto>? LineItems { get; set; }
    }
}
