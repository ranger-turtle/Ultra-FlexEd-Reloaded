using LevelSetData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelSetManagement
{
	internal class ResourceChecker
	{
		private List<string> corruptBrickNames = new List<string>();

		public void CheckLevelSetResources(string levelSetFileName, LevelSet levelSet, List<BrickProperties> brickProperties, SortedDictionary<int, string> brickNames)
		{
			List<string> missingResourceNames = new List<string>();
			List<int> missingBrickIds = new List<int>();

			CheckSounds(levelSetFileName, levelSet, missingResourceNames);
			CheckMusic(levelSetFileName, levelSet, missingResourceNames);
			CheckBackgrounds(levelSetFileName, levelSet, missingResourceNames);
			CheckBricks(levelSetFileName, levelSet, brickProperties, brickNames, missingResourceNames, missingBrickIds);
			CheckIfNonExistentBricksWereAdded(levelSet, brickNames, missingBrickIds);

			if (missingResourceNames.Count > 0)
				throw new ResourceCheckFailException()
				{
					MissingResourceNames = missingResourceNames,
					MissingBrickIds = missingBrickIds
				};
		}

		public void AddCorruptBrickNames(List<string> corruptBrickNames)
			=> this.corruptBrickNames = corruptBrickNames;

		private bool NotSpecialResourceName(string resourceName) => resourceName[0] != '<';

		private void CheckSounds(string levelSetFileName, LevelSet levelSet, List<string> missingResourceNames)
		{
			string soundDirectory = $"{levelSetFileName}/Sounds";

			string[] soundKeys = SoundLibrary.GetSoundKeys().ToArray();
			for (int i = 0; i < soundKeys.Length; i++)
			{
				string defaultSoundName = levelSet.LevelSetProperties.DefaultSoundLibrary.FromStringKey(soundKeys[i]);
				string defaultSoundNamePath = $"{soundDirectory}/{defaultSoundName}.wav";
				if (NotSpecialResourceName(defaultSoundName) && !File.Exists(defaultSoundNamePath))
					missingResourceNames.Add($"{defaultSoundNamePath} assigned to default sound of {soundKeys[i]}");
			}
			for (int i = 0; i < levelSet.Levels.Count(); i++)
			{
				for (int j = 0; j < soundKeys.Length; j++)
				{
					string soundName = levelSet.Levels[i].LevelProperties.SoundLibrary.FromStringKey(soundKeys[j]);
					string soundNamePath = $"{soundDirectory}/{soundName}.wav";
					if (NotSpecialResourceName(soundName) && !File.Exists(soundNamePath))
						missingResourceNames.Add($"{soundNamePath} assigned to sound of {soundKeys[j]} in level {i + 1} ({levelSet.Levels[i].LevelProperties.Name})");
				}
			}
		}

		private void CheckMusic(string levelSetFileName, LevelSet levelSet, List<string> missingResourceNames)
		{
			string musicDirectory = $"{levelSetFileName}/Music";

			string levelSetDefaultMusic = levelSet.LevelSetProperties.DefaultMusic;
			string levelSetDefaultMusicPath = $"{musicDirectory}/{levelSetDefaultMusic}.ogg";
			if (NotSpecialResourceName(levelSetDefaultMusic) && !File.Exists(levelSetDefaultMusicPath))
				missingResourceNames.Add($"{levelSetDefaultMusicPath} assigned to default music");
			for (int i = 0; i < levelSet.Levels.Count(); i++)
			{
				string musicName = levelSet.Levels[i].LevelProperties.Music;
				string musicNamePath = $"{musicDirectory}/{musicName}.ogg";
				if (NotSpecialResourceName(musicName) && !File.Exists(musicNamePath))
					missingResourceNames.Add($"{musicNamePath} to music in level {i + 1} ({levelSet.Levels[i].LevelProperties.Name})");
			}
		}

		private void CheckBackgrounds(string levelSetFileName, LevelSet levelSet, List<string> missingResourceNames)
		{
			string backgroundDirectory = $"{levelSetFileName}/Backgrounds";

			string levelSetDefaultBackgroundName = levelSet.LevelSetProperties.DefaultBackgroundName;
			string levelSetDefaultBackgroundNamePath = $"{backgroundDirectory}/{levelSetDefaultBackgroundName}.png";
			if (NotSpecialResourceName(levelSetDefaultBackgroundName) && !File.Exists(levelSetDefaultBackgroundNamePath))
				missingResourceNames.Add($"{levelSetDefaultBackgroundNamePath} assigned to default background");
			for (int i = 0; i < levelSet.Levels.Count(); i++)
			{
				string backgroundName = levelSet.Levels[i].LevelProperties.BackgroundName;
				string backgroundNamePath = $"{backgroundDirectory}/{backgroundName}.png";
				if (NotSpecialResourceName(backgroundName) && !File.Exists(backgroundNamePath))
					missingResourceNames.Add($"{backgroundNamePath} assigned to background in level {i + 1} ({levelSet.Levels[i].LevelProperties.Name})");
			}
		}

		private void CheckBricks(string levelSetFileName, LevelSet levelSet, List<BrickProperties> brickProperties, SortedDictionary<int, string> brickNames, List<string> missingResourceNames, List<int> missingBrickIds)
		{
			string brickDirectory = $"{levelSetFileName}/Bricks";

			foreach (var corruptBrickName in corruptBrickNames)
			{
				missingResourceNames.Add($"{corruptBrickName} is corrupt.");
			}

			for (int i = 0; i < brickProperties.Count; i++)
			{
				int idOfCheckedBrick = brickProperties[i].Id;
				string brickHitSound = brickProperties[i].HitSoundName;
				string brickHitSoundPath = $"{levelSetFileName}/Sounds/{brickHitSound}.wav";
				if (NotSpecialResourceName(brickHitSound) && !File.Exists(brickHitSoundPath))
					missingResourceNames.Add($"{brickHitSoundPath} assigned to hit sound of brick \"{brickNames[idOfCheckedBrick]}\"");
				string brickFramePath = $"{brickDirectory}/{brickNames[idOfCheckedBrick]}/frames.png";
				if (!File.Exists(brickFramePath))
				{
					missingResourceNames.Add($"{brickFramePath} assigned to brick \"{brickNames[idOfCheckedBrick]}\"");
					if (levelSet.Levels.Select(l => l.Bricks.Cast<BrickInLevel>()).SelectMany(bc => bc).Any(b => b.BrickId == idOfCheckedBrick))
					{
						missingResourceNames.Add($"Brick \"{brickNames[idOfCheckedBrick]}\"");
						missingBrickIds.Add(idOfCheckedBrick);
					}
				}
			}
		}

		public void CheckDefaultBricks()
		{
			List<string> errors = new List<string>();

			foreach (var corruptBrickName in corruptBrickNames)
			{
				errors.Add($"{corruptBrickName} is corrupt.");
			}

			if (errors.Count > 0)
				throw new ResourceCheckFailException()
				{
					MissingResourceNames = errors
				};
		}

		private void CheckIfNonExistentBricksWereAdded(LevelSet levelSet, SortedDictionary<int, string> brickNames, List<int> missingBrickIds)
		{
			List<int> uniqueIdsUsedInLevelSet = levelSet.Levels.Select(l => l.Bricks.Cast<BrickInLevel>()).SelectMany(b => b).Select(b => b.BrickId).Distinct().ToList();
			foreach (int brickId in uniqueIdsUsedInLevelSet)
			{
				if (!brickNames.ContainsKey(brickId) && brickId != 0)
				{
					missingBrickIds.Add(brickId);
				}
			}
		}
	}

	public class ResourceCheckFailException : FileNotFoundException
	{
		public List<string> MissingResourceNames { get; internal set; }
		public List<int> MissingBrickIds { get; internal set; }
	}
}
