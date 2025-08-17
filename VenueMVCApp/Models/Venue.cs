using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VenueDBApp.Models
{
    public class Venue
    {
        [Key]
        public int VenueID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string VenueName { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? Location { get; set; }
        
        public int? Capacity { get; set; }
        
        [StringLength(255)]
        public string? ImageUrl { get; set; }
        
        // Navigation properties
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
} 