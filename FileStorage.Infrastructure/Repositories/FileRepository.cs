
using Microsoft.EntityFrameworkCore;
using MyArchitechture.Domain.Entities;
using MyArchitechture.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = FileStorage.Domain.Entities.File;
namespace MyArchitechture.Infrastructure.Repositories
{
	public class FileRepository : GenericRepository<File>
	{
		private readonly DataDbContext _dbContext;
		public FileRepository(DataDbContext dbContext) : base(dbContext)
		{
			_dbContext = dbContext;
		}
		public async Task<IEnumerable<File>> SearchAsync(string searchTerm = null)
		{
			var query = _dbContext.Files.AsQueryable();

			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				var lowerSearchTerm = searchTerm.ToLower();

				query = query.Where(f =>
					f.Name.ToLower().Contains(lowerSearchTerm) ||
					f.OriginalName.ToLower().Contains(lowerSearchTerm)
				);
			}

			return await query.ToListAsync();
		}
		public async Task<File?> GetByIdAsync(Guid id)
		{
			return await _dbContext.Files.FindAsync(id);
		}
		public async Task DeleteAsync(Guid id)
		{
			var entity = await GetByIdAsync(id);
			if (entity != null)
			{
				_dbContext.Files.Remove(entity);
				await _dbContext.SaveChangesAsync();
			}
		}
	}
}
