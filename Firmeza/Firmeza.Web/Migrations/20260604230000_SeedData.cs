using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Firmeza.Web.Migrations
{
    public partial class SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT INTO ""Customers"" (""FirstName"", ""LastName"", ""DocumentNumber"", ""Phone"", ""Email"", ""Address"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
SELECT v.""FirstName"", v.""LastName"", v.""DocumentNumber"", v.""Phone"", v.""Email"", v.""Address"", v.""IsActive"", NOW(), NOW()
FROM (VALUES
    ('Carlos',   'Mendoza',   '10234567', '999-1001', 'carlos.mendoza@mail.com',    'Av. Lima 123, Lima',                      TRUE),
    ('María',    'Torres',    '10234568', '999-1002', 'maria.torres@mail.com',      'Jr. Cusco 456, Arequipa',                 TRUE),
    ('José',     'Quispe',    '10234569', '999-1003', 'jose.quispe@mail.com',       'Ca. Trujillo 789, Trujillo',              TRUE),
    ('Ana',      'Flores',    '10234570', '999-1004', 'ana.flores@mail.com',        'Av. Grau 321, Piura',                     TRUE),
    ('Luis',     'Romero',    '10234571', '999-1005', 'luis.romero@mail.com',       'Jr. Bolivar 654, Chiclayo',               TRUE),
    ('Rosa',     'Vargas',    '10234572', '999-1006', 'rosa.vargas@mail.com',       'Ca. Puno 987, Puno',                      TRUE),
    ('Pedro',    'Chavez',    '10234573', '999-1007', 'pedro.chavez@mail.com',      'Av. Tacna 147, Tacna',                    TRUE),
    ('Lucia',    'Mamani',    '10234574', '999-1008', 'lucia.mamani@mail.com',      'Jr. Ica 258, Ica',                        TRUE),
    ('Jorge',    'Huanca',    '10234575', '999-1009', 'jorge.huanca@mail.com',      'Ca. Moquegua 369, Moquegua',              TRUE),
    ('Carmen',   'Salazar',   '10234576', '999-1010', 'carmen.salazar@mail.com',    'Av. Ancash 741, Huaraz',                  TRUE),
    ('Miguel',   'Paredes',   '10234577', '999-1011', 'miguel.paredes@mail.com',    'Jr. Junin 852, Huancayo',                 TRUE),
    ('Elena',    'Castillo',  '10234578', '999-1012', 'elena.castillo@mail.com',    'Ca. Loreto 963, Iquitos',                 TRUE),
    ('Roberto',  'Gutierrez', '10234579', '999-1013', 'roberto.gutierrez@mail.com', 'Av. San Martin 159, Tarapoto',            TRUE),
    ('Silvia',   'Ramos',     '10234580', '999-1014', 'silvia.ramos@mail.com',      'Jr. Amazonas 357, Chachapoyas',           FALSE),
    ('Fernando', 'Diaz',      '10234581', '999-1015', 'fernando.diaz@mail.com',     'Ca. Tumbes 468, Tumbes',                  TRUE),
    ('Patricia', 'Ccopa',     '10234582', '999-1016', 'patricia.ccopa@mail.com',    'Av. Madre de Dios 579, Puerto Maldonado', TRUE),
    ('Ricardo',  'Lazo',      '10234583', '999-1017', 'ricardo.lazo@mail.com',      'Jr. Apurimac 681, Abancay',               TRUE),
    ('Gloria',   'Nieto',     '10234584', '999-1018', 'gloria.nieto@mail.com',      'Ca. Huanuco 792, Huanuco',                FALSE),
    ('Andres',   'Soto',      '10234585', '999-1019', 'andres.soto@mail.com',       'Av. Ayacucho 803, Ayacucho',              TRUE),
    ('Veronica', 'Palomino',  '10234586', '999-1020', 'veronica.palomino@mail.com', 'Jr. Cajamarca 914, Cajamarca',            TRUE)
) AS v(""FirstName"", ""LastName"", ""DocumentNumber"", ""Phone"", ""Email"", ""Address"", ""IsActive"")
WHERE NOT EXISTS (SELECT 1 FROM ""Customers"" WHERE ""DocumentNumber"" = v.""DocumentNumber"");
");

            migrationBuilder.Sql(@"
INSERT INTO ""Products"" (""Name"", ""Description"", ""Unit"", ""Price"", ""Stock"", ""Category"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
SELECT v.""Name"", v.""Description"", v.""Unit"", v.""Price"", v.""Stock"", v.""Category"", v.""IsActive"", NOW(), NOW()
FROM (VALUES
    ('Cemento Portland Tipo I 42.5kg',     'Cemento gris de uso general, fraguado normal',                 'bol',  28.50,  500,  'Cementos',     TRUE),
    ('Cemento Portland Tipo II 42.5kg',    'Resistente a sulfatos, ideal para obras con humedad',          'bol',  31.00,  300,  'Cementos',     TRUE),
    ('Cemento blanco 25kg',                'Para acabados finos, juntas y decoracion',                     'bol',  42.00,  150,  'Cementos',     TRUE),
    ('Mortero premezclado interior 25kg',  'Mezcla lista para tarrajeo de muros interiores',               'bol',  18.90,  200,  'Cementos',     TRUE),
    ('Mortero premezclado exterior 25kg',  'Aditivos impermeabilizantes, resiste lluvia y humedad',        'bol',  21.50,  180,  'Cementos',     TRUE),
    ('Arena fina saco 50kg',               'Arena lavada granulometria fina, para tarrajeos',              'sac',  12.00,  400,  'Agregados',    TRUE),
    ('Arena gruesa saco 50kg',             'Arena de rio, granulometria media, para concreto',             'sac',  11.00,  420,  'Agregados',    TRUE),
    ('Piedra chancada 3/4 saco 50kg',      'Piedra triturada, para vaciados de concreto estructural',     'sac',  14.50,  350,  'Agregados',    TRUE),
    ('Piedra chancada 1/2 saco 50kg',      'Para losas y elementos de menor espesor',                     'sac',  14.00,  300,  'Agregados',    TRUE),
    ('Confitillo saco 50kg',               'Piedra menuda para rellenos y concreto ciclopeo',              'sac',  10.50,  250,  'Agregados',    FALSE),
    ('Varilla corrugada 1/2 x 9m',         'Acero corrugado grado 60, para columnas y vigas',             'und',  38.00,  600,  'Acero',        TRUE),
    ('Varilla corrugada 3/8 x 9m',         'Acero corrugado grado 60, para estribos y losas',             'und',  22.00,  700,  'Acero',        TRUE),
    ('Varilla corrugada 1/4 x 9m',         'Acero liso, para mallas y refuerzos menores',                 'und',  14.00,  500,  'Acero',        TRUE),
    ('Alambre negro N16 rollo 1kg',        'Alambre recocido para amarre de fierro',                      'rol',   5.50,  800,  'Acero',        TRUE),
    ('Malla electrosoldada 15x15 6mm',     'Panel 2.4x6m, para losas y muros',                            'pnl', 185.00,   80,  'Acero',        TRUE),
    ('Ladrillo King Kong 18 huecos',       'Ladrillo industrial 23x13x9cm, para muros portantes',         'und',   1.20, 5000,  'Ladrillos',    TRUE),
    ('Ladrillo pandereta',                 'Para muros no portantes y tabiques, 23x11x9cm',               'und',   0.90, 4000,  'Ladrillos',    TRUE),
    ('Bloque de concreto 15x20x40cm',      'Para muros de contencion y cercos',                           'und',   3.80, 1200,  'Ladrillos',    TRUE),
    ('Ladrillo para techo 30x30',          'Bovedilla de arcilla para aligerado de losas',                'und',   2.10, 2000,  'Ladrillos',    TRUE),
    ('Adoquin gris 10x20x6cm',             'Para pisos exteriores, patios y veredas',                     'und',   2.50, 3000,  'Ladrillos',    FALSE),
    ('Tuberia PVC SAP 1/2 x 3m',           'Para instalaciones de agua a presion',                        'und',   8.50,  300,  'Tuberias',     TRUE),
    ('Tuberia PVC SAP 3/4 x 3m',           'Para instalaciones de agua a presion',                        'und',  12.00,  250,  'Tuberias',     TRUE),
    ('Tuberia PVC desague 4 x 3m',         'Para redes de desague domiciliario',                          'und',  22.00,  200,  'Tuberias',     TRUE),
    ('Tuberia PVC desague 2 x 3m',         'Para ventilacion y desague de lavatorios',                    'und',  11.50,  220,  'Tuberias',     TRUE),
    ('Codo PVC 90 grados 1/2',             'Accesorio para instalaciones de agua',                        'und',   0.80, 1000,  'Tuberias',     TRUE),
    ('Pintura latex interior blanco 4L',   'Rendimiento 40m2/gal, secado rapido, lavable',                'gal',  38.00,  120,  'Pinturas',     TRUE),
    ('Pintura latex exterior 4L',          'Resistente a rayos UV, alta cobertura',                       'gal',  45.00,  100,  'Pinturas',     TRUE),
    ('Pintura epoxica piso gris 4L',       'Para pisos de concreto, trafico pesado',                      'gal',  89.00,   60,  'Pinturas',     TRUE),
    ('Imprimante blanco 25kg',             'Para preparar superficies antes de pintar',                   'bol',  24.00,  180,  'Pinturas',     TRUE),
    ('Sellador de techos transparente 4L', 'Impermeabilizante acrilico para losas y techos',              'gal',  62.00,   75,  'Pinturas',     TRUE),
    ('Pala tipo cuchara',                  'Mango de madera reforzado, hoja de acero 1.5mm',              'und',  32.00,   80,  'Herramientas', TRUE),
    ('Pico de minero',                     'Doble punta, mango de madera, 2.5kg',                         'und',  38.00,   60,  'Herramientas', TRUE),
    ('Carretilla buggy 80L',               'Tina de polietileno, rueda neumatica, capacidad 80L',         'und', 189.00,   25,  'Herramientas', TRUE),
    ('Nivel de burbuja 60cm',              'Aluminio extruido, 3 ampollas, precision 0.5mm/m',            'und',  28.00,   50,  'Herramientas', TRUE),
    ('Plomada metalica 500g',              'Acero cromado, cordel de 10m incluido',                       'und',  15.00,   70,  'Herramientas', TRUE),
    ('Badilejo de albanil 7 pulgadas',     'Hoja de acero inoxidable, mango de goma',                     'und',  12.00,   90,  'Herramientas', TRUE),
    ('Frotacho de madera',                 'Para tarrajeo y acabado de superficies, 12x28cm',             'und',   8.50,  100,  'Herramientas', TRUE),
    ('Wincha metrica 5m',                  'Cinta de acero, gancho magnetico, freno automatico',          'und',  18.00,  120,  'Herramientas', TRUE),
    ('Cable TW 2.5mm rollo 100m',          'Conductor de cobre recocido, aislamiento PVC',               'rol', 145.00,   50,  'Electrico',    TRUE),
    ('Cable TW 4mm rollo 100m',            'Para circuitos de mayor demanda, cocinas y termas',           'rol', 210.00,   40,  'Electrico',    TRUE),
    ('Tomacorriente doble universal',      '16A 250V, con toma a tierra, marco incluido',                 'und',  12.50,  200,  'Electrico',    TRUE),
    ('Interruptor simple',                 '10A 250V, encendido/apagado, marco incluido',                 'und',   8.90,  250,  'Electrico',    TRUE),
    ('Tablero electrico 6 circuitos',      'Gabinete metalico con riel DIN, puerta ciega',                'und',  95.00,   30,  'Electrico',    TRUE),
    ('Sika 1 impermeabilizante 20kg',      'Aditivo cristalizante para concreto y mortero',               'bol',  68.00,   90,  'Aditivos',     TRUE),
    ('Chema 1 impermeabilizante 4kg',      'Liquido para techos y muros humedos',                         'gal',  48.00,   80,  'Aditivos',     TRUE),
    ('Aditivo acelerante de fragua 1L',    'Reduce tiempo de fraguado a 30 minutos',                      'und',  22.00,  110,  'Aditivos',     TRUE),
    ('Desmoldante para encofrado 4L',      'Facilita el retiro de encofrados de madera y metalicos',      'gal',  35.00,   70,  'Aditivos',     FALSE),
    ('Curador de concreto 4L',             'Pelicula sellante para curado de losas y pavimentos',         'gal',  42.00,   65,  'Aditivos',     TRUE),
    ('Disco corte concreto 4.5 pulgadas',  'Disco abrasivo para amoladora, corte en seco',                'und',  18.00,  150,  'Herramientas', TRUE),
    ('Casco de seguridad blanco',          'Polietileno de alta densidad, ratchet ajustable',             'und',  25.00,  100,  'Seguridad',    TRUE)
) AS v(""Name"", ""Description"", ""Unit"", ""Price"", ""Stock"", ""Category"", ""IsActive"")
WHERE NOT EXISTS (SELECT 1 FROM ""Products"" WHERE ""Name"" = v.""Name"");
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM ""Products"" WHERE ""Name"" IN (
'Cemento Portland Tipo I 42.5kg','Cemento Portland Tipo II 42.5kg','Cemento blanco 25kg',
'Mortero premezclado interior 25kg','Mortero premezclado exterior 25kg',
'Arena fina saco 50kg','Arena gruesa saco 50kg','Piedra chancada 3/4 saco 50kg',
'Piedra chancada 1/2 saco 50kg','Confitillo saco 50kg',
'Varilla corrugada 1/2 x 9m','Varilla corrugada 3/8 x 9m','Varilla corrugada 1/4 x 9m',
'Alambre negro N16 rollo 1kg','Malla electrosoldada 15x15 6mm',
'Ladrillo King Kong 18 huecos','Ladrillo pandereta','Bloque de concreto 15x20x40cm',
'Ladrillo para techo 30x30','Adoquin gris 10x20x6cm',
'Tuberia PVC SAP 1/2 x 3m','Tuberia PVC SAP 3/4 x 3m',
'Tuberia PVC desague 4 x 3m','Tuberia PVC desague 2 x 3m','Codo PVC 90 grados 1/2',
'Pintura latex interior blanco 4L','Pintura latex exterior 4L','Pintura epoxica piso gris 4L',
'Imprimante blanco 25kg','Sellador de techos transparente 4L',
'Pala tipo cuchara','Pico de minero','Carretilla buggy 80L','Nivel de burbuja 60cm',
'Plomada metalica 500g','Badilejo de albanil 7 pulgadas','Frotacho de madera','Wincha metrica 5m',
'Cable TW 2.5mm rollo 100m','Cable TW 4mm rollo 100m',
'Tomacorriente doble universal','Interruptor simple','Tablero electrico 6 circuitos',
'Sika 1 impermeabilizante 20kg','Chema 1 impermeabilizante 4kg',
'Aditivo acelerante de fragua 1L','Desmoldante para encofrado 4L','Curador de concreto 4L',
'Disco corte concreto 4.5 pulgadas','Casco de seguridad blanco');
");

            migrationBuilder.Sql(@"DELETE FROM ""Customers"" WHERE ""DocumentNumber"" IN (
'10234567','10234568','10234569','10234570','10234571','10234572','10234573','10234574',
'10234575','10234576','10234577','10234578','10234579','10234580','10234581','10234582',
'10234583','10234584','10234585','10234586');
");
        }
    }
}
