using LevelSetManagement;
using System.Diagnostics;
using System.IO;

namespace Ultra_FlexEd_Reloaded
{
	public class UltraFlexBall2000LevelTester : ILevelTester
	{
		private string GamePath { get; set; }

		public UltraFlexBall2000LevelTester(string gamePath) => GamePath = gamePath;

		public void TestLevelSet(string levelSetName)
		{
			Directory.SetCurrentDirectory(Path.GetDirectoryName(GamePath));
			Process.Start(Path.GetFileName(GamePath), $"/\"{levelSetName}\"");
			Directory.SetCurrentDirectory(MainWindow.DIRECTORY);
		}

		public void TestLevel(string levelSetName, int levelNum)
		{
			Directory.SetCurrentDirectory(Path.GetDirectoryName(GamePath));
			Process.Start(Path.GetFileName(GamePath), $"/\"{levelSetName}\" /t:{levelNum}");
			Directory.SetCurrentDirectory(MainWindow.DIRECTORY);
		}

		public void SetPath(string path) => GamePath = path;
	}
}
