# Beauty Studio - Professional Beauty Salon Website

A complete, production-ready web application for a beauty salon built with:
- **Frontend**: HTML5, CSS3, Vanilla JavaScript
- **Backend**: C#, .NET 8
- **Storage**: JSON File-based with rotation support

## Features

### Public Features
- ✅ Professional Landing Page with Hero Section
- ✅ Services Catalog with Search, Filter & Sort
- ✅ Specialists Directory
- ✅ Image Gallery with Lightbox
- ✅ Customer Reviews System
- ✅ Multi-step Booking System
- ✅ Appointment Confirmation with Reservation Code
- ✅ Dark/Light Mode Toggle
- ✅ Fully Responsive Design
- ✅ Toast Notifications

### Admin Dashboard
- ✅ Dashboard Statistics
- ✅ Customer Management
- ✅ Appointment Management (Confirm/Cancel)
- ✅ Service Management
- ✅ Specialist Management

### Backend Architecture
- ✅ Clean Architecture (API, Application, Domain, Infrastructure)
- ✅ Repository Pattern
- ✅ File-based Storage with Abstraction (IFileStorageService)
- ✅ Automatic File Rotation
- ✅ Thread-Safe File Operations
- ✅ Backup System
- ✅ Logging System
- ✅ Mock Data Generator

## Project Structure

```
BeautyStudio/
├── src/
│   ├── BeautyStudio.API/          # ASP.NET Core Web API
│   ├── BeautyStudio.Application/  # Business Logic
│   ├── BeautyStudio.Domain/       # Entities & Interfaces
│   └── BeautyStudio.Infrastructure/ # File Storage & Repositories
├── frontend/
│   ├── index.html                 # Main HTML file
│   ├── css/styles.css             # Complete styling
│   └── js/app.js                  # Frontend JavaScript
├── data/                          # JSON data storage
│   ├── Customers/
│   ├── Services/
│   ├── Specialists/
│   ├── Appointments/
│   ├── Payments/
│   ├── Reviews/
│   ├── Settings/
│   ├── Logs/
│   └── Backups/
└── README.md
```

## How to Run

### Prerequisites
- .NET 8 SDK
- A modern web browser

### Steps

1. **Navigate to the API project:**
   ```bash
   cd BeautyStudio/src/BeautyStudio.API
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

3. **Generate Mock Data:**
   Once the API is running, send a POST request to generate mock data:
   ```bash
   curl -X POST http://localhost:5000/api/seed/generate \
     -H "Content-Type: application/json" \
     -d '{"customersCount": 100, "servicesCount": 50, "specialistsCount": 20, "appointmentsCount": 500, "reviewsCount": 100}'
   ```

4. **Open the Frontend:**
   Open `frontend/index.html` in your browser, or access it via:
   ```
   http://localhost:5000
   ```

5. **Access Swagger UI:**
   ```
   http://localhost:5000/swagger
   ```

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/customers` | GET | Get all customers |
| `/api/customers` | POST | Create customer |
| `/api/services` | GET | Get all services |
| `/api/services/{id}` | GET | Get service by ID |
| `/api/specialists` | GET | Get all specialists |
| `/api/appointments` | GET | Get all appointments |
| `/api/appointments` | POST | Create appointment |
| `/api/appointments/{id}/status` | PUT | Update appointment status |
| `/api/reviews` | GET | Get all reviews |
| `/api/reviews` | POST | Create review |
| `/api/gallery` | GET | Get gallery images |
| `/api/dashboard/stats` | GET | Get dashboard statistics |
| `/api/seed/generate` | POST | Generate mock data |
| `/api/seed/test-rotation` | POST | Test file rotation |

## Configuration

Edit `appsettings.json` to configure:

```json
{
  "Storage": {
    "BasePath": "data",
    "MaxFileSizeMB": 10,
    "Encoding": "UTF-8",
    "BackupEnabled": true,
    "BackupPath": "data/Backups"
  }
}
```

## File Rotation

The system automatically rotates files when they reach the configured size limit:
- Files are named: `{Entity}_{index}.json` (e.g., `Appointments_001.json`)
- Old files are preserved, not deleted
- Reading aggregates data from all rotated files
- Thread-safe with semaphore locks

## Security Features

- Input Validation
- Path Traversal Prevention
- Exception Handling
- No Stack Trace Exposure
- CORS Configuration

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## License

This project is for demonstration purposes.
