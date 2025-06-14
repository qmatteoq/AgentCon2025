namespace OrchestrationSamples.Plugins;

public partial class TicketPlugin
{
    // Model for Ticket (matches OpenAPI schema)
    public class Ticket
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AssignedTo { get; set; }
        public string? Severity { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public string? Status { get; set; }
    }
}
