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
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Get reviews for a user
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserReviews(int userId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.ReviewedUserId == userId)
                .ToListAsync();

            var reviewDtos = reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                ReviewerId = r.ReviewerId,
                ReviewerName = r.Reviewer?.Name ?? string.Empty,
                ReviewedUserId = r.ReviewedUserId,
                ApplianceId = r.ApplianceId,
                ApplianceTitle = r.Appliance?.Title
            }).ToList();

            return Ok(reviewDtos);
        }

        // GET: Get reviews for an appliance
        [HttpGet("appliance/{applianceId}")]
        public async Task<IActionResult> GetApplianceReviews(int applianceId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                .Where(r => r.ApplianceId == applianceId)
                .ToListAsync();

            var reviewDtos = reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                ReviewerId = r.ReviewerId,
                ReviewerName = r.Reviewer?.Name ?? string.Empty,
                ReviewedUserId = r.ReviewedUserId,
                ApplianceId = r.ApplianceId,
                ApplianceTitle = r.Appliance?.Title
            }).ToList();

            return Ok(reviewDtos);
        }

        // GET: Get specific review
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReview(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.ReviewedUser)
                .Include(r => r.Appliance)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
                return NotFound(new { message = "Review not found." });

            var reviewDto = new ReviewDto
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewerId = review.ReviewerId,
                ReviewerName = review.Reviewer?.Name ?? string.Empty,
                ReviewedUserId = review.ReviewedUserId,
                ReviewedUserName = review.ReviewedUser?.Name ?? string.Empty,
                ApplianceId = review.ApplianceId,
                ApplianceTitle = review.Appliance?.Title
            };

            return Ok(reviewDto);
        }

        // POST: Create a review
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            // Verify the reviewed user exists
            var reviewedUser = await _context.Users.FindAsync(dto.ReviewedUserId);
            if (reviewedUser == null)
                return NotFound(new { message = "User to be reviewed not found." });

            // Can't review yourself
            if (dto.ReviewedUserId == userId.Value)
                return BadRequest(new { message = "You cannot review yourself." });

            var review = new Review
            {
                Rating = dto.Rating,
                Comment = dto.Comment,
                ReviewerId = userId.Value,
                ReviewedUserId = dto.ReviewedUserId,
                ApplianceId = dto.ApplianceId
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Review created successfully!", reviewId = review.Id });
        }

        // PUT: Update a review
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound(new { message = "Review not found." });

            // Only the reviewer can update their review
            if (review.ReviewerId != userId.Value)
                return Forbid();

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;

            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Review updated successfully!" });
        }

        // DELETE: Delete a review
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token: User ID is missing." });

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound(new { message = "Review not found." });

            // Only the reviewer or admin can delete the review
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && review.ReviewerId != userId.Value)
                return Forbid();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Review deleted successfully!" });
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
