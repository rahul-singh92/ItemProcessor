using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ItemProcessor.Models;
using System.Security.Claims;

namespace ItemProcessor.Controllers
{
    [Authorize] // ALL actions require login — redirects to /Account/Login if not
    public class ItemController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ItemController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Helper — gets the logged-in user's ID from their auth cookie claims
        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: /Item
        // Search is optional — if no search term, return all items
        [Route("Item")]
        [Route("Item/Index")]
        public async Task<IActionResult> Index(string? search)
        {
            Console.WriteLine($"=== IS AUTHENTICATED: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"=== USER: {User.Identity?.Name}");
            ViewBag.Search = search;

            var query = _db.Items
                .Include(i => i.Creator) // JOIN with Users table
                .Where(i => i.IsActive)  // only active items
                .AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query  = query.Where(i =>
                    i.ItemName.ToLower().Contains(search) ||
                    (i.Description != null &&
                     i.Description.ToLower().Contains(search)));
            }

            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(items);
        }

        // GET: /Item/Create
        public IActionResult Create() => View();

        // POST: /Item/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Item model)
        {
            // Remove Creator from validation — we set it manually
            ModelState.Remove("Creator");

            if (!ModelState.IsValid)
                return View(model);

            model.CreatedBy = GetUserId();
            model.CreatedAt = DateTime.Now;
            model.IsActive  = true;

            _db.Items.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Item '{model.ItemName}' created successfully!";
            return RedirectToAction("Index");
        }

        // GET: /Item/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.Items
                .FirstOrDefaultAsync(i => i.ItemId == id && i.IsActive);

            if (item == null)
                return NotFound();

            return View(item);
        }

        // POST: /Item/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Item model)
        {
            if (id != model.ItemId)
                return BadRequest();

            ModelState.Remove("Creator");

            if (!ModelState.IsValid)
                return View(model);

            var item = await _db.Items
                .FirstOrDefaultAsync(i => i.ItemId == id && i.IsActive);

            if (item == null)
                return NotFound();

            // Only update allowed fields — never trust all posted data
            item.ItemName    = model.ItemName;
            item.Weight      = model.Weight;
            item.Description = model.Description;

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Item '{item.ItemName}' updated successfully!";
            return RedirectToAction("Index");
        }

        // POST: /Item/Delete/5
        // Why POST not GET? GET requests can be triggered by links/bots — dangerous for deletes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Items
                .FirstOrDefaultAsync(i => i.ItemId == id && i.IsActive);

            if (item == null)
                return NotFound();

            // Soft delete — just mark inactive, never remove from DB
            item.IsActive = false;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Item '{item.ItemName}' deleted.";
            return RedirectToAction("Index");
        }

        // GET: /Item/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.Items
                .Include(i => i.Creator)
                .FirstOrDefaultAsync(i => i.ItemId == id && i.IsActive);

            if (item == null)
                return NotFound();

            return View(item);
        }
    }
}