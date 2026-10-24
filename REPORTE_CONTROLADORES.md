# Reporte: Controladores y capa de servicios

Proyecto **backend_II** — API REST en ASP.NET Core (.NET 9) que consulta productos en SQL Server mediante el procedimiento almacenado `sp_GetAllActiveProducts`.

---

## 1. Arquitectura por capas

```
Cliente HTTP (Postman, navegador, front-end)
        │  GET /api/products/search?sku=NET-003
        ▼
┌──────────────────────────┐
│ ProductsController       │  Controllers/ProductsController.cs
│ (rutas y parámetros)     │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ ProductsServices         │  Services/ProductsServices.cs
│ (validaciones y lógica)  │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ ProductRepository        │  Database/ProductRepository.cs
│ (acceso a datos)         │
└────────────┬─────────────┘
             ▼
   SQL Server → sp_GetAllActiveProducts
```

Cada capa tiene **una sola responsabilidad**:

| Capa | Archivo | Responsabilidad | Lo que NO hace |
|---|---|---|---|
| Controlador | `Controllers/ProductsController.cs` | Define las rutas HTTP, lee los parámetros (ruta, query, body) y delega al servicio. Más adelante: validar tokens y *claims*. | No valida datos de negocio ni habla con la base de datos. |
| Servicio | `Services/ProductsServices.cs` | Valida parámetros, llama al repositorio, maneja excepciones y arma la respuesta `GeneralResponse<T>`. | No conoce HTTP (rutas, verbos) ni SQL. |
| Repositorio | `Database/ProductRepository.cs` | Abre la conexión, ejecuta el procedimiento almacenado y convierte cada fila en un objeto `Product`. | No decide mensajes para el usuario. |

**Ventaja:** si mañana la lógica se necesita desde otro lugar (otro controlador, una tarea programada, el `Program.cs`), se llama al servicio sin duplicar código. Así lo hace la demo del `Program.cs`.

---

## 2. Cómo funciona un controlador

```csharp
[ApiController]                    // Activa comportamientos de API (binding automático, validación de modelo)
[Route("api/[controller]")]        // [controller] = nombre de la clase sin "Controller" → api/products
public class ProductsController : ControllerBase
{
    private readonly ProductsServices _services;

    public ProductsController(ProductsServices services)   // Inyección por constructor
    {
        _services = services;
    }

    [HttpGet("search")]                                     // Verbo + plantilla de ruta
    public async Task<GeneralResponse<Product>> GetByQuery([FromQuery] string sku)
    {
        return await _services.GetByQuery(sku);             // Solo delega
    }
}
```

Elementos clave:

- **`[ApiController]`**: indica que la clase es una API (no vistas HTML). Infiere de dónde vienen los parámetros y devuelve 400 automático si el modelo es inválido.
- **`[Route("api/[controller]")]`**: prefijo común de todas las rutas de la clase.
- **`[HttpGet]`, `[HttpPost]`**: el verbo HTTP y, opcionalmente, una plantilla de ruta (`"search"`, `"{sku}"`, `"by-name"`).
- **Origen de los parámetros**:
  - `[FromRoute]` → viene en la ruta: `/api/products/NET-003`
  - `[FromQuery]` → viene en la query string: `/api/products/search?sku=NET-003`
  - `[FromBody]` → viene en el cuerpo JSON de la petición: `{ "sku": "NET-003" }`
- **`async Task<...>`**: el método es asíncrono; mientras espera a la base de datos el hilo queda libre para atender otras peticiones.
- **Valor de retorno `GeneralResponse<T>`**: ASP.NET lo serializa a JSON automáticamente.

---

## 3. Endpoints disponibles

URL base en desarrollo: `http://localhost:5161` (o `https://localhost:7238` con el perfil `https`).

| Método del controlador | Verbo y ruta | Origen del dato | Método del servicio |
|---|---|---|---|
| `GetAll` | `GET /api/products` | — | `GetAll()` |
| `GetByRoute` | `GET /api/products/{sku}` | Ruta | `GetByRoute(sku)` |
| `GetByQuery` | `GET /api/products/search?sku=XYZ` | Query | `GetByQuery(sku)` |
| `GetByBody` | `POST /api/products/search` | Body `{ "sku": "XYZ" }` | `GetByBody(request)` |
| `GetByNameFromQuery` | `GET /api/products/by-name?name=laptop` | Query | `GetByName(name)` |
| `GetByNameFromBody` | `POST /api/products/by-name` | Body `{ "name": "laptop" }` | `GetByName(request?.Name)` |

> **Nota sobre rutas:** `GET /api/products/search` no choca con `GET /api/products/{sku}` porque ASP.NET da prioridad a los segmentos literales (`search`, `by-name`) sobre los parámetros (`{sku}`).

### Ejemplos

```http
GET http://localhost:5161/api/products/search?sku=NET-003
```

```http
POST http://localhost:5161/api/products/by-name
Content-Type: application/json

{ "name": "laptop" }
```

---

## 4. La capa de servicios

Cada método del servicio sigue el mismo patrón:

