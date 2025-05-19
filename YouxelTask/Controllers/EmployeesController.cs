using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyArchitechture.Application.Dtos;
using MyArchitechture.Application.Interfaces;

namespace MyArchitechture.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
		private readonly IEmployeeService _employeeService;
		public EmployeesController(IEmployeeService employeeService)
		{
			_employeeService = employeeService;
		}
		// GET: api/<EmployeesController>
		[HttpGet]
		public async Task<IEnumerable<EmployeeDto>> Get()
		{
			return await _employeeService.GetAllAsync();
		}
		// GET api/<EmployeesController>/5
		[HttpGet("{id}")]
		public async Task<EmployeeDto?> Get(int id)
		{
			return await _employeeService.GetByIdAsync(id);
		}
		// POST api/<EmployeesController>
		[HttpPost]
		public async Task <EmployeeDto> Post(EmployeeDto dto)
		{
			return await _employeeService.CreateAsync(dto);
		}
		// PUT api/<EmployeesController>/5
		[HttpPut("{id}")]
		public async Task<bool> Put(int id, EmployeeDto dto)
		{
			return await _employeeService.UpdateAsync(id, dto);
		}
		// DELETE api/<EmployeesController>/5
		[HttpDelete("{id}")]
		public async Task<bool> Delete(int id)
		{
			return await _employeeService.DeleteAsync(id);
		}
	}
}
