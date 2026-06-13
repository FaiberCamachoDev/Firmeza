# Docker — Arquitectura de Contenedores (Week 5)

## Objetivo

Contenerizar todos los servicios de Firmeza en imágenes reproducibles y orquestarlos con `docker-compose.yml`, garantizando que las pruebas automatizadas se ejecuten **antes** de levantar cualquier servicio de producción.

---

## Estructura de Dockerfiles

```
Module6-w1/
├── Dockerfile.tests                  ← Ejecuta xUnit (gate de CI)
└── Firmeza/
    ├── Firmeza.Api/Dockerfile        ← Multi-stage: SDK → aspnet runtime
    ├── Firmeza.Web/Dockerfile        ← Multi-stage: node (CSS) + SDK → aspnet runtime
    └── Firmeza.Client/Dockerfile     ← Multi-stage: node (Vite build) → nginx
```

---

## Dockerfile.tests — Gate de Pruebas

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS test
WORKDIR /src
# Copiar solo .csproj primero → cache de NuGet
COPY Firmeza/Firmeza.slnx .
COPY Firmeza/Firmeza.Web/Firmeza.Web.csproj     Firmeza.Web/
COPY Firmeza/Firmeza.Api/Firmeza.Api.csproj     Firmeza.Api/
COPY Firmeza/Firmeza.Tests/Firmeza.Tests.csproj Firmeza.Tests/
RUN dotnet restore Firmeza.Tests/Firmeza.Tests.csproj
COPY Firmeza/ .
RUN dotnet test Firmeza.Tests/Firmeza.Tests.csproj \
    --no-restore -c Release \
    --logger "console;verbosity=normal"
```

**Comportamiento clave:**
- Si algún test falla → `exit code 1` → Docker Compose aborta toda la cadena.
- Si todos pasan → `exit code 0` → los servicios dependientes continúan.

---

## Firmeza.Api/Dockerfile — API REST

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY Firmeza.Web/Firmeza.Web.csproj  Firmeza.Web/
COPY Firmeza.Api/Firmeza.Api.csproj  Firmeza.Api/
RUN dotnet restore Firmeza.Api/Firmeza.Api.csproj
COPY Firmeza.Web/ Firmeza.Web/
COPY Firmeza.Api/ Firmeza.Api/
RUN dotnet publish Firmeza.Api/Firmeza.Api.csproj \
    -c Release -o /app/publish \
    -p:ErrorOnDuplicatePublishOutputFiles=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Firmeza.Api.dll"]
```

**Nota:** `-p:ErrorOnDuplicatePublishOutputFiles=false` es necesario porque `Firmeza.Api.csproj` tiene una `ProjectReference` a `Firmeza.Web.csproj`; ambos proyectos generan `appsettings.json` con el mismo nombre.

---

## Firmeza.Web/Dockerfile — Panel Admin (Razor Pages)

Tres etapas:

1. **`css-builder`** (`node:20-alpine`) — Instala Tailwind y compila `app.css`.
2. **`build`** (`dotnet/sdk:9.0`) — Restaura, copia el CSS compilado y publica.
3. **`runtime`** (`dotnet/aspnet:9.0`) — Imagen mínima para producción.

El CSS se compila por separado para no incluir Node.js en la imagen final.

---

## Firmeza.Client/Dockerfile — SPA React + nginx

```dockerfile
FROM node:24-alpine AS build
ARG VITE_API_URL=http://localhost:5109
ENV VITE_API_URL=$VITE_API_URL
COPY Firmeza.Client/package*.json ./
RUN npm install          # npm ci falla por paquetes nativos @napi-rs/@emnapi
COPY Firmeza.Client/ .
RUN npm run build

FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY Firmeza.Client/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

**Por qué `npm install` y no `npm ci`:**
`@tailwindcss/vite` incluye `lightningcss` que usa bindings nativos `@napi-rs`. El lock file generado en Linux glibc tiene entradas de plataforma incompatibles con Alpine musl. `npm install` resuelve los paquetes correctos para la plataforma del contenedor.

**VITE_API_URL como ARG:**
Vite embebe la URL en el bundle en tiempo de build. El valor se pasa desde `docker-compose.yml` como build arg, lo que permite cambiar la URL por entorno sin recompilar el código fuente.

---

## nginx.conf para React Router

```nginx
location / {
    try_files $uri $uri/ /index.html;
}
```

Necesario para que React Router funcione: si el usuario navega directamente a `/catalog`, nginx sirve `index.html` y React Router toma control del cliente.
