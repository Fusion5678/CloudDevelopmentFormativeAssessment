using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VenueDBApp.Data;

public partial class Venue
{
    public int VenueId { get; set; }
    [Required(ErrorMessage = "Venue name field is required.")]
    public string VenueName { get; set; } = null!;

    public string Location { get; set; }

    public int Capacity { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
