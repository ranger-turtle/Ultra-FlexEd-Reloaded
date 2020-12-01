using System;
using System.Collections.Generic;
using System.Linq;

namespace LevelSetData
{
	[Serializable]
	public class LevelSet
	{
		public const int ROWS = 25;
		public const int COLUMNS = 20;
		public List<Level> Levels { get; } = new List<Level>();

		public LevelSetProperties LevelSetProperties { get; set; } = new LevelSetProperties();

		public bool BrickExistingInAnyLoadedLevel(int idOfCheckedBrick) => Levels.Select(l => l.Bricks.Cast<BrickInLevel>()).SelectMany(bc => bc).Any(b => b.BrickId == idOfCheckedBrick);

		public override string ToString() => $"{LevelSetProperties.Name} {Levels}";
	}
}
