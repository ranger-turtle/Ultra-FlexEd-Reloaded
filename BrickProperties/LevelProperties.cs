using System;

namespace LevelSetData
{
	[Serializable]
	public class LevelProperties : ICloneable
	{
		public string Name { get; set; } = "New Level";
		public string BackgroundName { get; set; } = "<level-set-default>";
		public string Music { get; set; } = "<level-set-default>";

		#region Quote Window Properties
		public string CharacterName { get; set; } = "<none>";
		public string Quote { get; set; } = string.Empty;
		public bool IsQuoteTip { get; set; } = true;
		#endregion

		public SoundLibrary SoundLibrary { get; set; } = new SoundLibrary("<level-set-default>");

		public string LeftWallName { get; set; } = "<level-set-default>";
		public string RightWallName { get; set; } = "<level-set-default>";

		public object Clone()
		{
			return new LevelProperties
			{
				Name = Name,
				BackgroundName = BackgroundName,
				Music = Music,
				SoundLibrary = SoundLibrary,

				CharacterName = CharacterName,
				Quote = Quote,
				IsQuoteTip = IsQuoteTip,

				LeftWallName = LeftWallName,
				RightWallName = RightWallName
			};
		}
	}
}
