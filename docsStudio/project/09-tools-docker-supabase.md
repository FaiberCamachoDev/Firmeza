# Herramientas del Proyecto — Docker, Supabase, xUnit, EPPlus, QuestPDF

---

## Docker — dos archivos, dos propósitos

### `Dockerfile` — la receta de la imagen

```dockerfile
# Stage 1: construye el CSS con Node.js
FROM node:20-alpine AS css-builder
WORKDIR /app
COPY package.json ./
RUN npm install
# ... genera app.css

# Stage 2: compila la app .NET
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# ... dotnet publish

# Stage 3: imagen final liviana (solo runtime, sin SDK)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Firmeza.Web.dll"]
```

**Multi-stage build:** el Stage 3 solo incluye el runtime de .NET (imagen liviana ~200MB). No lleva el SDK de Node ni el SDK de .NET que solo se necesitan para compilar.

**Analogía física:** cocinar una torta y servirla en un plato limpio, sin llevar toda la cocina a la mesa.

### `docker-compose.yml` — orquestación de servicios

```yaml
services:
  web:                          # la app ASP.NET Core
    build: .
    depends_on:
      db:
        condition: service_healthy  # espera a que Postgres esté listo

  db:                           # PostgreSQL local
    image: postgres:16-alpine
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U firmeza_user -d firmeza_db"]
```

**`service_healthy`:** el contenedor `web` no arranca hasta que el healthcheck de `db` pase. Sin esto, la app intentaría conectarse antes de que Postgres esté listo y crashearía.

**`depends_on` sin `condition: service_healthy`** (el error común) solo garantiza que el contenedor de DB *arrancó*, no que Postgres está *listo para conexiones*. La diferencia importa.

**Para deploy:**
```bash
cp .env.example .env        # completar con tus credentials
docker-compose up --build   # construye y levanta
```

---

## Supabase — por qué el pooler y no la conexión directa

El proyecto tiene registros DNS:
```
db.vhtrntyrmtfqkjjvkhdd.supabase.co  →  IPv6 only (2600:1f14:...)
aws-1-us-west-2.pooler.supabase.com  →  IPv4 (44.225.139.66)
```

Proyectos nuevos de Supabase usan IPv6 para la conexión directa. Máquinas sin IPv6 (la mayoría de ISPs residenciales en LATAM) no pueden conectarse.

**La solución:** usar el **Session Pooler** en `pooler.supabase.com`. Tiene IPv4 y soporta DDL/transacciones — necesario para EF Core Migrations.

| | Direct (5432) | Session Pooler (5432) | Transaction Pooler (6543) |
|---|---|---|---|
| IPv4 | ❌ | ✅ | ✅ |
| Migraciones DDL | ✅ | ✅ | ❌ |
| Username | `postgres` | `postgres.projectref` | `postgres.projectref` |

**Para Docker local:** `docker-compose.yml` levanta su propio Postgres en `localhost:5433`. No depende de Supabase. Para usar Supabase en Docker, pasar la connection string vía `.env`.

---

## xUnit — las pruebas y qué testean

```
Firmeza.Tests/
└── Products/
    └── ProductValidationTests.cs   ← 10 tests
```

### ¿Qué testeamos y por qué?

**Las Data Annotations en el ViewModel** — porque son la primera línea de validación. Si `[Required]` no funciona como esperamos, los usuarios pueden crear productos sin nombre.

```csharp
[Fact]
public void ProductCreate_EmptyName_FailsValidation()
{
    var vm = new ProductCreateViewModel { Name = "" };
    var errors = Validate(vm);
    Assert.Contains(errors, e => e.MemberNames.Contains(nameof(vm.Name)));
}
```

**`[Fact]`:** test sin parámetros. Corre una vez.

**`[Theory] + [InlineData]`:** el mismo test con múltiples inputs:

