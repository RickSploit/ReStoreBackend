namespace ReStore.API.DTOs
{
    public class ComplaintDto
    {
        public int Id { get; set; }
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
public int UserId { get; set; }
        public int OrderId { get; set; }
    }

    public class CreateComplaintDto
    {
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OrderId { get; set; }
    }

    public class UpdateComplaintDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
