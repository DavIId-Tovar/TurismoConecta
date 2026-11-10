/* ════════════════════════════════════════════════════════════════
   SEMILLA DE DESARROLLO: datos de prueba para la BD LOCAL
   ────────────────────────────────────────────────────────────────
   Agrega, sobre una BD creada con Baseline/ + Scripts/:
     - 5 municipios reales de Boyacá con coordenadas y etiquetas:
       Monguí, Aquitania, Chiquinquirá, Nobsa y El Cocuy
     - 10 usuarios FICTICIOS (5 turistas, 3 comercios, 1 admin municipal
       de Villa de Leyva y 1 admin general)
     - 10 negocios (8 aprobados, 1 pendiente en Villa de Leyva, 1 rechazado)
     - reseñas de municipios y negocios (una retenida por moderación y
       varias con respuesta del dueño), favoritos, gustos e itinerarios

   Usuarios de prueba (todos con la misma contraseña):  Prueba2026
     laura.gomez@turismoconecta.test       Turista
     andres.ruiz@turismoconecta.test       Turista
     camila.perez@turismoconecta.test      Turista
     diego.torres@turismoconecta.test      Turista
     valentina.rojas@turismoconecta.test   Turista
     comercio1@turismoconecta.test         AdminEstablecimiento
     comercio2@turismoconecta.test         AdminEstablecimiento
     comercio3@turismoconecta.test         AdminEstablecimiento
     admin.leyva@turismoconecta.test       AdminMunicipio (Villa de Leyva)
     admin@turismoconecta.test             AdminGeneral

   ⚠ SOLO PARA DESARROLLO. Nunca ejecutar en Azure ni en producción:
     el script se detiene solo si la BD no termina en "_Dev".
   Idempotente: se puede ejecutar varias veces; no duplica nada.
   No se registra en HistorialScripts (no cambia el esquema).
   ════════════════════════════════════════════════════════════════ */

SET NOCOUNT ON;

IF DB_NAME() NOT LIKE N'%[_]Dev'
BEGIN
    RAISERROR(N'Semilla cancelada: solo se ejecuta en una BD de desarrollo (nombre terminado en _Dev). BD actual: %s', 16, 1, N'(ver desplegable)');
    SET NOEXEC ON;   -- lo que sigue NO se ejecuta
END;
GO

/* ─── 1. Municipios reales ─────────────────────────────────────── */
DECLARE @idDepartamento INT = (SELECT TOP 1 idDepartamento FROM dbo.Departamento ORDER BY idDepartamento);

INSERT INTO dbo.Municipio (idDepartamento, nombre, descripcion, historia, clima, imagenUrl, latitud, longitud, activo)
SELECT @idDepartamento, v.nombre, v.descripcion, v.historia, v.clima, NULL, v.latitud, v.longitud, 1
FROM (VALUES
    (N'Monguí',
     N'Pueblo de calles empedradas y casas blancas con zócalos verdes, puerta de entrada al Páramo de Ocetá. Famoso por la fabricación artesanal de balones.',
     N'Fundado en 1601. Su Basílica de Nuestra Señora de Monguí y el Puente de Calicanto son testigos de la arquitectura colonial. Desde mediados del siglo XX sus familias cosen a mano balones de cuero.',
     N'Frío de montaña, alrededor de 13 °C', 5.723300, -72.849700),
    (N'Aquitania',
     N'Municipio a orillas del Lago de Tota, el lago natural más grande de Colombia. Tierra de cebolla larga, trucha arcoíris y la Playa Blanca.',
     N'Fundado en 1777. Su economía gira alrededor del cultivo de cebolla junca y la pesca de trucha en el lago.',
     N'Frío, entre 8 y 14 °C', 5.518600, -72.883600),
    (N'Chiquinquirá',
     N'Capital religiosa de Colombia, hogar de la Basílica de la Virgen del Rosario. Reconocida también por sus instrumentos musicales y miniaturas artesanales.',
     N'Fundada en 1586. El lienzo de la Virgen de Chiquinquirá convirtió al pueblo en destino de peregrinación nacional.',
     N'Templado frío, alrededor de 14 °C', 5.617200, -73.819700),
    (N'Nobsa',
     N'Conocido por sus ruanas, cobijas y tejidos de lana virgen, además de su arquitectura colonial y la capilla doctrinera de Belencito.',
     N'Pueblo de origen muisca fundado en 1593. La tradición textil se mantiene en talleres familiares alrededor de la plaza.',
     N'Frío, alrededor de 15 °C', 5.769400, -72.939400),
    (N'El Cocuy',
     N'Puerta de entrada al Parque Nacional Natural El Cocuy, con nevados, lagunas y frailejones. Destino de montañismo y caminatas de alta montaña.',
     N'Fundado en 1541. Su casco urbano de casas blancas y verdes es Bien de Interés Cultural.',
     N'Frío de alta montaña, alrededor de 11 °C', 6.407800, -72.444700)
) AS v(nombre, descripcion, historia, clima, latitud, longitud)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Municipio m WHERE m.nombre = v.nombre);

