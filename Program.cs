using DotNetEnv;
using backend_II.Controllers;
using backend_II.Database;
using backend_II.Models;
using backend_II.Services;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Registro de las capas en el contenedor de dependencias:
//   ProductsController -> ProductsServices -> ProductRepository -> SQL Server
// El controlador pide un ProductsServices y el servicio pide un ProductRepository.
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<ProductsServices>();

var app = builder.Build();

// ===== INYECCIÓN DE DEPENDENCIAS DESDE MAIN =====
// Este bloque se ejecuta una sola vez al arrancar, antes de que el servidor
// empiece a escuchar peticiones. Sirve para ver en consola cómo el contenedor
// de dependencias construye los objetos que normalmente crea el framework.

Console.WriteLine("\n===== DEMO: INYECCIÓN DE DEPENDENCIAS =====\n");

// 1) IConfiguration: no la registramos nosotros, CreateBuilder ya la trae.
//    Se resuelve directo desde app.Services porque es Singleton.
var configuration = app.Services.GetRequiredService<IConfiguration>();
var connectionString = configuration.GetConnectionString("DefaultConnection");
Console.WriteLine("[1] IConfiguration resuelta por el contenedor.");
Console.WriteLine($"    ¿Hay cadena de conexión? {(string.IsNullOrWhiteSpace(connectionString) ? "NO" : "SÍ")}\n");

// 2) Scoped: un servicio Scoped vive lo que dura un "scope" (en la web, una petición).
//    Fuera de una petición hay que crear el scope a mano; si no, el contenedor
//    no permite resolver servicios Scoped.
using (var scope1 = app.Services.CreateScope())
{
    var serviceA = scope1.ServiceProvider.GetRequiredService<ProductsServices>();
    var serviceB = scope1.ServiceProvider.GetRequiredService<ProductsServices>();

    Console.WriteLine("[2] ProductsServices pedido dos veces en el MISMO scope:");
    Console.WriteLine($"    serviceA = #{serviceA.GetHashCode()}  |  serviceB = #{serviceB.GetHashCode()}");
    Console.WriteLine($"    ¿Es la misma instancia? {ReferenceEquals(serviceA, serviceB)}  (Scoped => una por scope)\n");

    using (var scope2 = app.Services.CreateScope())
    {
        var serviceC = scope2.ServiceProvider.GetRequiredService<ProductsServices>();
        Console.WriteLine("[3] ProductsServices pedido en OTRO scope (otra \"petición\"):");
        Console.WriteLine($"    serviceC = #{serviceC.GetHashCode()}");
        Console.WriteLine($"    ¿Es la misma que serviceA? {ReferenceEquals(serviceA, serviceC)}  (cada petición recibe la suya)\n");
    }

    // 3) Usar la capa de servicios directamente. Para construir ProductsServices
    //    el contenedor tuvo que crear antes un ProductRepository (su dependencia).
    //    El servicio ya devuelve un GeneralResponse con validaciones y manejo de errores.
    Console.WriteLine("[4] Llamando a la capa de servicios: serviceA.GetAll()");
    var respuestaServicio = await serviceA.GetAll();
    Console.WriteLine($"    Success={respuestaServicio.Success} | {respuestaServicio.Message}");
    foreach (var p in (respuestaServicio.Data ?? new List<Product>()).Take(5))
    {
        Console.WriteLine($"    - {p.SKU} | {p.Name} | ${p.Price} | stock {p.StockQuantity}");
    }
    Console.WriteLine();

    // 4) Crear el controlador. AddControllers() NO registra los controladores como
    //    servicios, así que se usa ActivatorUtilities: lee el constructor de
    //    ProductsController, ve que pide un ProductsServices y se lo pide al scope.
    //    El controlador ya no habla con la base de datos: solo delega al servicio.
    var controller = ActivatorUtilities.CreateInstance<ProductsController>(scope1.ServiceProvider);
    Console.WriteLine("[5] ProductsController creado con ActivatorUtilities (el contenedor inyectó ProductsServices).");
    Console.WriteLine("    Cadena: Controller -> Services -> Repository -> sp_GetAllActiveProducts\n");

    var respuestaTodos = await controller.GetAll();
    Console.WriteLine($"    controller.GetAll()                -> Success={respuestaTodos.Success} | {respuestaTodos.Message}");

    // Se toma un SKU real de la lista para probar las búsquedas por SKU.
    var skuEjemplo = respuestaTodos.Data?.FirstOrDefault()?.SKU ?? "SKU-INEXISTENTE";

    var respuestaRuta = await controller.GetByRoute(skuEjemplo);
    Console.WriteLine($"    controller.GetByRoute(\"{skuEjemplo}\")  -> Success={respuestaRuta.Success} | {respuestaRuta.Message}");

    var respuestaQuery = await controller.GetByQuery(skuEjemplo);
    Console.WriteLine($"    controller.GetByQuery(\"{skuEjemplo}\")  -> Success={respuestaQuery.Success} | {respuestaQuery.Message}");

    var respuestaBody = await controller.GetByBody(new ProductSearchRequest { Sku = skuEjemplo });
    Console.WriteLine($"    controller.GetByBody(Sku)          -> Success={respuestaBody.Success} | {respuestaBody.Message}");

    var respuestaQueryVacia = await controller.GetByQuery("");
    Console.WriteLine($"    controller.GetByQuery(\"\")          -> Success={respuestaQueryVacia.Success} | {respuestaQueryVacia.Message}");

    var respuestaNombre = await controller.GetByNameFromQuery("a");
    Console.WriteLine($"    controller.GetByNameFromQuery(\"a\") -> Success={respuestaNombre.Success} | {respuestaNombre.Message}");

    var respuestaNombreBody = await controller.GetByNameFromBody(new ProductSearchRequest { Name = "a" });
    Console.WriteLine($"    controller.GetByNameFromBody(Name) -> Success={respuestaNombreBody.Success} | {respuestaNombreBody.Message}");

    var respuestaVacia = await controller.GetByNameFromQuery("");
    Console.WriteLine($"    controller.GetByNameFromQuery(\"\")  -> Success={respuestaVacia.Success} | {respuestaVacia.Message}");
}
// Al cerrar el using, el scope se destruye y con él todas sus instancias Scoped.

Console.WriteLine("\n===== FIN DEMO: el servidor ahora atiende peticiones HTTP =====\n");
// ===== FIN DEMOSTRACIÓN =====

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
