using System;

namespace LevelSetData
{
	[Serializable]
	public class LevelProperties : ICloneable
	{
		public string Name { get; set; } = "New Level";
		public string BackgroundName { get; set; } = "<level-set-default>";
		public string Music { get; set; } = "<level-set-default>";

		public SoundLibrary SoundLibrary { get; set; } = new SoundLibrary();

		public object Clone()
		{
			return new LevelProperties
			{
				Name = Name,
				BackgroundName = BackgroundName,
				Music = Music,
				SoundLibrary = SoundLibrary
			};
		}
	}
}
