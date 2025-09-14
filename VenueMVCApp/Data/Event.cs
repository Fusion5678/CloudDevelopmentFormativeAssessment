using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VenueDBApp.Data;

public partial class Event
{
    public int EventId { get; set; }
    [Required(ErrorMessage = "Event name field is required.")]
    public string EventName { get; set; } = null!;

    [Required(ErrorMessage = "Event date field is required.")]

    public DateTime EventDate { get; set; }

    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Venue is required.")]
    public int VenueId { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Venue? Venue { get; set; }
}
