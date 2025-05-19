using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouxelTask.FileStorage.Domain.Entities
{
    public class Audited
    {
		public string UploadedBy { get; set; }
		public DateTime? UploadedAt { get; set; }
		public DateTime? LastAccessed { get; set; }
	}
}
