/* ════════════════════════════════════════════════════════════════
   HistorialScripts: registro de los scripts ejecutados en esta BD.
   Cada script de Database/Scripts/ termina insertando su nombre aquí.
   Antes de ejecutar un script, consultar:
       SELECT * FROM dbo.HistorialScripts ORDER BY nombreScript;
   Idempotente: se puede ejecutar varias veces sin error.
   ════════════════════════════════════════════════════════════════ */

IF OBJECT_ID(N'dbo.HistorialScripts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HistorialScripts
    (
        nombreScript   NVARCHAR(200) NOT NULL
            CONSTRAINT PK_HistorialScripts PRIMARY KEY,
        fechaEjecucion DATETIME2 NOT NULL
            CONSTRAINT DF_HistorialScripts_fechaEjecucion DEFAULT (SYSUTCDATETIME())
    );
END;
GO

/* Registrar los scripts que YA se ejecutaron en Azure antes de que
   existiera esta tabla, y este mismo. */
INSERT INTO dbo.HistorialScripts (nombreScript)
SELECT v.nombre
FROM (VALUES
    (N'2026-11-05_001_recomendacion_descartada'),
    (N'2026-11-05_002_datos_recomendaciones'),
    (N'2026-11-09_001_historial_scripts')
) AS v(nombre)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.HistorialScripts h WHERE h.nombreScript = v.nombre
);
GO

-- Verificación
SELECT nombreScript, fechaEjecucion
FROM dbo.HistorialScripts
ORDER BY nombreScript;