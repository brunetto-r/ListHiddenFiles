using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListHiddenFiles
{
	internal class FakeFile : IFile
	{
		public string Name => throw new NotImplementedException();

		public FileAttributes Attributes => throw new NotImplementedException();

		public string FullName => throw new NotImplementedException();

		public long Size => throw new NotImplementedException();
	}
}
