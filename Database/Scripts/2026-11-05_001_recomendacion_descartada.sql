/* ════════════════════════════════════════════════════════════════
   Recomendaciones: "No me interesa"
   Un usuario oculta un municipio de sus recomendaciones. El motor:
     - ya no se lo vuelve a mostrar (hasta que lo restaure desde su Perfil),
     - resta peso a las etiquetas de ese municipio en su perfil de gustos,
     - y ML.NET lo cuenta como una calificación implícita de 2★.
   Idempotente: se puede ejecutar varias veces sin error.
   ════════════════════════════════════════════════════════════════ */

IF OBJECT_ID(N'dbo.RecomendacionDescartada', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RecomendacionDescartada
    (
        idRecomendacionDescartada INT IDENTITY(1,1) NOT NULL,
        idUsuario                 INT NOT NULL,
        idMunicipio               INT NOT NULL,
        fechaRegistro             DATETIME2 NOT NULL
            CONSTRAINT DF_RecomendacionDescartada_fechaRegistro DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_RecomendacionDescartada PRIMARY KEY (idRecomendacionDescartada),

        -- Si se borra el usuario, se borran sus descartes
        CONSTRAINT FK_RecomendacionDescartada_Usuario
            FOREIGN KEY (idUsuario) REFERENCES dbo.Usuario (idUsuario) ON DELETE CASCADE,

        CONSTRAINT FK_RecomendacionDescartada_Municipio
            FOREIGN KEY (idMunicipio) REFERENCES dbo.Municipio (idMunicipio)
    );
END;
GO

-- Un usuario descarta cada municipio una sola vez
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UQ_RecomendacionDescartada_Usuario_Municipio'
                 AND object_id = OBJECT_ID(N'dbo.RecomendacionDescartada'))
BEGIN
    CREATE UNIQUE INDEX UQ_RecomendacionDescartada_Usuario_Municipio
        ON dbo.RecomendacionDescartada (idUsuario, idMunicipio);
END;
GO

-- Verificación
SELECT COUNT(*) AS Descartes FROM dbo.RecomendacionDescartada;

-- Diagnóstico para "Queda cerca": municipios activos sin coordenadas (no aparecerán en esas sugerencias)
SELECT idMunicipio, nombre
FROM dbo.Municipio
WHERE activo = 1 AND (latitud IS NULL OR longitud IS NULL OR latitud = 0 OR longitud = 0)
ORDER BY nombre;
