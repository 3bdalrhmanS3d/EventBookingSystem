# Event Booking System V1

An **ASP.NET Core MVC** application for managing and booking events. It provides both
a public-facing site for browsing and booking events and an admin panel for managing
users, events, venues, categories, bookings, reviews, and notifications.

## Features

* **User Registration & Authentication**

  * Sign up with email verification (7-digit code)
  * Password hashing with ASP.NET Core Identity
  * Password reset via email
  * JWT issuing endpoint for API clients

* **Admin Panel** (`/admin`)

  * Dashboard with summary statistics (users, events, bookings, pending verifications)
  * CRUD for Users, Events, Venues, Categories
  * View and delete bookings
  * Audit logs for all create/update/delete operations

* **Public Site**

  * Browse all events (`/events/all`) with search, filter, sort, and pagination
  * View event details, reviews, average rating
  * Favorite events
  * Submit reviews

* **Booking Workflow**

  * Select ticket types (Regular, VIP, Early Bird)
  * Prevent double-booking
  * Generate unique booking reference
  * Create tickets, notifications, and audit log
  * Confirmation page and email notification
  * Print ticket (new tab)

* **Profile Area** (`/profile`)

  * View and edit profile (first name, last name, birthdate)
  * Change password
  * List and cancel bookings
  * Manage favorites
  * View and mark notifications as read
  * List and create event reviews
  * List and print tickets

## Tech Stack

* **Backend:** ASP.NET Core MVC (.NET 7+)
* **Data Access:** Entity Framework Core (SQL Server)
* **Authentication:** Cookie and JWT
* **Logging:** ILogger for controllers
* **Email:** MailKit with background queue
* **Frontend:** Razor views with Bootstrap 5, FontAwesome

## Prerequisites

* .NET SDK 7.0 or later
* SQL Server (local or remote)
* Visual Studio 2022 / VS Code

## Setup Instructions

1. **Clone the repository**

   ```bash
   git clone https://github.com/3bdalrhmanS3d/EventBookingSystem.git
   cd EventBookingSystemV1
   ```

2. **Configure Connection**

   * In `appsettings.json`, set your SQL Server connection:

     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=.;Database=EventBookingDb;Trusted_Connection=True;"
     }
     ```

3. **Apply Migrations & Seed Default Admin**

   ```bash
   dotnet ef database update
   ```
   or

   ```bash
    Update-Database
   ```

   A default admin account is created using `DefaultAdmin` section in `appsettings`:

   ```json
   "DefaultAdmin": {
     "Email": "admin@EBS.com",
     "FullName": "Administrator",
     "Password": "Admin@123"
   }
   ```

5. **User Secrets**
   This project uses **User Secrets** for storing sensitive settings in development.

   * Initialize user secrets:

     ```bash
     dotnet user-secrets init
     ```

   * **JWT Configuration**:

     ```bash
     dotnet user-secrets set "JWT:SecretKey" "YOUR_JWT_SECRET_KEY"
     dotnet user-secrets set "JWT:DurationInMinutes" "60"
     ```

   * **SMTP Email Configuration**:

     ```bash
     dotnet user-secrets set "EmailSettings:Host" "smtp.mailtrap.io"
     dotnet user-secrets set "EmailSettings:Port" "587"
     dotnet user-secrets set "EmailSettings:Username" "your-smtp-username"
     dotnet user-secrets set "EmailSettings:Password" "your-smtp-password"
     dotnet user-secrets set "EmailSettings:FromAddress" "no-reply@yourdomain.com"
     dotnet user-secrets set "EmailSettings:FromName" "EventBookingSystem"
     ```

6. **Run the application**

   ```bash
   dotnet run
   ```

   * Public site: [https://localhost:5001](https://localhost:5001)
   * Admin panel: [https://localhost:5001/admin/dashboard](https://localhost:5001/admin/dashboard)

## Testing & Development

* **Browser**: Use latest Chrome/Edge/Firefox
* **Debugging**: Set breakpoints in controllers or services
* **Logging**: Check console or configured sinks for ILogger output

## Folder Structure

* `Controllers/` — MVC controllers (Account, Admin, Event, Booking, Category, Venue, Profile)
* `Data/` — EF Core DbContext and migrations
* `Models/` — Entity definitions
* `DTOs/` — Data transfer objects for binding
* `ViewModels/` — Models for passing data to views
* `Services/` — Email queue, JWT service
* `Views/` — Razor views

---
