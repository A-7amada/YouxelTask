using FileStorage.Domain.Repositories;
using MyArchitechture.Application.Dtos;
using MyArchitechture.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyArchitechture.Domain.Entities;
using AutoMapper;
using MyArchitechture.Application.Dtos;
using MyArchitechture.Application.Interfaces;
namespace MyArchitechture.Application.Services.Employee
{
	public class EmployeeService : IEmployeeService
	{
		private readonly IRepository<MyArchitechture.Domain.Entities.Employee> _repository;
		private readonly IMapper _mapper;

		public EmployeeService(IRepository<MyArchitechture.Domain.Entities.Employee> repository, IMapper mapper)
		{
			_repository = repository;
			_mapper = mapper;
		}

		public async Task<IEnumerable<EmployeeDto>> GetAllAsync()
		{
			var employees = await _repository.GetAllAsync(CancellationToken.None);
			return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
		}

		public async Task<EmployeeDto?> GetByIdAsync(int id)
		{
			var employee = await _repository.GetByIdAsync(id, CancellationToken.None);
			return employee == null ? null : _mapper.Map<EmployeeDto>(employee);
		}

		public async Task<EmployeeDto> CreateAsync(EmployeeDto employeeDto)
		{
			var employee = _mapper.Map<MyArchitechture.Domain.Entities.Employee>(employeeDto);
			employee.UploadedAt = DateTime.UtcNow;
			employee.UploadedBy = "System"; 

			var created = await _repository.AddAsync(employee, CancellationToken.None);
			return _mapper.Map<EmployeeDto>(created);
		}

		public async Task<bool> UpdateAsync(int id, EmployeeDto employeeDto)
		{
			var existing = await _repository.GetByIdAsync(id, CancellationToken.None);
			if (existing == null)
			{
				return false;
			}

			_mapper.Map(employeeDto, existing);
			return await _repository.UpdateAsync(existing, CancellationToken.None);
		}

		public async Task<bool> DeleteAsync(int id)
		{
			return await _repository.DeleteAsync(id, CancellationToken.None);
		}
	}
}
