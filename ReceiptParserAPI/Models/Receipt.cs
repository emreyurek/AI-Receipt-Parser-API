using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReceiptParserAPI.Models
{
    public class Receipt
    {
        [Key]
        public int Id { get; set; }

        // Foreign Key
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string StoreName { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime ReceiptDate { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "TEXT")] // SQLite için uzun metin (Ham JSON)
        public string RawJson { get; set; }

        public User User { get; set; }

        // İlişki: Bu fişin birden fazla ürün kalemi olabilir.
        public ICollection<LineItem> LineItems { get; set; } = new List<LineItem>();
    }
}