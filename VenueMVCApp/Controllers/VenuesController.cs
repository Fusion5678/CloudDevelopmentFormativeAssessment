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
        public async Task<IActionResult> Index()
        {
            var venues = await _context.Venues.ToListAsync();
            return View(venues);
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
                    ModelState.AddModelError(string.Empty, $"Error uploading image: {ex.Message}");
                    return View(venue);
                }
            }

            _context.Add(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload if a new file is provided
                    if (imageFile is { Length: > 0 })
                    {
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
                            ModelState.AddModelError(string.Empty, $"Error uploading image: {ex.Message}");
                            return View(venue);
                        }
                    }
                    // If no new image is uploaded, keep the existing ImageUrl

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
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
            return View(venue);
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