using System.Collections.Generic;
using System.IO;

namespace LevelSetData
{
	public class UltraFlexBallReloadedFileLoader
	{
		public const string newTypeLevelSetFileSignature = "nuLev";
		public const string brickFileSignature = "ufbrb";
		public const string cutsceneMagicNumber = "ufbrcts";

		public static LevelSet LoadLevelSet(string levelSetFilePath)
		{
			using (FileStream fileStream = File.OpenRead(levelSetFilePath))
			{
				using (BinaryReader levelSetReader = new BinaryReader(fileStream))
				{
					string fileSignature = levelSetReader.ReadString();
					if (fileSignature == "nuLev")
					{
						LevelSet levelSet = new LevelSet();
						levelSet.LevelSetProperties.Name = levelSetReader.ReadString();
						levelSet.LevelSetProperties.DefaultBackgroundName = levelSetReader.ReadString();
						levelSet.LevelSetProperties.DefaultMusic = levelSetReader.ReadString();
						levelSet.LevelSetProperties.DefaultLeftWallName = levelSetReader.ReadString();
						levelSet.LevelSetProperties.DefaultRightWallName = levelSetReader.ReadString();
						//BONUS write internal function after upgrade to next C# version
						int customSoundInLevelSetSoundLibraryCount = levelSetReader.ReadInt32();
						for (int i = 0; i < customSoundInLevelSetSoundLibraryCount; i++)
						{
							string key = levelSetReader.ReadString();
							string value = levelSetReader.ReadString();
							levelSet.LevelSetProperties.DefaultSoundLibrary.SetSound(key, value);
						}
						int levelCount = levelSetReader.ReadInt32();
						for (int li = 0; li < levelCount; li++)
						{
							Level level = new Level();
							level.LevelProperties.Name = levelSetReader.ReadString();
							level.LevelProperties.BackgroundName = levelSetReader.ReadString();
							level.LevelProperties.Music = levelSetReader.ReadString();
							int customSoundInLevelSoundLibraryCount = levelSetReader.ReadInt32();
							for (int i = 0; i < customSoundInLevelSoundLibraryCount; i++)
							{
								string key = levelSetReader.ReadString();
								string value = levelSetReader.ReadString();
								level.LevelProperties.SoundLibrary.SetSound(key, value);
							}
							level.LevelProperties.IsQuoteTip = levelSetReader.ReadBoolean();
							level.LevelProperties.CharacterName = levelSetReader.ReadString();
							level.LevelProperties.Quote = levelSetReader.ReadString();
							level.LevelProperties.LeftWallName = levelSetReader.ReadString();
							level.LevelProperties.RightWallName = levelSetReader.ReadString();
							for (int i = 0; i < LevelSet.ROWS; i++)
							{
								for (int j = 0; j < LevelSet.COLUMNS; j++)
								{
									level.Bricks[i, j].BrickId = levelSetReader.ReadInt32();
								}
							}
							levelSet.Levels.Add(level);
						}
						return levelSet;
					}
					else
						throw new IOException("Invalid Ultra FlexBall Reloaded level set file loaded.");
				}

			}
		}

