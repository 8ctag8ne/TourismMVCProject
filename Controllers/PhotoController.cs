using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tourism;

namespace Tourism.Controllers_
{
    public class PhotoController : Controller
    {
        private readonly TourismDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public PhotoController(TourismDbContext context, SignInManager<User> signInManager, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Photo
        public async Task<IActionResult> Index()
        {
            var tourismDbContext = _context.Photos.Include(p => p.Tour);
            return View(await tourismDbContext.ToListAsync());
        }

        // GET: Photo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photo = await _context.Photos
                .Include(p => p.Tour)
                .FirstOrDefaultAsync(m => m.PhotoId == id);
            if (photo == null)
            {
                return NotFound();
            }

            return View(photo);
        }

        // GET: Photo/Create
        public IActionResult Create()
        {
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "Name");
            return View();
        }

        // POST: Photo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PhotoId,TourId,Path")] Photo photo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(photo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "Name", photo.TourId);
            return View(photo);
        }

        // GET: Photo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photo = await _context.Photos.FindAsync(id);
            if (photo == null)
            {
                return NotFound();
            }
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "Name", photo.TourId);
            return View(photo);
        }

        // POST: Photo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PhotoId,TourId,Path")] Photo photo)
        {
            if (id != photo.PhotoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(photo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhotoExists(photo.PhotoId))
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
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "Name", photo.TourId);
            return View(photo);
        }

        // GET: Photo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var photo = await _context.Photos
                .Include(p => p.Tour)
                .FirstOrDefaultAsync(m => m.PhotoId == id);
            if (photo == null)
            {
                return NotFound();
            }

            return View(photo);
        }

        // POST: Photo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo != null)
            {
                _context.Photos.Remove(photo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PhotoExists(int id)
        {
            return _context.Photos.Any(e => e.PhotoId == id);
        }
    }
}
