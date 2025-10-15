-- ========================================
-- SCRIPT 2: NUEVOS PROCEDIMIENTOS ALMACENADOS
-- App Gestión de Dengue
-- ========================================

USE `app_dengue`;

-- ========================================
-- PROCEDIMIENTOS PARA HOSPITALES (HU-009)
-- ========================================

-- Eliminar Hospital
DROP PROCEDURE IF EXISTS `EliminarHospital`;
DELIMITER //
CREATE PROCEDURE `EliminarHospital`(
    IN `idh` INT
)
BEGIN
    -- Declarar variables
    DECLARE casos_activos INT;

    -- Verificar que no tenga casos asociados activos
    SELECT COUNT(*) INTO casos_activos
    FROM casoreportado
    WHERE FK_ID_HOSPITAL = idh
    AND FK_ID_ESTADOCASO NOT IN (2, 5);

    IF casos_activos > 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'No se puede eliminar el hospital porque tiene casos activos asociados';
    ELSE
        -- Soft delete: cambiar estado en lugar de eliminar
        UPDATE hospital
        SET ESTADO_HOSPITAL = 0
        WHERE ID_HOSPITAL = idh;
    END IF;
END//
DELIMITER ;

-- ========================================
-- PROCEDIMIENTOS PARA PUBLICACIONES (HU-007)
-- ========================================

-- Eliminar Publicación
DROP PROCEDURE IF EXISTS `EliminarPublicacion`;
DELIMITER //
CREATE PROCEDURE `EliminarPublicacion`(
    IN `idp` INT
)
BEGIN
    DELETE FROM publicacion
    WHERE ID_PUBLICACION = idp;
END//
DELIMITER ;

-- ========================================
-- PROCEDIMIENTOS PARA CASOS (HU-006)
-- ========================================

-- Eliminar Caso
DROP PROCEDURE IF EXISTS `EliminarCaso`;
DELIMITER //
CREATE PROCEDURE `EliminarCaso`(
    IN `idc` INT
)
BEGIN
    -- Declarar variables al inicio
    DECLARE id_paciente INT;
    DECLARE otros_casos INT;

    -- Iniciar transacción
    START TRANSACTION;

    -- Obtener paciente antes de eliminar
    SELECT FK_ID_PACIENTE INTO id_paciente
    FROM casoreportado
    WHERE ID_CASOREPORTADO = idc;

    -- Eliminar el caso
    DELETE FROM casoreportado
    WHERE ID_CASOREPORTADO = idc;

    -- Verificar si el paciente tiene otros casos activos
    SELECT COUNT(*) INTO otros_casos
    FROM casoreportado
    WHERE FK_ID_PACIENTE = id_paciente
    AND FK_ID_ESTADOCASO NOT IN (2, 5);

    -- Si no tiene más casos activos, cambiar estado a "Vivo"
    IF otros_casos = 0 THEN
        UPDATE usuario
        SET FK_ID_ESTADOUSUARIO = 2
        WHERE ID_USUARIO = id_paciente;
    END IF;

    -- Confirmar transacción
    COMMIT;
END//
DELIMITER ;

-- ========================================
-- PROCEDIMIENTOS PARA USUARIOS (HU-004, HU-005)
-- ========================================

-- Eliminar Usuario (Soft Delete)
DROP PROCEDURE IF EXISTS `EliminarUsuario`;
DELIMITER //
CREATE PROCEDURE `EliminarUsuario`(
    IN `idu` INT
)
BEGIN
    -- Verificar que no sea un usuario con casos activos
    DECLARE casos_activos INT;

    SELECT COUNT(*) INTO casos_activos
    FROM casoreportado
    WHERE (FK_ID_PACIENTE = idu OR FK_ID_PERSONALMEDICO = idu)
    AND FK_ID_ESTADOCASO NOT IN (2, 5);

    IF casos_activos > 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'No se puede eliminar el usuario porque tiene casos activos asociados';
    ELSE
        -- Soft delete: cambiar email para evitar duplicados y marcar como eliminado
        UPDATE usuario
        SET CORREO_USUARIO = CONCAT('deleted_', ID_USUARIO, '_', CORREO_USUARIO),
            FK_ID_ESTADOUSUARIO = 3,
            NOMBRE_USUARIO = CONCAT('[ELIMINADO] ', NOMBRE_USUARIO)
        WHERE ID_USUARIO = idu;
    END IF;
END//
DELIMITER ;

