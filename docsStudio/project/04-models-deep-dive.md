# Modelos y Entidades — Cómo funcionan

Los modelos en `Models/` son clases C# que EF Core convierte en tablas PostgreSQL. Cada propiedad se vuelve una columna.

---

## Reto antes de leer

Abre `Models/Product.cs` y mira este campo:

```csharp
[Column(TypeName = "numeric(18,2)")]
public decimal Price { get; set; }
```

**Pregunta:** ¿Por qué no solo `public decimal Price { get; set; }` sin el `[Column]`?

Intenta adivinar antes de seguir.

---

## Qué hace `[Column(TypeName = "numeric(18,2)")]`

Sin ese atributo, EF Core mapea `decimal` a `numeric(18,2)` en PostgreSQL de todas formas. Entonces, ¿para qué sirve?

**Sirve para ser explícito y prevenir sorpresas.**

Sin el atributo, diferentes bases de datos o versiones de EF pueden elegir tipos distintos (`money`, `float`, `double precision`). Al especificarlo, tú decides exactamente qué tipo usa PostgreSQL, sin importar qué versión de EF o qué DB tengas.

**Analogía física:** es como especificar "tornillo M4 de acero inoxidable de 20mm" en lugar de solo "tornillo". El carpintero podría adivinar bien, pero tú no lo dejás a la suerte.

`numeric(18,2)` significa:
- `18` dígitos en total
- `2` decimales
- Exacto (no aproximado como `float`) — fundamental para dinero

---

## El resto de los atributos — rómpilos para entenderlos

### `[Required]`

```csharp
[Required, MaxLength(150)]
public string Name { get; set; } = string.Empty;
```

**Prueba:** en `Create.cshtml`, envía el formulario sin escribir nombre. ¿Qué pasa?

Aparece un error de validación antes de llegar al servidor — jQuery Validation lo intercepta en el browser. Si alguien deshabilita JS, el servidor lo valida en `ModelState.IsValid`.

**Rompiéndolo:** quita `[Required]` del ViewModel (no del modelo). Envía sin nombre. ¿Llega a la base de datos? Sí. ¿Qué error da PostgreSQL si la columna tiene `NOT NULL`? Pruébalo.

### `[MaxLength(150)]`

Dos efectos:
1. Genera `character varying(150)` en la columna de PostgreSQL (en lugar de `text`)
2. Valida en el servidor que la string no exceda 150 chars

Sin `[MaxLength]` → EF genera `text` (sin límite). Con él → `character varying(N)`.

### `[RegularExpression]` en CustomerCreateViewModel

```csharp
[RegularExpression(@"^[0-9]{6,15}$", ErrorMessage = "...")]
public string DocumentNumber { get; set; }
```

**Prueba:** en el formulario de crear cliente, escribe "abc123" como documento. ¿Qué pasa?

El regex `^[0-9]{6,15}$` dice:
- `^` = desde el inicio de la string
- `[0-9]` = solo dígitos del 0 al 9
- `{6,15}` = entre 6 y 15 de ellos
- `$` = hasta el final

**Rompiéndolo:** cambia `{6,15}` por `{6}`. ¿Un documento de 10 dígitos pasa o falla ahora?

---

## Por qué `SaleDetail.Subtotal` no va a la DB

```csharp
[Column(TypeName = "numeric(18,2)")]
public decimal Subtotal => Quantity * UnitPrice;  // propiedad calculada
```

Y en `ApplicationDbContext`:
```csharp
e.Ignore(d => d.Subtotal);
```

**Prueba mental:** si guardas `Subtotal` en la DB, ¿qué problema aparece si después corriges el `UnitPrice`?

Tendrías dos verdades: el `Subtotal` almacenado y `Quantity * UnitPrice` recalculado. Cuando difieren, ¿cuál es el correcto?

Al ignorar el campo en la DB y calcularlo siempre en tiempo real, hay una sola fuente de verdad: `Quantity` y `UnitPrice`. El subtotal es una consecuencia, no un dato.

---

## Por qué `UnitPrice` se guarda en SaleDetail y no se referencia desde Product

```csharp
public class SaleDetail
{
    public decimal UnitPrice { get; set; }   // precio al momento de la venta
    public Product Product { get; set; }     // referencia al producto
}
```

**Prueba mental:** si una venta referencia `Product.Price` directamente, ¿qué pasa cuando el precio del producto sube de $10 a $15 el mes siguiente?

Todas las ventas antiguas mostrarían $15. El historial contable estaría roto.

Al guardar `UnitPrice` en `SaleDetail`, congelamos el precio de ese momento. El producto puede cambiar de precio mil veces — las ventas ya registradas siempre muestran lo que costó cuando se vendió.

---

## Relaciones entre entidades

```
Customer  ──(1:N)──  Sale  ──(1:N)──  SaleDetail  ──(N:1)──  Product
```

En código:

```csharp
// En Customer.cs
public ICollection<Sale> Sales { get; set; } = [];

// En Sale.cs
public int CustomerId { get; set; }          // FK (columna en DB)
public Customer Customer { get; set; }       // navigation property (objeto cargado)

// En SaleDetail.cs
public int SaleId { get; set; }
public int ProductId { get; set; }
```

**¿Qué es una navigation property?** Es la propiedad que te da acceso al objeto relacionado. `Sale.Customer` no es una columna en la tabla `Sales` — es EF cargando el objeto `Customer` cuando lo necesitas (con `.Include()`).

**La columna real en la DB** es `CustomerId` (el `int`). La navigation property es un helper en memoria.

**Prueba:** en `DashboardModel.cs`, agrega `.Include(s => s.Customer)` al query de ventas. Sin él, `sale.Customer` es `null`. Con él, está cargado.

---

## `OnDelete(DeleteBehavior.Restrict)` vs `Cascade`

```csharp
// Sale → Customer: Restrict
.OnDelete(DeleteBehavior.Restrict)

// SaleDetail → Sale: Cascade
.OnDelete(DeleteBehavior.Cascade)
```

**¿Qué significa cada uno?**

`Restrict`: si intentas eliminar un `Customer` que tiene `Sale`s, la DB lanza error. No se puede borrar mientras haya referencias.

`Cascade`: si eliminas una `Sale`, todos sus `SaleDetail`s se borran automáticamente. La venta no puede existir sin la cabecera.

**¿Por qué Restrict en Customer→Sale?** Porque no queremos perder el historial de ventas si alguien toca el botón "eliminar cliente" por error. La DB actúa como red de seguridad.

**Rompiéndolo:** cambia `Restrict` a `Cascade` en Customer→Sale. Genera una migración y prueba eliminar un cliente con ventas. ¿Qué pasó con las ventas? ¿Era el comportamiento correcto para un negocio real?
