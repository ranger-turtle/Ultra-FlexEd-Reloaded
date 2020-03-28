using System;

namespace LevelSetData
{
	[Serializable]
	public enum BreakAnimationType
	{
		FADE, EXPLODE
	}

	[Serializable]
	public enum DirectionalBrickType
	{
		FUSE, PUSH
	}
	
	[Serializable]
	public enum Direction
	{
		UP, RIGHT, DOWN, LEFT
	}

	[Serializable]
	public class BrickProperties
	{
		public uint Id { get; set; }
		public float[][] FrameDurations { get; set; }
		public byte Durability { get; set; } = 1;
		public byte ExplosionRadius { get; set; } = 0;
		private bool _normalResistant;
		public bool NormalResistant
		{
			get { return _normalResistant; }
			set
			{
				_normalResistant = value;
				if (value == true)
				{
					_requiredToComplete = false;
				}
			}
		}
		private bool _requiredToComplete;
		public bool RequiredToComplete
		{
			get { return _requiredToComplete; }
			set { if (!NormalResistant) _requiredToComplete = value; }
		}
		public bool ExplosionResistant { get; set; }
		public bool OokimResistant { get; set; }
		public float BonusProbability { get; set; } = 0.2f;
		public BreakAnimationType BreakAnimationType { get; set; } = BreakAnimationType.FADE;
		public bool ChimneyLike { get; set; } = false;
		public byte ParticleX { get; set; } = 0;
		public byte ParticleY { get; set; } = 0;
		public byte[] RGB { get; set; }
		public uint DetonateId { get; set; } = 0;
		public uint TeleportId { get; set; } = 0;
		public uint FallingTurnId { get; set; } = 0;
		public uint OldBlockTypeId { get; set; } = 0;
		public uint NewBlockTypeId { get; set; } = 0;
		public DirectionalBrickType? DirectionalBrickType { get; set; }
		public Direction? Direction { get; set; }

		public bool IsExplosive
		{
			get
			{
				return ExplosionRadius > 0;
			}
		}
	}
}
