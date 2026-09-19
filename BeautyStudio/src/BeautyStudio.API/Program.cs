using BeautyStudio.Domain.Configuration;
using BeautyStudio.Domain.Interfaces;
using BeautyStudio.Infrastructure.Repositories;
using BeautyStudio.Infrastructure.Services;
using BeautyStudio.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Storage Settings
var storageSettings = new StorageSettings();
builder.Configuration.GetSection("Storage").Bind(storageSettings);
builder.Services.AddSingleton(storageSettings);

// Register Logger
builder.Services.AddSingleton<ILoggerService>(sp => 
    new LoggerService(storageSettings.BasePath));

// Register File Storage Service
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();

// Register Repositories
builder.Services.AddSingleton<ICustomerRepository, CustomerRepository>();
builder.Services.AddSingleton<IServiceRepository, ServiceRepository>();
builder.Services.AddSingleton<ISpecialistRepository, SpecialistRepository>();
builder.Services.AddSingleton<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();
builder.Services.AddSingleton<IReviewRepository, ReviewRepository>();
builder.Services.AddSingleton<IGalleryRepository, GalleryRepository>();

// Register Application Services
builder.Services.AddSingleton<CustomerService>();
builder.Services.AddSingleton<AppointmentService>();

// Enable CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseStaticFiles();
app.MapControllers();

// Serve frontend files
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "index.html" }
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "frontend")),
    RequestPath = ""
});

// Initialize mock data on startup
await InitializeMockDataAsync(app.Services);

Console.WriteLine("Beauty Studio API is running...");
Console.WriteLine($"Swagger UI: http://localhost:5000/swagger");
Console.WriteLine($"Frontend: http://localhost:5000");

app.Run("http://0.0.0.0:5000");

static async Task InitializeMockDataAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerService>();
    
    logger.LogInfo("Application started. Checking for mock data...");
    
    // Check if we need to generate mock data
    var storage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
    var customersExist = await storage.FileExistsAsync("Customers.json");
    
    if (!customersExist)
    {
        logger.LogInfo("No existing data found. Generating mock data...");
        // Mock data will be generated via API endpoint
    }
}
