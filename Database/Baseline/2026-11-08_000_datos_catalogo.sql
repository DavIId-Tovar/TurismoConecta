/* ════════════════════════════════════════════════════════════════
   BASELINE: datos de catálogo de TurismoConecta al 2026-11-08
   Tablas: Departamento, Municipio, Etiqueta, MunicipioEtiqueta,
   FechaRelevante, MunicipioFechaRelevante, Categoria, SitiosTuristicos,
   Rol, TiposNotificacion.
   SIN datos personales: no incluye usuarios, negocios, reseñas,
   favoritos, itinerarios ni notificaciones.
   Uso: una sola vez, sobre una BD recién creada con esquema_base.sql.
   Las URLs con localhost se corrigen en DT02.
   ════════════════════════════════════════════════════════════════ */



SET IDENTITY_INSERT [dbo].[FechaRelevante] ON 

INSERT [dbo].[FechaRelevante] ([idFechaRelevante], [nombreFestividad], [fechaInicio], [fechaFin], [descripcion], [historia], [esRecurrente], [mesCelebracion], [tipoFestividad], [recomendaciones], [activo]) VALUES (1, N'Festival de Luces', CAST(N'2026-12-07' AS Date), CAST(N'2026-12-08' AS Date), N'Espectacular concurso y exhibición de fuegos artificiales que ilumina la Plaza Mayor.', N'Inició como una celebración local de la Noche de las Velitas para homenajear a la Inmaculada Concepción y evolucionó en un evento de pirotecnia de talla internacional.', 1, 12, N'Cultural', N'Reservar hospedaje con meses de anticipación y llegar temprano a la plaza.', 1)
INSERT [dbo].[FechaRelevante] ([idFechaRelevante], [nombreFestividad], [fechaInicio], [fechaFin], [descripcion], [historia], [esRecurrente], [mesCelebracion], [tipoFestividad], [recomendaciones], [activo]) VALUES (2, N'Festival Internacional del Viento y las Cometas', CAST(N'2026-08-15' AS Date), CAST(N'2026-08-17' AS Date), N'Evento donde el cielo se llena de cometas gigantes de múltiples colores aprovechando los vientos de agosto.', N'Nació en los años 70 aprovechando las óptimas condiciones de viento de la zona árida del municipio.', 1, 8, N'Deportiva', N'Llevar bloqueador solar, sombrero y cometa propia para participar.', 1)
INSERT [dbo].[FechaRelevante] ([idFechaRelevante], [nombreFestividad], [fechaInicio], [fechaFin], [descripcion], [historia], [esRecurrente], [mesCelebracion], [tipoFestividad], [recomendaciones], [activo]) VALUES (3, N'Festival de la Arcilla y las Artesanías', CAST(N'2026-10-12' AS Date), CAST(N'2026-10-14' AS Date), N'Feria artesanal y concurso de alfarería en vivo donde los artesanos moldean la cerámica boyacense.', N'Ráquira fue poblada por maestros alfareros muiscas; el festival rinde homenaje a esta tradición milenaria.', 1, 10, N'Artesanal', N'Llevar dinero en efectivo para comprar directamente a los artesanos.', 1)
INSERT [dbo].[FechaRelevante] ([idFechaRelevante], [nombreFestividad], [fechaInicio], [fechaFin], [descripcion], [historia], [esRecurrente], [mesCelebracion], [tipoFestividad], [recomendaciones], [activo]) VALUES (4, N'Festival Nacional de la Ruana y el Pañolón', CAST(N'2026-05-29' AS Date), CAST(N'2026-05-31' AS Date), N'Encuentro cultural con amansamiento de ovejas, desfile de modas artesanales y reinado del esquilado.', N'Creado para honrar la tradición textil de la ruana de lana de oveja, elemento icónico de la identidad boyacense.', 1, 5, N'Folclórico', N'Disfrutar del consumo tradicional de queso paipa y amasijos durante el evento.', 1)
INSERT [dbo].[FechaRelevante] ([idFechaRelevante], [nombreFestividad], [fechaInicio], [fechaFin], [descripcion], [historia], [esRecurrente], [mesCelebracion], [tipoFestividad], [recomendaciones], [activo]) VALUES (5, N'Aguinaldo Boyacense', CAST(N'2026-12-16' AS Date), CAST(N'2026-12-22' AS Date), N'La fiesta navideña más antigua de Colombia, con desfiles de carrozas, verbenas populares y conciertos.', N'Nació en 1955 por iniciativa del sargento de policía Carlos Julio Umaña como una fiesta comunitaria.', 1, 12, N'Cultural', N'Abrigarse muy bien en las noches y estar atento a los cierres viales.', 1)
INSERT [dbo].[FechaRelevante] ([idFechaRelevante], [nombreFestividad], [fechaInicio], [fechaFin], [descripcion], [historia], [esRecurrente], [mesCelebracion], [tipoFestividad], [recomendaciones], [activo]) VALUES (6, N'Fiesta del Sol y del Acero', CAST(N'2026-07-17' AS Date), CAST(N'2026-07-20' AS Date), N'Celebración patronal con eventos folclóricos, conciertos y desfiles en honor al legado del Templo del Sol muisca.', N'Combina el culto al Sol del pueblo Muisca (Sua) con el progreso industrial siderúrgico del siglo XX.', 1, 7, N'Patronal', N'Visitar el Museo Arqueológico Suamox durante los días de festividad.', 1)
INSERT [dbo].[FechaRelevante] ([idFechaRelevante], [nombreFestividad], [fechaInicio], [fechaFin], [descripcion], [historia], [esRecurrente], [mesCelebracion], [tipoFestividad], [recomendaciones], [activo]) VALUES (7, N'11 de Julio - Combate de Gámeza (Ruta Libertadora', CAST(N'2026-09-08' AS Date), CAST(N'2026-09-08' AS Date), NULL, NULL, 1, NULL, NULL, NULL, 1)
INSERT [dbo].[FechaRelevante] ([idFechaRelevante], [nombreFestividad], [fechaInicio], [fechaFin], [descripcion], [historia], [esRecurrente], [mesCelebracion], [tipoFestividad], [recomendaciones], [activo]) VALUES (8, N'luces', CAST(N'2026-09-09' AS Date), CAST(N'2026-10-01' AS Date), N'Luz de vida.', NULL, 1, 9, N'Patronal', NULL, 1)
INSERT [dbo].[FechaRelevante] ([idFechaRelevante], [nombreFestividad], [fechaInicio], [fechaFin], [descripcion], [historia], [esRecurrente], [mesCelebracion], [tipoFestividad], [recomendaciones], [activo]) VALUES (9, N'Ferias y Fiestas', CAST(N'2026-10-02' AS Date), CAST(N'2026-10-29' AS Date), N'full ', NULL, 1, 10, N'Religiosa', NULL, 1)
INSERT [dbo].[FechaRelevante] ([idFechaRelevante], [nombreFestividad], [fechaInicio], [fechaFin], [descripcion], [historia], [esRecurrente], [mesCelebracion], [tipoFestividad], [recomendaciones], [activo]) VALUES (10, N'Concurso Nacional de Cargueros y Festival de la Arcilla', CAST(N'2026-10-09' AS Date), CAST(N'2026-10-30' AS Date), N'Competencia y homenaje tradicional donde los alfareros cargan a sus espaldas grandes estructuras de loza y artesanías de barro, recreando cómo se transportaba históricamente la cerámica. Incluye demostraciones de moldeado en vivo, música folclórica y muestras gastronómicas.', NULL, 1, 10, N'Cultural', NULL, 1)
INSERT [dbo].[FechaRelevante] ([idFechaRelevante], [nombreFestividad], [fechaInicio], [fechaFin], [descripcion], [historia], [esRecurrente], [mesCelebracion], [tipoFestividad], [recomendaciones], [activo]) VALUES (11, N'Fiestas Patronales de San Antonio de Padua', CAST(N'2026-07-08' AS Date), CAST(N'2026-07-11' AS Date), N'Celebración tradicional en honor al santo patrono de Ráquira. Se realizan procesiones, desfiles de comparsas artesanales, actos culturales, verbenas populares y muestras de alfarería local.', NULL, 1, 7, N'Folclórico', NULL, 1)
INSERT [dbo].[FechaRelevante] ([idFechaRelevante], [nombreFestividad], [fechaInicio], [fechaFin], [descripcion], [historia], [esRecurrente], [mesCelebracion], [tipoFestividad], [recomendaciones], [activo]) VALUES (12, N'CicloMontañismo', CAST(N'2026-09-10' AS Date), CAST(N'2026-09-10' AS Date), N'Carrera Ciclistica', NULL, 1, 9, N'Deportiva', NULL, 1)
INSERT [dbo].[FechaRelevante] ([idFechaRelevante], [nombreFestividad], [fechaInicio], [fechaFin], [descripcion], [historia], [esRecurrente], [mesCelebracion], [tipoFestividad], [recomendaciones], [activo]) VALUES (13, N'dee', CAST(N'2026-09-30' AS Date), CAST(N'2026-09-30' AS Date), N'ede', NULL, 1, 9, N'Cultural', NULL, 1)
SET IDENTITY_INSERT [dbo].[FechaRelevante] OFF
GO
SET IDENTITY_INSERT [dbo].[Departamento] ON 