```csharp
[Theory]
[InlineData("abc", false)]   // "abc" no es entero válido → debe fallar
[InlineData("0", true)]      // "0" es entero válido → debe pasar
[InlineData("50", true)]
public void StockInput_ParseValidation(string input, bool shouldSucceed)
{
    bool isValid = int.TryParse(input, out int value) && value >= 0;
    Assert.Equal(shouldSucceed, isValid);
}
```

Un `[Theory]` reemplaza 4 `[Fact]`s. Más datos de prueba = menos código.

**Reto:** agrega este test al archivo:
```csharp
[Fact]
public void ProductCreate_NegativePrice_FailsValidation()
{
    var vm = new ProductCreateViewModel
    {
        Name = "Test",
        Unit = "u",
        Price = -1m,
        StockInput = "10"
    };
    var errors = Validate(vm);
    // ¿Qué deberías Assert aquí?
}
```

Corre `dotnet test` antes de escribir el `Assert`. ¿Pasa o falla? ¿Por qué?

---

## EPPlus — qué es y por qué está instalado sin usarse

```xml
<PackageReference Include="EPPlus" Version="7.7.0" />
```

**EPPlus** genera archivos Excel (`.xlsx`) programáticamente desde C#. En módulos futuros se usará para:
- Exportar listado de productos a Excel
- Exportar reporte de ventas
- Importar productos masivamente desde un archivo Excel

Está instalado en este módulo por requerimiento del enunciado — para que el proyecto ya tenga la dependencia cuando se implemente.

**Licencia:** EPPlus 5+ requiere licencia comercial o declarar uso no comercial:
```csharp
// En Program.cs (cuando se active):
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
```

---

## QuestPDF — qué es y por qué está instalado sin usarse

```csharp
// En Program.cs:
QuestPDF.Settings.License = LicenseType.Community;
```

**QuestPDF** genera PDFs desde C# con una API fluent. Se usará para:
- Facturas/recibos de ventas en PDF
- Reportes de inventario en PDF

Está activo en `Program.cs` solo para declarar la licencia Community (gratuita para proyectos open source/educativos).

**Diferencia con iTextSharp:** QuestPDF tiene API moderna orientada a .NET, mejor documentación, y licencia Community clara. iTextSharp tiene API más verbosa heredada de Java.

---

## `dotnet-ef` — la herramienta de migraciones

```bash
# Instalar globalmente (ya instalada en este entorno):
dotnet tool install --global dotnet-ef

# Comandos del día a día:
dotnet ef migrations add NombreDeLaMigracion   # crear migración
dotnet ef migrations remove                    # deshacer la última migración
dotnet ef database update                      # aplicar migraciones manualmente
dotnet ef database drop                        # borrar la DB (cuidado)
```

`dotnet ef` lee el `ApplicationDbContext`, compara el estado actual de los modelos con lo que ya está en la DB, y genera el SQL necesario como archivo C# en `Migrations/`.

**Nunca edites manualmente los archivos en `Migrations/`.** Son generados — si los tocás, EF pierde el rastro.

---

## `npx skills add` vs `npx cct@latest`

Dos gestores de skills que coexisten:

| | `npx cct@latest` | `npx skills add` |
|---|---|---|
| Fuente | aitmpl.com (davila7/claude-code-templates) | skills.sh (cualquier repo GitHub) |
| Instalación | `.claude/settings.json` | `.agents/skills/` en el proyecto |
| Ejemplo | `npx cct@latest --skill database/postgres-schema-design` | `npx skills add supabase/agent-skills` |
| Skills instaladas | Skills globales de la plataforma aitmpl | Skills específicas de providers (Supabase, etc.) |

Las skills de Supabase instaladas (`supabase` y `supabase-postgres-best-practices`) viven en:
```
Firmeza.Web/.agents/skills/supabase/
Firmeza.Web/.agents/skills/supabase-postgres-best-practices/
```

Claude las usa automáticamente cuando trabaja en este proyecto para mejorar sugerencias relacionadas con Supabase y PostgreSQL.
