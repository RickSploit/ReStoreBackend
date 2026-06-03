using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.API.Entities;
using ReStore.Infrastructure.Data;

namespace ReStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public record DashboardStatsDto
        {
            public int TotalUsers { get; init; }
            public int TotalAppliances { get; init; }
            public int TotalOrders { get; init; }
            public int TotalRepairRequests { get; init; }
        }


        /// <summary>
        /// Returns dashboard statistics (Admin only)
        /// </summary>
        [HttpGet("dashboard-stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalAppliances = await _context.Appliances.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalRepairRequests = await _context.RepairRequests.CountAsync();

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalAppliances = totalAppliances,
                TotalOrders = totalOrders,
                TotalRepairRequests = totalRepairRequests
            };
        }

        /// <summary>
        /// Returns pending technician profiles (Admin only)
        /// </summary>
        [HttpGet("pending-technicians")]
        public async Task<IActionResult> GetPendingTechnicians()
        {
            var pendingTechnicians = await _context.TechnicianProfiles
                .Include(t => t.User)
                .Where(t => !t.IsApproved)
                .Select(t => new
                {
                    ProfileId = t.Id,
                    UserId = t.UserId,
                    t.User.Name,
                    t.User.Email,
                    t.Specialty,
                    t.ExperienceYears,
                    t.Rating
                })
                .ToListAsync();

            return Ok(pendingTechnicians);
        }

        /// <summary>
        /// Approve a technician account (Admin only)
        /// </summary>
        [HttpPut("approve-technician/{id}")]
        public async Task<IActionResult> ApproveTechnician(int id)
        {
            var profile = await _context.TechnicianProfiles.FindAsync(id);
            if (profile == null)
                return NotFound(new { message = "Technician profile not found." });

            profile.IsApproved = true;
            profile.ApprovedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Technician account approved successfully!" });
        }




        /// <summary>
        /// Reject a technician account (Admin only)
        /// </summary>
        [HttpPut("reject-technician/{id}")]
        public async Task<IActionResult> RejectTechnician(int id)
        {
            var profile = await _context.TechnicianProfiles.FindAsync(id);
            if (profile == null)
                return NotFound(new { message = "Technician profile not found." });

            // Optionally: mark as rejected or delete
            profile.IsApproved = false;

            _context.TechnicianProfiles.Update(profile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Technician account rejected." });
        }



        /// <summary>
        /// Get all approved technicians (Admin only)
        /// </summary>
        [HttpGet("approved-technicians")]
        public async Task<IActionResult> GetApprovedTechnicians()
        {
            var approvedTechnicians = await _context.TechnicianProfiles
                .Include(t => t.User)
                .Where(t => t.IsApproved)
                .ToListAsync();

return Ok(approvedTechnicians.Select(t => new
            {
                ProfileId = t.Id,
                UserId = t.User.Id,
                t.User.Name,
                t.User.Email,
                t.Specialty,
                t.ExperienceYears,
                t.Rating,
                t.ApprovedDate
            }));
        }
    }
}
