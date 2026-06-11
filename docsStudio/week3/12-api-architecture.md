# Firmeza.Api — Arquitectura General (Week 3)

## Proyecto

`Firmeza.Api` es un proyecto ASP.NET Core Web API (net9.0) que comparte la misma base de datos PostgreSQL con `Firmeza.Web` mediante una referencia de proyecto al `ApplicationDbContext` y los modelos existentes.

```
Firmeza.slnx
├── Firmeza.Web          ← Razor Pages (admin)
├── Firmeza.Api          ← REST API (este proyecto)
└── Firmeza.Tests        ← xUnit (referencia ambos proyectos)
```

## Estructura de directorios

```
Firmeza.Api/
├── Controllers/         ← AuthController, ProductsController,
│                           CustomersController, SalesController
├── DTOs/
│   ├── Auth/            ← LoginDto, RegisterDto, TokenResponseDto
│   ├── Products/        ← ProductDto, ProductCreateDto, ProductUpdateDto
│   ├── Customers/       ← CustomerDto, CustomerCreateDto, CustomerUpdateDto
│   └── Sales/           ← SaleDto, SaleCreateDto, SaleDetailDto
├── Mappings/            ← MappingProfile (AutoMapper)
├── Services/
│   ├── JwtService.cs
│   ├── IEmailService.cs
│   └── EmailService.cs
├── Program.cs
└── appsettings.json
```

## Dependencias de paquetes

| Paquete | Versión | Propósito |
|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 9.0.0 | Validación JWT |
| `AutoMapper` | 14.0.0 | Mapeo entidad ↔ DTO |
| `Swashbuckle.AspNetCore` | 7.3.1 | Swagger / OpenAPI |
| `MailKit` | 4.8.0 | Envío SMTP |
| `Microsoft.EntityFrameworkCore` | 9.0.5 | Alineación versión con Firmeza.Web |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.4 | Driver PostgreSQL |

## Base de datos

La API usa el mismo `ApplicationDbContext` y las mismas migraciones que `Firmeza.Web`. Al iniciar, ejecuta `db.Database.MigrateAsync()` y `DbSeeder.SeedAsync()`. Ambos proyectos pueden ejecutarse en paralelo apuntando a la misma base de datos.

La cadena de conexión se configura en `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=firmeza_db;..."
}
```

## CORS

Se habilita `AllowAnyOrigin/Header/Method` para permitir consumo desde Blazor u otros clientes en desarrollo. En producción se debe restringir al origen específico.

## Swagger

Accesible en `/` (raíz del servidor). Incluye el esquema Bearer JWT para probar endpoints protegidos directamente desde la UI.
