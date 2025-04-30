
using YouxelTask.FileStorage.Domain.Entities;

namespace FileStorage.Domain.Entities
{
	public class File : Audited
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string OriginalName { get; set; }
		public string ContentType { get; set; }
		public long Size { get; set; }
		public string Path { get; set; }
		public string PhysicalPath { get; set; }
		public int AccessCount { get; set; }
		
	}

}
