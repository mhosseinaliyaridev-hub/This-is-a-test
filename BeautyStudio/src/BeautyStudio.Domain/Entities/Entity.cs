namespace BeautyStudio.Domain.Entities;

public abstract class BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Customer : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<Appointment> Appointments { get; set; } = new();
}

public class Service : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<Appointment> Appointments { get; set; } = new();
}

public class Specialist : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public double Rating { get; set; } = 5.0;
    public string? ImageUrl { get; set; }
    public List<string> ServiceIds { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
}

public class Appointment : BaseEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string SpecialistId { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Completed, Cancelled
    public string? Notes { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    
    // Navigation properties (not stored in JSON)
    public Customer? Customer { get; set; }
    public Service? Service { get; set; }
    public Specialist? Specialist { get; set; }
}

public class Payment : BaseEntity
{
    public string AppointmentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Completed, Refunded
    public DateTime? PaidAt { get; set; }
}

public class Review : BaseEntity
{
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; } = 5;
    public string Comment { get; set; } = string.Empty;
    public string? ServiceId { get; set; }
    public string? SpecialistId { get; set; }
}

public class GalleryImage : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