INSERT INTO dbo.MunicipioEtiqueta (idMunicipio, idEtiqueta)
SELECT m.idMunicipio, e.idEtiqueta
FROM (VALUES
    (N'Monguí',       N'Paisaje & Páramo'),
    (N'Monguí',       N'Historia & Colonial'),
    (N'Monguí',       N'Artesanías'),
    (N'Monguí',       N'Ecoturismo'),
    (N'Aquitania',    N'Paisaje & Páramo'),
    (N'Aquitania',    N'Ecoturismo'),
    (N'Aquitania',    N'Gastronomía'),
    (N'Aquitania',    N'Aventura'),
    (N'Chiquinquirá', N'Historia & Colonial'),
    (N'Chiquinquirá', N'Fiestas & Ferias'),
    (N'Chiquinquirá', N'Artesanías'),
    (N'Nobsa',        N'Artesanías'),
    (N'Nobsa',        N'Historia & Colonial'),
    (N'Nobsa',        N'Gastronomía'),
    (N'El Cocuy',     N'Paisaje & Páramo'),
    (N'El Cocuy',     N'Montañas'),
    (N'El Cocuy',     N'Aventura'),
    (N'El Cocuy',     N'Ecoturismo')
) AS v(municipio, etiqueta)
JOIN dbo.Municipio m ON m.nombre = v.municipio
JOIN dbo.Etiqueta  e ON e.nombre = v.etiqueta
WHERE NOT EXISTS (SELECT 1 FROM dbo.MunicipioEtiqueta me
                  WHERE me.idMunicipio = m.idMunicipio AND me.idEtiqueta = e.idEtiqueta);
GO

/* ─── 2. Usuarios ficticios ────────────────────────────────────── */
-- Hash de "Prueba2026" en el formato de ASP.NET Core PasswordHasher (V3, PBKDF2-HMACSHA512, 100 000 iteraciones)
DECLARE @hash NVARCHAR(300) = N'AQAAAAIAAYagAAAAECo2Jaxr6Rou7TSgwNR+RS4QXWRBLV006D9rjFoe1s7NCbUMo+xNXLp9JQBqA8Kv7g==';

INSERT INTO dbo.Usuario (idRol, nombre, apellido, email, passwordHash, telefono, municipioAsignadoId, fechaRegistro, emailConfirmado, activo)
SELECT r.idRol, v.nombre, v.apellido, v.email, @hash, v.telefono, m.idMunicipio,
       DATEADD(DAY, -v.diasAtras, SYSDATETIME()), 1, 1
