using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using LevelSetData;

namespace LevelSetManagement
{
	//BONUS try to optimize manager moving operations on brick to Brick Manager and on cutscenes to Cutscene Manager.
	//BONUS Move all primary IO operations from UFB assembly to here.
	public class LevelSetManager
	{
		public class CheckResourcesFailException : Exception { }
		public class BrickCorruptException : Exception { }
		public class InvalidCutsceneException : Exception { }

		public const string MAIN_TITLE = "Ultra FlexEd Reloaded";

		public const int DEFAULT_BRICK_QUANTITY = 125;
		public const string DEFAULT_BRICK_DIRECTORY = "Default Bricks";

		public const string EMPTY_ELEMENT_PLACEHOLDER = "<none>";
		private readonly Comparison<BrickProperties> brickPropertyComparison = (bp1, bp2) => bp1.Id.CompareTo(bp2.Id);

		#region eventHandlers
		private Action<string> _updateTitle;

		public Action<string> UpdateTitle {
			get => _updateTitle;
			set
			{
				_updateTitle = value;
				_updateTitle(CurrentAppTitle);
			}
		}
		#endregion

		private readonly Dictionary<string, FormatType> formatTypes = new Dictionary<string, FormatType>
		{
			[".nlev"] = FormatType.New,
			[".lev"] = FormatType.Old
		};

		private readonly Dictionary<FormatType, ILevelTester> testers = new Dictionary<FormatType, ILevelTester>();
		private ResourceChecker resourceChecker = new ResourceChecker();

		public bool Changed { get; private set; }

		private LevelSet _levelSet = new LevelSet();
		public List<BrickProperties> Bricks { get; } = new List<BrickProperties>();
		public string FilePath { get; private set; }
		public string LevelSetResourceDirectory => Path.Combine(Path.GetDirectoryName(FilePath), Path.GetFileNameWithoutExtension(FilePath));

		public int CurrentBrickId { get; set; } = 1;
		public int CurrentLevelIndex { get; set; }

		public BrickProperties CurrentBrick => Bricks.Find(b => b.Id == CurrentBrickId);
		private Level CurrentLevel => _levelSet.Levels[CurrentLevelIndex];
		public string CurrentBrickName => CurrentBrick.Name;
		public string CurrentLevelName => _levelSet.Levels[CurrentLevelIndex].LevelProperties.Name;
		public FormatType CurrentFormatType => LevelSetLoaded ? formatTypes[Path.GetExtension(FilePath)] : FormatType.New;
		public bool LevelSetLoaded => !string.IsNullOrEmpty(FilePath);
		public int LevelCount => _levelSet.Levels.Count;
		private string CurrentAppTitle =>
			$"{MAIN_TITLE} - [{(FilePath != null && FilePath != string.Empty ? FilePath : "Untitled")}{(Changed ? "*" : string.Empty)}]";

		public ILevelTester CurrentTester { get; private set; }

		private static LevelSetManager instance;

		public static LevelSetManager GetInstance(bool reset = false) =>
			instance == null || reset ? (instance = new LevelSetManager()) : instance;

		private LevelSetManager()
		{
			LoadDefaultBricks();
			resourceChecker.CheckDefaultBricks();
			_levelSet.Levels.Add(new Level());
		}

		public void SetTesters(ILevelTester oldGameTester, ILevelTester newGameTester)
		{
			testers[FormatType.Old] = oldGameTester;
			CurrentTester = testers[FormatType.New] = newGameTester;
		}

		public void SetTesterPaths(string oldGamePath, string newGamePath)
		{
			testers[FormatType.Old].SetPath(oldGamePath);
			testers[FormatType.New].SetPath(newGamePath);
		}

		private void CheckBrickFileSystem(string defaultBrickDirectory, string brickName)
		{
			if (!Directory.Exists($"{defaultBrickDirectory}/{brickName}"))
				throw new DirectoryNotFoundException($"Could not find directory {defaultBrickDirectory}/{brickName}.");
			if (!File.Exists($"{defaultBrickDirectory}/{brickName}/frames.png"))
				throw new FileNotFoundException($"Could not find file {defaultBrickDirectory}/{brickName}/frames.png.");
		}

