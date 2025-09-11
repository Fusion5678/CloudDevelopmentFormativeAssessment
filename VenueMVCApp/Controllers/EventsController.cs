using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VenueDBApp.Data;

namespace VenueDBApp.Controllers
{
    public class EventsController : Controller
    {
        private readonly VenueDbContext _context;

        public EventsController(VenueDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index(string searchString)
        {
            var events = _context.Events
                .Include(e => e.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // Search by event ID (exact match), event name (contains), description (contains), or venue name (contains)
                if (int.TryParse(searchString, out int eventId))
                {
                    events = events.Where(e => 
                        e.EventId == eventId || 
                        e.EventName.Contains(searchString) ||
                        (e.Description != null && e.Description.Contains(searchString)) ||
                        (e.Venue != null && e.Venue.VenueName.Contains(searchString)));
                }
                else
                {
                    events = events.Where(e => 
                        e.EventName.Contains(searchString) ||
                        (e.Description != null && e.Description.Contains(searchString)) ||
                        (e.Venue != null && e.Venue.VenueName.Contains(searchString)));
                }
            }

            var results = await events.ToListAsync();
            ViewData["SearchString"] = searchString;
            return View(results);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            var venues = _context.Venues.ToList();
            
            if (!venues.Any())
            {
                TempData["ErrorMessage"] = "No venues available. Please create some venues first.";
                return RedirectToAction("Index", "Venues");
            }
            
            ViewData["VenueID"] = new SelectList(venues, "VenueId", "VenueName");
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventName,EventDate,Description,VenueId")] Event @event)
        {
            // Additional validation
            if (string.IsNullOrWhiteSpace(@event.EventName))
            {
                ModelState.AddModelError("EventName", "Event name is required.");
            }
            else if (@event.EventName.Length > 100)
            {
                ModelState.AddModelError("EventName", "Event name cannot exceed 100 characters.");
            }
            
            if (@event.EventDate < DateTime.Today)
            {
                ModelState.AddModelError("EventDate", "Event date cannot be in the past.");
            }
            
            if (@event.VenueId == null || @event.VenueId == 0)
            {
                ModelState.AddModelError("VenueId", "Please select a venue.");
            }
            else
            {
                // Check if venue exists
                var venueExists = await _context.Venues.AnyAsync(v => v.VenueId == @event.VenueId);
                if (!venueExists)
                {
                    ModelState.AddModelError("VenueId", "Selected venue does not exist.");
                }
            }
            
            if (!string.IsNullOrWhiteSpace(@event.Description) && @event.Description.Length > 500)
            {
                ModelState.AddModelError("Description", "Description cannot exceed 500 characters.");
            }
            
            if (!ModelState.IsValid)
            {
                ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
                return View(@event);
            }
            
            try
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating event: {ex.Message}";
                ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
                return View(@event);
            }
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,Description,VenueId")] Event @event)
        {
            if (id != @event.EventId)
            {
                return NotFound();
            }

            // Additional validation
            if (string.IsNullOrWhiteSpace(@event.EventName))
            {
                ModelState.AddModelError("EventName", "Event name is required.");
            }
            else if (@event.EventName.Length > 100)
            {
                ModelState.AddModelError("EventName", "Event name cannot exceed 100 characters.");
            }
            
            if (@event.EventDate < DateTime.Today)
            {
                ModelState.AddModelError("EventDate", "Event date cannot be in the past.");
            }
            
            if (@event.VenueId == null || @event.VenueId == 0)
            {
                ModelState.AddModelError("VenueId", "Please select a venue.");
            }
            else
            {
                // Check if venue exists
                var venueExists = await _context.Venues.AnyAsync(v => v.VenueId == @event.VenueId);
                if (!venueExists)
                {
                    ModelState.AddModelError("VenueId", "Selected venue does not exist.");
                }
            }
            
            if (!string.IsNullOrWhiteSpace(@event.Description) && @event.Description.Length > 500)
            {
                ModelState.AddModelError("Description", "Description cannot exceed 500 characters.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
                return View(@event);
            }
            
            try
            {
                _context.Update(@event);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Event updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(@event.EventId))
                {
                    return NotFound();
                }
                else
                {
                    TempData["ErrorMessage"] = "The event was modified by another user. Please refresh and try again.";
                    ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
                    return View(@event);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating event: {ex.Message}";
                ViewData["VenueID"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
                return View(@event);
            }
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }
        
        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                try
                {
                    _context.Events.Remove(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event deleted successfully.";
                }
                catch (DbUpdateException ex)
                {
                    TempData["ErrorMessage"] = "Cannot delete this event. It may have associated bookings. Please delete all related bookings first.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
} 