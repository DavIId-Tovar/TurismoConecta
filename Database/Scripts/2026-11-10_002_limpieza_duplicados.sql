/* ════════════════════════════════════════════════════════════════
   Limpieza de datos repetidos + restricciones para que no vuelvan
   ────────────────────────────────────────────────────────────────
   1. MunicipioEtiqueta: enlaces municipio-etiqueta repetidos
      → se deja uno y se crea el índice único.
   2. NegocioEtiqueta: lo mismo (hoy no hay repetidos, solo el índice).
   3. Negocio: el mismo establecimiento registrado dos veces
      (mismo nombre, municipio y dueño). Se conserva uno:
        a) el de mejor estado: Aprobado > Pendiente > Rechazado,
        b) a igual estado, el que tiene más reseñas, fotos y etiquetas,
        c) a igualdad, el más antiguo (id menor).
      Antes de borrar el repetido se le pasan al que se conserva:
      fotos, etiquetas, favoritos, reseñas, alertas y los datos que
      le falten (teléfono, horario, coordenadas…).
   4. Favorito: filas que apuntan a un municipio Y a un negocio a la vez
      (el botón de favoritos nunca las encuentra) → se separan en dos;
      favoritos repetidos → se deja uno. Se agregan las restricciones.

   No toca usuarios, itinerarios ni notificaciones.
   Todo en una transacción: si algo falla, no cambia nada.
   Idempotente: se puede ejecutar varias veces sin error.
   ════════════════════════════════════════════════════════════════ */

SET NOCOUNT ON;
SET XACT_ABORT ON;   -- cualquier error deshace toda la transacción
-- Necesarios para crear índices filtrados (SSMS los trae activos; sqlcmd no)
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ─── 0. Vista previa: qué se va a limpiar ─────────────────────── */
PRINT N'── Enlaces municipio-etiqueta repetidos';
SELECT me.idMunicipio, m.nombre AS municipio, me.idEtiqueta, e.nombre AS etiqueta, COUNT(*) AS veces
FROM dbo.MunicipioEtiqueta me
JOIN dbo.Municipio m ON m.idMunicipio = me.idMunicipio
JOIN dbo.Etiqueta e ON e.idEtiqueta = me.idEtiqueta
GROUP BY me.idMunicipio, m.nombre, me.idEtiqueta, e.nombre
HAVING COUNT(*) > 1;

PRINT N'── Negocios repetidos (mismo nombre, municipio y dueño)';
SELECT LTRIM(RTRIM(n.nombre)) AS negocio, n.idMunicipio, n.idUsuario,
       STRING_AGG(CONCAT(n.idNegocio, N' (', n.estado, N')'), N', ') AS ids
FROM dbo.Negocio n
GROUP BY LTRIM(RTRIM(n.nombre)), n.idMunicipio, n.idUsuario
HAVING COUNT(*) > 1;

PRINT N'── Favoritos con municipio y negocio a la vez';
SELECT idFavorito, idUsuario, idMunicipio, idNegocio
FROM dbo.Favorito
WHERE idMunicipio IS NOT NULL AND idNegocio IS NOT NULL;
GO

BEGIN TRANSACTION;

/* ─── 1. MunicipioEtiqueta ─────────────────────────────────────── */
;WITH repetidos AS (
    SELECT idMunicipioEtiqueta,
           ROW_NUMBER() OVER (PARTITION BY idMunicipio, idEtiqueta ORDER BY idMunicipioEtiqueta) AS n
    FROM dbo.MunicipioEtiqueta
)
DELETE FROM repetidos WHERE n > 1;
PRINT CONCAT(N'1. Enlaces municipio-etiqueta repetidos borrados: ', @@ROWCOUNT);

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UQ_MunicipioEtiqueta_Municipio_Etiqueta'
                 AND object_id = OBJECT_ID(N'dbo.MunicipioEtiqueta'))
    CREATE UNIQUE INDEX UQ_MunicipioEtiqueta_Municipio_Etiqueta
        ON dbo.MunicipioEtiqueta (idMunicipio, idEtiqueta);

/* ─── 2. NegocioEtiqueta ───────────────────────────────────────── */
;WITH repetidos AS (
    SELECT idNegocioEtiqueta,
           ROW_NUMBER() OVER (PARTITION BY idNegocio, idEtiqueta ORDER BY idNegocioEtiqueta) AS n
    FROM dbo.NegocioEtiqueta
)
DELETE FROM repetidos WHERE n > 1;
PRINT CONCAT(N'2. Enlaces negocio-etiqueta repetidos borrados: ', @@ROWCOUNT);

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UQ_NegocioEtiqueta_Negocio_Etiqueta'
                 AND object_id = OBJECT_ID(N'dbo.NegocioEtiqueta'))
    CREATE UNIQUE INDEX UQ_NegocioEtiqueta_Negocio_Etiqueta
        ON dbo.NegocioEtiqueta (idNegocio, idEtiqueta);

