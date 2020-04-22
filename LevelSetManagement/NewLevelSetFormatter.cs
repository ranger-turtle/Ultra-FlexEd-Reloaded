using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using LevelSetData;

namespace LevelSetManagement
{
	internal class NewLevelSetFormatter : ILevelSetFormatter
	{
		public LevelSet Load(string filepath)
		{
			using (FileStream fileStream = File.OpenRead(filepath))
				return new BinaryFormatter().Deserialize(fileStream) as LevelSet;
		}

		public void Save(string filepath, LevelSet levelSet)
		{
			using (FileStream fileStream = File.Open(filepath, FileMode.CreateNew))
				new BinaryFormatter().Serialize(fileStream, levelSet);
		}
	}
}
