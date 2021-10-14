using LevelSetData;
using LevelSetManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ultra_FlexEd_Reloaded.DialogWindows;

namespace Ultra_FlexEd_Reloaded
{
	class LevelEditWindowFactory
	{
		public static Window GenerateLevelAddWindow(FormatType formatType)
		{
			switch (formatType)
			{
				case FormatType.Old:
					return new OldTypeLevelWindow();
				case FormatType.New:
				default:
					return new LevelWindow();
			}
		}

		public static Window GenerateLevelEditWindow(FormatType formatType, LevelProperties levelProperties)
		{
			switch (formatType)
			{
				case FormatType.Old:
					return new OldTypeLevelWindow(levelProperties);
				case FormatType.New:
				default:
					return new LevelWindow(levelProperties);
			}
		}

		public static Window GenerateLevelSetWindow(FormatType formatType, LevelSetProperties levelSetProperties)
		{
			switch (formatType)
			{
				case FormatType.Old:
					return new OldTypeLevelSetWindow(levelSetProperties);
				case FormatType.New:
				default:
					return new LevelSetWindow(levelSetProperties);
			}
		}
	}
}
