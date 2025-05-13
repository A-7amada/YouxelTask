using FileStorage.Domain.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyArchitechture.Infrastructure.Repositories
{
	public class CachingRepositoryDecorator<T> : IRepository<T> where T : class
	{
		private readonly IRepository<T> _inner;
		private readonly IDistributedCache _cache;
		private readonly DistributedCacheEntryOptions _cacheOptions;

		public CachingRepositoryDecorator(
			IRepository<T> inner,
			IDistributedCache cache)
		{
			_inner = inner;
			_cache = cache;
			_cacheOptions = new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
				SlidingExpiration = TimeSpan.FromMinutes(2)
			};
		}

		private string GetKey(string method, int? id = null)
			=> $"{typeof(T).Name}:{method}:{(id.HasValue ? id.ToString() : "all")}";

		public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			var key = GetKey(nameof(GetByIdAsync), id);
			var cached = await _cache.GetStringAsync(key, cancellationToken);
			if (cached != null)
				return JsonSerializer.Deserialize<T>(cached);

			var entity = await _inner.GetByIdAsync(id, cancellationToken);
			if (entity != null)
			{
				var json = JsonSerializer.Serialize(entity);
				await _cache.SetStringAsync(key, json, _cacheOptions, cancellationToken);
			}
			return entity;
		}

		public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			var key = GetKey(nameof(GetAllAsync));
			var cached = await _cache.GetStringAsync(key, cancellationToken);
			if (cached != null)
				return JsonSerializer.Deserialize<IEnumerable<T>>(cached);

			var list = await _inner.GetAllAsync(cancellationToken);
			var json = JsonSerializer.Serialize(list);
			await _cache.SetStringAsync(key, json, _cacheOptions, cancellationToken);
			return list;
		}

		public IQueryable<T> Query()
			=> _inner.Query();  // do not cache queryable

		public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
		{
			var added = await _inner.AddAsync(entity, cancellationToken);
			// invalidate caches
			await _cache.RemoveAsync(GetKey(nameof(GetAllAsync)), cancellationToken);
			return added;
		}

		public async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
		{
			var result = await _inner.UpdateAsync(entity, cancellationToken);
			if (result)
			{
				// invalidate caches
				var idProp = entity.GetType().GetProperty("Id")?.GetValue(entity) as int?;
				if (idProp.HasValue)
					await _cache.RemoveAsync(GetKey(nameof(GetByIdAsync), idProp), cancellationToken);
				await _cache.RemoveAsync(GetKey(nameof(GetAllAsync)), cancellationToken);
			}
			return result;
		}

		public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
		{
			var result = await _inner.DeleteAsync(id, cancellationToken);
			if (result)
			{
				await _cache.RemoveAsync(GetKey(nameof(GetByIdAsync), id), cancellationToken);
				await _cache.RemoveAsync(GetKey(nameof(GetAllAsync)), cancellationToken);
			}
			return result;
		}

		public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
			=> _inner.SaveChangesAsync(cancellationToken);
	}
}
