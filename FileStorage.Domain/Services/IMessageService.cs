using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = FileStorage.Domain.Entities.File;
namespace FileStorage.Domain.Services
{
    public interface IMessageService
    {
		void PublishFileCreated(File file);
		void PublishFileAccessed(File file);
		void PublishFileDeleted(File file);
	}
}
