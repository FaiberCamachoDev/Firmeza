# Herramientas, Skills y Hooks — Module6-w1

## Contexto de Selección

Referencia base: `/home/msi/Escritorio/claude-tools-reference/`  
Herramientas disponibles: 837+ skills, 87 MCPs, 45+ hooks vía `npx cct@latest`

---

## Hooks Globales (ya instalados — aplican automáticamente)

| Hook | Por qué aplica aquí |
|---|---|
| `git/conventional-commits` | Commits del proyecto siguen `feat:`, `fix:`, `chore:`, etc. |
| `git/validate-branch-name` | Branches siguen Git Flow: `feature/`, `hotfix/`, `release/` |
| `git/prevent-direct-push` | Nadie empuja directo a `main` o `develop` |
| `development-tools/file-backup` | Cada edit crea backup en `.backups/` — seguridad ante errores |
| `monitoring/desktop-notification-on-stop` | Notificación cuando Claude termina generación larga |
| `statusline/context-monitor` | Monitoreo de context window y costo de sesión |

---

## Skills Locales — Instaladas para este Proyecto

### `database/postgres-schema-design`

**Por qué**: El proyecto usa PostgreSQL con EF Core Migrations. Esta skill aporta:
- Mejores prácticas de naming en tablas/columnas
- Estrategias de indexado para búsquedas (productos por nombre, clientes por documento)
- Convenciones de tipos de dato PostgreSQL (varchar vs text, numeric vs decimal)

**Cómo instalar (local)**:
```bash
cd /home/msi/Escritorio/modules-development/Module6-w1/Firmeza
npx cct@latest --skill database/postgres-schema-design --yes
```

### `database/database-architect`

**Por qué**: Definir las relaciones entre `Product`, `Customer`, `Sale`, `SaleDetail` con buenas prácticas de modelado:
- Llaves foráneas con comportamiento de cascada correcto
- Normalización vs. desnormalización en `SaleDetail` (guardar precio al momento de la venta)
- Diseño pensado para escalar a módulos futuros (Blazor, React, etc.)

**Cómo instalar (local)**:
```bash
npx cct@latest --skill database/database-architect --yes
```

### `development/docker-expert`

**Por qué**: Task 12 requiere Dockerfile + docker-compose con dos servicios (`web` + `postgres`).
- Multi-stage builds para imagen pequeña de producción
- Variables de entorno para connection strings
- Health checks en el servicio de DB antes de levantar la app

**Cómo instalar (local)**:
```bash
npx cct@latest --skill development/docker-expert --yes
```

---

## Skills No Disponibles (conocimiento nativo de Claude)

| Necesidad | Skill en aitmpl.com | Decisión |
|---|---|---|
| ASP.NET Core Razor Pages | No existe | Claude conoce el framework nativamente |
| Entity Framework Core | No existe | Claude conoce EF Core nativamente |
| ASP.NET Core Identity | No existe | Claude conoce Identity nativamente |
| xUnit | No existe | Claude conoce xUnit nativamente |
| C# best practices | No existe | Claude conoce C# nativamente |

**Por qué esto es aceptable**: aitmpl.com está orientado principalmente a stacks JS/Python. El ecosistema .NET es conocimiento nativo de Claude sin necesidad de skill adicional.

---

## Skills Instaladas vía `npx skills add` (nuevo gestor)

### `supabase/agent-skills` — 2 skills instaladas

**Comando usado**:
```bash
npx skills add supabase/agent-skills
```

**Por qué**: Instalador diferente al de `cct@latest` — proviene de `https://github.com/supabase/agent-skills.git`. Instala en `.agents/skills/` dentro del proyecto.

**Skills instaladas**:

| Skill | Ruta | Qué aporta |
|---|---|---|
| `supabase` | `.agents/skills/supabase` | Patrones de uso de Supabase API, auth, storage, realtime |
| `supabase-postgres-best-practices` | `.agents/skills/supabase-postgres-best-practices` | Buenas prácticas PostgreSQL en contexto Supabase: índices, RLS, tipos |

**Relevancia para el proyecto**:
- `supabase-postgres-best-practices` guía las decisiones de schema y query optimization al usar Supabase como host de PostgreSQL
- Útil para futuros módulos que expongan datos vía Supabase REST API o Realtime

