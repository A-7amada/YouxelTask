using FileStorage.Application.Dtos;
using FileStorage.Application.Interfaces;
using FileStorage.Domain.Services;
using MyArchitechture.Infrastructure.Repositories;
using File = FileStorage.Domain.Entities.File;

namespace FileStorage.Application.Services
{
	public class FileService : IFileService
	{
		private readonly FileRepository _fileRepository;
		private readonly IStorageService _storageService;
		private readonly IMessageService _messageService;

		public FileService(FileRepository fileRepository, IStorageService storageService, IMessageService messageService)
		{
			_fileRepository = fileRepository;
			_storageService = storageService;
			_messageService = messageService;
		}

		public async Task<FileDto> UploadFileAsync(FileUploadDto fileUpload, string uploadedBy)
		{
			var physicalPath = await _storageService.SaveFileAsync(
				fileUpload.Content,
				fileUpload.FileName,
				fileUpload.ContentType);

			// Create file entity
			var file = new File
			{
				Id = Guid.NewGuid(),
				Name = Path.GetFileName(fileUpload.FileName),
				OriginalName = fileUpload.FileName,
				ContentType = fileUpload.ContentType,
				Size = fileUpload.Size,
				Path = CreateLogicalPath(),
				PhysicalPath = physicalPath,
				UploadedBy = uploadedBy,
				UploadedAt = DateTime.UtcNow,
				AccessCount = 0,
			};
			await _fileRepository.AddAsync(file);
			_messageService.PublishFileCreated(file);

			return MapToFileDto(file);
		}

		public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadFileAsync(Guid fileId)
		{
			var file = await _fileRepository.GetByIdAsync(fileId);

			if (file == null)
				throw new FileNotFoundException($"File with ID {fileId} not found");

			file.LastAccessed = DateTime.UtcNow;
			file.AccessCount++;
			await _fileRepository.UpdateAsync(file);

			var fileStream = await _storageService.GetFileAsync(file.PhysicalPath);
			_messageService.PublishFileAccessed(file);

			return (fileStream, file.ContentType, file.Name);
		}

		public async Task<FileDto> GetFileInfoAsync(Guid fileId)
		{
			var file = await _fileRepository.GetByIdAsync(fileId);

			if (file == null)
				throw new FileNotFoundException($"File with ID {fileId} not found");

			return MapToFileDto(file);
		}

		public async Task DeleteFileAsync(Guid fileId)
		{
			var file = await _fileRepository.GetByIdAsync(fileId);

			if (file == null)
				throw new FileNotFoundException($"File with ID {fileId} not found");

			await _storageService.DeleteFileAsync(file.PhysicalPath);
			_messageService.PublishFileDeleted(file);
			await _fileRepository.DeleteAsync(fileId);
		}

		public async Task<IEnumerable<FileDto>> SearchFilesAsync(string tenantId, string searchTerm = null)
		{
			var files = await _fileRepository.SearchAsync(searchTerm);
			return files.Select(MapToFileDto);
		}

		private FileDto MapToFileDto(File file)
		{
			return new FileDto
			{
				Id = file.Id,
				Name = file.Name,
				ContentType = file.ContentType,
				Size = file.Size,
				DownloadUrl = _storageService.GetFileUrl(file.Id.ToString()),
				UploadedAt = file.UploadedAt,
			};
		}

		private string CreateLogicalPath()
		{
			var now = DateTime.UtcNow;
			return $"{now.Year}/{now.Month:D2}/{now.Day:D2}";
		}
	}
}