```csharp
public async Task<GeneralResponse<Product>> GetByQuery(string sku)
{
    // 1. Validación: se corta antes de abrir una conexión a la BD
    if (string.IsNullOrWhiteSpace(sku))
        return new GeneralResponse<Product> { Success = false, Message = "El parámetro 'sku' en la URL es obligatorio." };

    try
    {
        // 2. Llamada al repositorio
        var product = await _repository.GetProductBySkuAsync(sku);

        // 3. Regla de negocio: no encontrado
        if (product == null)
            return new GeneralResponse<Product> { Success = false, Message = "Producto no encontrado o inactivo." };

        // 4. Respuesta exitosa
        return new GeneralResponse<Product> { Success = true, Message = "Producto encontrado.", Data = product };
    }
    catch (Exception ex)
    {
        // 5. Error controlado: el mensaje viene del procedimiento almacenado o de SQL
        return new GeneralResponse<Product> { Success = false, Message = ex.Message };
    }
}
```

`GetByName` es la lógica compartida por los dos endpoints de búsqueda por nombre (GET y POST). Antes estaba como método privado dentro del controlador. Se movió al servicio para respetar la nueva arquitectura.

### Respuesta estándar `GeneralResponse<T>`

Todas las respuestas tienen la misma forma, lo que facilita el trabajo del front-end:

```json
{
  "success": true,
  "message": "Producto encontrado.",
  "data": { "productId": 1, "sku": "NET-003", "name": "Access Point AC", "description": "...", "price": 90.00 }
}
```

`StockQuantity`, `IsActive` y `CreatedAt` tienen `[JsonIgnore]` en `Models/Product.cs`, por eso no aparecen en el JSON.

---

## 5. Inyección de dependencias (DI)

En `Program.cs` se registran las clases en el contenedor de dependencias:

```csharp
builder.Services.AddControllers();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<ProductsServices>();
```

Cuando llega una petición, ASP.NET necesita un `ProductsController` y resuelve la cadena de constructores:

```
ProductsController(ProductsServices)
    └── ProductsServices(ProductRepository)
            └── ProductRepository(IConfiguration)   ← la registra CreateBuilder automáticamente
```

Nadie escribe `new ProductsServices(...)`: el contenedor crea cada objeto con sus dependencias.

**Tiempos de vida:**

| Registro | Vida de la instancia |
|---|---|
| `AddSingleton` | Una sola para toda la aplicación (ej. `IConfiguration`). |
| `AddScoped` | Una por petición HTTP (por *scope*). Es la que usamos. |
| `AddTransient` | Una nueva cada vez que se pide. |

### Demostración en `Program.cs`

Antes de `app.Run()`, el `Program.cs` crea un *scope* manual (una "petición" simulada) y:

1. Resuelve `IConfiguration` y comprueba la cadena de conexión.
2. Pide `ProductsServices` dos veces en el mismo scope → **misma instancia**.
3. Lo pide en otro scope → **instancia distinta** (eso significa `Scoped`).
4. Llama al servicio directamente: `serviceA.GetAll()`.
5. Crea el controlador con `ActivatorUtilities.CreateInstance<ProductsController>(...)` (porque `AddControllers()` no registra los controladores como servicios) y llama a todos sus métodos, incluidos los casos inválidos.

Salida obtenida:

```
[5] ProductsController creado con ActivatorUtilities (el contenedor inyectó ProductsServices).
    Cadena: Controller -> Services -> Repository -> sp_GetAllActiveProducts

    controller.GetAll()                -> Success=True | Productos obtenidos exitosamente.
    controller.GetByRoute("NET-003")  -> Success=True | Producto encontrado.
    controller.GetByQuery("NET-003")  -> Success=True | Producto encontrado.
    controller.GetByBody(Sku)          -> Success=True | Producto encontrado.
    controller.GetByQuery("")          -> Success=False | El parámetro 'sku' en la URL es obligatorio.
    controller.GetByNameFromQuery("a") -> Success=True | Se encontraron 41 producto(s).
    controller.GetByNameFromBody(Name) -> Success=True | Se encontraron 41 producto(s).
    controller.GetByNameFromQuery("")  -> Success=False | El parámetro 'name' es obligatorio.
```

---

## 6. Flujo completo de una petición

`GET /api/products/by-name?name=laptop`

1. **Kestrel** (el servidor web de .NET) recibe la petición.
2. El **enrutador** (`app.MapControllers()`) encuentra `GetByNameFromQuery` por el verbo `GET` y la ruta `by-name`.
3. El **contenedor DI** crea el scope de la petición y construye `ProductRepository` → `ProductsServices` → `ProductsController`.
4. El **model binding** lee `name=laptop` de la query string.
5. El controlador llama a `_services.GetByName("laptop")`.
6. El servicio valida y llama a `_repository.GetProductsByNameAsync("laptop")`.
7. El repositorio ejecuta `sp_GetAllActiveProducts` con `@method = 'GetProductsByName'` y `@Name = 'laptop'`, y mapea las filas a `List<Product>`.
8. El servicio envuelve el resultado en `GeneralResponse<List<Product>>`.
9. ASP.NET serializa la respuesta a JSON y la envía.
10. El scope termina y sus instancias se liberan.

---

## 7. Cómo ejecutar el proyecto

Requisitos:

- SDK de .NET (9 o superior) para compilar.
- Runtime de **.NET 9** y **ASP.NET Core 9** para ejecutar (el proyecto tiene `<TargetFramework>net9.0</TargetFramework>`).
- Archivo `.env` con la cadena de conexión (no se sube al repositorio, ver `.gitignore`).

```bash
dotnet build
dotnet run
```

La consola muestra la demostración de DI y luego `Now listening on: http://localhost:5161`.
