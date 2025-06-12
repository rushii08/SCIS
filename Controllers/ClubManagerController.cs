using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIS.Data;
using SCIS.Models;

namespace SCIS.Controllers
{
    [Authorize(Roles = "ClubPresident")]
    public class ClubManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClubManagerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.FullName = user?.FullName;
            ViewBag.UserRole = "Club Manager";

            var clubs = await _context.Clubs
                .Where(c => c.PresidentId == user.Id)
                .ToListAsync();

            ViewBag.ManagedClubs = clubs.Count;

            var clubIds = clubs.Select(c => c.ClubId).ToList();

            var totalMembers = await _context.Memberships
                .Where(m => clubIds.Contains(m.ClubId))
                .CountAsync();
            ViewBag.TotalMembers = totalMembers;

            var upcomingEvents = await _context.Events
                .Where(e => clubIds.Contains(e.ClubId) && e.EventDate >= DateTime.Now)
                .CountAsync();
            ViewBag.UpcomingEvents = upcomingEvents;

            return View(clubs);
        }

        public async Task<IActionResult> Members(int clubId)
        {
            var members = await _context.Memberships
                .Include(m => m.User)
                .Where(m => m.ClubId == clubId)
                .ToListAsync();
            ViewBag.ClubId = clubId;
            return View(members);
        }

        public async Task<IActionResult> Events(int clubId)
        {
            var events = await _context.Events
                .Where(e => e.ClubId == clubId)
                .OrderBy(e => e.EventDate)
                .ToListAsync();
            ViewBag.ClubId = clubId;
            return View(events);
        }

        public IActionResult CreateEvent(int clubId)
        {
            ViewBag.ClubId = clubId;
            return View(new Event { ClubId = clubId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(Event evt)
        {
            if (ModelState.IsValid)
            {
                _context.Events.Add(evt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ClubId = evt.ClubId;
            return View(evt);
        }

        public IActionResult PostAnnouncement(int clubId)
        {
            ViewBag.ClubId = clubId;
            return View(new Announcement { ClubId = clubId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostAnnouncement(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ClubId = announcement.ClubId;
            return View(announcement);
        }
    }
}
