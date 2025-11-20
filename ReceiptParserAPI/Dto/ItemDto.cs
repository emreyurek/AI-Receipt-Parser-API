namespace ReceiptParserAPI.Dto
{
    public class ItemDto
    {
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalLineAmount { get; set; }
    }
}
