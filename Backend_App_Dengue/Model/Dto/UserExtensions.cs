using Backend_App_Dengue.Data.Entities;

namespace Backend_App_Dengue.Model.Dto
{
    /// <summary>
    /// Extension methods for User entity to convert to DTOs
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// Convert User entity to UserResponseDto for Android compatibility
        /// Includes all related entity names
        /// </summary>
        public static UserResponseDto ToResponseDto(this User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Password = null, // Never expose password
                Address = user.Address,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name,
                CityId = user.CityId,
                CityName = user.City?.Name,
                BloodTypeId = user.BloodTypeId,
                BloodTypeName = user.BloodType?.Name,
                GenreId = user.GenreId,
                GenreName = user.Genre?.Name,
                DepartmentId = user.City?.DepartmentId ?? 0,
                UserStateName = user.IsActive ? "Activo" : "Inactivo"
            };
        }

        /// <summary>
        /// Convert list of Users to list of UserResponseDto
        /// </summary>
        public static IEnumerable<UserResponseDto> ToResponseDto(this IEnumerable<User> users)
        {
            return users.Select(u => u.ToResponseDto());
        }
    }
}
