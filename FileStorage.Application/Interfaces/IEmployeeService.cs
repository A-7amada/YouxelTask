using MyArchitechture.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyArchitechture.Application.Interfaces
{
	public interface IEmployeeService
	{
		Task<EmployeeDto> CreateAsync(EmployeeDto employeeDto);
		Task<bool> DeleteAsync(int id);
		Task<IEnumerable<EmployeeDto>> GetAllAsync();
		Task<EmployeeDto?> GetByIdAsync(int id);
		Task<bool> UpdateAsync(int id, EmployeeDto employeeDto);
	}
}
