# Estructura del Proyecto — Qué hace cada archivo

## El mapa completo

```
Firmeza/
├── Firmeza.sln                        ← contenedor de proyectos
├── Firmeza.Web/                       ← la app web
│   ├── Program.cs                     ← arranque + configuración de servicios
│   ├── appsettings.json               ← connection string + config
│   ├── Firmeza.Web.csproj             ← dependencias NuGet
│   │
│   ├── Data/
│   │   ├── ApplicationDbContext.cs    ← puerta de entrada a la DB
│   │   └── DbSeeder.cs                ← datos iniciales (admin, roles)
│   │
│   ├── Models/                        ← entidades = tablas en la DB
│   │   ├── ApplicationUser.cs
│   │   ├── Product.cs
│   │   ├── Customer.cs
│   │   ├── Sale.cs
│   │   └── SaleDetail.cs
│   │
│   ├── ViewModels/                    ← datos de formularios (NO son tablas)
│   │   ├── Auth/LoginViewModel.cs
│   │   ├── Products/ProductCreateViewModel.cs
│   │   ├── Products/ProductEditViewModel.cs
│   │   ├── Customers/CustomerCreateViewModel.cs
│   │   └── Customers/CustomerEditViewModel.cs
│   │
│   ├── Migrations/                    ← historial de cambios a la DB (auto-generado)
│   │   └── 20260604_InitialCreate.cs
│   │
│   ├── Pages/
│   │   ├── _ViewImports.cshtml        ← namespaces disponibles en todas las páginas
│   │   ├── _ViewStart.cshtml          ← layout por defecto para páginas públicas
│   │   ├── Auth/
│   │   │   ├── Login.cshtml + .cs     ← formulario de login
│   │   │   ├── Logout.cshtml + .cs    ← cierra sesión y redirige
│   │   │   └── AccessDenied.cshtml    ← pantalla 403
│   │   ├── Admin/
│   │   │   ├── Dashboard.cshtml + .cs ← métricas generales
│   │   │   ├── Products/              ← CRUD productos
│   │   │   └── Customers/             ← CRUD clientes
│   │   └── Shared/
│   │       ├── _Layout.cshtml         ← layout HTML para páginas públicas (login)
│   │       └── _AdminLayout.cshtml    ← layout HTML completo para el panel admin
│   │
│   ├── wwwroot/
│   │   └── css/
│   │       ├── tailwind.css           ← INPUT de Tailwind (directivas @tailwind)
│   │       └── app.css                ← OUTPUT compilado (el que usa el browser)
│   │
│   ├── package.json                   ← dependencias npm (tailwindcss)
│   └── tailwind.config.js             ← qué archivos escanea Tailwind
│
├── Firmeza.Tests/
│   └── Products/ProductValidationTests.cs
│
├── Dockerfile                         ← imagen para producción
├── docker-compose.yml                 ← orquestación app + DB
└── .env.example                       ← variables de entorno para docker
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
