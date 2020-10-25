using System;
using System.Collections.Generic;

namespace LevelSetData
{
	[Serializable]
	public class LevelSet
	{
		public const int ROWS = 25;
		public const int COLUMNS = 20;
		public List<Level> Levels { get; } = new List<Level>();

		public LevelSetProperties LevelSetProperties { get; set; } = new LevelSetProperties();

		public override string ToString() => $"{LevelSetProperties.Name} {Levels}";
	}
}
