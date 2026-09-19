using System.Text.Json;
using BeautyStudio.Domain.Entities;
using BeautyStudio.Domain.Interfaces;
using BeautyStudio.Infrastructure.Services;

namespace BeautyStudio.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IFileStorageService _storage;
    protected readonly ILoggerService _logger;
    protected readonly string _entityName;

    public Repository(IFileStorageService storage, ILoggerService logger, string entityName)
    {
        _storage = storage;
        _logger = logger;
        _entityName = entityName;
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _storage.ReadAllFilesAsync<T>(_entityName);
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        var items = await GetAllAsync();
        return items.FirstOrDefault(x => x.Id == id);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        entity.Id = Guid.NewGuid().ToString();
        entity.CreatedAt = DateTime.UtcNow;
        
        await _storage.AppendToFileAsync($"{_entityName}.json", entity);
        _logger.LogInfo($"Created {_entityName}: {entity.Id}");
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var items = await GetAllAsync();
        var existingIndex = items.FindIndex(x => x.Id == entity.Id);
        
        if (existingIndex == -1)
            throw new KeyNotFoundException($"{_entityName} with ID {entity.Id} not found.");

        items[existingIndex] = entity;
        await _storage.WriteFileAsync($"{_entityName}.json", items);
        _logger.LogInfo($"Updated {_entityName}: {entity.Id}");
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        var items = await GetAllAsync();
        var item = items.FirstOrDefault(x => x.Id == id);
        
        if (item == null)
            return false;

        item.IsActive = false;
        item.UpdatedAt = DateTime.UtcNow;
        
        var index = items.FindIndex(x => x.Id == id);
        items[index] = item;
        
        await _storage.WriteFileAsync($"{_entityName}.json", items);
        _logger.LogInfo($"Deleted {_entityName}: {id}");
        return true;
    }

    public virtual async Task<List<T>> SearchAsync(string searchTerm)
    {
        var items = await GetAllAsync();
        if (string.IsNullOrWhiteSpace(searchTerm))
            return items;

        var searchLower = searchTerm.ToLower();
        return items.Where(x => 
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.PropertyType == typeof(string))
                {
                    var value = prop.GetValue(x)?.ToString()?.ToLower();
                    if (value?.Contains(searchLower) == true)
                        return true;
                }
            }
            return false;
        }).ToList();
    }
}

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    private readonly IAppointmentRepository? _appointmentRepository;

    public CustomerRepository(IFileStorageService storage, ILoggerService logger) 
        : base(storage, logger, "Customers")
    {
    }

    public CustomerRepository(IFileStorageService storage, ILoggerService logger, IAppointmentRepository appointmentRepository) 
        : base(storage, logger, "Customers")
    {
        _appointmentRepository = appointmentRepository;
    }

    public override async Task<List<Customer>> GetAllAsync()
    {
        var customers = await base.GetAllAsync();
        return customers.Where(c => c.IsActive).ToList();
    }

    public async Task<List<Customer>> GetByPhoneNumberAsync(string phoneNumber)
    {
        var customers = await GetAllAsync();
        return customers.Where(c => c.PhoneNumber.Contains(phoneNumber)).ToList();
    }

    public async Task<List<Appointment>> GetCustomerAppointmentsAsync(string customerId)
    {
        if (_appointmentRepository == null)
            return new List<Appointment>();

        var allAppointments = await _appointmentRepository.GetAllAsync();
        return allAppointments.Where(a => a.CustomerId == customerId).ToList();
    }
}

public class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(IFileStorageService storage, ILoggerService logger) 
        : base(storage, logger, "Services")
    {
    }

    public override async Task<List<Service>> GetAllAsync()
    {
        var services = await base.GetAllAsync();
        return services.Where(s => s.IsActive).ToList();
    }

    public async Task<List<Service>> GetByCategoryAsync(string category)
    {
        var services = await GetAllAsync();
        return services.Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<List<Service>> GetActiveServicesAsync()
    {
        return await GetAllAsync();
    }
}

public class SpecialistRepository : Repository<Specialist>, ISpecialistRepository
{
    public SpecialistRepository(IFileStorageService storage, ILoggerService logger) 
        : base(storage, logger, "Specialists")
    {
    }

