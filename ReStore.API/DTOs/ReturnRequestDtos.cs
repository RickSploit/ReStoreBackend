namespace ReStore.API.DTOs
{
    public class ReturnRequestCreateDto
    {
        public int OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ReturnRequestDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? RefundAmount { get; set; }
        public string? AdminNote { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
    }

    public class ReturnRequestUpdateDto
    {
        public string Status { get; set; } = string.Empty;
        public decimal? RefundAmount { get; set; }
        public string? AdminNote { get; set; }
    }
}
