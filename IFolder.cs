using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListHiddenFiles
{
	internal interface IFolder
	{
		IEnumerable<IFolder> ListSubfolders();
		IEnumerable<IFile> ListFiles();
	}
}
