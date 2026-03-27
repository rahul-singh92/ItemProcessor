using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ItemProcessor.Models;
using ItemProcessor.Models.ViewModels;
using System.Security.Claims;

namespace ItemProcessor.Controllers
{
    [Authorize]
    public class ProcessController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProcessController(ApplicationDbContext db)
        {
            _db = db;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: /Process
        // Lists all processed items
        public async Task<IActionResult> Index()
        {
            var processed = await _db.ProcessedItems
                .Include(p => p.ParentItem)
                .Include(p => p.ChildItem)
                .Include(p => p.Processor)
                .OrderByDescending(p => p.ProcessedAt)
                .ToListAsync();

            return View(processed);
        }

        // GET: /Process/Create
        public async Task<IActionResult> Create()
        {
            var model = new ProcessItemViewModel
            {
                AvailableItems = await _db.Items
                    .Where(i => i.IsActive)
                    .OrderBy(i => i.ItemName)
                    .ToListAsync(),

                // Start with one empty child slot
                ChildItems = new List<ChildItemInput>
                {
                    new ChildItemInput()
                }
            };

            return View(model);
        }

        // POST: /Process/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProcessItemViewModel model)
        {
            // Reload available items for the dropdown
            model.AvailableItems = await _db.Items
                .Where(i => i.IsActive)
                .OrderBy(i => i.ItemName)
                .ToListAsync();

            // Validate parent item exists
            var parentItem = await _db.Items
                .FirstOrDefaultAsync(i =>
                    i.ItemId == model.ParentItemId && i.IsActive);

            if (parentItem == null)
            {
                ModelState.AddModelError("ParentItemId", "Selected item not found");
                return View(model);
            }

            // Validate at least one child item exists
            if (model.ChildItems == null || !model.ChildItems.Any())
            {
                ModelState.AddModelError("", "Add at least one output item");
                return View(model);
            }

            // Validate total child weight doesn't exceed parent weight
            // Why? Conservation of mass — you can't get more output than input
            var totalChildWeight = model.ChildItems.Sum(c => c.Weight);
            if (totalChildWeight > parentItem.Weight)
            {
                ModelState.AddModelError("",
                    $"Total output weight ({totalChildWeight:F3}) cannot exceed " +
                    $"parent weight ({parentItem.Weight:F3})");
                return View(model);
            }

            if (!ModelState.IsValid)
                return View(model);

            var userId = GetUserId();

            // Use a transaction — all child items must save or none do
            // Why transaction? If saving fails halfway, we don't get partial data
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var child in model.ChildItems)
                {
                    // Create the child item in Items table
                    var childItem = new Item
                    {
                        ItemName    = child.ItemName,
                        Weight      = child.Weight,
                        Description = child.Notes,
                        CreatedBy   = userId,
                        CreatedAt   = DateTime.Now,
                        IsActive    = true
                    };

                    _db.Items.Add(childItem);
                    await _db.SaveChangesAsync(); // save to get the new ItemId

                    // Create the processing record linking parent to child
                    var processedItem = new ProcessedItem
                    {
                        ParentItemId  = model.ParentItemId,
                        ChildItemId   = childItem.ItemId,
                        OutputWeight  = child.Weight,
                        ProcessedBy   = userId,
                        ProcessedAt   = DateTime.Now,
                        Notes         = child.Notes,
                        IsProcessed   = true
                    };

                    _db.ProcessedItems.Add(processedItem);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] =
                    $"'{parentItem.ItemName}' processed into " +
                    $"{model.ChildItems.Count} output item(s)!";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Processing failed: {ex.Message}");
                return View(model);
            }
        }

        // GET: /Process/Tree/5
        // Shows the full recursive tree for an item
        public async Task<IActionResult> Tree(int id)
        {
            var rootItem = await _db.Items
                .FirstOrDefaultAsync(i => i.ItemId == id && i.IsActive);

            if (rootItem == null)
                return NotFound();

            // Build the recursive tree starting from this item
            var tree = await BuildTree(id, 0);

            ViewBag.RootName = rootItem.ItemName;
            return View(tree);
        }

        // Recursive method — builds the full tree for any item
        // Why recursion? The tree depth is unknown — an item can have children
        // which have children, which have more children, etc.
        private async Task<ItemTreeViewModel> BuildTree(int itemId, int level)
        {
            var item = await _db.Items
                .FirstOrDefaultAsync(i => i.ItemId == itemId);

            if (item == null)
                return new ItemTreeViewModel();

            var node = new ItemTreeViewModel
            {
                ItemId   = item.ItemId,
                ItemName = item.ItemName,
                Weight   = item.Weight,
                Level    = level,
                Children = new List<ItemTreeViewModel>()
            };

            // Find all direct children of this item
            var childLinks = await _db.ProcessedItems
                .Where(p => p.ParentItemId == itemId)
                .ToListAsync();

            // Recursively build each child's subtree
            // Base case: when no children exist, the loop doesn't execute
            foreach (var link in childLinks)
            {
                var childNode = await BuildTree(link.ChildItemId, level + 1);
                childNode.OutputWeight = link.OutputWeight;
                childNode.IsProcessed  = link.IsProcessed;
                node.Children.Add(childNode);
            }

            return node;
        }

        // GET: /Process/AllTrees
        // Shows root items (items that are never a child) for tree selection
        public async Task<IActionResult> AllTrees()
        {
            // Root items = items that don't appear as a child in ProcessedItems
            var childIds = await _db.ProcessedItems
                .Select(p => p.ChildItemId)
                .Distinct()
                .ToListAsync();

            var rootItems = await _db.Items
                .Where(i => i.IsActive && !childIds.Contains(i.ItemId))
                .OrderBy(i => i.ItemName)
                .ToListAsync();

            return View(rootItems);
        }
    }
}
