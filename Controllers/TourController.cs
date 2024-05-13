using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tourism;
using Tourism.Status;

namespace Tourism.Controllers_
{
    public class TourController : Controller
    {

        private readonly TourismDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public TourController(TourismDbContext context, SignInManager<User> signInManager, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Tour
        public async Task<IActionResult> Index(string searchString, DateTime? startDate, DateTime? endDate, int? price, int? categoryId)
        {
            var tourismDbContext = _context.Tours.Include(t => t.Category).Include(t => t.City);
            if(searchString != null)
            {
                tourismDbContext = tourismDbContext.Where(t => t.Name.Contains(searchString)).Include(t => t.Category).Include(t => t.City);
                ViewData["searchString"] = searchString;
            }
            if(startDate != null)
            {
                tourismDbContext = tourismDbContext.Where(t => t.StartDate >= startDate).Include(t => t.Category).Include(t => t.City);
                ViewData["startDate"] = startDate;
            }
            if(endDate != null)
            {
                tourismDbContext = tourismDbContext.Where(t => t.EndDate <= endDate).Include(t => t.Category).Include(t => t.City);
                ViewData["endDate"] = endDate;
            }

            if(price != null)
            {
                tourismDbContext = tourismDbContext.Where(t => price >= t.Price*9/10 && price <= t.Price*11/10).Include(t => t.Category).Include(t => t.City);
                ViewData["price"] = price;
            }

            if(categoryId != null)
            {
                tourismDbContext = tourismDbContext.Where(t => t.CategoryId == categoryId).Include(t => t.Category).Include(t => t.City);
                ViewData["Category"] = categoryId;
            }

            tourismDbContext.Reverse();
            var Categories = new SelectList(_context.Categories, "CategoryId", "Name", categoryId).ToList();
            Categories.Insert(0, new SelectListItem() { Value = "null", Text = "Оберіть категорію" });
            ViewData["CategoryId"] = new SelectList(Categories, "Value", "Text", categoryId);
            return View(await tourismDbContext.ToListAsync());
        }

        // GET: Tour/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .Include(t => t.Category)
                .Include(t => t.City)
                .FirstOrDefaultAsync(m => m.TourId == id);
            if (tour == null)
            {
                return NotFound();
            }
            var isBooked = _context.Orders.Any(order => order.TourId == id 
            && Convert.ToString(order.UserId ?? 0) == this.User.FindFirstValue(ClaimTypes.NameIdentifier) 
            && order.Status != StatusHelper.GetStatus(StatusEnum.Cancelled));

            var comments = await _context.Comments.Where(c=> c.TourId == id).Include(c => c.User).ToListAsync();

            ViewData["Booked"] = isBooked;
            ViewData["Comments"] = comments;

            return View(tour);
        }

        // GET: Tour/Create
        [Authorize(Roles = "guide,admin")]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewData["CityId"] = new SelectList(_context.Cities, "CityId", "Name");
            ViewData["Guides"] = new MultiSelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: Tour/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "guide,admin")]
        public async Task<IActionResult> Create([Bind("TourId,CityId,Info,MainPhoto,Price,StartDate,EndDate,Capacity,CategoryId,StartPointName,StartPointGeo,Name,Guides")] Tour tour)
        {
            if (ModelState.IsValid)
            {
                tour.AvaibleTickets = tour.Capacity;
                foreach(var guide in tour.Guides)
                {
                    var gd = new GuideTour{
                                    Tour = tour, 
                                    TourId = tour.TourId, 
                                    Guide = await _context.Users.FirstOrDefaultAsync(g => g.Id == guide), 
                                    GuideId = guide};
                    _context.Add(gd);
                }
                _context.Add(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", tour.CategoryId);
            ViewData["CityId"] = new SelectList(_context.Cities, "CityId", "Name", tour.CityId);
            ViewData["Guides"] = new MultiSelectList(_context.Users, "Id", "UserName");
            return View(tour);
        }

        // GET: Tour/Edit/5
        [Authorize(Roles = "guide,admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", tour.CategoryId);
            ViewData["CityId"] = new SelectList(_context.Cities, "CityId", "Name", tour.CityId);
            tour.Guides = _context.GuideTours.Where(gd => gd.TourId == id).Select(gd => gd.GuideId).ToList();
            ViewData["Guides"] = new MultiSelectList(_context.Users, "Id", "UserName");
            return View(tour);
        }

        // POST: Tour/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "guide,admin")]
        public async Task<IActionResult> Edit(int id, [Bind("TourId,CityId,Info,MainPhoto,Price,StartDate,EndDate,Capacity,AvaibleTickets,CategoryId,StartPointName,StartPointGeo,Name, Guides")] Tour tour)
        {
            if (id != tour.TourId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var GuideList = _context.GuideTours.Where(gd => gd.TourId == id).ToList();
                    foreach(var gd in GuideList)
                    {
                        _context.Remove(gd);
                    }
                    foreach(var guide in tour.Guides)
                    {
                        var gd = new GuideTour{TourId = id, GuideId = guide};
                        _context.Add(gd);
                    }
                    _context.Update(tour);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TourExists(tour.TourId))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", tour.CategoryId);
            ViewData["CityId"] = new SelectList(_context.Cities, "CityId", "Name", tour.CityId);
            tour.Guides = _context.GuideTours.Where(gd => gd.TourId == id).Select(gd => gd.GuideId).ToList();
            ViewData["Guides"] = new MultiSelectList(_context.Users, "Id", "UserName");
            return View(tour);
        }

        // GET: Tour/Delete/5
        [Authorize(Roles = "guide,admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tour = await _context.Tours
                .Include(t => t.Category)
                .Include(t => t.City)
                .FirstOrDefaultAsync(m => m.TourId == id);
            if (tour == null)
            {
                return NotFound();
            }
            var isBooked = _context.Orders.Any(order => order.TourId == id 
            && Convert.ToString(order.UserId ?? 0) == this.User.FindFirstValue(ClaimTypes.NameIdentifier) 
            && order.Status != StatusHelper.GetStatus(StatusEnum.Cancelled));

            var comments = await _context.Comments.Where(c=> c.TourId == id).Include(c => c.User).ToListAsync();

            ViewData["Booked"] = isBooked;
            ViewData["Comments"] = comments;

            return View(tour);
        }

        // POST: Tour/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "guide,admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour != null)
            {
                _context.Tours.Remove(tour);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TourExists(int id)
        {
            return _context.Tours.Any(e => e.TourId == id);
        }
    }
}
