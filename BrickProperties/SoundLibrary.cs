using System;
using System.Collections.Generic;
using System.Linq;

namespace LevelSetData
{
	[Serializable]
	public class SoundLibrary
	{
		private static readonly IEnumerable<string> SoundNames = new string[]
		{
			"Normal Ball Bounce",
			"Paddle Side Bounce",
			"Fast Ball Bounce",
			"Bang",
			"Explosion",
			"Special Hit",
			"Power Up Yield",
			"Hit Wall",
			"Ball Fall",
			"Space Djoel Fall",
			"Power Up Fall",
			"Magnet Stick",
			"Ball Size Change",
			"Brick Descend",
			"Lose Paddle",
			"Bullet Shoot",
			"Bullet Bounce",
			"Ball Thrust",
			"Teleport",
			"Mega Missile Shoot",
			"Mega Explosion",
			"Protective Barrier Hit",
			"Win",

			"Ball Grow",
			"Ball Shrink",
			"Explosive Ball",
			"Split Ball",
			"Penetrating Ball",
			"Descending Bricks",
			"Space Djoel",
			"Extra Paddle",
			"Shooter",
			"Protective Barrier",
			"Explosive Multiplier",
			"Magnet Paddle",
			"Mega Split",
			"Paddle Shrink",
			"Paddle Grow",
			"Regular Multiplier",
			"Mega Missile",
			"Megajocke"
		};

		private readonly string defaultValue;

		private Dictionary<string, string> soundLibraryDictionary;

		public SoundLibrary(string value)
		{
			defaultValue = value;
			soundLibraryDictionary = new Dictionary<string, string>();
			foreach (string soundName in SoundNames)
				soundLibraryDictionary.Add(soundName, value);
		}

		public string FromStringKey(string soundKey) => soundLibraryDictionary[soundKey];

		public void SetSound(string soundKey, string newName) => soundLibraryDictionary[soundKey] = newName;

		public static IEnumerable<string> GetSoundKeys() => SoundNames;

		public Dictionary<string, string> GetChangedSoundKeys() => soundLibraryDictionary.Where(sn => sn.Value != defaultValue).ToDictionary(k => k.Key, v => v.Value);
	}
}
