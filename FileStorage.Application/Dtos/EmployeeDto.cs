using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyArchitechture.Application.Dtos
{
	public class EmployeeDto
	{
		/// <summary>
		/// Unique identifier
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// First name
		/// </summary>
		public string FirstName { get; set; } = string.Empty;

		/// <summary>
		/// Last name
		/// </summary>
		public string LastName { get; set; } = string.Empty;

		/// <summary>
		/// Email address
		/// </summary>
		public string Email { get; set; } = string.Empty;

		/// <summary>
		/// Date the employee was hired (ISO format)
		/// </summary>
		public DateTime HireDate { get; set; }

		/// <summary>
		/// Current salary (read-only in DTO, if you don't want clients to modify)
		/// </summary>
		public decimal Salary { get; init; }

		/// <summary>
		/// Department name
		/// </summary>
		public string Department { get; set; } = string.Empty;

		/// <summary>
		/// Manager’s Id (null if top-level)
		/// </summary>
		public int? ManagerId { get; set; }
	}
}
