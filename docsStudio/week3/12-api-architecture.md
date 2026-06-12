# Firmeza.Api — Arquitectura General (Week 3)

## Proyecto

`Firmeza.Api` es un proyecto ASP.NET Core Web API (net9.0) que comparte la misma base de datos PostgreSQL con `Firmeza.Web` mediante una referencia de proyecto al `ApplicationDbContext` y los modelos existentes.

```
Firmeza.slnx
├── Firmeza.Web          ← Razor Pages (panel administrativo)
├── Firmeza.Api          ← REST API (este proyecto)
└── Firmeza.Tests        ← xUnit (referencia ambos proyectos)
```

---

## Estructura de directorios y archivos

```
Firmeza.Api/
├── Controllers/
│   ├── AuthController.cs           ← login y registro con JWT
│   ├── ProductsController.cs       ← CRUD de productos con filtros
│   ├── CustomersController.cs      ← CRUD de clientes con soft-delete
│   └── SalesController.cs          ← listado y creación de ventas
├── DTOs/
│   ├── Auth/
│   │   ├── LoginDto.cs             ← email + password
│   │   ├── RegisterDto.cs          ← datos de nuevo usuario
│   │   └── TokenResponseDto.cs     ← token + metadata en respuesta
│   ├── Products/
│   │   ├── ProductDto.cs           ← respuesta de lectura
│   │   ├── ProductCreateDto.cs     ← campos para crear
│   │   └── ProductUpdateDto.cs     ← campos nullable para patch parcial
│   ├── Customers/
│   │   ├── CustomerDto.cs
│   │   ├── CustomerCreateDto.cs
│   │   └── CustomerUpdateDto.cs
│   └── Sales/
│       ├── SaleDto.cs              ← incluye CustomerName y CustomerDocument
│       ├── SaleCreateDto.cs        ← CustomerId + lista de ítems
│       └── SaleDetailDto.cs        ← incluye ProductName y Subtotal calculado
├── Mappings/
│   └── MappingProfile.cs           ← todos los mapeos AutoMapper
├── Services/
│   ├── JwtService.cs               ← genera tokens JWT con claims
│   ├── IEmailService.cs            ← interfaz del servicio de email
│   └── EmailService.cs             ← implementación con MailKit + Gmail SMTP
├── Program.cs                      ← configuración completa de la API
├── appsettings.json                ← connection string, JWT config, email config
└── Properties/
    └── launchSettings.json         ← puertos de desarrollo (5109 HTTP, 7245 HTTPS)
```

---

## Dependencias de paquetes

| Paquete | Versión | Propósito |
|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 9.0.0 | Validación de tokens JWT |
| `AutoMapper` | 15.1.3 | Mapeo entidad ↔ DTO |
| `Swashbuckle.AspNetCore` | 7.3.1 | Swagger / OpenAPI |
| `MailKit` | 4.17.0 | Envío SMTP |
| `Microsoft.EntityFrameworkCore` | 9.0.5 | Alineación de versión con Firmeza.Web |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.4 | Driver PostgreSQL |

---

## Base de datos compartida

La API usa el mismo `ApplicationDbContext` y las mismas migraciones que `Firmeza.Web`. Al iniciar, ejecuta `db.Database.MigrateAsync()` y `DbSeeder.SeedAsync()`. Ambos proyectos pueden ejecutarse en paralelo apuntando a la misma base de datos.

La cadena de conexión se configura en `Firmeza.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=firmeza_db;..."
}
```

---

## CORS

Se habilita `AllowAnyOrigin/Header/Method` para permitir consumo desde Blazor u otros clientes en desarrollo. En producción se debe restringir al origen específico del cliente.

---

## Swagger

Accesible en `/` (raíz del servidor). Incluye el esquema Bearer JWT para probar endpoints protegidos directamente desde la interfaz.

Ver documentación detallada en `17-swagger.md`.

---

## Cómo levantar la API

```bash
cd Firmeza/Firmeza.Api
dotnet run
```

- Swagger UI: `http://localhost:5109/`
- Especificación OpenAPI JSON: `http://localhost:5109/swagger/v1/swagger.json`
