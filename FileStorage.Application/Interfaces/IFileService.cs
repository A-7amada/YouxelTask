using FileStorage.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorage.Application.Interfaces
{
	public interface IFileService
	{
		Task<FileDto> UploadFileAsync(FileUploadDto fileUpload, string uploadedBy);
		Task<(Stream FileStream, string ContentType, string FileName)> DownloadFileAsync(Guid fileId);
		Task<FileDto> GetFileInfoAsync(Guid fileId);
		Task DeleteFileAsync(Guid fileId);
		Task<IEnumerable<FileDto>> SearchFilesAsync(string tenantId, string searchTerm = null);
	}
}