FROM (VALUES
    (N'Laura',     N'Gómez',    N'laura.gomez@turismoconecta.test',     N'Usuario',              NULL,             N'3000000001', 120),
    (N'Andrés',    N'Ruiz',     N'andres.ruiz@turismoconecta.test',     N'Usuario',              NULL,             N'3000000002', 110),
    (N'Camila',    N'Pérez',    N'camila.perez@turismoconecta.test',    N'Usuario',              NULL,             N'3000000003',  95),
    (N'Diego',     N'Torres',   N'diego.torres@turismoconecta.test',    N'Usuario',              NULL,             N'3000000004',  80),
    (N'Valentina', N'Rojas',    N'valentina.rojas@turismoconecta.test', N'Usuario',              NULL,             N'3000000005',  60),
    (N'Marta',     N'Suárez',   N'comercio1@turismoconecta.test',       N'AdminEstablecimiento', NULL,             N'3000000011', 150),
    (N'Jorge',     N'Cárdenas', N'comercio2@turismoconecta.test',       N'AdminEstablecimiento', NULL,             N'3000000012', 140),
    (N'Rosa',      N'Niño',     N'comercio3@turismoconecta.test',       N'AdminEstablecimiento', NULL,             N'3000000013', 130),
    (N'Hernán',    N'Becerra',  N'admin.leyva@turismoconecta.test',     N'AdminMunicipio',       N'Villa de Leyva', N'3000000021', 160),
    (N'Admin',     N'Pruebas',  N'admin@turismoconecta.test',           N'AdminGeneral',         NULL,             N'3000000031', 170)
) AS v(nombre, apellido, email, rol, municipioAsignado, telefono, diasAtras)
JOIN dbo.Rol r ON r.nombre = v.rol
LEFT JOIN dbo.Municipio m ON m.nombre = v.municipioAsignado
WHERE NOT EXISTS (SELECT 1 FROM dbo.Usuario u WHERE u.email = v.email);
GO

/* ─── 3. Negocios ──────────────────────────────────────────────── */
INSERT INTO dbo.Negocio (idMunicipio, idCategoria, idUsuario, nombre, descripcion, direccion, telefono, email, horario,
                         imagenPrincipalUrl, latitud, longitud, estado, fechaRegistro, fechaAprobacion)
SELECT m.idMunicipio, c.idCategoria, u.idUsuario, v.nombre, v.descripcion, v.direccion, v.telefono, v.correo, v.horario,
       NULL, v.latitud, v.longitud, v.estado,
       DATEADD(DAY, -v.diasAtras, SYSDATETIME()),
       CASE WHEN v.estado = N'Aprobado' THEN DATEADD(DAY, -v.diasAtras + 2, SYSDATETIME()) END
