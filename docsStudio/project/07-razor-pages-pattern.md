# Razor Pages — Cómo funciona el patrón Page + PageModel

## El concepto core

Cada página Razor son **dos archivos que trabajan juntos:**

```
Products/Create.cshtml      ← el HTML (lo que el usuario ve)
Products/Create.cshtml.cs   ← la lógica (lo que el servidor ejecuta)
```

El `.cshtml.cs` es el **PageModel**: contiene los métodos `OnGet()` y `OnPost()` que se ejecutan cuando el usuario carga la página o envía un formulario.

---

## Reto: rastrea una petición HTTP completa

Abre `Products/Create.cshtml.cs` y responde antes de leer:

**Cuando el usuario hace clic en "Guardar producto", ¿qué función se ejecuta primero en el servidor?**

Mira el `<form method="post">` en `Create.cshtml` y luego busca en el PageModel. No hay `action=""` en el form — ¿a dónde va?

---

## El ciclo request → response en Razor Pages

```
Usuario abre /Admin/Products/Create
         ↓
ASP.NET Core enruta a CreateModel.OnGet()
         ↓
OnGet() no hace nada (solo muestra el form vacío)
         ↓
Razor renderiza Create.cshtml con el PageModel como contexto
         ↓
Browser muestra el formulario
         ↓
Usuario completa campos y hace clic en "Guardar"
         ↓
Browser envía POST a /Admin/Products/Create (misma URL)
         ↓
ASP.NET Core ejecuta CreateModel.OnPostAsync()
         ↓
[BindProperty] mapea los campos del form a Input
         ↓
ModelState.IsValid valida anotaciones [Required], [Range], etc.
         ↓
Si válido: guarda en DB y redirige a Index
Si inválido: return Page() → muestra el form con errores
```

**La URL no cambia entre GET y POST.** La diferencia es el verbo HTTP. Razor Pages usa convención: `OnGet` para GET, `OnPost` para POST.

---

## `[BindProperty]` — cómo llegan los datos del form al servidor

```csharp
[BindProperty]
public ProductCreateViewModel Input { get; set; } = new();
```

Sin `[BindProperty]`, `Input` siempre estaría vacío en el `OnPost`. El atributo le dice a ASP.NET Core: "cuando llegue un POST, mapea los campos del formulario a esta propiedad".

**¿Cómo sabe qué campo del form corresponde a qué propiedad?** Por nombre. El input HTML `name="Input.Name"` mapea a `Input.Name` en el PageModel.

**Rómpiéndolo:** quita `[BindProperty]` de `Input` en `CreateModel`. Agrega un producto. ¿Qué pasa? ¿Llega a guardarse?

---

## `ModelState.IsValid` — validación en el servidor

```csharp
if (!ModelState.IsValid)
    return Page();
```

`ModelState` acumula todos los errores de validación de las Data Annotations (`[Required]`, `[Range]`, etc.). Si hay algún error, `IsValid` es `false`.

`return Page()` re-renderiza el formulario CON los errores visibles. Las etiquetas `<span asp-validation-for="Input.Name">` en el `.cshtml` muestran el mensaje de error de cada campo.

**Importante:** la validación del lado del cliente (jQuery Validation) previene el submit si hay errores obvios. Pero `ModelState.IsValid` en el servidor es la red de seguridad — siempre debe estar, porque cualquiera puede deshabilitar JS o enviar requests directos.

---

## `asp-page`, `asp-route-id` — Tag Helpers

```html
<a asp-page="Edit" asp-route-id="@p.Id">Editar</a>
```

Esto genera: `<a href="/Admin/Products/Edit/42">Editar</a>`

Los Tag Helpers son atributos especiales de Razor que generan HTML correcto. Ventajas sobre escribir las URLs a mano:
- Si cambias el nombre de la página, el link se actualiza automáticamente
- Si el `id` cambia, se refleja sin tocar el HTML
- Typos en nombres de página dan error en compilación, no en runtime

**`asp-page`** = nombre de la página Razor (relativo a la carpeta actual)
**`asp-route-id`** = parámetro de ruta (lo que va en la URL: `/Edit/{id}`)

---

## `RedirectToPage` vs `return Page()`

```csharp
// Éxito: redirige a otra página (nuevo GET)
return RedirectToPage("Index");

// Error: re-renderiza la página actual (muestra los errores)
return Page();
```

**¿Por qué redirigir en éxito?** Patrón POST-Redirect-GET. Si el usuario recarga la página después de un POST exitoso, sin el redirect el browser preguntaría "¿reenviar el formulario?". Con el redirect, recargar es un GET inocente a Index.

---

## `_ViewImports.cshtml` — por qué los namespaces están disponibles en todos los .cshtml

```csharp
@using Firmeza.Web.Models
@using Firmeza.Web.ViewModels.Products
@namespace Firmeza.Web.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

Sin este archivo, cada `.cshtml` necesitaría sus propios `@using`. `_ViewImports` aplica a todas las páginas en su carpeta y subcarpetas.

`@addTagHelper` habilita todos los Tag Helpers (`asp-page`, `asp-for`, `asp-validation-for`, etc.). Sin esa línea, los atributos `asp-*` no funcionan.

---

## `_ViewStart.cshtml` — el layout por defecto

```csharp
@{
    Layout = "_Layout";
}
```

Este archivo se ejecuta antes de cada página. Establece `_Layout` como el layout por defecto para todas las páginas.

Las páginas del admin lo sobreescriben:
```csharp
@{
    Layout = "~/Pages/Shared/_AdminLayout.cshtml";
}
```

Esa asignación pisa el `_ViewStart`. La página admin usa su propio layout completo con sidebar.

---

## Resumen: qué archivo hace qué en una petición típica

Petición: `GET /Admin/Products/Create`

```
1. ASP.NET Core routing → Pages/Admin/Products/Create.cshtml.cs
2. CreateModel.OnGet() ejecuta → return Page()
3. Razor engine fusiona Create.cshtml con _AdminLayout.cshtml
4. _AdminLayout.cshtml incluye el <link> a app.css
5. _ViewImports.cshtml aplica los @using y Tag Helpers
6. HTML final se envía al browser
```

Petición: `POST /Admin/Products/Create`

```
1. Routing → CreateModel.OnPostAsync()
2. [BindProperty] llena Input con los datos del form
3. int.TryParse(Input.StockInput) valida el stock
4. ModelState.IsValid verifica las Data Annotations
5. Si OK: new Product {...}, _db.SaveChangesAsync(), redirect
6. Si error: return Page() con ModelState errors visibles
```
