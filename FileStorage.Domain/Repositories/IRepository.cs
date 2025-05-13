using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileStorage.Domain.Entities;
using File = FileStorage.Domain.Entities.File;
namespace FileStorage.Domain.Repositories
{
	public interface IRepository<T> where T : class
	{
		Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
		Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
		IQueryable<T> Query();
		Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
		Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default);
		Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}

