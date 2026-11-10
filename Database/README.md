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
- `Baseline/`: foto del esquema y de los datos de catálogo al 2026-11-08, tomada de Azure. No se ejecuta en prod.
- `Scripts/`: cambios posteriores, en orden.
- `Semillas/`: datos de prueba ficticios, SOLO para la BD local (`*_Dev`). Nunca en Azure.
- `Local/`: archivos personales de cada desarrollador (por ejemplo, usuarios copiados de Azure). Ignorada por git.


## Levantar la BD desde cero (desarrollo local)
1. En SQL Server local:
   `CREATE DATABASE TurismoConectaDB_Dev COLLATE Modern_Spanish_CI_AS;`
2. Ejecutar sobre esa BD, en este orden (en SSMS, desplegable de BD = `TurismoConectaDB_Dev`):
   1. `Baseline/2026-11-08_000_esquema_base.sql`
   2. `Baseline/2026-11-08_000_datos_catalogo.sql`
   3. Todos los archivos de `Scripts/`, en orden de nombre.
3. Comprobar: `SELECT * FROM dbo.HistorialScripts ORDER BY nombreScript;`
4. Apuntar el API a la BD local (user-secrets, nunca en appsettings):
   `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=TurismoConectaDB_Dev;Trusted_Connection=True;TrustServerCertificate=True;" --project TurismoConecta/TurismoConecta.api`
5. La BD local no trae usuarios. Opcional: ejecutar `Semillas/2026-11-09_semilla_desarrollo.sql`
   para tener usuarios, negocios, reseñas, favoritos e itinerarios de prueba
   (la contraseña de los usuarios de prueba está en el encabezado del script).

## Cómo hacer un cambio de esquema
1. Escribir el script en `Scripts/` con el nombre `AAAA-MM-DD_NNN_descripcion.sql`.
2. El script debe ser idempotente (`IF NOT EXISTS` / `IF OBJECT_ID(...) IS NULL`).
3. Debe terminar registrándose:
   ```sql
   IF NOT EXISTS (SELECT 1 FROM dbo.HistorialScripts WHERE nombreScript = N'AAAA-MM-DD_NNN_descripcion')
       INSERT INTO dbo.HistorialScripts (nombreScript) VALUES (N'AAAA-MM-DD_NNN_descripcion');
   ```
4. Probarlo en la BD local.
5. PR a `main`. Después de fusionar, quien administra Azure lo ejecuta en Azure.
6. Nunca cambiar tablas directamente en Azure sin su script.