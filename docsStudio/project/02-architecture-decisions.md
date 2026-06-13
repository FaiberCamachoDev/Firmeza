# Decisiones de Arquitectura — Firmeza

## UI Framework: Tailwind CSS en ASP.NET Core

**Decisión**: Tailwind CSS vía CLI (npm)  
**Por qué**: Mayor control de diseño; preferencia del equipo. Se integra con Razor Pages mediante un paso de build npm.

**Setup**:
- `tailwind.config.js` con `content: ["./Pages/**/*.cshtml", "./wwwroot/**/*.js"]`
- Input: `wwwroot/css/tailwind.css` (directivas `@tailwind base/components/utilities`)
- Output: `wwwroot/css/app.css` (compilado, referenciado en `_Layout.cshtml`)
- `package.json` con scripts `build:css` y `watch:css`

---

## Patrón: Razor Pages (Page-centric) vs MVC

**Decisión**: Razor Pages  
**Por qué**: El enunciado lo especifica explícitamente. Razor Pages es adecuado para paneles CRUD administrativos — cada página es su propio modelo, sin necesidad de controladores separados. La lógica vive en `PageModel`, lo que mantiene cohesión.

**Cuando usar MVC en cambio**: APIs REST o proyectos con mucha lógica de routing compleja.

---

## Patrón de Datos: Repository vs DbContext Directo

**Decisión**: DbContext directo en PageModels  
**Por qué**: Para un módulo administrativo de esta escala, el patrón Repository añade indirección sin beneficio real. EF Core ya es una abstracción sobre la DB. Se usa `ApplicationDbContext` directamente en los `PageModel`.

**Cuándo revisar**: Si se agrega una API REST paralela que necesite compartir lógica, extraer a servicios.

---

## Entidades y Relaciones

```
Product (1) ←→ (N) SaleDetail (N) ←→ (1) Sale (N) ←→ (1) Customer
```

### Decisiones de modelado

| Decisión | Detalle |
|---|---|
| `UnitPrice` en `SaleDetail` | Se guarda el precio al momento de la venta, no referencia al precio actual del producto. Esto preserva el historial correcto. |
| `ApplicationUser` extiende `IdentityUser` | Agrega `FirstName`, `LastName`, `DocumentNumber` al usuario de Identity |
| Soft delete | No implementado en este módulo — delete real por simplicidad |
| Timestamps | `CreatedAt` en `Sale`; `UpdatedAt` en `Product` y `Customer` |

---

## Autenticación y Roles

**Decisión**: Identity con `[Authorize(Roles = "Admin")]` en el área `/Admin`

### Flujo de login
1. Usuario envía credenciales en `/Auth/Login`
2. Identity valida y emite cookie de sesión
3. Si el rol es `Cliente`, se redirige a página de "acceso denegado"
4. Si el rol es `Admin`, accede al panel

### Seed de datos
- El seed crea un usuario `admin@firmeza.com` con rol `Admin` en startup
- Permite arrancar sin configuración manual en primera ejecución

---

## Estructura de Páginas Razor

```
Pages/
├── Auth/
│   ├── Login.cshtml          ← pública
│   └── AccessDenied.cshtml   ← pública
├── Admin/
│   ├── Dashboard.cshtml      ← [Authorize(Roles="Admin")]
│   ├── Products/
│   │   ├── Index.cshtml      ← listado + búsqueda
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   └── Customers/
│       ├── Index.cshtml
│       ├── Create.cshtml
│       ├── Edit.cshtml
│       └── Delete.cshtml
└── Shared/
    ├── _Layout.cshtml         ← layout con sidebar Bootstrap 5
    └── _ValidationScripts.cshtml
```

---

## ViewModels

Se usan ViewModels separados de las entidades para formularios:

| Entidad | ViewModel | Por qué |
|---|---|---|
| `Product` | `ProductCreateViewModel`, `ProductEditViewModel` | Validaciones específicas de formulario; no exponer campos internos |
| `Customer` | `CustomerCreateViewModel`, `CustomerEditViewModel` | Idem |
| `ApplicationUser` | `LoginViewModel`, `RegisterViewModel` | Identity requiere campos específicos |

---

## Validaciones

- **Data Annotations** en ViewModels (`[Required]`, `[MaxLength]`, `[EmailAddress]`, `[RegularExpression]`)
- **Client-side**: jQuery Validation + Unobtrusive (incluido con Bootstrap)
- **Server-side**: `ModelState.IsValid` en PageModels + try-catch para conversiones (Task 7)

---

## Migrations Strategy

- Una sola migración inicial: `InitialCreate`
- Si cambian entidades: nueva migración con nombre descriptivo
- `dotnet ef database update` se ejecuta en startup (via `MigrateAsync()`) en desarrollo
- En producción (Docker): migrations aplicadas al iniciar el contenedor

---

## Naming Conventions

| Elemento | Convención | Ejemplo |
|---|---|---|
| Tablas DB | PascalCase plural | `Products`, `Customers`, `Sales` |
| Columnas | PascalCase | `UnitPrice`, `CreatedAt` |
| ViewModels | `[Entidad][Acción]ViewModel` | `ProductCreateViewModel` |
| PageModels | `[Nombre]Model` (automático Razor) | `IndexModel`, `CreateModel` |
| Rutas | kebab-case automático via Razor | `/admin/products`, `/auth/login` |
