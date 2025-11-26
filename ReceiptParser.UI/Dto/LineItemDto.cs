namespace ReceiptParser.UI.Dto
{
    public class LineItemDto
    {
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; } 
        public decimal TotalLineAmount { get; set; }
        public string Category { get; set; }

    }
}
