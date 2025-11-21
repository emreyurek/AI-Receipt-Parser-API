using System.ComponentModel.DataAnnotations;

namespace ReceiptParserAPI.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public ICollection<LineItem> LineItems { get; set; }
    }
}