INSERT [dbo].[Departamento] ([idDepartamento], [nombre], [pais], [descripcion]) VALUES (1, N'Boyaca', N'Colombia', N'Departamento ubicado en el centro-oriente de Colombia, conocido por su gran riqueza histórica, cultural y turística.')
SET IDENTITY_INSERT [dbo].[Departamento] OFF
GO
SET IDENTITY_INSERT [dbo].[Municipio] ON 

INSERT [dbo].[Municipio] ([idMunicipio], [idDepartamento], [nombre], [descripcion], [historia], [clima], [imagenUrl], [latitud], [longitud], [fechaCreacion], [activo]) VALUES (1, 1, N'Villa de Leyva', N'Villa colonial declarada Monumento Nacional en 1954, famosa por su plaza mayor empedrada -una de las más grandes de América- y su arquitectura blanca perfectamente conservada desde el siglo XVI.', N'Fundada el 12 de junio de 1572 por el capitán Hernán Suárez de Villalobos, por orden de Andrés Díaz Venero de Leyva, primer presidente del Nuevo Reino de Granada. Su aislamiento de las rutas comerciales importantes permitió que conservara casi intacta su arquitectura colonial original. En tiempos prehistóricos la región estuvo cubierta por un mar que dejó una gran riqueza fósil, visible hoy en sus museos paleontológicos.', N'Seco y templado durante todo el año, con temperaturas entre 12°C y 24°C; noches frías por su ubicación semidesértica a 2.149 msnm.', N'/images/municipios/VillaLeyvaImgPerfil.jpg', CAST(5.633000 AS Decimal(9, 6)), CAST(-73.533000 AS Decimal(9, 6)), CAST(N'2026-09-07T11:53:07.9533333' AS DateTime2), 1)
INSERT [dbo].[Municipio] ([idMunicipio], [idDepartamento], [nombre], [descripcion], [historia], [clima], [imagenUrl], [latitud], [longitud], [fechaCreacion], [activo]) VALUES (2, 1, N'Ráquira', N'Conocida como la "Capital Artesanal de Colombia", famosa! por sus calles multicolores y su tradición centenaria en cerámica y alfarería. Cerca del 70% de sus habitantes vive de la artesanía.', N'Su nombre viene del chibcha "Ruaquira" (Rua: olla, Quira: pueblo), es decir "pueblo de ollas". El territorio fue habitado por los muiscas mucho antes de la conquista española, ya reconocidos entonces por su trabajo en arcilla gracias a la calidad del barro de la zona. El pueblo moderno fue fundado el 18 de octubre de 1580 por el fraile Francisco de Orejuela.', N'Frío de montaña, temperatura promedio de 10°C a 16°C.', N'https://localhost:7078/images/municipios/mun_3b0ee2fcafa44ddfa72e24ff4f505697.jpg', CAST(5.533300 AS Decimal(9, 6)), CAST(-73.633300 AS Decimal(9, 6)), CAST(N'2026-09-07T11:53:07.9533333' AS DateTime2), 1)
INSERT [dbo].[Municipio] ([idMunicipio], [idDepartamento], [nombre], [descripcion], [historia], [clima], [imagenUrl], [latitud], [longitud], [fechaCreacion], [activo]) VALUES (3, 1, N'Paipa', N'Destino de bienestar y turismo termal, reconocido por sus aguas termominerales, el Lago Sochagota y el célebre Queso Paipa con denominación de origen. Cerca de aquí se libró la histórica Batalla del Pantano de Vargas. !!', N'El nombre proviene del cacique Paipa, quien gobernaba a los indígenas de la región antes de la llegada de las tropas de Gonzalo Jiménez de Quesada en 1539. El 25 de julio de 1819 fue escenario de la Batalla del Pantano de Vargas, enfrentamiento decisivo liderado por Simón Bolívar en la lucha por la independencia de Colombia.', N'Templado de montaña, temperatura promedio de 14°C.', N'https://localhost:7078/images/municipios/mun_b95b0717785f4c3daa3f75326e5664d3.jpg', CAST(5.776100 AS Decimal(9, 6)), CAST(-73.117700 AS Decimal(9, 6)), CAST(N'2026-09-07T11:53:07.9533333' AS DateTime2), 1)
INSERT [dbo].[Municipio] ([idMunicipio], [idDepartamento], [nombre], [descripcion], [historia], [clima], [imagenUrl], [latitud], [longitud], [fechaCreacion], [activo]) VALUES (4, 1, N'Tunja', N'Capital del departamento de Boyacá, apodada "Cuna y Taller de la Libertad" por Simón Bolívar. La ciudad habitable más alta y fría de Boyacá, con un centro histórico colonial declarado de gran valor cultural.', N'Antes de la conquista existía aquí el asentamiento indígena muisca de Hunza, sede del zaque (gobernante). La ciudad española fue fundada el 6 de agosto de 1539 por el capitán Gonzalo Suárez Rendón, exactamente un año después de la fundación de Bogotá. A pocos minutos se encuentra el Puente de Boyacá, donde el 7 de agosto de 1819 se libró la batalla que selló la independencia de Colombia.', N'Frío de montaña, temperatura promedio de 13°C, por su altitud de 2.820 msnm.', N'https://localhost:7078/images/municipios/mun_e92c83cffea64b4988e746f8b219ae2f.jpg', CAST(5.533000 AS Decimal(9, 6)), CAST(-73.367000 AS Decimal(9, 6)), CAST(N'2026-09-07T11:53:07.9533333' AS DateTime2), 1)
INSERT [dbo].[Municipio] ([idMunicipio], [idDepartamento], [nombre], [descripcion], [historia], [clima], [imagenUrl], [latitud], [longitud], [fechaCreacion], [activo]) VALUES (5, 1, N'Sogamoso', N'Conocida como la "Ciudad del Sol", fue un importante centro religioso muisca dedicado a la adoración del dios Sué. Hoy es un polo industrial y comercial, sede del Museo Arqueológico que resguarda el reconstruido Templo del Sol.', N'Su nombre deriva de "Suamox", que en chibcha significa "morada del sol". Fue la ciudad religiosa más importante de la cultura muisca, donde se construyó un santuario dorado -el Templo del Sol- como destino final de los caciques momificados del valle de Iraca. El templo fue saqueado por Gonzalo Jiménez de Quesada durante la conquista. La villa moderna fue fundada el 6 de septiembre de 1810.', N'Templado, con temperaturas entre 10°C y 24°C; recibe una de las mayores cantidades de luz solar del país, especialmente en diciembre.', N'https://localhost:7078/images/municipios/mun_fb2b9e0442fd4a6c8234efd360059c63.jpg', CAST(5.716700 AS Decimal(9, 6)), CAST(-72.920800 AS Decimal(9, 6)), CAST(N'2026-09-07T11:53:07.9533333' AS DateTime2), 1)
INSERT [dbo].[Municipio] ([idMunicipio], [idDepartamento], [nombre], [descripcion], [historia], [clima], [imagenUrl], [latitud], [longitud], [fechaCreacion], [activo]) VALUES (6, 1, N'Gámeza', N'Municipio boyacense de alta riqueza ecológica y arqueológica, famoso por sus petroglifos muiscas, sus lagunas de páramo y su tradición minera y gastronómica.', N'Gámeza destaca en Boyacá por su valor histórico y natural. En la época prehispánica estuvo bajo el dominio de los caciques Gamza y Siatoba, albergando vestigios arqueológicos conocidos como la "biblioteca Muisca" (Piedra Pintada y Piedra Rajada). Cuenta con impresionantes escenarios naturales como el Cañón de Gámeza y la Laguna Negra, además de un acogedor parque principal con réplica de una mina de carbón tradicional y esculturas de sus caciques.', N'Frio, 13 °C', N'https://localhost:7078/images/municipios/mun_3ffeab08726c467da492e32111afc940.jpg', CAST(5.802800 AS Decimal(9, 6)), CAST(-72.805600 AS Decimal(9, 6)), CAST(N'2026-09-09T03:55:13.5198495' AS DateTime2), 1)
INSERT [dbo].[Municipio] ([idMunicipio], [idDepartamento], [nombre], [descripcion], [historia], [clima], [imagenUrl], [latitud], [longitud], [fechaCreacion], [activo]) VALUES (7, 1, N'Prueba', N'ppppppppp', N'ggggggggg', N'12', N'https://localhost:7078/images/municipios/mun_72edf5365ff1449da7f178c257112940.jpg', NULL, NULL, CAST(N'2026-10-01T03:13:14.6679683' AS DateTime2), 0)
INSERT [dbo].[Municipio] ([idMunicipio], [idDepartamento], [nombre], [descripcion], [historia], [clima], [imagenUrl], [latitud], [longitud], [fechaCreacion], [activo]) VALUES (8, 1, N'Prueba 1', N'pppppppp', N'jja', N'23', NULL, NULL, NULL, CAST(N'2026-10-01T03:46:57.8458986' AS DateTime2), 0)
SET IDENTITY_INSERT [dbo].[Municipio] OFF
GO
SET IDENTITY_INSERT [dbo].[MunicipioFechaRelevante] ON 

