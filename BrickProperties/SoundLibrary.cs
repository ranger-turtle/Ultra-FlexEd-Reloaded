using System;
using System.Collections.Generic;

namespace LevelSetData
{
	[Serializable]
	public class SoundLibrary
	{
		[Serializable]
		public class SoundLibraryPropertyAccessor
		{
			public Action<string> SetValue { get; set; }
			public Func<string> GetValue { get; set; }
		}

		public static readonly IEnumerable<string> SoundNames = new string[]
		{
			"Normal Ball Bounce",
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
			"Ball Thrust",
			"Teleport",
			"Protective Barrier Hit",
			"Win"
		};

		private readonly Dictionary<string, SoundLibraryPropertyAccessor> soundLibraryDictionary;

		public string NormalBallBounceSoundName { get; set; } = "<level-set-default>";
		public string BangSoundName { get; set; } = "<level-set-default>";
		public string ExplosionSoundName { get; set; } = "<level-set-default>";
		public string SpecialHitSoundName { get; set; } = "<level-set-default>";
		public string PowerUpYieldSoundName { get; set; } = "<level-set-default>";
		public string HitWallSoundName { get; set; } = "<level-set-default>";
		public string BallFallSoundName { get; set; } = "<level-set-default>";
		public string SpaceDjoelFallSoundName { get; set; } = "<level-set-default>";
		public string PowerUpFallSoundName { get; set; } = "<level-set-default>";
		public string MagnetStickSoundName { get; set; } = "<level-set-default>";
		public string BallSizeChangeSoundName { get; set; } = "<level-set-default>";
		public string BrickDescendSoundName { get; set; } = "<level-set-default>";
		public string LosePaddleSoundName { get; set; } = "<level-set-default>";
		public string BulletShootSoundName { get; set; } = "<level-set-default>";
		public string BallThrustSoundName { get; set; } = "<level-set-default>";
		public string TeleportSoundName { get; set; } = "<level-set-default>";
		public string ProtectiveBarrierHitSoundName { get; set; } = "<level-set-default>";
		public string WinSoundName { get; set; } = "<level-set-default>";

		public SoundLibrary()
		{
			soundLibraryDictionary = new Dictionary<string, SoundLibraryPropertyAccessor>()
			{
				{ "Normal Ball Bounce", new SoundLibraryPropertyAccessor{ SetValue = name => NormalBallBounceSoundName = name, GetValue = () => NormalBallBounceSoundName } },
				{ "Bang", new SoundLibraryPropertyAccessor{ SetValue = name=>BangSoundName = name, GetValue = () => BangSoundName } },
				{ "Explosion", new SoundLibraryPropertyAccessor{ SetValue = name=>ExplosionSoundName = name, GetValue = () => ExplosionSoundName } },
				{ "Special Hit", new SoundLibraryPropertyAccessor{ SetValue = name=>SpecialHitSoundName = name, GetValue = () => SpecialHitSoundName } },
				{ "Power Up Yield", new SoundLibraryPropertyAccessor{ SetValue = name=>PowerUpYieldSoundName = name, GetValue = () => PowerUpYieldSoundName } },
				{ "Hit Wall", new SoundLibraryPropertyAccessor{ SetValue = name=>HitWallSoundName = name, GetValue = () => HitWallSoundName } },
				{ "Ball Fall", new SoundLibraryPropertyAccessor{ SetValue = name=>BallFallSoundName = name, GetValue = () => BallFallSoundName } },
				{ "Space Djoel Fall", new SoundLibraryPropertyAccessor{ SetValue = name=>SpaceDjoelFallSoundName = name, GetValue = () => SpaceDjoelFallSoundName } },
				{ "Power Up Fall", new SoundLibraryPropertyAccessor{ SetValue = name=>PowerUpFallSoundName = name, GetValue = () => PowerUpFallSoundName } },
				{ "Magnet Stick", new SoundLibraryPropertyAccessor{ SetValue = name=>MagnetStickSoundName = name, GetValue = () => MagnetStickSoundName } },
				{ "Ball Size Change", new SoundLibraryPropertyAccessor{ SetValue = name=>BallSizeChangeSoundName = name, GetValue = () => BallSizeChangeSoundName } },
				{ "Brick Descend", new SoundLibraryPropertyAccessor{ SetValue = name=>BrickDescendSoundName = name, GetValue = () => BrickDescendSoundName } },
				{ "Lose Paddle", new SoundLibraryPropertyAccessor{ SetValue = name=>LosePaddleSoundName = name, GetValue = () => LosePaddleSoundName } },
				{ "Bullet Shoot", new SoundLibraryPropertyAccessor{ SetValue = name=>BulletShootSoundName = name, GetValue = () => BulletShootSoundName } },
				{ "Ball Thrust", new SoundLibraryPropertyAccessor{ SetValue = name=>BallThrustSoundName = name, GetValue = () => BallThrustSoundName } },
				{ "Teleport", new SoundLibraryPropertyAccessor{ SetValue = name=>TeleportSoundName = name, GetValue = () => TeleportSoundName } },
				{ "Protective Barrier Hit", new SoundLibraryPropertyAccessor{ SetValue = name=>ProtectiveBarrierHitSoundName = name, GetValue = () => ProtectiveBarrierHitSoundName } },
				{ "Win", new SoundLibraryPropertyAccessor{ SetValue = name=>WinSoundName = name, GetValue = () => WinSoundName } }
			};
		}

		public SoundLibrary(string defaultValue) : this()
		{
			NormalBallBounceSoundName = defaultValue;
			BangSoundName = defaultValue;
			ExplosionSoundName = defaultValue;
			SpecialHitSoundName = defaultValue;
			PowerUpYieldSoundName = defaultValue;
			HitWallSoundName = defaultValue;
			BallFallSoundName = defaultValue;
			SpaceDjoelFallSoundName = defaultValue;
			PowerUpFallSoundName = defaultValue;
			MagnetStickSoundName = defaultValue;
			BallSizeChangeSoundName = defaultValue;
			BrickDescendSoundName = defaultValue;
			LosePaddleSoundName = defaultValue;
			BulletShootSoundName = defaultValue;
			BallThrustSoundName = defaultValue;
			TeleportSoundName = defaultValue;
			ProtectiveBarrierHitSoundName = defaultValue;
			WinSoundName = defaultValue;
		}

		public SoundLibraryPropertyAccessor FromStringKey(string soundKey) => soundLibraryDictionary[soundKey];
	}
}
