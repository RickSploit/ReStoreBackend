using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ReStore.Application.Interfaces;
using ReStore.Core.Entities;
using ReStore.DTOs;
using ReStore.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ReStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<User> userManager, 
            SignInManager<User> signInManager, 
            IConfiguration config,
            IEmailService emailService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _emailService = emailService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null) return BadRequest("Email is already registered.");

            var user = new User
            {
                Name = model.Name,
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.Phone
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            // Generate email confirmation token
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // Send confirmation email
            await _emailService.SendConfirmationEmailAsync(user.Email, user.Id.ToString(), confirmationToken);

            return Ok(new { 
                Message = "User registered successfully! Please check your email to confirm your account.",
                requireEmailConfirmation = true
            });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(new { message = "User ID and token are required." });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            
            if (result.Succeeded)
            {
                return Ok(new { Message = "Email confirmed successfully! You can now log in." });
            }
            
            return BadRequest(new { message = "Invalid confirmation token." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized("Invalid Email or Password.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded) return Unauthorized("Invalid Email or Password.");

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                return Unauthorized(new { 
                    message = "Please confirm your email before logging in.",
                    requireEmailConfirmation = true 
                });
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Check if user is a Technician and if they are approved
            if (roles.Contains("Technician"))
            {
                var profile = _context.TechnicianProfiles.FirstOrDefault(t => t.User.Id == user.Id);
                if (profile != null && !profile.IsApproved)
                {
                    return Unauthorized(new { 
                        message = "Your account is pending admin approval.",
                        requireApproval = true 
                    });
                }
            }

            var token = GenerateJwtToken(user, roles);

            return Ok(new
            {
                Message = "Login Successful",
                Token = token,
                User = new { user.Id, user.Name, user.Email, Roles = roles }
            });
        }

        private string GenerateJwtToken(User user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // قراءة البيانات بنفس الأسماء اللي في appsettings.json مع وضع قيمة افتراضية للحماية
            var jwtKey = _config["Jwt:Key"] ?? "ThisIsASuperSecretKeyForReStoreAppThatNeedsToBeLongEnough123!";
            var jwtIssuer = _config["Jwt:Issuer"] ?? "http://localhost:5104";
            var jwtAudience = _config["Jwt:Audience"] ?? "http://localhost:5104";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}