		private void LoadDefaultBricks()
		{
			IEnumerable<string> brickFilePaths = CheckDefaultBricks(DEFAULT_BRICK_DIRECTORY);
			LoadBricks(DEFAULT_BRICK_DIRECTORY, brickFilePaths);
		}

		private void LoadCustomBricks(string directory)
		{
			IEnumerable<string> brickFilePaths = Directory.EnumerateFiles(directory, "*.brick");
			LoadBricks(directory, brickFilePaths);
		}

		private void LoadBricks(string directory, IEnumerable<string> brickFilePaths)
		{
			List<string> corruptBrickNames = new List<string>();
			foreach (string brickPath in brickFilePaths)
			{
				try
				{
					BrickProperties brickProperties = UltraFlexBallReloadedFileLoader.LoadBrick(brickPath);
					Bricks.Add(brickProperties);
					CheckBrickFileSystem(directory, brickProperties.Name);
				}
				catch (IOException)
				{
					corruptBrickNames.Add(brickPath);
				}
			}
			Bricks.Sort(brickPropertyComparison);
			if (corruptBrickNames.Count > 0)
				resourceChecker.AddCorruptBrickNames(corruptBrickNames);
		}

		private IEnumerable<string> CheckDefaultBricks(string directory)
		{
			var brickFilePaths = Directory.EnumerateFiles(directory, "*.brick");
			if (brickFilePaths.Count() != DEFAULT_BRICK_QUANTITY)
				throw new FileNotFoundException($"{DEFAULT_BRICK_QUANTITY - brickFilePaths.Count()} of {DEFAULT_BRICK_QUANTITY} brick files are missing. {Environment.NewLine} Please redownload game to restore bricks or find missing brick(s).");
			return brickFilePaths;
		}

		public string GetBrickFolder(int brickId) => brickId <= DEFAULT_BRICK_QUANTITY ? DEFAULT_BRICK_DIRECTORY : $"{LevelSetResourceDirectory}/Bricks";

		public SortedDictionary<int, string> GetCustomBrickNames() => new SortedDictionary<int, string>(Bricks.SkipWhile(bn => bn.Id <= DEFAULT_BRICK_QUANTITY).ToDictionary(k => k.Id, v => v.Name));

		public void LoadLevelSetFile(string filepath)
		{
			string extension = Path.GetExtension(filepath);
			string levelSetFileName = Path.GetFileNameWithoutExtension(filepath);
			ILevelSetFormatter formatter = LevelSetFormatterFactory.GenerateLevelSetFormatter(formatTypes[extension]);
			_levelSet = formatter.Load(filepath);
			FilePath = filepath;
			if (Bricks.Count > DEFAULT_BRICK_QUANTITY)//if custom bricks were loaded
			{
				Bricks.RemoveRange(DEFAULT_BRICK_QUANTITY, Bricks.Count - DEFAULT_BRICK_QUANTITY);
			}
			//if loaded level set includes custom bricks
			string customBrickPath = $"{LevelSetResourceDirectory}/Bricks";
			if (formatTypes[extension] == FormatType.New && Directory.Exists(customBrickPath))
				LoadCustomBricks(customBrickPath);
			CurrentTester = testers[formatTypes[extension]];
			CurrentBrickId = 1;
			CurrentLevelIndex = 0;
			Changed = false;
			UpdateTitle(CurrentAppTitle);
			if (CurrentFormatType == FormatType.New)
			{
				try
				{
					CheckResources();
				}
				catch (ResourceCheckFailException rcf)
				{
					ClearBlocksOfTypes(rcf.MissingBrickIds);
					throw rcf;
				}
			}
		}

		public Level[] GetLevelsFromFile(string filepath)
		{
			string extension = Path.GetExtension(filepath);
			ILevelSetFormatter formatter = LevelSetFormatterFactory.GenerateLevelSetFormatter(formatTypes[extension]);
			return formatter.Load(filepath).Levels.ToArray();
		}

		public void SaveFile() => SaveFile(FilePath);

