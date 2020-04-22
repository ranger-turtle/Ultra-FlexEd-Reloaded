using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelSetData
{
	public class LevelSet
	{
		public const int ROWS = 25;
		public const int COLUMNS = 20;
		public string Name { get; set; } = "Level Set";
		public List<Level> Levels { get; } = new List<Level>();

		public override string ToString() => $"{Name} {Levels}";
	}
}