INSERT [dbo].[MunicipioFechaRelevante] ([idMunicipioFechaRelevante], [idMunicipio], [idFechaRelevante], [fotoUrl], [esFestividadPrincipal], [descripcionLocal], [orden], [fechaCreacion]) VALUES (1, 1, 1, N'/images/festividades/festival-luces-villa-leyva.jpg', 1, N'Evento insignia del municipio realizado en la Plaza Mayor.', 1, CAST(N'2026-09-07T16:15:02.670' AS DateTime))
INSERT [dbo].[MunicipioFechaRelevante] ([idMunicipioFechaRelevante], [idMunicipio], [idFechaRelevante], [fotoUrl], [esFestividadPrincipal], [descripcionLocal], [orden], [fechaCreacion]) VALUES (2, 1, 2, N'/images/festividades/festival-cometas-villa-leyva.jpg', 0, N'Competencia de cometas en la plaza principal y desierto.', 2, CAST(N'2026-09-07T16:15:02.670' AS DateTime))
INSERT [dbo].[MunicipioFechaRelevante] ([idMunicipioFechaRelevante], [idMunicipio], [idFechaRelevante], [fotoUrl], [esFestividadPrincipal], [descripcionLocal], [orden], [fechaCreacion]) VALUES (3, 2, 3, N'/images/festividades/festival-arcilla-raquira.jpg', 1, N'Exhibición principal en la plaza de las esculturas de barro.', 1, CAST(N'2026-09-07T16:15:02.673' AS DateTime))
INSERT [dbo].[MunicipioFechaRelevante] ([idMunicipioFechaRelevante], [idMunicipio], [idFechaRelevante], [fotoUrl], [esFestividadPrincipal], [descripcionLocal], [orden], [fechaCreacion]) VALUES (4, 3, 4, N'/images/festividades/festival-ruana-paipa.jpg', 1, N'Muestra artesanal en la Concha Acústica de Paipa.', 1, CAST(N'2026-09-07T16:15:02.680' AS DateTime))
INSERT [dbo].[MunicipioFechaRelevante] ([idMunicipioFechaRelevante], [idMunicipio], [idFechaRelevante], [fotoUrl], [esFestividadPrincipal], [descripcionLocal], [orden], [fechaCreacion]) VALUES (5, 4, 5, N'/images/festividades/aguinaldo-boyacense-tunja.jpg', 1, N'Desfiles por las principales avenidas y la Plaza de Bolívar.', 1, CAST(N'2026-09-07T16:15:02.687' AS DateTime))
INSERT [dbo].[MunicipioFechaRelevante] ([idMunicipioFechaRelevante], [idMunicipio], [idFechaRelevante], [fotoUrl], [esFestividadPrincipal], [descripcionLocal], [orden], [fechaCreacion]) VALUES (6, 5, 6, N'/images/festividades/fiesta-sol-acero-sogamoso.jpg', 1, N'Celebración central en la Plaza de Villa de Leyva y plazas públicas de Sogamoso.', 1, CAST(N'2026-09-07T16:15:02.690' AS DateTime))
INSERT [dbo].[MunicipioFechaRelevante] ([idMunicipioFechaRelevante], [idMunicipio], [idFechaRelevante], [fotoUrl], [esFestividadPrincipal], [descripcionLocal], [orden], [fechaCreacion]) VALUES (8, 6, 8, NULL, 0, NULL, 1, CAST(N'2026-09-09T04:41:48.713' AS DateTime))
INSERT [dbo].[MunicipioFechaRelevante] ([idMunicipioFechaRelevante], [idMunicipio], [idFechaRelevante], [fotoUrl], [esFestividadPrincipal], [descripcionLocal], [orden], [fechaCreacion]) VALUES (9, 6, 9, NULL, 0, NULL, 1, CAST(N'2026-09-09T05:08:56.063' AS DateTime))
INSERT [dbo].[MunicipioFechaRelevante] ([idMunicipioFechaRelevante], [idMunicipio], [idFechaRelevante], [fotoUrl], [esFestividadPrincipal], [descripcionLocal], [orden], [fechaCreacion]) VALUES (10, 2, 10, NULL, 0, NULL, 1, CAST(N'2026-09-09T13:01:44.367' AS DateTime))
INSERT [dbo].[MunicipioFechaRelevante] ([idMunicipioFechaRelevante], [idMunicipio], [idFechaRelevante], [fotoUrl], [esFestividadPrincipal], [descripcionLocal], [orden], [fechaCreacion]) VALUES (11, 2, 11, NULL, 0, NULL, 1, CAST(N'2026-09-09T13:02:53.400' AS DateTime))
INSERT [dbo].[MunicipioFechaRelevante] ([idMunicipioFechaRelevante], [idMunicipio], [idFechaRelevante], [fotoUrl], [esFestividadPrincipal], [descripcionLocal], [orden], [fechaCreacion]) VALUES (12, 2, 12, NULL, 0, NULL, 1, CAST(N'2026-09-10T15:55:46.117' AS DateTime))
INSERT [dbo].[MunicipioFechaRelevante] ([idMunicipioFechaRelevante], [idMunicipio], [idFechaRelevante], [fotoUrl], [esFestividadPrincipal], [descripcionLocal], [orden], [fechaCreacion]) VALUES (13, 7, 13, NULL, 0, NULL, 1, CAST(N'2026-10-01T03:13:15.713' AS DateTime))
SET IDENTITY_INSERT [dbo].[MunicipioFechaRelevante] OFF
GO
SET IDENTITY_INSERT [dbo].[Categoria] ON 

