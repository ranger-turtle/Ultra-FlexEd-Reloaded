using LevelSetManagement;
using System.Diagnostics;
using System.IO;

namespace Ultra_FlexEd_Reloaded
{
	public class UltraFlexBallReloadedLevelTester : ILevelTester
	{
		private string GamePath { get; set; }

		public UltraFlexBallReloadedLevelTester(string gamePath)
		{
			GamePath = gamePath;
		}

		public void TestLevelSet(string levelSetName)
		{
			Process.Start(GamePath, $"\"-s:{levelSetName}\"");
		}

		public void TestLevel(string levelSetName, int levelNum)
		{
			Process.Start(GamePath, $"\"-s:{levelSetName}\" -l:{levelNum}");
		}

		public void SetPath(string path)
		{
			if (File.Exists(path))
				GamePath = path;
			else
				throw new FileNotFoundException($"Ultra FlexBall Reloaded executable {path} not found. If you want to test on this game, you must set correct path.");
		}
	}
}
