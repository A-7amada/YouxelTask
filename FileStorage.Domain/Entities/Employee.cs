using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouxelTask.FileStorage.Domain.Entities;

namespace MyArchitechture.Domain.Entities
{
	public class Employee : Audited
	{
		/// <summary>
		/// Primary key
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// First name of the employee
		/// </summary>
		public string FirstName { get; set; } = string.Empty;

		/// <summary>
		/// Last name of the employee
		/// </summary>
		public string LastName { get; set; } = string.Empty;

		/// <summary>
		/// Full name, derived from first+last name
		/// </summary>
		public string FullName => $"{FirstName} {LastName}";

		/// <summary>
		/// Email address
		/// </summary>
		public string Email { get; set; } = string.Empty;

		/// <summary>
		/// Date the employee was hired
		/// </summary>
		public DateTime HireDate { get; set; }

		/// <summary>
		/// Current salary
		/// </summary>
		public decimal Salary { get; set; }

		/// <summary>
		/// Department the employee belongs to
		/// </summary>
		public string Department { get; set; } = string.Empty;

		/// <summary>
		/// Optional manager Id (null if top‐level)
		/// </summary>
		public int? ManagerId { get; set; }

		/// <summary>
		/// Navigation property (if using an ORM)
		/// </summary>
		public Employee? Manager { get; set; }
		// public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
	}

}
