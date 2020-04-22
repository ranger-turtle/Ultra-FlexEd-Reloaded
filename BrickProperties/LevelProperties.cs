using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelSetData
{
	//TODO Add sound types
	public class LevelProperties : ICloneable
	{
		public string Name { get; set; } = "New Level";
		public string BackgroundName { get; set; }
		public string Music { get; set; }

		public object Clone()
		{
			return new LevelProperties
			{
				Name = Name,
				BackgroundName = BackgroundName,
				Music = Music
			};
		}
	}
}
