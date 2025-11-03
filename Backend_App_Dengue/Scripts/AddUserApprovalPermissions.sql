-- Script para agregar permisos de gestión de aprobaciones de usuarios
-- Ejecutar este script en la base de datos app_dengue

USE app_dengue;

-- Insertar nuevos permisos para gestión de aprobaciones
INSERT INTO permiso (CODIGO_PERMISO, NOMBRE_PERMISO, DESCRIPCION_PERMISO, CATEGORIA_PERMISO, ESTADO_PERMISO)
VALUES
    ('USER_APPROVAL_VIEW', 'Ver Solicitudes de Aprobación', 'Permite ver las solicitudes de aprobación de usuarios pendientes', 'Gestión de Usuarios', 1),
    ('USER_APPROVAL_APPROVE', 'Aprobar Usuarios', 'Permite aprobar solicitudes de usuarios y cambiar sus roles', 'Gestión de Usuarios', 1),
    ('USER_APPROVAL_REJECT', 'Rechazar Usuarios', 'Permite rechazar solicitudes de usuarios', 'Gestión de Usuarios', 1),
    ('USER_APPROVAL_HISTORY', 'Ver Historial de Aprobaciones', 'Permite ver el historial completo de aprobaciones y rechazos', 'Gestión de Usuarios', 1),
    ('PERMISSION_MANAGE', 'Gestionar Permisos', 'Permite asignar y revocar permisos a los roles del sistema', 'Administración del Sistema', 1)
ON DUPLICATE KEY UPDATE CODIGO_PERMISO = CODIGO_PERMISO; -- No hacer nada si ya existe

-- Obtener los IDs de los permisos recién creados
SET @perm_view = (SELECT ID_PERMISO FROM permiso WHERE CODIGO_PERMISO = 'USER_APPROVAL_VIEW');
SET @perm_approve = (SELECT ID_PERMISO FROM permiso WHERE CODIGO_PERMISO = 'USER_APPROVAL_APPROVE');
SET @perm_reject = (SELECT ID_PERMISO FROM permiso WHERE CODIGO_PERMISO = 'USER_APPROVAL_REJECT');
SET @perm_history = (SELECT ID_PERMISO FROM permiso WHERE CODIGO_PERMISO = 'USER_APPROVAL_HISTORY');
SET @perm_manage = (SELECT ID_PERMISO FROM permiso WHERE CODIGO_PERMISO = 'PERMISSION_MANAGE');

-- Obtener el ID del rol de Administrador (ajustar según tu base de datos)
-- Asumiendo que el rol de admin tiene ID 3, pero verifica primero:
SELECT ID_ROL, NOMBRE_ROL FROM rol WHERE NOMBRE_ROL LIKE '%Admin%';

-- Asignar permisos al rol de Administrador (ajusta el ID del rol según corresponda)
-- Si tu rol de admin tiene otro ID, cambia el 3 por el ID correcto
SET @admin_role_id = 3;

INSERT INTO rol_permiso (FK_ID_ROL, FK_ID_PERMISO, ESTADO_ROL_PERMISO, FECHA_ASIGNACION)
VALUES
    (@admin_role_id, @perm_view, 1, NOW()),
    (@admin_role_id, @perm_approve, 1, NOW()),
    (@admin_role_id, @perm_reject, 1, NOW()),
    (@admin_role_id, @perm_history, 1, NOW()),
    (@admin_role_id, @perm_manage, 1, NOW())
ON DUPLICATE KEY UPDATE ESTADO_ROL_PERMISO = 1, FECHA_ASIGNACION = NOW(); -- Reactivar si ya existe

-- Verificar que se crearon correctamente
SELECT
    p.CODIGO_PERMISO,
    p.NOMBRE_PERMISO,
    p.DESCRIPCION_PERMISO,
    r.NOMBRE_ROL
FROM permiso p
JOIN rol_permiso rp ON p.ID_PERMISO = rp.FK_ID_PERMISO
JOIN rol r ON rp.FK_ID_ROL = r.ID_ROL
WHERE p.CODIGO_PERMISO IN ('USER_APPROVAL_VIEW', 'USER_APPROVAL_APPROVE', 'USER_APPROVAL_REJECT', 'USER_APPROVAL_HISTORY', 'PERMISSION_MANAGE')
    AND rp.ESTADO_ROL_PERMISO = 1
ORDER BY p.CODIGO_PERMISO;

-- Mostrar todos los roles disponibles para referencia
SELECT ID_ROL, NOMBRE_ROL, ESTADO_ROL FROM rol;
