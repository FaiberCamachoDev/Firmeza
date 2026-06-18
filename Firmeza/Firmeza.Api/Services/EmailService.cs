using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Firmeza.Api.Services;

public class EmailSettings
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromName { get; set; } = "Firmeza";
    public string FromAddress { get; set; } = string.Empty;
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _settings = config.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
        _logger = logger;
    }

    public async Task SendAsync(string toAddress, string toName, string subject, string htmlBody)
    {
        if (string.IsNullOrEmpty(_settings.Username) || string.IsNullOrEmpty(_settings.FromAddress))
        {
            _logger.LogWarning("Email no configurado (Username/FromAddress vacíos). Mensaje omitido: {Subject}", subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(new MailboxAddress(toName, toAddress));
        message.Subject = subject;

        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        // M9: enmascarar email en logs para no exponer PII
        var atIdx = toAddress.IndexOf('@');
        var masked = atIdx > 1
            ? $"{toAddress[..2]}***{toAddress[(atIdx - 1)..]}"
            : "***";
        _logger.LogInformation("Email enviado a {To}: {Subject}", masked, subject);
    }

    public Task SendPurchaseConfirmationAsync(string toEmail, string customerName, int saleId, decimal total) =>
        SendAsync(
            toEmail, customerName,
            $"Confirmación de compra #{saleId:D6} — Firmeza",
            BuildPurchaseHtml(customerName, saleId, total));

    public Task SendWelcomeAsync(string toEmail, string customerName) =>
        SendAsync(
            toEmail, customerName,
            "Bienvenido a Firmeza",
            BuildWelcomeHtml(customerName));

    // L7: HtmlEncode para evitar inyección HTML en correos si el nombre contiene caracteres especiales
    private static string BuildPurchaseHtml(string name, int saleId, decimal total)
    {
        var safeName = System.Net.WebUtility.HtmlEncode(name);
        return $"""
            <html><body style="font-family:Arial,sans-serif;color:#333;">
              <h2 style="color:#4F46E5;">Firmeza — Confirmación de Compra</h2>
              <p>Hola <strong>{safeName}</strong>,</p>
              <p>Tu compra <strong>#{saleId:D6}</strong> ha sido registrada exitosamente.</p>
              <p>Total: <strong>${total:N2}</strong></p>
              <p>Gracias por confiar en Firmeza.</p>
              <hr/><p style="font-size:12px;color:#999;">Firmeza — Sistema Administrativo</p>
            </body></html>
            """;
    }

    private static string BuildWelcomeHtml(string name)
    {
        var safeName = System.Net.WebUtility.HtmlEncode(name);
        return $"""
            <html><body style="font-family:Arial,sans-serif;color:#333;">
              <h2 style="color:#4F46E5;">¡Bienvenido a Firmeza!</h2>
              <p>Hola <strong>{safeName}</strong>,</p>
              <p>Tu cuenta de cliente ha sido creada. Ya puedes acceder al portal.</p>
              <hr/><p style="font-size:12px;color:#999;">Firmeza — Sistema Administrativo</p>
            </body></html>
            """;
    }
}
