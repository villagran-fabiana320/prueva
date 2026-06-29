// ============================================================
// PARTE E: Interfaz de consola principal - Program.cs
// Orquesta todo el flujo: configuración, validación, algoritmo,
// dibujo de la matriz, persistencia y reporte inverso
// ============================================================

using Microsoft.Extensions.Configuration;
using DronSimulator;

// ============================================================
// PARTE C: Leer la configuración desde appsettings.json
// (Prohibido hardcodear la cadena de conexión en el código)
// ============================================================
IConfiguration configuracion = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

string connectionString = configuracion.GetConnectionString("PostgreSQL")
    ?? throw new InvalidOperationException(
        "No se encontró 'ConnectionStrings:PostgreSQL' en appsettings.json");

// ============================================================
// PARTE E - PUNTO 2: Solicitar y validar N y las coordenadas
// ============================================================
Console.WriteLine("==============================================");
Console.WriteLine("  SIMULADOR DE TRAYECTORIA DE DRON - v1.0   ");
Console.WriteLine("==============================================\n");

// Leer y validar N (debe ser entero >= 1)
int n = 0;
bool nValido = false;

while (!nValido)
{
    Console.Write("Ingrese el tamaño del terreno N (entero >= 1): ");
    string? entradaN = Console.ReadLine();

    if (int.TryParse(entradaN, out n) && n >= 1)
    {
        nValido = true;
    }
    else
    {
        Console.WriteLine("Valor inválido. N debe ser un entero mayor o igual a 1.\n");
    }
}

// Leer y validar coordenada X (fila de despegue, rango [0, N-1])
int coordX = 0;
bool xValido = false;

while (!xValido)
{
    Console.Write($"Ingrese la fila de despegue X (0 a {n - 1}): ");
    string? entradaX = Console.ReadLine();

    if (int.TryParse(entradaX, out coordX) && coordX >= 0 && coordX <= n - 1)
    {
        xValido = true;
    }
    else
    {
        Console.WriteLine($"Valor inválido. X debe estar en el rango [0, {n - 1}].\n");
    }
}

// Leer y validar coordenada Y (columna de despegue, rango [0, N-1])
int coordY = 0;
bool yValido = false;

while (!yValido)
{
    Console.Write($"Ingrese la columna de despegue Y (0 a {n - 1}): ");
    string? entradaY = Console.ReadLine();

    if (int.TryParse(entradaY, out coordY) && coordY >= 0 && coordY <= n - 1)
    {
        yValido = true;
    }
    else
    {
        Console.WriteLine($"Valor inválido. Y debe estar en el rango [0, {n - 1}].\n");
    }
}

// ============================================================
// PARTE B: Ejecutar el algoritmo de vuelo recursivo
// ============================================================
Console.WriteLine($"\nIniciando simulación: terreno {n}x{n}, despegue en ({coordX}, {coordY})...\n");

AlgoritmoDron dron = new AlgoritmoDron();
bool encontroSolucion = dron.Resolver(n, coordX, coordY);

// ============================================================
// PARTE E - PUNTO 3: Mostrar la matriz del recorrido
// ============================================================
if (!encontroSolucion)
{
    Console.WriteLine($"SIN SOLUCIÓN: el dron puede alcanzar {dron.TotalAlcanzables} parcela(s) " +
                       "pero no existe ningún recorrido que las cubra todas sin repetir.");
    Console.WriteLine("  (Esto ocurre, por ejemplo, en terrenos de N=4)");
    return;
}

// Mostrar la matriz numérica con el recorrido encontrado
int[,] tablero = dron.ObtenerTablero();

Console.WriteLine($"ÉXITO: el dron cubrió {dron.TotalAlcanzables} parcela(s) de {n * n} en el terreno.\n");
Console.WriteLine("Recorrido calculado (número = orden de pisada, '.' = no alcanzable):");
Console.WriteLine();

int maxNumero   = dron.TotalAlcanzables - 1;
int anchoNumero = maxNumero.ToString().Length + 1;

for (int f = 0; f < n; f++)
{
    for (int c = 0; c < n; c++)
    {
        if (tablero[f, c] >= 0)
            Console.Write(tablero[f, c].ToString().PadLeft(anchoNumero));
        else
            Console.Write(".".PadLeft(anchoNumero));
    }
    Console.WriteLine();
}

Console.WriteLine();

// ============================================================
// PARTE D + E - PUNTO 4: Guardar en PostgreSQL e informar el ID
// ============================================================
Console.Write("Desea guardar esta simulación en PostgreSQL? (s/n): ");
string? respuesta = Console.ReadLine();

if (respuesta?.ToLower() != "s")
{
    Console.WriteLine("Simulación no guardada. Fin del programa.");
    return;
}

List<(int paso, int fila, int col)> secuencia = dron.ObtenerSecuencia();

RepositorioDron repositorio = new RepositorioDron(connectionString);

try
{
    int idSimulacion = repositorio.GuardarSimulacion(n, coordX, coordY, secuencia);
    Console.WriteLine($"Simulación guardada con ID: {idSimulacion}");

    // ============================================================
    // PARTE E - PUNTO 5: Reporte inverso con los últimos 5 pasos
    // ============================================================
    repositorio.MostrarUltimos5Pasos(idSimulacion);
}
catch (Exception ex)
{
    Console.WriteLine($"\nNo se pudo conectar o guardar en la base de datos.");
    Console.WriteLine($"  Detalle: {ex.Message}");
    Console.WriteLine("  Verifique los datos en appsettings.json");
}

Console.WriteLine("\n==============================================");
Console.WriteLine("  Fin de la simulación.");
Console.WriteLine("==============================================");
