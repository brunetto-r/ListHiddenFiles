using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListHiddenFiles
{
	internal class FakeFolder : IFolder
	{
		public IEnumerable<IFile> ListFiles()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IFolder> ListSubfolders()
		{
			throw new NotImplementedException();
		}
	}
}
