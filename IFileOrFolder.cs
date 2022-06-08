using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListHiddenFiles
{
	interface IFileOrFolder
	{
		string Name { get; }
		FileAttributes Attributes { get; }
		string FullName { get; }
	}
}
