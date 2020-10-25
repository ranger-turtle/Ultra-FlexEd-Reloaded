namespace LevelSetManagement
{
	public interface ILevelTester
	{
		void SetPath(string path);

		void TestLevel(string levelSetName, int levelNum);
		void TestLevelSet(string levelSetName);
	}
}
