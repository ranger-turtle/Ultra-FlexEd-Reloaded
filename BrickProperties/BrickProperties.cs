using System;

namespace LevelSetData
{
	[Serializable]
	public enum BreakAnimationType
	{
		Fade, Explode
	}

	[Serializable]
	public enum DirectionalBrickType
	{
		None, Fuse, Push
	}
	
	[Serializable]
	public enum Direction
	{
		Up, Right, Down, Left
	}
	
	[Serializable]
	public enum TeleportType
	{
		Single, Double
	}

	[Serializable]
	public enum ChimneyType
	{
		None, Regular, Confetti
	}

	/**
	 * <summary><para>Properties of the brick saved in the *.brick file.</para>
	 * <para>Id of the brick is counted from 1.</para></summary>
	 */
	[Serializable]
	public class BrickProperties
	{
		[Serializable]
		public struct Color
		{
			public byte Red { get; set; }
			public byte Green { get; set; }
			public byte Blue { get; set; }
		}
		public const int PIXEL_WIDTH = 60;
		public const int PIXEL_HEIGHT = 30;

		public int Id { get; set; }
		public float[][] FrameDurations { get; set; }
		public byte Durability { get; set; } = 1;
		public byte ExplosionRadius { get; set; } = 0;
		private bool _requiredToComplete = true;
		public bool RequiredToComplete
		{
			get => _requiredToComplete;
			set
			{
				_requiredToComplete = value;
				if (value == true) NormalResistant = false;
			}
		}
		private bool _normalResistant;
		public bool NormalResistant
		{
			get => _normalResistant;
			set
			{
				_normalResistant = value;
				if (value == true) RequiredToComplete = false;
			}
		}
		public bool ExplosionResistant { get; set; }
		public bool OokimResistant { get; set; }
		public float BonusProbability { get; set; } = 0.2f;
		public BreakAnimationType BreakAnimationType { get; set; } = BreakAnimationType.Fade;
		public ChimneyType ChimneyType { get; set; } = ChimneyType.None;
		public byte ParticleX { get; set; }
		public byte ParticleY { get; set; }
		public Color Color1 { get; set; }
		public Color Color2 { get; set; }
		public int DetonateId { get; set; }
		public int TeleportId { get; set; }
		public TeleportType TeleportType { get; set; }
		public int FallingTurnId { get; set; }
		public int OldBlockTypeId { get; set; }
		public int NewBlockTypeId { get; set; }
		public DirectionalBrickType DirectionalBrickType { get; set; } = DirectionalBrickType.None;
		public Direction Direction { get; set; }

		public bool IsExplosive => ExplosionRadius > 0;

		public bool IsTeleporter => TeleportId > 0;

		public bool IsDetonator => DetonateId > 0;

		public bool IsChimneyLike => ChimneyType != ChimneyType.None;
	}
}
