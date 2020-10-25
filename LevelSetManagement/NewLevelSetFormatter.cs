using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using LevelSetData;

namespace LevelSetManagement
{
	//BONUS try implementing level set compression
	internal class NewLevelSetFormatter : ILevelSetFormatter
	{
		private const string newLevelSetFileSignature = "nuLev";

		public LevelSet Load(string filepath)
		{
			using (FileStream fileStream = File.OpenRead(filepath))
			{
				byte[] fileSignature = new byte[newLevelSetFileSignature.Length];
				fileStream.Read(fileSignature, 0 , fileSignature.Length);
				if (Encoding.Default.GetString(fileSignature) == "nuLev")
					return new BinaryFormatter().Deserialize(fileStream) as LevelSet;
				else
					throw new FileFormatException("Invalid Ultra FlexBall Reloaded level set file loaded.");
			}
		}

		public void Save(string filepath, LevelSet levelSet)
		{
			using (FileStream fileStream = File.Open(filepath, FileMode.Create))
			{
				byte[] fileSignature = Encoding.Default.GetBytes(newLevelSetFileSignature);
				fileStream.Write(fileSignature, 0 , fileSignature.Length);
				new BinaryFormatter().Serialize(fileStream, levelSet);
			}
		}
	}
}
