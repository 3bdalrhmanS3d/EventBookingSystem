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

   ```powershell
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

4. **User Secrets**

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

5. **Run the application**

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

## Screenshots

### Home Page

![Home Page](docs/images/homepage.png)
![Home Page](docs/images/homepage2.png)]
*Displays event listings with search and pagination.*

### Register/Login

![SignUp](docs/images/Signup.png)
![Login](docs/images/login.png)]
![Verfication](docs/images/verification.png)
![Verification Email](docs/images/verification-email.png)
![Forgot Password](docs/images/forgot-password.png)
![Forgot Password](docs/images/forgot-password-email.png)
![Reset Password](docs/images/reset-password-email.png)
![Reset Password](docs/images/reset-password.png)
*User registration, login, email verification, and password reset forms.*

### Profile

![Profile](docs/images/profile.png)
![Profile Edit](docs/images/profile-edit.png)
![Change Password](docs/images/change-password.png)
![My Bookings](docs/images/my-bookings.png)
![My Favorites](docs/images/my-favorites.png)
![My Notifications](docs/images/my-notifications.png)
![My Reviews](docs/images/my-reviews.png)
![My Tickets](docs/images/my-tickets.png)
![Print Ticket](docs/images/print-ticket.png)
*User profile management, including editing personal information, changing password, viewing bookings, favorites, notifications, reviews, and tickets.*

### Event Details

![Event](docs/images/event.png)
![Event Details](docs/images/event-details.png)
![Add Event](docs/images/AddEvent.png)
*Shows detailed information and booking button.*

### Admin Dashboard

![Admin Dashboard](docs/images/admin-dashboard1.png)
![All Users](docs/images/all-users.png)
![Details](docs/images/user-details.png)
![All Events](docs/images/event-details2.png)
![Event Details](docs/images/event-details-admin.png)
![All Bookings](docs/images/all-bookings.png)
![Audit Log](docs/images/audit-log.png)
![Venue](docs/images/Venue.png)
![Category](docs/images/category.png)

*Overview of counts for users, events, bookings, and pending verifications. CRUD operations for users, events, venues, categories, and bookings. Audit log for all actions.*

### Booking Confirmation

![Booking View](docs/images/booking-view.png)
*Confirmation page after successful booking.*

## Endpoints

Below is the full list of available controller actions, grouped by controller and ordered by route:

### AccountController

| HTTP Verb | Route                        | Action Method                 | Description                     |
| --------- | ---------------------------- | ----------------------------- | ------------------------------- |
| GET       | /account/signup              | SignUp (GET)                  | Display signup form             |
| POST      | /account/signup              | SignUp (POST)                 | Process user registration       |
| GET       | /account/verify-email        | VerifyEmail (GET)             | Display email verification form |
| POST      | /account/verify-email        | VerifyEmail (POST)            | Process email activation code   |
| GET       | /account/signin              | SignIn (GET)                  | Display login form              |
| POST      | /account/signin              | SignIn (POST)                 | Authenticate user and sign in   |
| GET       | /account/forgot-password     | ForgotPassword (GET)          | Display forgot password form    |
| POST      | /account/forgot-password     | ForgotPassword (POST)         | Send reset link via email       |
| GET       | /account/reset-password      | ResetPassword (GET)           | Display reset password form     |
| POST      | /account/reset-password      | ResetPassword (POST)          | Process new password submission |
| GET       | /account/resend-verification | ResendVerificationEmail (GET) | Resend activation email         |
| POST      | /account/signout             | Logout (POST)                 | Sign out user                   |

### AdminController

| HTTP Verb | Route                        | Action Method  | Description                          |
| --------- | ---------------------------- | -------------- | ------------------------------------ |
| GET       | /admin/dashboard             | Index          | Display admin dashboard              |
| GET       | /admin/users                 | Users          | List users with search and paging    |
| GET       | /admin/users/{id}            | UserDetails    | Display user details                 |
| POST      | /admin/users/{id}/activate   | ActivateUser   | Activate a user account              |
| POST      | /admin/users/{id}/deactivate | DeactivateUser | Deactivate a user account            |
| POST      | /admin/users/{id}/delete     | DeleteUser     | Delete a user                        |
| GET       | /admin/events                | Events         | List events with search and paging   |
| GET       | /admin/events/{id}           | EventDetails   | Display event details                |
| POST      | /admin/events/{id}/delete    | DeleteEvent    | Delete an event                      |
| GET       | /admin/bookings              | Bookings       | List bookings with search and paging |
| POST      | /admin/bookings/{id}/delete  | DeleteBooking  | Delete a booking                     |
| GET       | /admin/audit                 | Audit          | List audit log entries               |
| GET       | /admin/audit/{id}/details    | AuditDetails   | Display audit log entry details      |

### EventController

| HTTP Verb | Route               | Action Method       | Description                    |
| --------- | ------------------- | ------------------- | ------------------------------ |
| GET       | /events             | Index (AdminEvents) | List events for admin          |
| GET       | /events/all         | Events (UserEvents) | List public events             |
| GET       | /events/create      | Create (GET)        | Display event creation form    |
| POST      | /events/create      | Create (POST)       | Process new event              |
| GET       | /events/{id}        | Details             | Display event details (public) |
| GET       | /events/edit/{id}   | Edit (GET)          | Display event edit form        |
| POST      | /events/edit/{id}   | Edit (POST)         | Process event update           |
| POST      | /events/delete/{id} | Delete              | Delete an event                |

### CategoryController

| HTTP Verb | Route                   | Action Method         | Description                    |
| --------- | ----------------------- | --------------------- | ------------------------------ |
| GET       | /categories             | Index                 | List categories                |
| GET       | /categories/create      | CreateCategory (GET)  | Display category creation form |
| POST      | /categories/create      | CreateCategory (POST) | Process new category           |
| GET       | /categories/edit/{id}   | EditCategory (GET)    | Display category edit form     |
| POST      | /categories/edit/{id}   | EditCategory (POST)   | Process category update        |
| POST      | /categories/delete/{id} | DeleteCategory        | Delete a category              |
| GET       | /categories/{id}        | Details               | Display category details       |

### VenueController

| HTTP Verb | Route               | Action Method      | Description                 |
| --------- | ------------------- | ------------------ | --------------------------- |
| GET       | /venues             | Index              | List venues                 |
| GET       | /venues/create      | CreateVenue (GET)  | Display venue creation form |
| POST      | /venues/create      | CreateVenue (POST) | Process new venue           |
| GET       | /venues/edit/{id}   | EditVenue (GET)    | Display venue edit form     |
| POST      | /venues/edit/{id}   | EditVenue (POST)   | Process venue update        |
| POST      | /venues/delete/{id} | DeleteVenue        | Delete a venue              |
| GET       | /venues/{id}        | Details            | Display venue details       |

### BookingController

| HTTP Verb | Route                      | Action Method | Description                        |
| --------- | -------------------------- | ------------- | ---------------------------------- |
| GET       | /booking/create/{eventId}  | Create (GET)  | Display booking form for event     |
| POST      | /booking/create/{eventId}  | Create (POST) | Process booking and create tickets |
| GET       | /booking/confirmation/{id} | Confirmation  | Display booking confirmation       |

### ProfileController

| HTTP Verb | Route                                 | Action Method         | Description                              |
| --------- | ------------------------------------- | --------------------- | ---------------------------------------- |
| GET       | /profile                              | Index                 | Display user profile                     |
| GET       | /profile/edit                         | Edit (GET)            | Display profile edit form                |
| POST      | /profile/edit                         | Edit (POST)           | Process profile updates                  |
| GET       | /profile/change-password              | ChangePassword (GET)  | Display change password form             |
| POST      | /profile/change-password              | ChangePassword (POST) | Process password change                  |
| GET       | /profile/bookings                     | MyBookings            | List user's bookings                     |
| POST      | /profile/bookings/{id}/cancel         | CancelBooking         | Cancel a booking                         |
| GET       | /profile/favorites                    | MyFavorites           | List favorite events                     |
| POST      | /profile/favorites/toggle/{eventId}   | ToggleFavorite        | Add or remove favorite                   |
| GET       | /profile/notifications                | MyNotifications       | List user notifications                  |
| POST      | /profile/notifications/{id}/mark-read | MarkNotificationRead  | Mark notification as read                |
| GET       | /profile/reviews                      | MyReviews             | List user reviews                        |
| POST      | /profile/reviews/create               | CreateReview          | Submit a new review                      |
| GET       | /profile/tickets                      | MyTickets             | List user tickets                        |
| GET       | /profile/tickets/{id}/print           | PrintTicket           | Display printable ticket view in new tab |

---