-- Buscar Usuarios (mejorado)
DROP PROCEDURE IF EXISTS `BuscarUsuarios`;
DELIMITER //
CREATE PROCEDURE `BuscarUsuarios`(
    IN `filtro` VARCHAR(50),
    IN `id_rol` INT
)
BEGIN
    SELECT DISTINCT
        usuario.ID_USUARIO,
        usuario.NOMBRE_USUARIO,
        usuario.CORREO_USUARIO,
        usuario.CONTRASENIA_USUARIO,
        usuario.DIRECCION_USUARIO,
        usuario.FK_ID_ROL,
        rol.NOMBRE_ROL,
        usuario.FK_ID_MUNICIPIO,
        municipio.NOMBRE_MUNICIPIO,
        usuario.FK_ID_TIPOSANGRE,
        tiposangre.NOMBRE_TIPOSANGRE,
        usuario.FK_ID_GENERO,
        genero.NOMBRE_GENERO,
        estadousuario.NOMBRE_ESTADOUSUARIO
    FROM usuario
    INNER JOIN rol ON usuario.FK_ID_ROL = rol.ID_ROL
    INNER JOIN municipio ON usuario.FK_ID_MUNICIPIO = municipio.ID_MUNICIPIO
    INNER JOIN tiposangre ON usuario.FK_ID_TIPOSANGRE = tiposangre.ID_TIPOSANGRE
    INNER JOIN genero ON usuario.FK_ID_GENERO = genero.ID_GENERO
    INNER JOIN estadousuario ON usuario.FK_ID_ESTADOUSUARIO = estadousuario.ID_ESTADOUSUARIO
    WHERE usuario.NOMBRE_USUARIO LIKE CONCAT('%', filtro, '%')
    AND (id_rol IS NULL OR usuario.FK_ID_ROL = id_rol)
    AND usuario.FK_ID_ESTADOUSUARIO != 3;
END//
DELIMITER ;

-- ========================================
-- PROCEDIMIENTOS PARA NOTIFICACIONES (HU-008)
-- ========================================

-- Marcar Notificación como Leída
DROP PROCEDURE IF EXISTS `MarcarNotificacionLeida`;
DELIMITER //
CREATE PROCEDURE `MarcarNotificacionLeida`(
    IN `idn` INT
)
BEGIN
    UPDATE notificacion
    SET LEIDA = 1
    WHERE ID_NOTIFICACION = idn;
END//
DELIMITER ;

-- Marcar Todas como Leídas
DROP PROCEDURE IF EXISTS `MarcarTodasLeidas`;
DELIMITER //
CREATE PROCEDURE `MarcarTodasLeidas`()
BEGIN
    UPDATE notificacion
    SET LEIDA = 1
    WHERE LEIDA = 0;
END//
DELIMITER ;

-- Obtener Notificaciones No Leídas
DROP PROCEDURE IF EXISTS `ObtenerNotificacionesNoLeidas`;
DELIMITER //
CREATE PROCEDURE `ObtenerNotificacionesNoLeidas`()
BEGIN
    SELECT
        n.ID_NOTIFICACION,
        n.FECHA_NOTIFICACION,
        n.LEIDA,
        tn.NOMBRE_TIPONOTIFICACION,
        tn.DESCRIPCION_TIPONOTIFICACION
    FROM notificacion AS n
    LEFT JOIN tiponotificacion AS tn
        ON n.FK_ID_TIPONOTIFICACION = tn.ID_TIPONOTIFICACION
    WHERE n.LEIDA = 0
    ORDER BY n.FECHA_NOTIFICACION DESC;
END//
DELIMITER ;

-- ========================================
-- PROCEDIMIENTOS PARA ESTADÍSTICAS (Dashboard)
-- ========================================

-- Estadísticas Generales
DROP PROCEDURE IF EXISTS `EstadisticasGenerales`;
DELIMITER //
CREATE PROCEDURE `EstadisticasGenerales`()
BEGIN
    SELECT
        (SELECT COUNT(*) FROM casoreportado) AS total_casos,
        (SELECT COUNT(*) FROM casoreportado WHERE FK_ID_ESTADOCASO NOT IN (2, 5)) AS casos_activos,
        (SELECT COUNT(*) FROM casoreportado WHERE FK_ID_ESTADOCASO = 2) AS casos_finalizados,
        (SELECT COUNT(*) FROM casoreportado WHERE FK_ID_ESTADOCASO = 5) AS casos_fallecidos,
        (SELECT COUNT(*) FROM usuario WHERE FK_ID_ESTADOUSUARIO = 1) AS usuarios_enfermos,
        (SELECT COUNT(*) FROM hospital WHERE ESTADO_HOSPITAL = 1) AS hospitales_activos,
        (SELECT COUNT(*) FROM publicacion) AS total_publicaciones,
        (SELECT COUNT(*) FROM notificacion WHERE LEIDA = 0) AS notificaciones_pendientes;
