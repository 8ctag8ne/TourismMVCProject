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
    public class RegionController : Controller
    {
        private readonly TourismDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public RegionController(TourismDbContext context, SignInManager<User> signInManager, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Region
        public async Task<IActionResult> Index()
        {
            return View(await _context.Regions.ToListAsync());
        }

        // GET: Region/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var region = await _context.Regions
                .FirstOrDefaultAsync(m => m.RegionId == id);
            if (region == null)
            {
                return NotFound();
            }
            var cities = await _context.Cities.Where(c => c.RegionId == id).ToListAsync();

            ViewData["cities"] = cities;

            return View(region);
        }

        // GET: Region/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Region/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("RegionId,Info,Name")] Region region)
        {
            if (ModelState.IsValid)
            {
                region.Info = region.Info?.Replace("\n", "<br / >");
                _context.Add(region);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(region);
        }

        // GET: Region/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var region = await _context.Regions.FindAsync(id);
            if (region == null)
            {
                return NotFound();
            }
            region.Info = region.Info?.Replace("<br / >", "\n");
            return View(region);
        }

        // POST: Region/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("RegionId,Info,Name")] Region region)
        {
            if (id != region.RegionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    region.Info = region.Info?.Replace("\n", "<br / >");
                    _context.Update(region);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegionExists(region.RegionId))
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
            return View(region);
        }

        // GET: Region/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var region = await _context.Regions
                .FirstOrDefaultAsync(m => m.RegionId == id);
            if (region == null)
            {
                return NotFound();
            }
            var cities = await _context.Cities.Where(c => c.RegionId == id).ToListAsync();

            ViewData["cities"] = cities;

            return View(region);
        }

        // POST: Region/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var region = await _context.Regions.FindAsync(id);
            if (region != null)
            {
                _context.Regions.Remove(region);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RegionExists(int id)
        {
            return _context.Regions.Any(e => e.RegionId == id);
        }
    }
}