		public void SaveFile(string filepath)
		{
			FilePath = filepath;
			string extension = Path.GetExtension(filepath);
			ILevelSetFormatter formatter = LevelSetFormatterFactory.GenerateLevelSetFormatter(formatTypes[extension]);
			formatter.Save(filepath, _levelSet);
			Changed = false;
			UpdateTitle(CurrentAppTitle);
			CurrentTester = testers[formatTypes[extension]];
			//TODO implement history index assignment
		}

		public void SetChangedState()
		{
			Changed = true;
			UpdateTitle(CurrentAppTitle);
			//TODO implement change history
		}

		public void UpdateLevelSet(LevelSetProperties levelSetProperties)
		{
			_levelSet.LevelSetProperties = levelSetProperties;
			SetChangedState();
		}

		private int EvaluateSmallestAbsentBrickTypeId(int[] ids)
		{
			int i = 1;
			while (i < ids.Length && ids[i] - ids[i - 1] <= 1) i++;
			if (i == int.MaxValue)
				throw new OverflowException($"Could not add a new brick.{Path.PathSeparator}Maximum number of bricks {int.MaxValue} has been exceeded.");
			return ids[i - 1] + 1;
		}

		public void AddBrickToLevelSet(BrickProperties brick, string frameSheetPath, Dictionary<string, string> optionalImagePaths)
		{
			if (frameSheetPath == null) throw new NullReferenceException("Frame sheet paths cannot be null.");
			int[] ids = Bricks.Select(b => b.Id).ToArray();
			int firstAbsentId = 1;
			if (ids.Length == 1 && ids[0] == 1)
				firstAbsentId = 2;
			else if (ids.Length > 1)
				firstAbsentId = EvaluateSmallestAbsentBrickTypeId(ids);
			brick.Id = firstAbsentId;
			Bricks.Add(brick);
			Bricks.Sort(brickPropertyComparison);
			SaveBrick(brick, frameSheetPath, optionalImagePaths);
		}

		public BrickProperties ImportLocalBrick(int brickTypeId, out string frameSheetPath, out Dictionary<string, string> optionalImagePaths)
		{
			BrickProperties brickProperties = SerializableCopier.Clone(GetBrickById(brickTypeId));
			string brickFolder = Path.Combine(GetBrickFolder(brickTypeId), brickProperties.Name);
			frameSheetPath = Path.Combine(brickFolder, "frames.png");

			optionalImagePaths = new Dictionary<string, string>();

			string hitBrickPath = Path.Combine(brickFolder, "hit.png");

			if (File.Exists(hitBrickPath))
				optionalImagePaths.Add("hit", hitBrickPath);

			string ballAnimationPath = Path.Combine(brickFolder, "ballbreak.png");
			string explosionAnimationPath = Path.Combine(brickFolder, "explosionbreak.png");
			string bulletAnimationPath = Path.Combine(brickFolder, "bulletbreak.png");

			if (File.Exists(ballAnimationPath))
				optionalImagePaths.Add("ballbreak", ballAnimationPath);
			if (File.Exists(explosionAnimationPath))
				optionalImagePaths.Add("explosionbreak", explosionAnimationPath);
			if (File.Exists(bulletAnimationPath))
				optionalImagePaths.Add("bulletbreak", bulletAnimationPath);

			brickProperties.Name += " - Copy";
			return brickProperties;
		}

		public void UpdateBrick(BrickProperties brick, string frameSheetPath, Dictionary<string, string> optionalFilePaths)
		{
			int changedBrickTypeIndex = Bricks.FindIndex(b => b.Id == brick.Id);
			string oldBrickName = Bricks[changedBrickTypeIndex].Name;
			Bricks[changedBrickTypeIndex] = brick;
			if (oldBrickName != brick.Name)
			{
				Directory.Move($"{GetBrickFolder(brick.Id)}/{oldBrickName}/", $"{GetBrickFolder(brick.Id)}/{brick.Name}/");
				File.Move($"{GetBrickFolder(brick.Id)}/{oldBrickName}.brick", $"{GetBrickFolder(brick.Id)}/{brick.Name}.brick");
			}
			SaveBrick(brick, frameSheetPath, optionalFilePaths);
		}

