using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelSetData
{
	public class BrickInLevel : ICloneable
	{
		public int BrickId { get; set; }
		public bool Hidden { get; set; }

		public void Reset()
		{
			BrickId = 0;
			Hidden = false;
		}

		public object Clone()
		{
			return new BrickInLevel
			{
				BrickId = BrickId,
				Hidden = Hidden
			};
		}
	}

	public class Level : ICloneable
	{
		public LevelProperties LevelProperties { get; set; } = new LevelProperties();
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
			LevelProperties.Name = name;
			LevelProperties.BackgroundName = backgroundName;
		}

		public override string ToString() => LevelProperties.Name;

		public object Clone()
		{
			return new Level
			{
				LevelProperties = LevelProperties.Clone() as LevelProperties
			};
		}
	}
}
