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
    public class CommentController : Controller
    {
        private readonly TourismDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public CommentController(TourismDbContext context, SignInManager<User> signInManager, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Comment
        public async Task<IActionResult> Index()
        {
            var tourismDbContext = _context.Comments.Include(c => c.ParentComment).Include(c => c.Tour).Include(c => c.User);
            return View(await tourismDbContext.ToListAsync());
        }

        // GET: Comment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.ParentComment)
                .Include(c => c.Tour)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comment/Create
        public IActionResult Create()
        {
            ViewData["ParentCommentId"] = new SelectList(_context.Comments, "CommentId", "CommentId");
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName");
            return View();
        }

        // POST: Comment/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CommentId,UserId,TourId,Text,ParentCommentId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tour", new{id = comment.TourId});
            }
            ViewData["ParentCommentId"] = new SelectList(_context.Comments, "CommentId", "CommentId", comment.ParentCommentId);
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourId", comment.TourId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName", comment.UserId);
            return View(comment);
        }

        // GET: Comment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            var CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var CurrentUser = await _userManager.FindByIdAsync(CurrentUserId);
            if(Convert.ToString(comment.UserId) != CurrentUserId  && !await _userManager.IsInRoleAsync(CurrentUser, "admin") && !await _userManager.IsInRoleAsync(CurrentUser, "guide"))
            {
                return RedirectToAction("Index", "AccessDenied");
            }
            ViewData["ParentCommentId"] = new SelectList(_context.Comments, "CommentId", "CommentId", comment.ParentCommentId);
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourId", comment.TourId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName", comment.UserId);
            return View(comment);
        }

        // POST: Comment/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CommentId,UserId,TourId,Text,ParentCommentId")] Comment comment)
        {
            if (id != comment.CommentId)
            {
                return NotFound();
            }
            var CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var CurrentUser = await _userManager.FindByIdAsync(CurrentUserId);
            if(Convert.ToString(comment.UserId) != CurrentUserId  && !await _userManager.IsInRoleAsync(CurrentUser, "admin") && !await _userManager.IsInRoleAsync(CurrentUser, "guide"))
            {
                return RedirectToAction("Index", "AccessDenied");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.CommentId))
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
            ViewData["ParentCommentId"] = new SelectList(_context.Comments, "CommentId", "CommentId", comment.ParentCommentId);
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "TourId", comment.TourId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserName", comment.UserId);
            return View(comment);
        }

        [HttpPost]
        public async Task<IActionResult> EditOnTourPage([FromForm] int CommentId, [FromForm] string CommentText)
        {
            var comment = _context.Comments.FirstOrDefault(com => com.CommentId == CommentId);
            if (comment == null)
            {
                return Json(new{ success = false});
            }
            var CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var CurrentUser = await _userManager.FindByIdAsync(CurrentUserId);
            if(Convert.ToString(comment.UserId) != CurrentUserId  && !await _userManager.IsInRoleAsync(CurrentUser, "admin") && !await _userManager.IsInRoleAsync(CurrentUser, "guide"))
            {
                return Json(new{ success = false});
            }

            comment.Text = CommentText;
            _context.Update(comment);
            await _context.SaveChangesAsync();
            return Json(new {success = true});
        }

        // GET: Comment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.ParentComment)
                .Include(c => c.Tour)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CommentId == id);
            if (comment == null)
            {
                return NotFound();
            }
            var CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var CurrentUser = await _userManager.FindByIdAsync(CurrentUserId);
            if(Convert.ToString(comment.UserId) != CurrentUserId  && !await _userManager.IsInRoleAsync(CurrentUser, "admin") && !await _userManager.IsInRoleAsync(CurrentUser, "guide"))
            {
                return RedirectToAction("Index", "AccessDenied");
            }

            return View(comment);
        }

        // POST: Comment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment != null)
            {
                var CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var CurrentUser = await _userManager.FindByIdAsync(CurrentUserId);
                if(Convert.ToString(comment.UserId) != CurrentUserId  && !await _userManager.IsInRoleAsync(CurrentUser, "admin") && !await _userManager.IsInRoleAsync(CurrentUser, "guide"))
                {
                    return RedirectToAction("Index", "AccessDenied");
                }
                var TourId = comment.TourId;
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Tour", new {id = TourId});
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}
