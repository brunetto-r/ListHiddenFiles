using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListHiddenFiles
{
	internal class RealFile : RealFileOrFolder, IFile
	{
		public RealFile(FileInfo info) : base(info)
		{
		}
		public long Size => ((FileInfo)info).Length;
	}
}
