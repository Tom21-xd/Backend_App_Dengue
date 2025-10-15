-- =============================================
-- Stored Procedure: CrearUsuario
-- Descripción: Crea un nuevo usuario en la base de datos
-- =============================================

DELIMITER $$

DROP PROCEDURE IF EXISTS CrearUsuario$$

CREATE PROCEDURE CrearUsuario(
    IN nombre VARCHAR(50),
    IN correo VARCHAR(50),
    IN contrasena VARCHAR(64),
    IN direccion VARCHAR(100),
    IN rol INT,
    IN municipio INT,
    IN tipoSangre INT,
    IN genero INT
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Error al crear el usuario';
    END;

    START TRANSACTION;

    -- Verificar si el correo ya existe
    IF EXISTS (SELECT 1 FROM usuario WHERE CORREO_USUARIO = correo) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El correo ya se encuentra registrado';
    END IF;

    -- Insertar el nuevo usuario
    INSERT INTO usuario (
        NOMBRE_USUARIO,
        CORREO_USUARIO,
        CONTRASENIA_USUARIO,
        DIRECCION_USUARIO,
        FK_ID_ROL,
        FK_ID_MUNICIPIO,
        FK_ID_TIPOSANGRE,
        FK_ID_GENERO
    ) VALUES (
        nombre,
        correo,
        contrasena,
        direccion,
        rol,
        municipio,
        tipoSangre,
        genero
    );

    -- Retornar el ID del usuario creado
    SELECT LAST_INSERT_ID() as usuarioId;

    COMMIT;
END$$

DELIMITER ;
