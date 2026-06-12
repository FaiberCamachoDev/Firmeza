# Swagger en Firmeza.Api (Week 3)

## ¿Qué es Swagger?

Swagger (parte del estándar **OpenAPI**) es un sistema que lee el código de los controladores y genera automáticamente una **especificación JSON** que describe todos los endpoints de la API: rutas, métodos HTTP, parámetros, cuerpos de request, respuestas y esquemas de datos.

Sobre esa especificación JSON monta una **interfaz web interactiva** (Swagger UI) desde la que se puede ver, entender y probar la API directamente en el navegador, sin necesidad de Postman ni curl.

---

## Archivos involucrados

```
Firmeza.Api/
├── Program.cs                  ← registra SwaggerGen y SwaggerUI; configura esquema JWT
├── appsettings.json            ← no requiere configuración adicional para Swagger
└── Controllers/
    ├── AuthController.cs       ← comentarios XML (<summary>) usados por Swagger
    ├── ProductsController.cs
    ├── CustomersController.cs
    └── SalesController.cs
```

---

## Paquete utilizado

```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.3.1" />
```

`Swashbuckle` integra Swagger con ASP.NET Core y hace dos cosas:

1. **SwaggerGen** — inspecciona todos los `[ApiController]` al arrancar y genera `swagger.json` con la especificación OpenAPI.
2. **SwaggerUI** — sirve la interfaz web HTML/JS que lee ese JSON y la presenta como documentación navegable.

---

## Registro en `Program.cs`

```csharp
// 1. Genera la especificación OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Firmeza API",
        Version     = "v1",
        Description = "API RESTful para la gestión de productos, clientes y ventas de Firmeza.",
    });

    // Añade el esquema JWT para el botón "Authorize" de Swagger UI
    var jwtScheme = new OpenApiSecurityScheme { ... };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, [] } });
});
```

```csharp
// 2. Sirve el JSON y la UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Firmeza API v1");
    c.RoutePrefix = string.Empty;  // La UI está en "/" (raíz del servidor)
});
```

---

## ¿Dónde acceder?

Levanta el proyecto API:

```bash
cd Firmeza/Firmeza.Api
dotnet run
```

| URL | Qué hay |
|---|---|
| `http://localhost:5109/` | Swagger UI — interfaz interactiva completa |
| `http://localhost:5109/swagger/v1/swagger.json` | Especificación OpenAPI en JSON puro |
| `https://localhost:7245/` | Ídem en HTTPS |

Los puertos vienen de `Firmeza.Api/Properties/launchSettings.json`. Si el puerto ya está en uso, .NET elige otro y lo muestra en la terminal.

---

## Proceso completo: de código a consumo

```
[Código C# con controladores y DTOs]
  ↓  SwaggerGen lee atributos y tipos al arrancar
[swagger.json] — especificación OpenAPI generada automáticamente
  ↓  SwaggerUI la renderiza
[Interfaz web] — documentación interactiva + testing
  ↓  Clientes (Blazor, Postman, fetch) la consumen
[Requests HTTP] → API → DB → Respuesta JSON
```

### Qué lee SwaggerGen del código

Cada vez que arranca la API, Swashbuckle escanea:

- Las rutas `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`
- Los parámetros de entrada (query, body, route)
- Los tipos de retorno (`ProductDto`, `TokenResponseDto`, etc.)
- Los atributos `[Authorize]` para marcar endpoints que requieren token
- Los comentarios XML `/// <summary>` para mostrar descripciones en la UI

Ejemplo de lo que lee en `ProductsController.cs`:
```csharp
/// <summary>Lista productos con filtros opcionales.</summary>
[HttpGet]
public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(
    [FromQuery] string? search,
    [FromQuery] string? category,
    [FromQuery] bool? active)
```
→ Swagger genera la entrada: `GET /api/products` con tres query params opcionales.

---

## Autenticación JWT en Swagger UI

Para probar endpoints protegidos con `[Authorize]` desde la interfaz:

1. Ejecuta `POST /api/auth/login` con las credenciales del admin:
   ```json
   { "email": "admin@firmeza.com", "password": "Admin@123!" }
   ```
2. Copia el campo `token` de la respuesta.
3. Haz clic en el botón **"Authorize"** (candado) en la esquina superior derecha.
4. Pega el token en el campo `Value` y haz clic en **Authorize**.
5. Swagger añade automáticamente el header `Authorization: Bearer <token>` a todas las peticiones siguientes.

---

## Resumen de rutas disponibles en la UI

```
http://localhost:5109/
      │
      ├── /                              → Swagger UI (navegador)
      ├── /swagger/v1/swagger.json       → OpenAPI JSON
      │
      ├── POST  /api/auth/login          → sin auth
      ├── POST  /api/auth/register       → sin auth
      │
      ├── GET   /api/products            → sin auth
      ├── GET   /api/products/{id}       → sin auth
      ├── GET   /api/products/categories → sin auth
      ├── POST  /api/products            → requiere JWT Admin
      ├── PUT   /api/products/{id}       → requiere JWT Admin
      ├── DELETE /api/products/{id}      → requiere JWT Admin
      │
      ├── GET   /api/customers           → requiere JWT Admin
      ├── GET   /api/customers/{id}      → requiere JWT Admin
      ├── POST  /api/customers           → requiere JWT Admin
      ├── PUT   /api/customers/{id}      → requiere JWT Admin
      ├── DELETE /api/customers/{id}     → requiere JWT Admin
      │
      ├── GET   /api/sales               → requiere JWT Admin
      ├── GET   /api/sales/{id}          → requiere JWT Admin
      └── POST  /api/sales               → requiere JWT Admin o Cliente
```

---

## Consumo desde otros clientes

El `swagger.json` puede usarse para generar clientes automáticos en otros lenguajes. Cuando se integre Blazor WebAssembly, se puede apuntar un generador de código al JSON para producir un cliente C# tipado:

```bash
dotnet tool install -g Microsoft.dotnet-openapi
dotnet openapi add url http://localhost:5109/swagger/v1/swagger.json
```
