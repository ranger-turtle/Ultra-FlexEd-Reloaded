using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using LevelSetData;

namespace LevelSetManagement
{
	public class FramesheetMetadata
	{
		public string FrameSheetPath { get; set; }
		public int FrameSheetIndex { get; set; }
	}

	//TODO add automatic update events (e.g. onBrickUpdate, onLevelUpdate)
	public class LevelSetManager
	{
		public const string MAIN_TITLE = "Ultra FlexEd Reloaded";

		public const int DEFAULT_BRICK_QUANTITY = 125;
		public const string DEFAULT_BRICK_DIRECTORY = "Default Bricks";

		private readonly Comparison<BrickProperties> brickPropertyComparison = (bp1, bp2) => bp1.Id.CompareTo(bp2.Id);

		public EventHandler UpdateTitle { get; set; }

		private readonly Dictionary<string, FormatType> _formatTypes = new Dictionary<string, FormatType>
		{
			[".nlev"] = FormatType.New,
			[".lev"] = FormatType.Old
		};

		public bool Changed { get; private set; }

		private LevelSet _levelSet = new LevelSet();
		public List<BrickProperties> Bricks { get; } = new List<BrickProperties>();
		public SortedDictionary<int, string> BrickNames { get; } = new SortedDictionary<int, string>();
		public string FilePath { get; private set; }

		public int CurrentBrickId { get; set; } = 1;
		public int CurrentLevelIndex { get; set; }
		public bool Hidden { get; set; }

		public BrickProperties CurrentBrick => Bricks[CurrentBrickId];
		private Level CurrentLevel => _levelSet.Levels[CurrentLevelIndex];
		public string CurrentBrickName => BrickNames[CurrentBrickId];
		public string CurrentLevelName => _levelSet.Levels[CurrentLevelIndex].LevelProperties.Name;
		public string CurrentLevelSetName => _levelSet.Name;
		public FormatType CurrentFormatType => FilePath != "" && FilePath != null ? _formatTypes[Path.GetExtension(FilePath)] : FormatType.New;
		public bool LevelLoaded => FilePath != null;
		public int LevelCount => _levelSet.Levels.Count;

		private static LevelSetManager instance;

		private LevelSetManager()
		{
			LoadBricks(DEFAULT_BRICK_DIRECTORY);
			_levelSet.Levels.Add(new Level());
		}

		private void LoadBricks(string directory)
		{
			var brickFilePaths = Directory.EnumerateFiles(directory, "*.brick");
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			foreach (string brickName in brickFilePaths)
				using (FileStream brickFile = File.OpenRead(brickName))
				{
					BrickProperties brickProperties = binaryFormatter.Deserialize(brickFile) as BrickProperties;
					Bricks.Add(brickProperties);
					BrickNames[brickProperties.Id] = Path.GetFileNameWithoutExtension(brickName);
				}
			Bricks.Sort(brickPropertyComparison);
		}

		public static LevelSetManager GetInstance(bool reset = false) =>
			instance == null || reset ? (instance = new LevelSetManager()) : instance;

		public string GetBrickFolder(int brickId) => brickId <= DEFAULT_BRICK_QUANTITY ? DEFAULT_BRICK_DIRECTORY : $"Custom/{_levelSet.Name}/Bricks";

		public void LoadFile(string filepath)
		{
			string extension = Path.GetExtension(filepath);
			ILevelSetFormatter formatter = LevelSetFormatterFactory.GenerateLevelSetFormatter(_formatTypes[extension]);
			_levelSet = formatter.Load(filepath);
			FilePath = filepath;
			if (Bricks.Count > DEFAULT_BRICK_QUANTITY)
			{
				for (int i = DEFAULT_BRICK_QUANTITY; i < Bricks.Count; i++)
					BrickNames.Remove(Bricks[i].Id);
				Bricks.RemoveRange(DEFAULT_BRICK_QUANTITY, Bricks.Count - DEFAULT_BRICK_QUANTITY);
			}
			if (_formatTypes[extension] == FormatType.New)
				LoadBricks($"{filepath}/Bricks");
			CurrentBrickId = 1;
			CurrentLevelIndex = 0;
			Changed = false;
			UpdateTitle(null, null);
		}

		public Level[] GetLevelsFromFile(string filepath)
		{
			string extension = Path.GetExtension(filepath);
			ILevelSetFormatter formatter = LevelSetFormatterFactory.GenerateLevelSetFormatter(_formatTypes[extension]);
			return formatter.Load(filepath).Levels.ToArray();
		}