INSERT [dbo].[Categoria] ([idCategoria], [nombre], [descripcion], [icono], [aplicaNegocio], [aplicaSitio]) VALUES (1, N'Gastronomía', N'Restaurantes, cafés y gastronomía típica', N'ph ph-fork-knife', 1, 0)
INSERT [dbo].[Categoria] ([idCategoria], [nombre], [descripcion], [icono], [aplicaNegocio], [aplicaSitio]) VALUES (2, N'Artesanías', N'Tiendas y talleres artesanales', N'ph ph-palette', 1, 1)
INSERT [dbo].[Categoria] ([idCategoria], [nombre], [descripcion], [icono], [aplicaNegocio], [aplicaSitio]) VALUES (3, N'Hospedaje', N'Hoteles, posadas y alojamientos', N'ph ph-bed', 1, 0)
INSERT [dbo].[Categoria] ([idCategoria], [nombre], [descripcion], [icono], [aplicaNegocio], [aplicaSitio]) VALUES (5, N'Lagos', N'Lagos, lagunas y fuentes hídricas naturales', N'ph ph-waves', 0, 1)
INSERT [dbo].[Categoria] ([idCategoria], [nombre], [descripcion], [icono], [aplicaNegocio], [aplicaSitio]) VALUES (6, N'Montañas', N'Atractivos de montaña, senderismo y naturaleza', N'ph ph-mountains', 0, 1)
INSERT [dbo].[Categoria] ([idCategoria], [nombre], [descripcion], [icono], [aplicaNegocio], [aplicaSitio]) VALUES (7, N'Monumentos', N'Sitios de interés histórico, plazas y arquitectura', N'ph ph-landmark', 0, 1)
INSERT [dbo].[Categoria] ([idCategoria], [nombre], [descripcion], [icono], [aplicaNegocio], [aplicaSitio]) VALUES (8, N'Páramos', N'Ecosistemas de páramo y reservas naturales', N'ph ph-tree', 0, 1)
INSERT [dbo].[Categoria] ([idCategoria], [nombre], [descripcion], [icono], [aplicaNegocio], [aplicaSitio]) VALUES (9, N'Pueblos', N'Pueblos patrimonio y arquitectura colonial', N'ph ph-house-line', 0, 1)
SET IDENTITY_INSERT [dbo].[Categoria] OFF
GO
SET IDENTITY_INSERT [dbo].[SitiosTuristicos] ON 

