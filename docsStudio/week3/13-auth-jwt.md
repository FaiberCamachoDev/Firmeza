# Autenticación JWT y Roles (Week 3 — Task 2 & 3)

## Flujo

```
Cliente → POST /api/auth/login  →  { token, expiresAt, roles }
Cliente → POST /api/auth/register → crea usuario con rol "Cliente" + JWT
```

El token se incluye en peticiones protegidas:
```
Authorization: Bearer <token>
```

## Roles

| Rol | Creado en | Descripción |
|---|---|---|
| `Admin` | `DbSeeder` | Acceso total — gestión de productos, clientes y ventas |
| `Cliente` | `DbSeeder` / `register` | Puede registrar ventas y ver su propio perfil |

`DbSeeder` ya crea ambos roles en la base de datos compartida.

## JwtService

`Services/JwtService.cs` genera el token con claims:
- `sub` = `user.Id`
- `email` = email del usuario
- `jti` = GUID único por token
- `firstName`, `lastName`
- `ClaimTypes.Role` = roles del usuario

Configuración en `appsettings.json`:
```json
"Jwt": {
  "Key": "firmeza-super-secret-key-min-32-chars-2026",
  "Issuer": "Firmeza.Api",
  "Audience": "Firmeza.Clients",
  "ExpireHours": 8
}
```

**En producción:** la `Key` debe venir de variables de entorno o Secret Manager, nunca del repositorio.

## AuthController

### `POST /api/auth/login`
- Busca el usuario por email con `UserManager`
- Verifica contraseña
- Retorna `TokenResponseDto` con token y metadata

### `POST /api/auth/register`
- Verifica que el email no exista
- Crea `ApplicationUser` con `UserManager`
- Asigna rol `Cliente`
- Dispara email de bienvenida en fire-and-forget (`_ = email.SendWelcomeAsync(...)`)
- Retorna `201 Created` con el token

## Políticas de autorización

```csharp
options.AddPolicy("RequireAdmin",   p => p.RequireRole("Admin"));
options.AddPolicy("RequireCliente", p => p.RequireRole("Cliente"));
```

Los controladores usan `[Authorize(Roles = "Admin")]` directamente en los actions.
