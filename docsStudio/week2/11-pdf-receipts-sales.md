# Recibos PDF y Módulo de Ventas — Week 2

## Descripción general

Al registrar una venta, el sistema genera automáticamente un recibo PDF y lo almacena en `wwwroot/recibos/`. El recibo puede descargarse desde la página de detalles de la venta en cualquier momento. Un botón "Regenerar" permite recrear el PDF si el archivo no existe.

---

## Archivos involucrados

```
Firmeza.Web/
├── Services/
│   └── PdfReceiptService.cs                    ← genera y guarda recibos PDF con QuestPDF
├── Pages/Admin/Sales/
│   ├── Index.cshtml + Index.cshtml.cs           ← listado de ventas + botón exportar Excel
│   ├── Create.cshtml + Create.cshtml.cs         ← formulario de nueva venta con filas dinámicas
│   └── Details.cshtml + Details.cshtml.cs       ← detalle + descarga/regeneración de PDF
├── ViewModels/Sales/
│   └── SaleCreateViewModel.cs                   ← datos del formulario de creación
└── wwwroot/recibos/
    └── .gitkeep                                 ← carpeta rastreada por git; los PDFs no se commitean
```

---

## PdfReceiptService

**Archivo:** `Firmeza.Web/Services/PdfReceiptService.cs`  
**Librería:** QuestPDF (licencia Community, ya configurada en `Program.cs`)

### Nombre del archivo generado

```
recibo-{saleId:D6}-{createdAt:yyyyMMdd}.pdf
```

Ejemplo: `recibo-000001-20260610.pdf`

### Estructura del recibo

| Sección | Contenido |
|---|---|
| Encabezado | Título "FIRMEZA" + subtítulo "Comprobante de Venta", borde inferior índigo |
| Datos de la venta | Número de venta, fecha, estado, notas opcionales |
| Datos del cliente | Nombre completo, documento, teléfono, email, dirección |
| Tabla de productos | Nombre del producto, cantidad, precio unitario, subtotal; colores alternados por fila |
| Totales | Subtotal, IVA 16%, **Total** |
| Pie de página | "Firmeza — Sistema Administrativo" + marca de tiempo de generación |

### IVA

El IVA se calcula como el **16%** del subtotal en el momento de la generación (solo para visualización — no se persiste de forma separada en la base de datos).

### Métodos públicos

```csharp
string Generate(Sale sale)              // Genera y guarda el PDF, retorna la URL relativa
bool   Exists(int saleId, DateTime)     // Verifica si el archivo existe en disco
string GetRelativePath(int, DateTime)   // Retorna /recibos/recibo-XXXXXX-YYYYMMDD.pdf
```

---

## Páginas del módulo de ventas

### `/Admin/Sales` (Index)

**Archivos:** `Firmeza.Web/Pages/Admin/Sales/Index.cshtml` + `Index.cshtml.cs`

- Lista todas las ventas ordenadas por fecha descendente.
- Cada fila muestra: número de venta, fecha, cliente, estado, total.
- Aparece un badge PDF cuando el archivo de recibo existe.
- Botón "Exportar Excel" → descarga `ventas-YYYYMMDD.xlsx`.

### `/Admin/Sales/Create`

**Archivos:** `Firmeza.Web/Pages/Admin/Sales/Create.cshtml` + `Create.cshtml.cs`  
**ViewModel:** `Firmeza.Web/ViewModels/Sales/SaleCreateViewModel.cs`

- Dropdown para selección de cliente (solo clientes activos).
- Filas de productos dinámicas: JavaScript agrega/elimina filas, cada una con un dropdown de producto y un campo de cantidad.
- Vista previa del total calculada en el cliente en cada cambio.
- Al enviar: crea los registros `Sale` + `SaleDetail`, luego llama a `PdfReceiptService.Generate`.
- Redirige a la página de Detalles al tener éxito.

### `/Admin/Sales/Details/{id}`

**Archivos:** `Firmeza.Web/Pages/Admin/Sales/Details.cshtml` + `Details.cshtml.cs`

- Muestra la venta completa: datos del cliente, tabla de productos y totales con IVA.
- Enlace "Descargar recibo PDF" cuando el archivo existe.
- Botón "Generar PDF" / "Regenerar PDF" mediante POST `?handler=Regenerate`.

---

## Cómo funciona el flujo completo

```
Usuario llena formulario en /Admin/Sales/Create
         ↓
Create.cshtml.cs → OnPostAsync()
  1. Valida ModelState
  2. Carga cliente activo desde DB
  3. Para cada ítem: carga producto, calcula subtotal, crea SaleDetail
  4. Calcula total = suma de subtotales
  5. Guarda Sale + SaleDetails en DB
  6. Llama PdfReceiptService.Generate(sale)
         ↓
PdfReceiptService.Generate(sale)
  1. Carga relaciones (Customer, SaleDetails.Product) si no están cargadas
  2. Construye documento QuestPDF con secciones definidas
  3. Guarda en wwwroot/recibos/recibo-{id}-{fecha}.pdf
  4. Retorna URL relativa del archivo
         ↓
Redirige a /Admin/Sales/Details/{id}
  → El usuario puede descargar el PDF inmediatamente
```

---

## Almacenamiento de archivos

Los recibos se guardan en `wwwroot/recibos/` y se sirven como archivos estáticos mediante el middleware `UseStaticFiles()` ya configurado. No se requiere configuración adicional.

La carpeta incluye un `.gitkeep` para que sea rastreada por git pero el contenido (los PDFs) no se suba al repositorio.

---

## Registro de servicios en `Program.cs`

```csharp
builder.Services.AddScoped<PdfReceiptService>();
```

`PdfReceiptService` recibe `IWebHostEnvironment` por inyección de dependencias para resolver la ruta absoluta de `wwwroot/recibos/`.
