using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LevelSetData;

namespace Ultra_FlexEd_Reloaded.LevelManagement
{
	public class LevelManager
	{
		public LevelSet LevelSet { get; set; } = new LevelSet();
		public uint CurrentBrickId { get; set; } = 1;
		public int CurrentLevelIndex { get; set; } = 0;
		public bool Hidden { get; set; }
	}
}
