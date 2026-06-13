namespace Firmeza.Api.Services;

public interface IEmailService
{
    Task SendAsync(string toAddress, string toName, string subject, string htmlBody);
    Task SendPurchaseConfirmationAsync(string toEmail, string customerName, int saleId, decimal total);
    Task SendWelcomeAsync(string toEmail, string customerName);
}
