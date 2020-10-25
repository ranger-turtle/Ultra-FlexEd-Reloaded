using LevelSetData;

namespace LevelSetManagement
{
	public interface ILevelSetFormatter
	{
		LevelSet Load(string filename);
		void Save(string filename, LevelSet levelSet);
	}
}
