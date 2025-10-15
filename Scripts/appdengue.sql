-- --------------------------------------------------------
-- Host:                         158.220.123.106
-- Versión del servidor:         8.0.39 - MySQL Community Server - GPL
-- SO del servidor:              Win64
-- HeidiSQL Versión:             12.10.0.7000
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Volcando estructura de base de datos para app_dengue
CREATE DATABASE IF NOT EXISTS `app_dengue` /*!40100 DEFAULT CHARACTER SET utf8mb3 */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `app_dengue`;

-- Volcando estructura para procedimiento app_dengue.ActualizarCaso
DELIMITER //
CREATE PROCEDURE `ActualizarCaso`(
	IN `idc` INT,
	IN `descri` VARCHAR(255),
	IN `estadoc` INT,
	IN `ihospital` INT,
	IN `tdengue` INT
)
BEGIN
UPDATE casoreportado
SET casoreportado.DESCRIPCION_CASOREPORTADO = descri,casoreportado.FK_ID_HOSPITAL=ihospital,casoreportado.FK_ID_TIPODENGUE = tdengue, casoreportado.FK_ID_ESTADOCASO = estadoc
WHERE casoreportado.ID_CASOREPORTADO= idc;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.CantidadCasoHospital
DELIMITER //
CREATE PROCEDURE `CantidadCasoHospital`()
BEGIN

SELECT hospital.ID_HOSPITAL, hospital.NOMBRE_HOSPITAL,hospital.ESTADO_HOSPITAL,hospital.DIRECCION_HOSPITAL,hospital.LATITUD_HOSPITAL,hospital.LONGITUD_HOSPITAL,hospital.FK_ID_MUNICIPIO,hospital.IMAGEN_HOSPITAL, COUNT(casoreportado.ID_CASOREPORTADO) AS CANTIDADCASOS_HOSPITAL
FROM hospital 
left JOIN casoreportado ON casoreportado.FK_ID_HOSPITAL = hospital.ID_HOSPITAL
GROUP BY hospital.NOMBRE_HOSPITAL;
END//
DELIMITER ;

-- Volcando estructura para tabla app_dengue.casoreportado
CREATE TABLE IF NOT EXISTS `casoreportado` (
  `ID_CASOREPORTADO` int NOT NULL AUTO_INCREMENT,
  `DESCRIPCION_CASOREPORTADO` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `FECHA_CASOREPORTADO` date NOT NULL DEFAULT (now()),
  `FECHAFINALIZACION_CASO` date DEFAULT NULL,
  `FK_ID_ESTADOCASO` int DEFAULT NULL,
  `FK_ID_HOSPITAL` int DEFAULT NULL,
  `FK_ID_TIPODENGUE` int DEFAULT NULL,
  `FK_ID_PACIENTE` int DEFAULT NULL,
  `FK_ID_PERSONALMEDICO` int DEFAULT NULL,
  `DIRECCION_CASOREPORTADO` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`ID_CASOREPORTADO`),
  KEY `3_idx` (`FK_ID_HOSPITAL`),
  KEY `6_idx` (`FK_ID_TIPODENGUE`),
  KEY `5_idx` (`FK_ID_PACIENTE`),
  KEY `15_idx` (`FK_ID_PERSONALMEDICO`),
  KEY `FK_ID_CASOREPORTADO` (`FK_ID_ESTADOCASO`),
  CONSTRAINT `3` FOREIGN KEY (`FK_ID_HOSPITAL`) REFERENCES `hospital` (`ID_HOSPITAL`),
  CONSTRAINT `6` FOREIGN KEY (`FK_ID_TIPODENGUE`) REFERENCES `tipodengue` (`ID_TIPODENGUE`),
  CONSTRAINT `FK_casoreportado_estadocaso` FOREIGN KEY (`FK_ID_ESTADOCASO`) REFERENCES `estadocaso` (`ID_ESTADOCASO`),
  CONSTRAINT `FK_casoreportado_usuario` FOREIGN KEY (`FK_ID_PACIENTE`) REFERENCES `usuario` (`ID_USUARIO`),
  CONSTRAINT `FK_casoreportado_usuario_2` FOREIGN KEY (`FK_ID_PERSONALMEDICO`) REFERENCES `usuario` (`ID_USUARIO`)
) ENGINE=InnoDB AUTO_INCREMENT=50 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.casoreportado: ~2 rows (aproximadamente)
INSERT INTO `casoreportado` (`ID_CASOREPORTADO`, `DESCRIPCION_CASOREPORTADO`, `FECHA_CASOREPORTADO`, `FECHAFINALIZACION_CASO`, `FK_ID_ESTADOCASO`, `FK_ID_HOSPITAL`, `FK_ID_TIPODENGUE`, `FK_ID_PACIENTE`, `FK_ID_PERSONALMEDICO`, `DIRECCION_CASOREPORTADO`) VALUES
	(48, 'prueba', '2025-05-27', NULL, 3, 6, 1, 10, 1, '1.6180698:-75.6136495'),
	(49, 'prueba', '2025-05-27', NULL, 4, 6, 3, 3, 1, '1.6180698:-75.6136495');

-- Volcando estructura para procedimiento app_dengue.CasosXHospital
DELIMITER //
CREATE PROCEDURE `CasosXHospital`(
	IN `idh` INT
)
BEGIN
SELECT casoreportado.ID_CASOREPORTADO,PACIENTE.NOMBRE_USUARIO AS NOMBRE_PACIENTE, estadocaso.NOMBRE_ESTADOCASO FROM casoreportado
INNER JOIN usuario AS PACIENTE ON casoreportado.FK_ID_PACIENTE =PACIENTE.ID_USUARIO
INNER JOIN estadocaso ON casoreportado.FK_ID_ESTADOCASO= estadocaso.ID_ESTADOCASO
WHERE casoreportado.FK_ID_ESTADOCASO != 2 AND casoreportado.FK_ID_ESTADOCASO != 5 AND casoreportado.FK_ID_HOSPITAL = idh;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.CrearCaso
DELIMITER //
CREATE PROCEDURE `CrearCaso`(
	IN `descri` VARCHAR(250),
	IN `ihospital` INT,
	IN `tdengue` INT,
	IN `paciente` INT,
	IN `personalmedico` INT,
	IN `direccion` VARCHAR(50)
)
BEGIN

DECLARE estado INT;

if tdengue = 1
then set estado = 3;
ELSEIF tdengue = 2
then set estado = 1;
ELSE set estado = 4;
END if;

INSERT INTO casoreportado (casoreportado.DESCRIPCION_CASOREPORTADO,casoreportado.FK_ID_HOSPITAL,casoreportado.FK_ID_TIPODENGUE,casoreportado.FK_ID_PACIENTE,casoreportado.FK_ID_PERSONALMEDICO,casoreportado.DIRECCION_CASOREPORTADO,casoreportado.FK_ID_ESTADOCASO)
VALUES (descri,ihospital,tdengue,paciente,personalmedico,direccion,estado);


END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.CrearHospital
DELIMITER //
CREATE PROCEDURE `CrearHospital`(
	IN `nombre` VARCHAR(50),
	IN `direccion` VARCHAR(50),
	IN `lat` VARCHAR(50),
	IN `lon` VARCHAR(50),
	IN `muni` INT,
	IN `imagen` VARCHAR(50)
)
BEGIN

INSERT hospital (hospital.NOMBRE_HOSPITAL,hospital.DIRECCION_HOSPITAL,hospital.LATITUD_HOSPITAL,hospital.LONGITUD_HOSPITAL,hospital.FK_ID_MUNICIPIO,hospital.IMAGEN_HOSPITAL)
VALUES (nombre,direccion,lat,lon,muni,imagen);

