using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorage.Domain.Services
{
    public interface IStorageService
    {
		Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType);
		Task<Stream> GetFileAsync(string filePath);
		Task DeleteFileAsync(string filePath);
		string GetFileUrl(string filePath);
	}
}
