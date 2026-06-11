# Servicio de Email (Week 3 — Task 8)

## Diseño

El servicio se accede siempre a través de la interfaz `IEmailService`, lo que permite sustituir la implementación Gmail por cualquier SMTP empresarial sin tocar los controladores.

```
IEmailService  ←  registrado como Scoped
    ↑
EmailService   ←  implementación concreta (MailKit + Gmail SMTP)
```

Para cambiar de proveedor basta con crear otra clase que implemente `IEmailService` y cambiar el registro en `Program.cs`.

## Configuración

```json
"Email": {
  "SmtpHost":    "smtp.gmail.com",
  "SmtpPort":    587,
  "Username":    "tu-cuenta@gmail.com",
  "Password":    "contraseña-de-aplicación",
  "FromName":    "Firmeza",
  "FromAddress": "tu-cuenta@gmail.com"
}
```

**Gmail requiere una "contraseña de aplicación"** (no la contraseña de la cuenta).  
Se genera en: Cuenta Google → Seguridad → Verificación en dos pasos → Contraseñas de aplicación.

**En producción** usar variables de entorno o Secret Manager para no exponer credenciales.

## Comportamiento cuando no está configurado

Si `Username` o `FromAddress` están vacíos, el servicio registra una advertencia y retorna sin lanzar excepción. Esto permite que la API funcione sin email en desarrollo.

## Métodos disponibles

```csharp
Task SendAsync(string toAddress, string toName, string subject, string htmlBody);
Task SendPurchaseConfirmationAsync(string toEmail, string customerName, int saleId, decimal total);
Task SendWelcomeAsync(string toEmail, string customerName);
```

Los métodos de conveniencia generan el HTML internamente y delegan en `SendAsync`.

## Uso en controladores (fire-and-forget)

Los controladores llaman al servicio de forma fire-and-forget para no bloquear la respuesta HTTP:

```csharp
_ = _email.SendWelcomeAsync(user.Email!, $"{user.FirstName} {user.LastName}");
```

Si el email falla no afecta la operación principal.
