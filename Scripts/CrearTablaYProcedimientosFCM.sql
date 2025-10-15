-- ===================================
-- TABLA PARA TOKENS FCM
-- ===================================

CREATE TABLE IF NOT EXISTS fcmtoken (
    ID_FCMTOKEN INT PRIMARY KEY AUTO_INCREMENT,
    FK_ID_USUARIO INT NOT NULL,
    FCM_TOKEN VARCHAR(500) NOT NULL,
    FECHA_REGISTRO DATETIME DEFAULT CURRENT_TIMESTAMP,
    FECHA_ACTUALIZACION DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (FK_ID_USUARIO) REFERENCES usuario(ID_USUARIO) ON DELETE CASCADE,
    UNIQUE KEY unique_usuario_token (FK_ID_USUARIO)
);

-- ===================================
-- PROCEDIMIENTO: GUARDAR O ACTUALIZAR TOKEN FCM
-- ===================================

DELIMITER $

DROP PROCEDURE IF EXISTS GuardarFCMToken$

CREATE PROCEDURE GuardarFCMToken(
    IN idUsuario INT,
    IN fcmToken VARCHAR(500)
)
BEGIN
    -- Si ya existe un token para este usuario, lo actualiza. Si no, lo inserta.
    INSERT INTO fcmtoken (FK_ID_USUARIO, FCM_TOKEN)
    VALUES (idUsuario, fcmToken)
    ON DUPLICATE KEY UPDATE
        FCM_TOKEN = fcmToken,
        FECHA_ACTUALIZACION = CURRENT_TIMESTAMP;
END$

DELIMITER ;

-- ===================================
-- PROCEDIMIENTO: ELIMINAR TOKEN FCM
-- ===================================

DELIMITER $

DROP PROCEDURE IF EXISTS EliminarFCMToken$

CREATE PROCEDURE EliminarFCMToken(
    IN idUsuario INT
)
BEGIN
    DELETE FROM fcmtoken WHERE FK_ID_USUARIO = idUsuario;
END$

DELIMITER ;

-- ===================================
-- PROCEDIMIENTO: OBTENER TOKEN FCM DE UN USUARIO
-- ===================================

DELIMITER $

DROP PROCEDURE IF EXISTS ObtenerFCMToken$

CREATE PROCEDURE ObtenerFCMToken(
    IN idUsuario INT
)
BEGIN
    SELECT FCM_TOKEN
    FROM fcmtoken
    WHERE FK_ID_USUARIO = idUsuario;
END$

DELIMITER ;

-- ===================================
-- PROCEDIMIENTO: OBTENER TODOS LOS TOKENS FCM
-- ===================================

DELIMITER $

DROP PROCEDURE IF EXISTS ObtenerTodosLosFCMTokens$

CREATE PROCEDURE ObtenerTodosLosFCMTokens()
BEGIN
    SELECT
        f.ID_FCMTOKEN,
        f.FK_ID_USUARIO,
        f.FCM_TOKEN,
        u.NOMBRE_USUARIO,
        f.FECHA_REGISTRO,
        f.FECHA_ACTUALIZACION
    FROM fcmtoken f
    INNER JOIN usuario u ON f.FK_ID_USUARIO = u.ID_USUARIO;
END$

DELIMITER ;

-- ===================================
-- PROCEDIMIENTO: OBTENER TOKENS POR ROL
-- ===================================

DELIMITER $

DROP PROCEDURE IF EXISTS ObtenerFCMTokensPorRol$

CREATE PROCEDURE ObtenerFCMTokensPorRol(
    IN rolId INT
)
BEGIN
    SELECT
        f.FCM_TOKEN,
        u.ID_USUARIO,
        u.NOMBRE_USUARIO
    FROM fcmtoken f
    INNER JOIN usuario u ON f.FK_ID_USUARIO = u.ID_USUARIO
    WHERE u.FK_ID_ROL = rolId;
END$

DELIMITER ;
