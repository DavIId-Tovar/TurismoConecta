/* ════════════════════════════════════════════════════════════════
   Verificación de correo (Módulo 1 del documento técnico)

   Desde ahora toda cuenta nueva nace con emailConfirmado = 0 y recibe
   un enlace para confirmarlo; sin confirmarlo no puede publicar reseñas.

   Las cuentas que ya existían se crearon cuando no había verificación:
   se marcan como confirmadas UNA sola vez para no bloquearlas.

   No cambia el esquema: el enlace es un token firmado (no se guarda).

   Idempotente: la actualización solo corre la primera vez (queda en
   HistorialScripts). Volver a ejecutarlo NO confirma cuentas nuevas.
   ════════════════════════════════════════════════════════════════ */

SET XACT_ABORT ON;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HistorialScripts
               WHERE nombreScript = N'2026-11-10_004_verificacion_correo')
BEGIN
    BEGIN TRANSACTION;

    UPDATE dbo.Usuario
       SET emailConfirmado = 1
     WHERE emailConfirmado = 0;

    PRINT CONCAT('Cuentas existentes marcadas como confirmadas: ', @@ROWCOUNT);

    INSERT INTO dbo.HistorialScripts (nombreScript) VALUES (N'2026-11-10_004_verificacion_correo');

    COMMIT TRANSACTION;
END
ELSE
    PRINT 'El script ya se había ejecutado: no se cambió ninguna cuenta.';
GO

-- Verificación
SELECT emailConfirmado, COUNT(*) AS Usuarios FROM dbo.Usuario GROUP BY emailConfirmado;
SELECT nombreScript, fechaEjecucion FROM dbo.HistorialScripts ORDER BY nombreScript;
