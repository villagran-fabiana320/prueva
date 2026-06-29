// ============================================================
// PARTE B: Algoritmo de vuelo recursivo con backtracking
// Heurística de Warnsdorff (menor grado primero - OBLIGATORIO)
// ============================================================

namespace DronSimulator;

public class AlgoritmoDron
{
    // Los 8 movimientos posibles en patrón "L" (2 en una dirección + 1 perpendicular)
    // (deltaFila, deltaColumna)
    private static readonly int[,] Movimientos = new int[,]
    {
        { -2, -1 }, // 2 arriba + 1 izquierda
        { -2,  1 }, // 2 arriba + 1 derecha
        {  2, -1 }, // 2 abajo  + 1 izquierda
        {  2,  1 }, // 2 abajo  + 1 derecha
        { -1, -2 }, // 1 arriba + 2 izquierda
        { -1,  2 }, // 1 arriba + 2 derecha
        {  1, -2 }, // 1 abajo  + 2 izquierda
        {  1,  2 }  // 1 abajo  + 2 derecha
    };

    // Tablero donde se guarda el orden de visita (-1 = libre, 0..M = paso en que se pisó)
    private int[,] _tablero;

    // Tamaño del terreno (recibido dinámicamente, sin constantes)
    private int _n;

    // Cuántas parcelas son alcanzables desde el despegue (objetivo a cubrir)
    private int _totalAlcanzables;

    // ----------------------------------------------------------------
    // Método público principal: devuelve true si encontró recorrido
    // ----------------------------------------------------------------
    public bool Resolver(int n, int filaInicio, int colInicio)
    {
        _n = n;
        _tablero = new int[_n, _n];

        // Inicializar todo el tablero como libre (-1)
        for (int f = 0; f < _n; f++)
            for (int c = 0; c < _n; c++)
                _tablero[f, c] = -1;

        // Antes de la recursión: contar cuántas parcelas son alcanzables
        // (pueden existir parcelas que el patrón nunca puede alcanzar)
        _totalAlcanzables = ContarAlcanzables(filaInicio, colInicio);

        // Marcar la posición de despegue como paso 0
        _tablero[filaInicio, colInicio] = 0;

        // Lanzar la recursión desde el paso 0
        return Backtrack(filaInicio, colInicio, 0);
    }

    // ----------------------------------------------------------------
    // Método recursivo con backtracking y heurística de menor grado
    // ----------------------------------------------------------------
    private bool Backtrack(int fila, int col, int paso)
    {
        // Condición de éxito: ya pisamos todas las parcelas alcanzables
        // (paso 0 ya está marcado, entonces terminamos cuando llegamos al M-1)
        if (paso == _totalAlcanzables - 1)
            return true;

        // Obtener los candidatos válidos desde la posición actual
        // Un candidato es válido si: está dentro del terreno Y está libre (-1)
        List<(int grado, int fila, int col)> candidatos = ObtenerCandidatosOrdenados(fila, col);

        // Probar cada candidato en orden de MENOR GRADO primero (heurística obligatoria)
        foreach (var (_, nf, nc) in candidatos)
        {
            // Marcar la parcela con el número de paso
            _tablero[nf, nc] = paso + 1;

            // Llamada recursiva para seguir desde la nueva posición
            if (Backtrack(nf, nc, paso + 1))
                return true;

            // Si no funcionó → BACKTRACKING: desmarcar y probar el siguiente candidato
            _tablero[nf, nc] = -1;
        }

        // Agotamos todos los candidatos sin éxito → callejón sin salida
        return false;
    }

