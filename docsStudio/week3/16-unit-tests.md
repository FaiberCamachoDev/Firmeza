# Pruebas Unitarias (Week 3 — Task 9)

## Proyecto

`Firmeza.Tests` (net9.0, xUnit) ya existía desde week 1. En week 3 se añadió:
- Referencia a `Firmeza.Api`
- `Microsoft.AspNetCore.Mvc.Testing 9.0.0`
- `Microsoft.EntityFrameworkCore.InMemory 9.0.5`

## Tests existentes (week 1)

`Products/ProductValidationTests.cs` — 4 tests sobre `ProductCreateViewModel`:
- Datos válidos pasan validación
- Nombre vacío falla
- Precio 0 falla
- Nombre > 150 chars falla
- `StockInput` parsing (theory con 5 casos)

## Tests añadidos (week 3)

### `Api/ProductsControllerTests.cs` — 7 tests

Usa `ApplicationDbContext` con `UseInMemoryDatabase` (GUID único por test para aislamiento). El `IMapper` se crea directamente con `MapperConfiguration`.

| Test | Qué verifica |
|---|---|
| `GetAll_ReturnsAllProducts` | Lista los 3 productos creados |
| `GetAll_WithSearchFilter_ReturnsMatches` | Búsqueda case-insensitive devuelve sólo los que coinciden |
| `GetAll_WithActiveFilter_ReturnsOnlyActive` | Filtro `active=true` excluye inactivos |
| `GetById_ExistingId_ReturnsProduct` | 200 OK con el DTO correcto |
| `GetById_NotFound_Returns404` | 404 para ID inexistente |
| `Create_ValidDto_ReturnsCreated` | 201 Created y registro en DB |
| `Update_ExistingProduct_UpdatesFields` | Patch parcial actualiza name y price |
| `Delete_ProductWithNoSales_ReturnsNoContent` | 204 y registro eliminado |
| `GetCategories_ReturnsDistinctCategories` | 2 categorías distintas para 3 productos con 2 categorías |

### `Api/EmailServiceTests.cs` — 4 tests

Usa `NullLogger` y `ConfigurationBuilder` con `AddInMemoryCollection` para inyectar configuración sin archivos en disco.

| Test | Qué verifica |
|---|---|
| `SendAsync_WithEmptyCredentials_DoesNotThrow` | No lanza cuando credenciales vacías |
| `SendPurchaseConfirmation_WithEmptyCredentials_DoesNotThrow` | Idem para confirmación de compra |
| `SendWelcome_WithEmptyCredentials_DoesNotThrow` | Idem para email de bienvenida |
| `EmailService_ImplementsIEmailService` | Comprueba el contrato de interfaz |

## Ejecutar

```bash
dotnet test Firmeza/Firmeza.Tests/
```

23 tests totales — 0 errores.
