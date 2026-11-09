/* ════════════════════════════════════════════════════════════════
   Datos para Recomendaciones (no cambia el esquema)
   1. Coordenadas de Gámeza: sin ellas no aparece en "Queda cerca".
   2. Desactiva (NO borra) los datos de prueba que se ven en la app:
      municipios "Prueba" / "Prueba 1" y la etiqueta "Team Soccer".
      Para revertir: el mismo UPDATE con activo = 1.
   Idempotente: se puede ejecutar varias veces.
   ════════════════════════════════════════════════════════════════ */

-- Revisión ANTES de cambiar nada
SELECT idMunicipio, nombre, activo, latitud, longitud
FROM dbo.Municipio
WHERE nombre IN (N'Gámeza', N'Prueba', N'Prueba 1');

SELECT idEtiqueta, nombre, activo
FROM dbo.Etiqueta
WHERE nombre = N'Team Soccer';
GO

-- 1. Gámeza (coordenadas aproximadas del casco urbano)
UPDATE dbo.Municipio
SET latitud = 5.802800, longitud = -72.805600
WHERE nombre = N'Gámeza'
  AND (latitud IS NULL OR longitud IS NULL OR latitud = 0 OR longitud = 0);

-- 2. Datos de prueba fuera de la vista pública
UPDATE dbo.Municipio
SET activo = 0
WHERE nombre IN (N'Prueba', N'Prueba 1') AND activo = 1;

UPDATE dbo.Etiqueta
SET activo = 0
WHERE nombre = N'Team Soccer' AND activo = 1;
GO

-- Verificación DESPUÉS
SELECT idMunicipio, nombre, activo, latitud, longitud
FROM dbo.Municipio
WHERE nombre IN (N'Gámeza', N'Prueba', N'Prueba 1');

SELECT idEtiqueta, nombre, activo
FROM dbo.Etiqueta
WHERE nombre = N'Team Soccer';