END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.CrearPublicacion
DELIMITER //
CREATE PROCEDURE `CrearPublicacion`(
	IN `titulo` VARCHAR(50),
	IN `idi` VARCHAR(50),
	IN `descri` VARCHAR(1500),
	IN `usua` INT
)
BEGIN
INSERT INTO publicacion (publicacion.TITULO_PUBLICACION,publicacion.IMAGEN_PUBLICACION,publicacion.DESCRIPCION_PUBLICACION,publicacion.FK_ID_USUARIO)
VALUES (titulo,idi,descri,usua);
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.CrearUsuario
DELIMITER //
CREATE PROCEDURE `CrearUsuario`(
	IN `nomu` VARCHAR(50),
	IN `correou` VARCHAR(100),
	IN `diru` VARCHAR(50),
	IN `rolu` INT,
	IN `muniu` INT,
	IN `tiposangreu` INT,
	IN `genu` INT
)
BEGIN
INSERT INTO usuario(usuario.NOMBRE_USUARIO,usuario.CORREO_USUARIO,usuario.CONTRASENIA_USUARIO,usuario.DIRECCION_USUARIO,usuario.FK_ID_ROL,usuario.FK_ID_MUNICIPIO,usuario.FK_ID_TIPOSANGRE,usuario.FK_ID_GENERO)
VALUES(nomu,correou,correou,diru,rolu,muniu,tiposangreu,genu);
END//
DELIMITER ;

-- Volcando estructura para tabla app_dengue.departamento
CREATE TABLE IF NOT EXISTS `departamento` (
  `ID_DEPARTAMENTO` int NOT NULL AUTO_INCREMENT,
  `NOMBRE_DEPARTAMENTO` varchar(45) NOT NULL,
  PRIMARY KEY (`ID_DEPARTAMENTO`)
) ENGINE=InnoDB AUTO_INCREMENT=35 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.departamento: ~32 rows (aproximadamente)
INSERT INTO `departamento` (`ID_DEPARTAMENTO`, `NOMBRE_DEPARTAMENTO`) VALUES
	(1, 'Amazonas'),
	(2, 'Antioquia'),
	(3, 'Arauca'),
	(4, 'Atlántico'),
	(5, 'Bolívar'),
	(6, 'Boyacá'),
	(7, 'Caldas'),
	(8, 'Caquetá'),
	(10, 'Cauca'),
	(11, 'Cesar'),
	(12, 'Chocó'),
	(13, 'Córdoba'),
	(14, 'Cundinamarca'),
	(16, 'Guaviare'),
	(17, 'Guajira'),
	(18, 'Huila'),
	(19, 'La Guajira'),
	(20, 'Magdalena'),
	(21, 'Meta'),
	(22, 'Nariño'),
	(23, 'Norte de Santander'),
	(24, 'Putumayo'),
	(25, 'Quindío'),
	(26, 'Risaralda'),
	(27, 'San Andrés y Providencia'),
	(28, 'Santa Marta'),
	(29, 'Santander'),
	(30, 'Sucre'),
	(31, 'Tolima'),
	(32, 'Valle del Cauca'),
	(33, 'Vaupés'),
	(34, 'Vichada');

-- Volcando estructura para procedimiento app_dengue.EditarCaso
DELIMITER //
CREATE PROCEDURE `EditarCaso`(
	IN `ID_CASO` INT,
	IN `ID_ESTADOCASO` INT,
	IN `ID_TIPODENGUE` INT,
	IN `DESCRIPCION` VARCHAR(255)
)
BEGIN
    UPDATE CASOREPORTADO
    SET 
        FK_ID_ESTADOCASO = CASE WHEN ID_ESTADOCASO IS NOT NULL THEN ID_ESTADOCASO ELSE FK_ID_ESTADOCASO END,
        FK_ID_TIPODENGUE = CASE WHEN ID_TIPODENGUE IS NOT NULL THEN ID_TIPODENGUE ELSE FK_ID_TIPODENGUE END,
        DESCRIPCION_CASOREPORTADO = CASE WHEN DESCRIPCION IS NOT NULL THEN DESCRIPCION ELSE DESCRIPCION_CASOREPORTADO END
    WHERE ID_CASOREPORTADO = ID_CASO;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.EditarHospital
DELIMITER //
CREATE PROCEDURE `EditarHospital`(
	IN `idh` INT,
	IN `nombre` VARCHAR(50),
	IN `imagenN` VARCHAR(50)
)
BEGIN

	UPDATE hospital
	SET hospital.NOMBRE_HOSPITAL = nombre, hospital.IMAGEN_HOSPITAL = imagen
	WHERE hospital.ID_HOSPITAL = idh;

END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.EditarPublicacion
DELIMITER //
CREATE PROCEDURE `EditarPublicacion`(
	IN `idp` INT,
	IN `titulo` VARCHAR(50),
	IN `descri` VARCHAR(50)
)
BEGIN
UPDATE publicacion
SET publicacion.TITULO_PUBLICACION = titulo,publicacion.DESCRIPCION_PUBLICACION =descri
WHERE publicacion.ID_PUBLICACION=idp;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.EditarUsuario
DELIMITER //
CREATE PROCEDURE `EditarUsuario`(
	IN `idu` INT,
	IN `nombre` VARCHAR(50),
	IN `correo` VARCHAR(50),
	IN `dire` VARCHAR(50),
	IN `rolu` INT,
	IN `muni` INT,
	IN `gene` INT
)
BEGIN
UPDATE usuario
SET usuario.NOMBRE_USUARIO=nombre,usuario.CORREO_USUARIO=correo,usuario.DIRECCION_USUARIO=dire,usuario.FK_ID_MUNICIPIO=muni,usuario.FK_ID_ROL=rolu,usuario.FK_ID_GENERO=gene
WHERE usuario.ID_USUARIO=idu;
END//
DELIMITER ;

