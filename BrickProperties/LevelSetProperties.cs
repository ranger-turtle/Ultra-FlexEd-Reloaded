using System;

namespace LevelSetData
{
	[Serializable]
	public class LevelSetProperties
	{
		public string Name { get; set; } = "Level Set";
		public string DefaultBackgroundName { get; set; } = "<none>";
		public string DefaultMusic { get; set; } = "<none>";
		public SoundLibrary DefaultSoundLibrary { get; set; } = new SoundLibrary("<game-default>");
		
		public string DefaultLeftWallName { get; set; } = "<none>";
		public string DefaultRightWallName { get; set; } = "<none>";
	}
}