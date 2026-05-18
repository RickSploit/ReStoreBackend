namespace ReStore.API.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int ReviewedUserId { get; set; }
        public string ReviewedUserName { get; set; } = string.Empty;
        public int? ApplianceId { get; set; }
        public string? ApplianceTitle { get; set; }
    }

    public class CreateReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int ReviewedUserId { get; set; }
        public int? ApplianceId { get; set; }
    }

    public class UpdateReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
