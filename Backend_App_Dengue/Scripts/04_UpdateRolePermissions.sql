-- Script to update role-permission assignments
-- Date: 2025-10-30
-- Description: Configures permissions for each role based on business rules
-- Usuario Regular (Rol 1): Basic permissions (map, publications, notifications, profile, quiz, certificates)
-- Administrador (Rol 2): ALL 33 permissions
-- Personal Médico (Rol 3): ALL 33 permissions (same as admin)

USE dengue_db;

-- First, deactivate all existing role-permission assignments
UPDATE rol_permiso SET ESTADO_ROL_PERMISO = 0;

-- ===========================
-- USUARIO REGULAR (ROL 1) - BASIC PERMISSIONS ONLY
-- ===========================
-- Insert Usuario Regular permissions (if they don't exist)
INSERT INTO rol_permiso (FK_ID_ROL, FK_ID_PERMISO, FECHA_ASIGNACION, ESTADO_ROL_PERMISO)
SELECT 1, 8, NOW(), 1 WHERE NOT EXISTS (SELECT 1 FROM rol_permiso WHERE FK_ID_ROL = 1 AND FK_ID_PERMISO = 8);

INSERT INTO rol_permiso (FK_ID_ROL, FK_ID_PERMISO, FECHA_ASIGNACION, ESTADO_ROL_PERMISO)
SELECT 1, 17, NOW(), 1 WHERE NOT EXISTS (SELECT 1 FROM rol_permiso WHERE FK_ID_ROL = 1 AND FK_ID_PERMISO = 17);

INSERT INTO rol_permiso (FK_ID_ROL, FK_ID_PERMISO, FECHA_ASIGNACION, ESTADO_ROL_PERMISO)
SELECT 1, 23, NOW(), 1 WHERE NOT EXISTS (SELECT 1 FROM rol_permiso WHERE FK_ID_ROL = 1 AND FK_ID_PERMISO = 23);

INSERT INTO rol_permiso (FK_ID_ROL, FK_ID_PERMISO, FECHA_ASIGNACION, ESTADO_ROL_PERMISO)
SELECT 1, 28, NOW(), 1 WHERE NOT EXISTS (SELECT 1 FROM rol_permiso WHERE FK_ID_ROL = 1 AND FK_ID_PERMISO = 28);

INSERT INTO rol_permiso (FK_ID_ROL, FK_ID_PERMISO, FECHA_ASIGNACION, ESTADO_ROL_PERMISO)
SELECT 1, 30, NOW(), 1 WHERE NOT EXISTS (SELECT 1 FROM rol_permiso WHERE FK_ID_ROL = 1 AND FK_ID_PERMISO = 30);

INSERT INTO rol_permiso (FK_ID_ROL, FK_ID_PERMISO, FECHA_ASIGNACION, ESTADO_ROL_PERMISO)
SELECT 1, 31, NOW(), 1 WHERE NOT EXISTS (SELECT 1 FROM rol_permiso WHERE FK_ID_ROL = 1 AND FK_ID_PERMISO = 31);

INSERT INTO rol_permiso (FK_ID_ROL, FK_ID_PERMISO, FECHA_ASIGNACION, ESTADO_ROL_PERMISO)
SELECT 1, 32, NOW(), 1 WHERE NOT EXISTS (SELECT 1 FROM rol_permiso WHERE FK_ID_ROL = 1 AND FK_ID_PERMISO = 32);

INSERT INTO rol_permiso (FK_ID_ROL, FK_ID_PERMISO, FECHA_ASIGNACION, ESTADO_ROL_PERMISO)
SELECT 1, 33, NOW(), 1 WHERE NOT EXISTS (SELECT 1 FROM rol_permiso WHERE FK_ID_ROL = 1 AND FK_ID_PERMISO = 33);

-- Reactivate Usuario Regular permissions
UPDATE rol_permiso SET ESTADO_ROL_PERMISO = 1, FECHA_ASIGNACION = NOW()
WHERE FK_ID_ROL = 1 AND FK_ID_PERMISO IN (8, 17, 23, 28, 30, 31, 32, 33);

-- ===========================
-- ADMINISTRADOR (ROL 2) - ALL PERMISSIONS
-- ===========================
-- Insert all permissions for Administrator (if they don't exist)
INSERT INTO rol_permiso (FK_ID_ROL, FK_ID_PERMISO, FECHA_ASIGNACION, ESTADO_ROL_PERMISO)
SELECT 2, p.ID_PERMISO, NOW(), 1
FROM permiso p
WHERE p.ESTADO_PERMISO = 1
  AND NOT EXISTS (
    SELECT 1 FROM rol_permiso rp
    WHERE rp.FK_ID_ROL = 2 AND rp.FK_ID_PERMISO = p.ID_PERMISO
  );

-- Reactivate all permissions for Administrator
UPDATE rol_permiso SET ESTADO_ROL_PERMISO = 1, FECHA_ASIGNACION = NOW()
WHERE FK_ID_ROL = 2;

-- ===========================
-- PERSONAL MÉDICO (ROL 3) - ALL PERMISSIONS (same as Admin)
-- ===========================
-- Insert all permissions for Medical Staff (if they don't exist)
INSERT INTO rol_permiso (FK_ID_ROL, FK_ID_PERMISO, FECHA_ASIGNACION, ESTADO_ROL_PERMISO)
SELECT 3, p.ID_PERMISO, NOW(), 1
FROM permiso p
WHERE p.ESTADO_PERMISO = 1
  AND NOT EXISTS (
    SELECT 1 FROM rol_permiso rp
    WHERE rp.FK_ID_ROL = 3 AND rp.FK_ID_PERMISO = p.ID_PERMISO
  );

-- Reactivate all permissions for Medical Staff
UPDATE rol_permiso SET ESTADO_ROL_PERMISO = 1, FECHA_ASIGNACION = NOW()
WHERE FK_ID_ROL = 3;

-- ===========================
-- VERIFICATION QUERY
-- ===========================
SELECT
    r.ID_ROL,
    r.NOMBRE_ROL,
    COUNT(rp.FK_ID_PERMISO) as total_permissions
FROM rol r
LEFT JOIN rol_permiso rp ON r.ID_ROL = rp.FK_ID_ROL AND rp.ESTADO_ROL_PERMISO = TRUE
GROUP BY r.ID_ROL, r.NOMBRE_ROL
ORDER BY r.ID_ROL;
