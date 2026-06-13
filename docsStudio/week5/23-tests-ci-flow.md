# Tests automatizados y flujo CI con Docker (Week 5)

## Objetivo

Los 62 tests xUnit actúan como **gate de calidad**: si alguno falla, Docker Compose detiene la orquestación y ningún servicio de producción se levanta.

---

## Cobertura de pruebas — Firmeza.Tests

| Suite | Tests | Qué valida |
|---|---|---|
| `AuthControllerTests` | 8 | Registro, login, JWT, creación de Customer, CustomerId en respuesta |
| `ProductControllerTests` | 12 | CRUD de productos, autorización por rol |
| `CategoryControllerTests` | 8 | CRUD de categorías |
| `SaleControllerTests` | 10 | Registro de ventas, detalle, permisos |
| `CustomerControllerTests` | 8 | CRUD de clientes |
| `SupplierControllerTests` | 8 | CRUD de proveedores |
| `EmailServiceTests` | 4 | Construcción de mensajes SMTP |
| `JwtServiceTests` | 4 | Generación y validación de tokens JWT |

**Total: 62 tests**

---

## Infraestructura de testing — InMemory Database

Cada test que necesita `ApplicationDbContext` crea su propia instancia en memoria con un GUID único:

```csharp
public class AuthControllerTests : IDisposable
{
    private readonly ApplicationDbContext _db;

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())  // aislamiento total
            .Options;
        _db = new ApplicationDbContext(options);
    }

    public void Dispose() => _db.Dispose();
}
```

**Por qué `Guid.NewGuid()`:** Cada test class obtiene una base de datos limpia. Sin esto, los tests que crean registros contaminarían los tests posteriores.

---

## Integración con Docker — Dockerfile.tests

El contenedor de tests es un **one-shot container**: arranca, ejecuta todos los tests y termina con el código de salida de `dotnet test`.

```
docker compose up
    │
    ├─ tests (one-shot)
    │      └─ dotnet test → exit 0 ✓
    │
    ├─ db (healthcheck: pg_isready)
    │      └─ healthy ✓
    │
    ├─ api   (depends_on: tests✓ + db healthy)
    ├─ admin (depends_on: tests✓ + db healthy)
    └─ client (depends_on: tests✓ + api started)
```

---

## Cadena de dependencias en docker-compose.yml

```yaml
db:
  depends_on:
    tests:
      condition: service_completed_successfully   # tests pasaron

api:
  depends_on:
    tests:
      condition: service_completed_successfully
    db:
      condition: service_healthy                  # postgres listo

client:
  depends_on:
    tests:
      condition: service_completed_successfully
    api:
      condition: service_started
```

`service_completed_successfully` → el contenedor terminó con exit code 0. Si los tests fallan (exit code 1), todos los servicios dependientes quedan en estado `Waiting` y Compose reporta el error.

---

## Ejecución y verificación

```bash
# Construir y levantar todo
docker compose up --build

# Solo correr las pruebas (sin levantar servicios)
docker compose up --build tests

# Ver logs de los tests
docker compose logs tests
```

**Puertos en producción Docker:**

| Servicio | Puerto host | Puerto contenedor |
|---|---|---|
| API REST | 5109 | 8080 |
| Admin (Razor Pages) | 5006 | 8080 |
| Client (React/nginx) | 5173 | 80 |
| PostgreSQL | 5433 | 5432 |
