using System;
using System.Collections.Generic;

namespace VenueDBApp.Data;

public partial class BookingSummary
{
    public int BookingId { get; set; }

    public int EventId { get; set; }

    public int VenueId { get; set; }

    public DateTime BookingDate { get; set; }

    public string EventName { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public string? Description { get; set; }

    public string VenueName { get; set; } = null!;

    public string? Location { get; set; }

    public int? Capacity { get; set; }

    public string? ImageUrl { get; set; }
}
