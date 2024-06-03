using System;
using System.IO;
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

            var tourList = await tourismDbContext.ToListAsync();
            tourList.Reverse();
            var Categories = new SelectList(_context.Categories, "CategoryId", "Name", categoryId).ToList();
            Categories.Insert(0, new SelectListItem() { Value = "null", Text = "Оберіть категорію" });
            ViewData["CategoryId"] = new SelectList(Categories, "Value", "Text", categoryId);
            return View(tourList);
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
            ViewData["Photos"] = await _context.Photos.Where(p => p.TourId == id).ToListAsync();
            ViewData["Booked"] = isBooked;
            ViewData["Comments"] = comments;

            return View(tour);
        }

        // GET: Tour/Create
        [Authorize(Roles = "guide,admin")]
        public async Task<IActionResult> CreateAsync()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewData["CityId"] = new SelectList(_context.Cities, "CityId", "Name");
            var adminUsers = await _userManager.GetUsersInRoleAsync("guide");
            ViewData["Guides"] = new MultiSelectList(adminUsers, "Id", "UserName");
            return View();
        }

        // POST: Tour/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "guide,admin")]
        public async Task<IActionResult> Create([Bind("TourId,CityId,Info,MainPhoto,Price,StartDate,EndDate,Capacity,CategoryId,StartPointName,StartPointGeo,Name,Guides")] Tour tour, [FromForm] IFormFile? MainPhotoFile, List<IFormFile> Photos, List<string> UploadedPhotos)
        {

            foreach (var photoBase64 in UploadedPhotos)
            {
                var photoBytes = Convert.FromBase64String(photoBase64);
                var stream = new MemoryStream(photoBytes);
                IFormFile photo = new FormFile(stream, 0, photoBytes.Length, Guid.NewGuid().ToString(), Guid.NewGuid().ToString()+".jpg");
                Photos.Add(photo);
            }
            if (ModelState.IsValid)
            {
                if(MainPhotoFile != null)
                {
                    string folder = "Tours/MainPhotos/";
                    string FileNameWithoutSpaces = string.Join("", MainPhotoFile.FileName.Split(" ", StringSplitOptions.RemoveEmptyEntries));
                    folder +=  Guid.NewGuid().ToString() + "_" + FileNameWithoutSpaces;
                    string ServerFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                    await MainPhotoFile.CopyToAsync(new FileStream(ServerFolder, FileMode.Create));
                    tour.MainPhoto = "/"+folder;
                }
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

                foreach(var photo in Photos)
                {
                    string folder = $"Tours/{tour.TourId}/";
                    string FileNameWithoutSpaces = string.Join("", photo.FileName.Split(" ", StringSplitOptions.RemoveEmptyEntries));
                    folder +=  Guid.NewGuid().ToString() + "_" + FileNameWithoutSpaces;
                    string ServerFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                    Directory.CreateDirectory(Path.GetDirectoryName(ServerFolder));
                    await photo.CopyToAsync(new FileStream(ServerFolder, FileMode.Create));
                    string path = "/"+folder;
                    var ph = new Photo{
                        TourId = tour.TourId,
                        Path = path,
                        Tour = tour,
                    };
                    _context.Add(ph);
                }
                tour.Info = tour.Info?.Replace("\n", "<br / >");
                _context.Add(tour);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UploadedPhotos"] = Photos.Select(p => 
            {
                using (var ms = new MemoryStream())
                {
                    p.CopyTo(ms);
                    return ms.ToArray();
                }
            }).ToList();
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", tour.CategoryId);
            ViewData["CityId"] = new SelectList(_context.Cities, "CityId", "Name", tour.CityId);
            var adminUsers = await _userManager.GetUsersInRoleAsync("guide");
            ViewData["Guides"] = new MultiSelectList(adminUsers, "Id", "UserName");
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
            var Photos = await _context.Photos.Where(p => p.TourId == id).ToListAsync();
            ViewData["UploadedPhotos"] = Photos.Select(p =>
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, p.Path.TrimStart('/'));
                byte[] fileBytes;
                using (var fileStream = System.IO.File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                }
                return fileBytes;
            }).ToList();
            tour.Info = tour.Info?.Replace("<br / >", "\n");
            
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", tour.CategoryId);
            ViewData["CityId"] = new SelectList(_context.Cities, "CityId", "Name", tour.CityId);
            tour.Guides = _context.GuideTours.Where(gd => gd.TourId == id).Select(gd => gd.GuideId).ToList();
            var adminUsers = await _userManager.GetUsersInRoleAsync("guide");
            ViewData["Guides"] = new MultiSelectList(adminUsers, "Id", "UserName");
            return View(tour);
        }

        // POST: Tour/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "guide,admin")]
        public async Task<IActionResult> Edit(int id, [Bind("TourId,CityId,Info,MainPhoto,Price,StartDate,EndDate,Capacity,AvaibleTickets,CategoryId,StartPointName,StartPointGeo,Name, Guides")] Tour tour, [FromForm] IFormFile? MainPhotoFile, List<IFormFile> Photos, List<string> UploadedPhotos)
        {
            if (id != tour.TourId)
            {
                return NotFound();
            }
            List<IFormFile>AllPhotos = new List<IFormFile>();

            foreach (var photoBase64 in UploadedPhotos)
            {
                var photoBytes = Convert.FromBase64String(photoBase64);
                var stream = new MemoryStream(photoBytes);
                IFormFile photo = new FormFile(stream, 0, photoBytes.Length, Guid.NewGuid().ToString(), Guid.NewGuid().ToString()+".jpg");
                AllPhotos.Add(photo);
            }
            foreach(var photo in Photos)
            {
                AllPhotos.Add(photo);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if(MainPhotoFile != null)
                    {
                        string folder = "Tours/MainPhotos/";
                        string FileNameWithoutSpaces = string.Join("", MainPhotoFile.FileName.Split(" ", StringSplitOptions.RemoveEmptyEntries));
                        folder +=  Guid.NewGuid().ToString() + "_" + FileNameWithoutSpaces;
                        string ServerFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                        await MainPhotoFile.CopyToAsync(new FileStream(ServerFolder, FileMode.Create));
                        tour.MainPhoto = "/"+folder;
                    }
                    var PhotoList = await _context.Photos.Where(ph => ph.TourId == id).ToListAsync();

                    foreach(var photo in PhotoList)
                    {
                        _context.Remove(photo);
                    }
                    foreach(var photo in AllPhotos)
                    {
                        string folder = $"Tours/{tour.TourId}/";
                        string FileNameWithoutSpaces = string.Join("", photo.FileName.Split(" ", StringSplitOptions.RemoveEmptyEntries));
                        folder +=  Guid.NewGuid().ToString() + "_" + FileNameWithoutSpaces;
                        string ServerFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                        Directory.CreateDirectory(Path.GetDirectoryName(ServerFolder));
                        await photo.CopyToAsync(new FileStream(ServerFolder, FileMode.Create));
                        string path = "/"+folder;
                        var ph = new Photo{
                            TourId = tour.TourId,
                            Path = path,
                        };
                        _context.Add(ph);
                    }

                    var GuideList = await _context.GuideTours.Where(gd => gd.TourId == id).ToListAsync();
                    foreach(var gd in GuideList)
                    {
                        _context.Remove(gd);
                    }
                    foreach(var guide in tour.Guides)
                    {
                        var gd = new GuideTour{TourId = id, GuideId = guide};
                        _context.Add(gd);
                    }
                    tour.Info = tour.Info?.Replace("\n", "<br / >");
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
            ViewData["UploadedPhotos"] = AllPhotos.Select(p => 
            {
                using (var ms = new MemoryStream())
                {
                    p.CopyTo(ms);
                    return ms.ToArray();
                }
            }).ToList();
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", tour.CategoryId);
            ViewData["CityId"] = new SelectList(_context.Cities, "CityId", "Name", tour.CityId);
            tour.Guides = _context.GuideTours.Where(gd => gd.TourId == id).Select(gd => gd.GuideId).ToList();
            var adminUsers = await _userManager.GetUsersInRoleAsync("guide");
            ViewData["Guides"] = new MultiSelectList(adminUsers, "Id", "UserName");
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
