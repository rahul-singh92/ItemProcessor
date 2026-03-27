using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemProcessor.Models
{
    public class ProcessedItem
    {
        public int ProcessedItemId { get; set; }

        [Required(ErrorMessage = "Parent item is required")]
        public int ParentItemId { get; set; }

        [Required(ErrorMessage = "Child item is required")]
        public int ChildItemId { get; set; }

        [Required(ErrorMessage = "Output weight is required")]
        [Range(0.001, 999999.999, ErrorMessage = "Weight must be greater than 0")]
        [Column(TypeName = "decimal(10,3)")]
        public decimal OutputWeight { get; set; }

        public int ProcessedBy { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsProcessed { get; set; } = false;

        // Navigation properties
        [ForeignKey("ParentItemId")]
        public Item? ParentItem { get; set; }

        [ForeignKey("ChildItemId")]
        public Item? ChildItem { get; set; }

        [ForeignKey("ProcessedBy")]
        public User? Processor { get; set; }
    }
}