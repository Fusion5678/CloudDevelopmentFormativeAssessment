-- Create the database
CREATE DATABASE VenueDB;
GO

-- Switch to the new database
USE VenueDB;
GO

-- Create Venue table
CREATE TABLE Venue (
    VenueID INT IDENTITY(1,1) PRIMARY KEY,
    VenueName NVARCHAR(100) NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Capacity INT NOT NULL,
    ImageUrl NVARCHAR(255) NULL
);

-- Create Event table
CREATE TABLE Event (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    EventName NVARCHAR(100) NOT NULL,
    EventDate DATE NOT NULL,
    Description NVARCHAR(MAX) NULL,
    VenueID INT NOT NULL,
    CONSTRAINT FK_Event_Venue FOREIGN KEY (VenueID) REFERENCES Venue(VenueID)
);

-- Create Booking table
CREATE TABLE Booking (
    BookingID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    VenueID INT NOT NULL,
    BookingDate DATE NOT NULL,
    CONSTRAINT FK_Booking_Event FOREIGN KEY (EventID) REFERENCES Event(EventID),
    CONSTRAINT FK_Booking_Venue FOREIGN KEY (VenueID) REFERENCES Venue(VenueID)
);


Create VIEW BookingSummary AS
SELECT 
    b.BookingID,
    b.eventID,
    b.venueID,
    b.BookingDate,
    e.EventName,
    e.EventDate,
    e.Description,
    v.VenueName,
    v.Location,
    v.Capacity,
    v.ImageUrl
FROM Booking b
INNER JOIN Event e ON b.EventID = e.EventID
INNER JOIN Venue v ON b.VenueID = v.VenueID;

