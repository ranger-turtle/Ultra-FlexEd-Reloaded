using System;

namespace LevelSetData
{
	//TODO convert it to id on release if it won't be needed
	[Serializable]
	public class BrickInLevel : ICloneable
	{
		public int BrickId { get; set; }

		public void Reset() => BrickId = 0;

		public object Clone()
		{
			return new BrickInLevel
			{
				BrickId = BrickId,
			};
		}
	}

	[Serializable]
	public class Level : ICloneable
	{
		public LevelProperties LevelProperties { get; set; } = new LevelProperties();
		public BrickInLevel[,] Bricks { get; set; } = new BrickInLevel[LevelSet.ROWS, LevelSet.COLUMNS];

		public Level() => Clear();

		public Level(string name, string backgroundName) : this()
		{
			LevelProperties.Name = name;
			LevelProperties.BackgroundName = backgroundName;
		}

		public void Clear()
		{
			for (int i = 0; i < LevelSet.ROWS; i++)
			{
				for (int j = 0; j < LevelSet.COLUMNS; j++)
					Bricks[i, j] = new BrickInLevel();
			}
		}

		public override string ToString() => LevelProperties.Name;

		public object Clone() => new Level
		{
			LevelProperties = LevelProperties.Clone() as LevelProperties
		};
	}
}
