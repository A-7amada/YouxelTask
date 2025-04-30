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
		Task<T> GetByIdAsync(Guid id);
		Task<T> AddAsync(T file);
		Task UpdateAsync(T file);
		Task DeleteAsync(Guid id);
	}
}
