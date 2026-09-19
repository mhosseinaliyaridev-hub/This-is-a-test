using BeautyStudio.Domain.Entities;
using BeautyStudio.Domain.Interfaces;

namespace BeautyStudio.Application.Services;

public class AppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly ISpecialistRepository _specialistRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILoggerService _logger;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IServiceRepository serviceRepository,
        ISpecialistRepository specialistRepository,
        ICustomerRepository customerRepository,
        ILoggerService logger)
    {
        _appointmentRepository = appointmentRepository;
        _serviceRepository = serviceRepository;
        _specialistRepository = specialistRepository;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<List<Appointment>> GetAllAsync()
    {
        return await _appointmentRepository.GetAllAsync();
    }

    public async Task<Appointment?> GetByIdAsync(string id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment != null)
        {
            await LoadNavigationPropertiesAsync(appointment);
        }
        return appointment;
    }

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        ValidateAppointment(appointment);
        
        // Check availability
        var isAvailable = await _appointmentRepository.IsTimeSlotAvailableAsync(
            appointment.SpecialistId, 
            appointment.AppointmentDate, 
            appointment.AppointmentTime);

        if (!isAvailable)
        {
            throw new ValidationException("The selected time slot is not available. Please choose another time.");
        }

        // Generate reservation code
        appointment.ReservationCode = GenerateReservationCode();
        
        // Calculate total price
        var service = await _serviceRepository.GetByIdAsync(appointment.ServiceId);
        if (service != null)
        {
            appointment.TotalPrice = service.Price;
        }

        _logger.LogInfo($"Creating new appointment: {appointment.ReservationCode}");
        var created = await _appointmentRepository.CreateAsync(appointment);
        await LoadNavigationPropertiesAsync(created);
        return created;
    }

    public async Task<Appointment> UpdateStatusAsync(string id, string status)
    {
        var validStatuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };
        if (!validStatuses.Contains(status))
        {
            throw new ValidationException($"Invalid status: {status}. Valid statuses are: {string.Join(", ", validStatuses)}");
        }

        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
        {
            throw new KeyNotFoundException($"Appointment with ID {id} not found.");
        }

        appointment.Status = status;
        appointment.UpdatedAt = DateTime.UtcNow;
        
        _logger.LogInfo($"Updating appointment {id} status to {status}");
        var updated = await _appointmentRepository.UpdateAsync(appointment);
        await LoadNavigationPropertiesAsync(updated);
        return updated;
    }

    public async Task<bool> CancelAsync(string id)
    {
        _logger.LogInfo($"Cancelling appointment: {id}");
        return await UpdateStatusAsync(id, "Cancelled") != null;
    }

    public async Task<List<Appointment>> GetByCustomerIdAsync(string customerId)
    {
        var appointments = await _appointmentRepository.GetByCustomerIdAsync(customerId);
        foreach (var appointment in appointments)
        {
            await LoadNavigationPropertiesAsync(appointment);
        }
        return appointments;
    }

    public async Task<List<Appointment>> GetBySpecialistIdAsync(string specialistId)
    {
        var appointments = await _appointmentRepository.GetBySpecialistIdAsync(specialistId);
        foreach (var appointment in appointments)
        {
            await LoadNavigationPropertiesAsync(appointment);
        }
        return appointments;
    }

    public async Task<List<Appointment>> GetByDateAsync(DateTime date)
    {
        var appointments = await _appointmentRepository.GetByDateAsync(date);
        foreach (var appointment in appointments)
        {
            await LoadNavigationPropertiesAsync(appointment);
        }
        return appointments;
    }

    public async Task<List<Appointment>> GetByStatusAsync(string status)
    {
        var appointments = await _appointmentRepository.GetByStatusAsync(status);
        foreach (var appointment in appointments)
        {
            await LoadNavigationPropertiesAsync(appointment);
        }
        return appointments;
    }

    public async Task<bool> IsTimeSlotAvailableAsync(string specialistId, DateTime date, TimeSpan time)
    {
        return await _appointmentRepository.IsTimeSlotAvailableAsync(specialistId, date, time);
    }

    private async Task LoadNavigationPropertiesAsync(Appointment appointment)
    {
        if (appointment == null) return;

        appointment.Customer = await _customerRepository.GetByIdAsync(appointment.CustomerId);
        appointment.Service = await _serviceRepository.GetByIdAsync(appointment.ServiceId);
        appointment.Specialist = await _specialistRepository.GetByIdAsync(appointment.SpecialistId);
    }

    private void ValidateAppointment(Appointment appointment)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(appointment.CustomerId))
            errors.Add("Customer ID is required.");

        if (string.IsNullOrWhiteSpace(appointment.ServiceId))
            errors.Add("Service ID is required.");

        if (string.IsNullOrWhiteSpace(appointment.SpecialistId))
            errors.Add("Specialist ID is required.");

        if (appointment.AppointmentDate == default)
            errors.Add("Appointment date is required.");

        if (appointment.AppointmentDate < DateTime.Today)
            errors.Add("Appointment date cannot be in the past.");

        if (errors.Count > 0)
            throw new ValidationException(string.Join(" ", errors));
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