FROM (VALUES
    (N'Posada Los Alisos',        N'Villa de Leyva', N'Hospedaje',   N'comercio1@turismoconecta.test',
     N'Casa colonial con patio de alisos, a dos cuadras de la Plaza Mayor. Desayuno boyacense incluido.',
     N'Calle 12 # 8-40', N'3101000001', N'posada.alisos@turismoconecta.test', N'Recepción 24 horas', 5.634500, -73.524500, N'Aprobado', 100),
    (N'Fogón de la Plaza',        N'Villa de Leyva', N'Gastronomía', N'comercio1@turismoconecta.test',
     N'Cocina típica en horno de leña: cuchuco con espinazo, mazamorra chiquita y arepas boyacenses.',
     N'Carrera 9 # 13-05', N'3101000002', N'fogon.plaza@turismoconecta.test', N'Martes a domingo, 11:00 a 21:00', 5.633800, -73.526100, N'Aprobado', 95),
    (N'Café del Claustro',        N'Villa de Leyva', N'Gastronomía', N'comercio1@turismoconecta.test',
     N'Café de especialidad y postres caseros en un antiguo claustro. Recién registrado, pendiente de revisión.',
     N'Calle 13 # 9-12', N'3101000003', N'cafe.claustro@turismoconecta.test', N'Todos los días, 8:00 a 19:00', 5.633100, -73.525200, N'Pendiente', 3),
    (N'Taller de Barro Muisca',   N'Ráquira',        N'Artesanías',  N'comercio2@turismoconecta.test',
     N'Taller familiar de alfarería: ollas, vasijas y figuras en barro. Clases cortas para visitantes.',
     N'Carrera 3 # 4-21', N'3102000001', N'barro.muisca@turismoconecta.test', N'Lunes a sábado, 9:00 a 17:00', 5.538700, -73.631900, N'Aprobado', 90),
    (N'Sabores del Lago',         N'Aquitania',      N'Gastronomía', N'comercio2@turismoconecta.test',
     N'Trucha arcoíris del Lago de Tota a la plancha, al ajillo o en salsa de cebolla junca.',
     N'Vía Playa Blanca km 2', N'3102000002', N'sabores.lago@turismoconecta.test', N'Todos los días, 10:00 a 18:00', 5.520400, -72.880900, N'Aprobado', 85),
    (N'Cabañas Orilla de Tota',   N'Aquitania',      N'Hospedaje',   N'comercio2@turismoconecta.test',
     N'Cabañas de madera frente al lago, con chimenea y kayak para huéspedes.',
     N'Vereda Hato Laguna', N'3102000003', N'orilla.tota@turismoconecta.test', N'Check-in desde las 15:00', 5.534800, -72.902300, N'Aprobado', 70),
    (N'Tejidos El Telar',         N'Nobsa',          N'Artesanías',  N'comercio3@turismoconecta.test',
     N'Ruanas, cobijas y bufandas tejidas en telar de pedal con lana virgen de la región.',
     N'Calle 4 # 6-18', N'3103000001', N'eltelar@turismoconecta.test', N'Lunes a domingo, 8:00 a 18:00', 5.770200, -72.938100, N'Aprobado', 75),
    (N'Refugio Nevado',           N'El Cocuy',       N'Hospedaje',   N'comercio3@turismoconecta.test',
     N'Hostal de montaña para caminantes del parque nacional. Guías locales y alquiler de equipo.',
     N'Carrera 4 # 7-30', N'3103000002', N'refugio.nevado@turismoconecta.test', N'Recepción 5:00 a 22:00', 6.408600, -72.443900, N'Aprobado', 65),
    (N'Balones y Cuero Monguí',   N'Monguí',         N'Artesanías',  N'comercio3@turismoconecta.test',
     N'Fábrica artesanal de balones cosidos a mano. Recorrido por el taller y venta directa.',
     N'Calle 5 # 3-10', N'3103000003', N'balones.mongui@turismoconecta.test', N'Lunes a sábado, 8:00 a 17:00', 5.724100, -72.848900, N'Aprobado', 60),
    (N'Hostal Aguas Termales',    N'Paipa',          N'Hospedaje',   N'comercio1@turismoconecta.test',
     N'Registro de prueba rechazado por información incompleta.',
     N'Sin dirección', N'3101000004', N'aguas.termales@turismoconecta.test', NULL, 5.776900, -73.112600, N'Rechazado', 20)
) AS v(nombre, municipio, categoria, dueno, descripcion, direccion, telefono, correo, horario, latitud, longitud, estado, diasAtras)
JOIN dbo.Municipio m ON m.nombre = v.municipio
JOIN dbo.Categoria c ON c.nombre = v.categoria
JOIN dbo.Usuario   u ON u.email  = v.dueno
WHERE NOT EXISTS (SELECT 1 FROM dbo.Negocio n WHERE n.nombre = v.nombre AND n.idMunicipio = m.idMunicipio);

