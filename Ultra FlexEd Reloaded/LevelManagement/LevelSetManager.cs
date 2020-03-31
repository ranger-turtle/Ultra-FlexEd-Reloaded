using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using LevelSetData;

namespace Ultra_FlexEd_Reloaded.LevelManagement
{
	public class LevelSetManager
	{
		public const int DEFAULT_BRICK_NUMBER = 125;
		public const string DEFAULT_BRICK_DIRECTORY = "Default Bricks";

		public static readonly Comparison<BrickProperties> brickPropertyComparison = (bp1, bp2) => bp1.Id.CompareTo(bp2.Id);

		public LevelSet LevelSet { get; } = new LevelSet();
		public List<BrickProperties> Bricks { get; } = new List<BrickProperties>();
		public SortedDictionary<int, string> BrickNames { get; } = new SortedDictionary<int, string>();
		public int CurrentBrickId { get; set; } = 1;
		public int CurrentLevelIndex { get; set; }
		public bool Hidden { get; set; }

		public BrickProperties CurrentBrick => Bricks[CurrentBrickId];
		public Level CurrentLevel => LevelSet.Levels[CurrentLevelIndex];
		public string CurrentBrickName => BrickNames[CurrentBrickId];

		public LevelSetManager()
		{
			Directory.CreateDirectory(DEFAULT_BRICK_DIRECTORY);
			var brickFilePaths = Directory.EnumerateFiles(DEFAULT_BRICK_DIRECTORY, "*.brick");
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

		public string GetBrickFolder(int brickId) => brickId <= DEFAULT_BRICK_NUMBER ? DEFAULT_BRICK_DIRECTORY: $"Custom/{LevelSet.Name}/Bricks";

		private int EvaluateSmallestAbsentBrickTypeId(int[] ids)
		{
			int i = 1;
			while (i < ids.Length && ids[i] - ids[i - 1] <= 1) i++;
			if (i == int.MaxValue)
				throw new OverflowException($"Could not add a new brick.{Path.PathSeparator}Maximum number of bricks {int.MaxValue} has been exceeded.");
			//else if (i == ids.Length)
				return ids[i - 1] + 1;
			//else
			//	return ids[i] + 1;
		}

		public void AddBrickToLevelSet(string brickName, BrickProperties brick, string[] frameSheetPaths, string hitBrickImagePath)
		{
			if (frameSheetPaths == null) throw new NullReferenceException("Frame sheet paths cannot be null.");
			int[] ids = Bricks.Select(b => b.Id).ToArray();
			int firstAbsentId;
			if (ids.Length == 0)
				firstAbsentId = 1;
			else if (ids.Length == 1)
				firstAbsentId = 2;
			else
				firstAbsentId = EvaluateSmallestAbsentBrickTypeId(ids);
			brick.Id = firstAbsentId;
			string brickSheetFolder = $"{GetBrickFolder(brick.Id)}/{brickName}";
			Directory.CreateDirectory(brickSheetFolder);
			using (FileStream file = File.Create($"{brickSheetFolder}.brick"))
				new BinaryFormatter().Serialize(file, brick);
			Bricks.Add(brick);
			Bricks.Sort(brickPropertyComparison);
			BrickNames[firstAbsentId] = brickName;
			for (int i = 0; i < frameSheetPaths.Length; i++)
			{
				string frameSheetExtension = Path.GetExtension(frameSheetPaths[i]);
				File.Copy(frameSheetPaths[i], $"{brickSheetFolder}/brick{i}{frameSheetExtension}", true);
			}
			if (hitBrickImagePath != null)
			{
				string hitBrickImageExtension = Path.GetExtension(hitBrickImagePath);
				File.Copy(hitBrickImagePath, $"{brickSheetFolder}/hit{hitBrickImageExtension}", true);
			}
		}

		/**
		 * Get Brick by Id.
		 * <param name="id">Id of the brick</param>
		 */
		internal BrickProperties GetBrickById(int id)
		{
			return Bricks.First(b => b.Id == id);
		}
	}
}
