# EF Core + Migraciones — Cómo funciona la DB

## El concepto en una línea

**EF Core es un traductor:** convierte clases C# ↔ tablas PostgreSQL, y tus consultas LINQ ↔ SQL.

---

## Reto antes de leer

Mira `ApplicationDbContext.cs`:

```csharp
public DbSet<Product> Products { get; set; }
public DbSet<Customer> Customers { get; set; }
```

**Pregunta:** Si cambias `Products` por `Productos` aquí, ¿cómo se llama la tabla en PostgreSQL?

Pruébalo: renombra a `Productos`, crea una migración con `dotnet ef migrations add TestRename`, ábrela en `Migrations/` y mira qué SQL generó. Luego bórrala con `dotnet ef migrations remove`.

---

## Flujo completo: del modelo a la tabla

```
1. Escribes la clase C# (Model)
         ↓
2. Corres: dotnet ef migrations add NombreMigracion
         ↓
3. EF genera un archivo en Migrations/ con el SQL equivalente
         ↓
4. Corres: dotnet ef database update
   (o en el proyecto: db.Database.MigrateAsync() al arrancar)
         ↓
5. EF ejecuta ese SQL en PostgreSQL
         ↓
6. La tabla existe en Supabase
```

**Nunca escribes SQL a mano.** EF lo genera comparando el estado actual del `DbContext` con lo que ya aplicó (rastreado en la tabla `__EFMigrationsHistory`).

---

## `__EFMigrationsHistory` — la memoria de EF

```sql
SELECT * FROM "__EFMigrationsHistory";
-- MigrationId                              | ProductVersion
-- 20260604220551_InitialCreate             | 9.0.5
```

Cada migración aplicada queda registrada aquí. Cuando corres `MigrateAsync()`:
1. EF lee esta tabla
2. Compara con las migraciones que existen en `Migrations/`
3. Aplica solo las que faltan
4. Si ya están todas → "No migrations were applied. Database is already up to date."

**Por eso el `fail` al primera ejecución es normal:** EF intenta leer `__EFMigrationsHistory` pero la tabla no existe todavía. Inmediatamente la crea y continúa.

---

## `ApplicationDbContext` — la puerta de entrada

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Product> Products { get; set; }
    ...

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);  // ← CRÍTICO: configura las tablas de Identity

        builder.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Name);   // ← crea un índice en la columna Name
        });
    }
}
```

**¿Por qué hereda de `IdentityDbContext` y no de `DbContext`?**

`IdentityDbContext` agrega automáticamente las tablas que necesita ASP.NET Identity:
- `AspNetUsers` (usuarios)
- `AspNetRoles` (roles)
- `AspNetUserRoles` (qué rol tiene cada usuario)
- `AspNetUserClaims`, `AspNetRoleClaims`, etc.

Si heredaras de `DbContext`, tendrías que definir todas esas tablas tú mismo.

**Reto:** quita el `base.OnModelCreating(builder)` y genera una migración. ¿Qué le pasa a las tablas de Identity en el SQL generado?

---

## Índices — por qué los agregamos

```csharp
e.HasIndex(p => p.Name);
e.HasIndex(c => c.DocumentNumber).IsUnique();
```

**Sin índice:** buscar "Cemento Portland" en 100,000 productos = PostgreSQL lee TODOS los registros uno por uno (full table scan).

**Con índice:** PostgreSQL mantiene una estructura ordenada separada. Buscar por nombre = saltar directamente al grupo correcto.

`.IsUnique()` en `DocumentNumber` hace dos cosas:
1. Crea el índice para búsquedas rápidas
2. Garantiza a nivel de DB que no existan dos clientes con el mismo documento

**Analogía física:** el índice de un libro. Sin él, buscas "migrations" leyendo cada página. Con él, vas directo a la página correcta.

---

## `MigrateAsync()` en `Program.cs` — por qué está en startup

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();    // aplica migraciones pendientes
    await DbSeeder.SeedAsync(...);       // crea admin + roles si no existen
}
```

**¿Por qué en startup y no en un script separado?**

En desarrollo es conveniente: arrancas la app y la DB se actualiza sola. En Docker es crítico: el contenedor de la app inicia después del contenedor de la DB (health check), y las migraciones se aplican al primer arranque.

**Advertencia:** en producción con equipos grandes, esto puede causar problemas si varias instancias arrancan a la vez y corren migraciones simultáneamente. Para escala mayor se usa un job de migración separado. Para este proyecto, el startup approach es correcto.

---

## El archivo `.Designer.cs` — por qué es obligatorio

Cada migración tiene dos archivos:

```
Migrations/
├── 20260604220551_InitialCreate.cs          ← lógica Up()/Down()
├── 20260604220551_InitialCreate.Designer.cs ← atributos de descubrimiento
├── 20260604230000_SeedData.cs
└── 20260604230000_SeedData.Designer.cs
```

El `.Designer.cs` contiene el atributo `[Migration("...")]` que EF Core usa para **descubrir** la migración al escanear el assembly:

```csharp
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260604230000_SeedData")]   // ← sin esto, EF no la ve
partial class SeedData
{
    protected override void BuildTargetModel(...) { ... }
}
```

**Sin el `.Designer.cs`, la migración no aparece en `dotnet ef migrations list` y nunca se aplica.** EF no lanza error — simplemente la ignora en silencio.

Esto causó que `SeedData` nunca se aplicara a Supabase, dejando `Products` y `Customers` vacíos aunque el archivo `SeedData.cs` existía en el repo.

**Regla:** cada vez que creas una migración con `dotnet ef migrations add`, se generan automáticamente ambos archivos. Si alguna vez creás el archivo `.cs` a mano (como ocurrió aquí con `SeedData`), el `.Designer.cs` debe crearse también manualmente.

---

## Rollback sin DROP DATABASE — `database update 0`

En Supabase no podés correr `dotnet ef database drop` porque el pooler está conectado a la DB `postgres` — PostgreSQL no permite borrar la DB a la que estás conectado:

```
ERROR: 55006: cannot drop the currently open database
```

La alternativa: revertir todas las migraciones a cero, que ejecuta los métodos `Down()` en orden inverso (borra tablas, no la DB):

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update 0
ASPNETCORE_ENVIRONMENT=Development dotnet ef database update
```

`database update 0` es el equivalente a "deshacer todo" — deja la DB vacía con solo `__EFMigrationsHistory` (que también se limpia). Luego `database update` re-aplica todo desde `InitialCreate`.
