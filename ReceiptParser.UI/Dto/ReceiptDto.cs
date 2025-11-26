namespace ReceiptParser.UI.Dto
{
    public class ReceiptDto
    {
        public string StoreName { get; set; }
        public DateTime ReceiptDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public List<LineItemDto> LineItems { get; set; } 
    }
}
