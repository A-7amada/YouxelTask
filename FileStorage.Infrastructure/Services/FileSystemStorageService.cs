using FileStorage.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorage.Infrastructure.Services
{
	public class FileSystemStorageService : IStorageService
	{
		private readonly string _basePath;
		private readonly string _baseUrl;
		private readonly ILogger<FileSystemStorageService> _logger;

		public FileSystemStorageService(IConfiguration configuration, ILogger<FileSystemStorageService> logger)
		{
			_basePath = configuration["Storage:FilePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Storage");
			_baseUrl = configuration["Storage:BaseUrl"] ?? "/api/files";
			_logger = logger;

			if (!Directory.Exists(_basePath))
			{
				Directory.CreateDirectory(_basePath);
			}
		}

		public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)
		{
			var fileId = Guid.NewGuid().ToString();
			var extension = Path.GetExtension(fileName);
			var storedFileName = $"{fileId}{extension}";
			var folderPath = CreateFolderStructure(fileId);
			var filePath = Path.Combine(folderPath, storedFileName);

			try
			{
				using (var fileStream2 = new FileStream(filePath, FileMode.Create))
				{
					await fileStream.CopyToAsync(fileStream2);
				}

				return filePath;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to save file {FileName}", fileName);
				throw;
			}
		}

		public Task<Stream> GetFileAsync(string filePath)
		{
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException($"File not found at path: {filePath}");
			}

			return Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open, FileAccess.Read));
		}

		public Task DeleteFileAsync(string filePath)
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
				try
				{
					var directory = Path.GetDirectoryName(filePath);
					if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
					{
						Directory.Delete(directory);
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Failed to clean up directory after file deletion");
				}
			}

			return Task.CompletedTask;
		}

		public string GetFileUrl(string fileId)
		{
			return $"{_baseUrl}/{fileId}/download";
		}

		private string CreateFolderStructure(string fileId)
		{
			// folder1 1 firt two characters of the fileId
			// folder1 2 second two characters of the fileId

			var folder1 = fileId.Substring(0, 2);
			var folder2 = fileId.Substring(2, 2);

			var path = Path.Combine(_basePath, folder1, folder2);

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			return path;
		}
	}
}
