using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.API.DTOs;
using ReStore.Core.Entities;
using ReStore.Infrastructure.Data;
using System.Security.Claims;

namespace ReStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CertificatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CertificatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Get certificates for an appliance
        [HttpGet("appliance/{applianceId}")]
        public async Task<IActionResult> GetApplianceCertificates(int applianceId)
        {
            var certificates = await _context.Certificates
                .Include(c => c.Appliance)
                .Include(c => c.Technician)
                .Where(c => c.ApplianceId == applianceId)
                .ToListAsync();

            var dtos = certificates.Select(c => new CertificateDto
            {
                Id = c.Id,
                IssueDate = c.IssueDate,
                WarrantyMonths = c.WarrantyMonths,
                EvaluationDetails = c.EvaluationDetails,
                ApplianceId = c.ApplianceId,
                ApplianceTitle = c.Appliance?.Title,
                TechnicianId = c.TechnicianId,
                TechnicianName = c.Technician?.Name ?? string.Empty
            }).ToList();

            return Ok(dtos);
        }

        // GET: Get specific certificate
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCertificate(int id)
        {
            var certificate = await _context.Certificates
                .Include(c => c.Appliance)
                .Include(c => c.Technician)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (certificate == null)
                return NotFound(new { message = "Certificate not found." });

            var dto = new CertificateDto
            {
                Id = certificate.Id,
                IssueDate = certificate.IssueDate,
                WarrantyMonths = certificate.WarrantyMonths,
                EvaluationDetails = certificate.EvaluationDetails,
                ApplianceId = certificate.ApplianceId,
                ApplianceTitle = certificate.Appliance?.Title,
                TechnicianId = certificate.TechnicianId,
                TechnicianName = certificate.Technician?.Name ?? string.Empty
            };

            return Ok(dto);
        }

        // POST: Create a certificate (Technician only)
        [HttpPost]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<IActionResult> CreateCertificate([FromBody] CreateCertificateDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var appliance = await _context.Appliances.FindAsync(dto.ApplianceId);
            if (appliance == null)
                return NotFound(new { message = "Appliance not found." });

            var certificate = new Certificate
            {
                ApplianceId = dto.ApplianceId,
                TechnicianId = userId.Value,
                WarrantyMonths = dto.WarrantyMonths,
                EvaluationDetails = dto.EvaluationDetails,
                IssueDate = DateTime.UtcNow
            };

            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Certificate created successfully!", certificateId = certificate.Id });
        }

        // PUT: Update a certificate
        [HttpPut("{id}")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<IActionResult> UpdateCertificate(int id, [FromBody] UpdateCertificateDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null)
                return NotFound(new { message = "Certificate not found." });

            // Only the issuing technician or admin can update
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && certificate.TechnicianId != userId.Value)
                return Forbid();

            certificate.WarrantyMonths = dto.WarrantyMonths;
            certificate.EvaluationDetails = dto.EvaluationDetails;

            _context.Certificates.Update(certificate);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Certificate updated successfully!" });
        }

        // DELETE: Delete a certificate
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCertificate(int id)
        {
            var certificate = await _context.Certificates.FindAsync(id);
            if (certificate == null)
                return NotFound(new { message = "Certificate not found." });

            _context.Certificates.Remove(certificate);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Certificate deleted successfully!" });
        }

        // GET: Admin get all certificates
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCertificates()
        {
            var certificates = await _context.Certificates
                .Include(c => c.Appliance)
                .Include(c => c.Technician)
                .ToListAsync();

            var dtos = certificates.Select(c => new CertificateDto
            {
                Id = c.Id,
                IssueDate = c.IssueDate,
                WarrantyMonths = c.WarrantyMonths,
                EvaluationDetails = c.EvaluationDetails,
                ApplianceId = c.ApplianceId,
                ApplianceTitle = c.Appliance?.Title,
                TechnicianId = c.TechnicianId,
                TechnicianName = c.Technician?.Name ?? string.Empty
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