INSERT INTO dbo.NegocioEtiqueta (idNegocio, idEtiqueta)
SELECT n.idNegocio, e.idEtiqueta
FROM (VALUES
    (N'Posada Los Alisos',      N'Historia & Colonial'),
    (N'Fogón de la Plaza',      N'Gastronomía'),
    (N'Taller de Barro Muisca', N'Artesanías'),
    (N'Sabores del Lago',       N'Gastronomía'),
    (N'Cabañas Orilla de Tota', N'Ecoturismo'),
    (N'Tejidos El Telar',       N'Artesanías'),
    (N'Refugio Nevado',         N'Aventura'),
    (N'Refugio Nevado',         N'Montañas'),
    (N'Balones y Cuero Monguí', N'Artesanías')
) AS v(negocio, etiqueta)
JOIN dbo.Negocio  n ON n.nombre = v.negocio
JOIN dbo.Etiqueta e ON e.nombre = v.etiqueta
WHERE NOT EXISTS (SELECT 1 FROM dbo.NegocioEtiqueta ne
                  WHERE ne.idNegocio = n.idNegocio AND ne.idEtiqueta = e.idEtiqueta);
GO

/* ─── 4. Reseñas de municipios ─────────────────────────────────── */
-- moderada = 1 → visible.  Cada turista tiene gustos distintos para que
-- el motor de recomendaciones (ML.NET) encuentre patrones.
INSERT INTO dbo.Reseña (idUsuario, idMunicipio, idNegocio, calificacion, comentario, moderada, fechaCreacion)
SELECT u.idUsuario, m.idMunicipio, NULL, v.calificacion, v.comentario, 1, DATEADD(DAY, -v.diasAtras, SYSDATETIME())
FROM (VALUES
    (N'laura.gomez@turismoconecta.test',     N'Villa de Leyva', 5, N'La Plaza Mayor al atardecer es otra cosa. Volvería sin pensarlo.', 90),
    (N'laura.gomez@turismoconecta.test',     N'Ráquira',        5, N'Colores por todas partes y artesanos muy amables.', 88),
    (N'laura.gomez@turismoconecta.test',     N'Monguí',         4, N'El Puente de Calicanto y las calles empedradas son preciosos.', 60),
    (N'laura.gomez@turismoconecta.test',     N'Nobsa',          4, N'Compré una ruana tejida a mano, excelente calidad.', 58),
    (N'laura.gomez@turismoconecta.test',     N'Chiquinquirá',   4, N'La basílica es impresionante.', 40),
    (N'andres.ruiz@turismoconecta.test',     N'El Cocuy',       5, N'Los nevados y las lagunas valen cada paso de la caminata.', 70),
    (N'andres.ruiz@turismoconecta.test',     N'Aquitania',      5, N'Amanecer en el Lago de Tota, increíble.', 66),
    (N'andres.ruiz@turismoconecta.test',     N'Monguí',         5, N'Subida al Páramo de Ocetá: frailejones hasta donde alcanza la vista.', 64),
    (N'andres.ruiz@turismoconecta.test',     N'Sogamoso',       4, N'Buena base para moverse por los pueblos del valle.', 50),
    (N'andres.ruiz@turismoconecta.test',     N'Gámeza',         4, N'Tranquilo y con buenos paisajes de montaña.', 45),
    (N'camila.perez@turismoconecta.test',    N'Paipa',          5, N'Termales y almojábanas, combinación perfecta.', 55),
    (N'camila.perez@turismoconecta.test',    N'Tunja',          4, N'Mucha historia en el centro y buena comida.', 52),
    (N'camila.perez@turismoconecta.test',    N'Villa de Leyva', 4, N'Muy lindo, aunque en temporada alta hay mucha gente.', 48),
    (N'camila.perez@turismoconecta.test',    N'Aquitania',      4, N'La trucha del lago es lo mejor del viaje.', 30),
    (N'diego.torres@turismoconecta.test',    N'Villa de Leyva', 3, N'Bonito pero caro en fin de semana.', 42),
    (N'diego.torres@turismoconecta.test',    N'El Cocuy',       4, N'Hace mucho frío, pero el paisaje lo compensa.', 35),
    (N'diego.torres@turismoconecta.test',    N'Paipa',          4, N'Buen plan de descanso.', 28),
    (N'diego.torres@turismoconecta.test',    N'Chiquinquirá',   3, N'Interesante para un día.', 20),
    (N'valentina.rojas@turismoconecta.test', N'Ráquira',        5, N'El paraíso de las artesanías.', 25),
    (N'valentina.rojas@turismoconecta.test', N'Nobsa',          5, N'Los tejidos son hermosos y los precios justos.', 22),
    (N'valentina.rojas@turismoconecta.test', N'Chiquinquirá',   4, N'Me encantaron las miniaturas y los tiples.', 18),
    (N'valentina.rojas@turismoconecta.test', N'Tunja',          3, N'Bien, aunque me faltó tiempo.', 12)
) AS v(email, municipio, calificacion, comentario, diasAtras)
JOIN dbo.Usuario   u ON u.email  = v.email
JOIN dbo.Municipio m ON m.nombre = v.municipio
WHERE NOT EXISTS (SELECT 1 FROM dbo.Reseña r
                  WHERE r.idUsuario = u.idUsuario AND r.idMunicipio = m.idMunicipio AND r.idNegocio IS NULL);
