# Pruebas Unitarias (Week 3)

## Proyecto

**Ruta:** `Firmeza/Firmeza.Tests/`

`Firmeza.Tests` (net9.0, xUnit) ya existía desde week 1. En week 3 se añadió:
- Referencia al proyecto `Firmeza.Api`
- `Microsoft.AspNetCore.Mvc.Testing 9.0.0`
- `Microsoft.EntityFrameworkCore.InMemory 9.0.5`
- `Moq 4.x` (para mock de UserManager en AuthControllerTests)

---

## Estructura de archivos de pruebas

```
Firmeza.Tests/
├── Products/
│   └── ProductValidationTests.cs       ← validaciones de ViewModel (week 1)
└── Api/
    ├── ProductsControllerTests.cs       ← 9 tests
    ├── EmailServiceTests.cs             ← 4 tests
    ├── AuthControllerTests.cs           ← 6 tests (nuevos)
    ├── CustomersControllerTests.cs      ← 10 tests (nuevos)
    ├── JwtServiceTests.cs               ← 7 tests (nuevos)
    └── SalesControllerTests.cs          ← 8 tests (nuevos)
```

---

## Tests existentes desde week 1

**Archivo:** `Firmeza.Tests/Products/ProductValidationTests.cs`

4 tests sobre `ProductCreateViewModel` + 1 theory con 5 casos:
- Datos válidos pasan la validación
- Nombre vacío falla
- Precio 0 falla
- Nombre mayor a 150 caracteres falla
- Parsing de `StockInput` (theory con 5 casos: "abc", "0", "50", "-1", "")

---

## Tests añadidos en week 3

### `Api/ProductsControllerTests.cs` — 9 tests

Usa `ApplicationDbContext` con `UseInMemoryDatabase` (GUID único por test para aislamiento). El `IMapper` se crea mediante `ServiceCollection` con `AddAutoMapper`.

| Test | Qué verifica |
|---|---|
| `GetAll_ReturnsAllProducts` | Lista los 3 productos creados |
| `GetAll_WithSearchFilter_ReturnsMatches` | Búsqueda sin distinguir mayúsculas devuelve solo coincidencias |
| `GetAll_WithActiveFilter_ReturnsOnlyActive` | Filtro `active=true` excluye inactivos |
| `GetById_ExistingId_ReturnsProduct` | 200 OK con el DTO correcto |
| `GetById_NotFound_Returns404` | 404 para ID inexistente |
| `Create_ValidDto_ReturnsCreated` | 201 Created y registro guardado en DB |
| `Update_ExistingProduct_UpdatesFields` | Patch parcial actualiza nombre y precio |
| `Delete_ProductWithNoSales_ReturnsNoContent` | 204 y registro eliminado de la DB |
| `GetCategories_ReturnsDistinctCategories` | 2 categorías distintas para 3 productos con 2 categorías |
| `Update_NotFound_Returns404` | 404 al actualizar un ID inexistente |
| `Delete_ProductInUse_ReturnsConflict` | 409 al eliminar producto con ventas asociadas |
| `GetAll_WithCategoryFilter_ReturnsOnlyMatchingCategory` | Filtro por categoría retorna solo los que coinciden |

### `Api/EmailServiceTests.cs` — 4 tests

Usa `NullLogger` y `ConfigurationBuilder` con `AddInMemoryCollection` para inyectar configuración sin archivos en disco.

| Test | Qué verifica |
|---|---|
| `SendAsync_WithEmptyCredentials_DoesNotThrow` | No lanza excepción cuando las credenciales están vacías |
| `SendPurchaseConfirmation_WithEmptyCredentials_DoesNotThrow` | Ídem para confirmación de compra |
| `SendWelcome_WithEmptyCredentials_DoesNotThrow` | Ídem para email de bienvenida |
| `EmailService_ImplementsIEmailService` | Comprueba que la clase cumple el contrato de la interfaz |

### `Api/AuthControllerTests.cs` — 6 tests

