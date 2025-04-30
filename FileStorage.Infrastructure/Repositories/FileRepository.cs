using FileStorage.Infrastructure.Data;
using FileStorage.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = FileStorage.Domain.Entities.File;
namespace YouxelTask.FileStorage.Infrastructure.Repositories
{
	public class FileRepository : GRepository<File>
	{
		private readonly FileStorageDbContext _dbContext;
		public FileRepository(FileStorageDbContext dbContext) : base(dbContext)
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
	}
}
