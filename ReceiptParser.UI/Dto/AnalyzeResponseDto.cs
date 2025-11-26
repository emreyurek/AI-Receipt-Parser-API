namespace ReceiptParser.UI.Dto
{
    public class AnalyzeResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? ReceiptId { get; set; }
        public int? UserId { get; set; }
        public ReceiptDto Analysis { get; set; }
    }
}
