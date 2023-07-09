using System;

//BONUS Do distinct hit sounds for each hit type (other for ball hit, other for bullet hit)
//BONUS Do distinct brick animation after breaking with Space Djoel
//BONUS Make scripting possibility
namespace LevelSetData
{
	[Serializable]
	public enum GraphicType
	{
		Smooth, Pixel
	}

	[Serializable]
	public enum BreakAnimationType
	{
		Fade, Burn, Custom
	}

	[Serializable]
	public enum Direction
	{
		None, Up, Right, Down, Left
	}
	
	[Serializable]
	public enum TeleportType
	{
		Single, All
	}

	[Serializable]
	public enum ChimneyType
	{
		None, Vertical, Sprinkling
	}

	[Serializable]
	public enum ChimneyColourSchemeType
	{
		TwoColours, WholeHueRange
	}

	[Serializable]
	public enum DetonationRange
	{
		One, All
	}

	[Serializable]
	public enum YieldedPowerUp
	{
		BallGrow, BallShrink, ExplodingBall, SplitBall, PenetratingBall, BrickDescend, SpaceDjoel, ExtraPaddle, Shooter, ProtectiveBarrier, ExplosiveMultiplier, MagnetPaddle, MegaSplit, PaddleShrink, PaddleGrow, RegularMultiplier, MegaMissile, Any
	}

	[Serializable]
	public enum EffectTrigger
	{
		Destroy, Hit, Both
	}

	/*Sequential brick type is the brick you have to destroy from first to last
	(such in levels from "BreakQuest", where you have to hit the bottommost brick in sequenc)
	//[Serializable]
	//public enum DirectionalBrickType
	//{
	//	None, Fuse, ThrustBall, Sequential
	//}*/

	//Moving bricks move automatically
	[Serializable]
	public enum MovingBrickType
	{
		None, Horizontal, Vertical, Oblique
	}

	/**
	 * <summary><para>Properties of the brick saved in the *.brick file.</para>
	 * <para>Id of the brick is count from 1.</para></summary>
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
		public const int PIXEL_WIDTH = 30;
		public const int PIXEL_HEIGHT = 15;
		public const float STANDARD_DIMENSION_RATIO = (float)PIXEL_HEIGHT / PIXEL_WIDTH;

		#region General
		public int Id { get; set; }
		public string Name { get; set; } = "New Brick";
		public float[] FrameDurations { get; set; } = new float[] { 0.4f };
		public bool StartAnimationFromRandomFrame { get; set; }
		//public byte Durability { get; set; } = 1;
		public int NextBrickTypeId { get; set; }
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
		public GraphicType GraphicType { get; set; }
		//special hit sound is made and white particles are sprinkled (such when hitting an explosive or power-up yielding)
		public bool AlwaysSpecialHit { get; set; }
		public int Points { get; set; } = 10;
		public string HitSoundName { get; set; } = "<default>";
		#endregion

		#region Power-up yield properties
		public bool AlwaysPowerUpYielding { get; set; }
		public int PowerUpMeterUnits { get; set; } = 5;
		public YieldedPowerUp YieldedPowerUp { get; set; } = YieldedPowerUp.Any;
		#endregion

		#region Resistance types
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
		public bool PenetrationResistant { get; set; }
		#endregion

		#region Animation properties
		public BreakAnimationType BallBreakAnimationType { get; set; }
		public BreakAnimationType ExplosionBreakAnimationType { get; set; }
		public BreakAnimationType BulletBreakAnimationType { get; set; }
		#endregion

		#region Teleport properties
		public int[] TeleportExits { get; set; }
		public TeleportType TeleportType { get; set; }
		public bool TeleportExit { get; set; }
		#endregion

		#region Hidden brick properties
		public bool Hidden { get; set; }
		public bool RequiredToCompleteWhenHidden { get; set; }
		#endregion

		#region Multiplier properties
		public bool CanBeOverridenByStandardMultiplier { get; set; } = true;
		public bool CanBeOverridenByExplosiveMultiplier { get; set; } = true;
		public bool CanBeMultipliedByExplosiveMultiplier { get; set; }
		#endregion

		#region Chimney-like brick properties
		public ChimneyType ChimneyType { get; set; }
		public ChimneyColourSchemeType ChimneyColourSchemeType { get; set; }
		public byte ParticleX { get; set; }
		public byte ParticleY { get; set; }
		public Color Color1 { get; set; }
		public Color Color2 { get; set; }
		#endregion

		#region Descending bricks properties
		public int DescendingBottomTurnId { get; set; } //Id of block block fallen to bottom will turn to
		public int DescendingPressTurnId { get; set; } //Id of block block pressed on will turn to
		public bool IsDescending { get; set; } = true;
		#endregion

		#region Directional type properties
		public Direction BallThrustDirection { get; set; }
		public Direction FuseDirection { get; set; }
		public Direction SequenceDirection { get; set; }
		#endregion

		#region Detonator properties
		public int OldBrickTypeId { get; set; }
		public int NewBrickTypeId { get; set; }
		public DetonationRange DetonationRange { get; set; }
		#endregion

		#region Triggers
		public EffectTrigger DetonationTrigger { get; set; }
		public EffectTrigger ExplosionTrigger { get; set; }
		public EffectTrigger FuseTrigger { get; set; }
		#endregion

		#region Moving brick properties
		public MovingBrickType MovingBrickType { get; set; }
		public int BoundOne { get; set; }
		public int BoundTwo { get; set; }
		public float BrickMoveInterval { get; set; }
		#endregion

		public bool IsExplosive => ExplosionRadius > 0;

		public bool IsTeleporter => TeleportExits != null && TeleportExits.Length > 0;

		public bool IsFuse => FuseDirection != Direction.None;

		public bool IsBallThrusting => BallThrustDirection != Direction.None;

		public bool IsSequential => SequenceDirection != Direction.None;

		public bool IsDetonator => OldBrickTypeId > 0;

		public bool IsChimneyLike => ChimneyType != ChimneyType.None;

		public bool IsMoving => MovingBrickType != MovingBrickType.None;

		public bool IsRegular => !IsExplosive && !IsTeleporter && !TeleportExit && !IsFuse && !IsBallThrusting && !IsSequential && !IsDetonator && !IsChimneyLike && !AlwaysPowerUpYielding && !NormalResistant && !IsMoving;

		public BrickProperties(int id)
		{
			if (id > 0)
				Id = id;
			else
				throw new InvalidIdException();
		}

		public class InvalidIdException : Exception { }
	}
}
