using System.ComponentModel.DataAnnotations;

namespace ReceiptParserAPI.Models
{
    public class LineItem
    {
        [Key]
        public int Id { get; set; }

        // Yabancı Anahtar
        public int ReceiptId { get; set; }

        [Required]
        public string ItemName { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalLineAmount { get; set; }

        public Receipt Receipt { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

    }
}