    // ----------------------------------------------------------------
    // Obtiene y ordena los candidatos por grado ascendente (menor primero)
    // Grado = cantidad de salidas libres que tiene el candidato
    // ----------------------------------------------------------------
    private List<(int grado, int fila, int col)> ObtenerCandidatosOrdenados(int fila, int col)
    {
        var candidatos = new List<(int grado, int fila, int col)>();

        for (int i = 0; i < 8; i++)
        {
            int nf = fila + Movimientos[i, 0];
            int nc = col  + Movimientos[i, 1];

            // Verificar que el destino esté dentro del terreno y libre
            if (EsValido(nf, nc) && _tablero[nf, nc] == -1)
            {
                // Calcular el grado de este candidato (sus propias salidas disponibles)
                int grado = CalcularGrado(nf, nc);
                candidatos.Add((grado, nf, nc));
            }
        }

        // Ordenar de MENOR a MAYOR grado (heurística de Warnsdorff)
        candidatos.Sort((a, b) => a.grado.CompareTo(b.grado));

        return candidatos;
    }

    // ----------------------------------------------------------------
    // Calcula el grado de una parcela: cuántos saltos libres tiene
    // (Esta es la métrica que usa la heurística de menor grado)
    // ----------------------------------------------------------------
    private int CalcularGrado(int fila, int col)
    {
        int grado = 0;

        for (int i = 0; i < 8; i++)
        {
            int nf = fila + Movimientos[i, 0];
            int nc = col  + Movimientos[i, 1];

            // Solo cuenta como salida si está dentro del terreno Y está libre
            if (EsValido(nf, nc) && _tablero[nf, nc] == -1)
                grado++;
        }

        return grado;
    }

    // ----------------------------------------------------------------
    // Cuenta cuántas parcelas son alcanzables desde el punto de despegue
    // usando una búsqueda en profundidad (DFS iterativo)
    // Esto determina el OBJETIVO real del dron (puede ser < N*N)
    // ----------------------------------------------------------------
    private int ContarAlcanzables(int filaInicio, int colInicio)
    {
        bool[,] visitado = new bool[_n, _n];
        visitado[filaInicio, colInicio] = true;
        int cantidad = 1;

        // Usamos una pila para el DFS iterativo
        var pila = new Stack<(int f, int c)>();
        pila.Push((filaInicio, colInicio));

        while (pila.Count > 0)
        {
            var (f, c) = pila.Pop();

            for (int i = 0; i < 8; i++)
            {
                int nf = f + Movimientos[i, 0];
                int nc = c + Movimientos[i, 1];

                if (EsValido(nf, nc) && !visitado[nf, nc])
                {
                    visitado[nf, nc] = true;
                    pila.Push((nf, nc));
                    cantidad++;
                }
            }
        }

        return cantidad;
    }

    // ----------------------------------------------------------------
    // Verifica si una posición está dentro del terreno NxN
    // ----------------------------------------------------------------
    private bool EsValido(int fila, int col)
    {
        return fila >= 0 && fila < _n && col >= 0 && col < _n;
    }

    // ----------------------------------------------------------------
    // Retorna el tablero resuelto (para mostrarlo en consola y guardar en BD)
    // ----------------------------------------------------------------
    public int[,] ObtenerTablero() => _tablero;

    // ----------------------------------------------------------------
    // Retorna la secuencia ordenada de movimientos para guardar en BD
    // Devuelve una lista de (paso, fila, col) ordenada por número de paso
    // ----------------------------------------------------------------
    public List<(int paso, int fila, int col)> ObtenerSecuencia()
    {
        var secuencia = new List<(int paso, int fila, int col)>();

        for (int f = 0; f < _n; f++)
        {
            for (int c = 0; c < _n; c++)
            {
                // Solo incluimos las parcelas que el dron pisó (>= 0)
                if (_tablero[f, c] >= 0)
                    secuencia.Add((_tablero[f, c], f, c));
            }
        }

        // Ordenar por número de paso para insertar en orden correcto
        secuencia.Sort((a, b) => a.paso.CompareTo(b.paso));

        return secuencia;
    }

    // ----------------------------------------------------------------
    // Retorna cuántas parcelas son alcanzables (para mostrar en pantalla)
    // ----------------------------------------------------------------
    public int TotalAlcanzables => _totalAlcanzables;
}
