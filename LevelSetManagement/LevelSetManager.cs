using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using LevelSetData;

namespace LevelSetManagement
{
	//TODO add automatic update events (e.g. onBrickUpdate, onLevelUpdate)
	//TODO add brick import (cannot add bricks with mere copying brick file due to possible id conflicts)
	public class LevelSetManager
	{
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

		public bool Changed { get; private set; }

		private LevelSet _levelSet = new LevelSet();
		public List<BrickProperties> Bricks { get; } = new List<BrickProperties>();
		public SortedDictionary<int, string> BrickNames { get; } = new SortedDictionary<int, string>();
		public string FilePath { get; private set; }
		public string FilePathWithoutExtension => Path.Combine(Path.GetDirectoryName(FilePath), Path.GetFileNameWithoutExtension(FilePath));

		public int CurrentBrickId { get; set; } = 1;
		public int CurrentLevelIndex { get; set; }

		public BrickProperties CurrentBrick => Bricks[CurrentBrickId];
		private Level CurrentLevel => _levelSet.Levels[CurrentLevelIndex];
		public string CurrentBrickName => BrickNames[CurrentBrickId];
		public string CurrentLevelName => _levelSet.Levels[CurrentLevelIndex].LevelProperties.Name;
		public FormatType CurrentFormatType => string.IsNullOrEmpty(FilePath) ? formatTypes[Path.GetExtension(FilePath)] : FormatType.New;
		public bool LevelLoaded => FilePath != null;
		public int LevelCount => _levelSet.Levels.Count;
		private string CurrentAppTitle =>
			$"{MAIN_TITLE} - [{(FilePath != null && FilePath != string.Empty ? FilePath : "Untitled")}{(Changed ? "*" : string.Empty)}]";

		public ILevelTester CurrentTester { get; private set; }

		private static LevelSetManager instance;

