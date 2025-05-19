using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorage.Application.Dtos
{
	public class FileDto
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string ContentType { get; set; }
		public long Size { get; set; }
		public string DownloadUrl { get; set; }
		public DateTime? UploadedAt { get; set; }
		
	}
}
