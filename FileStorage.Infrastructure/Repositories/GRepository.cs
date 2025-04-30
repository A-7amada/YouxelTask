using FileStorage.Domain.Repositories;
using FileStorage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = FileStorage.Domain.Entities.File;
namespace FileStorage.Infrastructure.Repositories
{
	public class GRepository<T> : IRepository<T> where T : class
	{
		private readonly FileStorageDbContext _dbContext;
		private readonly DbSet<T> _dbSet;

		public GRepository(FileStorageDbContext dbContext)
		{
			_dbContext = dbContext;
			_dbSet = _dbContext.Set<T>();
		}

		public async Task<T> GetByIdAsync(Guid id)
		{
			return await _dbSet.FindAsync(id);
		}

		public async Task<T> AddAsync(T entity)
		{
			await _dbSet.AddAsync(entity);
			await _dbContext.SaveChangesAsync();
			return entity;
		}

		public async Task UpdateAsync(T entity)
		{
			_dbSet.Update(entity);
			await _dbContext.SaveChangesAsync();
		}

		public async Task DeleteAsync(Guid id)
		{
			var entity = await GetByIdAsync(id);
			if (entity != null)
			{
				_dbSet.Remove(entity);
				await _dbContext.SaveChangesAsync();
			}
		}
	}
}

