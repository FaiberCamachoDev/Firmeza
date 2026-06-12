# Estructura del Proyecto — Qué hace cada archivo

## El mapa completo

```
Firmeza/
├── Firmeza.slnx                        ← contenedor de proyectos
│
├── Firmeza.Web/                        ← app web (Razor Pages + servicios)
│   ├── Program.cs                      ← arranque + configuración de servicios
│   ├── appsettings.json                ← connection string + config
│   ├── Firmeza.Web.csproj              ← dependencias NuGet
│   │
│   ├── Data/
│   │   ├── ApplicationDbContext.cs     ← puerta de entrada a la DB
│   │   └── DbSeeder.cs                 ← datos iniciales (admin, roles, productos, clientes)
│   │
│   ├── Models/                         ← entidades = tablas en la DB
│   │   ├── ApplicationUser.cs
│   │   ├── Product.cs
│   │   ├── Customer.cs
│   │   ├── Sale.cs
│   │   └── SaleDetail.cs
│   │
│   ├── ViewModels/                     ← datos de formularios (NO son tablas)
│   │   ├── Auth/LoginViewModel.cs
│   │   ├── Products/ProductCreateViewModel.cs
│   │   ├── Products/ProductEditViewModel.cs
│   │   ├── Customers/CustomerCreateViewModel.cs
│   │   ├── Customers/CustomerEditViewModel.cs
│   │   └── Sales/SaleCreateViewModel.cs        ← [week 2] datos del formulario de ventas
│   │
│   ├── Services/                       ← [week 2] servicios de Excel y PDF
│   │   ├── ExcelImportService.cs       ← importación masiva desde .xlsx con EPPlus
│   │   ├── ExcelExportService.cs       ← exportación de Productos/Clientes/Ventas a .xlsx
│   │   └── PdfReceiptService.cs        ← generación de recibos PDF con QuestPDF
│   │
│   ├── Migrations/                     ← historial de cambios a la DB (auto-generado)
│   │   └── 20260604_InitialCreate.cs
│   │
│   ├── Pages/
│   │   ├── _ViewImports.cshtml         ← namespaces disponibles en todas las páginas
│   │   ├── _ViewStart.cshtml           ← layout por defecto para páginas públicas
│   │   ├── Auth/
│   │   │   ├── Login.cshtml + .cs      ← formulario de login
│   │   │   ├── Logout.cshtml + .cs     ← cierra sesión y redirige
│   │   │   └── AccessDenied.cshtml     ← pantalla 403
│   │   ├── Admin/
│   │   │   ├── Dashboard.cshtml + .cs  ← métricas generales
│   │   │   ├── Products/               ← CRUD productos (Index, Create, Edit, Delete)
│   │   │   ├── Customers/              ← CRUD clientes (Index, Create, Edit, Delete)
│   │   │   ├── Sales/                  ← [week 2] módulo de ventas
│   │   │   │   ├── Index.cshtml + .cs  ← listado de ventas + exportar Excel
│   │   │   │   ├── Create.cshtml + .cs ← nueva venta con filas dinámicas + genera PDF
│   │   │   │   └── Details.cshtml + .cs ← detalle + descarga/regeneración de PDF
│   │   │   └── Import/                 ← [week 2] importación masiva
│   │   │       └── Index.cshtml + .cs  ← formulario de carga Excel
│   │   └── Shared/
│   │       ├── _Layout.cshtml          ← layout HTML para páginas públicas (login)
│   │       └── _AdminLayout.cshtml     ← layout HTML completo para el panel admin
│   │
│   ├── wwwroot/
│   │   ├── css/
│   │   │   ├── tailwind.css            ← INPUT de Tailwind (directivas @tailwind)
│   │   │   └── app.css                 ← OUTPUT compilado (el que usa el browser)
│   │   └── recibos/                    ← [week 2] almacén de PDFs generados
│   │       └── .gitkeep
│   │
│   ├── package.json                    ← dependencias npm (tailwindcss)
│   └── tailwind.config.js              ← qué archivos escanea Tailwind
│
├── Firmeza.Api/                        ← [week 3] REST API con JWT
│   ├── Program.cs                      ← configuración completa: JWT, AutoMapper, Swagger, CORS
│   ├── appsettings.json                ← connection string, JWT config, email config
│   ├── Firmeza.Api.csproj              ← dependencias NuGet
│   ├── Controllers/
│   │   ├── AuthController.cs           ← login y registro
│   │   ├── ProductsController.cs       ← CRUD con filtros
│   │   ├── CustomersController.cs      ← CRUD con soft-delete
│   │   └── SalesController.cs          ← listado y creación de ventas
│   ├── DTOs/
│   │   ├── Auth/                       ← LoginDto, RegisterDto, TokenResponseDto
│   │   ├── Products/                   ← ProductDto, ProductCreateDto, ProductUpdateDto
│   │   ├── Customers/                  ← CustomerDto, CustomerCreateDto, CustomerUpdateDto
│   │   └── Sales/                      ← SaleDto, SaleCreateDto, SaleDetailDto
│   ├── Mappings/
│   │   └── MappingProfile.cs           ← todos los mapeos AutoMapper entidad ↔ DTO
│   ├── Services/
│   │   ├── JwtService.cs               ← genera tokens con claims
│   │   ├── IEmailService.cs            ← interfaz del servicio de email
│   │   └── EmailService.cs             ← implementación con MailKit + Gmail SMTP
│   └── Properties/
│       └── launchSettings.json         ← puertos: 5109 (HTTP), 7245 (HTTPS)
│
├── Firmeza.Tests/                      ← pruebas unitarias xUnit
│   ├── Firmeza.Tests.csproj
│   ├── Products/
│   │   └── ProductValidationTests.cs   ← [week 1] validaciones de ViewModel
│   └── Api/
│       ├── ProductsControllerTests.cs  ← [week 3] 9 tests
│       ├── EmailServiceTests.cs        ← [week 3] 4 tests
│       ├── AuthControllerTests.cs      ← [week 3] 6 tests
│       ├── CustomersControllerTests.cs ← [week 3] 10 tests
│       ├── JwtServiceTests.cs          ← [week 3] 7 tests
│       └── SalesControllerTests.cs     ← [week 3] 8 tests
│
├── Dockerfile                          ← imagen multi-stage para producción
├── docker-compose.yml                  ← orquestación app + DB
└── .env.example                        ← variables de entorno para docker
```

