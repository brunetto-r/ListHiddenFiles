using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListHiddenFiles
{
	internal class RealFolder : RealFileOrFolder, IFolder
	{
		public RealFolder(DirectoryInfo info) : base(info)
		{
		}

		public IEnumerable<IFolder> GetDirectories()
		{
			return ((DirectoryInfo)info).GetDirectories().Select(sub => new RealFolder(sub));
		}

		public IEnumerable<IFile> GetFiles()
		{
			return ((DirectoryInfo)info).GetFiles().Select(sub => new RealFile(sub));
		}
	}
}