-- Volcando estructura para tabla app_dengue.estadocaso
CREATE TABLE IF NOT EXISTS `estadocaso` (
  `ID_ESTADOCASO` int NOT NULL AUTO_INCREMENT,
  `NOMBRE_ESTADOCASO` varchar(45) NOT NULL,
  `ESTADO_ESTADOCASO` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`ID_ESTADOCASO`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.estadocaso: ~5 rows (aproximadamente)
INSERT INTO `estadocaso` (`ID_ESTADOCASO`, `NOMBRE_ESTADOCASO`, `ESTADO_ESTADOCASO`) VALUES
	(1, 'En cama', 1),
	(2, 'finalizado', 1),
	(3, 'En tratamiento', 1),
	(4, 'Hospitalizado', 1),
	(5, 'muerte por dengue', 1);

-- Volcando estructura para tabla app_dengue.estadousuario
CREATE TABLE IF NOT EXISTS `estadousuario` (
  `ID_ESTADOUSUARIO` int NOT NULL AUTO_INCREMENT,
  `NOMBRE_ESTADOUSUARIO` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  PRIMARY KEY (`ID_ESTADOUSUARIO`) USING BTREE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.estadousuario: ~3 rows (aproximadamente)
INSERT INTO `estadousuario` (`ID_ESTADOUSUARIO`, `NOMBRE_ESTADOUSUARIO`) VALUES
	(1, 'Enfermo'),
	(2, 'Vivo'),
	(3, 'Muerto');

-- Volcando estructura para procedimiento app_dengue.Filtrar
DELIMITER //
CREATE PROCEDURE `Filtrar`(
	IN `filtro` VARCHAR(50)
)
BEGIN
SELECT DISTINCT usuario.ID_USUARIO,usuario.NOMBRE_USUARIO,usuario.CORREO_USUARIO,usuario.CONTRASENIA_USUARIO,usuario.DIRECCION_USUARIO,usuario.FK_ID_ROL,rol.NOMBRE_ROL,usuario.FK_ID_MUNICIPIO,municipio.NOMBRE_MUNICIPIO,usuario.FK_ID_TIPOSANGRE,tiposangre.NOMBRE_TIPOSANGRE,usuario.FK_ID_GENERO,genero.NOMBRE_GENERO FROM usuario
INNER JOIN rol ON usuario.FK_ID_ROL= rol.ID_ROL
INNER JOIN municipio ON usuario.FK_ID_MUNICIPIO=municipio.ID_MUNICIPIO
INNER JOIN tiposangre ON usuario.FK_ID_TIPOSANGRE=tiposangre.ID_TIPOSANGRE
INNER JOIN genero ON usuario.FK_ID_GENERO= genero.ID_GENERO

WHERE usuario.NOMBRE_USUARIO LIKE CONCAT('%', filtro, '%') AND usuario.FK_ID_ESTADOUSUARIO =2;

END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.FiltrarHospital
DELIMITER //
CREATE PROCEDURE `FiltrarHospital`(
	IN `nombre` VARCHAR(50)
)
BEGIN
    IF nombre IS NULL OR nombre = '' THEN
        SELECT * FROM hospital;
    ELSE
        SELECT * FROM hospital
        WHERE hospital.NOMBRE_HOSPITAL LIKE CONCAT('%', nombre, '%');
    END IF;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.Filtrarpublicacion
DELIMITER //
CREATE PROCEDURE `Filtrarpublicacion`(
	IN `nombre` VARCHAR(50)
)
BEGIN
SELECT publicacion.ID_PUBLICACION,publicacion.TITULO_PUBLICACION,publicacion.IMAGEN_PUBLICACION,publicacion.DESCRIPCION_PUBLICACION,publicacion.FECHA_PUBLICACION,publicacion.FK_ID_USUARIO,usuario.NOMBRE_USUARIO FROM publicacion
	INNER JOIN usuario ON publicacion.FK_ID_USUARIO = usuario.ID_USUARIO
	WHERE publicacion.TITULO_PUBLICACION LIKE CONCAT('%', nombre, '%');
END//
DELIMITER ;

-- Volcando estructura para tabla app_dengue.genero
CREATE TABLE IF NOT EXISTS `genero` (
  `ID_GENERO` int NOT NULL AUTO_INCREMENT,
  `NOMBRE_GENERO` varchar(45) NOT NULL,
  `ESTADO_GENERO` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`ID_GENERO`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.genero: ~3 rows (aproximadamente)
INSERT INTO `genero` (`ID_GENERO`, `NOMBRE_GENERO`, `ESTADO_GENERO`) VALUES
	(1, 'Masculino', 1),
	(2, 'Femenino', 1),
	(3, 'No binario', 1);

-- Volcando estructura para procedimiento app_dengue.HistorialCasos
DELIMITER //
CREATE PROCEDURE `HistorialCasos`(
	IN `idp` INT
)
BEGIN
	SELECT casoreportado.ID_CASOREPORTADO,
	casoreportado.DESCRIPCION_CASOREPORTADO,
	casoreportado.FECHA_CASOREPORTADO,
	casoreportado.FK_ID_ESTADOCASO,
	estadocaso.NOMBRE_ESTADOCASO,
	casoreportado.FK_ID_HOSPITAL,
	casoreportado.FK_ID_TIPODENGUE,
	casoreportado.FK_ID_PACIENTE,
	paciente.NOMBRE_USUARIO AS NOMBRE_PACIENTE,
	casoreportado.FK_ID_PERSONALMEDICO,
	medico.NOMBRE_USUARIO AS NOMBRE_PERSONALMEDICO ,casoreportado.FECHAFINALIZACION_CASO FROM casoreportado
INNER join usuario AS paciente  ON casoreportado.FK_ID_PACIENTE = paciente.ID_USUARIO
INNER JOIN usuario AS medico ON casoreportado.FK_ID_PERSONALMEDICO = medico.ID_USUARIO
INNER JOIN estadocaso ON casoreportado.FK_ID_ESTADOCASO = estadocaso.ID_ESTADOCASO
	WHERE casoreportado.FK_ID_PACIENTE =  idp;
END//
DELIMITER ;

-- Volcando estructura para tabla app_dengue.hospital
CREATE TABLE IF NOT EXISTS `hospital` (
  `ID_HOSPITAL` int NOT NULL AUTO_INCREMENT,
  `NOMBRE_HOSPITAL` varchar(45) NOT NULL,
  `ESTADO_HOSPITAL` tinyint NOT NULL DEFAULT '1',
  `DIRECCION_HOSPITAL` varchar(45) NOT NULL,
  `LATITUD_HOSPITAL` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `LONGITUD_HOSPITAL` varchar(100) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `FK_ID_MUNICIPIO` int DEFAULT NULL,
  `IMAGEN_HOSPITAL` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`ID_HOSPITAL`),
  KEY `13_idx` (`FK_ID_MUNICIPIO`) USING BTREE,
  CONSTRAINT `13` FOREIGN KEY (`FK_ID_MUNICIPIO`) REFERENCES `municipio` (`ID_MUNICIPIO`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.hospital: ~4 rows (aproximadamente)
INSERT INTO `hospital` (`ID_HOSPITAL`, `NOMBRE_HOSPITAL`, `ESTADO_HOSPITAL`, `DIRECCION_HOSPITAL`, `LATITUD_HOSPITAL`, `LONGITUD_HOSPITAL`, `FK_ID_MUNICIPIO`, `IMAGEN_HOSPITAL`) VALUES
	(1, 'Hospital Maria Inmaculada E.S.E', 1, 'Diagonal 20 # 7-29', '1.6199426474047687', '-75.61123518689837', 1, '663bad7193963e0a02537339'),
	(2, 'E.S.E. Hospital Comunal las Malvinas', 1, 'Av Circunvalar Calle #4', '1.6123542763833667', '-75.6055220942673', 1, '663bb00693963e0a0253733a'),
	(3, 'Medilaser', 1, 'Calle 6 #- a -91, Cl. 14 #1457', '1.608810', '-75.609798', 1, '664f1a11d10edc5e3477727b'),
	(6, 'Salucoop', 1, 'carrera 11', '1.612045', '-75.610466', 1, '66504b60c218677a9c6ab40b');

-- Volcando estructura para procedimiento app_dengue.InsertarUsuario
DELIMITER //
CREATE PROCEDURE `InsertarUsuario`(
	IN `p_nombre` VARCHAR(45),
	IN `p_correo` VARCHAR(45),
	IN `p_contrasenia` VARCHAR(45),
	IN `p_direccion` VARCHAR(45),
	IN `p_id_rol` INT,
	IN `p_id_municipio` INT,
	IN `p_id_tiposangre` INT,
	IN `p_id_genero` INT
)
BEGIN
    INSERT INTO usuario (NOMBRE_USUARIO, CORREO_USUARIO, CONTRASENIA_USUARIO, DIRECCION_USUARIO, 
                         FK_ID_ROL, FK_ID_MUNICIPIO, FK_ID_TIPOSANGRE, FK_ID_GENERO, FK_ID_ESTADOUSUARIO)
    VALUES (p_nombre, p_correo, p_contrasenia, p_direccion, 
            p_id_rol, p_id_municipio, p_id_tiposangre, p_id_genero, p_id_estado_usuario);
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarCasos
DELIMITER //
CREATE PROCEDURE `ListarCasos`()
BEGIN
SELECT casoreportado.ID_CASOREPORTADO,casoreportado.DESCRIPCION_CASOREPORTADO,casoreportado.FECHA_CASOREPORTADO,casoreportado.FK_ID_ESTADOCASO,estadocaso.NOMBRE_ESTADOCASO,casoreportado.FK_ID_HOSPITAL,casoreportado.FK_ID_TIPODENGUE,casoreportado.FK_ID_PACIENTE,paciente.NOMBRE_USUARIO AS NOMBRE_PACIENTE,casoreportado.FK_ID_PERSONALMEDICO,medico.NOMBRE_USUARIO AS NOMBRE_PERSONALMEDICO,casoreportado.DIRECCION_CASOREPORTADO FROM casoreportado
INNER join usuario AS paciente  ON casoreportado.FK_ID_PACIENTE = paciente.ID_USUARIO
INNER JOIN usuario AS medico ON casoreportado.FK_ID_PERSONALMEDICO = medico.ID_USUARIO
INNER JOIN estadocaso ON casoreportado.FK_ID_ESTADOCASO = estadocaso.ID_ESTADOCASO;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarDepartamento
DELIMITER //
CREATE PROCEDURE `ListarDepartamento`()
BEGIN
SELECT * FROM departamento;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarEstadocaso
DELIMITER //
CREATE PROCEDURE `ListarEstadocaso`()
BEGIN

SELECT * FROM estadocaso;

END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarGenero
DELIMITER //
CREATE PROCEDURE `ListarGenero`()
BEGIN
SELECT * FROM genero;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarHospi
DELIMITER //
CREATE PROCEDURE `ListarHospi`()
BEGIN

SELECT 
  hospital.ID_HOSPITAL, 
  hospital.NOMBRE_HOSPITAL,
  hospital.ESTADO_HOSPITAL,
  hospital.DIRECCION_HOSPITAL,
  hospital.FK_ID_MUNICIPIO,
  hospital.IMAGEN_HOSPITAL,
  COUNT(casoreportado.ID_CASOREPORTADO) AS CANTIDADCASOS_HOSPITAL,
  departamento.ID_DEPARTAMENTO,
  departamento.NOMBRE_DEPARTAMENTO
FROM hospital
INNER JOIN municipio ON hospital.FK_ID_MUNICIPIO = municipio.ID_MUNICIPIO
INNER JOIN departamento ON municipio.FK_ID_DEPARTAMENTO = departamento.ID_DEPARTAMENTO
LEFT JOIN casoreportado ON casoreportado.FK_ID_HOSPITAL = hospital.ID_HOSPITAL
GROUP BY 
  hospital.ID_HOSPITAL,
  hospital.NOMBRE_HOSPITAL,
  hospital.ESTADO_HOSPITAL,
  hospital.DIRECCION_HOSPITAL,
  hospital.FK_ID_MUNICIPIO,
  hospital.IMAGEN_HOSPITAL,
  departamento.ID_DEPARTAMENTO,
  departamento.NOMBRE_DEPARTAMENTO;

END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarHospital
DELIMITER //
CREATE PROCEDURE `ListarHospital`(
	IN `filtro` INT
)
BEGIN

SELECT 
  hospital.ID_HOSPITAL, 
  hospital.NOMBRE_HOSPITAL,
  hospital.ESTADO_HOSPITAL,
  hospital.DIRECCION_HOSPITAL,
  hospital.FK_ID_MUNICIPIO,
  hospital.IMAGEN_HOSPITAL,
  COUNT(casoreportado.ID_CASOREPORTADO) AS CANTIDADCASOS_HOSPITAL,
  departamento.ID_DEPARTAMENTO,
  departamento.NOMBRE_DEPARTAMENTO
FROM hospital
INNER JOIN municipio ON hospital.FK_ID_MUNICIPIO = municipio.ID_MUNICIPIO
INNER JOIN departamento ON municipio.FK_ID_DEPARTAMENTO = departamento.ID_DEPARTAMENTO
LEFT JOIN casoreportado ON casoreportado.FK_ID_HOSPITAL = hospital.ID_HOSPITAL
WHERE hospital.FK_ID_MUNICIPIO = filtro
GROUP BY 
  hospital.ID_HOSPITAL,
  hospital.NOMBRE_HOSPITAL,
  hospital.ESTADO_HOSPITAL,
  hospital.DIRECCION_HOSPITAL,
  hospital.FK_ID_MUNICIPIO,
  hospital.IMAGEN_HOSPITAL,
  departamento.ID_DEPARTAMENTO,
  departamento.NOMBRE_DEPARTAMENTO;
  


END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarMuni
DELIMITER //
CREATE PROCEDURE `ListarMuni`()
BEGIN
SELECT * FROM municipio;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarMunicipio
DELIMITER //
CREATE PROCEDURE `ListarMunicipio`(
	IN `filtro` INT
)
BEGIN
SELECT * FROM municipio
WHERE municipio.FK_ID_DEPARTAMENTO = filtro;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarNotificaciones
DELIMITER //
CREATE PROCEDURE `ListarNotificaciones`()
BEGIN
  SELECT
    n.ID_NOTIFICACION,
    n.FECHA_NOTIFICACION,
    tn.NOMBRE_TIPONOTIFICACION,
    tn.DESCRIPCION_TIPONOTIFICACION
  FROM notificacion AS n
  LEFT JOIN tiponotificacion AS tn
    ON n.FK_ID_TIPONOTIFICACION = tn.ID_TIPONOTIFICACION
  ORDER BY n.FECHA_NOTIFICACION DESC;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarPublicaciones
DELIMITER //
CREATE PROCEDURE `ListarPublicaciones`()
BEGIN
	SELECT publicacion.ID_PUBLICACION,publicacion.TITULO_PUBLICACION,publicacion.IMAGEN_PUBLICACION,publicacion.DESCRIPCION_PUBLICACION,publicacion.FECHA_PUBLICACION,publicacion.FK_ID_USUARIO,usuario.NOMBRE_USUARIO FROM publicacion
	INNER JOIN usuario ON publicacion.FK_ID_USUARIO = usuario.ID_USUARIO;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarRoles
DELIMITER //
CREATE PROCEDURE `ListarRoles`()
BEGIN
SELECT * FROM rol;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarSintomas
DELIMITER //
CREATE PROCEDURE `ListarSintomas`()
BEGIN
SELECT * FROM sintoma;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarTIpoDengue
DELIMITER //
CREATE PROCEDURE `ListarTIpoDengue`()
BEGIN
SELECT * FROM tipodengue;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarTipoSangre
DELIMITER //
CREATE PROCEDURE `ListarTipoSangre`()
BEGIN
SELECT * FROM tiposangre
WHERE tiposangre.ESTADO_TIPOSANGRE = 1;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarUsuarios
DELIMITER //
CREATE PROCEDURE `ListarUsuarios`()
BEGIN
SELECT usuario.ID_USUARIO,usuario.NOMBRE_USUARIO,usuario.CORREO_USUARIO,usuario.CONTRASENIA_USUARIO,usuario.DIRECCION_USUARIO,usuario.FK_ID_ROL,rol.NOMBRE_ROL,usuario.FK_ID_MUNICIPIO,municipio.NOMBRE_MUNICIPIO,usuario.FK_ID_TIPOSANGRE,tiposangre.NOMBRE_TIPOSANGRE,usuario.FK_ID_GENERO,genero.NOMBRE_GENERO,estadousuario.NOMBRE_ESTADOUSUARIO FROM usuario
INNER JOIN rol ON usuario.FK_ID_ROL= rol.ID_ROL
INNER JOIN municipio ON usuario.FK_ID_MUNICIPIO=municipio.ID_MUNICIPIO
INNER JOIN tiposangre ON usuario.FK_ID_TIPOSANGRE=tiposangre.ID_TIPOSANGRE
INNER JOIN genero ON usuario.FK_ID_GENERO= genero.ID_GENERO
INNER JOIN estadousuario ON usuario.FK_ID_ESTADOUSUARIO = estadousuario.ID_ESTADOUSUARIO;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ListarUsuarioSanos
DELIMITER //
CREATE PROCEDURE `ListarUsuarioSanos`()
BEGIN
SELECT usuario.ID_USUARIO,usuario.NOMBRE_USUARIO,usuario.CORREO_USUARIO,usuario.CONTRASENIA_USUARIO,usuario.DIRECCION_USUARIO,usuario.FK_ID_ROL,rol.NOMBRE_ROL,usuario.FK_ID_MUNICIPIO,municipio.NOMBRE_MUNICIPIO,usuario.FK_ID_TIPOSANGRE,tiposangre.NOMBRE_TIPOSANGRE,usuario.FK_ID_GENERO,genero.NOMBRE_GENERO,estadousuario.NOMBRE_ESTADOUSUARIO FROM usuario
INNER JOIN rol ON usuario.FK_ID_ROL= rol.ID_ROL
INNER JOIN municipio ON usuario.FK_ID_MUNICIPIO=municipio.ID_MUNICIPIO
INNER JOIN tiposangre ON usuario.FK_ID_TIPOSANGRE=tiposangre.ID_TIPOSANGRE
INNER JOIN genero ON usuario.FK_ID_GENERO= genero.ID_GENERO
INNER JOIN estadousuario ON usuario.FK_ID_ESTADOUSUARIO = estadousuario.ID_ESTADOUSUARIO
WHERE usuario.FK_ID_ESTADOUSUARIO=2;
END//
DELIMITER ;

-- Volcando estructura para tabla app_dengue.municipio
CREATE TABLE IF NOT EXISTS `municipio` (
  `ID_MUNICIPIO` int NOT NULL AUTO_INCREMENT,
  `NOMBRE_MUNICIPIO` varchar(45) DEFAULT NULL,
  `FK_ID_DEPARTAMENTO` int NOT NULL,
  PRIMARY KEY (`ID_MUNICIPIO`),
  KEY `10_idx` (`FK_ID_DEPARTAMENTO`),
  CONSTRAINT `10` FOREIGN KEY (`FK_ID_DEPARTAMENTO`) REFERENCES `departamento` (`ID_DEPARTAMENTO`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.municipio: ~2 rows (aproximadamente)
INSERT INTO `municipio` (`ID_MUNICIPIO`, `NOMBRE_MUNICIPIO`, `FK_ID_DEPARTAMENTO`) VALUES
	(1, 'Florencia', 8),
	(2, 'Cali', 32);

-- Volcando estructura para tabla app_dengue.notificacion
CREATE TABLE IF NOT EXISTS `notificacion` (
  `ID_NOTIFICACION` int NOT NULL AUTO_INCREMENT,
  `FECHA_NOTIFICACION` varchar(45) NOT NULL,
  `FK_ID_TIPONOTIFICACION` int DEFAULT NULL,
  PRIMARY KEY (`ID_NOTIFICACION`),
  KEY `9_idx` (`FK_ID_TIPONOTIFICACION`),
  CONSTRAINT `9` FOREIGN KEY (`FK_ID_TIPONOTIFICACION`) REFERENCES `tiponotificacion` (`ID_TIPONOTIFICACION`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.notificacion: ~6 rows (aproximadamente)
INSERT INTO `notificacion` (`ID_NOTIFICACION`, `FECHA_NOTIFICACION`, `FK_ID_TIPONOTIFICACION`) VALUES
	(1, '2025-05-22 05:05:52', 3),
	(2, '2025-05-22 05:17:43', 3),
	(3, '2025-05-22 05:19:42', 1),
	(4, '2025-05-23 17:02:58', 1),
	(5, '2025-05-27 00:31:04', 1),
	(6, '2025-05-27 00:38:45', 1),
	(7, '2025-05-27 14:37:31', 1);

-- Volcando estructura para procedimiento app_dengue.ObtenerCaso
DELIMITER //
CREATE PROCEDURE `ObtenerCaso`(
	IN `idc` INT
)
BEGIN

SELECT casoreportado.ID_CASOREPORTADO,casoreportado.DESCRIPCION_CASOREPORTADO,casoreportado.FECHA_CASOREPORTADO,casoreportado.FK_ID_ESTADOCASO, estadocaso.NOMBRE_ESTADOCASO,departamento.ID_DEPARTAMENTO, municipio.ID_MUNICIPIO,casoreportado.FK_ID_HOSPITAL,casoreportado.FK_ID_TIPODENGUE,tipodengue.NOMBRE_TIPODENGUE,casoreportado.FK_ID_PACIENTE,paciente.NOMBRE_USUARIO AS NOMBRE_PACIENTE,casoreportado.FK_ID_PERSONALMEDICO,medico.NOMBRE_USUARIO AS NOMBRE_PERSONALMEDICO FROM casoreportado
INNER join usuario AS paciente  ON casoreportado.FK_ID_PACIENTE = paciente.ID_USUARIO
INNER JOIN usuario AS medico ON casoreportado.FK_ID_PERSONALMEDICO = medico.ID_USUARIO
INNER JOIN hospital ON casoreportado.FK_ID_HOSPITAL = hospital.ID_HOSPITAL
INNER JOIN municipio ON hospital.FK_ID_MUNICIPIO=municipio.ID_MUNICIPIO
INNER JOIN departamento ON municipio.FK_ID_DEPARTAMENTO = departamento.ID_DEPARTAMENTO
INNER JOIN estadocaso ON casoreportado.FK_ID_ESTADOCASO = estadocaso.ID_ESTADOCASO
INNER JOIN tipodengue ON casoreportado.FK_ID_TIPODENGUE = tipodengue.ID_TIPODENGUE
	WHERE casoreportado.ID_CASOREPORTADO = idc;
	
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ObtenerHospital
DELIMITER //
CREATE PROCEDURE `ObtenerHospital`(
	IN `idh` INT
)
BEGIN

SELECT hospital.ID_HOSPITAL, hospital.NOMBRE_HOSPITAL,hospital.ESTADO_HOSPITAL,hospital.DIRECCION_HOSPITAL,hospital.FK_ID_MUNICIPIO,hospital.IMAGEN_HOSPITAL,COUNT(casoreportado.ID_CASOREPORTADO) AS CANTIDADCASOS_HOSPITAL,departamento.ID_DEPARTAMENTO FROM hospital
INNER JOIN municipio ON  hospital.FK_ID_MUNICIPIO = municipio.ID_MUNICIPIO
INNER JOIN departamento ON municipio.FK_ID_DEPARTAMENTO =  departamento.ID_DEPARTAMENTO
LEFT JOIN casoreportado ON casoreportado.FK_ID_HOSPITAL = hospital.ID_HOSPITAL
WHERE hospital.ID_HOSPITAL =  idh
GROUP BY hospital.NOMBRE_HOSPITAL;

END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ObtenerPublicacion
DELIMITER //
CREATE PROCEDURE `ObtenerPublicacion`(
	IN `idp` INT
)
BEGIN
SELECT publicacion.ID_PUBLICACION,publicacion.TITULO_PUBLICACION, publicacion.IMAGEN_PUBLICACION,publicacion.DESCRIPCION_PUBLICACION,publicacion.FECHA_PUBLICACION,usuario.NOMBRE_USUARIO
FROM publicacion
INNER JOIN usuario ON publicacion.FK_ID_USUARIO =  usuario.ID_USUARIO
WHERE publicacion.ID_PUBLICACION= idp;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.ObtenerUsuario
DELIMITER //
CREATE PROCEDURE `ObtenerUsuario`(
	IN `idu` INT
)
BEGIN
SELECT usuario.ID_USUARIO,usuario.NOMBRE_USUARIO,usuario.CORREO_USUARIO,usuario.CONTRASENIA_USUARIO,usuario.DIRECCION_USUARIO,usuario.FK_ID_ROL,rol.NOMBRE_ROL,usuario.FK_ID_MUNICIPIO,municipio.NOMBRE_MUNICIPIO,usuario.FK_ID_TIPOSANGRE,tiposangre.NOMBRE_TIPOSANGRE,usuario.FK_ID_GENERO,genero.NOMBRE_GENERO,departamento.ID_DEPARTAMENTO FROM usuario
INNER JOIN rol ON usuario.FK_ID_ROL= rol.ID_ROL
INNER JOIN municipio ON usuario.FK_ID_MUNICIPIO=municipio.ID_MUNICIPIO
INNER JOIN departamento ON municipio.FK_ID_DEPARTAMENTO = departamento.ID_DEPARTAMENTO
INNER JOIN tiposangre ON usuario.FK_ID_TIPOSANGRE=tiposangre.ID_TIPOSANGRE
INNER JOIN genero ON usuario.FK_ID_GENERO= genero.ID_GENERO
WHERE usuario.ID_USUARIO=idu;
END//
DELIMITER ;

-- Volcando estructura para tabla app_dengue.publicacion
CREATE TABLE IF NOT EXISTS `publicacion` (
  `ID_PUBLICACION` int NOT NULL AUTO_INCREMENT,
  `TITULO_PUBLICACION` varchar(45) NOT NULL,
  `IMAGEN_PUBLICACION` varchar(50) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `DESCRIPCION_PUBLICACION` varchar(1500) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `FECHA_PUBLICACION` date NOT NULL DEFAULT (now()),
  `FK_ID_USUARIO` int DEFAULT NULL,
  PRIMARY KEY (`ID_PUBLICACION`),
  KEY `7_idx` (`FK_ID_USUARIO`),
  CONSTRAINT `FK_publicacion_usuario` FOREIGN KEY (`FK_ID_USUARIO`) REFERENCES `usuario` (`ID_USUARIO`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.publicacion: ~9 rows (aproximadamente)
INSERT INTO `publicacion` (`ID_PUBLICACION`, `TITULO_PUBLICACION`, `IMAGEN_PUBLICACION`, `DESCRIPCION_PUBLICACION`, `FECHA_PUBLICACION`, `FK_ID_USUARIO`) VALUES
	(2, 'Cuidados del dengue', '663a6b5ea62761e7747b8693', 'Tips para la prevencion', '2024-05-07', 1),
	(3, 'Prevención contra el dengue', '665032e006221a2ec6224537', 'La medida más importante de prevención es la eliminación de todos los criaderos de mosquitos, es decir, de todos los recipientes u objetos que puedan acumular agua. Por ello, es fundamental: Eliminar todos los recipientes en desuso que puedan acumular agua (como latas, botellas, neumáticos)', '2024-05-24', 1),
	(4, 'waos', '66504bb0c218677a9c6ab40c', 'wapos', '2024-05-24', 1),
	(6, 'a', '682139e56b2fad953b67babb', 'a', '2025-05-11', 1),
	(7, 'a', '68213b005d106988cfbec1c5', 'a', '2025-05-11', 1),
	(8, 'aASDASD', '68214fd0714c086966bae7e2', 'a', '2025-05-11', 1),
	(9, 'PRUEBA', '6821500c714c086966bae7e3', 'a', '2025-05-11', 1),
	(11, 'sada', '6821546f032cff9fcda24768', 'asdadsa', '2025-05-11', 1),
	(12, 'asdads', '68215537032cff9fcda24769', 'sadasda', '2025-05-11', 1),
	(13, 'asdads', '68216ed9681ef7f6074ba36f', 'asdadsad', '2025-05-11', 1),
	(14, 'Dengue en el caqueta', '682ef6ff495f790352963658', 'Se ha evidenciado el brote masivo en el departamento del caqueta, las autoriades recomiendan por favor lavar sus tanques cada semana', '2025-05-22', 1),
	(15, 'prueba', '682ef9c4310ef54fa1929414', 'prueba', '2025-05-22', 1);

-- Volcando estructura para procedimiento app_dengue.RecuperarContra
DELIMITER //
CREATE PROCEDURE `RecuperarContra`(
	IN `correo` VARCHAR(50),
	IN `contra` VARCHAR(50)
)
BEGIN
UPDATE usuario
set usuario.CONTRASENIA_USUARIO = contra
WHERE usuario.CORREO_USUARIO=correo;
END//
DELIMITER ;

-- Volcando estructura para procedimiento app_dengue.RegistrarUsuario
DELIMITER //
CREATE PROCEDURE `RegistrarUsuario`(
	IN `nomu` VARCHAR(50),
	IN `correou` VARCHAR(50),
	IN `contra` VARCHAR(50),
	IN `diru` VARCHAR(50),
	IN `rolu` INT,
	IN `muniu` INT,
	IN `tiposangreu` INT,
	IN `genu` INT
)
BEGIN
    INSERT INTO usuario (NOMBRE_USUARIO, CORREO_USUARIO, CONTRASENIA_USUARIO, DIRECCION_USUARIO, FK_ID_ROL, FK_ID_MUNICIPIO, FK_ID_TIPOSANGRE, FK_ID_GENERO)
    VALUES (nomu, correou, contra, diru, rolu, muniu, tiposangreu, genu);
END//
DELIMITER ;

-- Volcando estructura para tabla app_dengue.rol
CREATE TABLE IF NOT EXISTS `rol` (
  `ID_ROL` int NOT NULL AUTO_INCREMENT,
  `NOMBRE_ROL` varchar(45) NOT NULL,
  `ESTADO_ROL` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`ID_ROL`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.rol: ~3 rows (aproximadamente)
INSERT INTO `rol` (`ID_ROL`, `NOMBRE_ROL`, `ESTADO_ROL`) VALUES
	(1, 'admin', 1),
	(2, 'usuario', 1),
	(3, 'Personal Medico', 1);

-- Volcando estructura para tabla app_dengue.sintoma
CREATE TABLE IF NOT EXISTS `sintoma` (
  `ID_SINTOMA` int NOT NULL AUTO_INCREMENT,
  `NOMBRE_SINTOMA` varchar(150) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci DEFAULT NULL,
  `ESTADO_SINTOMA` tinyint DEFAULT '1',
  PRIMARY KEY (`ID_SINTOMA`)
) ENGINE=InnoDB AUTO_INCREMENT=1123 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.sintoma: ~21 rows (aproximadamente)
INSERT INTO `sintoma` (`ID_SINTOMA`, `NOMBRE_SINTOMA`, `ESTADO_SINTOMA`) VALUES
	(1, 'Fiebre alta', 1),
	(2, 'Fuertes dolores de cabeza', 1),
	(3, 'Dolor detrás de los ojos', 1),
	(4, 'Dolores musculares y articulares', 1),
	(5, 'Cansancio', 1),
	(6, 'Náuseas', 1),
	(7, 'Vómitos', 1),
	(8, 'Erupción cutánea', 1),
	(9, 'Molestia general o malestar', 1),
	(10, 'Dolor abdominal intenso y persistente', 1),
	(11, 'Vómitos persistentes', 1),
	(12, 'Acumulación de líquidos', 1),
	(13, 'Sangrado de mucosas', 1),
	(14, 'Somnolencia o irritabilidad', 1),
	(15, 'Agrandamiento del hígado', 1),
	(16, 'Incremento del hematocrito con disminución de plaquetas', 1),
	(17, 'Falla circulatoria', 1),
	(18, 'Hemorragias severas', 1),
	(19, 'Derrames pleurales', 1),
	(20, 'Shock', 1),
	(21, 'Insuficiencia orgánica', 1);

-- Volcando estructura para tabla app_dengue.sintomatipodengue
CREATE TABLE IF NOT EXISTS `sintomatipodengue` (
  `FK_ID_SINTOMA` int NOT NULL,
  `FK_ID_TIPODENGUE` int NOT NULL,
  PRIMARY KEY (`FK_ID_SINTOMA`,`FK_ID_TIPODENGUE`),
  KEY `16_idx` (`FK_ID_TIPODENGUE`),
  KEY `4_idx` (`FK_ID_SINTOMA`),
  CONSTRAINT `16` FOREIGN KEY (`FK_ID_TIPODENGUE`) REFERENCES `tipodengue` (`ID_TIPODENGUE`),
  CONSTRAINT `4` FOREIGN KEY (`FK_ID_SINTOMA`) REFERENCES `sintoma` (`ID_SINTOMA`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.sintomatipodengue: ~44 rows (aproximadamente)
INSERT INTO `sintomatipodengue` (`FK_ID_SINTOMA`, `FK_ID_TIPODENGUE`) VALUES
	(1, 1),
	(2, 1),
	(3, 1),
	(4, 1),
	(5, 1),
	(6, 1),
	(7, 1),
	(8, 1),
	(9, 1),
	(1, 2),
	(2, 2),
	(3, 2),
	(4, 2),
	(5, 2),
	(6, 2),
	(7, 2),
	(8, 2),
	(10, 2),
	(11, 2),
	(12, 2),
	(13, 2),
	(14, 2),
	(15, 2),
	(16, 2),
	(1, 3),
	(2, 3),
	(3, 3),
	(4, 3),
	(5, 3),
	(6, 3),
	(7, 3),
	(8, 3),
	(10, 3),
	(11, 3),
	(12, 3),
	(13, 3),
	(14, 3),
	(15, 3),
	(16, 3),
	(17, 3),
	(18, 3),
	(19, 3),
	(20, 3),
	(21, 3);

-- Volcando estructura para tabla app_dengue.tipodengue
CREATE TABLE IF NOT EXISTS `tipodengue` (
  `ID_TIPODENGUE` int NOT NULL AUTO_INCREMENT,
  `NOMBRE_TIPODENGUE` varchar(45) NOT NULL,
  `ESTADO_TIPODENGUE` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`ID_TIPODENGUE`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.tipodengue: ~3 rows (aproximadamente)
INSERT INTO `tipodengue` (`ID_TIPODENGUE`, `NOMBRE_TIPODENGUE`, `ESTADO_TIPODENGUE`) VALUES
	(1, 'Sin signos de alarma', 1),
	(2, 'Con signos de alarma', 1),
	(3, 'Dengue grave', 1);

-- Volcando estructura para tabla app_dengue.tiponotificacion
CREATE TABLE IF NOT EXISTS `tiponotificacion` (
  `ID_TIPONOTIFICACION` int NOT NULL AUTO_INCREMENT,
  `NOMBRE_TIPONOTIFICACION` varchar(45) NOT NULL,
  `DESCRIPCION_TIPONOTIFICACION` varchar(45) NOT NULL,
  PRIMARY KEY (`ID_TIPONOTIFICACION`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.tiponotificacion: ~5 rows (aproximadamente)
INSERT INTO `tiponotificacion` (`ID_TIPONOTIFICACION`, `NOMBRE_TIPONOTIFICACION`, `DESCRIPCION_TIPONOTIFICACION`) VALUES
	(1, 'Nuevo Caso', 'Un nuevo caso de dengue ha sido registrado.'),
	(2, 'Caso Finalizado', 'Un caso ha sido finalizado.'),
	(3, 'Nueva Publicación', 'Se ha creado una nueva publicación.'),
	(4, 'Actualización Hospital', 'Información del hospital modificada.'),
	(5, 'Fallecimiento', 'Un paciente ha fallecido por dengue.');

-- Volcando estructura para tabla app_dengue.tiposangre
CREATE TABLE IF NOT EXISTS `tiposangre` (
  `ID_TIPOSANGRE` int NOT NULL AUTO_INCREMENT,
  `NOMBRE_TIPOSANGRE` varchar(45) NOT NULL,
  `ESTADO_TIPOSANGRE` tinyint NOT NULL DEFAULT '1',
  PRIMARY KEY (`ID_TIPOSANGRE`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.tiposangre: ~8 rows (aproximadamente)
INSERT INTO `tiposangre` (`ID_TIPOSANGRE`, `NOMBRE_TIPOSANGRE`, `ESTADO_TIPOSANGRE`) VALUES
	(1, 'A+', 1),
	(2, 'A-', 1),
	(3, 'B+', 1),
	(4, 'B-', 1),
	(5, 'AB+', 1),
	(6, 'AB-', 1),
	(7, 'O+', 1),
	(8, 'O-', 1);

-- Volcando estructura para tabla app_dengue.usuario
CREATE TABLE IF NOT EXISTS `usuario` (
  `ID_USUARIO` int NOT NULL AUTO_INCREMENT,
  `NOMBRE_USUARIO` varchar(45) NOT NULL,
  `CORREO_USUARIO` varchar(45) NOT NULL,
  `CONTRASENIA_USUARIO` varchar(45) CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci NOT NULL,
  `DIRECCION_USUARIO` varchar(45) NOT NULL,
  `FK_ID_ROL` int DEFAULT NULL,
  `FK_ID_MUNICIPIO` int DEFAULT NULL,
  `FK_ID_TIPOSANGRE` int DEFAULT NULL,
  `FK_ID_GENERO` int DEFAULT NULL,
  `FK_ID_ESTADOUSUARIO` int DEFAULT '2',
  PRIMARY KEY (`ID_USUARIO`),
  KEY `2_idx` (`FK_ID_ROL`),
  KEY `11_idx` (`FK_ID_MUNICIPIO`),
  KEY `12_idx` (`FK_ID_TIPOSANGRE`),
  KEY `17_idx` (`FK_ID_GENERO`),
  KEY `FK_ID_ESTADOUSUARIO` (`FK_ID_ESTADOUSUARIO`),
  CONSTRAINT `11` FOREIGN KEY (`FK_ID_MUNICIPIO`) REFERENCES `municipio` (`ID_MUNICIPIO`),
  CONSTRAINT `12` FOREIGN KEY (`FK_ID_TIPOSANGRE`) REFERENCES `tiposangre` (`ID_TIPOSANGRE`),
  CONSTRAINT `17` FOREIGN KEY (`FK_ID_GENERO`) REFERENCES `genero` (`ID_GENERO`),
  CONSTRAINT `2` FOREIGN KEY (`FK_ID_ROL`) REFERENCES `rol` (`ID_ROL`),
  CONSTRAINT `FK_usuario_estadousuario` FOREIGN KEY (`FK_ID_ESTADOUSUARIO`) REFERENCES `estadousuario` (`ID_ESTADOUSUARIO`)
) ENGINE=InnoDB AUTO_INCREMENT=12 DEFAULT CHARSET=utf8mb3;

-- Volcando datos para la tabla app_dengue.usuario: ~9 rows (aproximadamente)
INSERT INTO `usuario` (`ID_USUARIO`, `NOMBRE_USUARIO`, `CORREO_USUARIO`, `CONTRASENIA_USUARIO`, `DIRECCION_USUARIO`, `FK_ID_ROL`, `FK_ID_MUNICIPIO`, `FK_ID_TIPOSANGRE`, `FK_ID_GENERO`, `FK_ID_ESTADOUSUARIO`) VALUES
	(1, 'Johan Ramirez', 'johans.ramirez@udla.edu.co', '0518', 'Calle 18 n2b-52', 1, 1, 8, 1, 2),
	(2, 'Maria cordoba', 'abc@hotmail.com', '1234', 'calle 10 n3', 2, 1, 2, 2, 2),
	(3, 'Micha Martinez', 'mic.martinez@udla.edu.co', '1234', 'B/porvenir', 2, 1, 3, 1, 1),
	(4, 'Andres marty', 'abc@udla.edu.co', 'abc@udla.edu.co', 'calle 10', 1, 1, 1, 1, 2),
	(6, 'Danna Vannesa', 'da.navia@udla.edu.co', 'da.navia@udla.edu.co', 'Barrio raicero', 2, 1, 7, 2, 2),
	(7, 'Cristian camilo', 'abc@udla.edu.co', 'abc@udla.edu.co', 'Barrio los alpes calle 18 N 2b-50', 2, 1, 5, 1, 2),
	(8, 'luis miguel', 'l@gmail.com', 'l@gmail.com', 'manzana e casa100 B villa erika', 2, 1, 8, 1, 2),
	(9, 'karen dayana', 'k@gmail.com', 'k@gmail.com', 'calle 20', 2, 1, 8, 2, 2),
	(10, 'lalo', 'l@gmial.com', 'l@gmial.com', 'calle 18 ', 2, 1, 5, 1, NULL);

-- Volcando estructura para procedimiento app_dengue.ValidarUsuario
DELIMITER //
CREATE PROCEDURE `ValidarUsuario`(
	IN `correo` VARCHAR(50),
	IN `contra` VARCHAR(50)
)
BEGIN
	SELECT usuario.ID_USUARIO,usuario.NOMBRE_USUARIO,usuario.CORREO_USUARIO,usuario.CONTRASENIA_USUARIO,usuario.DIRECCION_USUARIO,usuario.FK_ID_ROL,rol.NOMBRE_ROL,usuario.FK_ID_MUNICIPIO,municipio.NOMBRE_MUNICIPIO,usuario.FK_ID_TIPOSANGRE,tiposangre.NOMBRE_TIPOSANGRE,usuario.FK_ID_GENERO,genero.NOMBRE_GENERO FROM usuario
	INNER JOIN rol ON usuario.FK_ID_ROL= rol.ID_ROL
INNER JOIN municipio ON usuario.FK_ID_MUNICIPIO=municipio.ID_MUNICIPIO
INNER JOIN tiposangre ON usuario.FK_ID_TIPOSANGRE=tiposangre.ID_TIPOSANGRE
INNER JOIN genero ON usuario.FK_ID_GENERO= genero.ID_GENERO
	WHERE usuario.CORREO_USUARIO = correo AND usuario.CONTRASENIA_USUARIO = contra;
END//
DELIMITER ;

-- Volcando estructura para disparador app_dengue.ActualizarEstado
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `ActualizarEstado` AFTER UPDATE ON `casoreportado` FOR EACH ROW BEGIN

DECLARE estado INT;

    IF NEW.FK_ID_ESTADOCASO = 2 THEN
        SET estado = 2;
    ELSEIF NEW.FK_ID_ESTADOCASO = 5 THEN
        SET estado = 3;
    END IF;

    UPDATE usuario
    SET usuario.FK_ID_ESTADOUSUARIO = estado
    WHERE usuario.ID_USUARIO = OLD.FK_ID_PACIENTE;

END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Volcando estructura para disparador app_dengue.ActualizarEstadoInser
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `ActualizarEstadoInser` AFTER INSERT ON `casoreportado` FOR EACH ROW BEGIN

UPDATE usuario
SET usuario.FK_ID_ESTADOUSUARIO = 1
WHERE usuario.ID_USUARIO=NEW.FK_ID_PACIENTE;

END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Volcando estructura para disparador app_dengue.ActualizarFechas
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `ActualizarFechas` BEFORE UPDATE ON `casoreportado` FOR EACH ROW BEGIN
if NEW.FK_ID_ESTADOCASO = 2 OR NEW.FK_ID_ESTADOCASO = 5
then
	SET NEW.FECHAFINALIZACION_CASO = NOW();
END if;	
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Volcando estructura para disparador app_dengue.trg_NotificarCasoFinalizado
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `trg_NotificarCasoFinalizado` AFTER UPDATE ON `casoreportado` FOR EACH ROW BEGIN
  IF NEW.FK_ID_ESTADOCASO = 2 AND OLD.FK_ID_ESTADOCASO <> 2 THEN
    INSERT INTO notificacion (FECHA_NOTIFICACION, FK_ID_TIPONOTIFICACION)
    VALUES (
      NOW(),
      (SELECT ID_TIPONOTIFICACION
       FROM tiponotificacion
       WHERE NOMBRE_TIPONOTIFICACION = 'Caso Finalizado')
    );
  END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Volcando estructura para disparador app_dengue.trg_NotificarFallecimiento
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `trg_NotificarFallecimiento` AFTER UPDATE ON `casoreportado` FOR EACH ROW BEGIN
  IF NEW.FK_ID_ESTADOCASO = 5 AND OLD.FK_ID_ESTADOCASO <> 5 THEN
    INSERT INTO notificacion (FECHA_NOTIFICACION, FK_ID_TIPONOTIFICACION)
    VALUES (
      NOW(),
      (SELECT ID_TIPONOTIFICACION
       FROM tiponotificacion
       WHERE NOMBRE_TIPONOTIFICACION = 'Fallecimiento')
    );
  END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Volcando estructura para disparador app_dengue.trg_NotificarNuevaPublicacion
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `trg_NotificarNuevaPublicacion` AFTER INSERT ON `publicacion` FOR EACH ROW BEGIN
  INSERT INTO notificacion (FECHA_NOTIFICACION, FK_ID_TIPONOTIFICACION)
  VALUES (
    NOW(),
    (SELECT ID_TIPONOTIFICACION
     FROM tiponotificacion
     WHERE NOMBRE_TIPONOTIFICACION = 'Nueva Publicación')
  );
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Volcando estructura para disparador app_dengue.trg_NotificarNuevoCaso
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `trg_NotificarNuevoCaso` AFTER INSERT ON `casoreportado` FOR EACH ROW BEGIN
  INSERT INTO notificacion (FECHA_NOTIFICACION, FK_ID_TIPONOTIFICACION)
  VALUES (
    NOW(),
    (SELECT ID_TIPONOTIFICACION
     FROM tiponotificacion
     WHERE NOMBRE_TIPONOTIFICACION = 'Nuevo Caso')
  );
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
