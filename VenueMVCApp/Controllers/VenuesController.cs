using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VenueDBApp.Data;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace VenueDBApp.Controllers
{
    public class VenuesController : Controller
    {
        private readonly VenueDbContext _context;
        private readonly IConfiguration _configuration;

        public VenuesController(VenueDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        // GET: Venues
        public async Task<IActionResult> Index(string searchString)
        {
            var venues = _context.Venues.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // Search by venue ID (exact match), venue name (contains), location (contains), or capacity (exact match)
                if (int.TryParse(searchString, out int venueId))
                {
                    venues = venues.Where(v => 
                        v.VenueId == venueId || 
                        v.VenueName.Contains(searchString) ||
                        (v.Location != null && v.Location.Contains(searchString)) ||
                        v.Capacity == venueId);
                }
                else
                {
                    venues = venues.Where(v => 
                        v.VenueName.Contains(searchString) ||
                        (v.Location != null && v.Location.Contains(searchString)));
                }
            }

            var results = await venues.ToListAsync();
            ViewData["SearchString"] = searchString;
            return View(results);
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues
                .Include(v => v.Events)
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null) return NotFound();

            return View(venue);
        }
        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueName,Location,Capacity")] Venue venue, IFormFile? imageFile)
        {
            // Additional validation
            if (string.IsNullOrWhiteSpace(venue.VenueName))
            {
                ModelState.AddModelError("VenueName", "Venue name is required.");
            }
            else if (venue.VenueName.Length > 100)
            {
                ModelState.AddModelError("VenueName", "Venue name cannot exceed 100 characters.");
            }
            
            if (!string.IsNullOrWhiteSpace(venue.Location) && venue.Location.Length > 200)
            {
                ModelState.AddModelError("Location", "Location cannot exceed 200 characters.");
            }
            
            if (venue.Capacity.HasValue && venue.Capacity <= 0)
            {
                ModelState.AddModelError("Capacity", "Capacity must be greater than 0.");
            }
            else if (venue.Capacity.HasValue && venue.Capacity > 100000)
            {
                ModelState.AddModelError("Capacity", "Capacity cannot exceed 100,000.");
            }
            
            // Image file validation
            if (imageFile != null && imageFile.Length > 0)
            {
                // Check file size (5MB limit)
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("imageFile", "Image file size cannot exceed 5MB.");
                }
                
                // Check file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
                {
                    ModelState.AddModelError("imageFile", "Only JPEG, PNG, and GIF images are allowed.");
                }
            }

            if (!ModelState.IsValid) return View(venue);

            if (imageFile is { Length: > 0 })
            {
                try
                {
                    var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
                    var containerName = _configuration["AzureStorage:ContainerName"];

                    var blobServiceClient = new BlobServiceClient(connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                    // Public container
                    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                    var fileName = $"{Guid.NewGuid()}-{Path.GetFileName(imageFile.FileName)}";
                    var blobClient = containerClient.GetBlobClient(fileName);

                    using var stream = imageFile.OpenReadStream();
                    await blobClient.UploadAsync(stream, new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = imageFile.ContentType
                        }
                    });

                    // Store full public URL in DB
                    venue.ImageUrl = blobClient.Uri.ToString();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("imageFile", $"Error uploading image: {ex.Message}");
                    return View(venue);
                }
            }

            try
            {
                _context.Add(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venue created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating venue: {ex.Message}";
                return View(venue);
            }
        }



        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }

         

            return View(venue);
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity,ImageUrl")] Venue venue, IFormFile? imageFile)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            // Additional validation
            if (string.IsNullOrWhiteSpace(venue.VenueName))
            {
                ModelState.AddModelError("VenueName", "Venue name is required.");
            }
            else if (venue.VenueName.Length > 100)
            {
                ModelState.AddModelError("VenueName", "Venue name cannot exceed 100 characters.");
            }
            
            if (!string.IsNullOrWhiteSpace(venue.Location) && venue.Location.Length > 200)
            {
                ModelState.AddModelError("Location", "Location cannot exceed 200 characters.");
            }
            
            if (venue.Capacity.HasValue && venue.Capacity <= 0)
            {
                ModelState.AddModelError("Capacity", "Capacity must be greater than 0.");
            }
            else if (venue.Capacity.HasValue && venue.Capacity > 100000)
            {
                ModelState.AddModelError("Capacity", "Capacity cannot exceed 100,000.");
            }
            
            // Image file validation
            if (imageFile != null && imageFile.Length > 0)
            {
                // Check file size (5MB limit)
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("imageFile", "Image file size cannot exceed 5MB.");
                }
                
                // Check file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
                {
                    ModelState.AddModelError("imageFile", "Only JPEG, PNG, and GIF images are allowed.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(venue);
            }
            
            try
            {
                // Handle image upload if a new file is provided
                if (imageFile is { Length: > 0 })
                {
                    // Delete old image if exists
                    var existingVenue = await _context.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.VenueId == id);
                    if (existingVenue != null && !string.IsNullOrEmpty(existingVenue.ImageUrl))
                    {
                        var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
                        var containerName = _configuration["AzureStorage:ContainerName"];
                        var blobServiceClient = new BlobServiceClient(connectionString);
                        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                        var blobName = Path.GetFileName(new Uri(existingVenue.ImageUrl).AbsolutePath);
                        var blobClient = containerClient.GetBlobClient(blobName);
                        await blobClient.DeleteIfExistsAsync();
                    }
                    try
                    {
                        var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
                        var containerName = _configuration["AzureStorage:ContainerName"];

                        var blobServiceClient = new BlobServiceClient(connectionString);
                        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                        // Ensure container exists and is public
                        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                        var fileName = $"{Guid.NewGuid()}-{Path.GetFileName(imageFile.FileName)}";
                        var blobClient = containerClient.GetBlobClient(fileName);

                        using var stream = imageFile.OpenReadStream();
                        await blobClient.UploadAsync(stream, new BlobUploadOptions
                        {
                            HttpHeaders = new BlobHttpHeaders
                            {
                                ContentType = imageFile.ContentType
                            }
                        });

                        // Update the ImageUrl with the new blob URL
                        venue.ImageUrl = blobClient.Uri.ToString();
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("imageFile", $"Error uploading image: {ex.Message}");
                        return View(venue);
                    }
                }
                else
                {
                    // No new image uploaded, preserve existing image URL
                    var existingVenue = await _context.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.VenueId == id);
                    if (existingVenue != null)
                    {
                        venue.ImageUrl = existingVenue.ImageUrl;
                    }
                }

                // If no new image is uploaded, keep the existing ImageUrl

                _context.Update(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venue updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VenueExists(venue.VenueId))
                {
                    return NotFound();
                }
                else
                {
                    TempData["ErrorMessage"] = "The venue was modified by another user. Please refresh and try again.";
                    return View(venue);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating venue: {ex.Message}";
                return View(venue);
            }
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue != null)
            {
                try
                {
                    // Delete blob image if exists  
                    if (!string.IsNullOrEmpty(venue.ImageUrl))
                    {
                        var connectionString = _configuration.GetConnectionString("AzureBlobStorage");
                        var containerName = _configuration["AzureStorage:ContainerName"];   
                        var blobServiceClient = new BlobServiceClient(connectionString);
                        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                        var blobName = Path.GetFileName(new Uri(venue.ImageUrl).AbsolutePath);
                        var blobClient = containerClient.GetBlobClient(blobName);
                        await blobClient.DeleteIfExistsAsync();
                    }
                    _context.Venues.Remove(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Venue deleted successfully.";
                }
                catch (DbUpdateException ex)
                {
                    TempData["ErrorMessage"] = "Cannot delete this venue. It may have associated events or bookings. Please delete all related events and bookings first.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}