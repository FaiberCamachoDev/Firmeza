# Referencia de Endpoints de la API (Week 3)

## Archivos de controladores

```
Firmeza.Api/Controllers/
├── AuthController.cs        ← /api/auth
├── ProductsController.cs    ← /api/products
├── CustomersController.cs   ← /api/customers
└── SalesController.cs       ← /api/sales
```

---

## Auth

**Archivo:** `Firmeza.Api/Controllers/AuthController.cs`

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| POST | `/api/auth/login` | — | Obtiene un token JWT |
| POST | `/api/auth/register` | — | Registra un usuario con rol Cliente y retorna token |

---

## Products

**Archivo:** `Firmeza.Api/Controllers/ProductsController.cs`

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/api/products` | — | Lista productos (filtros opcionales) |
| GET | `/api/products/{id}` | — | Detalle de un producto por ID |
| GET | `/api/products/categories` | — | Lista de categorías distintas |
| POST | `/api/products` | Admin | Crea un producto nuevo |
| PUT | `/api/products/{id}` | Admin | Actualiza campos (patch parcial) |
| DELETE | `/api/products/{id}` | Admin | Elimina (rechaza si tiene ventas asociadas) |

### Filtros de `GET /api/products`
- `?search=cemento` — busca en nombre y descripción (sin distinguir mayúsculas/minúsculas)
- `?category=Cementos` — filtra por categoría exacta
- `?active=true` — solo productos activos / inactivos

---

## Customers

**Archivo:** `Firmeza.Api/Controllers/CustomersController.cs`

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/api/customers` | Admin | Lista clientes (filtros opcionales) |
| GET | `/api/customers/{id}` | Admin | Detalle de un cliente por ID |
| POST | `/api/customers` | Admin | Crea cliente (valida documento único) |
| PUT | `/api/customers/{id}` | Admin | Actualiza campos del cliente |
| DELETE | `/api/customers/{id}` | Admin | Soft-delete (`IsActive = false`) |

### Filtros de `GET /api/customers`
- `?search=juan` — busca en nombre, apellido y número de documento
- `?active=true` — solo clientes activos / inactivos

---

## Sales

**Archivo:** `Firmeza.Api/Controllers/SalesController.cs`

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/api/sales` | Admin | Lista todas las ventas ordenadas por fecha |
| GET | `/api/sales/{id}` | Admin | Detalle de una venta por ID |
| POST | `/api/sales` | Admin, Cliente | Registra una venta + envía email de confirmación |

### Proceso al crear una venta (`POST /api/sales`)
1. Valida que el cliente exista y esté activo
2. Valida que todos los productos existan y estén activos
3. Verifica que haya stock suficiente para cada ítem
4. Descuenta el stock de cada producto
5. Captura el precio actual como `UnitPrice` en `SaleDetail`
6. Calcula el total como suma de subtotales
7. Dispara email de confirmación en fire-and-forget

---

## DTOs y AutoMapper

**Archivo de mapeos:** `Firmeza.Api/Mappings/MappingProfile.cs`

```
Product           → ProductDto
ProductCreateDto  → Product

Customer          → CustomerDto
CustomerCreateDto → Customer

Sale              → SaleDto       (incluye CustomerName y CustomerDocument)
SaleDetail        → SaleDetailDto (incluye ProductName y Subtotal calculado)
```

`ProductUpdateDto` y `CustomerUpdateDto` usan campos nullable y se aplican manualmente en cada controlador para hacer patch parcial sin sobrescribir campos que no se enviaron en el request.

---

## Códigos de respuesta por endpoint

| Situación | Código HTTP |
|---|---|
| Operación exitosa (lectura) | 200 OK |
| Recurso creado | 201 Created |
| Sin contenido (delete exitoso) | 204 No Content |
| Validación fallida / stock insuficiente | 400 Bad Request |
| Sin token o token inválido | 401 Unauthorized |
| Token válido pero sin el rol requerido | 403 Forbidden |
| Recurso no encontrado | 404 Not Found |
| Conflicto (documento duplicado / producto en uso) | 409 Conflict |
