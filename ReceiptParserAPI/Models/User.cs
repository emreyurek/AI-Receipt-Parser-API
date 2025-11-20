using System.ComponentModel.DataAnnotations;

namespace ReceiptParserAPI.Models
{
    [Tags("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Receipt>? Receipts { get; set; } = new List<Receipt>();
    }
}
