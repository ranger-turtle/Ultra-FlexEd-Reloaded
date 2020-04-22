using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LevelSetData;

namespace LevelSetManagement
{
	internal class OldLevelSetFormatter : ILevelSetFormatter
	{
		private readonly int LEVEL_SET_NAME_LENGTH = 176;
		private readonly int LEVEL_NAME_LENGTH = 80;
		private readonly int BG_NAME_LENGTH = 96;

		private readonly List<int> _teleportIds = new List<int>();
		private readonly List<int> _detonateIds = new List<int>();

		public int ConvertId(BrickProperties brickProperties, bool hidden)
		{
			if (hidden)
			{
				if (brickProperties.Id == 2 || brickProperties.Id == 34)
					return 34;
				else if (brickProperties.Id == 3 || brickProperties.Id == 33)
					return 33;
			}
			if (brickProperties.Id > LevelSetManager.DEFAULT_BRICK_QUANTITY)
			{
				if (brickProperties.IsTeleporter)
				{
					_teleportIds.Add(brickProperties.TeleportId);
					switch (brickProperties.TeleportType)
					{
						case TeleportType.Single when brickProperties.ExplosionResistant:
							return 25;
						case TeleportType.Single when !brickProperties.ExplosionResistant:
							return 22;
						case TeleportType.Double when brickProperties.ExplosionResistant:
							return 27;
						case TeleportType.Double when !brickProperties.ExplosionResistant:
							return 24;
						default:
							return 0;
					}
				}
				else if (_teleportIds.Exists(id => id == brickProperties.Id))
				{
					return brickProperties.ExplosionResistant ? 26 : 23;
				}
				else if (brickProperties.IsDetonator)
				{
					_detonateIds.Add(brickProperties.DetonateId);
					return 124;
				}
				else if (_detonateIds.Exists(id => id == brickProperties.Id))
				{
					return 123;
				}
				else if (brickProperties.DirectionalBrickType == DirectionalBrickType.Fuse)
				{
					switch (brickProperties.Direction)
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
				else if (brickProperties.DirectionalBrickType == DirectionalBrickType.Push)
				{
					switch (brickProperties.Direction)
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
						if (brickProperties.OokimResistant)
							return 30;
						else
							return 21;
					}
					else if (brickProperties.IsExplosive)
						return 29;
					else
						return 20;
				}
				else if (brickProperties.Durability > 1)
				{
					switch (brickProperties.Durability)
					{
						case 2:
							return 49;
						case 3:
							return 48;
						default:
							return 47;
					}
				}
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
						case ChimneyType.Regular:
							return 88;
						case ChimneyType.Confetti:
							return 89;
						default:
							return 0;
					}
				}
				else if (brickProperties.BonusProbability == 1.0f)
					return 90;
				else
					return (brickProperties.Id - 126) % 15 + 1;
			}
			else
				return brickProperties.Id;
		}

		public LevelSet Load(string filename)
		{
			using (FileStream fileStream = File.Open(filename, FileMode.Open, FileAccess.Read))
			{
				byte[] bytes = new byte[LEVEL_SET_NAME_LENGTH];
				fileStream.Seek(4, SeekOrigin.Begin);
				fileStream.Read(bytes, 0, 4);
				int levelCount = BitConverter.ToInt32(bytes, 0);
				LevelSet levelSet = new LevelSet();
				fileStream.Read(bytes, 0, LEVEL_SET_NAME_LENGTH);
				levelSet.Name = Encoding.Default.GetString(bytes, 0, Math.Min(Array.FindIndex(bytes, b => b == 0), LEVEL_SET_NAME_LENGTH - 1));
				for (int i = 0; i < levelCount; i++)
				{
					Level level = new Level();
					fileStream.Read(bytes, 0, LEVEL_NAME_LENGTH);
					level.LevelProperties.Name = Encoding.Default.GetString(bytes, 0, Math.Min(Array.FindIndex(bytes, b => b == 0), LEVEL_NAME_LENGTH - 1));
					fileStream.Read(bytes, 0, BG_NAME_LENGTH);
					level.LevelProperties.BackgroundName = Encoding.Default.GetString(bytes, 0, Math.Min(Array.FindIndex(bytes, b => b == 0), BG_NAME_LENGTH - 1));
					for (int j = 0; j < 25; j++)
					{
						for (int k = 0; k < 20; k++)
						{
							int readBrickId = fileStream.ReadByte();
							if (readBrickId == 33 || readBrickId == 34)
							{
								level.Bricks[j, k].BrickId = readBrickId == 33 ? 3 : 2;
								level.Bricks[j, k].Hidden = true;
							}
							else
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
				byte[] bytes = Encoding.Default.GetBytes(!string.IsNullOrEmpty(levelSet.Name) ? levelSet.Name : "(No name)");
				byte[] levelSetNameToSave = new byte[LEVEL_SET_NAME_LENGTH];
				byte[] levelNameToSave = new byte[LEVEL_NAME_LENGTH];
				byte[] backgroundNameToSave = new byte[BG_NAME_LENGTH];
				int byteCount = bytes.Length < levelSetNameToSave.Length ? bytes.Length : levelSetNameToSave.Length - 1;
				Array.Copy(bytes, levelSetNameToSave, byteCount);
				fileStream.Write(levelSetNameToSave, 0, levelSetNameToSave.Length);
				foreach (Level level in levelSet.Levels)
				{
					SaveString(fileStream, level, Encoding.Default.GetBytes(level.LevelProperties.Name ?? ""), levelNameToSave);
					SaveString(fileStream, level, Encoding.Default.GetBytes(level.LevelProperties.BackgroundName ?? ""), backgroundNameToSave);
					foreach (BrickInLevel brickInLevel in level.Bricks)
					{
						int idToSave = brickInLevel.BrickId;
						if (idToSave != 0)
							idToSave = ConvertId(LevelSetManager.GetInstance().GetBrickById(brickInLevel.BrickId), brickInLevel.Hidden);
						fileStream.Write(BitConverter.GetBytes(idToSave), 0, 1);
					}
				}
			}
		}

		private static void SaveString(FileStream fileStream, Level level, byte[] bytes, byte[] bytesToSave)
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
