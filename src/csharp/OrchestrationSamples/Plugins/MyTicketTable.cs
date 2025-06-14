namespace OrchestrationSamples.Plugins;

public partial class TicketPlugin
{
    // Model for MyTicketTable (response from CreateTicket)
    public class MyTicketTable
    {
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public object? ETag { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? AssignedTo { get; set; }
        public string? Severity { get; set; }
        public string? Status { get; set; }
    }
}
