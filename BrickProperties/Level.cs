using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelSetData
{
	public class BrickInLevel
	{
		public int BrickId { get; set; }
		public bool Hidden { get; set; }
	}

	//TODO Add sound types
	public class Level
	{
		public string Name { get; set; } = "New Level";
		public string BackgroundName { get; set; }
		public string Music { get; set; }
		public BrickInLevel[,] Bricks { get; set; } = new BrickInLevel[LevelSet.ROWS, LevelSet.COLUMNS];

		public Level()
		{
			for (int i = 0; i < LevelSet.ROWS; i++)
			{
				for (int j = 0; j < LevelSet.COLUMNS; j++)
					Bricks[i, j] = new BrickInLevel();
			}
		}

		public Level(string name, string backgroundName) : this()
		{
			Name = name;
			BackgroundName = backgroundName;
		}

		public override string ToString() => Name;
	}
}
