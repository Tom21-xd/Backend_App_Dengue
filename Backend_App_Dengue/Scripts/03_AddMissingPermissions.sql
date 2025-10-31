-- Script to add missing permissions to the system
-- Date: 2025-10-30
-- Description: Adds NOTIFICATION_VIEW, PUBLICATION_SAVE, and MAP_VIEW permissions

USE dengue_db;

-- Insert missing permissions (only if they don't exist)
INSERT INTO permiso (CODIGO_PERMISO, NOMBRE_PERMISO, DESCRIPCION_PERMISO, CATEGORIA_PERMISO, ESTADO_PERMISO)
SELECT 'NOTIFICATION_VIEW', 'Ver Notificaciones', 'Permite al usuario ver sus notificaciones', 'Notificaciones', TRUE
WHERE NOT EXISTS (SELECT 1 FROM permiso WHERE CODIGO_PERMISO = 'NOTIFICATION_VIEW');

INSERT INTO permiso (CODIGO_PERMISO, NOMBRE_PERMISO, DESCRIPCION_PERMISO, CATEGORIA_PERMISO, ESTADO_PERMISO)
SELECT 'PUBLICATION_SAVE', 'Guardar Publicaciones', 'Permite al usuario guardar publicaciones para consultar después', 'Publicaciones', TRUE
WHERE NOT EXISTS (SELECT 1 FROM permiso WHERE CODIGO_PERMISO = 'PUBLICATION_SAVE');

INSERT INTO permiso (CODIGO_PERMISO, NOMBRE_PERMISO, DESCRIPCION_PERMISO, CATEGORIA_PERMISO, ESTADO_PERMISO)
SELECT 'MAP_VIEW', 'Ver Mapa de Calor', 'Permite al usuario visualizar el mapa de calor de casos de dengue', 'Estadísticas', TRUE
WHERE NOT EXISTS (SELECT 1 FROM permiso WHERE CODIGO_PERMISO = 'MAP_VIEW');
