/* ════════════════════════════════════════════════════════════════
   Alertas municipales (Módulo 8 del documento técnico)
   Avisos que publica un municipio (o un comercio sobre su propio
   negocio) y que les llegan en vivo a los turistas afectados:
   quienes tienen el municipio en favoritos o un itinerario que pasa
   por él en esas fechas.
     tipo:      Evento | CambioHorario | CierreTemporal | Movilidad
     severidad: Informativa | Precaucion | Importante
   Una alerta deja de mostrarse sola cuando pasa su fechaFin,
   o antes si alguien la finaliza (activa = 0).
   Idempotente: se puede ejecutar varias veces sin error.
   ════════════════════════════════════════════════════════════════ */

IF OBJECT_ID(N'dbo.AlertaMunicipal', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AlertaMunicipal
    (
        idAlertaMunicipal INT IDENTITY(1,1) NOT NULL,
        idMunicipio       INT NOT NULL,
        idNegocio         INT NULL,             -- opcional: la alerta es sobre un establecimiento
        tipo              NVARCHAR(20)   NOT NULL,
        severidad         NVARCHAR(20)   NOT NULL,
        titulo            NVARCHAR(120)  NOT NULL,
        descripcion       NVARCHAR(1000) NOT NULL,
        fechaInicio       DATETIME2 NOT NULL,
        fechaFin          DATETIME2 NOT NULL,
        activa            BIT NOT NULL
            CONSTRAINT DF_AlertaMunicipal_activa DEFAULT (1),
        idUsuarioCreador  INT NOT NULL,
        fechaCreacion     DATETIME2 NOT NULL
            CONSTRAINT DF_AlertaMunicipal_fechaCreacion DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_AlertaMunicipal PRIMARY KEY (idAlertaMunicipal),

        CONSTRAINT FK_AlertaMunicipal_Municipio
            FOREIGN KEY (idMunicipio) REFERENCES dbo.Municipio (idMunicipio),

        -- Si se borra el negocio, sus alertas ya no tienen sentido
        CONSTRAINT FK_AlertaMunicipal_Negocio
            FOREIGN KEY (idNegocio) REFERENCES dbo.Negocio (idNegocio) ON DELETE CASCADE,

        CONSTRAINT FK_AlertaMunicipal_Usuario
            FOREIGN KEY (idUsuarioCreador) REFERENCES dbo.Usuario (idUsuario),

        CONSTRAINT CK_AlertaMunicipal_tipo
            CHECK (tipo IN (N'Evento', N'CambioHorario', N'CierreTemporal', N'Movilidad')),

        CONSTRAINT CK_AlertaMunicipal_severidad
            CHECK (severidad IN (N'Informativa', N'Precaucion', N'Importante')),

        CONSTRAINT CK_AlertaMunicipal_fechas
            CHECK (fechaFin > fechaInicio)
    );
END;
GO

-- Consulta principal: "alertas vigentes de este municipio"
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_AlertaMunicipal_Municipio_FechaFin'
                 AND object_id = OBJECT_ID(N'dbo.AlertaMunicipal'))
BEGIN
    CREATE INDEX IX_AlertaMunicipal_Municipio_FechaFin
        ON dbo.AlertaMunicipal (idMunicipio, fechaFin)
        INCLUDE (activa, fechaInicio);
END;
GO

-- Tipo de notificación para la campana
IF NOT EXISTS (SELECT 1 FROM dbo.TiposNotificacion WHERE codigo = N'ALERTA_MUNICIPAL')
    INSERT INTO dbo.TiposNotificacion (codigo, descripcion)
    VALUES (N'ALERTA_MUNICIPAL', N'Aviso de un municipio: evento, cierre, cambio de horario o movilidad');
GO

-- Registro del script
IF NOT EXISTS (SELECT 1 FROM dbo.HistorialScripts WHERE nombreScript = N'2026-11-10_001_alerta_municipal')
    INSERT INTO dbo.HistorialScripts (nombreScript) VALUES (N'2026-11-10_001_alerta_municipal');
GO

-- Verificación
SELECT COUNT(*) AS Alertas FROM dbo.AlertaMunicipal;
SELECT idTipoNotificacion, codigo FROM dbo.TiposNotificacion WHERE codigo = N'ALERTA_MUNICIPAL';
SELECT nombreScript, fechaEjecucion FROM dbo.HistorialScripts ORDER BY nombreScript;
