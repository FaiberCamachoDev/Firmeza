# Firmeza — Sistema de Gestión Administrativa

Sistema web para la gestión de productos y clientes de una distribuidora de materiales de construcción. Panel administrativo construido con ASP.NET Core 9 Razor Pages, PostgreSQL (Supabase) y Tailwind CSS.

## Stack

| Tecnología | Versión | Rol |
|---|---|---|
| ASP.NET Core | 9.0 | Framework web + Razor Pages |
| Entity Framework Core | 9.0.5 | ORM + Migraciones |
| Npgsql | 9.0.4 | Driver PostgreSQL |
| ASP.NET Core Identity | 9.0.5 | Autenticación + Roles |
| Tailwind CSS | 3.4.x | Estilos |
| QuestPDF | 2025.5.1 | Reportes PDF (módulos futuros) |
| EPPlus | 7.7.0 | Exportación Excel (módulos futuros) |
| xUnit | 2.x | Pruebas unitarias |
| Docker | — | Contenerización |

## Requisitos Previos

- .NET 9 SDK
- Node.js 18+
- PostgreSQL 14+ (o cuenta en Supabase)
- Docker + Docker Compose (para despliegue)

## Configuración Local

### 1. Clonar y restaurar dependencias

```bash
cd Firmeza/Firmeza.Web
dotnet restore
npm install
```

### 2. Configurar connection string

Edita `Firmeza/Firmeza.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=TU_HOST;Database=TU_DB;Username=TU_USER;Password=TU_PASSWORD;Port=5432;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

### 3. Compilar CSS

```bash
cd Firmeza/Firmeza.Web
npm run build:css
```

### 4. Aplicar migraciones y correr

```bash
cd Firmeza/Firmeza.Web
dotnet run
```

Las migraciones se aplican automáticamente al iniciar. El seed crea el usuario administrador:

- **Email**: `admin@firmeza.com`
- **Contraseña**: `Admin@123!`

## Despliegue con Docker

```bash
# Copiar y configurar variables de entorno
cp .env.example .env
# Editar .env con tus credenciales

# Levantar servicios
docker-compose up --build -d

# Ver logs
docker-compose logs -f web
```

La app estará disponible en `http://localhost:8080`.

## Pruebas

```bash
cd Firmeza
dotnet test
```

**Cobertura actual**: 10 tests — validaciones de ViewModel de Producto.

## Estructura del Proyecto

```
Module6-w1/
├── docsStudio/               ← Documentación técnica y decisiones
├── Firmeza/
│   ├── Firmeza.sln
│   ├── Firmeza.Web/          ← Proyecto principal
│   │   ├── Data/             ← DbContext + Seeder
│   │   ├── Models/           ← Entidades EF Core
│   │   ├── ViewModels/       ← Formularios con validaciones
│   │   ├── Pages/
│   │   │   ├── Auth/         ← Login, Logout, AccessDenied
│   │   │   └── Admin/        ← Dashboard, Products, Customers
│   │   └── wwwroot/css/      ← Tailwind compilado
│   └── Firmeza.Tests/        ← xUnit
├── Dockerfile
├── docker-compose.yml
└── .env.example
```

## Modelo Entidad-Relación

```
ApplicationUser (Identity)
       |
       |
   Customer ──────────── Sale ──────────── SaleDetail ─── Product
   [Id]                  [Id]              [Id]            [Id]
   [FirstName]           [CustomerId FK]   [SaleId FK]     [Name]
   [LastName]            [Total]           [ProductId FK]  [Price]
   [DocumentNumber]      [Status]          [Quantity]      [Stock]
   [Phone]               [CreatedAt]       [UnitPrice]     [Category]
   [Email]               [Notes]                           [Unit]
   [Address]
```

## Roles y Autenticación

| Rol | Acceso |
|---|---|
| `Admin` | Panel Razor completo — Dashboard, Productos, Clientes |
| `Cliente` | Bloqueado en Razor — solo frontends futuros (Blazor/React/Angular) |

## Comandos Útiles

```bash
# Agregar nueva migración
dotnet ef migrations add NombreMigracion

# Revertir última migración
dotnet ef migrations remove

# Aplicar migraciones manualmente
dotnet ef database update

# Desarrollo con hot-reload + Tailwind watch (dos terminales)
dotnet watch run
npm run watch:css
```

## Variables de Entorno (Producción)

| Variable | Descripción |
|---|---|
| `ConnectionStrings__DefaultConnection` | Connection string de PostgreSQL |
| `ASPNETCORE_ENVIRONMENT` | `Production` o `Development` |
| `POSTGRES_PASSWORD` | Password para el servicio DB en compose |
