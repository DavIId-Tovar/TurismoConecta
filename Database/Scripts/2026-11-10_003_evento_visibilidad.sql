/* ════════════════════════════════════════════════════════════════
   Estadísticas de visibilidad y reportes municipales
   (tabla de roles del documento técnico: "ver estadísticas básicas
   de visibilidad" del comercio y "reportes municipales").

   Cada fila es una interacción de un visitante con un municipio o
   un establecimiento:
     VistaMunicipio   abrió la ficha del municipio
     VistaNegocio     abrió la ficha (modal) de un establecimiento
     ClicWhatsApp     tocó "Contactar por WhatsApp"
     ClicTelefono     tocó "Llamar directamente"
     ClicComoLlegar   tocó "Ver en Google Maps"

   Privacidad: no se guarda IP ni navegador. idVisitante es un
   identificador aleatorio del navegador (localStorage) que solo
   sirve para no contar dos veces la misma vista en 30 minutos.
   Favoritos, reseñas e itinerarios NO se duplican aquí: los
   reportes los leen de sus propias tablas.

   Idempotente: se puede ejecutar varias veces sin error.
   ════════════════════════════════════════════════════════════════ */

IF OBJECT_ID(N'dbo.EventoVisibilidad', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventoVisibilidad
    (
        idEventoVisibilidad BIGINT IDENTITY(1,1) NOT NULL,
        tipo                NVARCHAR(20) NOT NULL,
        idMunicipio         INT NOT NULL,          -- siempre: también en eventos de negocio (reporte municipal)
        idNegocio           INT NULL,              -- solo en eventos de un establecimiento
        idUsuario           INT NULL,              -- NULL = visitante sin sesión
        idVisitante         UNIQUEIDENTIFIER NULL, -- anti-duplicados, no identifica a la persona
        fecha               DATETIME2(0) NOT NULL
            CONSTRAINT DF_EventoVisibilidad_fecha DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_EventoVisibilidad PRIMARY KEY (idEventoVisibilidad),

        CONSTRAINT FK_EventoVisibilidad_Municipio
            FOREIGN KEY (idMunicipio) REFERENCES dbo.Municipio (idMunicipio),

        -- Si algún día se borra un negocio, sus estadísticas se van con él
        CONSTRAINT FK_EventoVisibilidad_Negocio
            FOREIGN KEY (idNegocio) REFERENCES dbo.Negocio (idNegocio) ON DELETE CASCADE,

        -- Si se borra un usuario, el evento queda como anónimo
        CONSTRAINT FK_EventoVisibilidad_Usuario
            FOREIGN KEY (idUsuario) REFERENCES dbo.Usuario (idUsuario) ON DELETE SET NULL,

        CONSTRAINT CK_EventoVisibilidad_tipo
            CHECK (tipo IN (N'VistaMunicipio', N'VistaNegocio', N'ClicWhatsApp', N'ClicTelefono', N'ClicComoLlegar')),

        -- Los eventos de establecimiento llevan negocio; la vista de municipio no
        CONSTRAINT CK_EventoVisibilidad_negocio
            CHECK ((tipo = N'VistaMunicipio' AND idNegocio IS NULL)
                OR (tipo <> N'VistaMunicipio' AND idNegocio IS NOT NULL))
    );
END;
GO

-- Panel del comercio: "eventos de mi negocio entre dos fechas"
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_EventoVisibilidad_Negocio_Fecha'
                 AND object_id = OBJECT_ID(N'dbo.EventoVisibilidad'))
BEGIN
    CREATE INDEX IX_EventoVisibilidad_Negocio_Fecha
        ON dbo.EventoVisibilidad (idNegocio, fecha)
        INCLUDE (tipo);
END;
GO

-- Reporte municipal y panel general: "eventos del municipio entre dos fechas"
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_EventoVisibilidad_Municipio_Fecha'
                 AND object_id = OBJECT_ID(N'dbo.EventoVisibilidad'))
BEGIN
    CREATE INDEX IX_EventoVisibilidad_Municipio_Fecha
        ON dbo.EventoVisibilidad (idMunicipio, fecha)
        INCLUDE (tipo, idNegocio);
END;
GO

-- Anti-duplicados: "¿este visitante ya registró este evento hace poco?"
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_EventoVisibilidad_Visitante_Fecha'
                 AND object_id = OBJECT_ID(N'dbo.EventoVisibilidad'))
BEGIN
    CREATE INDEX IX_EventoVisibilidad_Visitante_Fecha
        ON dbo.EventoVisibilidad (idVisitante, fecha)
        INCLUDE (tipo, idMunicipio, idNegocio);
END;
GO

-- Registro del script (solo si la tabla quedó creada)
IF OBJECT_ID(N'dbo.EventoVisibilidad', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM dbo.HistorialScripts WHERE nombreScript = N'2026-11-10_003_evento_visibilidad')
    INSERT INTO dbo.HistorialScripts (nombreScript) VALUES (N'2026-11-10_003_evento_visibilidad');
GO

-- Verificación
SELECT COUNT(*) AS Eventos FROM dbo.EventoVisibilidad;
SELECT name AS Indice FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.EventoVisibilidad') ORDER BY name;
SELECT nombreScript, fechaEjecucion FROM dbo.HistorialScripts ORDER BY nombreScript;
