namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// DTO para aprobar un usuario
    /// </summary>
    public class ApproveUserDto
    {
        public int UserId { get; set; }
        public int NewRoleId { get; set; } // Rol que se le asignará al aprobar
    }
}
