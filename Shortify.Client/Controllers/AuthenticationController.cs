using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using Shortify.Client.Data.ViewModels;
using Shortify.Client.Helpers.Roles;
using Shortify.Data.Models;
using Shortify.Data.Services;
using System.Security.Claims;

namespace Shortify.Client.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserService _userService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        
        public AuthenticationController(IUserService userService, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }
        
        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> Login()
        {
            var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
            var loginVM = new LoginVM()
            {
                Password = string.Empty,
                Email = string.Empty,
                Schemes = schemes
            };

            return View(new LoginVM { Email = string.Empty, Password = string.Empty, Schemes = schemes });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginSubmitted(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", loginVM);
            }

            var user = await _userManager.FindByEmailAsync(loginVM.Email);

            if (user == null)
            {
                ModelState.AddModelError("Email", "No account found with this email.");
                return View("Login", loginVM);
            }

            // Check if user is already locked out before attempting sign-in
            if (await _userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Your account is locked. Please try again in 10 minutes.");
                return View("Login", loginVM);
            }

            // Enable lockout tracking by setting the 4th parameter to true
            var signInResult = await _signInManager.PasswordSignInAsync(user, loginVM.Password, 
                loginVM.RememberMe, lockoutOnFailure: true);

            if (signInResult.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account is locked due to multiple failed login attempts. Please try again in 10 minutes.");
                return View("Login", loginVM);
            }

            if (signInResult.RequiresTwoFactor)
            {
                return RedirectToAction("TwoFactorConfirmation", new { loggedInUserId = user.Id });
            }

            if (signInResult.IsNotAllowed)
            {
                return RedirectToAction("EmailConfirmation");
            }

            // Password was incorrect
            var failedAttempts = await _userManager.GetAccessFailedCountAsync(user);
            var maxAttempts = 3; // From your IdentityOptions configuration
            var attemptsRemaining = maxAttempts - failedAttempts;

            if (attemptsRemaining > 0)
            {
                ModelState.AddModelError("Password", $"Invalid password. You have {attemptsRemaining} attempt(s) remaining before your account is locked.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt. Please, check your username and password.");
            }

            return View("Login", loginVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterSubmitted(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", registerVM);
            }

            var userExists = await _userManager.FindByEmailAsync(registerVM.Email);
            if (userExists != null)
            {
                ModelState.AddModelError("", "Email address is already in use!");
                return View("Register", registerVM);
            }

            var newUser = new AppUser()
            {
                Email = registerVM.Email,
                UserName = registerVM.Email,
                FullName = registerVM.FullName,
                LockoutEnabled = true
            };

            var userCreated = await _userManager.CreateAsync(newUser, registerVM.Password);

            if (userCreated.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, Role.User);

                // Generate confirmation token
                var userToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                
                // Create the confirmation URL
                var confirmationLink = Url.Action(
                    "ConfirmEmail", 
                    "Authentication", 
                    new { userId = newUser.Id, token = userToken }, 
                    protocol: Request.Scheme);

                // Send confirmation email
                try
                {
                    var apiKey = _configuration["SendGrid:ShortifyKey"];
                    var sendGridClient = new SendGridClient(apiKey);
                    var fromEmailAddress = new EmailAddress(_configuration["SendGrid:FromAddress"], "Shortify Client App");
                    var emailSubject = "Account verification for Shortify Client App";
                    var toEmailAddress = new EmailAddress(registerVM.Email);
                    var emailContentTxt = $"Hello {registerVM.FullName}! Welcome to Shortify App. Please click the link to verify your account: {confirmationLink}";
                    var emailContentHtml = $"<strong>Hello {registerVM.FullName}!</strong><br/><br/>Welcome to Shortify App!<br/><br/>Please click the link below to verify your account:<br/><a href='{confirmationLink}'>Verify Account</a>";
                    var emailRequest = MailHelper.CreateSingleEmail(fromEmailAddress, toEmailAddress, emailSubject, emailContentTxt, emailContentHtml);

                    var response = await sendGridClient.SendEmailAsync(emailRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["EmailConfirmation"] = "Registration successful! Please check your email to verify your account.";
                    }
                    else
                    {
                        TempData["EmailConfirmation"] = "Registration successful! However, there was an issue sending the confirmation email. Please use the 'Resend Email' option.";
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception if you have logging configured
                    TempData["EmailConfirmation"] = "Registration successful! However, there was an issue sending the confirmation email. Please use the 'Resend Email' option.";
                }

                return RedirectToAction("EmailConfirmation");
            }

            // Add specific Identity errors to ModelState
            foreach (var error in userCreated.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Register", registerVM);
        }

        public async Task<IActionResult> Register()
        {
            return View(new RegisterVM());
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> EmailConfirmation()
        {
            var confirmEmail = new ConfirmEmailLoginVM();
            return View(confirmEmail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmailConfirmation(ConfirmEmailLoginVM confirmEmailLoginVM) 
        {
            // 1. Check if the user exists 
            var user = await _userManager.FindByEmailAsync(confirmEmailLoginVM.Email);

            if (user != null)
            {
                // Check if email is already confirmed
                if (await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "This email is already confirmed. You can log in.");
                    return View("EmailConfirmation", confirmEmailLoginVM);
                }

                // 2. Generate confirmation token
                var userToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                
                // Create the confirmation URL
                var confirmationLink = Url.Action(
                    "ConfirmEmail", 
                    "Authentication", 
                    new { userId = user.Id, token = userToken }, 
                    protocol: Request.Scheme);

                // 3. Send the email
                try
                {
                    var apiKey = _configuration["SendGrid:ShortifyKey"];
                    var sendGridClient = new SendGridClient(apiKey);
                    var fromEmailAddress = new EmailAddress(_configuration["SendGrid:FromAddress"], "Shortify Client App");
                    var emailSubject = "Account verification for Shortify Client App";
                    var toEmailAddress = new EmailAddress(confirmEmailLoginVM.Email);
                    var emailContentTxt = $"Hello from Shortify App! Please click the link to verify your account: {confirmationLink}";
                    var emailContentHtml = $"<strong>Hello from Shortify App!</strong><br/><br/>Please click the link below to verify your account:<br/><a href='{confirmationLink}'>Verify Account</a>";
                    var emailRequest = MailHelper.CreateSingleEmail(fromEmailAddress, toEmailAddress, emailSubject, emailContentTxt, emailContentHtml);

                    var response = await sendGridClient.SendEmailAsync(emailRequest);

                    // 4. Check response status - SendGrid returns 202 Accepted on success
                    if (!response.IsSuccessStatusCode)
                    {
                        // Log the error for debugging
                        var errorBody = await response.Body.ReadAsStringAsync();
                        
                        ModelState.AddModelError(string.Empty, "Failed to send confirmation email. Please try again later.");
                        return View("EmailConfirmation", confirmEmailLoginVM);
                    }

                    TempData["EmailConfirmation"] = "Thank you! Please check your email to verify your account.";
                    return View("EmailConfirmation", confirmEmailLoginVM);
                }
                catch (Exception ex)
                {
                    // Log the exception if you have logging configured
                    ModelState.AddModelError(string.Empty, "An error occurred while sending the email. Please try again later.");
                    return View("EmailConfirmation", confirmEmailLoginVM);
                }
            }

            ModelState.AddModelError("Email", "No account found with this email.");
            return View("EmailConfirmation", confirmEmailLoginVM);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Invalid email confirmation link.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["Success"] = "Email confirmed successfully! You can now log in.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = "Error confirming your email.";
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> TwoFactorConfirmation(string loggedInUserId)
        {
            var user = await _userManager.FindByIdAsync(loggedInUserId);

            if (user != null)
            {
                try
                {
                    // Email provider kullan (SMS yerine)
                    var userToken = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

                    // SendGrid ile email gönder
                    var apiKey = _configuration["SendGrid:ShortifyKey"];
                    var sendGridClient = new SendGridClient(apiKey);
                    var fromEmailAddress = new EmailAddress(_configuration["SendGrid:FromAddress"], "Shortify Client App");
                    var emailSubject = "Your Two-Factor Authentication Code";
                    var toEmailAddress = new EmailAddress(user.Email);
                    var emailContentTxt = $"Your verification code is: {userToken}\n\nThis code will expire in 5 minutes.";
                    var emailContentHtml = $@"
                <h2>Two-Factor Authentication</h2>
                <p>Your verification code is:</p>
                <h1 style='color: #007bff; font-size: 32px; letter-spacing: 5px;'>{userToken}</h1>
                <p>This code will expire in 5 minutes.</p>
                <p>If you didn't request this code, please ignore this email.</p>";

                    var emailRequest = MailHelper.CreateSingleEmail(
                        fromEmailAddress,
                        toEmailAddress,
                        emailSubject,
                        emailContentTxt,
                        emailContentHtml
                    );

                    var response = await sendGridClient.SendEmailAsync(emailRequest);

                    if (!response.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError(string.Empty, "Failed to send verification code.");
                        TempData["Error"] = "Unable to send verification email. Please try again.";
                        return RedirectToAction("Login");
                    }

                    var confirm2FALoginVM = new Confirm2FALoginVm()
                    {
                        UserId = loggedInUserId
                    };

                    TempData["Info"] = "A verification code has been sent to your email address.";
                    return View(confirm2FALoginVM);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while sending verification code.");
                    TempData["Error"] = "An error occurred. Please try again.";
                    return RedirectToAction("Login");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm2FALogin(Confirm2FALoginVm confirm2FALoginVm)
        {
            var user = await _userManager.FindByIdAsync(confirm2FALoginVm.UserId);
            if(user != null)
            {
                // Changed from "Phone" to "Email"
                var is2FATokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", confirm2FALoginVm.UserConfirmationCode);
                if(is2FATokenValid)
                {
                    // Changed from "Phone" to "Email"
                    var tokenSignIn = await _signInManager.TwoFactorSignInAsync("Email", confirm2FALoginVm.UserConfirmationCode, 
                        false, false);

                    if(tokenSignIn.Succeeded)
                    {
                        return RedirectToAction("Index","Home");
                    }
                }
                ModelState.AddModelError("TwoFactorCode", "Invalid two-factor authentication code.");
                return View("TwoFactorConfirmation", confirm2FALoginVm);
            }
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> TwoFactorConfirmationVerified(Confirm2FALoginVm confirm2FALoginVm) 
        {
            var user = await _userManager.FindByIdAsync(confirm2FALoginVm.UserId); 

            if(user != null)
            {
                // Changed from "Phone" to "Email"
                var tokenVerification = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", confirm2FALoginVm.UserConfirmationCode);
            
                if(tokenVerification)
                {
                    // Changed from "Phone" to "Email"
                    var tokenSignIn = await _signInManager.TwoFactorSignInAsync("Email", confirm2FALoginVm.UserConfirmationCode,
                        false, false);
                    if(tokenSignIn.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ModelState.AddModelError("", "Confirmation code is not correct!");
            return View(confirm2FALoginVm);
        }

        public IActionResult ExternalLogin(string provider,string returnUrl = "")
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Authentication", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "", string remoteError = "")
        {
            var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
            var loginVM = new LoginVM()
            {
                Password = string.Empty,
                Email = string.Empty,
                Schemes = schemes
            };

            if (!string.IsNullOrEmpty(remoteError))
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();

            if(info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information.");
                return View("Login");
            }

            var signinResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, 
                isPersistent: false, bypassTwoFactor : true);

            if(signinResult.Succeeded) {
                return RedirectToAction("Index", "Home");
            } else
            {
                var userEmail = info.Principal.FindFirstValue(ClaimTypes.Email);

                if(!string.IsNullOrEmpty(userEmail)) {

                    var user = _userManager.FindByEmailAsync(userEmail).Result;
                    if(user == null)
                    {
                        user = new AppUser()
                        {
                            UserName = userEmail,
                            Email = userEmail,
                            EmailConfirmed = true
                        };
                        var createdResult = await _userManager.CreateAsync(user);
                        await _userManager.AddToRoleAsync(user, Role.User);
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
            }
                ModelState.AddModelError(string.Empty, "Error during external login.");
            return View("Login",loginVM);
        }
    }
}
