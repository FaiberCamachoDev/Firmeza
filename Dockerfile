# Stage 1: Build Tailwind CSS
FROM node:20-alpine AS css-builder
WORKDIR /app
COPY Firmeza/Firmeza.Web/package.json ./
RUN npm install
COPY Firmeza/Firmeza.Web/tailwind.config.js ./
COPY Firmeza/Firmeza.Web/wwwroot ./wwwroot
COPY Firmeza/Firmeza.Web/Pages ./Pages
RUN npm run build:css

# Stage 2: Build .NET app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY Firmeza/Firmeza.Web/Firmeza.Web.csproj Firmeza.Web/
RUN dotnet restore Firmeza.Web/Firmeza.Web.csproj
COPY Firmeza/Firmeza.Web/ Firmeza.Web/
COPY --from=css-builder /app/wwwroot/css/app.css Firmeza.Web/wwwroot/css/app.css
WORKDIR /src/Firmeza.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Firmeza.Web.dll"]