---

## ¿Por qué dos carpetas: Models vs ViewModels?

**Pruébalo antes de leer la respuesta:**

Imagina que tienes un formulario de "Crear Producto". ¿Qué pasa si usas directamente el modelo `Product` en el formulario?

```csharp
// Si haces esto en Create.cshtml.cs:
[BindProperty]
public Product Input { get; set; }  // ← ¿qué problema tiene?
```

El campo `Id` viene del formulario. El campo `CreatedAt` también. El campo `IsActive` también. Alguien podría manipular el HTML y enviarte `Id=999, IsActive=false, CreatedAt=2020-01-01`. **El usuario controla campos que no debería.**

**Por eso existen los ViewModels:** solo exponen los campos que el usuario puede tocar.

```
Product (entidad)          ProductCreateViewModel (formulario)
───────────────────        ──────────────────────────────────
Id          ←──────── NO   Name        ←── SÍ (usuario lo llena)
Name        ←──────── SÍ   Description ←── SÍ
CreatedAt   ←──────── NO   Unit        ←── SÍ
UpdatedAt   ←──────── NO   Price       ←── SÍ
IsActive    ←──────── NO   StockInput  ←── SÍ (string para validar)
```

**Regla:** Models = forma de la DB. ViewModels = forma del formulario. Son cosas distintas.

---

## ¿Por qué dos layouts: _Layout y _AdminLayout?

`_ViewStart.cshtml` asigna `_Layout` a TODAS las páginas por defecto. Ese layout es el HTML público (solo CSS, sin sidebar).

Las páginas de `/Admin/` sobreescriben eso explícitamente:
```csharp
@{
    Layout = "~/Pages/Shared/_AdminLayout.cshtml";  // ← pisa el default
}
```

`_AdminLayout.cshtml` es un HTML completo con `<head>`, CSS, sidebar, topbar y `@RenderBody()`. Las páginas admin inyectan su contenido donde está `@RenderBody()`.

**Bug clásico (ya resuelto):** si el layout no tiene `<head>` con el link al CSS, la página carga sin estilos. Eso pasó aquí — `_AdminLayout` era solo un `<div>`, sin el `<!DOCTYPE html>` completo.
