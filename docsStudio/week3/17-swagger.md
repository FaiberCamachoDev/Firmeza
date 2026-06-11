# Swagger en Firmeza.Api (Week 3 — Task 7)

## ¿Qué es Swagger?

Swagger (ahora parte del estándar **OpenAPI**) es un sistema que lee tu código de controladores y genera automáticamente una **especificación JSON** que describe todos los endpoints de tu API: rutas, métodos HTTP, parámetros, cuerpos de request, respuestas y esquemas de datos.

Sobre esa especificación JSON monta una **interfaz web interactiva** (Swagger UI) desde la que puedes ver, entender y probar la API directamente en el navegador, sin Postman ni curl.

## Cómo funciona en este proyecto

### Paquete

```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.3.1" />
```

`Swashbuckle` es la librería .NET que integra Swagger con ASP.NET Core. Hace dos cosas:

1. **SwaggerGen** — inspecciona todos los `[ApiController]` en tiempo de arranque y genera el archivo `swagger.json` con la especificación OpenAPI.
2. **SwaggerUI** — sirve la interfaz web HTML/JS que lee ese JSON y la muestra como documentación navegable.

### Registro en `Program.cs`

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
    c.RoutePrefix = string.Empty;  // La UI está en "/" (raíz)
});
```

## ¿Dónde lo veo?

Arranca el proyecto API:

```bash
cd Firmeza/Firmeza.Api
dotnet run
```

Y abre en el navegador:

| URL | Qué hay |
|---|---|
| `http://localhost:5109/` | Swagger UI — interfaz interactiva completa |
| `http://localhost:5109/swagger/v1/swagger.json` | Especificación OpenAPI en JSON puro |
| `https://localhost:7245/` | Idem en HTTPS |

> Los puertos vienen de `Properties/launchSettings.json`. Si el puerto ya está en uso, .NET elige otro y lo muestra en la terminal.

## Proceso completo: de código a consumo

```
[Código C#]
  ↓  SwaggerGen lee atributos y tipos
[swagger.json] — especificación OpenAPI
  ↓  SwaggerUI la renderiza
[Interfaz web] — documentación + testing
  ↓  Cliente (Blazor, Postman, fetch) la consume
[HTTP requests] → API → DB → JSON response
```

### 1. El código genera la especificación

Cada vez que arranca la API, Swashbuckle escanea:

- Las rutas `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`
- Los parámetros de entrada (query, body, route)
- Los tipos de retorno (`ProductDto`, `TokenResponseDto`, etc.)
- Los atributos `[Authorize]` para marcar qué endpoints requieren token
- Los `/// <summary>` XML para mostrar descripciones en la UI

Ejemplo de lo que lee en `ProductsController`:
```csharp
/// <summary>Lista productos con filtros opcionales.</summary>
[HttpGet]
public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(
    [FromQuery] string? search,
    [FromQuery] string? category,
    [FromQuery] bool? active)
```
→ Swagger genera la entrada: `GET /api/products` con tres query params opcionales.

### 2. La UI lo muestra interactivo

En `http://localhost:5109/` ves una página con:
- Todos los endpoints agrupados por controlador
- Botón **"Try it out"** para ejecutar requests reales desde el navegador
- Formularios con los parámetros detectados
- Respuesta HTTP completa (status, headers, body)

### 3. Autenticación JWT en Swagger UI

La API tiene endpoints protegidos con `[Authorize]`. Para probarlos desde Swagger:

1. Ejecuta `POST /api/auth/login` con las credenciales del admin:
   ```json
   { "email": "admin@firmeza.com", "password": "Admin@123!" }
   ```
2. Copia el `token` de la respuesta.
3. Haz clic en el botón **"Authorize"** (candado 🔓) en la esquina superior derecha.
4. Pega el token en el campo `Value` y haz clic en **Authorize**.
5. Swagger añade automáticamente el header `Authorization: Bearer <token>` a todas las peticiones siguientes.

### 4. Consumo desde otros clientes

El `swagger.json` también sirve para generar clientes automáticos en otros lenguajes. Por ejemplo, cuando se integre Blazor WebAssembly, se puede apuntar un generador de código al JSON para producir un cliente C# tipado sin escribir código manualmente.

```bash
# Ejemplo con la herramienta oficial de Microsoft
dotnet tool install -g Microsoft.dotnet-openapi
dotnet openapi add url http://localhost:5109/swagger/v1/swagger.json
```

## Resumen visual

```
dotnet run (Firmeza.Api)
      │
      ▼
http://localhost:5109/
      │
      ├── /                    → Swagger UI  (navegador)
      ├── /swagger/v1/swagger.json → OpenAPI JSON
      ├── /api/auth/login      → POST  (sin auth)
      ├── /api/products        → GET   (sin auth)
      ├── /api/products/{id}   → GET   (sin auth)
      ├── /api/products        → POST  (requiere Admin JWT)
      ├── /api/customers       → GET   (requiere Admin JWT)
      └── /api/sales           → POST  (requiere Admin o Cliente JWT)
```