		public static BrickProperties LoadBrick(string brickFilePath)
		{
			using (FileStream fileStream = File.OpenRead(brickFilePath))
			{
				using (BinaryReader brickReader = new BinaryReader(fileStream))
				{
					string fileSignature = brickReader.ReadString();
					if (fileSignature == brickFileSignature)
					{
#pragma warning disable IDE0017 // Simplify object initialization
						BrickProperties brickProperties = new BrickProperties(brickReader.ReadInt32());
#pragma warning restore IDE0017 // Simplify object initialization
						brickProperties.Name = Path.GetFileNameWithoutExtension(brickFilePath);
						int frameDurationCount = brickReader.ReadInt32();
						brickProperties.FrameDurations = new float[frameDurationCount];
						for (int i = 0; i < frameDurationCount; i++)
							brickProperties.FrameDurations[i] = brickReader.ReadSingle();
						brickProperties.StartAnimationFromRandomFrame = brickReader.ReadBoolean();
						brickProperties.NextBrickTypeId = brickReader.ReadInt32();
						brickProperties.ExplosionRadius = brickReader.ReadByte();
						brickProperties.RequiredToComplete = brickReader.ReadBoolean();
						brickProperties.AlwaysSpecialHit = brickReader.ReadBoolean();
						brickProperties.Points = brickReader.ReadInt32();
						brickProperties.HitSoundName = brickReader.ReadString();
						brickProperties.GraphicType = (GraphicType)brickReader.ReadInt32();

						#region Power-Up
						brickProperties.AlwaysPowerUpYielding = brickReader.ReadBoolean();
						if (!brickProperties.AlwaysPowerUpYielding)
							brickProperties.PowerUpMeterUnits = brickReader.ReadInt32();
						brickProperties.YieldedPowerUp = (YieldedPowerUp)brickReader.ReadInt32();
						#endregion

						#region Resistance Types
						brickProperties.NormalResistant = brickReader.ReadBoolean();
						brickProperties.ExplosionResistant = brickReader.ReadBoolean();
						brickProperties.PenetrationResistant = brickReader.ReadBoolean();
						#endregion

						#region Animation Types
						brickProperties.BallBreakAnimationType = (BreakAnimationType)brickReader.ReadInt32();
						brickProperties.ExplosionBreakAnimationType = (BreakAnimationType)brickReader.ReadInt32();
						brickProperties.BulletBreakAnimationType = (BreakAnimationType)brickReader.ReadInt32();
						#endregion

						#region Teleport
						int teleportExitCount = brickReader.ReadInt32();
						if (teleportExitCount > 0)
						{
							brickProperties.TeleportExits = new int[teleportExitCount];
							for (int i = 0; i < teleportExitCount; i++)
								brickProperties.TeleportExits[i] = brickReader.ReadInt32();
						}
						brickProperties.TeleportType = (TeleportType)brickReader.ReadInt32();
						brickProperties.TeleportExit = brickReader.ReadBoolean();
						#endregion

						#region Hidden Brick
						brickProperties.Hidden = brickReader.ReadBoolean();
						if (brickProperties.Hidden)
							brickProperties.RequiredToCompleteWhenHidden = brickReader.ReadBoolean();
						#endregion

						#region Multiplier
						brickProperties.CanBeOverridenByStandardMultiplier = brickReader.ReadBoolean();
						brickProperties.CanBeOverridenByExplosiveMultiplier = brickReader.ReadBoolean();
						brickProperties.CanBeMultipliedByExplosiveMultiplier = brickReader.ReadBoolean();
						#endregion

						#region Chimney like
						brickProperties.ChimneyType = (ChimneyType)brickReader.ReadInt32();
						if (brickProperties.IsChimneyLike)
						{
							brickProperties.ParticleX = brickReader.ReadByte();
							brickProperties.ParticleY = brickReader.ReadByte();
							brickProperties.ChimneyColourSchemeType = (ChimneyColourSchemeType)brickReader.ReadInt32();
							if (brickProperties.ChimneyColourSchemeType == ChimneyColourSchemeType.TwoColours)
							{
								brickProperties.Color1 = new BrickProperties.Color()
								{
									Red = brickReader.ReadByte(),
									Green = brickReader.ReadByte(),
									Blue = brickReader.ReadByte()
								};
								brickProperties.Color2 = new BrickProperties.Color()
								{
									Red = brickReader.ReadByte(),
									Green = brickReader.ReadByte(),
									Blue = brickReader.ReadByte()
								};
							}
						}
						#endregion

						#region Descending
						brickProperties.IsDescending = brickReader.ReadBoolean();
						brickProperties.DescendingPressTurnId = brickReader.ReadInt32();
						if (brickProperties.IsDescending)
						{
							brickProperties.DescendingBottomTurnId = brickReader.ReadInt32();
						}
						#endregion

						#region Directional
						brickProperties.BallThrustDirection = (Direction)brickReader.ReadInt32();
						brickProperties.FuseDirection = (Direction)brickReader.ReadInt32();
						brickProperties.SequenceDirection = (Direction)brickReader.ReadInt32();
						#endregion

						#region Detonator
						brickProperties.OldBrickTypeId = brickReader.ReadInt32();
						if (brickProperties.OldBrickTypeId > 0)
						{
							brickProperties.NewBrickTypeId = brickReader.ReadInt32();
							brickProperties.DetonationRange = (DetonationRange)brickReader.ReadInt32();
							brickProperties.DetonationTrigger = (EffectTrigger)brickReader.ReadInt32();
						}
						#endregion

						#region Triggers
						brickProperties.DetonationTrigger = (EffectTrigger)brickReader.ReadInt32();
						brickProperties.ExplosionTrigger = (EffectTrigger)brickReader.ReadInt32();
						brickProperties.FuseTrigger = (EffectTrigger)brickReader.ReadInt32();
						#endregion

						#region Moving Bricks
						brickProperties.MovingBrickType = (MovingBrickType)brickReader.ReadInt32();
						if (brickProperties.IsMoving)
						{
							brickProperties.BoundOne = brickReader.ReadInt32();
							brickProperties.BoundTwo = brickReader.ReadInt32();
							brickProperties.BrickMoveInterval = brickReader.ReadSingle();
						}
						#endregion

						return brickProperties;
					}

					else
						throw new IOException("Invalid Ultra FlexBall Reloaded brick file loaded.");
				}

			}
		}

		public static string[] LoadCutsceneDialogues(string cutscenePath)
		{
			using (FileStream fileStream = File.OpenRead(cutscenePath))
			{
				using (BinaryReader cutsceneReader = new BinaryReader(fileStream))
				{
					string fileSignature = cutsceneReader.ReadString();
					if (fileSignature == cutsceneMagicNumber)
					{
						List<string> cutsceneDialogues = new List<string>();
						int dialogueCount = cutsceneReader.ReadInt32();
						for (int i = 0; i < dialogueCount; i++)
						{
							cutsceneDialogues.Add(cutsceneReader.ReadString());
						}
						return cutsceneDialogues.ToArray();
					}
					else
						throw new IOException("Invalid Ultra FlexBall Reloaded cutscene file loaded.");
				}
			}
		}
	}
}
