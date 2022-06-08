using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListHiddenFiles
{
	internal class FakeFolder : IFolder
	{
		public string Name => throw new NotImplementedException();

		public FileAttributes Attributes => throw new NotImplementedException();

		public string FullName => throw new NotImplementedException();

		public IEnumerable<IFile> GetFiles()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IFolder> GetDirectories()
		{
			throw new NotImplementedException();
		}
	}
}
