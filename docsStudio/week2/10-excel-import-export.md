# Importación y Exportación Excel — Week 2

## Descripción general

El sistema soporta dos flujos relacionados con Excel:

1. **Importación masiva** — se sube un archivo `.xlsx` con datos desnormalizados/mezclados; el sistema los normaliza e inserta o actualiza en las tablas correctas.
2. **Exportación** — se descarga el conjunto de datos actual (productos, clientes o ventas) como un archivo `.xlsx` formateado.

Ambos flujos están implementados con **EPPlus 7** (licencia NonCommercial).

---

## Archivos involucrados

```
Firmeza.Web/
├── Services/
│   ├── ExcelImportService.cs       ← lógica de importación masiva
│   └── ExcelExportService.cs       ← lógica de exportación por entidad
├── Pages/Admin/
│   └── Import/
│       ├── Index.cshtml            ← formulario de carga + tabla de referencia de columnas
│       └── Index.cshtml.cs         ← PageModel: maneja GET (formulario) y POST (procesa archivo)
└── Program.cs                      ← registra ExcelImportService y ExcelExportService como Scoped
```

---

## Importación masiva (`ExcelImportService`)

**Archivo:** `Firmeza.Web/Services/ExcelImportService.cs`

### Estrategia de detección

Cada hoja del archivo se inspecciona de forma independiente. La detección se basa en qué encabezados están presentes (sin distinguir mayúsculas/minúsculas, sin tildes):

| Condición | Detectado como |
|---|---|
| Tiene `Nombre`/`Producto` **y** `Precio` | Hoja de Productos |
| Tiene `Cliente`/`Nombre` **y** `Documento`/`Cedula` | Hoja de Clientes |
| Tiene `Documento_Cliente` **y** `Producto_Venta` **y** `Cantidad` | Hoja de Ventas |

Un mismo archivo puede tener múltiples hojas de distintos tipos — todas se procesan.

### Columnas requeridas por tipo

**Productos**
| Columna | Requerida | Alias aceptados |
|---|---|---|
| Nombre | Sí | Producto, Name |
| Precio | Sí | Price |
| Descripcion | No | Description |
| Categoria | No | Category |
| Unidad | No | Unit |
| Stock | No | Inventario, Cantidad |

**Clientes**
| Columna | Requerida | Alias aceptados |
|---|---|---|
| Cliente / Nombre | Sí | First_Name, Nombres |
| Documento | Sí | Cedula, RFC, Document_Number |
| Apellido | No | Last_Name, Apellidos |
| Telefono | No | Phone |
| Email | No | Correo |
| Direccion | No | Address |

**Ventas**
| Columna | Requerida | Notas |
|---|---|---|
| Documento_Cliente | Sí | Debe coincidir con un Cliente existente |
| Producto_Venta | Sí | Debe coincidir con un Producto existente |
| Cantidad | Sí | Entero > 0 |
| Precio_Unitario | No | Por defecto usa el precio actual del producto |
| Notas | No | |

### Lógica de upsert

- **Productos**: se busca por `Name`. Si existe → actualiza Precio, Stock, Categoría, Unidad, Descripción. Si no → inserta.
- **Clientes**: se busca por `DocumentNumber`. Si existe → actualiza campos. Si no → inserta.
- **Ventas**: siempre se insertan (una fila = una venta con un ítem). El Cliente y el Producto deben existir previamente.

### Registro de errores

`ImportResult` acumula todos los errores a nivel de fila (precio inválido, cliente inexistente, etc.). La interfaz muestra el conteo y cada mensaje después de la importación.

---

## Exportación (`ExcelExportService`)

**Archivo:** `Firmeza.Web/Services/ExcelExportService.cs`

Tres métodos, cada uno retorna `byte[]`:

| Método | Nombre de hoja | Endpoint |
|---|---|---|
| `ExportProductsAsync()` | Productos | `/Admin/Products/Index?handler=ExportExcel` |
| `ExportCustomersAsync()` | Clientes | `/Admin/Customers/Index?handler=ExportExcel` |
| `ExportSalesAsync()` | Ventas | `/Admin/Sales/Index?handler=ExportExcel` |

Los encabezados usan fondo índigo (`#4F46E5`) con texto blanco en negrita. Las columnas numéricas tienen formato `#,##0.00`. El ancho de columnas se ajusta automáticamente.

La exportación de ventas desnormaliza los datos: una fila por `SaleDetail` (cada línea de producto), repitiendo las columnas de cabecera de la venta.

---

## Páginas

### `/Admin/Import`

**Archivos:** `Firmeza.Web/Pages/Admin/Import/Index.cshtml` + `Index.cshtml.cs`

- `GET` — muestra el formulario de carga y la tabla de referencia de columnas aceptadas.
- `POST` — recibe el archivo, ejecuta `ExcelImportService.ImportAsync`, muestra el resumen del resultado.

La tabla de referencia en la página lista los nombres de columna aceptados para que los usuarios puedan preparar sus archivos correctamente antes de importar.

---

## Cómo funciona el flujo completo

```
Usuario sube archivo .xlsx
         ↓
Index.cshtml.cs → OnPostAsync()
         ↓
ExcelImportService.ImportAsync(stream)
         ↓
Para cada hoja:
  1. Lee encabezados → detecta tipo (Productos / Clientes / Ventas)
  2. Itera filas → valida campos requeridos
  3. Busca entidad existente en DB (por Name o DocumentNumber)
  4. Upsert: actualiza si existe, inserta si no
  5. Acumula errores en ImportResult
         ↓
Devuelve ImportResult con: filas procesadas, insertadas, actualizadas, errores
         ↓
La página muestra el resumen al usuario
```

---

## Registro de servicios en `Program.cs`

```csharp
builder.Services.AddScoped<ExcelImportService>();
builder.Services.AddScoped<ExcelExportService>();
```
