using System.ComponentModel.DataAnnotations;

namespace ItemProcessor.Models.ViewModels
{
    public class ProcessItemViewModel
    {
        // The parent item being processed
        public int ParentItemId { get; set; }
        public string ParentItemName { get; set; } = string.Empty;
        public decimal ParentWeight { get; set; }

        // List of child items to create during processing
        public List<ChildItemInput> ChildItems { get; set; } 
            = new List<ChildItemInput>();

        // All available items for the dropdown
        public List<Item> AvailableItems { get; set; } 
            = new List<Item>();
    }

    public class ChildItemInput
    {
        [Required(ErrorMessage = "Child item name is required")]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        [Range(0.001, 999999.999, ErrorMessage = "Weight must be greater than 0")]
        public decimal Weight { get; set; }

        public string? Notes { get; set; }
    }
}
