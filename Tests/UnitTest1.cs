using LevelSetManagement;
using NUnit.Framework;

namespace Tests
{
	public class Tests
	{
		private LevelSetManager levelSetManager = LevelSetManager.GetInstance(true);

		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void UpdateBricksAfterPropertyChange()
		{
			try
			{
				Assert.Pass();
			}
			catch (System.Exception)
			{
				Assert.Fail();
			}
		}
	}
}