GO

/* ─── 5. Reseñas de negocios (solo aprobados; nunca del propio dueño) ─ */
INSERT INTO dbo.Reseña (idUsuario, idMunicipio, idNegocio, calificacion, comentario, respuesta, fechaRespuesta, moderada, fechaCreacion)
SELECT u.idUsuario, NULL, n.idNegocio, v.calificacion, v.comentario, v.respuesta,
       CASE WHEN v.respuesta IS NOT NULL THEN DATEADD(DAY, -v.diasAtras + 1, SYSDATETIME()) END,
       v.moderada, DATEADD(DAY, -v.diasAtras, SYSDATETIME())
FROM (VALUES
    (N'laura.gomez@turismoconecta.test',     N'Posada Los Alisos',      5, N'Habitación impecable y el desayuno delicioso.',
     N'¡Gracias, Laura! Te esperamos de nuevo.', 1, 89),
    (N'camila.perez@turismoconecta.test',    N'Fogón de la Plaza',      5, N'El cuchuco con espinazo es de los mejores que he probado.',
     NULL, 1, 47),
    (N'diego.torres@turismoconecta.test',    N'Fogón de la Plaza',      2, N'Visiten mi página de ofertas, gana dinero fácil desde casa.',
     NULL, 0, 41),   -- ← retenida: aparece en el panel de moderación
    (N'laura.gomez@turismoconecta.test',     N'Taller de Barro Muisca', 5, N'La clase de alfarería fue lo mejor del viaje.',
     N'Nos alegra mucho. ¡Vuelvan pronto!', 1, 87),
    (N'valentina.rojas@turismoconecta.test', N'Taller de Barro Muisca', 4, N'Piezas muy bonitas, algo difícil de encontrar el lugar.',
     NULL, 1, 24),
    (N'andres.ruiz@turismoconecta.test',     N'Sabores del Lago',       5, N'Trucha fresca y porciones generosas.',
     NULL, 1, 65),
    (N'camila.perez@turismoconecta.test',    N'Sabores del Lago',       4, N'Muy rica la salsa de cebolla junca.',
     N'Gracias, la cebolla es de nuestra propia huerta.', 1, 29),
    (N'andres.ruiz@turismoconecta.test',     N'Cabañas Orilla de Tota', 4, N'La vista al lago es increíble; el agua caliente tarda un poco.',
     NULL, 1, 66),
    (N'valentina.rojas@turismoconecta.test', N'Tejidos El Telar',       5, N'La ruana que compré es suavísima.',
     NULL, 1, 21),
    (N'andres.ruiz@turismoconecta.test',     N'Refugio Nevado',         5, N'Los guías conocen el parque como nadie.',
     NULL, 1, 69),
    (N'diego.torres@turismoconecta.test',    N'Refugio Nevado',         4, N'Cómodo y caliente después de la caminata.',
     NULL, 1, 34),
    (N'laura.gomez@turismoconecta.test',     N'Balones y Cuero Monguí', 4, N'Ver cómo cosen los balones a mano es fascinante.',
     NULL, 1, 59)
) AS v(email, negocio, calificacion, comentario, respuesta, moderada, diasAtras)
JOIN dbo.Usuario u ON u.email  = v.email
JOIN dbo.Negocio n ON n.nombre = v.negocio AND n.estado = N'Aprobado'
WHERE n.idUsuario <> u.idUsuario
  AND NOT EXISTS (SELECT 1 FROM dbo.Reseña r
                  WHERE r.idUsuario = u.idUsuario AND r.idNegocio = n.idNegocio AND r.idMunicipio IS NULL);
