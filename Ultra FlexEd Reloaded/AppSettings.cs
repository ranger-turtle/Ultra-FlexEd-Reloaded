using System;

namespace Ultra_FlexEd_Reloaded
{
	public class AppSettings : ICloneable
	{
		public static string UFB2000_DEFAULT_PATH { get; } = "../uflex2000-16.exe";
		public static string UFBR_DEFAULT_PATH { get; } = "../Ultra FlexBall Reloaded.exe";

		public string UltraFlexBall2000Path { get; set; }
		public string UltraFlexBallReloadedPath { get; set; }

		public static AppSettings LoadSettings()
		{
			AppSettings appSettings = new AppSettings
			{
				UltraFlexBall2000Path = Properties.Settings.Default.OldTester,
				UltraFlexBallReloadedPath = Properties.Settings.Default.NewTester
			};
			return appSettings;
		}

		public void SaveSettings()
		{
			Properties.Settings.Default["OldTester"] = UltraFlexBall2000Path;
			Properties.Settings.Default["NewTester"] = UltraFlexBallReloadedPath;
			Properties.Settings.Default.Save();
		}

		public object Clone()
		{
			return new AppSettings
			{
				UltraFlexBall2000Path = UltraFlexBall2000Path,
				UltraFlexBallReloadedPath = UltraFlexBallReloadedPath
			};
		}
	}
}
