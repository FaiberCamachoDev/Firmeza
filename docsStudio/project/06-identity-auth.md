# ASP.NET Core Identity — Autenticación y Roles

## ¿Qué es Identity en una línea?

**Identity es el sistema que sabe quién eres y qué puedes hacer.** Maneja registro, login, contraseñas (hasheadas), roles y sesiones — sin que tú implementes nada de eso desde cero.

---

## Reto antes de leer

Abre `Program.cs` y mira:

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(...)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
```

**Pregunta:** ¿Qué son los `<ApplicationUser, IdentityRole>`? ¿Por qué van ahí en lugar de los tipos por defecto?

No busques — intenta razonarlo viendo `ApplicationUser.cs`. Sigue leyendo cuando tengas una hipótesis.

---

## Las dos piezas de Identity: quién eres + qué rol tienes

```
ApplicationUser   ←── extiende IdentityUser
     │
     │  tiene roles
     ↓
IdentityRole      ←── clase estándar de Identity
```

**`IdentityUser`** es la clase base de Identity. Tiene: `Id`, `UserName`, `Email`, `PasswordHash`, `PhoneNumber`, etc.

**`ApplicationUser`** la extiende con campos propios del negocio:

```csharp
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DocumentNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

Esto genera una sola tabla `AspNetUsers` que combina los campos de `IdentityUser` + los nuestros.

**¿Por qué no crear una tabla `Persons` separada?** Podrías, pero significa unir dos tablas en cada consulta de usuario. Extender `IdentityUser` mantiene todo en un lugar.

---

## El flujo de login — paso a paso

```
Usuario envía email + password
         ↓
Login.cshtml.cs → _signInManager.PasswordSignInAsync(email, password, ...)
         ↓
Identity busca el usuario por email en AspNetUsers
Identity hashea el password enviado y compara con el hash guardado
         ↓
Si coincide: ¿qué rol tiene?
         ↓
Si rol = "Admin" → redirige a /Admin/Dashboard
Si rol = "Cliente" → SignOut() + error "solo administradores"
Si no existe o password mal → "Correo o contraseña incorrectos"
```

**Nunca se guarda la contraseña en texto plano.** Identity usa PBKDF2 con salt aleatorio. Lo que está en la DB es irreversible — si olvidás la contraseña, se resetea, no se recupera.

---

## `SignInManager` vs `UserManager` — cuándo usar cada uno

```csharp
private readonly SignInManager<ApplicationUser> _signInManager;
private readonly UserManager<ApplicationUser> _userManager;
```

| | `UserManager` | `SignInManager` |
|---|---|---|
| **Qué hace** | CRUD de usuarios | Maneja sesiones |
| **Ejemplos** | `CreateAsync`, `FindByEmailAsync`, `AddToRoleAsync`, `IsInRoleAsync` | `PasswordSignInAsync`, `SignOutAsync` |
| **Cuándo usarlo** | Cuando operás sobre datos del usuario | Cuando iniciás/cerrás sesión |

**En `Login.cshtml.cs`:**
```csharp
// SignInManager valida password y crea la cookie de sesión
var result = await _signInManager.PasswordSignInAsync(email, password, ...);

// UserManager consulta el rol del usuario
var user = await _userManager.FindByEmailAsync(email);
var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
```

---

## El sistema de cookies — cómo persiste la sesión

```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";       // redirige aquí si no autenticado
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});
```

Cuando `PasswordSignInAsync` tiene éxito, Identity emite una **cookie cifrada** al browser. Esa cookie identifica al usuario en cada request siguiente.

**Analogía física:** la pulsera de un evento. La pulsera es la cookie: te la dan al entrar (login), la mostrás en cada acceso (cada request HTTP), y cuando expira o la quitás (logout), perdés el acceso.

El servidor nunca guarda la sesión en memoria — solo verifica la cookie en cada request.

**Reto:** cambia `ExpireTimeSpan` a `TimeSpan.FromSeconds(30)`. Inicia sesión, espera 30 segundos, recarga una página del admin. ¿Qué pasa?

---

## Proteger páginas con `[Authorize]` y políticas

### Opción 1: Atributo en la página
```csharp
[Authorize(Roles = "Admin")]
public class DashboardModel : PageModel { ... }
```

### Opción 2: Proteger toda una carpeta (lo que usamos)
```csharp
// En Program.cs
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "RequireAdmin");
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
});
```

Con esto, TODA página dentro de `Pages/Admin/` requiere rol `Admin` automáticamente. No hay que agregar `[Authorize]` a cada página — es una protección en bloque.

**¿Qué pasa si un Cliente intenta entrar a `/Admin/Dashboard`?**
1. El middleware de autorización detecta que no tiene rol "Admin"
2. Redirige a `/Auth/AccessDenied`
3. El `Login.cshtml.cs` también lo bloquea manualmente si intenta hacer login

Doble protección: a nivel de routing (middleware) y a nivel de lógica (código).

---

## `DbSeeder.cs` — por qué existe y cómo funciona

```csharp
public static async Task SeedAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // 1. Crear roles si no existen
    string[] roles = ["Admin", "Cliente"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // 2. Crear usuario admin si no existe
    if (await userManager.FindByEmailAsync("admin@firmeza.com") is null)
    {
        var admin = new ApplicationUser { ... };
        await userManager.CreateAsync(admin, "Admin@123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}
```

**¿Por qué los `if`?** El seeder corre en CADA arranque de la app (está en startup). Sin las verificaciones, intentaría crear los roles y el admin cada vez — y fallaría con errores de duplicado.

**El patrón "create if not exists"** es la clave: idempotente. Puedes correrlo 100 veces y el resultado es el mismo que correrlo una vez.

**Reto:** borra el usuario admin desde Supabase Dashboard (tabla `AspNetUsers`). Reinicia la app. ¿Se recreó automáticamente? ¿Con qué contraseña?

---

## Resumen: archivos que tocan Identity

| Archivo | Qué hace con Identity |
|---|---|
| `Program.cs` | Registra y configura todos los servicios de Identity |
| `ApplicationUser.cs` | Define qué campos extra tiene el usuario |
| `ApplicationDbContext.cs` | Hereda de `IdentityDbContext` para incluir las tablas |
| `DbSeeder.cs` | Crea roles y usuario admin en el primer arranque |
| `Pages/Auth/Login.cshtml.cs` | Usa `SignInManager` + `UserManager` para autenticar |
| `Pages/Auth/Logout.cshtml.cs` | Usa `SignInManager.SignOutAsync()` |
| `Pages/Auth/AccessDenied.cshtml` | Página que ve el Cliente si intenta entrar al admin |
| `appsettings.json` → `Program.cs` | Reglas de contraseña (largo, mayúsculas, símbolos) |
