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

namespace Tourism.Controllers_
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly TourismDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(TourismDbContext context, SignInManager<User> signInManager, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: User
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var Orders = await _context.Orders.Where(order => order.UserId == id).Include(order => order.Guide).Include(order => order.Tour).ToListAsync();
            Orders.Reverse();
            ViewData["Orders"] = Orders;

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("Id,UserName,Phone,Email,Info,ProfilePhoto, ProfilePic")] User user)
        {
            if (ModelState.IsValid)
            {
                if(user.ProfilePic != null)
                {
                    string folder = "Users/ProfilePics/";
                    string FileNameWithoutSpaces = string.Join("", user.ProfilePic.FileName.Split(" ", StringSplitOptions.RemoveEmptyEntries));
                    folder +=  Guid.NewGuid().ToString() + "_" + FileNameWithoutSpaces;
                    string ServerFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                    await user.ProfilePic.CopyToAsync(new FileStream(ServerFolder, FileMode.Create));
                    user.ProfilePhoto = "/"+folder;
                }
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var CurrentUser = await _userManager.FindByIdAsync(CurrentUserId);
            if(Convert.ToString(id) != CurrentUserId && !await _userManager.IsInRoleAsync(CurrentUser, "admin"))
            {
                return RedirectToAction("Index", "AccessDenied");
            }
            ViewData["IsGuide"] = await _userManager.IsInRoleAsync(user, "guide");
            ViewData["IsAdmin"] = await _userManager.IsInRoleAsync(user, "admin");
            return View(user);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserName,PhoneNumber,Email,Info,ProfilePhoto, ProfilePic")] User user, bool isGuide)
        {
            if (id != user.Id)
            {
                return NotFound();
            }
            var CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var CurrentUser = await _userManager.FindByIdAsync(CurrentUserId);
            if(Convert.ToString(id) != CurrentUserId && !await _userManager.IsInRoleAsync(CurrentUser, "admin"))
            {
                return RedirectToAction("Index", "AccessDenied");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var UpdatedUser = await _userManager.FindByIdAsync(Convert.ToString(id));
                    if(user.ProfilePic != null)
                    {
                        string folder = "Users/ProfilePics/";
                        string FileNameWithoutSpaces = string.Join("", user.ProfilePic.FileName.Split(" ", StringSplitOptions.RemoveEmptyEntries));
                        folder +=  Guid.NewGuid().ToString() + "_" + FileNameWithoutSpaces;
                        string ServerFolder = Path.Combine(_webHostEnvironment.WebRootPath, folder);
                        await user.ProfilePic.CopyToAsync(new FileStream(ServerFolder, FileMode.Create));
                        user.ProfilePhoto = "/"+folder;
                    }
                    UpdatedUser.UserName = user.UserName;
                    UpdatedUser.ProfilePhoto = user.ProfilePhoto;
                    UpdatedUser.PhoneNumber = user.PhoneNumber;
                    UpdatedUser.Info = user.Info;
                    UpdatedUser.Email = user.Email;

                    await _userManager.UpdateAsync(UpdatedUser);
                    if(isGuide && !await _userManager.IsInRoleAsync(UpdatedUser, "guide"))
                    {
                        await _userManager.AddToRoleAsync(UpdatedUser, "guide");
                    } else
                    if(!isGuide && await _userManager.IsInRoleAsync(UpdatedUser, "guide"))
                    {
                        await _userManager.RemoveFromRoleAsync(UpdatedUser, "guide");
                    }
                    await _userManager.UpdateAsync(UpdatedUser);
                    // _context.Update(user);
                    // await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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
            ViewData["IsGuide"] = await _userManager.IsInRoleAsync(user, "guide");
            ViewData["IsAdmin"] = await _userManager.IsInRoleAsync(user, "admin");
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var CurrentUser = await _userManager.FindByIdAsync(CurrentUserId);
            if(Convert.ToString(id) != CurrentUserId && !await _userManager.IsInRoleAsync(CurrentUser, "admin"))
            {
                return RedirectToAction("Index", "AccessDenied");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            var CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var CurrentUser = await _userManager.FindByIdAsync(CurrentUserId);
            if(Convert.ToString(id) != CurrentUserId && !await _userManager.IsInRoleAsync(CurrentUser, "admin"))
            {
                return RedirectToAction("Index", "AccessDenied");
            }
            if (user != null)
            {
                var orders = _context.Orders.Where(order => order.GuideId == id).ToList();
                foreach(var order in orders)
                {
                    order.GuideId = null;
                    _context.Orders.Update(order);
                }
                if(Convert.ToString(user.Id) == this.User.FindFirstValue(ClaimTypes.NameIdentifier))
                {
                    await _signInManager.SignOutAsync();
                }
                // var filePath = Path.Combine(_webHostEnvironment.WebRootPath, (user.ProfilePhoto ?? "1")[1..]);
                // user.ProfilePhoto=null;
                
                // if (System.IO.File.Exists(filePath))
                // {
                //     System.IO.File.Delete(filePath);
                // }
                await _userManager.DeleteAsync(user);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