		//BONUS try implementing brick file compression
		private void SaveBrick(BrickProperties brick, string mainFrameSheetPath, Dictionary<string, string> optionalBrickImagePaths)
		{
			string brickFolder = GetBrickFolder(brick.Id);
			string brickSheetFolder = $"{brickFolder}/{brick.Name}";
			Directory.CreateDirectory(brickSheetFolder);
			SaveBrickFile(brickSheetFolder, brick);
			if (mainFrameSheetPath != null)
			{
				string frameSheetExtension = Path.GetExtension(mainFrameSheetPath);
				File.Copy(mainFrameSheetPath, $"{brickSheetFolder}/frames{frameSheetExtension}", true);
			}
			if (optionalBrickImagePaths != null)
				foreach (var optionalImagePath in optionalBrickImagePaths)
				{
					string hitBrickImageExtension = Path.GetExtension(optionalImagePath.Value);
					if (optionalImagePath.Value != null)
						File.Copy(optionalImagePath.Value, $"{brickSheetFolder}/{optionalImagePath.Key}{hitBrickImageExtension}", true);
					else
						File.Delete($"{brickSheetFolder}/{optionalImagePath.Key}{hitBrickImageExtension}");
				}
		}

		private void SaveBrickFile(string brickPath, BrickProperties brickProperties)
		{
			using (FileStream file = File.Create($"{brickPath}.brick"))
			{
				using (BinaryWriter brickWriter = new BinaryWriter(file))
				{
					brickWriter.Write(UltraFlexBallReloadedFileLoader.brickFileSignature);
					brickWriter.Write(brickProperties.Id);
					brickWriter.Write(brickProperties.FrameDurations.Length);
					foreach (float frameDuration in brickProperties.FrameDurations)
						brickWriter.Write(frameDuration);
					brickWriter.Write(brickProperties.StartAnimationFromRandomFrame);
					brickWriter.Write(brickProperties.NextBrickTypeId);
					brickWriter.Write(brickProperties.ExplosionRadius);
					brickWriter.Write(brickProperties.RequiredToComplete);
					brickWriter.Write(brickProperties.AlwaysSpecialHit);
					brickWriter.Write(brickProperties.Points);
					brickWriter.Write(brickProperties.HitSoundName);
					brickWriter.Write((int)brickProperties.GraphicType);

					#region Power-Up
					brickWriter.Write(brickProperties.AlwaysPowerUpYielding);
					if (!brickProperties.AlwaysPowerUpYielding)
						brickWriter.Write(brickProperties.PowerUpMeterUnits);
					brickWriter.Write((int)brickProperties.YieldedPowerUp);
					#endregion

					#region Resistance Types
					brickWriter.Write(brickProperties.NormalResistant);
					brickWriter.Write(brickProperties.ExplosionResistant);
					brickWriter.Write(brickProperties.PenetrationResistant);
					#endregion

					#region Animation Types
					brickWriter.Write((int)brickProperties.BallBreakAnimationType);
					brickWriter.Write((int)brickProperties.ExplosionBreakAnimationType);
					brickWriter.Write((int)brickProperties.BulletBreakAnimationType);
					#endregion

					#region Teleport
					brickWriter.Write(brickProperties.TeleportExits?.Length ?? 0);
					if (brickProperties.TeleportExits != null)
							foreach (int teleportExit in brickProperties.TeleportExits)
							brickWriter.Write(teleportExit);
					brickWriter.Write((int)brickProperties.TeleportType);
					brickWriter.Write(brickProperties.TeleportExit);
					#endregion

					#region Hidden Brick
					brickWriter.Write(brickProperties.Hidden);
					if (brickProperties.Hidden)
						brickWriter.Write(brickProperties.RequiredToCompleteWhenHidden);
					#endregion

					#region Multiplier
					brickWriter.Write(brickProperties.CanBeOverridenByStandardMultiplier);
					brickWriter.Write(brickProperties.CanBeOverridenByExplosiveMultiplier);
					brickWriter.Write(brickProperties.CanBeMultipliedByExplosiveMultiplier);
					#endregion

					#region Chimney like
					brickWriter.Write((int)brickProperties.ChimneyType);
					if (brickProperties.IsChimneyLike)
					{
						brickWriter.Write(brickProperties.ParticleX);
						brickWriter.Write(brickProperties.ParticleY);
						brickWriter.Write((int)brickProperties.ChimneyColourSchemeType);
						if (brickProperties.ChimneyColourSchemeType == ChimneyColourSchemeType.TwoColours)
						{
							brickWriter.Write(brickProperties.Color1.Red);
							brickWriter.Write(brickProperties.Color1.Green);
							brickWriter.Write(brickProperties.Color1.Blue);
							brickWriter.Write(brickProperties.Color2.Red);
							brickWriter.Write(brickProperties.Color2.Green);
							brickWriter.Write(brickProperties.Color2.Blue);
						}
					}
					#endregion

					#region Descending
					brickWriter.Write(brickProperties.IsDescending);
					brickWriter.Write(brickProperties.DescendingPressTurnId);
					if (brickProperties.IsDescending)
					{
						brickWriter.Write(brickProperties.DescendingBottomTurnId);
					}
					#endregion

					#region Directional
					brickWriter.Write((int)brickProperties.BallThrustDirection);
					brickWriter.Write((int)brickProperties.FuseDirection);
					brickWriter.Write((int)brickProperties.SequenceDirection);
					#endregion

					#region Detonator
					brickWriter.Write(brickProperties.OldBrickTypeId);
					if (brickProperties.OldBrickTypeId > 0)
					{
						brickWriter.Write(brickProperties.NewBrickTypeId);
						brickWriter.Write((int)brickProperties.DetonationRange);
						brickWriter.Write((int)brickProperties.DetonationTrigger);
					}
					#endregion

					#region Triggers
					brickWriter.Write((int)brickProperties.DetonationTrigger);
					brickWriter.Write((int)brickProperties.ExplosionTrigger);
					brickWriter.Write((int)brickProperties.FuseTrigger);
					#endregion

					#region Moving Bricks
					brickWriter.Write((int)brickProperties.MovingBrickType);
					if (brickProperties.IsMoving)
					{
						brickWriter.Write(brickProperties.BoundOne);
						brickWriter.Write(brickProperties.BoundTwo);
						brickWriter.Write(brickProperties.BrickMoveInterval);
					}
					#endregion
				}
			}
		}

