// ============================================================
// PARTE D: Persistencia en PostgreSQL con ADO.NET SÍNCRONO
// Prohibido: ORMs, async/await, for/foreach en persistencia
// ============================================================

using Npgsql;

namespace DronSimulator;

public class RepositorioDron
{
    // La cadena de conexión se recibe desde afuera (leída del JSON en Program.cs)
    private readonly string _connectionString;

    public RepositorioDron(string connectionString)
    {
        _connectionString = connectionString;
    }

    // ----------------------------------------------------------------
    // Guarda una simulación exitosa: cabecera + todos los movimientos
    // Todo dentro de una transacción para que sea atómico
    // ----------------------------------------------------------------
    public int GuardarSimulacion(int n, int filaInicio, int colInicio,
                                 List<(int paso, int fila, int col)> secuencia)
    {
        int idGenerado = 0;

        // "using" garantiza que la conexión se cierre aunque haya un error
        using (NpgsqlConnection conexion = new NpgsqlConnection(_connectionString))
        {
            conexion.Open();

            // Iniciamos la transacción para que todo se guarde junto o nada
            NpgsqlTransaction transaccion = conexion.BeginTransaction();

            try
            {
                // --------------------------------------------------------
                // PASO 1: Insertar la cabecera en tb_master_control
                // Usamos RETURNING id para obtener el ID autogenerado
                // --------------------------------------------------------
                string sqlCabecera = @"
                    INSERT INTO tb_master_control (fecha_hora, dimension_n, coord_x, coord_y)
                    VALUES (@fecha, @n, @x, @y)
                    RETURNING id";

                using (NpgsqlCommand cmdCabecera = new NpgsqlCommand(sqlCabecera, conexion, transaccion))
                {
                    // Parametrización obligatoria (prohibido concatenar strings)
                    cmdCabecera.Parameters.AddWithValue("@fecha", DateTime.Now);
                    cmdCabecera.Parameters.AddWithValue("@n",     n);
                    cmdCabecera.Parameters.AddWithValue("@x",     filaInicio);
                    cmdCabecera.Parameters.AddWithValue("@y",     colInicio);

                    // ExecuteScalar devuelve el primer valor de la primera fila
                    // que en este caso es el ID recién generado por RETURNING id
                    object? resultado = cmdCabecera.ExecuteScalar();
                    idGenerado = Convert.ToInt32(resultado);
                }

                // --------------------------------------------------------
                // PASO 2: Insertar los movimientos con bucle WHILE
                // (Restricción del examen: NO se puede usar for ni foreach)
                // --------------------------------------------------------
                string sqlDetalle = @"
                    INSERT INTO tb_det_log (id_master, nro_paso, coord_x, coord_y)
                    VALUES (@idMaster, @paso, @x, @y)";

                int i = 0;  // índice manual del bucle (exigido por el examen)

                while (i < secuencia.Count)
                {
                    // --------------------------------------------------------
                    // REGLA DE OFUSCACIÓN (Parte D):
                    // Si el paso es PAR  → guardar paso * 2
                    // Si el paso es IMPAR → guardar como NEGATIVO (-paso)
                    // --------------------------------------------------------
                    int pasoReal = secuencia[i].paso;
                    int pasoOfuscado;

                    if (pasoReal % 2 == 0)
                        pasoOfuscado = pasoReal * 2;      // par: multiplicar por 2
                    else
                        pasoOfuscado = -pasoReal;         // impar: hacer negativo

                    using (NpgsqlCommand cmdDetalle = new NpgsqlCommand(sqlDetalle, conexion, transaccion))
                    {
                        cmdDetalle.Parameters.AddWithValue("@idMaster", idGenerado);
                        cmdDetalle.Parameters.AddWithValue("@paso",     pasoOfuscado); // valor ofuscado
                        cmdDetalle.Parameters.AddWithValue("@x",        secuencia[i].fila);
                        cmdDetalle.Parameters.AddWithValue("@y",        secuencia[i].col);

                        cmdDetalle.ExecuteNonQuery();
                    }

                    i++;  // avance manual del índice (exigido por el examen)
                }

                // Si llegamos hasta acá sin errores → confirmar todo
                transaccion.Commit();
                Console.WriteLine($"✓ Transacción confirmada. ID de simulación: {idGenerado}");
            }
            catch (Exception ex)
            {
                // Si algo falló → revertir todos los cambios
                transaccion.Rollback();
                Console.WriteLine($"✗ Error al guardar. Se revirtió la transacción: {ex.Message}");
                throw; // relanzar para que el Main lo maneje
            }
        }

        return idGenerado;
    }

    // ----------------------------------------------------------------
    // PARTE E: Lee los últimos 5 movimientos de una simulación
    // y aplica ingeniería inversa para reconstruir el paso real
    // ----------------------------------------------------------------
    public void MostrarUltimos5Pasos(int idSimulacion)
    {
        Console.WriteLine("\n--- Últimos 5 pasos del recorrido (reconstruidos) ---");

        string sqlLeer = @"
            SELECT id, nro_paso, coord_x, coord_y
            FROM tb_det_log
            WHERE id_master = @idMaster
            ORDER BY id DESC
            LIMIT 5";

        // "using" para liberar el lector y la conexión correctamente
        using (NpgsqlConnection conexion = new NpgsqlConnection(_connectionString))
        {
            conexion.Open();

            using (NpgsqlCommand cmd = new NpgsqlCommand(sqlLeer, conexion))
            {
                cmd.Parameters.AddWithValue("@idMaster", idSimulacion);

                // ExecuteReader para leer múltiples filas
                using (NpgsqlDataReader lector = cmd.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        int idFila       = lector.GetInt32(0);
                        int pasoOfuscado = lector.GetInt32(1);
                        int coordX       = lector.GetInt32(2);
                        int coordY       = lector.GetInt32(3);

                        // --------------------------------------------------------
                        // INGENIERÍA INVERSA (Parte E):
                        // Si el valor guardado es NEGATIVO → era IMPAR → recuperar cambiando signo
                        // Si el valor guardado es >= 0    → era PAR   → recuperar dividiendo por 2
                        // --------------------------------------------------------
                        int pasoReal;

                        if (pasoOfuscado < 0)
                            pasoReal = -pasoOfuscado;          // era impar: invertir signo
                        else
                            pasoReal = pasoOfuscado / 2;       // era par: dividir por 2

                        Console.WriteLine($"  ID registro: {idFila} | " +
                                          $"Paso REAL: {pasoReal} | " +
                                          $"Posición: ({coordX}, {coordY})");
                    }
                }
            }
        }
    }
}
