using System.ComponentModel;
using System.Net.Http.Json;
using Microsoft.SemanticKernel;

namespace OrchestrationSamples.Plugins;

public partial class TicketPlugin
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://ticket-copilot.azurewebsites.net/api";

    public TicketPlugin(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    [KernelFunction, Description("Create a new ticket with the provided details.")]
    // Create a new ticket
    public async Task<Ticket?> CreateTicketAsync([Description("The title of the ticket")]string title, [Description("The description of the ticket")]string description, [Description("The person assigned to the ticket")]string? assignedTo = "Matteo Pagani", [Description("The severity of the ticket")]string? severity = "Normal", [Description("The status of the ticket")] string? status = "Open")
    {
        var ticket = new Ticket
        {
            Title = title,
            Description = description,
            AssignedTo = assignedTo,
            Severity = severity,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/tickets", ticket);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine(error);
            throw new Exception($"Failed to create ticket: {response.StatusCode} - {error}");
        }
        return await response.Content.ReadFromJsonAsync<Ticket>();
    }

    // Get tickets (optionally filter by search, assignedTo, status)
    [KernelFunction, Description("Get the list of tickets, optionally filtered by search term, assigned user, and status.")]
    public async Task<List<Ticket>?> GetTicketsAsync([Description("The search term")]string? search = null,[Description("The person assigned to the ticket")] string? assignedTo = null, [Description("The status of the ticket")]string? status = null)
    {
        var url = $"{BaseUrl}/tickets";
        var query = new List<string>();
        if (!string.IsNullOrEmpty(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrEmpty(assignedTo)) query.Add($"assignedTo={Uri.EscapeDataString(assignedTo)}");
        if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");
        if (query.Count > 0) url += "?" + string.Join("&", query);

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to get tickets: {response.StatusCode} - {error}");
        }
        return await response.Content.ReadFromJsonAsync<List<Ticket>>();
    }

    // Delete a ticket by id
    public async Task DeleteTicketAsync(string id)
    {
        var response = await _httpClient.DeleteAsync($"{BaseUrl}/tickets/{Uri.EscapeDataString(id)}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to delete ticket: {response.StatusCode} - {error}");
        }
    }
}
