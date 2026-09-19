using BeautyStudio.Domain.Entities;

namespace BeautyStudio.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
    Task<List<T>> SearchAsync(string searchTerm);
}

public interface ICustomerRepository : IRepository<Customer>
{
    Task<List<Customer>> GetByPhoneNumberAsync(string phoneNumber);
    Task<List<Appointment>> GetCustomerAppointmentsAsync(string customerId);
}

public interface IServiceRepository : IRepository<Service>
{
    Task<List<Service>> GetByCategoryAsync(string category);
    Task<List<Service>> GetActiveServicesAsync();
}

public interface ISpecialistRepository : IRepository<Specialist>
{
    Task<List<Specialist>> GetActiveSpecialistsAsync();
    Task<List<Specialist>> GetByServiceIdAsync(string serviceId);
}

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<List<Appointment>> GetByCustomerIdAsync(string customerId);
    Task<List<Appointment>> GetBySpecialistIdAsync(string specialistId);
    Task<List<Appointment>> GetByDateAsync(DateTime date);
    Task<bool> IsTimeSlotAvailableAsync(string specialistId, DateTime date, TimeSpan time);
    Task<List<Appointment>> GetByStatusAsync(string status);
}

public interface IPaymentRepository : IRepository<Payment>
{
    Task<List<Payment>> GetByAppointmentIdAsync(string appointmentId);
    Task<List<Payment>> GetByStatusAsync(string status);
}

public interface IReviewRepository : IRepository<Review>
{
    Task<List<Review>> GetByServiceIdAsync(string serviceId);
    Task<List<Review>> GetBySpecialistIdAsync(string specialistId);
    Task<double> GetAverageRatingAsync(string? serviceId, string? specialistId);
}

public interface IGalleryRepository : IRepository<GalleryImage>
{
    Task<List<GalleryImage>> GetByCategoryAsync(string category);
}

public interface ILoggerService
{
    void LogInfo(string message);
    void LogError(string message, Exception? ex = null);
    void LogWarning(string message);
    Task WriteLogAsync(string message, string level = "INFO");
}

public interface IFileStorageService
{
    Task<T?> ReadFileAsync<T>(string fileName) where T : class;
    Task<List<T>> ReadAllFilesAsync<T>(string entityName) where T : class;
    Task<bool> WriteFileAsync<T>(string fileName, T data) where T : class;
    Task<bool> AppendToFileAsync<T>(string fileName, T item) where T : class;
    Task<bool> FileExistsAsync(string fileName);
    Task<long> GetFileSizeAsync(string fileName);
    Task<string[]> GetAllFilesAsync(string entityName);
    Task<bool> CreateBackupAsync(string fileName);
}