INSERT [dbo].[SitiosTuristicos] ([idSitiosTuristicos], [nombre], [descripcion], [imagenUrl], [idMunicipio], [idReseña], [idCategoria], [altitud], [destacado], [activo]) VALUES (1, N'Plaza Mayor de Villa de Leyva', N'Una de las plazas totalmente empedradas más grandes de América Latina, rodeada de edificios coloniales.', N'https://lh3.googleusercontent.com/gps-cs-s/AHRPTWlE93erQLmEJEjRy4ba5FejFxbZdkDTgxFI-ChQDaqtdr_tLsmpZS6KCT_hiO2fiomfgXm9HjEz8cWY6DZntsNMzH3IlB69Irv2iBBTlMcRPg4cMZE03D3TX08QccGi2VKTmGGYQA=s680-w680-h510-rw', 1, 1, 9, 2149, 1, 1)
INSERT [dbo].[SitiosTuristicos] ([idSitiosTuristicos], [nombre], [descripcion], [imagenUrl], [idMunicipio], [idReseña], [idCategoria], [altitud], [destacado], [activo]) VALUES (2, N'Taller de Alfarería y Artesanías', N'Centro cultural y artesanal donde los maestros alfareros enseñan el moldeo tradicional del barro.', N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTwp-4TrHQS66xYJJBgl0cbk_vgMlL7Q-73ckYDf0KjwX7KW0a9K-aImWCe&s=10', 2, 2, 2, 2150, 1, 1)
INSERT [dbo].[SitiosTuristicos] ([idSitiosTuristicos], [nombre], [descripcion], [imagenUrl], [idMunicipio], [idReseña], [idCategoria], [altitud], [destacado], [activo]) VALUES (3, N'Termales de Paipa', N'Complejo turístico de aguas termominero-medicinales ideal para el descanso y el bienestar.', N'https://cuponassetsv2.cuponatic-latam.com/backendCo/uploads/imagenes_descuentos/200969/7d521c6dfa0f6b724c7625bf07811e407d570bb4.XL2.jpg', 3, 3, 5, 2525, 1, 1)
INSERT [dbo].[SitiosTuristicos] ([idSitiosTuristicos], [nombre], [descripcion], [imagenUrl], [idMunicipio], [idReseña], [idCategoria], [altitud], [destacado], [activo]) VALUES (4, N'Catedral Basílica Metropolitana', N'Imponente templo religioso de arquitectura colonial ubicado en la Plaza de Bolívar de Tunja.', N'https://dynamic-media-cdn.tripadvisor.com/media/photo-o/09/23/23/c5/catedral-basilica-metropolitan.jpg?w=800&h=-1&s=1', 4, 4, 7, 2782, 1, 1)
INSERT [dbo].[SitiosTuristicos] ([idSitiosTuristicos], [nombre], [descripcion], [imagenUrl], [idMunicipio], [idReseña], [idCategoria], [altitud], [destacado], [activo]) VALUES (5, N'Museo del Sol Suamox', N'Templo y museo arqueológico que rinde homenaje a la cultura Muisca y al culto al Sol.', N'https://situr.boyaca.gov.co/wp-content/uploads/2017/06/Museo-arqueol%C3%B3gico-1-Sogamoso-Boyac%C3%A1-Colombia.jpg', 5, 5, 7, 2569, 1, 1)
INSERT [dbo].[SitiosTuristicos] ([idSitiosTuristicos], [nombre], [descripcion], [imagenUrl], [idMunicipio], [idReseña], [idCategoria], [altitud], [destacado], [activo]) VALUES (6, N'Piedra del Sol y Jeroglíficos', N'Zona arqueológica con pinturas rupestres y vestigios prehispánicos de la cultura Muisca.', N'https://pbs.twimg.com/media/B8ddIcXCQAAGaTo.jpg', 6, NULL, 7, 2750, 1, 1)
SET IDENTITY_INSERT [dbo].[SitiosTuristicos] OFF
GO
SET IDENTITY_INSERT [dbo].[Etiqueta] ON 

INSERT [dbo].[Etiqueta] ([idEtiqueta], [nombre], [descripcion], [activo], [icono]) VALUES (1, N'Paisaje & Páramo', NULL, 1, N'ph ph-tree')
INSERT [dbo].[Etiqueta] ([idEtiqueta], [nombre], [descripcion], [activo], [icono]) VALUES (2, N'Fiestas & Ferias', NULL, 1, N'ph ph-confetti')
INSERT [dbo].[Etiqueta] ([idEtiqueta], [nombre], [descripcion], [activo], [icono]) VALUES (3, N'Montañas', NULL, 1, N'ph ph-mountains')
INSERT [dbo].[Etiqueta] ([idEtiqueta], [nombre], [descripcion], [activo], [icono]) VALUES (4, N'Ecoturismo', NULL, 1, N'ph ph-leaf')
INSERT [dbo].[Etiqueta] ([idEtiqueta], [nombre], [descripcion], [activo], [icono]) VALUES (5, N'Gastronomía', NULL, 1, N'ph ph-fork-knife')
INSERT [dbo].[Etiqueta] ([idEtiqueta], [nombre], [descripcion], [activo], [icono]) VALUES (6, N'Artesanías', NULL, 1, N'ph ph-palette')
INSERT [dbo].[Etiqueta] ([idEtiqueta], [nombre], [descripcion], [activo], [icono]) VALUES (7, N'Aventura', NULL, 1, N'ph ph-compass')
INSERT [dbo].[Etiqueta] ([idEtiqueta], [nombre], [descripcion], [activo], [icono]) VALUES (8, N'Paraíso del Photo Dump', NULL, 1, N'ph ph-camera')
INSERT [dbo].[Etiqueta] ([idEtiqueta], [nombre], [descripcion], [activo], [icono]) VALUES (9, N'Ciclismo', NULL, 1, N'ph ph-bicycle')
INSERT [dbo].[Etiqueta] ([idEtiqueta], [nombre], [descripcion], [activo], [icono]) VALUES (10, N'Team Soccer', NULL, 0, NULL)
INSERT [dbo].[Etiqueta] ([idEtiqueta], [nombre], [descripcion], [activo], [icono]) VALUES (11, N'Historia & Colonial', NULL, 1, NULL)
SET IDENTITY_INSERT [dbo].[Etiqueta] OFF
GO
SET IDENTITY_INSERT [dbo].[MunicipioEtiqueta] ON 

INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (1, 6, 1)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (2, 6, 2)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (3, 6, 3)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (4, 2, 4)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (5, 2, 5)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (6, 2, 6)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (7, 2, 7)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (8, 2, 8)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (9, 2, 9)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (10, 1, 2)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (11, 1, 5)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (12, 1, 6)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (13, 1, 8)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (14, 2, 4)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (15, 2, 6)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (16, 2, 8)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (17, 3, 2)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (18, 3, 5)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (19, 3, 7)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (20, 3, 9)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (21, 4, 2)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (22, 4, 5)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (23, 5, 1)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (24, 5, 3)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (25, 5, 9)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (26, 6, 1)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (27, 6, 3)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (28, 6, 4)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (29, 7, 10)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (30, 8, 11)
INSERT [dbo].[MunicipioEtiqueta] ([idMunicipioEtiqueta], [idMunicipio], [idEtiqueta]) VALUES (31, 8, 10)
SET IDENTITY_INSERT [dbo].[MunicipioEtiqueta] OFF
GO
SET IDENTITY_INSERT [dbo].[Rol] ON 

INSERT [dbo].[Rol] ([idRol], [nombre], [descripcion]) VALUES (1, N'AdminGeneral', N'Control total del sistema ')
INSERT [dbo].[Rol] ([idRol], [nombre], [descripcion]) VALUES (2, N'AdminMunicipio', N'Gestiona su municipio asignado')
INSERT [dbo].[Rol] ([idRol], [nombre], [descripcion]) VALUES (3, N'AdminEstablecimiento', N'Gestiona su negocio registrado')
INSERT [dbo].[Rol] ([idRol], [nombre], [descripcion]) VALUES (4, N'Usuario', N'Turista - acceso público')
SET IDENTITY_INSERT [dbo].[Rol] OFF
GO
SET IDENTITY_INSERT [dbo].[TiposNotificacion] ON 

INSERT [dbo].[TiposNotificacion] ([idTipoNotificacion], [codigo], [descripcion]) VALUES (1, N'FAV_NUEVO', N'Un usuario guardó un negocio en favoritos')
INSERT [dbo].[TiposNotificacion] ([idTipoNotificacion], [codigo], [descripcion]) VALUES (2, N'INFO_GENERAL', N'Notificación informativa del sistema')
INSERT [dbo].[TiposNotificacion] ([idTipoNotificacion], [codigo], [descripcion]) VALUES (3, N'PROMO_NEGOCIO', N'Promociones o eventos de negocios locales')
INSERT [dbo].[TiposNotificacion] ([idTipoNotificacion], [codigo], [descripcion]) VALUES (4, N'NEGOCIO_APROBADO', N'Un establecimiento del usuario fue aprobado por el municipio')
INSERT [dbo].[TiposNotificacion] ([idTipoNotificacion], [codigo], [descripcion]) VALUES (5, N'NEGOCIO_RECHAZADO', N'Un establecimiento del usuario fue rechazado por el municipio')
INSERT [dbo].[TiposNotificacion] ([idTipoNotificacion], [codigo], [descripcion]) VALUES (6, N'NEGOCIO_PENDIENTE', N'Un establecimiento del municipio espera revisión')
INSERT [dbo].[TiposNotificacion] ([idTipoNotificacion], [codigo], [descripcion]) VALUES (7, N'RESENA_NUEVA', N'RESENA_NUEVA')
SET IDENTITY_INSERT [dbo].[TiposNotificacion] OFF
GO
