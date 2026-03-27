using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItemProcessor.Models
{
    public class Item
    {
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Weight is required")]
        [Range(0.001, 999999.999, ErrorMessage = "Weight must be greater than 0")]
        [Column(TypeName = "decimal(10,3)")]
        public decimal Weight { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("CreatedBy")]
        public User? Creator { get; set; }

        // Children of this item in processed items
        public ICollection<ProcessedItem> ParentProcessedItems { get; set; } 
            = new List<ProcessedItem>();
        public ICollection<ProcessedItem> ChildProcessedItems  { get; set; } 
            = new List<ProcessedItem>();
    }
}