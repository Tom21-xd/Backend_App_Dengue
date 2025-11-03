namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// DTO para solicitudes de aprobación de usuarios
    /// </summary>
    public class ApprovalRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // PENDIENTE, APROBADO, RECHAZADO
        public int RequestedRoleId { get; set; }
        public string RequestedRoleName { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public int? ApprovedByAdminId { get; set; }
        public string? ApprovedByAdminName { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ResolutionDate { get; set; }
        public string? RethusData { get; set; }
        public string? RethusError { get; set; }
    }
}
