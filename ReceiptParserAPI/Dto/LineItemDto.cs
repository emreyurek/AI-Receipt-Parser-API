namespace ReceiptParserAPI.Dto
{
    public class LineItemDto
    {
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalLineAmount { get; set; }
        public string Category { get; set; }

    }
}
