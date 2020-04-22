using LevelSetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelSetManagement
{
	public interface ILevelSetFormatter
	{
		LevelSet Load(string filename);
		void Save(string filename, LevelSet levelSet);
	}
}
