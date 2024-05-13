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
    public class CityController : Controller
    {
        private readonly TourismDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public CityController(TourismDbContext context, SignInManager<User> signInManager, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: City
        public async Task<IActionResult> Index()
        {
            var tourismDbContext = _context.Cities.Include(c => c.Region);
            return View(await tourismDbContext.ToListAsync());
        }

        // GET: City/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities
                .Include(c => c.Region)
                .FirstOrDefaultAsync(m => m.CityId == id);
            if (city == null)
            {
                return NotFound();
            }

            var Tours = await _context.Tours.Where(tour => tour.CityId == id && tour.StartDate > DateTime.UtcNow && tour.AvaibleTickets > 0).Include(tour => tour.Category).ToListAsync();
            ViewData["Tours"] = Tours;
            return View(city);
        }

        // GET: City/Create
        [Authorize(Roles ="admin")]
        public IActionResult Create()
        {
            ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "Name");
            return View();
        }

        // POST: City/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Create([Bind("CityId,RegionId,Info,Name")] City city)
        {
            if (ModelState.IsValid)
            {
                city.Info = city.Info?.Replace("\n", "<br / >");
                _context.Add(city);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "Name", city.RegionId);
            return View(city);
        }

        // GET: City/Edit/5
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }
            city.Info = city.Info?.Replace("<br / >", "\n");
            ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "Name", city.RegionId);
            return View(city);
        }

        // POST: City/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Edit(int id, [Bind("CityId,RegionId,Info,Name")] City city)
        {
            if (id != city.CityId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    city.Info = city.Info?.Replace("\n", "<br / >");
                    _context.Update(city);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CityExists(city.CityId))
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
            ViewData["RegionId"] = new SelectList(_context.Regions, "RegionId", "Name", city.RegionId);
            return View(city);
        }

        // GET: City/Delete/5
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var city = await _context.Cities
                .Include(c => c.Region)
                .FirstOrDefaultAsync(m => m.CityId == id);
            if (city == null)
            {
                return NotFound();
            }
            var Tours = await _context.Tours.Where(tour => tour.CityId == id && tour.StartDate > DateTime.UtcNow && tour.AvaibleTickets > 0).Include(tour => tour.Category).ToListAsync();
            ViewData["Tours"] = Tours;

            return View(city);
        }

        // POST: City/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city != null)
            {
                _context.Cities.Remove(city);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CityExists(int id)
        {
            return _context.Cities.Any(e => e.CityId == id);
        }
    }
}
