namespace LevelSetManagement
{
	public enum FormatType
	{
		Old, New
	}

	public class LevelSetFormatterFactory
	{
		public static ILevelSetFormatter GenerateLevelSetFormatter(FormatType format)
		{
			switch (format)
			{
				case FormatType.Old:
					return new OldLevelSetFormatter();
				case FormatType.New:
					return new NewLevelSetFormatter();
				default:
					return new NewLevelSetFormatter();
			}
		}
	}
}