---

## Incidente: IPv6 vs IPv4 en Supabase (resuelto)

**Problema**: La conexión directa `db.xxxx.supabase.co:5432` solo tiene registro DNS IPv6 (AAAA). La máquina de desarrollo no tiene conectividad IPv6 → `Network is unreachable`.

**Solución**: Usar el **Session Pooler** de Supabase que tiene IPv4:
- Host: `aws-1-us-west-2.pooler.supabase.com` (IPv4: `44.225.139.66`)
- Username: `postgres.[project-ref]` (incluye el ref del proyecto)
- Puerto: `5432` (session mode — soporta DDL y transacciones, necesario para EF Core migrations)

**Por qué Session y no Transaction pooler (6543)**:
EF Core Migrations ejecuta DDL en transacciones explícitas. El Transaction pooler (port 6543) de PgBouncer no soporta esto — las migraciones fallarían. Session mode (5432) sí soporta el ciclo completo.

---

## MCPs Opcionales para este Proyecto

### `database/postgresql` (recomendado durante desarrollo)

**Por qué**: Permite a Claude hacer queries directos a PostgreSQL para:
- Verificar que las migraciones aplicaron correctamente
- Inspeccionar datos de seed
- Debuggear relaciones

**Requiere**: Connection string configurada  
**Cómo instalar**:
```bash
npx cct@latest --mcp database/postgresql --yes
```
**Config en `.claude/settings.json`**:
```json
{
  "env": {
    "DATABASE_URL": "postgres://firmeza_user:password@localhost:5432/firmeza_db"
  }
}
```

---

## Hooks Locales — No Instalados (y por qué)

| Hook | Motivo de no instalar |
|---|---|
| `development-tools/lint-on-save` | Orientado a JS/TS. Para C# usamos `dotnet format` manual |
| `development-tools/smart-formatting` | No soporta C# nativamente |
| `automation/build-on-change` | El dev server de Razor Pages tiene hot-reload nativo con `dotnet watch` |

---

## Configuración Local del Proyecto (.claude/settings.json)

Archivo a crear en `Module6-w1/Firmeza/.claude/settings.json`:

```json
{
  "permissions": {
    "allow": [
      "Bash(dotnet:*)",
      "Bash(psql:*)",
      "Bash(docker:*)",
      "Bash(docker-compose:*)"
    ]
  }
}
```

**Por qué estos permisos**:
- `dotnet:*` — para `dotnet build`, `dotnet ef migrations add`, `dotnet run`, `dotnet test`
- `psql:*` — para verificar estado de la base de datos
- `docker:*` y `docker-compose:*` — para build y run del entorno contenedorizado

---

## Resumen de Decisiones

| Decisión | Alternativa Considerada | Por qué la elegida |
|---|---|---|
| .NET 9 | .NET 8 (LTS) | Disponible en el sistema; suficientemente estable |
| Tailwind CSS | Bootstrap 5 | Preferencia del equipo; más control de diseño |
| Supabase (PostgreSQL managed) | PostgreSQL local | Evita configuración local; DB siempre disponible |
| QuestPDF | iTextSharp | Licencia open-source más clara; API más moderna |
| Migraciones EF Core | Scripts SQL manuales | Requisito explícito del enunciado |
| EPPlus | ClosedXML | Requisito explícito del enunciado (instalado) |

### Tailwind en ASP.NET Core — Estrategia

Tailwind requiere un paso de build para generar el CSS final. En Razor Pages se hace con:

1. `npm init` + `npm install tailwindcss @tailwindcss/forms` en la raíz del proyecto web
2. `tailwind.config.js` con el glob `./Pages/**/*.cshtml`
3. Script `npm run build:css` que genera `wwwroot/css/app.css`
4. En desarrollo: `npm run watch:css` en paralelo con `dotnet watch`

### Supabase — Conexión EF Core

Npgsql conecta a Supabase vía:
- SSL requerido (`SSL Mode=Require`)
- Host: `db.[project-ref].supabase.co` (IPv4/IPv6 según plan)
- Para Connection Pooling en producción: usar puerto `6543` (PgBouncer) en vez de `5432`
- En desarrollo y migraciones: usar `5432` (directa)
