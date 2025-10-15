using Backend_App_Dengue.Data.Entities;
using Backend_App_Dengue.Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Backend_App_Dengue.Controllers
{
    [Route("Department")]
    [ApiController]
    public class DepartmentControllerEF : ControllerBase
    {
        private readonly IRepository<Department> _departmentRepository;

        public DepartmentControllerEF(IRepository<Department> departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        /// <summary>
        /// Get all departments
        /// </summary>
        [HttpGet]
        [Route("getDepartments")]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments = await _departmentRepository.GetAllAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener departamentos", error = ex.Message });
            }
        }

        /// <summary>
        /// Get department by ID
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            try
            {
                var department = await _departmentRepository.GetByIdAsync(id);

                if (department == null)
                {
                    return NotFound(new { message = "Departamento no encontrado" });
                }

                return Ok(department);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el departamento", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new department
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] Department department)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(department.Name))
                {
                    return BadRequest(new { message = "El nombre del departamento es requerido" });
                }

                var createdDepartment = await _departmentRepository.AddAsync(department);
                return CreatedAtAction(nameof(GetDepartmentById), new { id = createdDepartment.Id }, createdDepartment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el departamento", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing department
        /// </summary>
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] Department department)
        {
            try
            {
                var existingDepartment = await _departmentRepository.GetByIdAsync(id);

                if (existingDepartment == null)
                {
                    return NotFound(new { message = "Departamento no encontrado" });
                }

                existingDepartment.Name = department.Name;
                existingDepartment.IsActive = department.IsActive;

                await _departmentRepository.UpdateAsync(existingDepartment);
                return Ok(new { message = "Departamento actualizado con éxito", department = existingDepartment });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el departamento", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a department
        /// </summary>
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var department = await _departmentRepository.GetByIdAsync(id);

                if (department == null)
                {
                    return NotFound(new { message = "Departamento no encontrado" });
                }

                await _departmentRepository.DeleteAsync(department);
                return Ok(new { message = "Departamento eliminado con éxito" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el departamento", error = ex.Message });
            }
        }
    }
}
