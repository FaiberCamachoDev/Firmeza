# Guía práctica — Docker completo de Firmeza (Week 5)

---

## Qué tenemos y dónde está

```
Module6-w1/
├── Dockerfile.tests              ← corre los 62 tests xUnit; si falla, nada más arranca
├── docker-compose.yml            ← orquesta todo el stack
└── Firmeza/
    ├── Firmeza.Api/Dockerfile    ← API REST  → :5109
    ├── Firmeza.Web/Dockerfile    ← Admin Razor Pages → :5006
    └── Firmeza.Client/Dockerfile ← React SPA via nginx → :5173
```

---

## Levantar todo desde cero

```bash
cd Module6-w1
docker compose up --build
```

Orden de arranque que aplica Docker automáticamente:

```
tests ──► db ──► api
          │
          └──► admin
tests ──────────────► client
```

Si los tests fallan → `exit code 1` → Compose aborta. Nada más se levanta.

---

## Solo correr los tests (sin levantar servicios)

```bash
docker compose up --build tests
echo "Exit code: $?"   # 0 = todos pasan
```

---

## Variables de entorno — archivo .env

Crear `.env` en la raíz del proyecto copiando `.env.example`:

```bash
cp .env.example .env
```

Variables que hay que completar para email y seguridad:

```env
POSTGRES_PASSWORD=TuPasswordDB123!
JWT_KEY=clave-secreta-minimo-32-caracteres-aqui
EMAIL_USERNAME=cuenta@gmail.com
EMAIL_PASSWORD=xxxx xxxx xxxx xxxx
EMAIL_FROM=cuenta@gmail.com
```

Sin `.env`, Compose usa los defaults del `docker-compose.yml` (suficiente para desarrollo local, no para producción real).

---

## Puertos del stack

| URL | Servicio |
|---|---|
| http://localhost:5109 | API REST (Swagger en `/`) |
| http://localhost:5006 | Admin panel (Razor Pages) |
| http://localhost:5173 | Portal cliente (React SPA) |
| localhost:5433 | PostgreSQL (acceso directo) |

---

## Comandos del día a día

```bash
# Ver estado de contenedores
docker ps

# Ver logs de un servicio
docker compose logs api
docker compose logs tests
docker compose logs client

# Detener sin borrar datos
docker compose down

# Detener y borrar la base de datos también
docker compose down -v

# Reconstruir solo un servicio
docker compose up --build api

# Acceder a la base de datos directamente
docker exec -it firmeza_db psql -U firmeza_user -d firmeza_db
```

---

## Cómo funciona cada Dockerfile

### Dockerfile.tests — gate de CI

```
dotnet restore → dotnet test → exit 0/1
```

Un contenedor one-shot: arranca, corre los 62 tests, termina. El exit code es el que decide si el resto del stack puede arrancar.

### Firmeza.Api/Dockerfile — dos etapas

```
[sdk:9.0]  restore + publish  →  [aspnet:9.0]  runtime mínimo
```

Nota: usa `-p:ErrorOnDuplicatePublishOutputFiles=false` porque `Firmeza.Api.csproj` referencia `Firmeza.Web.csproj` y ambos generan `appsettings.json` con el mismo nombre.

### Firmeza.Web/Dockerfile — tres etapas

```
[node:20-alpine]  npm run build:css  →  [sdk:9.0]  publish  →  [aspnet:9.0]  runtime
```

El CSS de Tailwind se compila en una etapa separada de Node para no incluir Node.js en la imagen final.

### Firmeza.Client/Dockerfile — dos etapas

```
[node:24-alpine]  npm install + vite build  →  [nginx:1.27-alpine]  sirve /dist
```

Dos detalles importantes:
- Usa `npm install` (no `npm ci`) porque `@tailwindcss/vite` tira de `lightningcss` con bindings nativos `@napi-rs`, cuyas versiones difieren entre Linux glibc y Alpine musl.
- `VITE_API_URL` es un `ARG` que se inyecta en tiempo de build; Vite lo embebe en el bundle JS.

---

## React Router en nginx

El archivo `Firmeza.Client/nginx.conf` tiene esta línea crítica:

```nginx
location / {
    try_files $uri $uri/ /index.html;
}
```

Sin ella, al navegar directamente a `http://localhost:5173/catalog` nginx devolvería 404. Con ella, sirve `index.html` y React Router toma el control.

---

## Qué pasa cuando corro docker compose up --build

1. Docker construye las 4 imágenes en paralelo.
2. Arranca `firmeza_tests` → corre `dotnet test` → si pasa, exit 0.
3. `firmeza_db` espera que `tests` haya terminado con exit 0, luego arranca y espera a que `pg_isready` responda (healthcheck).
4. `firmeza_api` y `firmeza_admin` esperan a que `tests` pase Y a que `db` esté healthy.
5. `firmeza_client` espera a que `tests` pase Y a que `api` haya iniciado.
6. La aplicación completa está disponible.

---

## Diagnóstico rápido si algo falla

| Síntoma | Qué revisar |
|---|---|
| `tests` falla | `docker compose logs tests` — ver qué test falló |
| `api` no arranca | `docker compose logs api` — migration error o connection string |
| `client` muestra pantalla en blanco | Revisar `VITE_API_URL` en docker-compose.yml — debe apuntar a donde responde la API |
| Puerto ocupado (`address already in use`) | `lsof -ti:5109 \| xargs kill -9` (o el puerto que falle) |
| `npm ci` falla en Docker | Usar `npm install` en lugar de `npm ci` — conflicto de plataforma @napi-rs |
