using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VenueDBApp.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }
        
        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }
        
        public string? Description { get; set; }
        
        public int? VenueID { get; set; }
        
        // Navigation properties
        [ForeignKey("VenueID")]
        public virtual Venue? Venue { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
} 