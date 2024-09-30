using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Tourism;
using TourismMVCProject.Services.Interfaces;

namespace Tourism.Controllers_
{
    public class CategoryController : Controller
{
    private readonly TourismDbContext _context;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileService _fileService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CategoryController> _logger;
    private const string ALL_CATEGORIES_KEY = "all_categories";
    private const string CATEGORY_DETAILS_KEY = "category_details_{0}"; // {0} - categoryId
    private const int CACHE_DURATION_MINUTES = 10;

    public CategoryController(
        TourismDbContext context, 
        SignInManager<User> signInManager, 
        UserManager<User> userManager, 
        IWebHostEnvironment webHostEnvironment, 
        IFileService fileService,
        IMemoryCache cache,
        ILogger<CategoryController> logger)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _webHostEnvironment = webHostEnvironment;
        _fileService = fileService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var stopwatch = Stopwatch.StartNew();
        bool isCacheHit = true;

        var categories = await _cache.GetOrCreateAsync(ALL_CATEGORIES_KEY, async entry =>
        {
            isCacheHit = false;
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            entry.SlidingExpiration = TimeSpan.FromMinutes(2);
            
            // Імітуємо затримку БД для наочності
            await Task.Delay(1000); 
            var result = await _context.Categories.ToListAsync();
            
            _logger.LogInformation($"Дані завантажено з БД. Кількість категорій: {result.Count}");
            return result;
        });

        stopwatch.Stop();
        
        if(isCacheHit)
        {
            _logger.LogInformation($"Дані отримано з кешу. Час виконання: {stopwatch.ElapsedMilliseconds}ms");
        }
        else
        {
            _logger.LogInformation($"Дані отримано з БД. Час виконання: {stopwatch.ElapsedMilliseconds}ms");
        }

        // Додаємо інформацію про джерело даних у ViewBag для відображення
        ViewBag.DataSource = isCacheHit ? "Cache" : "Database";
        ViewBag.ExecutionTime = stopwatch.ElapsedMilliseconds;

        return View(categories);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var cacheKey = string.Format(CATEGORY_DETAILS_KEY, id);
        
        var category = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES);
            entry.SlidingExpiration = TimeSpan.FromMinutes(2);
            
            return await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
        });

        if (category == null)
        {
            return NotFound();
        }

        var tours = await _context.Tours
            .Where(t => t.CategoryId == category.CategoryId)
            .Include(t => t.Category)
            .ToListAsync();
            
        ViewData["Tours"] = tours;

        return View(category);
    }

    // GET: Category/Create
    [Authorize(Roles ="admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    // [ValidateAntiForgeryToken]
    [Authorize(Roles ="admin")]
    public async Task<IActionResult> Create([Bind("CategoryId,Info,Name")] Category category, [FromForm] IFormFile? MainPhotoFile)
    {
        if (ModelState.IsValid)
        {
            if(MainPhotoFile != null)
            {
                category.MainPhoto = await _fileService.UploadAsync(MainPhotoFile, "Categories/MainPhotos/");
            }
            category.Info = category.Info?.Replace("\n", "<br / >");
            _context.Add(category);
            await _context.SaveChangesAsync();
            
            // Інвалідуємо кеш всіх категорій
            _cache.Remove(ALL_CATEGORIES_KEY);
            
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

    [HttpPost]
    // [ValidateAntiForgeryToken]
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
                    if(category.MainPhoto != null)
                    {
                        await _fileService.DeleteAsync(category.MainPhoto);
                    }
                    category.MainPhoto = await _fileService.UploadAsync(MainPhotoFile, "Categories/MainPhotos/");
                }
                category.Info = category.Info?.Replace("\n", "<br / >");
                _context.Update(category);
                await _context.SaveChangesAsync();

                // Інвалідуємо кеш
                _cache.Remove(ALL_CATEGORIES_KEY);
                _cache.Remove(string.Format(CATEGORY_DETAILS_KEY, id));
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

    [HttpPost, ActionName("Delete")]
    // [ValidateAntiForgeryToken]
    [Authorize(Roles ="admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            // Інвалідуємо кеш
            _cache.Remove(ALL_CATEGORIES_KEY);
            _cache.Remove(string.Format(CATEGORY_DETAILS_KEY, id));
        }

        return RedirectToAction(nameof(Index));
    }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
