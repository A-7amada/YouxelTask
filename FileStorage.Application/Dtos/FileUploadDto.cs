using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorage.Application.Dtos
{
	public class FileUploadDto
	{
		public Stream Content { get; set; }
		public string FileName { get; set; }
		public string ContentType { get; set; }
		public long Size { get; set; }
	}
}
