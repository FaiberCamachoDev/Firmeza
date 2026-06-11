# Tailwind CSS — Cómo funciona en este proyecto

## ¿Por qué Tailwind necesita un paso de build?

A diferencia de Bootstrap (un archivo CSS ya listo), **Tailwind genera solo el CSS que realmente usás**. Escanea todos los `.cshtml`, ve qué clases usás (`flex`, `bg-gray-900`, `px-4`, etc.) y genera un `app.css` con solo esas clases.

Si usás 50 clases de las 10,000 que tiene Tailwind, tu `app.css` pesa 5KB en lugar de 3MB.

---

## El pipeline de build

```
tailwind.css (input)              tailwind.config.js
    │                                     │
    │  @tailwind base;                    │  content: ["./Pages/**/*.cshtml"]
    │  @tailwind components;              │  ← escanea estos archivos
    │  @tailwind utilities;               │    buscando clases Tailwind
    │                                     │
    └──────────── npm run build:css ───────┘
                        │
                        ↓
                    app.css (output)
                    ← solo las clases usadas
                        │
                        ↓
              <link href="~/css/app.css" />
              ← el browser lo carga
```

---

## El bug que causó el CSS roto — y por qué pasó

`_AdminLayout.cshtml` era solo un `<div>`:

```html
<!-- ANTES (roto): -->
<div class="flex h-screen">
    ...
    @RenderBody()
</div>
```

Sin `<!DOCTYPE html>`, `<head>`, ni `<link rel="stylesheet">`. Era solo fragmento HTML, no un documento completo.

Cuando las páginas admin seteaban `Layout = "_AdminLayout.cshtml"`, usaban ese fragmento como su contenedor, pero el `<link>` a `app.css` nunca existía en la respuesta.

**El fix:**

```html
<!-- DESPUÉS (correcto): -->
<!DOCTYPE html>
<html lang="es">
<head>
    <link rel="stylesheet" href="~/css/app.css" asp-append-version="true" />
</head>
<body>
<div class="flex h-screen">
    ...
    @RenderBody()
</div>
@await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

Un layout Razor **debe ser un documento HTML completo** si no tiene un layout padre.

**Reto:** quita el `<link>` del `_AdminLayout.cshtml`. Recarga el dashboard. ¿Se ve exactamente igual que antes del fix? Eso confirma que el CSS ahora viene de `_AdminLayout` y no de ningún otro lugar.

---

## `asp-append-version="true"` — cache busting

```html
<link rel="stylesheet" href="~/css/app.css" asp-append-version="true" />
```

Genera: `<link href="/css/app.css?v=abc123def456" />`

El `?v=` es un hash del contenido del archivo. Cuando cambias `app.css` (por ejemplo, agregas clases), el hash cambia, y el browser descarga la versión nueva en lugar de usar la caché.

Sin esto: cambias estilos, el browser sigue mostrando el CSS viejo porque lo tenía en caché.

---

## Los componentes custom en `tailwind.css`

```css
@layer components {
  .btn-primary {
    @apply bg-indigo-600 hover:bg-indigo-700 text-white font-medium py-2 px-4 rounded-lg;
  }
  .card {
    @apply bg-white rounded-xl shadow-sm border border-gray-100 p-6;
  }
  .form-input {
    @apply w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-indigo-500;
  }
}
```

`@apply` extrae clases utilitarias de Tailwind en un nombre reutilizable. En lugar de escribir `bg-indigo-600 hover:bg-indigo-700 text-white...` en cada botón del proyecto, usás `.btn-primary`.

**Cuándo usar `@apply` vs clases inline:**
- `@apply`: cuando el mismo grupo de clases aparece 5+ veces (botones, cards, inputs)
- Clases inline: cuando el estilo es único de esa página

---

## Desarrollo con watch (dos terminales)

```bash
# Terminal 1: app .NET con hot reload
dotnet watch run

# Terminal 2: Tailwind observando cambios en .cshtml
npm run watch:css
```

Cuando agregas una clase nueva en un `.cshtml` y guardás, Tailwind la detecta y regenera `app.css`. El browser recarga automáticamente (con `dotnet watch`).

Sin el `watch:css`, podés usar clases en el HTML que no aparecen en `app.css` — el estilo no se aplica, aunque estés seguro de haberlo escrito bien.

---

## `tailwind.config.js` — el escáner de clases

```js
module.exports = {
  content: [
    "./Pages/**/*.cshtml",   // escanea todos los archivos Razor
    "./wwwroot/**/*.js"      // y cualquier JS en wwwroot
  ],
  plugins: [require('@tailwindcss/forms')]
}
```

**`@tailwindcss/forms`:** resetea los estilos por defecto de `<input>`, `<select>`, `<textarea>`. Sin él, los inputs se ven distintos en cada browser. Con él, todos parten de la misma base y los estilos Tailwind los controlan completamente.

**Reto:** agrega `./Data/**/*.cs` al `content`. ¿Qué pasaría si un archivo `.cs` contiene el string `"bg-red-500"` como mensaje de error? ¿Tailwind lo incluiría en el CSS? ¿Es un problema o no?
