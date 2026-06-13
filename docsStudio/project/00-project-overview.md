# Firmeza — Sistema de Gestión

## Descripción General

**Firmeza** es un sistema de gestión para una empresa distribuidora de materiales de construcción. Comprende tres capas implementadas a lo largo de tres semanas:

1. **Week 1** — Panel administrativo Razor Pages
2. **Week 2** — Importación masiva Excel, exportación y recibos PDF
3. **Week 3** — API RESTful con JWT, AutoMapper, Swagger y email SMTP

## Stack Tecnológico

| Capa | Tecnología | Versión |
|---|---|---|
| Razor Pages (admin) | ASP.NET Core Razor Pages | .NET 9 |
| REST API | ASP.NET Core Web API | .NET 9 |
| Base de Datos | PostgreSQL | 16.x |
| ORM | Entity Framework Core + Npgsql | 9.x |
| Autenticación | Identity + JWT Bearer | 9.x |
| Mapeo | AutoMapper | 15.x |
| Documentación API | Swashbuckle (Swagger) | 7.x |
| Reportes | QuestPDF + EPPlus | instalado |
| Email | MailKit (Gmail SMTP) | 4.x |
| Pruebas | xUnit + InMemory DB | 2.x |
| Contenedores | Docker + Docker Compose | — |

## Proyectos en la Solución

```
Module6-w1/
├── docsStudio/
│   ├── project/         ← docs week 1
│   ├── week2/           ← docs week 2
│   └── week3/           ← docs week 3
├── Firmeza/
│   ├── Firmeza.slnx
│   ├── Firmeza.Web/     ← Razor Pages + servicios Excel/PDF
│   ├── Firmeza.Api/     ← REST API
│   └── Firmeza.Tests/   ← xUnit (23 tests)
├── Dockerfile
└── docker-compose.yml
```

## Módulos por Semana

### Week 1

| Task | Descripción | Estado |
|---|---|---|
| 1 | Proyecto Razor + dependencias | ✅ |
| 2 | PostgreSQL + ApplicationDbContext | ✅ |
| 3 | Entidades: Product, Customer, Sale, SaleDetail | ✅ |
| 4 | Identity: roles Admin / Cliente | ✅ |
| 5 | Dashboard administrativo | ✅ |
| 6 | CRUD Productos | ✅ |
| 7 | Validación y manejo de errores | ✅ |
| 8 | CRUD Clientes | ✅ |
| 9 | Tailwind CSS + diseño uniforme | ✅ |
| 10 | Documentación + docsStudio | ✅ |
| 11 | Pruebas xUnit iniciales | ✅ |
| 12 | Dockerfile + docker-compose | ✅ |

### Week 2 (rama `week2/excel-import-pdf-receipts`)

| Task | Descripción | Estado |
|---|---|---|
| 1 | ExcelImportService — carga masiva desnormalizada con EPPlus | ✅ |
| 2 | ExcelExportService — exportación Productos/Clientes/Ventas | ✅ |
| 3 | PdfReceiptService — recibos PDF con QuestPDF | ✅ |
| 4 | Módulo de Ventas (Create, Index, Details) en Razor Pages | ✅ |
| 5 | `wwwroot/recibos/` — almacenamiento y descarga de PDFs | ✅ |

### Week 3 (rama `week3/api-rest-jwt-swagger`)

| Task | Descripción | Estado |
|---|---|---|
| 1 | Proyecto `Firmeza.Api` agregado a la solución | ✅ |
| 2 | Misma DB PostgreSQL que Firmeza.Web | ✅ |
| 3 | Identity + JWT + rol Cliente | ✅ |
| 4 | AutoMapper 14 + DTOs (Products, Customers, Sales, Auth) | ✅ |
| 5+6 | Controladores CRUD con filtros y autorización por rol | ✅ |
| 7 | Swagger con esquema Bearer JWT | ✅ |
| 8 | EmailService Gmail SMTP (IEmailService, MailKit) | ✅ |
| 9 | Pruebas unitarias (total: 48 passing) | ✅ |

## Roles de Usuario

| Rol | Razor Pages | API |
|---|---|---|
| `Admin` | Acceso total al panel | CRUD completo en todos los endpoints |
| `Cliente` | Sin acceso | Registrar ventas, ver su perfil |
| Anónimo | Sin acceso | GET productos, login, register |
