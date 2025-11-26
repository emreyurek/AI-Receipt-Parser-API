namespace ReceiptParser.UI.Dto
{
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public string Message { get; set; } 
    }
}
