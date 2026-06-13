# Servicio de Email (Week 3)

## Archivos involucrados

```
Firmeza.Api/
├── Services/
│   ├── IEmailService.cs    ← interfaz con los métodos del servicio
│   └── EmailService.cs     ← implementación concreta con MailKit + Gmail SMTP
└── appsettings.json        ← sección "Email": SmtpHost, SmtpPort, Username, Password, etc.
```

---

## Diseño

El servicio se accede siempre a través de la interfaz `IEmailService`, lo que permite sustituir la implementación Gmail por cualquier proveedor SMTP sin modificar los controladores.

```
IEmailService  ←  registrado como Scoped en Program.cs
    ↑
EmailService   ←  implementación concreta (MailKit + Gmail SMTP)
```

Para cambiar de proveedor basta con crear otra clase que implemente `IEmailService` y cambiar el registro en `Program.cs`.

---

## Configuración

En `Firmeza.Api/appsettings.json`:

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

**En producción** usar variables de entorno o Secret Manager para no exponer credenciales en el repositorio.

---

## Comportamiento cuando no está configurado

Si `Username` o `FromAddress` están vacíos en `appsettings.json`, el servicio registra una advertencia en el log y retorna sin lanzar excepción. Esto permite que la API funcione sin email configurado en entornos de desarrollo y pruebas.

---

## Métodos disponibles

**Archivo:** `Firmeza.Api/Services/IEmailService.cs`

```csharp
Task SendAsync(string toAddress, string toName, string subject, string htmlBody);
Task SendWelcomeAsync(string toEmail, string customerName);
Task SendPurchaseConfirmationAsync(string toEmail, string customerName, int saleId, decimal total);
```

Los métodos de conveniencia (`SendWelcomeAsync`, `SendPurchaseConfirmationAsync`) generan el HTML internamente y delegan en `SendAsync`.

---

## Uso en controladores (fire-and-forget)

Los controladores llaman al servicio en modo fire-and-forget para no bloquear la respuesta HTTP:

```csharp
// En AuthController.Register
_ = _email.SendWelcomeAsync(user.Email!, $"{user.FirstName} {user.LastName}");

// En SalesController.Create
_ = _email.SendPurchaseConfirmationAsync(customer.Email!, customerName, sale.Id, sale.Total);
```

Si el envío de email falla, no afecta la operación principal ni la respuesta al cliente.

---

## Registro en `Program.cs`

```csharp
builder.Services.AddScoped<IEmailService, EmailService>();
```

---

## Pruebas unitarias del servicio

Las pruebas del servicio están en `Firmeza.Tests/Api/EmailServiceTests.cs` y verifican que el servicio no lance excepciones cuando las credenciales están vacías (escenario de desarrollo sin email configurado). Ver detalle en `16-unit-tests.md`.
