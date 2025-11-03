namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// DTO para rechazar un usuario
    /// </summary>
    public class RejectUserDto
    {
        public int UserId { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
    }
}
