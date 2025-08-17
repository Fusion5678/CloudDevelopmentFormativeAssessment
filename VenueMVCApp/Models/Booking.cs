using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VenueDBApp.Models
{
 
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }
        
        [Required]
        public int EventID { get; set; }
        
        [Required]
        public int VenueID { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }
        
        // Navigation properties
        [ForeignKey("EventID")]
        public virtual Event Event { get; set; } = null!;
        
        [ForeignKey("VenueID")]
        public virtual Venue Venue { get; set; } = null!;
    }
} 