/* ─── 3. Negocios repetidos ────────────────────────────────────── */
-- Mapa: cada negocio repetido → el que se conserva
CREATE TABLE #Mapa (idRepetido INT PRIMARY KEY, idConserva INT NOT NULL);

;WITH candidatos AS (
    SELECT n.idNegocio,
           LTRIM(RTRIM(n.nombre)) AS nombreLimpio,
           n.idMunicipio,
           ISNULL(n.idUsuario, -1) AS dueno,
           CASE n.estado WHEN N'Aprobado' THEN 0 WHEN N'Pendiente' THEN 1 ELSE 2 END AS pesoEstado,
             (SELECT COUNT(*) FROM dbo.[Reseña] r        WHERE r.idNegocio = n.idNegocio)
           + (SELECT COUNT(*) FROM dbo.GaleriaNegocio g  WHERE g.idNegocio = n.idNegocio)
           + (SELECT COUNT(*) FROM dbo.NegocioEtiqueta e WHERE e.idNegocio = n.idNegocio) AS contenido
    FROM dbo.Negocio n
),
ordenados AS (
    SELECT idNegocio,
           FIRST_VALUE(idNegocio) OVER (PARTITION BY nombreLimpio, idMunicipio, dueno
                                        ORDER BY pesoEstado, contenido DESC, idNegocio) AS idConserva
    FROM candidatos
)
INSERT INTO #Mapa (idRepetido, idConserva)
SELECT idNegocio, idConserva FROM ordenados WHERE idNegocio <> idConserva;

PRINT N'3. Negocios repetidos → el que se conserva:';
SELECT m.idRepetido, r.estado AS estadoRepetido, m.idConserva, c.estado AS estadoConserva, c.nombre
FROM #Mapa m
JOIN dbo.Negocio r ON r.idNegocio = m.idRepetido
JOIN dbo.Negocio c ON c.idNegocio = m.idConserva;

-- 3a. Datos que le falten al que se conserva, tomados del repetido
UPDATE c SET
    c.descripcion        = CASE WHEN LEN(ISNULL(r.descripcion, N'')) > LEN(ISNULL(c.descripcion, N'')) THEN r.descripcion ELSE c.descripcion END,
    c.direccion          = COALESCE(c.direccion, r.direccion),
    c.telefono           = COALESCE(c.telefono, r.telefono),
    c.email              = COALESCE(c.email, r.email),
    c.horario            = COALESCE(c.horario, r.horario),
    c.imagenPrincipalUrl = COALESCE(c.imagenPrincipalUrl, r.imagenPrincipalUrl),
    c.latitud            = COALESCE(c.latitud, r.latitud),
    c.longitud           = COALESCE(c.longitud, r.longitud)
FROM dbo.Negocio c
JOIN #Mapa m ON m.idConserva = c.idNegocio
JOIN dbo.Negocio r ON r.idNegocio = m.idRepetido;

-- 3b. Fotos: se pasan las que el otro no tenga; las iguales se borran
UPDATE g SET g.idNegocio = m.idConserva
FROM dbo.GaleriaNegocio g
JOIN #Mapa m ON m.idRepetido = g.idNegocio
WHERE NOT EXISTS (SELECT 1 FROM dbo.GaleriaNegocio x
                  WHERE x.idNegocio = m.idConserva AND x.imagenUrl = g.imagenUrl);
DELETE g FROM dbo.GaleriaNegocio g JOIN #Mapa m ON m.idRepetido = g.idNegocio;

-- 3c. Etiquetas
UPDATE e SET e.idNegocio = m.idConserva
FROM dbo.NegocioEtiqueta e
JOIN #Mapa m ON m.idRepetido = e.idNegocio
WHERE NOT EXISTS (SELECT 1 FROM dbo.NegocioEtiqueta x
                  WHERE x.idNegocio = m.idConserva AND x.idEtiqueta = e.idEtiqueta);
DELETE e FROM dbo.NegocioEtiqueta e JOIN #Mapa m ON m.idRepetido = e.idNegocio;

-- 3d. Favoritos (los que queden repetidos se limpian en el paso 4)
UPDATE f SET f.idNegocio = m.idConserva
FROM dbo.Favorito f
JOIN #Mapa m ON m.idRepetido = f.idNegocio;

-- 3e. Reseñas: una por usuario y lugar. Si el usuario ya opinó del que se conserva, se deja esa.
UPDATE r SET r.idNegocio = m.idConserva
FROM dbo.[Reseña] r
JOIN #Mapa m ON m.idRepetido = r.idNegocio
WHERE NOT EXISTS (SELECT 1 FROM dbo.[Reseña] x
                  WHERE x.idNegocio = m.idConserva AND x.idUsuario = r.idUsuario);
