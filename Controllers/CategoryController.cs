using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tourism;

namespace Tourism.Controllers_
{
    public class CategoryController : Controller
    {
        private readonly TourismDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryController(TourismDbContext context, SignInManager<User> signInManager, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }
            var tours = await _context.Tours.Where(t => t.CategoryId == category.CategoryId).Include(t => t.Category).ToListAsync();
            ViewData["Tours"] = tours;

            return View(category);
        }

        // GET: Category/Create
        [Authorize(Roles ="admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Create([Bind("CategoryId,Info,Name")] Category category, [FromForm] IFormFile? MainPhotoFile)
        {
            if (ModelState.IsValid)
            {
                if(MainPhotoFile != null)
                {
                    string folder = "Categories/MainPhotos/";
                    string FileNameWithoutSpaces = string.Join("", MainPhotoFile.FileName.Split(" ", StringSplitOptions.RemoveEmptyEntries));
                    folder +=  Guid.NewGuid().ToString() + "_" + FileNameWithoutSpaces;
                    string ServerFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                    await MainPhotoFile.CopyToAsync(new FileStream(ServerFolder, FileMode.Create));
                    category.MainPhoto = "/"+folder;
                }
                category.Info = category.Info?.Replace("\n", "<br / >");
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Edit/5
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            category.Info = category.Info?.Replace("<br / >", "\n");
            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Info,MainPhoto,Name")] Category category, [FromForm] IFormFile? MainPhotoFile)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if(MainPhotoFile != null)
                    {
                        string folder = "Categories/MainPhotos/";
                        string FileNameWithoutSpaces = string.Join("", MainPhotoFile.FileName.Split(" ", StringSplitOptions.RemoveEmptyEntries));
                        folder +=  Guid.NewGuid().ToString() + "_" + FileNameWithoutSpaces;
                        string ServerFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                        await MainPhotoFile.CopyToAsync(new FileStream(ServerFolder, FileMode.Create));
                        category.MainPhoto = "/"+folder;
                    }
                    category.Info = category.Info?.Replace("\n", "<br / >");
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Delete/5
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
