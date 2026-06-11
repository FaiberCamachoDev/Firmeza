using Firmeza.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Firmeza.Tests.Api;

public class EmailServiceTests
{
    private static EmailService BuildService(Dictionary<string, string?> config)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build();

        return new EmailService(configuration, NullLogger<EmailService>.Instance);
    }

    [Fact]
    public async Task SendAsync_WithEmptyCredentials_DoesNotThrow()
    {
        // When SMTP credentials are not configured, the service logs and returns gracefully.
        var svc = BuildService(new Dictionary<string, string?>
        {
            ["Email:SmtpHost"]     = "smtp.gmail.com",
            ["Email:SmtpPort"]     = "587",
            ["Email:Username"]     = "",
            ["Email:Password"]     = "",
            ["Email:FromName"]     = "Firmeza",
            ["Email:FromAddress"]  = "",
        });

        // Should complete without throwing — credentials are empty so the send is skipped
        await svc.SendAsync("test@test.com", "Test", "Subject", "<p>Body</p>");
    }

    [Fact]
    public async Task SendPurchaseConfirmation_WithEmptyCredentials_DoesNotThrow()
    {
        var svc = BuildService(new Dictionary<string, string?>
        {
            ["Email:Username"]    = "",
            ["Email:FromAddress"] = "",
        });

        await svc.SendPurchaseConfirmationAsync("client@test.com", "Juan Pérez", 42, 1500.00m);
    }

    [Fact]
    public async Task SendWelcome_WithEmptyCredentials_DoesNotThrow()
    {
        var svc = BuildService(new Dictionary<string, string?>
        {
            ["Email:Username"]    = "",
            ["Email:FromAddress"] = "",
        });

        await svc.SendWelcomeAsync("client@test.com", "Juan Pérez");
    }

    [Fact]
    public void EmailService_ImplementsIEmailService()
    {
        var svc = BuildService([]);
        Assert.IsAssignableFrom<IEmailService>(svc);
    }
}