    public override async Task<List<Specialist>> GetAllAsync()
    {
        var specialists = await base.GetAllAsync();
        return specialists.Where(s => s.IsActive).ToList();
    }

    public async Task<List<Specialist>> GetActiveSpecialistsAsync()
    {
        return await GetAllAsync();
    }

    public async Task<List<Specialist>> GetByServiceIdAsync(string serviceId)
    {
        var specialists = await GetAllAsync();
        return specialists.Where(s => s.ServiceIds.Contains(serviceId)).ToList();
    }
}

public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(IFileStorageService storage, ILoggerService logger) 
        : base(storage, logger, "Appointments")
    {
    }

    public override async Task<List<Appointment>> GetAllAsync()
    {
        var appointments = await base.GetAllAsync();
        return appointments.Where(a => a.IsActive).ToList();
    }

    public async Task<List<Appointment>> GetByCustomerIdAsync(string customerId)
    {
        var appointments = await GetAllAsync();
        return appointments.Where(a => a.CustomerId == customerId).ToList();
    }

    public async Task<List<Appointment>> GetBySpecialistIdAsync(string specialistId)
    {
        var appointments = await GetAllAsync();
        return appointments.Where(a => a.SpecialistId == specialistId).ToList();
    }

    public async Task<List<Appointment>> GetByDateAsync(DateTime date)
    {
        var appointments = await GetAllAsync();
        return appointments.Where(a => a.AppointmentDate.Date == date.Date).ToList();
    }

    public async Task<List<Appointment>> GetByStatusAsync(string status)
    {
        var appointments = await GetAllAsync();
        return appointments.Where(a => a.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public async Task<bool> IsTimeSlotAvailableAsync(string specialistId, DateTime date, TimeSpan time)
    {
        var appointments = await GetByDateAsync(date);
        var activeAppointments = appointments.Where(a => 
            a.SpecialistId == specialistId && 
            a.Status != "Cancelled" &&
            a.AppointmentTime.Hours == time.Hours &&
            a.AppointmentTime.Minutes == time.Minutes).ToList();

        return !activeAppointments.Any();
    }
}

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(IFileStorageService storage, ILoggerService logger) 
        : base(storage, logger, "Payments")
    {
    }

    public async Task<List<Payment>> GetByAppointmentIdAsync(string appointmentId)
    {
        var payments = await GetAllAsync();
        return payments.Where(p => p.AppointmentId == appointmentId).ToList();
    }

    public async Task<List<Payment>> GetByStatusAsync(string status)
    {
        var payments = await GetAllAsync();
        return payments.Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}

public class ReviewRepository : Repository<Review>, IReviewRepository
{
    public ReviewRepository(IFileStorageService storage, ILoggerService logger) 
        : base(storage, logger, "Reviews")
    {
    }

    public async Task<List<Review>> GetByServiceIdAsync(string serviceId)
    {
        var reviews = await GetAllAsync();
        return reviews.Where(r => r.ServiceId == serviceId).ToList();
    }

    public async Task<List<Review>> GetBySpecialistIdAsync(string specialistId)
    {
        var reviews = await GetAllAsync();
        return reviews.Where(r => r.SpecialistId == specialistId).ToList();
    }

    public async Task<double> GetAverageRatingAsync(string? serviceId, string? specialistId)
    {
        var reviews = await GetAllAsync();
        
        if (!string.IsNullOrEmpty(serviceId))
            reviews = reviews.Where(r => r.ServiceId == serviceId).ToList();
        
        if (!string.IsNullOrEmpty(specialistId))
            reviews = reviews.Where(r => r.SpecialistId == specialistId).ToList();

        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
    }
}

public class GalleryRepository : Repository<GalleryImage>, IGalleryRepository
{
    public GalleryRepository(IFileStorageService storage, ILoggerService logger) 
        : base(storage, logger, "GalleryImages")
    {
    }

    public async Task<List<GalleryImage>> GetByCategoryAsync(string category)
    {
        var images = await GetAllAsync();
        return images.Where(i => i.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
