using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
	public class UploadFileRequest
	{
		[Required]
		public IFormFile File { get; set; }

	}
}