		private LevelSetManager()
		{
			LoadBricks(DEFAULT_BRICK_DIRECTORY);
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

		private void LoadBricks(string directory)
		{
			var brickFilePaths = Directory.EnumerateFiles(directory, "*.brick");
			if (brickFilePaths.Count() != DEFAULT_BRICK_QUANTITY)
				throw new FileNotFoundException($"{DEFAULT_BRICK_QUANTITY - brickFilePaths.Count()} of {DEFAULT_BRICK_QUANTITY} brick files are missing. {Environment.NewLine} Please redownload game to restore bricks or find missing brick(s).");
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			foreach (string brickName in brickFilePaths)
				using (FileStream brickFile = File.OpenRead(brickName))
				{
					BrickProperties brickProperties = binaryFormatter.Deserialize(brickFile) as BrickProperties;
					Bricks.Add(brickProperties);
					BrickNames[brickProperties.Id] = Path.GetFileNameWithoutExtension(brickName);
					CheckBrickFileSystem(directory, BrickNames[brickProperties.Id]);
				}
			Bricks.Sort(brickPropertyComparison);
		}

		public static LevelSetManager GetInstance(bool reset = false) =>
			instance == null || reset ? (instance = new LevelSetManager()) : instance;

		public string GetBrickFolder(int brickId) => brickId <= DEFAULT_BRICK_QUANTITY ? DEFAULT_BRICK_DIRECTORY : $"Custom/{_levelSet.LevelSetProperties.Name}/Bricks";

		public void LoadLevelSetFile(string filepath)
		{
			string extension = Path.GetExtension(filepath);
			string levelSetName = Path.GetFileNameWithoutExtension(filepath);
			ILevelSetFormatter formatter = LevelSetFormatterFactory.GenerateLevelSetFormatter(formatTypes[extension]);
			_levelSet = formatter.Load(filepath);
			FilePath = filepath;
			if (Bricks.Count > DEFAULT_BRICK_QUANTITY)
			{
				for (int i = DEFAULT_BRICK_QUANTITY; i < Bricks.Count; i++)
					BrickNames.Remove(Bricks[i].Id);
				Bricks.RemoveRange(DEFAULT_BRICK_QUANTITY, Bricks.Count - DEFAULT_BRICK_QUANTITY);
			}
			if (formatTypes[extension] == FormatType.New && Directory.Exists(levelSetName))
				LoadBricks(levelSetName);
			CurrentTester = testers[formatTypes[extension]];
			CurrentBrickId = 1;
			CurrentLevelIndex = 0;
			Changed = false;
			UpdateTitle(CurrentAppTitle);
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

		public void AddBrickToLevelSet(string brickName, BrickProperties brick, string frameSheetPath, string hitBrickImagePath)
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
			BrickNames[firstAbsentId] = brickName;
			SaveBrick(brickName, brick, frameSheetPath, hitBrickImagePath);
		}

		public void UpdateBrick(string brickName, BrickProperties brick, string frameSheetPath, string hitBrickImagePath)
		{
			string oldBrickName = BrickNames[brick.Id];
			BrickNames[brick.Id] = brickName;
			Bricks[Bricks.FindIndex(b => b.Id == brick.Id)] = brick;
			if (oldBrickName != brickName)
			{
				Directory.Move($"{GetBrickFolder(brick.Id)}/{oldBrickName}/", $"{GetBrickFolder(brick.Id)}/{brickName}/");
				File.Move($"{GetBrickFolder(brick.Id)}/{oldBrickName}.brick", $"{GetBrickFolder(brick.Id)}/{brickName}.brick");
			}
			SaveBrick(brickName, brick, frameSheetPath, hitBrickImagePath);
		}

		//BONUS try implementing brick file compression
		private void SaveBrick(string brickName, BrickProperties brick, string frameSheetPath, string hitBrickImagePath)
		{
			string brickSheetFolder = $"{GetBrickFolder(brick.Id)}/{brickName}";
			Directory.CreateDirectory(brickSheetFolder);
			using (FileStream file = File.Create($"{brickSheetFolder}.brick"))
				new BinaryFormatter().Serialize(file, brick);
			if (frameSheetPath != null)
			{
				string frameSheetExtension = Path.GetExtension(frameSheetPath);
				File.Copy(frameSheetPath, $"{brickSheetFolder}/frames{frameSheetExtension}", true);
			}
			if (hitBrickImagePath != null)
			{
				string hitBrickImageExtension = Path.GetExtension(hitBrickImagePath);
				File.Copy(hitBrickImagePath, $"{brickSheetFolder}/hit{hitBrickImageExtension}", true);
			}
		}

		public void RemoveBrick(int brickId)
		{
			string brickName = BrickNames[brickId];
			Directory.Delete($"Default Bricks/{brickName}", true);
			File.Delete($"Default Bricks/{brickName}.brick");
			BrickNames.Remove(brickId);
			Bricks.Remove(Bricks.Find(b => b.Id == brickId));
			ClearBlocksOfType(brickId);
		}

		public BrickInLevel CopyBrickInCurrentLevel(int brickX, int brickY)
		{
			return _levelSet.Levels[CurrentLevelIndex].Bricks[brickY, brickX].Clone() as BrickInLevel;
		}

		public void UpdateBrickInLevel(int brickX, int brickY, BrickInLevel brickInLevel)
		{
			_levelSet.Levels[CurrentLevelIndex].Bricks[brickY, brickX] = brickInLevel;
		}

		public void ClearBlocksOfType(int brickId)
		{
			foreach (Level level in _levelSet.Levels)
				foreach (BrickInLevel brickInLevel in level.Bricks)
					if (brickInLevel.BrickId == brickId)
						brickInLevel.Reset();
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

		/**
		 * Get Brick by Id.
		 * <param name="id">Id of the brick</param>
		 */
		public BrickProperties GetBrickById(int id) => Bricks.First(b => b.Id == id);

		public Level GetLevel(int index) => _levelSet.Levels[index];

		public LevelProperties CopyCurrentLevelProperties() => SerializableCopier.Clone(_levelSet.Levels[CurrentLevelIndex].LevelProperties);

		public LevelSetProperties CopyCurrentLevelSetProperties() => SerializableCopier.Clone(_levelSet.LevelSetProperties);
	}
}
