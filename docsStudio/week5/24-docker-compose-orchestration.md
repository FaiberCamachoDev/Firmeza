# Docker Compose — Orquestación completa (Week 5)

## Resumen de servicios

```yaml
services:
  tests:   # Gate: xUnit, one-shot, exit 0 = todos pasan
  db:      # PostgreSQL 16 con healthcheck
  api:     # ASP.NET Core API JWT  → :5109
  admin:   # ASP.NET Core Razor    → :5006
  client:  # React SPA via nginx   → :5173
```

---

## Variables de entorno requeridas

Para producción real, crear un archivo `.env` en la raíz del proyecto:

```env
POSTGRES_PASSWORD=TuPasswordSeguro123!
JWT_KEY=clave-secreta-minimo-32-caracteres-aqui
EMAIL_USERNAME=cuenta@gmail.com
EMAIL_PASSWORD=xxxx xxxx xxxx xxxx   # App Password de Google
EMAIL_FROM=cuenta@gmail.com
```

En desarrollo local, los defaults del `docker-compose.yml` son suficientes excepto para las credenciales de email.

---

## Connection string hacia el contenedor de Postgres

```
Host=db;Port=5432;Database=firmeza_db;Username=firmeza_user;Password=...
```

`Host=db` — el hostname es el nombre del servicio en la red interna de Docker Compose. Los contenedores se resuelven por nombre de servicio, no por IP.

---

## Healthcheck de PostgreSQL

```yaml
healthcheck:
  test: ["CMD-SHELL", "pg_isready -U firmeza_user -d firmeza_db"]
  interval: 10s
  timeout: 5s
  retries: 5
```

`pg_isready` verifica que Postgres acepta conexiones. La API y Admin no arrancan hasta que este check pase — evita errores de migración al conectarse antes de que la DB esté lista.

---

## Build args para el cliente React

```yaml
client:
  build:
    context: ./Firmeza
    dockerfile: Firmeza.Client/Dockerfile
    args:
      VITE_API_URL: http://localhost:5109
```

`VITE_API_URL` se embebe en el bundle de JavaScript durante el build. En producción real, cambiar a la URL pública de la API (ej. `https://api.firmeza.com`).

---

## Comandos útiles

```bash
# Levantar todo desde cero
docker compose up --build

# Ver estado de contenedores
docker ps

# Logs de un servicio específico
docker compose logs api
docker compose logs tests

# Detener y eliminar contenedores (mantiene volúmenes)
docker compose down

# Detener y eliminar todo incluyendo la base de datos
docker compose down -v
```
