using System;

namespace LevelSetData
{
	[Serializable]
	public class SoundLibrary
	{
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
		public string BallPushSoundName { get; set; } = "<level-set-default>";
		public string TeleportSoundName { get; set; } = "<level-set-default>";
		public string ProtectiveBarrierHitSoundName { get; set; } = "<level-set-default>";
		public string WinSoundName { get; set; } = "<level-set-default>";

		public SoundLibrary(string defaultValue)
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
			BallPushSoundName = defaultValue;
			TeleportSoundName = defaultValue;
			ProtectiveBarrierHitSoundName = defaultValue;
			WinSoundName = defaultValue;
		}
	}
}
