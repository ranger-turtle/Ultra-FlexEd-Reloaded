using System.Collections.Generic;

namespace LevelSetManagement
{
	public class SmallUtilities
	{
		public static string GetLevelNameForGUI(string levelName, int index) => levelName ?? $"Level {index}";
	}

	public static class ExtensionMethods
	{
		public static void Move<T>(this List<T> list, int i1, int i2)
		{
			T tmp = list[i1];
			list[i1] = list[i2];
			list[i2] = tmp;
		}
	}
}
