using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Tourism;
using Tourism.Status;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Tourism.Controllers_
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly TourismDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        private readonly IWebHostEnvironment _webHostEnvironment;

        public OrderController(TourismDbContext context, SignInManager<User> signInManager, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Order
        public async Task<IActionResult> Index()
        {
            var tourismDbContext = await _context.Orders.Include(o => o.Guide).Include(o => o.Tour).Include(o => o.User).ToListAsync();
            tourismDbContext.Reverse();
            return View(tourismDbContext);
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Guide)
                .Include(o => o.Tour)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Order/Create
        public IActionResult Create()
        {
            ViewData["GuideId"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["Status"] = new SelectList(new List<string>{"Збережено", "Відправлено", "Опрацьовується", "Прийнято", "Скасовано"});
            return View();
        }

        // POST: Order/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,UserId,TourId,GuideId,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GuideId"] = new SelectList(_context.Users, "Id", "UserName", order.GuideId);
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "Name", order.TourId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", order.UserId);
            ViewData["Status"] = new SelectList(new List<string>{"Збережено", "Відправлено", "Опрацьовується", "Прийнято", "Скасовано"});
            return View(order);
        }

        // GET: Order/Edit/5
        [Authorize(Roles = "guide,admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var CurrentUser = await _userManager.FindByIdAsync(CurrentUserId);
            if(Convert.ToString(order.UserId) != CurrentUserId && Convert.ToString(order.GuideId) != CurrentUserId && !await _userManager.IsInRoleAsync(CurrentUser, "admin"))
            {
                return RedirectToAction("Index", "AccessDenied");
            }

            ViewData["GuideId"] = new SelectList(_context.Users, "Id", "UserName", order.GuideId);
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "Name", order.TourId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", order.UserId);
            ViewData["Status"] = new SelectList(new List<string>{"Збережено", "Відправлено", "Опрацьовується", "Прийнято", "Скасовано"});
            return View(order);
        }

        // POST: Order/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "guide,admin")]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,UserId,TourId,GuideId,Status")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }
            var OldOrder = await _context.Orders.FirstOrDefaultAsync(order => order.OrderId == id);
            var CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var CurrentUser = await _userManager.FindByIdAsync(CurrentUserId);
            if(Convert.ToString(OldOrder?.UserId) != CurrentUserId && Convert.ToString(OldOrder?.GuideId) != CurrentUserId && !await _userManager.IsInRoleAsync(CurrentUser, "admin"))
            {
                return RedirectToAction("Index", "AccessDenied");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["GuideId"] = new SelectList(_context.Users, "Id", "UserName", order.GuideId);
            ViewData["TourId"] = new SelectList(_context.Tours, "TourId", "Name", order.TourId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", order.UserId);
            ViewData["Status"] = new SelectList(new List<string>{"Збережено", "Відправлено", "Опрацьовується", "Прийнято", "Скасовано"});
            return View(order);
        }

        // GET: Order/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Guide)
                .Include(o => o.Tour)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }
            var CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var CurrentUser = await _userManager.FindByIdAsync(CurrentUserId);
            if(Convert.ToString(order.UserId) != CurrentUserId && Convert.ToString(order.GuideId) != CurrentUserId && !await _userManager.IsInRoleAsync(CurrentUser, "admin"))
            {
                return RedirectToAction("Index", "AccessDenied");
            }

            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "guide,admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            var CurrentUserId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var CurrentUser = await _userManager.FindByIdAsync(CurrentUserId);
            if(Convert.ToString(order?.UserId) != CurrentUserId && Convert.ToString(order?.GuideId) != CurrentUserId && !await _userManager.IsInRoleAsync(CurrentUser, "admin"))
            {
                return RedirectToAction("Index", "AccessDenied");
            }

            if (order != null)
            {
                if(order.Status != StatusHelper.GetStatus(StatusEnum.Cancelled))
                {
                    var tour = await _context.Tours.FirstOrDefaultAsync(tour => tour.TourId == order.TourId);
                    tour.AvaibleTickets++;
                }
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> PostOrder(int TourId, int UserId)
        {
            var tour = await _context.Tours.FirstOrDefaultAsync(t => t.TourId == TourId);

            if (tour == null)
            {
                return Json(new { success = false });
            }

            if(_context.Orders.Any(order => order.UserId == UserId && order.TourId == TourId && order.Status != StatusHelper.GetStatus(StatusEnum.Cancelled)))
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == UserId && o.TourId == TourId && o.Status != StatusHelper.GetStatus(StatusEnum.Cancelled));
                tour.AvaibleTickets++;
                _context.Orders.Remove(order);
            } else
            {
                var order = new Order();
                order.UserId = UserId;
                order.GuideId = null; 
                order.TourId = TourId; 
                order.Status = StatusHelper.GetStatus(StatusEnum.Posted);

                if (tour.AvaibleTickets <= 0)
                {
                    return Json(new { success = false });
                }

                tour.AvaibleTickets--;

                _context.Orders.Add(order);
            }

            _context.Tours.Update(tour);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int OrderId, int UserId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(t=>t.OrderId == OrderId);
            if(order == null)
            {
                return Json(new { success = false });
            }

            var tour = await _context.Tours.FirstOrDefaultAsync(t=>t.TourId == order.TourId);
            if(tour == null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                return Json(new { success = false });
            }

            if(order.GuideId != UserId && order.UserId != UserId)
            {
                return Json(new { success = false });
            }
            tour.AvaibleTickets++;
            order.Status = StatusHelper.GetStatus(StatusEnum.Cancelled);

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Roles ="guide,admin")]
        public async Task<IActionResult> TakeOrder(int OrderId, int GuideId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(t=>t.OrderId == OrderId);
            if(order == null)
            {
                return Json(new { success = false });
            }
            if(order.Status != StatusHelper.GetStatus(StatusEnum.Posted))
            {
                return Json(new { success = false });
            }
            order.GuideId = GuideId;
            order.Status = StatusHelper.GetStatus(StatusEnum.InReview);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Roles ="guide,admin")]
        public async Task<IActionResult> AcceptOrder(int OrderId, int GuideId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(t=>t.OrderId == OrderId);
            if(order == null)
            {
                return Json(new { success = false });
            }
            if(order.Status != StatusHelper.GetStatus(StatusEnum.InReview) || order.GuideId != GuideId)
            {
                return Json(new { success = false });
            }
            order.Status = StatusHelper.GetStatus(StatusEnum.Accepted);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
