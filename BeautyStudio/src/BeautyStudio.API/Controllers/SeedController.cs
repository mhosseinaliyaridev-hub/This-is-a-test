using Microsoft.AspNetCore.Mvc;
using BeautyStudio.Domain.Entities;
using BeautyStudio.Domain.Interfaces;

namespace BeautyStudio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly IFileStorageService _storage;
    private readonly ILoggerService _logger;

    public SeedController(IFileStorageService storage, ILoggerService logger)
    {
        _storage = storage;
        _logger = logger;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateMockData([FromBody] MockDataRequest? request = null)
    {
        try
        {
            var customersCount = request?.CustomersCount ?? 100;
            var servicesCount = request?.ServicesCount ?? 50;
            var specialistsCount = request?.SpecialistsCount ?? 20;
            var appointmentsCount = request?.AppointmentsCount ?? 500;
            var reviewsCount = request?.ReviewsCount ?? 100;

            _logger.LogInfo($"Starting mock data generation: {customersCount} customers, {servicesCount} services, {specialistsCount} specialists, {appointmentsCount} appointments, {reviewsCount} reviews");

            // Generate Customers
            var customers = new List<Customer>();
            var firstNames = new[] { "Emma", "Olivia", "Sophia", "Isabella", "Ava", "Mia", "Charlotte", "Amelia", "Harper", "Evelyn", "Mohammad", "Ali", "Reza", "Hassan", "Maryam", "Zahra", "Fatima", "Sarah", "Narges", "Leila" };
            var lastNames = new[] { "Smith", "Johnson", "Brown", "Taylor", "Wilson", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Garcia", "Rodriguez", "Martinez", "Ahmadi", "Hosseini", "Karimi", "Moradi", "Rashidi", "Tehrani" };

            for (int i = 0; i < customersCount; i++)
            {
                var customer = new Customer
                {
                    FirstName = firstNames[new Random().Next(firstNames.Length)],
                    LastName = lastNames[new Random().Next(lastNames.Length)],
                    Email = $"customer{i}@email.com",
                    PhoneNumber = $"+1-555-{new Random().Next(100, 999)}-{new Random().Next(1000, 9999)}",
                    Notes = $"Customer #{i}",
                    IsActive = true
                };
                customers.Add(customer);
            }

            await _storage.WriteFileAsync("Customers.json", customers);
            _logger.LogInfo($"Created {customers.Count} customers");

            // Generate Services
            var services = new List<Service>();
            var serviceCategories = new[] { "Hair", "Skin", "Nails", "Makeup", "Spa", "Massage" };
            var serviceNames = new[]
            {
                ("Hair", new[] { "Haircut", "Hair Coloring", "Highlights", "Blowout", "Updo", "Keratin Treatment", "Hair Extensions", "Perm" }),
                ("Skin", new[] { "Facial", "Chemical Peel", "Microdermabrasion", "Acne Treatment", "Anti-Aging Treatment", "Skin Rejuvenation" }),
                ("Nails", new[] { "Manicure", "Pedicure", "Gel Nails", "Acrylic Nails", "Nail Art", "Nail Repair" }),
                ("Makeup", new[] { "Day Makeup", "Evening Makeup", "Bridal Makeup", "Party Makeup", "Photo Shoot Makeup" }),
                ("Spa", new[] { "Aromatherapy", "Body Scrub", "Body Wrap", "Hydrotherapy", "Reflexology" }),
                ("Massage", new[] { "Swedish Massage", "Deep Tissue", "Hot Stone", "Sports Massage", "Prenatal Massage", "Thai Massage" })
            };

            int serviceIndex = 0;
            foreach (var (category, names) in serviceNames)
            {
                foreach (var name in names)
                {
                    if (serviceIndex >= servicesCount) break;
                    
                    var service = new Service
                    {
                        Name = name,
                        Description = $"Professional {name.ToLower()} service at Beauty Studio",
                        Price = new decimal(new Random().Next(30, 300)),
                        DurationMinutes = new Random().Next(30, 120),
                        Category = category,
                        IsActive = true
                    };
                    services.Add(service);
                    serviceIndex++;
                }
            }

            await _storage.WriteFileAsync("Services.json", services);
            _logger.LogInfo($"Created {services.Count} services");

            // Generate Specialists
            var specialists = new List<Specialist>();
            var titles = new[] { "Senior Stylist", "Master Colorist", "Nail Technician", "Makeup Artist", "Spa Therapist", "Massage Therapist", "Beauty Specialist", "Senior Therapist" };

            for (int i = 0; i < specialistsCount; i++)
            {
                var specialist = new Specialist
                {
                    FirstName = firstNames[new Random().Next(firstNames.Length)],
                    LastName = lastNames[new Random().Next(lastNames.Length)],
                    Title = titles[new Random().Next(titles.Length)],
                    Bio = $"Experienced professional with expertise in beauty services.",
                    ExperienceYears = new Random().Next(1, 15),
                    Rating = Math.Round(new Random().NextDouble() * 2 + 3, 1), // 3.0 to 5.0
                    IsActive = true,
                    ServiceIds = services.Take(new Random().Next(3, 8)).Select(s => s.Id).ToList()
                };
                specialists.Add(specialist);
            }

            await _storage.WriteFileAsync("Specialists.json", specialists);
            _logger.LogInfo($"Created {specialists.Count} specialists");

            // Generate Appointments
            var appointments = new List<Appointment>();
            var statuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };
            var random = new Random();

            for (int i = 0; i < appointmentsCount; i++)
            {
                var service = services[random.Next(services.Count)];
                var specialist = specialists.FirstOrDefault(s => s.ServiceIds.Contains(service.Id)) ?? specialists[random.Next(specialists.Count)];
                
                var appointment = new Appointment
                {
                    CustomerId = customers[random.Next(customers.Count)].Id,
                    ServiceId = service.Id,
                    SpecialistId = specialist.Id,
                    AppointmentDate = DateTime.Today.AddDays(random.Next(-30, 60)),
                    AppointmentTime = TimeSpan.FromHours(random.Next(9, 18)) + TimeSpan.FromMinutes(random.Next(0, 4) * 15),
                    Status = statuses[random.Next(statuses.Length)],
                    Notes = $"Appointment #{i}",
                    ReservationCode = GenerateReservationCode(),
                    TotalPrice = service.Price,
                    IsActive = true
                };
                appointments.Add(appointment);
            }

            await _storage.WriteFileAsync("Appointments.json", appointments);
            _logger.LogInfo($"Created {appointments.Count} appointments");

            // Generate Reviews
            var reviews = new List<Review>();
            var reviewComments = new[]
            {
                "Excellent service! Highly recommended.",
                "Very professional and friendly staff.",
                "Great experience, will come back again.",
                "Amazing results, love it!",
                "Good service but a bit pricey.",
                "The best salon in town!",
                "Very satisfied with the treatment.",
                "Professional work, great atmosphere.",
                "Exceeded my expectations!",
                "Wonderful experience from start to finish."
            };

            for (int i = 0; i < reviewsCount; i++)
            {
                var review = new Review
                {
                    CustomerName = $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}",
                    Rating = random.Next(3, 6), // 3 to 5 stars
                    Comment = reviewComments[random.Next(reviewComments.Length)],
                    ServiceId = services[random.Next(services.Count)].Id,
                    SpecialistId = specialists[random.Next(specialists.Count)].Id
                };
                reviews.Add(review);
            }

            await _storage.WriteFileAsync("Reviews.json", reviews);
            _logger.LogInfo($"Created {reviews.Count} reviews");

            // Generate Gallery Images
            var galleryImages = new List<GalleryImage>();
            var galleryCategories = new[] { "Hair", "Nails", "Makeup", "Interior", "Before/After" };

            for (int i = 0; i < 30; i++)
            {
                var image = new GalleryImage
                {
                    Title = $"Gallery Image {i + 1}",
                    Description = "Beautiful work by our talented team",
                    ImageUrl = $"https://picsum.photos/seed/{i}/800/600",
                    Category = galleryCategories[random.Next(galleryCategories.Length)]
                };
                galleryImages.Add(image);
            }

            await _storage.WriteFileAsync("GalleryImages.json", galleryImages);
            _logger.LogInfo($"Created {galleryImages.Count} gallery images");

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Mock data generated successfully: {customers.Count} customers, {services.Count} services, {specialists.Count} specialists, {appointments.Count} appointments, {reviews.Count} reviews, {galleryImages.Count} gallery images",
                Data = new
                {
                    customersCount = customers.Count,
                    servicesCount = services.Count,
                    specialistsCount = specialists.Count,
                    appointmentsCount = appointments.Count,
                    reviewsCount = reviews.Count,
                    galleryImagesCount = galleryImages.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error generating mock data", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while generating mock data."
            });
        }
    }

    [HttpPost("test-rotation")]
    public async Task<IActionResult> TestFileRotation()
    {
        try
        {
            _logger.LogInfo("Starting file rotation test...");

            // Create test items rapidly
            var testItems = new List<TestItem>();
            
            for (int i = 0; i < 100; i++)
            {
                var item = new TestItem
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"Test Item {i}",
                    Value = i,
                    CreatedAt = DateTime.UtcNow
                };
                testItems.Add(item);
                
                // Write each item to trigger rotation
                await _storage.AppendToFileAsync("TestRotation", item);
            }

            // Read all files to verify
            var allItems = await _storage.ReadAllFilesAsync<TestItem>("TestRotation");
            var files = await _storage.GetAllFilesAsync("TestRotation");

            _logger.LogInfo($"File rotation test completed. Files created: {files.Length}, Total items: {allItems.Count}");

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"File rotation test completed successfully. Created {files.Length} files with {allItems.Count} total items.",
                Data = new
                {
                    filesCreated = files.Length,
                    totalItems = allItems.Count,
                    fileNames = files
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during file rotation test", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred during file rotation test."
            });
        }
    }

    private string GenerateReservationCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var code = new char[8];
        
        for (int i = 0; i < 8; i++)
        {
            code[i] = chars[random.Next(chars.Length)];
        }

        return new string(code);
    }
}

public class MockDataRequest
{
    public int CustomersCount { get; set; } = 100;
    public int ServicesCount { get; set; } = 50;
    public int SpecialistsCount { get; set; } = 20;
    public int AppointmentsCount { get; set; } = 500;
    public int ReviewsCount { get; set; } = 100;
}

public class TestItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public DateTime CreatedAt { get; set; }
}
