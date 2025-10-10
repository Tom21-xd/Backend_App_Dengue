-- ========================================
-- SCRIPT 1: CORRECCIONES Y OPTIMIZACIÓN
-- App Gestión de Dengue
-- MySQL Compatible
-- ========================================

USE `app_dengue`;

-- ========================================
-- PARTE 1: CORREGIR PROCEDIMIENTOS CON ERRORES
-- ========================================

-- 1.1 Corregir EditarHospital (Error en línea 240 del script original)
DROP PROCEDURE IF EXISTS `EditarHospital`;
DELIMITER //
CREATE PROCEDURE `EditarHospital`(
    IN `idh` INT,
    IN `nombre` VARCHAR(50),
    IN `imagenN` VARCHAR(50)
)
BEGIN
    UPDATE hospital
    SET hospital.NOMBRE_HOSPITAL = nombre,
        hospital.IMAGEN_HOSPITAL = imagenN  -- Corregido: antes era 'imagen'
    WHERE hospital.ID_HOSPITAL = idh;
END//
DELIMITER ;

-- 1.2 Corregir InsertarUsuario (Error: variable no definida)
DROP PROCEDURE IF EXISTS `InsertarUsuario`;
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
            p_id_rol, p_id_municipio, p_id_tiposangre, p_id_genero, 2);  -- Estado 2 = Vivo (por defecto)
END//
DELIMITER ;

-- ========================================
-- PARTE 2: AGREGAR ÍNDICES DE OPTIMIZACIÓN
-- ========================================

-- 2.1 Índices para tabla casoreportado
ALTER TABLE casoreportado ADD INDEX idx_caso_fecha (FECHA_CASOREPORTADO);
ALTER TABLE casoreportado ADD INDEX idx_caso_estado (FK_ID_ESTADOCASO);
ALTER TABLE casoreportado ADD INDEX idx_caso_paciente (FK_ID_PACIENTE);
ALTER TABLE casoreportado ADD INDEX idx_caso_hospital (FK_ID_HOSPITAL);
ALTER TABLE casoreportado ADD INDEX idx_caso_medico (FK_ID_PERSONALMEDICO);
ALTER TABLE casoreportado ADD INDEX idx_caso_tipodengue (FK_ID_TIPODENGUE);

-- 2.2 Índices para tabla usuario
ALTER TABLE usuario ADD INDEX idx_usuario_correo (CORREO_USUARIO);
ALTER TABLE usuario ADD INDEX idx_usuario_rol (FK_ID_ROL);
ALTER TABLE usuario ADD INDEX idx_usuario_municipio (FK_ID_MUNICIPIO);
ALTER TABLE usuario ADD INDEX idx_usuario_estado (FK_ID_ESTADOUSUARIO);

-- 2.3 Índices para tabla publicacion
ALTER TABLE publicacion ADD INDEX idx_publicacion_fecha (FECHA_PUBLICACION);
ALTER TABLE publicacion ADD INDEX idx_publicacion_usuario (FK_ID_USUARIO);
ALTER TABLE publicacion ADD INDEX idx_publicacion_titulo (TITULO_PUBLICACION);

-- 2.4 Índices para tabla notificacion
ALTER TABLE notificacion ADD INDEX idx_notificacion_fecha (FECHA_NOTIFICACION);
ALTER TABLE notificacion ADD INDEX idx_notificacion_tipo (FK_ID_TIPONOTIFICACION);

-- 2.5 Índices para tabla hospital
ALTER TABLE hospital ADD INDEX idx_hospital_municipio (FK_ID_MUNICIPIO);
ALTER TABLE hospital ADD INDEX idx_hospital_estado (ESTADO_HOSPITAL);
ALTER TABLE hospital ADD INDEX idx_hospital_nombre (NOMBRE_HOSPITAL);

-- ========================================
-- PARTE 3: MEJORAR ESTRUCTURA DE TABLA NOTIFICACION
-- ========================================

-- 3.1 Agregar campo para marcar notificaciones como leídas (si no existe)
SET @col_exists = 0;
SELECT COUNT(*) INTO @col_exists
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'app_dengue'
AND TABLE_NAME = 'notificacion'
AND COLUMN_NAME = 'LEIDA';

SET @sql = IF(@col_exists = 0,
    'ALTER TABLE notificacion ADD COLUMN LEIDA TINYINT DEFAULT 0 COMMENT ''0=No leída, 1=Leída''',
    'SELECT ''Column LEIDA already exists'' AS result');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 3.2 Agregar campo para usuario destinatario (si no existe)
SET @col_exists = 0;
SELECT COUNT(*) INTO @col_exists
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'app_dengue'
AND TABLE_NAME = 'notificacion'
AND COLUMN_NAME = 'FK_ID_USUARIO_DESTINATARIO';

SET @sql = IF(@col_exists = 0,
    'ALTER TABLE notificacion ADD COLUMN FK_ID_USUARIO_DESTINATARIO INT DEFAULT NULL COMMENT ''NULL=Notificación global, ID=Notificación específica''',
    'SELECT ''Column FK_ID_USUARIO_DESTINATARIO already exists'' AS result');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- 3.3 Agregar índice para notificaciones leídas
ALTER TABLE notificacion ADD INDEX idx_notificacion_leida (LEIDA);

-- ========================================
-- PARTE 4: AGREGAR CONSTRAINTS Y MEJORAS
-- ========================================

-- 4.1 Asegurar que las fechas tengan valor por defecto
ALTER TABLE casoreportado
MODIFY COLUMN FECHA_CASOREPORTADO DATE NOT NULL DEFAULT (CURDATE());

-- 4.2 Asegurar que el estado del hospital sea válido
ALTER TABLE hospital
MODIFY COLUMN ESTADO_HOSPITAL TINYINT NOT NULL DEFAULT 1 COMMENT '0=Inactivo, 1=Activo';

-- ========================================
-- VERIFICACIÓN DE CAMBIOS
-- ========================================

-- Verificar procedimientos corregidos
SELECT 'Verificando procedimientos corregidos...' as mensaje;
SELECT ROUTINE_NAME, ROUTINE_TYPE
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = 'app_dengue'
AND ROUTINE_NAME IN ('EditarHospital', 'InsertarUsuario');

-- Verificar índices creados en casoreportado
SELECT 'Índices en casoreportado:' as tabla;
SHOW INDEX FROM casoreportado WHERE Key_name LIKE 'idx_%';

-- Verificar índices creados en usuario
SELECT 'Índices en usuario:' as tabla;
SHOW INDEX FROM usuario WHERE Key_name LIKE 'idx_%';

-- Verificar índices creados en publicacion
SELECT 'Índices en publicacion:' as tabla;
SHOW INDEX FROM publicacion WHERE Key_name LIKE 'idx_%';

-- Verificar índices creados en notificacion
SELECT 'Índices en notificacion:' as tabla;
SHOW INDEX FROM notificacion WHERE Key_name LIKE 'idx_%';

-- Verificar índices creados en hospital
SELECT 'Índices en hospital:' as tabla;
SHOW INDEX FROM hospital WHERE Key_name LIKE 'idx_%';

-- Verificar nuevas columnas en notificacion
SELECT 'Estructura de tabla notificacion:' as tabla;
DESCRIBE notificacion;

SELECT '✅ Script de correcciones y optimización completado exitosamente' as resultado;