		public void RemoveBrick(int brickId)
		{
			string brickName = GetBrickById(brickId).Name;
			string brickFolder = GetBrickFolder(brickId);
			Bricks.Remove(Bricks.Find(b => b.Id == brickId));
			ClearBlocksOfType(brickId);
			Directory.Delete($"{brickFolder}/{brickName}", true);
			File.Delete($"{brickFolder}/{brickName}.brick");
		}

		public BrickInLevel CopyBrickInCurrentLevel(int brickX, int brickY)
		{
			return _levelSet.Levels[CurrentLevelIndex].Bricks[brickY, brickX].Clone() as BrickInLevel;
		}

		public void UpdateBrickInLevel(int brickX, int brickY, BrickInLevel brickInLevel)
		{
			_levelSet.Levels[CurrentLevelIndex].Bricks[brickY, brickX] = brickInLevel;
		}

		public void ClearBlocksOfTypes(List<int> brickIds)
		{
			foreach (int brickId in brickIds)
				ClearBlocksOfType(brickId);
		}

		public void ClearBlocksOfType(int brickId)
		{
			bool changed = false;
			foreach (Level level in _levelSet.Levels)
				foreach (BrickInLevel brickInLevel in level.Bricks)
					if (brickInLevel.BrickId == brickId)
					{
						brickInLevel.Reset();
						if (!changed)
						{
							changed = true;
							SetChangedState();
						}
					}
		}

		public void AddLevel(Level level)
		{
			_levelSet.Levels.Add(level);
			SetChangedState();
		}

		public void InsertLevel(int index, Level level)
		{
			_levelSet.Levels.Insert(index, level);
			SetChangedState();
		}

		public void UpdateLevelProperties(int index, LevelProperties levelProperties)
		{
			_levelSet.Levels[index].LevelProperties = levelProperties;
			SetChangedState();
		}

		public void ClearCurrentLevel()
		{
			CurrentLevel.Clear();
			SetChangedState();
		}