GO

/* ─── 6. Favoritos (municipios y negocios) ─────────────────────── */
INSERT INTO dbo.Favorito (idUsuario, idMunicipio, idNegocio, fechaGuardado)
SELECT u.idUsuario, m.idMunicipio, n.idNegocio, DATEADD(DAY, -v.diasAtras, SYSDATETIME())
FROM (VALUES
    (N'laura.gomez@turismoconecta.test',     N'Villa de Leyva', NULL,                     85),
    (N'laura.gomez@turismoconecta.test',     N'Ráquira',        NULL,                     84),
    (N'laura.gomez@turismoconecta.test',     NULL,              N'Posada Los Alisos',      83),
    (N'andres.ruiz@turismoconecta.test',     N'El Cocuy',       NULL,                     68),
    (N'andres.ruiz@turismoconecta.test',     N'Monguí',         NULL,                     63),
    (N'andres.ruiz@turismoconecta.test',     NULL,              N'Refugio Nevado',         67),
    (N'camila.perez@turismoconecta.test',    N'Paipa',          NULL,                     54),
    (N'camila.perez@turismoconecta.test',    N'Aquitania',      NULL,                     31),
    (N'diego.torres@turismoconecta.test',    N'El Cocuy',       NULL,                     33),
    (N'valentina.rojas@turismoconecta.test', N'Nobsa',          NULL,                     20),
    (N'valentina.rojas@turismoconecta.test', N'Chiquinquirá',   NULL,                     17),
    (N'valentina.rojas@turismoconecta.test', NULL,              N'Tejidos El Telar',       19)
) AS v(email, municipio, negocio, diasAtras)
JOIN dbo.Usuario u ON u.email = v.email
LEFT JOIN dbo.Municipio m ON m.nombre = v.municipio
LEFT JOIN dbo.Negocio   n ON n.nombre = v.negocio
WHERE (m.idMunicipio IS NOT NULL OR n.idNegocio IS NOT NULL)
  AND NOT EXISTS (SELECT 1 FROM dbo.Favorito f
                  WHERE f.idUsuario = u.idUsuario
                    AND ISNULL(f.idMunicipio, 0) = ISNULL(m.idMunicipio, 0)
                    AND ISNULL(f.idNegocio, 0)   = ISNULL(n.idNegocio, 0));
GO

/* ─── 7. Gustos declarados ("Para ti") ─────────────────────────── */
INSERT INTO dbo.PreferenciaUsuario (idUsuario, idEtiqueta)
SELECT u.idUsuario, e.idEtiqueta
FROM (VALUES
    (N'andres.ruiz@turismoconecta.test',     N'Aventura'),
    (N'andres.ruiz@turismoconecta.test',     N'Paisaje & Páramo'),
    (N'andres.ruiz@turismoconecta.test',     N'Montañas'),
    (N'valentina.rojas@turismoconecta.test', N'Artesanías'),
    (N'valentina.rojas@turismoconecta.test', N'Historia & Colonial'),
    (N'camila.perez@turismoconecta.test',    N'Gastronomía'),
    (N'camila.perez@turismoconecta.test',    N'Fiestas & Ferias')
) AS v(email, etiqueta)
JOIN dbo.Usuario  u ON u.email  = v.email
JOIN dbo.Etiqueta e ON e.nombre = v.etiqueta
WHERE NOT EXISTS (SELECT 1 FROM dbo.PreferenciaUsuario p
                  WHERE p.idUsuario = u.idUsuario AND p.idEtiqueta = e.idEtiqueta);
