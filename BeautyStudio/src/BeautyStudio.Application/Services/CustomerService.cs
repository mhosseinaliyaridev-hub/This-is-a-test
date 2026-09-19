using BeautyStudio.Domain.Entities;
using BeautyStudio.Domain.Interfaces;

namespace BeautyStudio.Application.Services;

public class CustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly ILoggerService _logger;

    public CustomerService(ICustomerRepository repository, ILoggerService logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Customer?> GetByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        ValidateCustomer(customer);
        _logger.LogInfo($"Creating new customer: {customer.FirstName} {customer.LastName}");
        return await _repository.CreateAsync(customer);
    }

    public async Task<Customer> UpdateAsync(Customer customer)
    {
        ValidateCustomer(customer);
        customer.UpdatedAt = DateTime.UtcNow;
        _logger.LogInfo($"Updating customer: {customer.Id}");
        return await _repository.UpdateAsync(customer);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        _logger.LogInfo($"Deleting customer: {id}");
        return await _repository.DeleteAsync(id);
    }

    public async Task<List<Customer>> SearchAsync(string searchTerm)
    {
        return await _repository.SearchAsync(searchTerm);
    }

    public async Task<List<Customer>> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _repository.GetByPhoneNumberAsync(phoneNumber);
    }

    public async Task<List<Appointment>> GetCustomerAppointmentsAsync(string customerId)
    {
        return await _repository.GetCustomerAppointmentsAsync(customerId);
    }

    private void ValidateCustomer(Customer customer)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(customer.FirstName))
            errors.Add("First name is required.");

        if (string.IsNullOrWhiteSpace(customer.LastName))
            errors.Add("Last name is required.");

        if (string.IsNullOrWhiteSpace(customer.Email))
            errors.Add("Email is required.");
        else if (!IsValidEmail(customer.Email))
            errors.Add("Invalid email format.");

        if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
            errors.Add("Phone number is required.");

        if (errors.Count > 0)
            throw new ValidationException(string.Join(" ", errors));
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
