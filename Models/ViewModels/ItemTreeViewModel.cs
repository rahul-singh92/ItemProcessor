namespace ItemProcessor.Models.ViewModels
{
    public class ItemTreeViewModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal? OutputWeight { get; set; }
        public bool IsProcessed { get; set; }
        public int Level { get; set; }

        // Children of this node — recursive structure
        public List<ItemTreeViewModel> Children { get; set; } 
            = new List<ItemTreeViewModel>();
    }
}
