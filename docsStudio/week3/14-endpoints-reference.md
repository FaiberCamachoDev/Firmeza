# Endpoints de la API (Week 3 — Tasks 4-6)

## Auth

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| POST | `/api/auth/login` | — | Obtiene JWT |
| POST | `/api/auth/register` | — | Registra usuario Cliente |

## Products

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/api/products` | — | Lista productos (filtros: `search`, `category`, `active`) |
| GET | `/api/products/{id}` | — | Detalle por ID |
| GET | `/api/products/categories` | — | Categorías distintas |
| POST | `/api/products` | Admin | Crea producto |
| PUT | `/api/products/{id}` | Admin | Actualiza campos (patch parcial) |
| DELETE | `/api/products/{id}` | Admin | Elimina (rechaza si tiene ventas) |

### Filtros de GET /api/products
- `?search=cemento` — busca en nombre y descripción (case-insensitive)
- `?category=Cementos` — filtra por categoría exacta
- `?active=true` — sólo activos / inactivos

## Customers

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/api/customers` | Admin | Lista (filtros: `search`, `active`) |
| GET | `/api/customers/{id}` | Admin | Detalle |
| POST | `/api/customers` | Admin | Crea (valida documento único) |
| PUT | `/api/customers/{id}` | Admin | Actualiza campos |
| DELETE | `/api/customers/{id}` | Admin | Soft-delete (IsActive = false) |

## Sales

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/api/sales` | Admin | Lista todas las ventas |
| GET | `/api/sales/{id}` | Autenticado | Detalle |
| POST | `/api/sales` | Admin, Cliente | Registra venta + envía email de confirmación |

Al crear una venta:
1. Valida que el cliente exista
2. Valida que todos los productos existan y estén activos
3. Captura el precio actual como `UnitPrice` en `SaleDetail`
4. Dispara email de confirmación en fire-and-forget

## DTOs y AutoMapper

`MappingProfile` registra los mapeos:

```
Product     → ProductDto
ProductCreateDto → Product

Customer    → CustomerDto
CustomerCreateDto → Customer

Sale        → SaleDto (con CustomerName y CustomerDocument)
SaleDetail  → SaleDetailDto (con ProductName y Subtotal calculado)
```

`ProductUpdateDto` y `CustomerUpdateDto` usan campos nullable y se aplican manualmente en el controller para hacer patch parcial sin sobrescribir campos no enviados.
