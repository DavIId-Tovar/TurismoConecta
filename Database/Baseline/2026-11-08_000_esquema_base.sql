/* ════════════════════════════════════════════════════════════════
   BASELINE: esquema de TurismoConecta al 2026-11-08
   Generado con SSMS; desde TurismoConectaDB_Azure.
   Uso: SOLO sobre una base de datos VACÍA (desarrollo o reconstrucción).
   NO ejecutar en producción. Después de este archivo se ejecutan:
     1) Baseline/2026-11-08_000_datos_catalogo.sql
     2) los scripts de Database/Scripts/, en orden de nombre.
   ════════════════════════════════════════════════════════════════ */




/****** Objeto: Table [dbo].[__EFMigrationsHistory] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[ProductVersion] [nvarchar](32) COLLATE Modern_Spanish_CI_AS NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[Categoria] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categoria]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Categoria](
	[idCategoria] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[descripcion] [nvarchar](300) COLLATE Modern_Spanish_CI_AS NULL,
	[icono] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NULL,
	[aplicaNegocio] [bit] NOT NULL,
	[aplicaSitio] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idCategoria] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[Departamento] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Departamento]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Departamento](
	[idDepartamento] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[pais] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[descripcion] [nvarchar](500) COLLATE Modern_Spanish_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[idDepartamento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[Etiqueta] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Etiqueta]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Etiqueta](
	[idEtiqueta] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[descripcion] [nvarchar](300) COLLATE Modern_Spanish_CI_AS NULL,
	[activo] [bit] NOT NULL,
	[icono] [nvarchar](100) COLLATE Modern_Spanish_CI_AI NULL,
 CONSTRAINT [PK_Etiqueta] PRIMARY KEY CLUSTERED 
(
	[idEtiqueta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[Favorito] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Favorito]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Favorito](
	[idFavorito] [int] IDENTITY(1,1) NOT NULL,
	[idUsuario] [int] NOT NULL,
	[idMunicipio] [int] NULL,
	[idNegocio] [int] NULL,
	[fechaGuardado] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Favorito] PRIMARY KEY CLUSTERED 
(
	[idFavorito] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[FechaRelevante] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FechaRelevante]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[FechaRelevante](
	[idFechaRelevante] [int] IDENTITY(1,1) NOT NULL,
	[nombreFestividad] [nvarchar](150) COLLATE Modern_Spanish_CI_AI NOT NULL,
	[fechaInicio] [date] NOT NULL,
	[fechaFin] [date] NOT NULL,
	[descripcion] [nvarchar](max) COLLATE Modern_Spanish_CI_AI NULL,
	[historia] [nvarchar](max) COLLATE Modern_Spanish_CI_AI NULL,
	[esRecurrente] [bit] NOT NULL,
	[mesCelebracion] [int] NULL,
	[tipoFestividad] [nvarchar](50) COLLATE Modern_Spanish_CI_AI NULL,
	[recomendaciones] [nvarchar](max) COLLATE Modern_Spanish_CI_AI NULL,
	[activo] [bit] NOT NULL,
 CONSTRAINT [PK_FechaRelevante] PRIMARY KEY CLUSTERED 
(
	[idFechaRelevante] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[GaleriaNegocio] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GaleriaNegocio]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[GaleriaNegocio](
	[idGaleria] [int] IDENTITY(1,1) NOT NULL,
	[idNegocio] [int] NOT NULL,
	[imagenUrl] [nvarchar](300) COLLATE Modern_Spanish_CI_AS NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idGaleria] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[Itinerario] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Itinerario]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Itinerario](
	[idItinerario] [int] IDENTITY(1,1) NOT NULL,
	[idUsuario] [int] NOT NULL,
	[nombre] [nvarchar](150) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[fechaInicio] [date] NULL,
	[fechaFin] [date] NULL,
	[compartido] [bit] NOT NULL,
	[codigoCompartir] [uniqueidentifier] NOT NULL,
	[fechaCreacion] [datetime2](7) NOT NULL,
	[observaciones] [nvarchar](150) COLLATE Modern_Spanish_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[idItinerario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[ItinerarioDetalle] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ItinerarioDetalle]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ItinerarioDetalle](
	[idItinerarioDetalle] [int] IDENTITY(1,1) NOT NULL,
	[idItinerario] [int] NOT NULL,
	[idMunicipio] [int] NOT NULL,
	[diaNumero] [int] NOT NULL,
	[orden] [int] NOT NULL,
	[fechaVisita] [date] NULL,
	[distanciaKm] [decimal](6, 2) NULL,
	[tiempoEstimadoMin] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[idItinerarioDetalle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[Municipio] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Municipio]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Municipio](
	[idMunicipio] [int] IDENTITY(1,1) NOT NULL,
	[idDepartamento] [int] NOT NULL,
	[nombre] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[descripcion] [nvarchar](1000) COLLATE Modern_Spanish_CI_AS NULL,
	[historia] [nvarchar](max) COLLATE Modern_Spanish_CI_AS NULL,
	[clima] [nvarchar](200) COLLATE Modern_Spanish_CI_AS NULL,
	[imagenUrl] [nvarchar](300) COLLATE Modern_Spanish_CI_AS NULL,
	[latitud] [decimal](9, 6) NULL,
	[longitud] [decimal](9, 6) NULL,
	[fechaCreacion] [datetime2](7) NOT NULL,
	[activo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idMunicipio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[MunicipioEtiqueta] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MunicipioEtiqueta]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[MunicipioEtiqueta](
	[idMunicipioEtiqueta] [int] IDENTITY(1,1) NOT NULL,
	[idMunicipio] [int] NOT NULL,
	[idEtiqueta] [int] NOT NULL,
 CONSTRAINT [PK_MunicipioEtiqueta] PRIMARY KEY CLUSTERED 
(
	[idMunicipioEtiqueta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[MunicipioFechaRelevante] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MunicipioFechaRelevante]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[MunicipioFechaRelevante](
	[idMunicipioFechaRelevante] [int] IDENTITY(1,1) NOT NULL,
	[idMunicipio] [int] NOT NULL,
	[idFechaRelevante] [int] NOT NULL,
	[fotoUrl] [nvarchar](max) COLLATE Modern_Spanish_CI_AI NULL,
	[esFestividadPrincipal] [bit] NOT NULL,
	[descripcionLocal] [nvarchar](max) COLLATE Modern_Spanish_CI_AI NULL,
	[orden] [int] NOT NULL,
	[fechaCreacion] [datetime] NOT NULL,
 CONSTRAINT [PK_MunicipioFechaRelevante] PRIMARY KEY CLUSTERED 
(
	[idMunicipioFechaRelevante] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[Negocio] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Negocio]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Negocio](
	[idNegocio] [int] IDENTITY(1,1) NOT NULL,
	[idMunicipio] [int] NOT NULL,
	[idCategoria] [int] NOT NULL,
	[idUsuario] [int] NULL,
	[nombre] [nvarchar](150) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[descripcion] [nvarchar](1000) COLLATE Modern_Spanish_CI_AS NULL,
	[direccion] [nvarchar](250) COLLATE Modern_Spanish_CI_AS NULL,
	[telefono] [nvarchar](30) COLLATE Modern_Spanish_CI_AS NULL,
	[email] [nvarchar](200) COLLATE Modern_Spanish_CI_AS NULL,
	[horario] [nvarchar](200) COLLATE Modern_Spanish_CI_AS NULL,
	[imagenPrincipalUrl] [nvarchar](300) COLLATE Modern_Spanish_CI_AS NULL,
	[latitud] [decimal](9, 6) NULL,
	[longitud] [decimal](9, 6) NULL,
	[estado] [nvarchar](20) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[fechaRegistro] [datetime2](7) NULL,
	[fechaAprobacion] [datetime2](7) NULL,
 CONSTRAINT [PK_Negocio] PRIMARY KEY CLUSTERED 
(
	[idNegocio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[NegocioEtiqueta] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NegocioEtiqueta]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[NegocioEtiqueta](
	[idNegocioEtiqueta] [int] IDENTITY(1,1) NOT NULL,
	[idEtiqueta] [int] NOT NULL,
	[idNegocio] [int] NOT NULL,
 CONSTRAINT [PK_NegocioEtiqueta] PRIMARY KEY CLUSTERED 
(
	[idNegocioEtiqueta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[Notificacion] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notificacion]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Notificacion](
	[idNotificacion] [int] IDENTITY(1,1) NOT NULL,
	[idUsuario] [int] NOT NULL,
	[idTipoNotificacion] [int] NOT NULL,
	[mensaje] [nvarchar](500) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[leida] [bit] NOT NULL,
	[fechaCreacion] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idNotificacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[PreferenciaUsuario] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PreferenciaUsuario]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PreferenciaUsuario](
	[idPreferenciaUsuario] [int] IDENTITY(1,1) NOT NULL,
	[idUsuario] [int] NOT NULL,
	[idEtiqueta] [int] NOT NULL,
	[fechaRegistro] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_PreferenciaUsuario] PRIMARY KEY CLUSTERED 
(
	[idPreferenciaUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_PreferenciaUsuario_Usuario_Etiqueta] UNIQUE NONCLUSTERED 
(
	[idUsuario] ASC,
	[idEtiqueta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[RecomendacionDescartada] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RecomendacionDescartada]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[RecomendacionDescartada](
	[idRecomendacionDescartada] [int] IDENTITY(1,1) NOT NULL,
	[idUsuario] [int] NOT NULL,
	[idMunicipio] [int] NOT NULL,
	[fechaRegistro] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_RecomendacionDescartada] PRIMARY KEY CLUSTERED 
(
	[idRecomendacionDescartada] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[Reseña] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Reseña]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Reseña](
	[idReseña] [int] IDENTITY(1,1) NOT NULL,
	[idUsuario] [int] NOT NULL,
	[idMunicipio] [int] NULL,
	[idNegocio] [int] NULL,
	[calificacion] [tinyint] NOT NULL,
	[comentario] [nvarchar](1000) COLLATE Modern_Spanish_CI_AS NULL,
	[respuesta] [nvarchar](1000) COLLATE Modern_Spanish_CI_AS NULL,
	[fechaRespuesta] [datetime2](7) NULL,
	[moderada] [bit] NOT NULL,
	[fechaCreacion] [datetime2](7) NOT NULL,
	[idSitiosTuristicos] [int] NULL,
 CONSTRAINT [PK_Reseña] PRIMARY KEY CLUSTERED 
(
	[idReseña] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[Rol] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Rol]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Rol](
	[idRol] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](50) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[descripcion] [nvarchar](300) COLLATE Modern_Spanish_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[idRol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[SitiosTuristicos] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SitiosTuristicos]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[SitiosTuristicos](
	[idSitiosTuristicos] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [varchar](150) COLLATE Modern_Spanish_CI_AI NOT NULL,
	[descripcion] [varchar](max) COLLATE Modern_Spanish_CI_AI NULL,
	[imagenUrl] [varchar](max) COLLATE Modern_Spanish_CI_AI NULL,
	[idMunicipio] [int] NOT NULL,
	[idReseña] [int] NULL,
	[idCategoria] [int] NULL,
	[altitud] [int] NULL,
	[destacado] [bit] NOT NULL,
	[activo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[idSitiosTuristicos] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Objeto: Table [dbo].[TiposNotificacion] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TiposNotificacion]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TiposNotificacion](
	[idTipoNotificacion] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [nvarchar](50) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[descripcion] [nvarchar](300) COLLATE Modern_Spanish_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[idTipoNotificacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Objeto: Table [dbo].[Usuario] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Usuario]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Usuario](
	[idUsuario] [int] IDENTITY(1,1) NOT NULL,
	[idRol] [int] NOT NULL,
	[nombre] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[apellido] [nvarchar](100) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[email] [nvarchar](200) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[passwordHash] [nvarchar](300) COLLATE Modern_Spanish_CI_AS NOT NULL,
	[telefono] [nvarchar](30) COLLATE Modern_Spanish_CI_AS NULL,
	[municipioAsignadoId] [int] NULL,
	[fechaRegistro] [datetime2](7) NOT NULL,
	[emailConfirmado] [bit] NOT NULL,
	[activo] [bit] NOT NULL,
	[PasswordResetToken] [nvarchar](256) COLLATE Modern_Spanish_CI_AS NULL,
	[PasswordResetExpira] [datetime2](7) NULL,
	[FotoUrl] [varchar](max) COLLATE Modern_Spanish_CI_AS NULL,
PRIMARY KEY CLUSTERED 
(
	[idUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_PADDING OFF
GO
/****** Objeto: Index [UQ_RecomendacionDescartada_Usuario_Municipio] Fecha de script: 9/11/2026 5:31:08 p. m. ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[RecomendacionDescartada]') AND name = N'UQ_RecomendacionDescartada_Usuario_Municipio')
CREATE UNIQUE NONCLUSTERED INDEX [UQ_RecomendacionDescartada_Usuario_Municipio] ON [dbo].[RecomendacionDescartada]
(
	[idUsuario] ASC,
	[idMunicipio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_Categoria_AplicaNegocio]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Categoria] ADD  CONSTRAINT [DF_Categoria_AplicaNegocio]  DEFAULT ((0)) FOR [aplicaNegocio]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_Categoria_AplicaSitio]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Categoria] ADD  CONSTRAINT [DF_Categoria_AplicaSitio]  DEFAULT ((0)) FOR [aplicaSitio]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Departamen__pais__10566F31]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Departamento] ADD  DEFAULT ('Colombia') FOR [pais]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Etiqueta__Activo__3587F3E0]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Etiqueta] ADD  DEFAULT ((1)) FOR [activo]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_Favorito_fechaGuardado]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Favorito] ADD  CONSTRAINT [DF_Favorito_fechaGuardado]  DEFAULT (sysdatetime()) FOR [fechaGuardado]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__FechaRele__esRec__3F115E1A]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[FechaRelevante] ADD  DEFAULT ((1)) FOR [esRecurrente]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__FechaRele__activ__40058253]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[FechaRelevante] ADD  DEFAULT ((1)) FOR [activo]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Itinerari__compa__123EB7A3]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Itinerario] ADD  DEFAULT ((0)) FOR [compartido]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Itinerari__codig__1332DBDC]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Itinerario] ADD  DEFAULT (newid()) FOR [codigoCompartir]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Itinerari__fecha__14270015]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Itinerario] ADD  DEFAULT (sysdatetime()) FOR [fechaCreacion]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Itinerari__diaNu__151B244E]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ItinerarioDetalle] ADD  DEFAULT ((1)) FOR [diaNumero]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Itinerari__orden__160F4887]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ItinerarioDetalle] ADD  DEFAULT ((1)) FOR [orden]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Municipio__fecha__17036CC0]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Municipio] ADD  DEFAULT (sysdatetime()) FOR [fechaCreacion]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Municipio__activ__17F790F9]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Municipio] ADD  DEFAULT ((1)) FOR [activo]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Municipio__esFes__42E1EEFE]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[MunicipioFechaRelevante] ADD  DEFAULT ((0)) FOR [esFestividadPrincipal]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Municipio__orden__43D61337]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[MunicipioFechaRelevante] ADD  DEFAULT ((1)) FOR [orden]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Municipio__fecha__44CA3770]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[MunicipioFechaRelevante] ADD  DEFAULT (getdate()) FOR [fechaCreacion]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_Negocio_estado]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Negocio] ADD  CONSTRAINT [DF_Negocio_estado]  DEFAULT ('Pendiente') FOR [estado]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Notificac__leida__19DFD96B]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Notificacion] ADD  DEFAULT ((0)) FOR [leida]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Notificac__fecha__1AD3FDA4]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Notificacion] ADD  DEFAULT (sysdatetime()) FOR [fechaCreacion]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_PreferenciaUsuario_fechaRegistro]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[PreferenciaUsuario] ADD  CONSTRAINT [DF_PreferenciaUsuario_fechaRegistro]  DEFAULT (sysutcdatetime()) FOR [fechaRegistro]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_RecomendacionDescartada_fechaRegistro]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[RecomendacionDescartada] ADD  CONSTRAINT [DF_RecomendacionDescartada_fechaRegistro]  DEFAULT (sysutcdatetime()) FOR [fechaRegistro]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_SitiosTuristicos_destacado]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[SitiosTuristicos] ADD  CONSTRAINT [DF_SitiosTuristicos_destacado]  DEFAULT ((0)) FOR [destacado]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF_SitiosTuristicos_activo]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[SitiosTuristicos] ADD  CONSTRAINT [DF_SitiosTuristicos_activo]  DEFAULT ((1)) FOR [activo]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Usuario__fechaRe__1BC821DD]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Usuario] ADD  DEFAULT (sysdatetime()) FOR [fechaRegistro]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Usuario__emailCo__1CBC4616]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Usuario] ADD  DEFAULT ((0)) FOR [emailConfirmado]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Usuario__activo__1DB06A4F]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Usuario] ADD  DEFAULT ((1)) FOR [activo]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Favorito_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[Favorito]'))
ALTER TABLE [dbo].[Favorito]  WITH CHECK ADD  CONSTRAINT [FK_Favorito_Municipio] FOREIGN KEY([idMunicipio])
REFERENCES [dbo].[Municipio] ([idMunicipio])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Favorito_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[Favorito]'))
ALTER TABLE [dbo].[Favorito] CHECK CONSTRAINT [FK_Favorito_Municipio]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Favorito_Negocio]') AND parent_object_id = OBJECT_ID(N'[dbo].[Favorito]'))
ALTER TABLE [dbo].[Favorito]  WITH CHECK ADD  CONSTRAINT [FK_Favorito_Negocio] FOREIGN KEY([idNegocio])
REFERENCES [dbo].[Negocio] ([idNegocio])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Favorito_Negocio]') AND parent_object_id = OBJECT_ID(N'[dbo].[Favorito]'))
ALTER TABLE [dbo].[Favorito] CHECK CONSTRAINT [FK_Favorito_Negocio]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Favorito_Usuario]') AND parent_object_id = OBJECT_ID(N'[dbo].[Favorito]'))
ALTER TABLE [dbo].[Favorito]  WITH CHECK ADD  CONSTRAINT [FK_Favorito_Usuario] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuario] ([idUsuario])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Favorito_Usuario]') AND parent_object_id = OBJECT_ID(N'[dbo].[Favorito]'))
ALTER TABLE [dbo].[Favorito] CHECK CONSTRAINT [FK_Favorito_Usuario]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GaleriaNegocio_Negocio]') AND parent_object_id = OBJECT_ID(N'[dbo].[GaleriaNegocio]'))
ALTER TABLE [dbo].[GaleriaNegocio]  WITH CHECK ADD  CONSTRAINT [FK_GaleriaNegocio_Negocio] FOREIGN KEY([idNegocio])
REFERENCES [dbo].[Negocio] ([idNegocio])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_GaleriaNegocio_Negocio]') AND parent_object_id = OBJECT_ID(N'[dbo].[GaleriaNegocio]'))
ALTER TABLE [dbo].[GaleriaNegocio] CHECK CONSTRAINT [FK_GaleriaNegocio_Negocio]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Itinerarios_Usuarios]') AND parent_object_id = OBJECT_ID(N'[dbo].[Itinerario]'))
ALTER TABLE [dbo].[Itinerario]  WITH CHECK ADD  CONSTRAINT [FK_Itinerarios_Usuarios] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuario] ([idUsuario])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Itinerarios_Usuarios]') AND parent_object_id = OBJECT_ID(N'[dbo].[Itinerario]'))
ALTER TABLE [dbo].[Itinerario] CHECK CONSTRAINT [FK_Itinerarios_Usuarios]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ItinerarioDetalle_Itinerarios]') AND parent_object_id = OBJECT_ID(N'[dbo].[ItinerarioDetalle]'))
ALTER TABLE [dbo].[ItinerarioDetalle]  WITH CHECK ADD  CONSTRAINT [FK_ItinerarioDetalle_Itinerarios] FOREIGN KEY([idItinerario])
REFERENCES [dbo].[Itinerario] ([idItinerario])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ItinerarioDetalle_Itinerarios]') AND parent_object_id = OBJECT_ID(N'[dbo].[ItinerarioDetalle]'))
ALTER TABLE [dbo].[ItinerarioDetalle] CHECK CONSTRAINT [FK_ItinerarioDetalle_Itinerarios]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ItinerarioDetalle_Municipios]') AND parent_object_id = OBJECT_ID(N'[dbo].[ItinerarioDetalle]'))
ALTER TABLE [dbo].[ItinerarioDetalle]  WITH CHECK ADD  CONSTRAINT [FK_ItinerarioDetalle_Municipios] FOREIGN KEY([idMunicipio])
REFERENCES [dbo].[Municipio] ([idMunicipio])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ItinerarioDetalle_Municipios]') AND parent_object_id = OBJECT_ID(N'[dbo].[ItinerarioDetalle]'))
ALTER TABLE [dbo].[ItinerarioDetalle] CHECK CONSTRAINT [FK_ItinerarioDetalle_Municipios]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Municipios_Departamentos]') AND parent_object_id = OBJECT_ID(N'[dbo].[Municipio]'))
ALTER TABLE [dbo].[Municipio]  WITH CHECK ADD  CONSTRAINT [FK_Municipios_Departamentos] FOREIGN KEY([idDepartamento])
REFERENCES [dbo].[Departamento] ([idDepartamento])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Municipios_Departamentos]') AND parent_object_id = OBJECT_ID(N'[dbo].[Municipio]'))
ALTER TABLE [dbo].[Municipio] CHECK CONSTRAINT [FK_Municipios_Departamentos]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MunicipioEtiqueta_Etiqueta]') AND parent_object_id = OBJECT_ID(N'[dbo].[MunicipioEtiqueta]'))
ALTER TABLE [dbo].[MunicipioEtiqueta]  WITH CHECK ADD  CONSTRAINT [FK_MunicipioEtiqueta_Etiqueta] FOREIGN KEY([idEtiqueta])
REFERENCES [dbo].[Etiqueta] ([idEtiqueta])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MunicipioEtiqueta_Etiqueta]') AND parent_object_id = OBJECT_ID(N'[dbo].[MunicipioEtiqueta]'))
ALTER TABLE [dbo].[MunicipioEtiqueta] CHECK CONSTRAINT [FK_MunicipioEtiqueta_Etiqueta]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MunicipioEtiqueta_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[MunicipioEtiqueta]'))
ALTER TABLE [dbo].[MunicipioEtiqueta]  WITH CHECK ADD  CONSTRAINT [FK_MunicipioEtiqueta_Municipio] FOREIGN KEY([idMunicipio])
REFERENCES [dbo].[Municipio] ([idMunicipio])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MunicipioEtiqueta_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[MunicipioEtiqueta]'))
ALTER TABLE [dbo].[MunicipioEtiqueta] CHECK CONSTRAINT [FK_MunicipioEtiqueta_Municipio]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MunicipioFechaRelevante_FechaRelevante]') AND parent_object_id = OBJECT_ID(N'[dbo].[MunicipioFechaRelevante]'))
ALTER TABLE [dbo].[MunicipioFechaRelevante]  WITH CHECK ADD  CONSTRAINT [FK_MunicipioFechaRelevante_FechaRelevante] FOREIGN KEY([idFechaRelevante])
REFERENCES [dbo].[FechaRelevante] ([idFechaRelevante])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MunicipioFechaRelevante_FechaRelevante]') AND parent_object_id = OBJECT_ID(N'[dbo].[MunicipioFechaRelevante]'))
ALTER TABLE [dbo].[MunicipioFechaRelevante] CHECK CONSTRAINT [FK_MunicipioFechaRelevante_FechaRelevante]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MunicipioFechaRelevante_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[MunicipioFechaRelevante]'))
ALTER TABLE [dbo].[MunicipioFechaRelevante]  WITH CHECK ADD  CONSTRAINT [FK_MunicipioFechaRelevante_Municipio] FOREIGN KEY([idMunicipio])
REFERENCES [dbo].[Municipio] ([idMunicipio])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_MunicipioFechaRelevante_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[MunicipioFechaRelevante]'))
ALTER TABLE [dbo].[MunicipioFechaRelevante] CHECK CONSTRAINT [FK_MunicipioFechaRelevante_Municipio]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Negocio_Categoria]') AND parent_object_id = OBJECT_ID(N'[dbo].[Negocio]'))
ALTER TABLE [dbo].[Negocio]  WITH CHECK ADD  CONSTRAINT [FK_Negocio_Categoria] FOREIGN KEY([idCategoria])
REFERENCES [dbo].[Categoria] ([idCategoria])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Negocio_Categoria]') AND parent_object_id = OBJECT_ID(N'[dbo].[Negocio]'))
ALTER TABLE [dbo].[Negocio] CHECK CONSTRAINT [FK_Negocio_Categoria]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Negocio_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[Negocio]'))
ALTER TABLE [dbo].[Negocio]  WITH CHECK ADD  CONSTRAINT [FK_Negocio_Municipio] FOREIGN KEY([idMunicipio])
REFERENCES [dbo].[Municipio] ([idMunicipio])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Negocio_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[Negocio]'))
ALTER TABLE [dbo].[Negocio] CHECK CONSTRAINT [FK_Negocio_Municipio]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Negocio_Usuario]') AND parent_object_id = OBJECT_ID(N'[dbo].[Negocio]'))
ALTER TABLE [dbo].[Negocio]  WITH CHECK ADD  CONSTRAINT [FK_Negocio_Usuario] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuario] ([idUsuario])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Negocio_Usuario]') AND parent_object_id = OBJECT_ID(N'[dbo].[Negocio]'))
ALTER TABLE [dbo].[Negocio] CHECK CONSTRAINT [FK_Negocio_Usuario]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_NegocioEtiqueta_Etiqueta]') AND parent_object_id = OBJECT_ID(N'[dbo].[NegocioEtiqueta]'))
ALTER TABLE [dbo].[NegocioEtiqueta]  WITH CHECK ADD  CONSTRAINT [FK_NegocioEtiqueta_Etiqueta] FOREIGN KEY([idEtiqueta])
REFERENCES [dbo].[Etiqueta] ([idEtiqueta])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_NegocioEtiqueta_Etiqueta]') AND parent_object_id = OBJECT_ID(N'[dbo].[NegocioEtiqueta]'))
ALTER TABLE [dbo].[NegocioEtiqueta] CHECK CONSTRAINT [FK_NegocioEtiqueta_Etiqueta]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_NegocioEtiqueta_Negocio]') AND parent_object_id = OBJECT_ID(N'[dbo].[NegocioEtiqueta]'))
ALTER TABLE [dbo].[NegocioEtiqueta]  WITH CHECK ADD  CONSTRAINT [FK_NegocioEtiqueta_Negocio] FOREIGN KEY([idNegocio])
REFERENCES [dbo].[Negocio] ([idNegocio])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_NegocioEtiqueta_Negocio]') AND parent_object_id = OBJECT_ID(N'[dbo].[NegocioEtiqueta]'))
ALTER TABLE [dbo].[NegocioEtiqueta] CHECK CONSTRAINT [FK_NegocioEtiqueta_Negocio]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Notificaciones_Tipos]') AND parent_object_id = OBJECT_ID(N'[dbo].[Notificacion]'))
ALTER TABLE [dbo].[Notificacion]  WITH CHECK ADD  CONSTRAINT [FK_Notificaciones_Tipos] FOREIGN KEY([idTipoNotificacion])
REFERENCES [dbo].[TiposNotificacion] ([idTipoNotificacion])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Notificaciones_Tipos]') AND parent_object_id = OBJECT_ID(N'[dbo].[Notificacion]'))
ALTER TABLE [dbo].[Notificacion] CHECK CONSTRAINT [FK_Notificaciones_Tipos]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Notificaciones_Usuarios]') AND parent_object_id = OBJECT_ID(N'[dbo].[Notificacion]'))
ALTER TABLE [dbo].[Notificacion]  WITH CHECK ADD  CONSTRAINT [FK_Notificaciones_Usuarios] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuario] ([idUsuario])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Notificaciones_Usuarios]') AND parent_object_id = OBJECT_ID(N'[dbo].[Notificacion]'))
ALTER TABLE [dbo].[Notificacion] CHECK CONSTRAINT [FK_Notificaciones_Usuarios]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PreferenciaUsuario_Etiqueta]') AND parent_object_id = OBJECT_ID(N'[dbo].[PreferenciaUsuario]'))
ALTER TABLE [dbo].[PreferenciaUsuario]  WITH CHECK ADD  CONSTRAINT [FK_PreferenciaUsuario_Etiqueta] FOREIGN KEY([idEtiqueta])
REFERENCES [dbo].[Etiqueta] ([idEtiqueta])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PreferenciaUsuario_Etiqueta]') AND parent_object_id = OBJECT_ID(N'[dbo].[PreferenciaUsuario]'))
ALTER TABLE [dbo].[PreferenciaUsuario] CHECK CONSTRAINT [FK_PreferenciaUsuario_Etiqueta]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PreferenciaUsuario_Usuario]') AND parent_object_id = OBJECT_ID(N'[dbo].[PreferenciaUsuario]'))
ALTER TABLE [dbo].[PreferenciaUsuario]  WITH CHECK ADD  CONSTRAINT [FK_PreferenciaUsuario_Usuario] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuario] ([idUsuario])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PreferenciaUsuario_Usuario]') AND parent_object_id = OBJECT_ID(N'[dbo].[PreferenciaUsuario]'))
ALTER TABLE [dbo].[PreferenciaUsuario] CHECK CONSTRAINT [FK_PreferenciaUsuario_Usuario]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RecomendacionDescartada_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[RecomendacionDescartada]'))
ALTER TABLE [dbo].[RecomendacionDescartada]  WITH CHECK ADD  CONSTRAINT [FK_RecomendacionDescartada_Municipio] FOREIGN KEY([idMunicipio])
REFERENCES [dbo].[Municipio] ([idMunicipio])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RecomendacionDescartada_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[RecomendacionDescartada]'))
ALTER TABLE [dbo].[RecomendacionDescartada] CHECK CONSTRAINT [FK_RecomendacionDescartada_Municipio]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RecomendacionDescartada_Usuario]') AND parent_object_id = OBJECT_ID(N'[dbo].[RecomendacionDescartada]'))
ALTER TABLE [dbo].[RecomendacionDescartada]  WITH CHECK ADD  CONSTRAINT [FK_RecomendacionDescartada_Usuario] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuario] ([idUsuario])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_RecomendacionDescartada_Usuario]') AND parent_object_id = OBJECT_ID(N'[dbo].[RecomendacionDescartada]'))
ALTER TABLE [dbo].[RecomendacionDescartada] CHECK CONSTRAINT [FK_RecomendacionDescartada_Usuario]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Reseña_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[Reseña]'))
ALTER TABLE [dbo].[Reseña]  WITH CHECK ADD  CONSTRAINT [FK_Reseña_Municipio] FOREIGN KEY([idMunicipio])
REFERENCES [dbo].[Municipio] ([idMunicipio])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Reseña_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[Reseña]'))
ALTER TABLE [dbo].[Reseña] CHECK CONSTRAINT [FK_Reseña_Municipio]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Reseña_Negocio]') AND parent_object_id = OBJECT_ID(N'[dbo].[Reseña]'))
ALTER TABLE [dbo].[Reseña]  WITH CHECK ADD  CONSTRAINT [FK_Reseña_Negocio] FOREIGN KEY([idNegocio])
REFERENCES [dbo].[Negocio] ([idNegocio])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Reseña_Negocio]') AND parent_object_id = OBJECT_ID(N'[dbo].[Reseña]'))
ALTER TABLE [dbo].[Reseña] CHECK CONSTRAINT [FK_Reseña_Negocio]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Reseña_SitiosTuristicos]') AND parent_object_id = OBJECT_ID(N'[dbo].[Reseña]'))
ALTER TABLE [dbo].[Reseña]  WITH CHECK ADD  CONSTRAINT [FK_Reseña_SitiosTuristicos] FOREIGN KEY([idSitiosTuristicos])
REFERENCES [dbo].[SitiosTuristicos] ([idSitiosTuristicos])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Reseña_SitiosTuristicos]') AND parent_object_id = OBJECT_ID(N'[dbo].[Reseña]'))
ALTER TABLE [dbo].[Reseña] CHECK CONSTRAINT [FK_Reseña_SitiosTuristicos]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Reseña_Usuario]') AND parent_object_id = OBJECT_ID(N'[dbo].[Reseña]'))
ALTER TABLE [dbo].[Reseña]  WITH CHECK ADD  CONSTRAINT [FK_Reseña_Usuario] FOREIGN KEY([idUsuario])
REFERENCES [dbo].[Usuario] ([idUsuario])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Reseña_Usuario]') AND parent_object_id = OBJECT_ID(N'[dbo].[Reseña]'))
ALTER TABLE [dbo].[Reseña] CHECK CONSTRAINT [FK_Reseña_Usuario]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_SitiosTuristicos_Categoria]') AND parent_object_id = OBJECT_ID(N'[dbo].[SitiosTuristicos]'))
ALTER TABLE [dbo].[SitiosTuristicos]  WITH CHECK ADD  CONSTRAINT [FK_SitiosTuristicos_Categoria] FOREIGN KEY([idCategoria])
REFERENCES [dbo].[Categoria] ([idCategoria])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_SitiosTuristicos_Categoria]') AND parent_object_id = OBJECT_ID(N'[dbo].[SitiosTuristicos]'))
ALTER TABLE [dbo].[SitiosTuristicos] CHECK CONSTRAINT [FK_SitiosTuristicos_Categoria]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_SitiosTuristicos_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[SitiosTuristicos]'))
ALTER TABLE [dbo].[SitiosTuristicos]  WITH CHECK ADD  CONSTRAINT [FK_SitiosTuristicos_Municipio] FOREIGN KEY([idMunicipio])
REFERENCES [dbo].[Municipio] ([idMunicipio])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_SitiosTuristicos_Municipio]') AND parent_object_id = OBJECT_ID(N'[dbo].[SitiosTuristicos]'))
ALTER TABLE [dbo].[SitiosTuristicos] CHECK CONSTRAINT [FK_SitiosTuristicos_Municipio]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Usuarios_Municipios]') AND parent_object_id = OBJECT_ID(N'[dbo].[Usuario]'))
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuarios_Municipios] FOREIGN KEY([municipioAsignadoId])
REFERENCES [dbo].[Municipio] ([idMunicipio])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Usuarios_Municipios]') AND parent_object_id = OBJECT_ID(N'[dbo].[Usuario]'))
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuarios_Municipios]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Usuarios_Roles]') AND parent_object_id = OBJECT_ID(N'[dbo].[Usuario]'))
ALTER TABLE [dbo].[Usuario]  WITH CHECK ADD  CONSTRAINT [FK_Usuarios_Roles] FOREIGN KEY([idRol])
REFERENCES [dbo].[Rol] ([idRol])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Usuarios_Roles]') AND parent_object_id = OBJECT_ID(N'[dbo].[Usuario]'))
ALTER TABLE [dbo].[Usuario] CHECK CONSTRAINT [FK_Usuarios_Roles]
GO
