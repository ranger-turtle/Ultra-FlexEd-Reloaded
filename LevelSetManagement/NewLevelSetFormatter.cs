#define NEWLOAD
#define NEWSAVE
using System.Collections.Generic;
using System.IO;
using LevelSetData;

namespace LevelSetManagement
{
	//BONUS try implementing level set compression
	internal class NewLevelSetFormatter : ILevelSetFormatter
	{
		public LevelSet Load(string filepath)
		{
#if NEWLOAD
			return UltraFlexBallReloadedFileLoader.LoadLevelSet(filepath);
#else
			using (FileStream fileStream = File.OpenRead(filepath))
			{
				byte[] fileSignature = new byte[UltraFlexBallReloadedFileLoader.newTypeLevelSetFileSignature.Length];
				fileStream.Read(fileSignature, 0, fileSignature.Length);
				if (Encoding.Default.GetString(fileSignature) == "nuLev")
					return new BinaryFormatter().Deserialize(fileStream) as LevelSet;
				else
					throw new FileFormatException("Invalid Ultra FlexBall Reloaded level set file loaded.");
			}
#endif
		}

		public void Save(string filepath, LevelSet levelSet)
		{
#if NEWSAVE
			using (FileStream fileStream = File.Open(filepath, FileMode.Create))
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
				{
					binaryWriter.Write(UltraFlexBallReloadedFileLoader.newTypeLevelSetFileSignature);
					binaryWriter.Write(levelSet.LevelSetProperties.Name);
					string defaultBackgroundName = levelSet.LevelSetProperties.DefaultBackgroundName;
					if (defaultBackgroundName == string.Empty)
						defaultBackgroundName = "<none>";
					binaryWriter.Write(defaultBackgroundName);
					binaryWriter.Write(levelSet.LevelSetProperties.DefaultMusic);
					binaryWriter.Write(levelSet.LevelSetProperties.DefaultLeftWallName);
					binaryWriter.Write(levelSet.LevelSetProperties.DefaultRightWallName);
					//BONUS write internal function after upgrade to next C# version
					Dictionary<string, string> changedLevelSetSoundNames = levelSet.LevelSetProperties.DefaultSoundLibrary.GetChangedSoundKeys();
					binaryWriter.Write(changedLevelSetSoundNames.Count);
					foreach (KeyValuePair<string, string> keyName in changedLevelSetSoundNames)
					{
						binaryWriter.Write(keyName.Key);
						binaryWriter.Write(keyName.Value);
					}
					binaryWriter.Write(levelSet.Levels.Count);
					foreach (Level level in levelSet.Levels)
					{
						binaryWriter.Write(level.LevelProperties.Name);
						string backgroundName = level.LevelProperties.BackgroundName;
						if (backgroundName == string.Empty)
							backgroundName = "<none>";
						binaryWriter.Write(backgroundName);
						binaryWriter.Write(level.LevelProperties.Music);
						Dictionary<string, string> changedLevelSoundNames = level.LevelProperties.SoundLibrary.GetChangedSoundKeys();
						binaryWriter.Write(changedLevelSoundNames.Count);
						foreach (KeyValuePair<string, string> keyName in changedLevelSoundNames)
						{
							binaryWriter.Write(keyName.Key);
							binaryWriter.Write(keyName.Value);
						}
						binaryWriter.Write(level.LevelProperties.IsQuoteTip);
						binaryWriter.Write(level.LevelProperties.CharacterName);
						binaryWriter.Write(level.LevelProperties.Quote);
						binaryWriter.Write(level.LevelProperties.LeftWallName);
						binaryWriter.Write(level.LevelProperties.RightWallName);
						foreach (BrickInLevel brickInLevel in level.Bricks)
						{
							binaryWriter.Write(brickInLevel.BrickId);
						}
					}
				}
			}
#else
			//Old
			using (FileStream fileStream = File.Open(filepath, FileMode.Create))
			{
				byte[] fileSignature = Encoding.Default.GetBytes(UltraFlexBallReloadedFileLoader.newTypeLevelSetFileSignature);
				fileStream.Write(fileSignature, 0, fileSignature.Length);
				new BinaryFormatter().Serialize(fileStream, levelSet);
			}
#endif
		}
	}
}
