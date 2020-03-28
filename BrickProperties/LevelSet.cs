using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelSetData
{
	public class LevelSet
	{
		public static readonly int ROWS = 25;
		public static readonly int COLUMNS = 20;
		public string Name { get; set; } = "Level Set";
		public List<Level> Levels { get; set; } = new List<Level> { new Level() };
		public List<BrickProperties> Bricks { get; set; }
	}
}