GO

/* ─── 8. Itinerarios ───────────────────────────────────────────── */
-- Los km y minutos por tramo quedan en NULL: el API los completa al abrir el itinerario.
INSERT INTO dbo.Itinerario (idUsuario, nombre, fechaInicio, fechaFin, compartido, observaciones)
SELECT u.idUsuario, v.nombre, v.inicio, v.fin, 0, v.observaciones
FROM (VALUES
    (N'laura.gomez@turismoconecta.test', N'Ruta colonial y artesanal', CAST('2026-12-18' AS DATE), CAST('2026-12-20' AS DATE), N'Comprar artesanías en Ráquira'),
    (N'andres.ruiz@turismoconecta.test', N'Páramos del norte',         CAST('2027-01-08' AS DATE), CAST('2027-01-11' AS DATE), N'Llevar ropa térmica')
) AS v(email, nombre, inicio, fin, observaciones)
JOIN dbo.Usuario u ON u.email = v.email
WHERE NOT EXISTS (SELECT 1 FROM dbo.Itinerario i WHERE i.idUsuario = u.idUsuario AND i.nombre = v.nombre);

INSERT INTO dbo.ItinerarioDetalle (idItinerario, idMunicipio, diaNumero, orden, fechaVisita)
SELECT i.idItinerario, m.idMunicipio, v.dia, v.orden, DATEADD(DAY, v.dia - 1, i.fechaInicio)
FROM (VALUES
    (N'Ruta colonial y artesanal', N'Villa de Leyva', 1, 1),
    (N'Ruta colonial y artesanal', N'Ráquira',        2, 2),
    (N'Ruta colonial y artesanal', N'Chiquinquirá',   3, 3),
    (N'Páramos del norte',         N'Sogamoso',       1, 1),
    (N'Páramos del norte',         N'Monguí',         2, 2),
    (N'Páramos del norte',         N'Aquitania',      3, 3),
    (N'Páramos del norte',         N'El Cocuy',       4, 4)
) AS v(itinerario, municipio, dia, orden)
JOIN dbo.Itinerario i ON i.nombre = v.itinerario
JOIN dbo.Municipio  m ON m.nombre = v.municipio
WHERE NOT EXISTS (SELECT 1 FROM dbo.ItinerarioDetalle d
                  WHERE d.idItinerario = i.idItinerario AND d.idMunicipio = m.idMunicipio);
GO

/* ─── Resumen ──────────────────────────────────────────────────── */
SELECT N'Municipios activos' AS dato, COUNT(*) AS total FROM dbo.Municipio WHERE activo = 1
UNION ALL SELECT N'Usuarios de prueba',   COUNT(*) FROM dbo.Usuario WHERE email LIKE N'%@turismoconecta.test'
UNION ALL SELECT N'Negocios aprobados',   COUNT(*) FROM dbo.Negocio WHERE estado = N'Aprobado'
UNION ALL SELECT N'Negocios pendientes',  COUNT(*) FROM dbo.Negocio WHERE estado = N'Pendiente'
UNION ALL SELECT N'Reseñas visibles',     COUNT(*) FROM dbo.Reseña  WHERE moderada = 1
UNION ALL SELECT N'Reseñas retenidas',    COUNT(*) FROM dbo.Reseña  WHERE moderada = 0
UNION ALL SELECT N'Favoritos',            COUNT(*) FROM dbo.Favorito
UNION ALL SELECT N'Itinerarios',          COUNT(*) FROM dbo.Itinerario;
GO

SET NOEXEC OFF;
GO
