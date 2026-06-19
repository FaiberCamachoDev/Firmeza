# ── Stage 1: Build React SPA ──────────────────────────────────────────────────
FROM node:24-alpine AS build
WORKDIR /app

# VITE_API_URL se inyecta en tiempo de build para que Vite lo embeba en el bundle
ARG VITE_API_URL=http://localhost:5109
ENV VITE_API_URL=$VITE_API_URL

COPY Firmeza.Client/package*.json ./
RUN npm install

COPY Firmeza.Client/ .

RUN npm run build

# ── Stage 2: Servir con nginx ─────────────────────────────────────────────────
FROM nginx:1.27-alpine AS runtime

# Railway inyecta $PORT; fallback 80 para local/Docker
ENV PORT=80

COPY --from=build /app/dist /usr/share/nginx/html
# nginx:1.19+ procesa automáticamente los *.template con envsubst al arrancar
COPY Firmeza.Client/nginx.conf /etc/nginx/templates/default.conf.template

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