		public void SaveFile() => SaveFile(FilePath);

		public void SaveFile(string filepath)
		{
			string extension = Path.GetExtension(filepath);
			ILevelSetFormatter formatter = LevelSetFormatterFactory.GenerateLevelSetFormatter(_formatTypes[extension]);
			formatter.Save(filepath, _levelSet);
			Changed = false;
			//TODO implement history index assignment
		}

		public void Change()
		{
			Changed = true;
			UpdateTitle(null, null);
			//TODO implement change history
		}

		public void UpdateLevelSet(string name)
		{
			_levelSet.Name = name;
			Change();
		}

		private int EvaluateSmallestAbsentBrickTypeId(int[] ids)
		{
			int i = 1;
			while (i < ids.Length && ids[i] - ids[i - 1] <= 1) i++;
			if (i == int.MaxValue)
				throw new OverflowException($"Could not add a new brick.{Path.PathSeparator}Maximum number of bricks {int.MaxValue} has been exceeded.");
			return ids[i - 1] + 1;
		}

		public void AddBrickToLevelSet(string brickName, BrickProperties brick, FramesheetMetadata[] frameSheetMetadata, string hitBrickImagePath)
		{
			if (frameSheetMetadata == null) throw new NullReferenceException("Frame sheet paths cannot be null.");
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
			SaveBrick(brickName, brick, frameSheetMetadata, hitBrickImagePath);
		}

		public void UpdateBrick(string brickName, BrickProperties brick, FramesheetMetadata[] frameSheetMetadata, string hitBrickImagePath)
		{
			string oldBrickName = BrickNames[brick.Id];
			BrickNames[brick.Id] = brickName;
			Bricks[Bricks.FindIndex(b => b.Id == brick.Id)] = brick;
			if (oldBrickName != brickName)
			{
				Directory.Move($"{GetBrickFolder(brick.Id)}/{oldBrickName}/", $"{GetBrickFolder(brick.Id)}/{brickName}/");
				File.Move($"{GetBrickFolder(brick.Id)}/{oldBrickName}.brick", $"{GetBrickFolder(brick.Id)}/{brickName}.brick");
			}
			SaveBrick(brickName, brick, frameSheetMetadata, hitBrickImagePath);
		}

		private void SaveBrick(string brickName, BrickProperties brick, FramesheetMetadata[] frameSheetMetadata, string hitBrickImagePath)
		{
			string brickSheetFolder = $"{GetBrickFolder(brick.Id)}/{brickName}";
			Directory.CreateDirectory(brickSheetFolder);
			using (FileStream file = File.Create($"{brickSheetFolder}.brick"))
				new BinaryFormatter().Serialize(file, brick);
			for (int i = 0; i < frameSheetMetadata.Length; i++)
			{
				string frameSheetExtension = Path.GetExtension(frameSheetMetadata[i].FrameSheetPath);
				File.Copy(frameSheetMetadata[i].FrameSheetPath, $"{brickSheetFolder}/brick{frameSheetMetadata[i].FrameSheetIndex}{frameSheetExtension}", true);
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
			Change();
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
			Change();
		}

		public void InsertLevel(int index, Level level)
		{
			_levelSet.Levels.Insert(index, level);
			Change();
		}

		public void UpdateLevelProperties(int index, LevelProperties levelProperties)
		{
			_levelSet.Levels[index].LevelProperties = levelProperties;
			Change();
		}

		public void RemoveLevel(int index)
		{
			_levelSet.Levels.RemoveAt(index);
			Change();
		}

		public void MoveLevel(int i1, int i2)
		{
			_levelSet.Levels.Move(i1, i2);
			Change();
		}

		public void Reset()
		{
			_levelSet.Name = null;
			_levelSet.Levels.Clear();
			_levelSet.Levels.Add(new Level());
			CurrentBrickId = 1;
			CurrentLevelIndex = 0;
			Hidden = false;
		}

		/**
		 * Get Brick by Id.
		 * <param name="id">Id of the brick</param>
		 */
		public BrickProperties GetBrickById(int id) => Bricks.First(b => b.Id == id);

		public Level GetLevel(int index) => _levelSet.Levels[index];

		public LevelProperties CopyCurrentLevelProperties() => _levelSet.Levels[CurrentLevelIndex].LevelProperties.Clone() as LevelProperties;
	}
}
