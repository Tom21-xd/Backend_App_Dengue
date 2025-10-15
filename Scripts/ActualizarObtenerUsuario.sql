DELIMITER $

DROP PROCEDURE IF EXISTS ObtenerUsuario$

CREATE PROCEDURE ObtenerUsuario(
    IN idu INT
)
BEGIN
    SELECT
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
        departamento.ID_DEPARTAMENTO,
        estadousuario.NOMBRE_ESTADOUSUARIO
    FROM usuario
    INNER JOIN rol ON usuario.FK_ID_ROL = rol.ID_ROL
    INNER JOIN municipio ON usuario.FK_ID_MUNICIPIO = municipio.ID_MUNICIPIO
    INNER JOIN departamento ON municipio.FK_ID_DEPARTAMENTO = departamento.ID_DEPARTAMENTO
    INNER JOIN tiposangre ON usuario.FK_ID_TIPOSANGRE = tiposangre.ID_TIPOSANGRE
    INNER JOIN genero ON usuario.FK_ID_GENERO = genero.ID_GENERO
    INNER JOIN estadousuario ON usuario.FK_ID_ESTADOUSUARIO = estadousuario.ID_ESTADOUSUARIO
    WHERE usuario.ID_USUARIO = idu;
END$

DELIMITER ;
