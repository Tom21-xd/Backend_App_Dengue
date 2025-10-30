namespace Backend_App_Dengue.Data.Enums
{
    /// <summary>
    /// Códigos de permisos del sistema para control de acceso granular
    /// </summary>
    public static class PermissionCode
    {
        // Casos - Gestión básica
        public const string CASE_VIEW = "CASE_VIEW";
        public const string CASE_VIEW_ALL = "CASE_VIEW_ALL";
        public const string CASE_CREATE = "CASE_CREATE";
        public const string CASE_UPDATE = "CASE_UPDATE";
        public const string CASE_DELETE = "CASE_DELETE";
        public const string CASE_IMPORT_CSV = "CASE_IMPORT_CSV";
        public const string CASE_EXPORT = "CASE_EXPORT";

        // Usuarios
        public const string USER_VIEW = "USER_VIEW";
        public const string USER_VIEW_ALL = "USER_VIEW_ALL";
        public const string USER_CREATE = "USER_CREATE";
        public const string USER_UPDATE = "USER_UPDATE";
        public const string USER_DELETE = "USER_DELETE";

        // Hospitales
        public const string HOSPITAL_VIEW = "HOSPITAL_VIEW";
        public const string HOSPITAL_CREATE = "HOSPITAL_CREATE";
        public const string HOSPITAL_UPDATE = "HOSPITAL_UPDATE";
        public const string HOSPITAL_DELETE = "HOSPITAL_DELETE";

        // Publicaciones
        public const string PUBLICATION_VIEW = "PUBLICATION_VIEW";
        public const string PUBLICATION_CREATE = "PUBLICATION_CREATE";
        public const string PUBLICATION_UPDATE = "PUBLICATION_UPDATE";
        public const string PUBLICATION_DELETE = "PUBLICATION_DELETE";

        // Notificaciones
        public const string NOTIFICATION_SEND = "NOTIFICATION_SEND";
        public const string NOTIFICATION_VIEW_ALL = "NOTIFICATION_VIEW_ALL";

        // Estadísticas y Reportes
        public const string STATISTICS_VIEW = "STATISTICS_VIEW";
        public const string REPORTS_GENERATE = "REPORTS_GENERATE";

        // Administración del Sistema
        public const string ROLE_MANAGE = "ROLE_MANAGE";
        public const string PERMISSION_MANAGE = "PERMISSION_MANAGE";
        public const string SYSTEM_CONFIG = "SYSTEM_CONFIG";

        // Quiz y Certificados
        public const string QUIZ_VIEW = "QUIZ_VIEW";
        public const string QUIZ_MANAGE = "QUIZ_MANAGE";
        public const string CERTIFICATE_VIEW = "CERTIFICATE_VIEW";
    }
}
