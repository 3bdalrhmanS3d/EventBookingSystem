# Event Booking System V1

An **ASP.NET Core MVC** application for managing and booking events. It provides both a public-facing site for browsing and booking events and an admin panel for managing users, events, venues, categories, bookings, reviews, and notifications.

---

## 📑 Table of Contents

- [Features](#features)  
- [Tech Stack](#tech-stack)  
- [Prerequisites](#prerequisites)  
- [Setup Instructions](#setup-instructions)  
- [Testing & Development](#testing--development)  
- [Folder Structure](#folder-structure)  
- [Endpoints](#endpoints)  
- [Screenshots](#screenshots)  
  - [Home Page](#home-page)  
  - [Register & Login](#register--login)  
  - [Profile](#profile)  
  - [Event Details](#event-details)  
  - [Admin Dashboard](#admin-dashboard)  
  - [Booking Confirmation](#booking-confirmation)  

---

## Features

- **User Registration & Authentication**  
  - Sign up with email verification (7-digit code)  
  - Password hashing with ASP.NET Core Identity  
  - Password reset via email  
  - JWT issuing endpoint for API clients

- **Admin Panel** (`/admin`)  
  - Dashboard with summary statistics (users, events, bookings, pending verifications)  
  - CRUD for Users, Events, Venues, Categories  
  - View and delete bookings  
  - Audit logs for all create/update/delete operations

- **Public Site**  
  - Browse all events (`/events/all`) with search, filter, sort, and pagination  
  - View event details, reviews, average rating  
  - Favorite events  
  - Submit reviews

- **Booking Workflow**  
  - Select ticket types (Regular, VIP, Early Bird)  
  - Prevent double-booking  
  - Generate unique booking reference  
  - Create tickets, notifications, and audit log  
  - Confirmation page and email notification  
  - Print ticket (opens in new tab)

- **Profile Area** (`/profile`)  
  - View and edit profile (first name, last name, birthdate)  
  - Change password  
  - List and cancel bookings  
  - Manage favorites  
  - View and mark notifications as read  
  - List and create event reviews  
  - List and print tickets

---

## Tech Stack

- **Backend:** ASP.NET Core MVC (.NET 7+)  
- **Data Access:** Entity Framework Core (SQL Server)  
- **Authentication:** Cookies & JWT  
- **Logging:** `ILogger` for controllers  
- **Email:** MailKit with background queue  
- **Frontend:** Razor views with Bootstrap 5, FontAwesome

---

## Prerequisites

- .NET SDK 7.0 or later  
- SQL Server (local or remote)  
- Visual Studio 2022 / VS Code

---

## Setup Instructions

1. **Clone the repository**  
   ```bash
       git clone https://github.com/3bdalrhmanS3d/EventBookingSystem.git
       cd EventBookingSystemV1
    ```

2. **Configure database connection**
   In `appsettings.json`, set your SQL Server connection:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=.;Database=EventBookingDb;Trusted_Connection=True;"
   }
   ```

3. **Apply migrations & seed default admin**

   ```bash
   dotnet ef database update
   ```

   Or in the Package Manager Console:

   ```powershell
   Update-Database
   ```

   A default admin account is created via the `DefaultAdmin` section in `appsettings.json`:

   ```json
   "DefaultAdmin": {
     "Email": "admin@EBS.com",
     "FullName": "Administrator",
     "Password": "Admin@123"
   }
   ```

4. **Set up User Secrets**

   ```bash
   dotnet user-secrets init
   ```

   * **JWT configuration**

     ```bash
     dotnet user-secrets set "JWT:SecretKey" "YOUR_JWT_SECRET_KEY"
     dotnet user-secrets set "JWT:DurationInMinutes" "60"
     ```
   * **SMTP email configuration**

     ```bash
     dotnet user-secrets set "EmailSettings:Host" "smtp.mailtrap.io"
     dotnet user-secrets set "EmailSettings:Port" "587"
     dotnet user-secrets set "EmailSettings:Username" "your-smtp-username"
     dotnet user-secrets set "EmailSettings:Password" "your-smtp-password"
     dotnet user-secrets set "EmailSettings:FromAddress" "no-reply@yourdomain.com"
     dotnet user-secrets set "EmailSettings:FromName" "EventBookingSystem"
     ```

5. **Run the application**

   ```bash
   dotnet run
   ```

   * Public site: `https://localhost:5001`
   * Admin panel: `https://localhost:5001/admin/dashboard`

---

## Testing & Development

* **Browser:** Latest Chrome, Edge, or Firefox
* **Debugging:** Set breakpoints in controllers or services
* **Logging:** Check console or configured sinks for `ILogger` output

---

## Folder Structure

```
Controllers/      — MVC controllers (Account, Admin, Event, Booking, Category, Venue, Profile)
Data/             — EF Core DbContext and migrations
Models/           — Entity definitions
DTOs/             — Data transfer objects for binding
ViewModels/       — Models for passing data to views
Services/         — Email queue, JWT service
Views/            — Razor views
```

---

## Endpoints

> Below is the full list of available controller actions, grouped by controller and ordered by route.

### AccountController

| Verb | Route                          | Action Method                 | Description                     |
| ---- | ------------------------------ | ----------------------------- | ------------------------------- |
| GET  | `/account/signup`              | SignUp (GET)                  | Display signup form             |
| POST | `/account/signup`              | SignUp (POST)                 | Process user registration       |
| GET  | `/account/verify-email`        | VerifyEmail (GET)             | Display email verification form |
| POST | `/account/verify-email`        | VerifyEmail (POST)            | Process activation code         |
| GET  | `/account/signin`              | SignIn (GET)                  | Display login form              |
| POST | `/account/signin`              | SignIn (POST)                 | Authenticate user               |
| GET  | `/account/forgot-password`     | ForgotPassword (GET)          | Display forgot password form    |
| POST | `/account/forgot-password`     | ForgotPassword (POST)         | Send reset link via email       |
| GET  | `/account/reset-password`      | ResetPassword (GET)           | Display reset password form     |
| POST | `/account/reset-password`      | ResetPassword (POST)          | Process new password            |
| GET  | `/account/resend-verification` | ResendVerificationEmail (GET) | Resend activation email         |
| POST | `/account/signout`             | Logout (POST)                 | Sign out user                   |

### AdminController

| Verb | Route                          | Action Method  | Description                        |
| ---- | ------------------------------ | -------------- | ---------------------------------- |
| GET  | `/admin/dashboard`             | Index          | Display admin dashboard            |
| GET  | `/admin/users`                 | Users          | List users with search & paging    |
| GET  | `/admin/users/{id}`            | UserDetails    | Display user details               |
| POST | `/admin/users/{id}/activate`   | ActivateUser   | Activate user account              |
| POST | `/admin/users/{id}/deactivate` | DeactivateUser | Deactivate user account            |
| POST | `/admin/users/{id}/delete`     | DeleteUser     | Delete user                        |
| GET  | `/admin/events`                | Events         | List events with search & paging   |
| GET  | `/admin/events/{id}`           | EventDetails   | Display event details              |
| POST | `/admin/events/{id}/delete`    | DeleteEvent    | Delete an event                    |
| GET  | `/admin/bookings`              | Bookings       | List bookings with search & paging |
| POST | `/admin/bookings/{id}/delete`  | DeleteBooking  | Delete a booking                   |
| GET  | `/admin/audit`                 | Audit          | List audit log entries             |
| GET  | `/admin/audit/{id}/details`    | AuditDetails   | Display audit log entry details    |

### EventController

| Verb | Route                 | Action Method       | Description                    |
| ---- | --------------------- | ------------------- | ------------------------------ |
| GET  | `/events`             | Index (AdminEvents) | List events for admin          |
| GET  | `/events/all`         | Events (UserEvents) | List public events             |
| GET  | `/events/create`      | Create (GET)        | Display creation form          |
| POST | `/events/create`      | Create (POST)       | Process new event              |
| GET  | `/events/{id}`        | Details             | Display event details (public) |
| GET  | `/events/edit/{id}`   | Edit (GET)          | Display edit form              |
| POST | `/events/edit/{id}`   | Edit (POST)         | Process event update           |
| POST | `/events/delete/{id}` | Delete              | Delete an event                |

### CategoryController

| Verb | Route                     | Action Method         | Description              |
| ---- | ------------------------- | --------------------- | ------------------------ |
| GET  | `/categories`             | Index                 | List categories          |
| GET  | `/categories/create`      | CreateCategory (GET)  | Display creation form    |
| POST | `/categories/create`      | CreateCategory (POST) | Process new category     |
| GET  | `/categories/edit/{id}`   | EditCategory (GET)    | Display edit form        |
| POST | `/categories/edit/{id}`   | EditCategory (POST)   | Process category update  |
| POST | `/categories/delete/{id}` | DeleteCategory        | Delete a category        |
| GET  | `/categories/{id}`        | Details               | Display category details |

### VenueController

| Verb | Route                 | Action Method      | Description           |
| ---- | --------------------- | ------------------ | --------------------- |
| GET  | `/venues`             | Index              | List venues           |
| GET  | `/venues/create`      | CreateVenue (GET)  | Display creation form |
| POST | `/venues/create`      | CreateVenue (POST) | Process new venue     |
| GET  | `/venues/edit/{id}`   | EditVenue (GET)    | Display edit form     |
| POST | `/venues/edit/{id}`   | EditVenue (POST)   | Process venue update  |
| POST | `/venues/delete/{id}` | DeleteVenue        | Delete a venue        |
| GET  | `/venues/{id}`        | Details            | Display venue details |

### BookingController

| Verb | Route                        | Action Method | Description                       |
| ---- | ---------------------------- | ------------- | --------------------------------- |
| GET  | `/booking/create/{eventId}`  | Create (GET)  | Display booking form for event    |
| POST | `/booking/create/{eventId}`  | Create (POST) | Process booking & create tickets  |
| GET  | `/booking/confirmation/{id}` | Confirmation  | Display booking confirmation page |

### ProfileController

| Verb | Route                                   | Action Method         | Description                         |
| ---- | --------------------------------------- | --------------------- | ----------------------------------- |
| GET  | `/profile`                              | Index                 | Display user profile                |
| GET  | `/profile/edit`                         | Edit (GET)            | Display profile edit form           |
| POST | `/profile/edit`                         | Edit (POST)           | Process profile updates             |
| GET  | `/profile/change-password`              | ChangePassword (GET)  | Display change password form        |
| POST | `/profile/change-password`              | ChangePassword (POST) | Process password change             |
| GET  | `/profile/bookings`                     | MyBookings            | List user’s bookings                |
| POST | `/profile/bookings/{id}/cancel`         | CancelBooking         | Cancel a booking                    |
| GET  | `/profile/favorites`                    | MyFavorites           | List favorite events                |
| POST | `/profile/favorites/toggle/{eventId}`   | ToggleFavorite        | Add or remove favorite              |
| GET  | `/profile/notifications`                | MyNotifications       | List user notifications             |
| POST | `/profile/notifications/{id}/mark-read` | MarkNotificationRead  | Mark notification as read           |
| GET  | `/profile/reviews`                      | MyReviews             | List user reviews                   |
| POST | `/profile/reviews/create`               | CreateReview          | Submit a new review                 |
| GET  | `/profile/tickets`                      | MyTickets             | List user tickets                   |
| GET  | `/profile/tickets/{id}/print`           | PrintTicket           | Display printable ticket in new tab |

---

## Screenshots

### Home Page
*Displays event listings with search and pagination.*

![Home Page](docs/images/homepage.png)
![Home Page (page 2)](docs/images/homepage2.png)

### Register & Login
*User registration, login, email verification, and password reset flows.*

![Sign Up](docs/images/Signup.png)
![Login](docs/images/login.png)
![Verification Prompt](docs/images/verification.png)
![Verification Email](docs/images/verification-email.png)
![Forgot Password Form](docs/images/forgot-password.png)
![Forgot Password Email](docs/images/forgot-password-email.png)
![Reset Password Email](docs/images/reset-password-email.png)
![Reset Password Form](docs/images/reset-password.png)

### Profile
*Manage personal info, bookings, favorites, notifications, reviews, and tickets.*

![Profile](docs/images/profile.png)
![Edit Profile](docs/images/profile-edit.png)
![Change Password](docs/images/change-password.png)
![My Bookings](docs/images/my-bookings.png)
![My Favorites](docs/images/my-favorites.png)
![Notifications](docs/images/my-notifications.png)
![Reviews](docs/images/my-reviews.png)
![Tickets List](docs/images/my-tickets.png)
![Print Ticket](docs/images/print-ticket.png)

### Event Details
*Detailed event information and booking interface.*

![Event Overview](docs/images/event.png)
![Event Details & Booking](docs/images/event-details.png)
![Add/Edit Event](docs/images/AddEvent.png)

### Admin Dashboard
*Admin overview, CRUD for users/events/venues/categories/bookings, and audit logs.*

![Dashboard Overview](docs/images/admin-dashboard1.png)
![Users List](docs/images/all-users.png)
![User Details](docs/images/user-details.png)
![Events List](docs/images/event-details2.png)
![Event Details (Admin)](docs/images/event-details-admin.png)
![Bookings List](docs/images/all-bookings.png)
![Audit Log](docs/images/audit-log.png)
![Venue Management](docs/images/Venue.png)
![Category Management](docs/images/category.png)

### Booking Confirmation

![Booking Confirmation](docs/images/booking-view.png)
*Confirmation page after a successful booking.*

---
