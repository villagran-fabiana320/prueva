# Simulador de Trayectoria de Dron - Examen Parcial

## Estructura del proyecto

```
DronSimulator/
├── Program.cs          → Parte E: flujo principal, validación, consola
├── AlgoritmoDron.cs    → Parte B: algoritmo recursivo + backtracking + heurística
├── RepositorioDron.cs  → Parte D: persistencia ADO.NET con ofuscación
├── appsettings.json    → Parte C: configuración de conexión (NO hardcodeada)
├── DronSimulator.csproj
└── scripts/
    └── crear_tablas.sql → Parte A: DDL para PostgreSQL
```

## Setup rápido

### 1. Crear la base de datos en PostgreSQL
```sql
CREATE DATABASE dron_db;
```
Luego ejecutar el script `scripts/crear_tablas.sql`

### 2. Editar appsettings.json
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=dron_db;Username=postgres;Password=TU_CLAVE"
  }
}
```

### 3. Restaurar y ejecutar
```bash
dotnet restore
dotnet run
```

## Casos de prueba esperados

| N | Despegue | Resultado |
|---|----------|-----------|
| 1 | (0,0) | ÉXITO: 1/1 parcelas |
| 2 | (0,0) | ÉXITO: 1/4 parcelas |
| 3 | (0,0) | ÉXITO: 8/9 parcelas |
| 4 | (0,0) | SIN SOLUCIÓN |
| 6 | (0,0) | ÉXITO: 36/36 parcelas |
| 7 | (5,3) | ÉXITO: 49/49 parcelas |
| 8 | (0,0) | ÉXITO: 64/64 parcelas |
