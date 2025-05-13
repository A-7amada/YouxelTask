using FileStorage.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using MyArchitechture.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = FileStorage.Domain.Entities.File;
namespace MyArchitechture.Infrastructure.Repositories
{
	public class GenericRepository<T> : IRepository<T> where T : class
	{
		private readonly DataDbContext _dbContext;
		private readonly DbSet<T> _dbSet;

		public GenericRepository(DataDbContext dbContext)
		{
			_dbContext = dbContext;
			_dbSet = _dbContext.Set<T>();
		}

		public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			return await _dbSet.FindAsync(id, cancellationToken);
		}

		public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
		}

		public IQueryable<T> Query()
		{
			return _dbSet.AsNoTracking();
		}

		public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
		{
			await _dbSet.AddAsync(entity, cancellationToken);
			await _dbContext.SaveChangesAsync(cancellationToken);
			return entity;
		}

		public async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
		{
			_dbSet.Update(entity);
			try
			{
				await _dbContext.SaveChangesAsync(cancellationToken);
				return true;
			}
			catch (DbUpdateConcurrencyException)
			{
				return false;
			}
		}

		public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
		{
			var entity = await GetByIdAsync(id, cancellationToken);
			if (entity == null)
			{
				return false;
			}

			_dbSet.Remove(entity);
			await _dbContext.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return await _dbContext.SaveChangesAsync(cancellationToken);
		}
	}
}