Usa `Mock<UserManager<ApplicationUser>>` (Moq) para evitar dependencia de Identity completo.

| Test | Qué verifica |
|---|---|
| `Login_ValidCredentials_Returns200WithToken` | 200 OK con token y metadata correctos |
| `Login_UserNotFound_Returns401` | 401 cuando el email no existe |
| `Login_WrongPassword_Returns401` | 401 cuando la contraseña es incorrecta |
| `Register_NewEmail_Returns201WithToken` | 201 Created con token para email nuevo |
| `Register_EmailAlreadyTaken_Returns409` | 409 Conflict cuando el email ya está registrado |
| `Register_WeakPassword_Returns400WithErrors` | 400 Bad Request con los errores de Identity |

### `Api/CustomersControllerTests.cs` — 10 tests

| Test | Qué verifica |
|---|---|
| `GetAll_ReturnsAllCustomers` | Lista todos los clientes |
| `GetAll_WithSearchFilter_ReturnsMatches` | Búsqueda por nombre sin distinguir mayúsculas |
| `GetAll_WithActiveFilter_ReturnsOnlyActive` | Solo clientes activos |
| `GetById_ExistingId_ReturnsCustomer` | 200 OK con el DTO correcto |
| `GetById_NotFound_Returns404` | 404 para ID inexistente |
| `Create_ValidDto_ReturnsCreated` | 201 Created y registro en DB |
| `Create_DuplicateDocument_ReturnsConflict` | 409 cuando el número de documento ya existe |
| `Update_ExistingCustomer_UpdatesFields` | Patch parcial actualiza nombre y teléfono |
| `Update_NotFound_Returns404` | 404 al actualizar ID inexistente |
| `Delete_ExistingCustomer_SoftDeletesAndReturnsNoContent` | 204 y `IsActive = false` |
| `Delete_NotFound_Returns404` | 404 al eliminar ID inexistente |

### `Api/JwtServiceTests.cs` — 7 tests

| Test | Qué verifica |
|---|---|
| `GenerateToken_ReturnsNonEmptyToken` | El token generado no está vacío |
| `GenerateToken_ExpiresAt_IsCorrectOffset` | La expiración coincide con `ExpireHours` configurado |
| `GenerateToken_ContainsEmailClaim` | El token incluye el claim `email` |
| `GenerateToken_ContainsSubClaim_WithUserId` | El token incluye el claim `sub` con el ID del usuario |
| `GenerateToken_ContainsRoleClaims` | Los roles aparecen como claims en el token |
| `GenerateToken_HasUniqueJti_PerCall` | Cada llamada genera un `jti` distinto |
| `GenerateToken_ContainsFirstAndLastNameClaims` | El token incluye `firstName` y `lastName` |

### `Api/SalesControllerTests.cs` — 8 tests

Incluye un stub interno `NoOpEmailService` para evitar llamadas SMTP reales.

| Test | Qué verifica |
|---|---|
| `GetAll_ReturnsSalesOrderedByDateDesc` | Lista ordenada por fecha descendente |
| `GetById_ExistingId_ReturnsSale` | 200 OK con el DTO correcto |
| `GetById_NotFound_Returns404` | 404 para ID inexistente |
| `Create_ValidSale_ReturnsCreatedWithCorrectTotal` | 201 Created con total calculado correctamente |
| `Create_DecrementsProductStock` | El stock del producto disminuye tras la venta |
| `Create_InsufficientStock_ReturnsBadRequest` | 400 cuando la cantidad supera el stock disponible |
| `Create_InvalidCustomer_ReturnsBadRequest` | 400 cuando el cliente no existe |
| `Create_InactiveProduct_ReturnsBadRequest` | 400 cuando el producto está inactivo |
| `Create_MultipleItems_TotalIsSumOfSubtotals` | Total = suma correcta de todos los subtotales |

---

## Ejecutar las pruebas

```bash
dotnet test Firmeza/Firmeza.Tests/
```

Total: **48 tests** — 0 errores.
