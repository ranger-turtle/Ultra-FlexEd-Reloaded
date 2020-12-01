using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LevelSetData;

namespace LevelSetManagement
{
	internal class OldLevelSetFormatter : ILevelSetFormatter
	{
		private readonly int LEVEL_SET_NAME_LENGTH = 176;
		private readonly int LEVEL_NAME_LENGTH = 80;
		private readonly int BG_NAME_LENGTH = 96;

		private readonly List<int> _detonateIds = new List<int>();

		public int ConvertId(BrickProperties brickProperties)
		{
			if (brickProperties.Id > LevelSetManager.DEFAULT_BRICK_QUANTITY)
			{
				if (brickProperties.Hidden)
					return new Random().Next(33, 34);
				if (brickProperties.IsTeleporter)
				{
					switch (brickProperties.TeleportType)
					{
						case TeleportType.Single when brickProperties.ExplosionResistant:
							return 25;
						case TeleportType.Single when !brickProperties.ExplosionResistant:
							return 22;
						case TeleportType.All when brickProperties.ExplosionResistant:
							return 27;
						case TeleportType.All when !brickProperties.ExplosionResistant:
							return 24;
						default:
							return 0;
					}
				}
				else if (brickProperties.TeleportExits.Contains(brickProperties.Id))
				{
					return brickProperties.ExplosionResistant ? 26 : 23;
				}
				else if (brickProperties.IsDetonator)
				{
					_detonateIds.Add(brickProperties.OldBrickTypeId);
					return 124;
				}
				else if (_detonateIds.Exists(id => id == brickProperties.Id))
				{
					return 123;
				}
				else if (brickProperties.IsFuse)
				{
					switch (brickProperties.FuseDirection)
					{
						case Direction.Up:
							return 35;
						case Direction.Down:
							return 36;
						case Direction.Right:
							return 37;
						case Direction.Left:
							return 38;
						default:
							return 0;
					}
				}
				else if (brickProperties.IsBallThrusting)
				{
					switch (brickProperties.BallThrustDirection)
					{
						case Direction.Right when !brickProperties.NormalResistant:
							return 39;
						case Direction.Right when brickProperties.NormalResistant:
							return 43;
						case Direction.Up when !brickProperties.NormalResistant:
							return 40;
						case Direction.Up when brickProperties.NormalResistant:
							return 44;
						case Direction.Down when !brickProperties.NormalResistant:
							return 41;
						case Direction.Down when brickProperties.NormalResistant:
							return 45;
						case Direction.Left when !brickProperties.NormalResistant:
							return 42;
						case Direction.Left when brickProperties.NormalResistant:
							return 46;
						default:
							return 0;
					}
				}
				else if (brickProperties.NormalResistant)
				{
					if (brickProperties.ExplosionResistant)
					{
						if (brickProperties.PenetrationResistant)
							return 30;
						else
							return 21;
					}
					else if (brickProperties.IsExplosive)
						return 29;
					else
						return 20;
				}
				else if (brickProperties.NextBrickTypeId > 1)
					return 49;
				else if (brickProperties.IsExplosive)
				{
					int explosionRadius = brickProperties.ExplosionRadius;
					if (explosionRadius >= 8)
						return 28;
					else if (explosionRadius >= 5 && explosionRadius < 8)
						return 19;
					else if (explosionRadius >= 3 && explosionRadius < 5)
						return 32;
					else
						return 18;
				}
				else if (brickProperties.IsChimneyLike)
				{
					switch (brickProperties.ChimneyType)
					{
						case ChimneyType.Vertical:
							return 88;
						case ChimneyType.Sprinkling:
							return 89;
						default:
							return 0;
					}
				}
				else if (brickProperties.AlwaysPowerUpYielding)
					return 90;
				else
					return (brickProperties.Id - LevelSetManager.DEFAULT_BRICK_QUANTITY + 1) % 15 + 1;
			}
			else
				return brickProperties.Id;
		}

