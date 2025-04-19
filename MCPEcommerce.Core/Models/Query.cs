namespace MCPEcommerce.Core.Models
{
    public class Query
    {
        public int Id { get; set; }
        public string? Question { get; set; }
        public string? Answer { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ProcessedBy { get; set; }

        public Query()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
