using Microsoft.AspNetCore.Mvc;
using BeautyStudio.Domain.Entities;
using BeautyStudio.Application.Services;
using BeautyStudio.Domain.Interfaces;

namespace BeautyStudio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _customerService;
    private readonly ILoggerService _logger;

    public CustomersController(CustomerService customerService, ILoggerService logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search = null)
    {
        try
        {
            var customers = search != null 
                ? await _customerService.SearchAsync(search)
                : await _customerService.GetAllAsync();
            
            return Ok(new ApiResponse<List<Customer>>
            {
                Success = true,
                Data = customers
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting customers", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving customers."
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Customer not found."
                });
            }

            return Ok(new ApiResponse<Customer>
            {
                Success = true,
                Data = customer
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting customer {id}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving the customer."
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Customer customer)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid input data."
                });
            }

            var created = await _customerService.CreateAsync(customer);
            
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new ApiResponse<Customer>
            {
                Success = true,
                Message = "Customer created successfully.",
                Data = created
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating customer", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while creating the customer."
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Customer customer)
    {
        try
        {
            if (id != customer.Id)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "ID mismatch."
                });
            }

            var updated = await _customerService.UpdateAsync(customer);
            
            return Ok(new ApiResponse<Customer>
            {
                Success = true,
                Message = "Customer updated successfully.",
                Data = updated
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating customer {id}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating the customer."
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _customerService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Customer not found."
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Customer deleted successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting customer {id}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the customer."
            });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IServiceRepository _repository;
    private readonly ILoggerService _logger;

    public ServicesController(IServiceRepository repository, ILoggerService logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null, [FromQuery] string? search = null)
    {
        try
        {
            var services = await _repository.GetAllAsync();
            
            if (!string.IsNullOrEmpty(category))
            {
                services = services.Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            
            if (!string.IsNullOrEmpty(search))
            {
                services = services.Where(s => s.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                                               s.Description.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Ok(new ApiResponse<List<Service>>
            {
                Success = true,
                Data = services
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting services", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving services."
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var service = await _repository.GetByIdAsync(id);
            if (service == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Service not found."
                });
            }

            return Ok(new ApiResponse<Service>
            {
                Success = true,
                Data = service
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting service {id}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving the service."
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Service service)
    {
        try
        {
            var created = await _repository.CreateAsync(service);
            
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new ApiResponse<Service>
            {
                Success = true,
                Message = "Service created successfully.",
                Data = created
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating service", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while creating the service."
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Service service)
    {
        try
        {
            service.Id = id;
            var updated = await _repository.UpdateAsync(service);
            
            return Ok(new ApiResponse<Service>
            {
                Success = true,
                Message = "Service updated successfully.",
                Data = updated
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating service {id}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating the service."
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _repository.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Service not found."
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Service deleted successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting service {id}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the service."
            });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class SpecialistsController : ControllerBase
{
    private readonly ISpecialistRepository _repository;
    private readonly ILoggerService _logger;

    public SpecialistsController(ISpecialistRepository repository, ILoggerService logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search = null)
    {
        try
        {
            var specialists = await _repository.GetAllAsync();
            
            if (!string.IsNullOrEmpty(search))
            {
                specialists = specialists.Where(s => 
                    s.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                    s.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Ok(new ApiResponse<List<Specialist>>
            {
                Success = true,
                Data = specialists
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting specialists", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving specialists."
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var specialist = await _repository.GetByIdAsync(id);
            if (specialist == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Specialist not found."
                });
            }

            return Ok(new ApiResponse<Specialist>
            {
                Success = true,
                Data = specialist
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting specialist {id}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving the specialist."
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Specialist specialist)
    {
        try
        {
            var created = await _repository.CreateAsync(specialist);
            
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new ApiResponse<Specialist>
            {
                Success = true,
                Message = "Specialist created successfully.",
                Data = created
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating specialist", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while creating the specialist."
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Specialist specialist)
    {
        try
        {
            specialist.Id = id;
            var updated = await _repository.UpdateAsync(specialist);
            
            return Ok(new ApiResponse<Specialist>
            {
                Success = true,
                Message = "Specialist updated successfully.",
                Data = updated
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating specialist {id}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating the specialist."
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _repository.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Specialist not found."
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Specialist deleted successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting specialist {id}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the specialist."
            });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly AppointmentService _appointmentService;
    private readonly ILoggerService _logger;

    public AppointmentsController(AppointmentService appointmentService, ILoggerService logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] DateTime? date = null)
    {
        try
        {
            List<Appointment> appointments;
            
            if (date.HasValue)
            {
                appointments = await _appointmentService.GetByDateAsync(date.Value);
            }
            else if (!string.IsNullOrEmpty(status))
            {
                appointments = await _appointmentService.GetByStatusAsync(status);
            }
            else
            {
                appointments = await _appointmentService.GetAllAsync();
            }

            return Ok(new ApiResponse<List<Appointment>>
            {
                Success = true,
                Data = appointments
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting appointments", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving appointments."
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Appointment not found."
                });
            }

            return Ok(new ApiResponse<Appointment>
            {
                Success = true,
                Data = appointment
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting appointment {id}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving the appointment."
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Appointment appointment)
    {
        try
        {
            var created = await _appointmentService.CreateAsync(appointment);
            
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new ApiResponse<Appointment>
            {
                Success = true,
                Message = "Appointment created successfully.",
                Data = created
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating appointment", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while creating the appointment."
            });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] string status)
    {
        try
        {
            var updated = await _appointmentService.UpdateStatusAsync(id, status);
            
            return Ok(new ApiResponse<Appointment>
            {
                Success = true,
                Message = "Appointment status updated successfully.",
                Data = updated
            });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating appointment status {id}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating the appointment status."
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(string id)
    {
        try
        {
            var result = await _appointmentService.CancelAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Appointment not found."
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Appointment cancelled successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error cancelling appointment {id}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while cancelling the appointment."
            });
        }
    }

    [HttpGet("specialist/{specialistId}")]
    public async Task<IActionResult> GetBySpecialist(string specialistId)
    {
        try
        {
            var appointments = await _appointmentService.GetBySpecialistIdAsync(specialistId);
            
            return Ok(new ApiResponse<List<Appointment>>
            {
                Success = true,
                Data = appointments
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting appointments for specialist {specialistId}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving appointments."
            });
        }
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(string customerId)
    {
        try
        {
            var appointments = await _appointmentService.GetByCustomerIdAsync(customerId);
            
            return Ok(new ApiResponse<List<Appointment>>
            {
                Success = true,
                Data = appointments
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting appointments for customer {customerId}", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving appointments."
            });
        }
    }

    [HttpGet("check-availability")]
    public async Task<IActionResult> CheckAvailability([FromQuery] string specialistId, [FromQuery] DateTime date, [FromQuery] TimeSpan time)
    {
        try
        {
            var isAvailable = await _appointmentService.IsTimeSlotAvailableAsync(specialistId, date, time);
            
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = isAvailable
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error checking availability", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while checking availability."
            });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewRepository _repository;
    private readonly ILoggerService _logger;

    public ReviewsController(IReviewRepository repository, ILoggerService logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var reviews = await _repository.GetAllAsync();
            
            return Ok(new ApiResponse<List<Review>>
            {
                Success = true,
                Data = reviews
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting reviews", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving reviews."
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Review review)
    {
        try
        {
            var created = await _repository.CreateAsync(review);
            
            return CreatedAtAction(nameof(GetAll), new ApiResponse<Review>
            {
                Success = true,
                Message = "Review created successfully.",
                Data = created
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating review", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while creating the review."
            });
        }
    }

    [HttpGet("average-rating")]
    public async Task<IActionResult> GetAverageRating([FromQuery] string? serviceId = null, [FromQuery] string? specialistId = null)
    {
        try
        {
            var average = await _repository.GetAverageRatingAsync(serviceId, specialistId);
            
            return Ok(new ApiResponse<double>
            {
                Success = true,
                Data = average
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting average rating", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while calculating average rating."
            });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class GalleryController : ControllerBase
{
    private readonly IGalleryRepository _repository;
    private readonly ILoggerService _logger;

    public GalleryController(IGalleryRepository repository, ILoggerService logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category = null)
    {
        try
        {
            List<GalleryImage> images;
            
            if (!string.IsNullOrEmpty(category))
            {
                images = await _repository.GetByCategoryAsync(category);
            }
            else
            {
                images = await _repository.GetAllAsync();
            }
            
            return Ok(new ApiResponse<List<GalleryImage>>
            {
                Success = true,
                Data = images
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting gallery images", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving gallery images."
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GalleryImage image)
    {
        try
        {
            var created = await _repository.CreateAsync(image);
            
            return CreatedAtAction(nameof(GetAll), new ApiResponse<GalleryImage>
            {
                Success = true,
                Message = "Image added to gallery successfully.",
                Data = created
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error adding image to gallery", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while adding the image."
            });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly ISpecialistRepository _specialistRepository;
    private readonly ILoggerService _logger;

    public DashboardController(
        IAppointmentRepository appointmentRepository,
        ICustomerRepository customerRepository,
        IServiceRepository serviceRepository,
        ISpecialistRepository specialistRepository,
        ILoggerService logger)
    {
        _appointmentRepository = appointmentRepository;
        _customerRepository = customerRepository;
        _serviceRepository = serviceRepository;
        _specialistRepository = specialistRepository;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var customers = await _customerRepository.GetAllAsync();
            var appointments = await _appointmentRepository.GetAllAsync();
            var services = await _serviceRepository.GetAllAsync();
            var specialists = await _specialistRepository.GetAllAsync();

            var today = DateTime.Today;
            var todayAppointments = appointments.Where(a => a.AppointmentDate.Date == today).ToList();
            var completedAppointments = appointments.Where(a => a.Status == "Completed").ToList();
            var cancelledAppointments = appointments.Where(a => a.Status == "Cancelled").ToList();
            var totalRevenue = completedAppointments.Sum(a => a.TotalPrice);

            // Group appointments by service for popular services
            var popularServices = appointments
                .GroupBy(a => a.ServiceId)
                .Select(g => new { ServiceId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            var stats = new
            {
                totalCustomers = customers.Count,
                totalAppointments = appointments.Count,
                todayAppointments = todayAppointments.Count,
                completedAppointments = completedAppointments.Count,
                cancelledAppointments = cancelledAppointments.Count,
                totalRevenue = totalRevenue,
                activeSpecialists = specialists.Count,
                totalServices = services.Count,
                popularServices
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting dashboard stats", ex);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving dashboard statistics."
            });
        }
    }
}

// Generic API Response
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}
