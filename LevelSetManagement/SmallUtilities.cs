﻿using System.Collections.Generic;

namespace LevelSetManagement
{
	public class SmallUtilities
	{
		public static string GetLevelNameForGUI(string levelName, int index) => levelName ?? $"Level {index}";
		public static string MakeTitle(string filepath = "", bool changed = false) =>
			$"{LevelSetManager.MAIN_TITLE} - [{(filepath != null && filepath != "" ? filepath : "Untitled")}{(changed ? "*" : "")}]";
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
