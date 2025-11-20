namespace ReceiptParserAPI.Dto
{
    public class ReceiptListDto
    {
        public int Id { get; set; }
        public string StoreName { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime ReceiptDate { get; set; }
        public DateTime UploadedAt { get; set; }

        // Ürün kalemlerini ekledik
        public List<LineItemDto> LineItems { get; set; } = new List<LineItemDto>();
    }
}