		public LevelSet Load(string filename)
		{
			using (FileStream fileStream = File.Open(filename, FileMode.Open, FileAccess.Read))
			{
				byte[] fileSignatureBytes = new byte[4];
				fileStream.Read(fileSignatureBytes, 0, 4);//Read file signature
				if (Encoding.Default.GetString(fileSignatureBytes) != "uLev")
					throw new FileFormatException("Invalid Ultra FlexBall 2000 level set file loaded.");
				byte[] bytes = new byte[LEVEL_SET_NAME_LENGTH];
				fileStream.Read(bytes, 0, 4);//Read level count saved after file signature
				int levelCount = BitConverter.ToInt32(bytes, 0);
				LevelSet levelSet = new LevelSet();
				fileStream.Read(bytes, 0, LEVEL_SET_NAME_LENGTH);
				levelSet.LevelSetProperties.Name = Encoding.Default.GetString(bytes, 0, Math.Min(Array.FindIndex(bytes, b => b == 0), LEVEL_SET_NAME_LENGTH - 1));
				for (int i = 0; i < levelCount; i++)
				{
					Level level = new Level();
					fileStream.Read(bytes, 0, LEVEL_NAME_LENGTH);
					level.LevelProperties.Name = Encoding.Default.GetString(bytes, 0, Math.Min(Array.FindIndex(bytes, b => b == 0), LEVEL_NAME_LENGTH - 1));
					fileStream.Read(bytes, 0, BG_NAME_LENGTH);
					string backgroundName = Encoding.Default.GetString(bytes, 0, Math.Min(Array.FindIndex(bytes, b => b == 0), BG_NAME_LENGTH - 1));
					if (backgroundName != string.Empty)
						level.LevelProperties.BackgroundName = backgroundName;
					for (int j = 0; j < 25; j++)
					{
						for (int k = 0; k < 20; k++)
						{
							int readBrickId = fileStream.ReadByte();
							level.Bricks[j, k].BrickId = readBrickId;
						}
					}
					levelSet.Levels.Add(level);
				}
				return levelSet;
			}
		}

		public void Save(string filename, LevelSet levelSet)
		{
			using (FileStream fileStream = File.Open(filename, FileMode.Create))
			{
				fileStream.Write(Encoding.Default.GetBytes("uLev"), 0, 4);
				fileStream.Write(BitConverter.GetBytes(levelSet.Levels.Count), 0, 4);
				byte[] bytes = Encoding.Default.GetBytes(!string.IsNullOrEmpty(levelSet.LevelSetProperties.Name) && levelSet.LevelSetProperties.Name != "<none>" ? levelSet.LevelSetProperties.Name : "(No name)");
				byte[] levelSetNameToSave = new byte[LEVEL_SET_NAME_LENGTH];
				byte[] levelNameToSave = new byte[LEVEL_NAME_LENGTH];
				byte[] backgroundNameToSave = new byte[BG_NAME_LENGTH];
				int byteCount = bytes.Length < levelSetNameToSave.Length ? bytes.Length : levelSetNameToSave.Length - 1;
				Array.Copy(bytes, levelSetNameToSave, byteCount);
				fileStream.Write(levelSetNameToSave, 0, levelSetNameToSave.Length);
				foreach (Level level in levelSet.Levels)
				{
					SaveString(fileStream, Encoding.Default.GetBytes(level.LevelProperties.Name ?? string.Empty), levelNameToSave);
					SaveString(fileStream, Encoding.Default.GetBytes(level.LevelProperties.BackgroundName ?? string.Empty), backgroundNameToSave);
					foreach (BrickInLevel brickInLevel in level.Bricks)
					{
						int idToSave = brickInLevel.BrickId;
						if (idToSave != 0)
							idToSave = ConvertId(LevelSetManager.GetInstance().GetBrickById(brickInLevel.BrickId));
						fileStream.Write(BitConverter.GetBytes(idToSave), 0, 1);
					}
				}
			}
		}

		private static void SaveString(FileStream fileStream, byte[] bytes, byte[] bytesToSave)
		{
			int byteCount = bytes.Length < bytesToSave.Length ? bytes.Length : bytesToSave.Length - 1;
			Array.Copy(bytes, bytesToSave, byteCount);
			bytesToSave[byteCount] = 0;
			fileStream.Write(bytesToSave, 0, bytesToSave.Length);
			/*It is neccessary, because program overwrites previously used data and if it won't be terminated, 
			 it will print further characters before nearest null character, which can be out of level set name data.*/
		}
	}
}