DELETE r FROM dbo.[Reseña] r JOIN #Mapa m ON m.idRepetido = r.idNegocio;
PRINT CONCAT(N'   Reseñas duplicadas del mismo usuario borradas: ', @@ROWCOUNT);

-- 3f. Alertas municipales
UPDATE a SET a.idNegocio = m.idConserva
FROM dbo.AlertaMunicipal a
JOIN #Mapa m ON m.idRepetido = a.idNegocio;

-- 3g. Ya sin nada que dependa de ellos, se borran los repetidos
DELETE n FROM dbo.Negocio n JOIN #Mapa m ON m.idRepetido = n.idNegocio;
PRINT CONCAT(N'   Negocios repetidos borrados: ', @@ROWCOUNT);

DROP TABLE #Mapa;

/* ─── 4. Favoritos ─────────────────────────────────────────────── */
-- 4a. Fila con municipio Y negocio → favorito del municipio (si no lo tenía) + favorito del negocio
INSERT INTO dbo.Favorito (idUsuario, idMunicipio, idNegocio, fechaGuardado)
SELECT DISTINCT f.idUsuario, f.idMunicipio, NULL, f.fechaGuardado
FROM dbo.Favorito f
WHERE f.idMunicipio IS NOT NULL AND f.idNegocio IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.Favorito x
                  WHERE x.idUsuario = f.idUsuario AND x.idMunicipio = f.idMunicipio AND x.idNegocio IS NULL);
PRINT CONCAT(N'4. Favoritos de municipio separados: ', @@ROWCOUNT);

UPDATE dbo.Favorito SET idMunicipio = NULL
WHERE idMunicipio IS NOT NULL AND idNegocio IS NOT NULL;

-- 4b. Favoritos repetidos (mismo usuario y mismo lugar): se deja el más antiguo
;WITH repetidos AS (
    SELECT idFavorito,
           ROW_NUMBER() OVER (PARTITION BY idUsuario, idMunicipio, idNegocio ORDER BY fechaGuardado, idFavorito) AS n
    FROM dbo.Favorito
)
DELETE FROM repetidos WHERE n > 1;
PRINT CONCAT(N'   Favoritos repetidos borrados: ', @@ROWCOUNT);

-- 4c. Restricciones: un favorito es de un municipio O de un negocio, y no se repite
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Favorito_UnSoloLugar')
    ALTER TABLE dbo.Favorito WITH CHECK ADD CONSTRAINT CK_Favorito_UnSoloLugar
        CHECK ((idMunicipio IS NULL AND idNegocio IS NOT NULL) OR (idMunicipio IS NOT NULL AND idNegocio IS NULL));

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Favorito_Usuario_Municipio' AND object_id = OBJECT_ID(N'dbo.Favorito'))
    CREATE UNIQUE INDEX UQ_Favorito_Usuario_Municipio
        ON dbo.Favorito (idUsuario, idMunicipio) WHERE idMunicipio IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UQ_Favorito_Usuario_Negocio' AND object_id = OBJECT_ID(N'dbo.Favorito'))
    CREATE UNIQUE INDEX UQ_Favorito_Usuario_Negocio
        ON dbo.Favorito (idUsuario, idNegocio) WHERE idNegocio IS NOT NULL;

/* ─── 5. Registro del script ───────────────────────────────────── */
IF NOT EXISTS (SELECT 1 FROM dbo.HistorialScripts WHERE nombreScript = N'2026-11-10_002_limpieza_duplicados')
    INSERT INTO dbo.HistorialScripts (nombreScript) VALUES (N'2026-11-10_002_limpieza_duplicados');

COMMIT TRANSACTION;
PRINT N'Listo: transacción confirmada.';
GO

/* ─── Verificación: todas deben dar 0 filas ────────────────────── */
SELECT N'MunicipioEtiqueta' AS tabla, COUNT(*) AS gruposRepetidos FROM (
    SELECT 1 AS x FROM dbo.MunicipioEtiqueta GROUP BY idMunicipio, idEtiqueta HAVING COUNT(*) > 1) t
UNION ALL
SELECT N'NegocioEtiqueta', COUNT(*) FROM (
    SELECT 1 AS x FROM dbo.NegocioEtiqueta GROUP BY idNegocio, idEtiqueta HAVING COUNT(*) > 1) t
UNION ALL
SELECT N'Negocio', COUNT(*) FROM (
    SELECT 1 AS x FROM dbo.Negocio GROUP BY LTRIM(RTRIM(nombre)), idMunicipio, idUsuario HAVING COUNT(*) > 1) t
UNION ALL
SELECT N'Favorito', COUNT(*) FROM (
    SELECT 1 AS x FROM dbo.Favorito GROUP BY idUsuario, idMunicipio, idNegocio HAVING COUNT(*) > 1) t;

SELECT nombreScript, fechaEjecucion FROM dbo.HistorialScripts ORDER BY nombreScript;
