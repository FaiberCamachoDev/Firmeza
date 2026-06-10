using Firmeza.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        string[] roles = ["Admin", "Cliente"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        const string adminEmail = "admin@firmeza.com";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "Firmeza",
                DocumentNumber = "00000000",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        await SeedCustomersAsync(db);
        await SeedProductsAsync(db);
    }

    private static async Task SeedCustomersAsync(ApplicationDbContext db)
    {
        if (await db.Customers.AnyAsync()) return;

        var customers = new List<Customer>
        {
            new() { FirstName = "Carlos",    LastName = "Mendoza",    DocumentNumber = "10234567", Phone = "999-1001", Email = "carlos.mendoza@mail.com",   Address = "Av. Lima 123, Lima",            IsActive = true  },
            new() { FirstName = "María",     LastName = "Torres",     DocumentNumber = "10234568", Phone = "999-1002", Email = "maria.torres@mail.com",     Address = "Jr. Cusco 456, Arequipa",       IsActive = true  },
            new() { FirstName = "José",      LastName = "Quispe",     DocumentNumber = "10234569", Phone = "999-1003", Email = "jose.quispe@mail.com",      Address = "Ca. Trujillo 789, Trujillo",    IsActive = true  },
            new() { FirstName = "Ana",       LastName = "Flores",     DocumentNumber = "10234570", Phone = "999-1004", Email = "ana.flores@mail.com",       Address = "Av. Grau 321, Piura",           IsActive = true  },
            new() { FirstName = "Luis",      LastName = "Romero",     DocumentNumber = "10234571", Phone = "999-1005", Email = "luis.romero@mail.com",      Address = "Jr. Bolívar 654, Chiclayo",     IsActive = true  },
            new() { FirstName = "Rosa",      LastName = "Vargas",     DocumentNumber = "10234572", Phone = "999-1006", Email = "rosa.vargas@mail.com",      Address = "Ca. Puno 987, Puno",            IsActive = true  },
            new() { FirstName = "Pedro",     LastName = "Chávez",     DocumentNumber = "10234573", Phone = "999-1007", Email = "pedro.chavez@mail.com",     Address = "Av. Tacna 147, Tacna",          IsActive = true  },
            new() { FirstName = "Lucía",     LastName = "Mamani",     DocumentNumber = "10234574", Phone = "999-1008", Email = "lucia.mamani@mail.com",     Address = "Jr. Ica 258, Ica",              IsActive = true  },
            new() { FirstName = "Jorge",     LastName = "Huanca",     DocumentNumber = "10234575", Phone = "999-1009", Email = "jorge.huanca@mail.com",     Address = "Ca. Moquegua 369, Moquegua",    IsActive = true  },
            new() { FirstName = "Carmen",    LastName = "Salazar",    DocumentNumber = "10234576", Phone = "999-1010", Email = "carmen.salazar@mail.com",   Address = "Av. Ancash 741, Huaraz",        IsActive = true  },
            new() { FirstName = "Miguel",    LastName = "Paredes",    DocumentNumber = "10234577", Phone = "999-1011", Email = "miguel.paredes@mail.com",   Address = "Jr. Junín 852, Huancayo",       IsActive = true  },
            new() { FirstName = "Elena",     LastName = "Castillo",   DocumentNumber = "10234578", Phone = "999-1012", Email = "elena.castillo@mail.com",   Address = "Ca. Loreto 963, Iquitos",       IsActive = true  },
            new() { FirstName = "Roberto",   LastName = "Gutiérrez",  DocumentNumber = "10234579", Phone = "999-1013", Email = "roberto.gutierrez@mail.com", Address = "Av. San Martín 159, Tarapoto", IsActive = true  },
            new() { FirstName = "Silvia",    LastName = "Ramos",      DocumentNumber = "10234580", Phone = "999-1014", Email = "silvia.ramos@mail.com",     Address = "Jr. Amazonas 357, Chachapoyas", IsActive = false },
            new() { FirstName = "Fernando",  LastName = "Díaz",       DocumentNumber = "10234581", Phone = "999-1015", Email = "fernando.diaz@mail.com",    Address = "Ca. Tumbes 468, Tumbes",        IsActive = true  },
            new() { FirstName = "Patricia",  LastName = "Ccopa",      DocumentNumber = "10234582", Phone = "999-1016", Email = "patricia.ccopa@mail.com",   Address = "Av. Madre de Dios 579, Puerto Maldonado", IsActive = true },
            new() { FirstName = "Ricardo",   LastName = "Lazo",       DocumentNumber = "10234583", Phone = "999-1017", Email = "ricardo.lazo@mail.com",     Address = "Jr. Apurímac 681, Abancay",     IsActive = true  },
            new() { FirstName = "Gloria",    LastName = "Nieto",      DocumentNumber = "10234584", Phone = "999-1018", Email = "gloria.nieto@mail.com",     Address = "Ca. Huánuco 792, Huánuco",      IsActive = false },
            new() { FirstName = "Andrés",    LastName = "Soto",       DocumentNumber = "10234585", Phone = "999-1019", Email = "andres.soto@mail.com",      Address = "Av. Ayacucho 803, Ayacucho",    IsActive = true  },
            new() { FirstName = "Verónica",  LastName = "Palomino",   DocumentNumber = "10234586", Phone = "999-1020", Email = "veronica.palomino@mail.com", Address = "Jr. Cajamarca 914, Cajamarca", IsActive = true  },
        };

        db.Customers.AddRange(customers);
        await db.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ApplicationDbContext db)
    {
        if (await db.Products.AnyAsync()) return;

        var products = new List<Product>
        {
            // Cementos y morteros
            new() { Name = "Cemento Portland Tipo I 42.5kg",    Description = "Cemento gris de uso general, fraguado normal",              Unit = "bol", Price = 28.50m,  Stock = 500, Category = "Cementos",      IsActive = true  },
            new() { Name = "Cemento Portland Tipo II 42.5kg",   Description = "Resistente a sulfatos, ideal para obras con humedad",       Unit = "bol", Price = 31.00m,  Stock = 300, Category = "Cementos",      IsActive = true  },
            new() { Name = "Cemento blanco 25kg",               Description = "Para acabados finos, juntas y decoración",                  Unit = "bol", Price = 42.00m,  Stock = 150, Category = "Cementos",      IsActive = true  },
            new() { Name = "Mortero premezclado interior 25kg", Description = "Mezcla lista para tarrajeo de muros interiores",            Unit = "bol", Price = 18.90m,  Stock = 200, Category = "Cementos",      IsActive = true  },
            new() { Name = "Mortero premezclado exterior 25kg", Description = "Aditivos impermeabilizantes, resiste lluvia y humedad",     Unit = "bol", Price = 21.50m,  Stock = 180, Category = "Cementos",      IsActive = true  },

            // Agregados
            new() { Name = "Arena fina (saco 50kg)",            Description = "Arena lavada granulometría fina, para tarrajeos",           Unit = "sac", Price = 12.00m,  Stock = 400, Category = "Agregados",     IsActive = true  },
            new() { Name = "Arena gruesa (saco 50kg)",          Description = "Arena de río, granulometría media, para concreto",          Unit = "sac", Price = 11.00m,  Stock = 420, Category = "Agregados",     IsActive = true  },
            new() { Name = "Piedra chancada 3/4\" (saco 50kg)", Description = "Piedra triturada, para vaciados de concreto estructural",   Unit = "sac", Price = 14.50m,  Stock = 350, Category = "Agregados",     IsActive = true  },
            new() { Name = "Piedra chancada 1/2\" (saco 50kg)", Description = "Para losas y elementos de menor espesor",                  Unit = "sac", Price = 14.00m,  Stock = 300, Category = "Agregados",     IsActive = true  },
            new() { Name = "Confitillo (saco 50kg)",            Description = "Piedra menuda para rellenos y concreto ciclópeo",           Unit = "sac", Price = 10.50m,  Stock = 250, Category = "Agregados",     IsActive = false },

            // Acero y fierro
            new() { Name = "Varilla corrugada 1/2\" x 9m",     Description = "Acero corrugado grado 60, para columnas y vigas",           Unit = "und", Price = 38.00m,  Stock = 600, Category = "Acero",         IsActive = true  },
            new() { Name = "Varilla corrugada 3/8\" x 9m",     Description = "Acero corrugado grado 60, para estribos y losas",           Unit = "und", Price = 22.00m,  Stock = 700, Category = "Acero",         IsActive = true  },
            new() { Name = "Varilla corrugada 1/4\" x 9m",     Description = "Acero liso, para mallas y refuerzos menores",               Unit = "und", Price = 14.00m,  Stock = 500, Category = "Acero",         IsActive = true  },
            new() { Name = "Alambre negro N°16 (rollo 1kg)",   Description = "Alambre recocido para amarre de fierro",                    Unit = "rol", Price = 5.50m,   Stock = 800, Category = "Acero",         IsActive = true  },
            new() { Name = "Malla electrosoldada 15x15 6mm",   Description = "Panel 2.4x6m, para losas y muros",                         Unit = "pnl", Price = 185.00m, Stock = 80,  Category = "Acero",         IsActive = true  },

            // Ladrillos y bloques
            new() { Name = "Ladrillo King Kong 18 huecos",     Description = "Ladrillo industrial 23x13x9cm, para muros portantes",       Unit = "und", Price = 1.20m,   Stock = 5000,Category = "Ladrillos",     IsActive = true  },
            new() { Name = "Ladrillo pandereta",               Description = "Para muros no portantes y tabiques, 23x11x9cm",             Unit = "und", Price = 0.90m,   Stock = 4000,Category = "Ladrillos",     IsActive = true  },
            new() { Name = "Bloque de concreto 15x20x40cm",    Description = "Para muros de contención y cercos",                         Unit = "und", Price = 3.80m,   Stock = 1200,Category = "Ladrillos",     IsActive = true  },
            new() { Name = "Ladrillo para techo 30x30",        Description = "Bovedilla de arcilla para aligerado de losas",              Unit = "und", Price = 2.10m,   Stock = 2000,Category = "Ladrillos",     IsActive = true  },
            new() { Name = "Adoquín gris 10x20x6cm",           Description = "Para pisos exteriores, patios y veredas",                  Unit = "und", Price = 2.50m,   Stock = 3000,Category = "Ladrillos",     IsActive = false },

            // Tuberías y sanitarios
            new() { Name = "Tubería PVC SAP 1/2\" x 3m",       Description = "Para instalaciones de agua a presión",                     Unit = "und", Price = 8.50m,   Stock = 300, Category = "Tuberías",      IsActive = true  },
            new() { Name = "Tubería PVC SAP 3/4\" x 3m",       Description = "Para instalaciones de agua a presión",                     Unit = "und", Price = 12.00m,  Stock = 250, Category = "Tuberías",      IsActive = true  },
            new() { Name = "Tubería PVC desagüe 4\" x 3m",     Description = "Para redes de desagüe domiciliario",                       Unit = "und", Price = 22.00m,  Stock = 200, Category = "Tuberías",      IsActive = true  },
            new() { Name = "Tubería PVC desagüe 2\" x 3m",     Description = "Para ventilación y desagüe de lavatorios",                 Unit = "und", Price = 11.50m,  Stock = 220, Category = "Tuberías",      IsActive = true  },
            new() { Name = "Codo PVC 90° 1/2\"",               Description = "Accesorio para instalaciones de agua",                     Unit = "und", Price = 0.80m,   Stock = 1000,Category = "Tuberías",      IsActive = true  },

            // Pinturas
            new() { Name = "Pintura látex interior blanco 4L", Description = "Rendimiento 40m²/gal, secado rápido, lavable",             Unit = "gal", Price = 38.00m,  Stock = 120, Category = "Pinturas",      IsActive = true  },
            new() { Name = "Pintura látex exterior 4L",        Description = "Resistente a rayos UV, alta cobertura",                    Unit = "gal", Price = 45.00m,  Stock = 100, Category = "Pinturas",      IsActive = true  },
            new() { Name = "Pintura epóxica piso gris 4L",     Description = "Para pisos de concreto, tráfico pesado",                   Unit = "gal", Price = 89.00m,  Stock = 60,  Category = "Pinturas",      IsActive = true  },
            new() { Name = "Imprimante blanco 25kg",           Description = "Para preparar superficies antes de pintar",                Unit = "bol", Price = 24.00m,  Stock = 180, Category = "Pinturas",      IsActive = true  },
            new() { Name = "Sellador de techos transparente 4L", Description = "Impermeabilizante acrílico para losas y techos",         Unit = "gal", Price = 62.00m,  Stock = 75,  Category = "Pinturas",      IsActive = true  },

            // Herramientas
            new() { Name = "Pala tipo cuchara",                Description = "Mango de madera reforzado, hoja de acero 1.5mm",           Unit = "und", Price = 32.00m,  Stock = 80,  Category = "Herramientas",  IsActive = true  },
            new() { Name = "Pico de minero",                   Description = "Doble punta, mango de madera, 2.5kg",                      Unit = "und", Price = 38.00m,  Stock = 60,  Category = "Herramientas",  IsActive = true  },
            new() { Name = "Carretilla buggy 80L",             Description = "Tina de polietileno, rueda neumática, capacidad 80L",      Unit = "und", Price = 189.00m, Stock = 25,  Category = "Herramientas",  IsActive = true  },
            new() { Name = "Nivel de burbuja 60cm",            Description = "Aluminio extruido, 3 ampollas, precisión ±0.5mm/m",        Unit = "und", Price = 28.00m,  Stock = 50,  Category = "Herramientas",  IsActive = true  },
            new() { Name = "Plomada metálica 500g",            Description = "Acero cromado, cordel de 10m incluido",                    Unit = "und", Price = 15.00m,  Stock = 70,  Category = "Herramientas",  IsActive = true  },
            new() { Name = "Badilejo de albañil 7\"",          Description = "Hoja de acero inoxidable, mango de goma",                  Unit = "und", Price = 12.00m,  Stock = 90,  Category = "Herramientas",  IsActive = true  },
            new() { Name = "Frotacho de madera",               Description = "Para tarrajeo y acabado de superficies, 12x28cm",          Unit = "und", Price = 8.50m,   Stock = 100, Category = "Herramientas",  IsActive = true  },
            new() { Name = "Wincha métrica 5m",                Description = "Cinta de acero, gancho magnético, freno automático",       Unit = "und", Price = 18.00m,  Stock = 120, Category = "Herramientas",  IsActive = true  },

            // Eléctrico
            new() { Name = "Cable TW 2.5mm² (rollo 100m)",    Description = "Conductor de cobre recocido, aislamiento PVC",             Unit = "rol", Price = 145.00m, Stock = 50,  Category = "Eléctrico",     IsActive = true  },
            new() { Name = "Cable TW 4mm² (rollo 100m)",      Description = "Para circuitos de mayor demanda, cocinas y termas",        Unit = "rol", Price = 210.00m, Stock = 40,  Category = "Eléctrico",     IsActive = true  },
            new() { Name = "Tomacorriente doble universal",    Description = "16A 250V, con toma a tierra, marco incluido",              Unit = "und", Price = 12.50m,  Stock = 200, Category = "Eléctrico",     IsActive = true  },
            new() { Name = "Interruptor simple",               Description = "10A 250V, encendido/apagado, marco incluido",              Unit = "und", Price = 8.90m,   Stock = 250, Category = "Eléctrico",     IsActive = true  },
            new() { Name = "Tablero eléctrico 6 circuitos",   Description = "Gabinete metálico con riel DIN, puerta ciega",             Unit = "und", Price = 95.00m,  Stock = 30,  Category = "Eléctrico",     IsActive = true  },

            // Impermeabilizantes y aditivos
            new() { Name = "Sika 1 impermeabilizante 20kg",   Description = "Aditivo cristalizante para concreto y mortero",            Unit = "bol", Price = 68.00m,  Stock = 90,  Category = "Aditivos",      IsActive = true  },
            new() { Name = "Chema 1 impermeabilizante 4kg",   Description = "Líquido para techos y muros húmedos",                      Unit = "gal", Price = 48.00m,  Stock = 80,  Category = "Aditivos",      IsActive = true  },
            new() { Name = "Aditivo acelerante de fragua 1L", Description = "Reduce tiempo de fraguado a 30 minutos",                   Unit = "und", Price = 22.00m,  Stock = 110, Category = "Aditivos",      IsActive = true  },
            new() { Name = "Desmoldante para encofrado 4L",   Description = "Facilita el retiro de encofrados de madera y metálicos",   Unit = "gal", Price = 35.00m,  Stock = 70,  Category = "Aditivos",      IsActive = false },
            new() { Name = "Curador de concreto 4L",          Description = "Película sellante para curado de losas y pavimentos",      Unit = "gal", Price = 42.00m,  Stock = 65,  Category = "Aditivos",      IsActive = true  },
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }
}
