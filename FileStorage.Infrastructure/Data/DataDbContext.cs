using Microsoft.EntityFrameworkCore;
using MyArchitechture.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using File = FileStorage.Domain.Entities.File;
namespace MyArchitechture.Infrastructure.Data
{
	public class DataDbContext : DbContext
	{
		public DataDbContext(DbContextOptions<DataDbContext> options) : base(options)
		{
		}


		public DbSet<File> Files { get; set; }
		public DbSet<Employee> Employees { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<File>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
				entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
				entity.Property(e => e.PhysicalPath).IsRequired();
				entity.HasIndex(e => e.Name);
			});
		}
	}
}
