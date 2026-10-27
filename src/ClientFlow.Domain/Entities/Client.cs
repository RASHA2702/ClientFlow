using ClientFlow.Domain.Enums;
using ClientFlow.Domain.Exceptions;

namespace ClientFlow.Domain.Entities;

public class Client
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? BillingAddress { get; private set; }
    public string Currency { get; private set; } = "USD";
    public ClientStatus Status { get; private set; } = ClientStatus.Active;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<Project> _projects = new();
    public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();

    private Client() { }

    public static Client Create(string name, string companyName, string email, string? phone = null, string? billingAddress = null, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException(nameof(Name), "Client name is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new BusinessRuleValidationException(nameof(Email), "A valid email address is required.");

        return new Client
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            CompanyName = companyName?.Trim() ?? string.Empty,
            Email = email.Trim().ToLowerInvariant(),
            Phone = phone?.Trim(),
            BillingAddress = billingAddress?.Trim(),
            Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant(),
            Status = ClientStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdateDetails(string name, string companyName, string email, string? phone, string? billingAddress, string currency)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException(nameof(Name), "Client name is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new BusinessRuleValidationException(nameof(Email), "A valid email address is required.");

        Name = name.Trim();
        CompanyName = companyName?.Trim() ?? string.Empty;
        Email = email.Trim().ToLowerInvariant();
        Phone = phone?.Trim();
        BillingAddress = billingAddress?.Trim();
        Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetStatus(ClientStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
