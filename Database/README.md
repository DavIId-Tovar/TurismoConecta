# Base de datos — TurismoConecta (database-first, Azure SQL)

## Reglas
1. Ningún cambio de esquema se hace en Azure sin un script en `Scripts/`.
2. El modelo C# se genera desde la BD (EF Core Power Tools). Nunca usar Add-Migration / Update-Database.
3. Los archivos generados (Models/, Data/AppDbContext.cs) no se editan a mano: lo propio va en partials.
4. Se desarrolla contra la BD dev. Prod solo la toca el responsable, después del merge a main.
5. Cada script es idempotente y se registra en `dbo.HistorialScripts`.

## Nombres de los scripts
`AAAA-MM-DD_NNN_descripcion_corta.sql`, por ejemplo `2026-10-20_002_resena_unica_por_negocio.sql`.

## Carpetas
- `Baseline/`: foto del esquema y de los datos catálogo al 2026-10-16. No se ejecuta en prod.
- `Scripts/`: cambios posteriores, en orden.