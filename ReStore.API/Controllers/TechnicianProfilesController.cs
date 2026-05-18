using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.API.DTOs;
using ReStore.API.Entities;
using ReStore.Core.Entities;
using ReStore.Infrastructure.Data;
using System.Security.Claims;

namespace ReStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TechnicianProfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TechnicianProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Get approved technicians
        [HttpGet]
        public async Task<IActionResult> GetApprovedTechnicians()
        {
            var technicians = await _context.TechnicianProfiles
                .Include(t => t.User)
                .Where(t => t.IsApproved)
                .ToListAsync();

            var dtos = technicians.Select(t => new TechnicianProfileDto
            {
                Id = t.Id,
                UserId = t.UserId,
                Name = t.User?.Name ?? string.Empty,
                Rating = t.Rating,
                Specialty = t.Specialty,
                ExperienceYears = t.ExperienceYears,
                IsApproved = t.IsApproved
            }).ToList();

            return Ok(dtos);
        }

        // GET: Get specific technician profile
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTechnicianProfile(int id)
        {
            var profile = await _context.TechnicianProfiles
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (profile == null)
                return NotFound(new { message = "Technician profile not found." });

            var dto = new TechnicianProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                Name = profile.User?.Name ?? string.Empty,
                Rating = profile.Rating,
                Specialty = profile.Specialty,
                ExperienceYears = profile.ExperienceYears,
                IsApproved = profile.IsApproved
            };

            return Ok(dto);
        }

        // POST: Create a technician profile (register as technician)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTechnicianProfile([FromBody] CreateTechnicianProfileDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            // Check if profile already exists
            var existing = await _context.TechnicianProfiles
                .FirstOrDefaultAsync(t => t.UserId == userId.Value);
            
            if (existing != null)
                return BadRequest(new { message = "Technician profile already exists." });

            var profile = new TechnicianProfile
            {
                UserId = userId.Value,
                Specialty = dto.Specialty,
                ExperienceYears = dto.ExperienceYears,
                Rating = 0, // Start with no rating
                IsApproved = false // Requires admin approval
            };

            _context.TechnicianProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Technician profile created! Waiting for admin approval.", profileId = profile.Id });
        }

        // PUT: Update own technician profile
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTechnicianProfile(int id, [FromBody] UpdateTechnicianProfileDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var profile = await _context.TechnicianProfiles.FindAsync(id);
            if (profile == null)
                return NotFound(new { message = "Technician profile not found." });

            // Only the owner can update
            if (profile.UserId != userId.Value)
                return Forbid();

            profile.Specialty = dto.Specialty;
            profile.ExperienceYears = dto.ExperienceYears;

            _context.TechnicianProfiles.Update(profile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Technician profile updated successfully!" });
        }

        // PUT: Admin approves technician
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveTechnician(int id)
        {
            var profile = await _context.TechnicianProfiles
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (profile == null)
                return NotFound(new { message = "Technician profile not found." });

            profile.IsApproved = true;
            profile.ApprovedDate = DateTime.UtcNow;

            _context.TechnicianProfiles.Update(profile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Technician approved successfully!" });
        }

        // GET: Admin get pending technician requests
        [HttpGet("admin/pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingTechnicians()
        {
            var technicians = await _context.TechnicianProfiles
                .Include(t => t.User)
                .Where(t => !t.IsApproved)
                .ToListAsync();

            var dtos = technicians.Select(t => new TechnicianProfileDto
            {
                Id = t.Id,
                UserId = t.UserId,
                Name = t.User?.Name ?? string.Empty,
                Rating = t.Rating,
                Specialty = t.Specialty,
                ExperienceYears = t.ExperienceYears,
                IsApproved = t.IsApproved
            }).ToList();

            return Ok(dtos);
        }

        // GET: Admin get all technicians
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTechnicians()
        {
            var technicians = await _context.TechnicianProfiles
                .Include(t => t.User)
                .ToListAsync();

            var dtos = technicians.Select(t => new TechnicianProfileDto
            {
                Id = t.Id,
                UserId = t.UserId,
                Name = t.User?.Name ?? string.Empty,
                Rating = t.Rating,
                Specialty = t.Specialty,
                ExperienceYears = t.ExperienceYears,
                IsApproved = t.IsApproved
            }).ToList();

            return Ok(dtos);
        }

        // Helper method to get current user ID
        private int? GetCurrentUserId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
