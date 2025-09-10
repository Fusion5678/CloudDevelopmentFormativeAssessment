using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VenueDBApp.Data;

namespace VenueDBApp.Controllers
{
    public class BookingsController : Controller
    {
        private readonly VenueDbContext _context;

        public BookingsController(VenueDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index(string searchString)
        {
            var bookingSummaries = _context.BookingSummaries.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // Search by booking ID (exact match) or event name (contains)
                if (int.TryParse(searchString, out int bookingId))
                {
                    bookingSummaries = bookingSummaries.Where(b => 
                        b.BookingId == bookingId || 
                        b.EventName.Contains(searchString));
                }
                else
                {
                    bookingSummaries = bookingSummaries.Where(b => b.EventName.Contains(searchString));
                }
            }

            var results = await bookingSummaries.ToListAsync();
            ViewData["SearchString"] = searchString;
            return View(results);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingSummary = await _context.BookingSummaries
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (bookingSummary == null)
            {
                return NotFound();
            }

            return View(bookingSummary);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            var events = _context.Events.ToList();
            var venues = _context.Venues.ToList();
            
            if (!events.Any())
            {
                TempData["ErrorMessage"] = "No events available. Please create some events first.";
                return RedirectToAction("Index", "Events");
            }
            
            if (!venues.Any())
            {
                TempData["ErrorMessage"] = "No venues available. Please create some venues first.";
                return RedirectToAction("Index", "Venues");
            }
            
            ViewData["EventID"] = new SelectList(events, "EventId", "EventName");
            ViewData["VenueID"] = new SelectList(venues, "VenueId", "VenueName");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,VenueId,BookingDate")] Booking booking)
        {
            // Additional validation
            if (booking.EventId == 0)
            {
                ModelState.AddModelError("EventId", "Please select an event.");
            }
            if (booking.VenueId == 0)
            {
                ModelState.AddModelError("VenueId", "Please select a venue.");
            }
            if (booking.BookingDate < DateTime.Today)
            {
                ModelState.AddModelError("BookingDate", "Booking date cannot be in the past.");
            }
            // Double-booking validation
            bool isDoubleBooked = await _context.Bookings.AnyAsync(b => b.VenueId == booking.VenueId && b.BookingDate == booking.BookingDate);
            if (isDoubleBooked)
            {
                ModelState.AddModelError("VenueId", "This venue is already booked for the selected date.");
            }
            if (!ModelState.IsValid)
            {
                ViewData["EventID"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }
            try
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating booking: {ex.Message}";
                ViewData["EventID"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["EventID"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }
            // Double-booking validation (exclude current booking)
            bool isDoubleBooked = await _context.Bookings.AnyAsync(b => b.VenueId == booking.VenueId && b.BookingDate == booking.BookingDate && b.BookingId != booking.BookingId);
            if (isDoubleBooked)
            {
                ModelState.AddModelError("VenueId", "This venue is already booked for the selected date.");
            }
            if (!ModelState.IsValid)
            {
                ViewData["EventID"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }
            try
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.BookingId))
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

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bookingSummary = await _context.BookingSummaries
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (bookingSummary == null)
            {
                return NotFound();
            }

            return View(bookingSummary);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                try
                {
                    _context.Bookings.Remove(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking deleted successfully.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] =$"Error deleting booking : {ex.Message}";
                    return RedirectToAction(nameof(Index));
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}