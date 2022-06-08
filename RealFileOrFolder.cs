using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListHiddenFiles
{
	internal class RealFileOrFolder : IFileOrFolder
	{
		readonly FileSystemInfo info;
		public RealFileOrFolder(FileSystemInfo info)
		{ 
			this.info = info;
		}
		public string Name => info.Name;

		public FileAttributes Attributes => info.Attributes;

		public string FullName => info.FullName;
	}
}
