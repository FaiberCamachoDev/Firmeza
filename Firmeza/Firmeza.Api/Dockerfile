# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto para cache de restauración
COPY Firmeza.Web/Firmeza.Web.csproj  Firmeza.Web/
COPY Firmeza.Api/Firmeza.Api.csproj  Firmeza.Api/

RUN dotnet restore Firmeza.Api/Firmeza.Api.csproj

# Copiar código fuente
COPY Firmeza.Web/ Firmeza.Web/
COPY Firmeza.Api/ Firmeza.Api/

RUN dotnet publish Firmeza.Api/Firmeza.Api.csproj \
    -c Release \
    -o /app/publish \
    -p:ErrorOnDuplicatePublishOutputFiles=false

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

# L5: ejecutar como usuario no-root
RUN adduser --disabled-password --no-create-home --gecos "" appuser
USER appuser

# Railway inyecta $PORT en tiempo de ejecución; fallback 8080 para local/Docker
ENTRYPOINT ["sh", "-c", "exec dotnet Firmeza.Api.dll --urls \"http://+:${PORT:-8080}\""]