END//
DELIMITER ;

-- Casos por Tipo de Dengue
DROP PROCEDURE IF EXISTS `CasosPorTipoDengue`;
DELIMITER //
CREATE PROCEDURE `CasosPorTipoDengue`()
BEGIN
    SELECT
        td.ID_TIPODENGUE,
        td.NOMBRE_TIPODENGUE,
        COUNT(c.ID_CASOREPORTADO) AS total_casos,
        COUNT(CASE WHEN c.FK_ID_ESTADOCASO NOT IN (2, 5) THEN 1 END) AS casos_activos,
        COUNT(CASE WHEN c.FK_ID_ESTADOCASO = 2 THEN 1 END) AS casos_finalizados,
        COUNT(CASE WHEN c.FK_ID_ESTADOCASO = 5 THEN 1 END) AS casos_fallecidos,
        ROUND(COUNT(CASE WHEN c.FK_ID_ESTADOCASO = 5 THEN 1 END) * 100.0 / NULLIF(COUNT(c.ID_CASOREPORTADO), 0), 2) AS tasa_mortalidad
    FROM tipodengue td
    LEFT JOIN casoreportado c ON td.ID_TIPODENGUE = c.FK_ID_TIPODENGUE
    GROUP BY td.ID_TIPODENGUE, td.NOMBRE_TIPODENGUE
    ORDER BY total_casos DESC;
END//
DELIMITER ;

-- Casos por Mes
DROP PROCEDURE IF EXISTS `CasosPorMes`;
DELIMITER //
CREATE PROCEDURE `CasosPorMes`(
    IN `anio` INT
)
BEGIN
    SELECT
        MONTH(FECHA_CASOREPORTADO) AS mes,
        MONTHNAME(FECHA_CASOREPORTADO) AS nombre_mes,
        COUNT(*) AS total_casos,
        COUNT(CASE WHEN FK_ID_TIPODENGUE = 1 THEN 1 END) AS sin_alarma,
        COUNT(CASE WHEN FK_ID_TIPODENGUE = 2 THEN 1 END) AS con_alarma,
        COUNT(CASE WHEN FK_ID_TIPODENGUE = 3 THEN 1 END) AS grave
    FROM casoreportado
    WHERE YEAR(FECHA_CASOREPORTADO) = anio
    GROUP BY MONTH(FECHA_CASOREPORTADO), MONTHNAME(FECHA_CASOREPORTADO)
    ORDER BY mes;
END//
DELIMITER ;

-- Casos por Departamento
DROP PROCEDURE IF EXISTS `CasosPorDepartamento`;
DELIMITER //
CREATE PROCEDURE `CasosPorDepartamento`()
BEGIN
    SELECT
        d.ID_DEPARTAMENTO,
        d.NOMBRE_DEPARTAMENTO,
        COUNT(c.ID_CASOREPORTADO) AS total_casos,
        COUNT(CASE WHEN c.FK_ID_ESTADOCASO NOT IN (2, 5) THEN 1 END) AS casos_activos,
        COUNT(DISTINCT h.ID_HOSPITAL) AS hospitales_involucrados
    FROM departamento d
    LEFT JOIN municipio m ON d.ID_DEPARTAMENTO = m.FK_ID_DEPARTAMENTO
    LEFT JOIN hospital h ON m.ID_MUNICIPIO = h.FK_ID_MUNICIPIO
    LEFT JOIN casoreportado c ON h.ID_HOSPITAL = c.FK_ID_HOSPITAL
    GROUP BY d.ID_DEPARTAMENTO, d.NOMBRE_DEPARTAMENTO
    ORDER BY total_casos DESC;
END//
DELIMITER ;

-- Tendencia de Casos (últimos N meses)
DROP PROCEDURE IF EXISTS `TendenciaCasos`;
DELIMITER //
CREATE PROCEDURE `TendenciaCasos`(
    IN `meses` INT
)
BEGIN
    SELECT
        DATE_FORMAT(FECHA_CASOREPORTADO, '%Y-%m') AS periodo,
        COUNT(*) AS total_casos,
        AVG(CASE WHEN FK_ID_ESTADOCASO = 5 THEN 1 ELSE 0 END) AS tasa_mortalidad
    FROM casoreportado
    WHERE FECHA_CASOREPORTADO >= DATE_SUB(CURDATE(), INTERVAL meses MONTH)
    GROUP BY DATE_FORMAT(FECHA_CASOREPORTADO, '%Y-%m')
    ORDER BY periodo;
END//
DELIMITER ;

