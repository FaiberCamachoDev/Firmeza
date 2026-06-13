# Autenticación JWT y Roles (Week 3)

## Archivos involucrados

```
Firmeza.Api/
├── Controllers/
│   └── AuthController.cs       ← endpoints de login y registro
├── Services/
│   ├── JwtService.cs           ← generación del token con claims
│   └── IEmailService.cs        ← interfaz usada para enviar bienvenida en registro
├── DTOs/Auth/
│   ├── LoginDto.cs             ← { email, password }
│   ├── RegisterDto.cs          ← { email, password, firstName, lastName, documentNumber, phone }
│   └── TokenResponseDto.cs     ← { token, expiresAt, email, fullName, roles }
└── appsettings.json            ← sección "Jwt": Key, Issuer, Audience, ExpireHours
```

---

## Flujo

```
Cliente → POST /api/auth/login    →  { token, expiresAt, roles }
Cliente → POST /api/auth/register → crea usuario con rol "Cliente" + JWT
```

El token se incluye en peticiones protegidas:
```
Authorization: Bearer <token>
```

---

## Roles

| Rol | Creado en | Descripción |
|---|---|---|
| `Admin` | `DbSeeder` | Acceso total — gestión de productos, clientes y ventas |
| `Cliente` | `DbSeeder` / endpoint `register` | Puede registrar ventas y ver su propio perfil |

`DbSeeder` ya crea ambos roles en la base de datos compartida al arrancar la aplicación.

---

## JwtService

**Archivo:** `Firmeza.Api/Services/JwtService.cs`

Genera el token JWT con los siguientes claims:

| Claim | Valor |
|---|---|
| `sub` | `user.Id` |
| `email` | email del usuario |
| `jti` | GUID único por token (evita reutilización) |
| `firstName` | nombre del usuario |
| `lastName` | apellido del usuario |
| `ClaimTypes.Role` | uno por cada rol asignado |

Configuración en `Firmeza.Api/appsettings.json`:
```json
"Jwt": {
  "Key": "firmeza-super-secret-key-min-32-chars-2026",
  "Issuer": "Firmeza.Api",
  "Audience": "Firmeza.Clients",
  "ExpireHours": 8
}
```

**En producción:** la `Key` debe venir de variables de entorno o Secret Manager, nunca del repositorio.

---

## AuthController

**Archivo:** `Firmeza.Api/Controllers/AuthController.cs`

### `POST /api/auth/login`
1. Busca el usuario por email con `UserManager`
2. Verifica la contraseña con `CheckPasswordAsync`
3. Obtiene los roles del usuario
4. Retorna `TokenResponseDto` con el token y metadata

### `POST /api/auth/register`
1. Verifica que el email no esté en uso
2. Crea `ApplicationUser` con `UserManager.CreateAsync`
3. Asigna rol `Cliente` con `AddToRoleAsync`
4. Dispara email de bienvenida en fire-and-forget (`_ = email.SendWelcomeAsync(...)`)
5. Retorna `201 Created` con el token generado

---

## Políticas de autorización

Configuradas en `Firmeza.Api/Program.cs`:

```csharp
options.AddPolicy("RequireAdmin",   p => p.RequireRole("Admin"));
options.AddPolicy("RequireCliente", p => p.RequireRole("Cliente"));
```

Los controladores usan `[Authorize(Roles = "Admin")]` directamente en los actions. Ver la referencia completa de endpoints en `14-endpoints-reference.md`.

---

## Cómo funciona la validación del token en cada request

```
Request con header "Authorization: Bearer <token>"
         ↓
Middleware JwtBearer (configurado en Program.cs)
  1. Extrae el token del header
  2. Verifica firma con la Key configurada
  3. Verifica Issuer y Audience
  4. Verifica que no haya expirado
         ↓
Si válido: popula HttpContext.User con los claims del token
Si inválido: retorna 401 Unauthorized automáticamente
         ↓
[Authorize(Roles = "Admin")] verifica que el claim de rol sea "Admin"
Si no tiene el rol: retorna 403 Forbidden
```