		public void RemoveLevel(int index)
		{
			_levelSet.Levels.RemoveAt(index);
			SetChangedState();
		}

		public void MoveLevel(int i1, int i2)
		{
			_levelSet.Levels.Move(i1, i2);
			SetChangedState();
		}

		public void Reset()
		{
			FilePath = null;
			_levelSet.LevelSetProperties = new LevelSetProperties();
			_levelSet.Levels.Clear();
			_levelSet.Levels.Add(new Level());
			CurrentBrickId = 1;
			CurrentLevelIndex = 0;
			Changed = false;
			UpdateTitle(CurrentAppTitle);
		}

		#region cutscenes
		public void CreateCutscene(string cutsceneName, string[] dialogues, string[] imagePaths, string musicPath)
		{
			if (dialogues.Length != imagePaths.Length)
				throw new InvalidCutsceneException();
			string cutsceneDir = Path.Combine(LevelSetResourceDirectory, cutsceneName);
			Directory.CreateDirectory(cutsceneDir);
			string cutscenePath = Path.Combine(cutsceneDir, $"{cutsceneName}.cutscene");
			string outputMusiMusic = Path.Combine(cutsceneDir, $"music.ogg");
			if (musicPath != outputMusiMusic && !string.IsNullOrEmpty(musicPath))
				File.Copy(musicPath, outputMusiMusic);
			using (FileStream fileStream = File.Create(cutscenePath))
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
				{
					binaryWriter.Write(UltraFlexBallReloadedFileLoader.cutsceneMagicNumber);
					binaryWriter.Write(dialogues.Length);
					for (int i = 0; i < dialogues.Length; i++)
					{
						string outputImageName = Path.Combine(cutsceneDir, $"frame{i + 1}.jpg");
						if (imagePaths[i] != outputImageName)
							File.Copy(imagePaths[i], outputImageName);
						binaryWriter.Write(dialogues[i]);
					}
				}
			}
		}

		public void ImportCutscene(string cutsceneName, out string[] dialogues, out string[] imagePaths, out string musicPath)
		{
			string cutsceneDir = Path.Combine(LevelSetResourceDirectory, cutsceneName);
			string cutscenePath = Path.Combine(cutsceneDir, $"{cutsceneName}.cutscene");
			if (File.Exists(cutscenePath))
			{
				imagePaths = Directory.EnumerateFiles(cutsceneDir, "*.jpg").ToArray();
				musicPath = Path.Combine(cutsceneDir, "music.ogg");
				if (!File.Exists(musicPath))
					musicPath = null;
				using (FileStream fileStream = File.OpenRead(cutscenePath))
				{
					using (BinaryReader BinaryReader = new BinaryReader(fileStream))
					{
						string cutsceneMagicNumber = BinaryReader.ReadString();
						if (cutsceneMagicNumber != UltraFlexBallReloadedFileLoader.cutsceneMagicNumber)
							throw new IOException("Invalid cutscene file.");
						int dialogueCount = BinaryReader.ReadInt32();
						if (dialogueCount != imagePaths.Length)
							throw new IOException("Dialogues are mismatched with frames (their count not equal).");
						dialogues = new string[dialogueCount];
						for (int i = 0; i < dialogueCount; i++)
							dialogues[i] = BinaryReader.ReadString();
					}
				}
			}
			else
			{
				dialogues = null;
				imagePaths = null;
				musicPath = null;
			}
		}
		#endregion

		/**
		 * Get Brick by Id.
		 * <param name="id">Id of the brick</param>
		 */
		public BrickProperties GetBrickById(int id) => Bricks.First(b => b.Id == id);

		public Level GetLevel(int index) => _levelSet.Levels[index];

		public LevelProperties CopyCurrentLevelProperties() => SerializableCopier.Clone(_levelSet.Levels[CurrentLevelIndex].LevelProperties);

		public LevelSetProperties CopyCurrentLevelSetProperties() => SerializableCopier.Clone(_levelSet.LevelSetProperties);

		public void CheckResources()
		{
			string levelSetFileName = LevelSetResourceDirectory;
			resourceChecker.CheckLevelSetResources(levelSetFileName, _levelSet, Bricks);
		}
	}
}