-- Top Hospitales con más casos
DROP PROCEDURE IF EXISTS `TopHospitalesCasos`;
DELIMITER //
CREATE PROCEDURE `TopHospitalesCasos`(
    IN `limite` INT
)
BEGIN
    SELECT
        h.ID_HOSPITAL,
        h.NOMBRE_HOSPITAL,
        m.NOMBRE_MUNICIPIO,
        d.NOMBRE_DEPARTAMENTO,
        COUNT(c.ID_CASOREPORTADO) AS total_casos,
        COUNT(CASE WHEN c.FK_ID_ESTADOCASO NOT IN (2, 5) THEN 1 END) AS casos_activos
    FROM hospital h
    INNER JOIN municipio m ON h.FK_ID_MUNICIPIO = m.ID_MUNICIPIO
    INNER JOIN departamento d ON m.FK_ID_DEPARTAMENTO = d.ID_DEPARTAMENTO
    LEFT JOIN casoreportado c ON h.ID_HOSPITAL = c.FK_ID_HOSPITAL
    WHERE h.ESTADO_HOSPITAL = 1
    GROUP BY h.ID_HOSPITAL, h.NOMBRE_HOSPITAL, m.NOMBRE_MUNICIPIO, d.NOMBRE_DEPARTAMENTO
    ORDER BY total_casos DESC
    LIMIT limite;
END//
DELIMITER ;

-- ========================================
-- PROCEDIMIENTO PARA INFERENCIA DE DENGUE (HU-013)
-- ========================================

DROP PROCEDURE IF EXISTS `InferirTipoDengue`;
DELIMITER //
CREATE PROCEDURE `InferirTipoDengue`(
    IN `sintomas_ids` VARCHAR(500)
)
BEGIN
    SELECT
        td.ID_TIPODENGUE,
        td.NOMBRE_TIPODENGUE,
        COUNT(CASE WHEN FIND_IN_SET(std.FK_ID_SINTOMA, sintomas_ids) > 0 THEN 1 END) AS puntaje,
        COUNT(CASE WHEN FIND_IN_SET(std.FK_ID_SINTOMA, sintomas_ids) > 0 THEN 1 END) AS sintomas_coincidentes,
        COUNT(std.FK_ID_SINTOMA) AS total_sintomas,
        ROUND((COUNT(CASE WHEN FIND_IN_SET(std.FK_ID_SINTOMA, sintomas_ids) > 0 THEN 1 END) * 100.0 / NULLIF(COUNT(std.FK_ID_SINTOMA), 0)), 2) AS porcentaje_coincidencia,
        CASE
            WHEN COUNT(CASE WHEN FIND_IN_SET(std.FK_ID_SINTOMA, sintomas_ids) > 0 THEN 1 END) >= COUNT(std.FK_ID_SINTOMA) * 0.8 THEN 'Alta probabilidad'
            WHEN COUNT(CASE WHEN FIND_IN_SET(std.FK_ID_SINTOMA, sintomas_ids) > 0 THEN 1 END) >= COUNT(std.FK_ID_SINTOMA) * 0.5 THEN 'Probabilidad media'
            WHEN COUNT(CASE WHEN FIND_IN_SET(std.FK_ID_SINTOMA, sintomas_ids) > 0 THEN 1 END) > 0 THEN 'Baja probabilidad'
            ELSE 'No coincide'
        END AS diagnostico
    FROM tipodengue td
    LEFT JOIN sintomatipodengue std ON td.ID_TIPODENGUE = std.FK_ID_TIPODENGUE
    GROUP BY td.ID_TIPODENGUE, td.NOMBRE_TIPODENGUE
    HAVING puntaje > 0
    ORDER BY puntaje DESC, porcentaje_coincidencia DESC;
END//
DELIMITER ;

-- ========================================
-- VERIFICACIÓN
-- ========================================

SELECT '✅ Todos los procedimientos nuevos han sido creados exitosamente' as resultado;

-- Mostrar lista de procedimientos creados
SELECT ROUTINE_NAME, ROUTINE_TYPE
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'app_dengue'
AND ROUTINE_NAME IN (
    'EliminarHospital',
    'EliminarPublicacion',
    'EliminarCaso',
    'EliminarUsuario',
    'BuscarUsuarios',
    'MarcarNotificacionLeida',
    'MarcarTodasLeidas',
    'ObtenerNotificacionesNoLeidas',
    'EstadisticasGenerales',
    'CasosPorTipoDengue',
    'CasosPorMes',
    'CasosPorDepartamento',
    'TendenciaCasos',
    'TopHospitalesCasos',
    'InferirTipoDengue'
)
ORDER BY ROUTINE_NAME;
