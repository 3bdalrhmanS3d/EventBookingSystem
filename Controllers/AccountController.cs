using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using EventBookingSystemV1.Data;
using EventBookingSystemV1.DTOs;
using EventBookingSystemV1.Models;
using EventBookingSystemV1.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventBookingSystemV1.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly EmailQueueService _emailQueue;
        private readonly IJwtService _jwtService;

        public AccountController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher, EmailQueueService emailQueue, IJwtService jwtService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _emailQueue = emailQueue;
            _jwtService = jwtService;
        }

        // Display signup form
        [HttpGet("signup")]
        public IActionResult SignUp()
        {
            return View();
        }
        // POST: /Account/SignUp
        /// <summary>
        /// Registers a new user, creates an activation token, and emails the code.
        /// </summary>

        [HttpPost("signup")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpDto dto)
        {
            if (!dto.AcceptTerms)
                ModelState.AddModelError(nameof(dto.AcceptTerms), "You must agree to the terms.");

            if (await _context.Users.AnyAsync(u => u.Email == dto.EmailAddress))
                ModelState.AddModelError(nameof(dto.EmailAddress), "Email is already in use.");

            if (!ModelState.IsValid)
                return View(dto);

            var user = new User
            {
                FullName = $"{dto.FirstName.Trim()} {dto.LastName.Trim()}",
                Email = dto.EmailAddress,
                BirthDate = dto.BirthDate,
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = false,
                IsDeleted = false,
                Role = UserRole.User
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate 7-digit numeric activation code
            var rnd = new Random();
            string code = rnd.Next(1000000, 10000000).ToString();

            var activationToken = new AccountActivation
            {
                UserId = user.Id,
                Code = code,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiryDate = DateTimeOffset.UtcNow.AddHours(6),
                IsUsed = false
            };
            _context.AccountActivation.Add(activationToken);
            await _context.SaveChangesAsync();

            _emailQueue.QueueVerificationEmail(
                user.Email,
                user.FullName,
                activationToken.Code,
                isResend: false);

            Response.Cookies.Append("pending_email", user.Email, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = activationToken.ExpiryDate
            });

            return RedirectToAction("VerifyEmail", new { email = user.Email });
        }

        // Display verification form
        [HttpGet("verify-email")]
        public IActionResult VerifyEmail(string email)
        {
            var model = new VerifyEmailDto { EmailAddress = email };
            return View(model);
        }

        // POST: /account/verify-email
        /// <summary>
        /// Processes the verification code, activates the user account, and redirects to SignIn.
        /// </summary>
        [HttpPost("verify-email")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
        {
            // 1) Model validation
            if (!ModelState.IsValid)
                return View(dto);

            // 2) Determine email: form field or pending_email cookie
            var email = dto.EmailAddress;
            if (string.IsNullOrEmpty(email)
                && Request.Cookies.TryGetValue("pending_email", out var cookieEmail))
            {
                email = cookieEmail;
            }

            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(nameof(dto.EmailAddress), "Email is required to verify the account.");
                return View(dto);
            }

            // 3) Lookup activation token
            var tokenEntry = await _context.AccountActivation
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.User.Email == email && t.Code == dto.Token);

            if (tokenEntry == null)
            {
                ModelState.AddModelError(nameof(dto.Token), "Invalid activation code.");
                return View(dto);
            }
            if (tokenEntry.IsUsed)
            {
                ModelState.AddModelError(nameof(dto.Token), "Activation code has already been used.");
                return View(dto);
            }
            if (DateTimeOffset.UtcNow > tokenEntry.ExpiryDate)
            {
                ModelState.AddModelError(nameof(dto.Token), "Activation code has expired.");
                return View(dto);
            }

            // 4) Activate account
            tokenEntry.IsUsed = true;
            tokenEntry.UsedAt = DateTimeOffset.UtcNow;
            tokenEntry.User.IsActive = true;
            await _context.SaveChangesAsync();

            // 5) Clear pending_email cookie
            Response.Cookies.Delete("pending_email");

            // 6) Redirect to login
            return RedirectToAction("SignIn");
        }

        // GET: /account/signin
        [HttpGet("signin")]
        public IActionResult SignIn() => View();

        // Post /account/signin 
        [HttpPost("signin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInDto dto , string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.EmailAddress);
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials.");
                return View(dto);
            }
            if (!user.IsActive)
            {
                // Resend activation
                var rnd = new Random();
                var code = rnd.Next(1000000, 10000000).ToString();
                var activationToken = new AccountActivation
                {
                    UserId = user.Id,
                    Code = code,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ExpiryDate = DateTimeOffset.UtcNow.AddHours(6),
                    IsUsed = false
                };
                _context.AccountActivation.Add(activationToken);
                await _context.SaveChangesAsync();

                _emailQueue.QueueVerificationEmail(user.Email, user.FullName, code, isResend: true);
                Response.Cookies.Append("pending_email", user.Email, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = activationToken.ExpiryDate
                });
                return RedirectToAction("VerifyEmail", new { email = user.Email });
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,           user.FullName),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           user.Role.ToString()),
                new Claim(ClaimTypes.DateOfBirth, user.BirthDate.ToString("yyyy-MM-dd"))
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var props = new AuthenticationProperties
            {
                IsPersistent = dto.RememberMe,
                ExpiresUtc = dto.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(15)
                    : (DateTimeOffset?)null
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                props);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // GET: /account/forgot-password
        [HttpGet("forgot-password")]
        public IActionResult ForgotPassword() => View();

        // POST: /account/forgot-password
        [HttpPost("forgot-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.EmailAddress);
            if (user != null)
            {
                var token = Guid.NewGuid().ToString("N");
                var resetToken = new PasswordResetToken
                {
                    UserId = user.Id,
                    Token = token,
                    ExpiryDate = DateTimeOffset.UtcNow.AddHours(6),
                    IsUsed = false
                };
                _context.PasswordResetTokens.Add(resetToken);
                await _context.SaveChangesAsync();

                var resetLink = Url.Action("ResetPassword", "Account",
                    new { email = user.Email, token = token }, Request.Scheme);
                _emailQueue.QueueResetPasswordEmail(user.Email, user.FullName, resetLink);
            }
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        // GET: /account/forgot-password-confirmation
        [HttpGet("forgot-password-confirmation")]
        public IActionResult ForgotPasswordConfirmation() => View();

        // GET: /account/reset-password
        [HttpGet("reset-password")]
        public IActionResult ResetPassword(string email, string token)
        {
            var model = new ResetPasswordDto { EmailAddress = email, Token = token };
            return View(model);
        }
        // POST: /account/reset-password
        [HttpPost("reset-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var entry = await _context.PasswordResetTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.User.Email == dto.EmailAddress && r.Token == dto.Token);
            if (entry == null || entry.IsUsed || DateTimeOffset.UtcNow > entry.ExpiryDate)
            {
                ModelState.AddModelError(nameof(dto.Token), "Invalid or expired reset token.");
                return View(dto);
            }
            entry.IsUsed = true;
            entry.ExpiryDate = DateTimeOffset.UtcNow;
            entry.User.PasswordHash = _passwordHasher.HashPassword(entry.User, dto.NewPassword);
            await _context.SaveChangesAsync();
            return RedirectToAction("SignIn");
        }

        // GET: /account/resend-verification
        [HttpGet("resend-verification")]
        public async Task<IActionResult> ResendVerificationEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound();

            // generate a new 7-digit code
            var rnd = new Random();
            var code = rnd.Next(1000000, 10000000).ToString();

            var activation = new AccountActivation
            {
                UserId = user.Id,
                Code = code,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiryDate = DateTimeOffset.UtcNow.AddHours(6),
                IsUsed = false
            };
            _context.AccountActivation.Add(activation);
            await _context.SaveChangesAsync();

            // enqueue a *resend* email
            _emailQueue.QueueVerificationEmail(user.Email, user.FullName, code, isResend: true);

            // reset the pending_email cookie so the VerifyEmail page still knows which user
            Response.Cookies.Append("pending_email", user.Email, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = activation.ExpiryDate
            });

            // back to the same verify page
            TempData["SuccessMessage"] = "A new verification code has been sent to your inbox.";
            return RedirectToAction("VerifyEmail", new { email });
        }

        // POST: /account/signout
        [HttpPost("signout")]
        [ValidateAntiForgeryToken]
        public IActionResult SignOut()
        {
            Response.Cookies.Delete("jwt_token");

            return RedirectToAction("SignIn");
        }
    }
}
