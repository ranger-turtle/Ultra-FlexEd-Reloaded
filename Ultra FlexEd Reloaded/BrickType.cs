using LevelSetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Ultra_FlexEd_Reloaded
{
	public class BrickType
	{
		public BitmapImage Image { get; }
		public BrickProperties Properties { get; }
		public string BrickName { get; set; }

		public BrickType() { }

		public BrickType(BitmapImage image, BrickProperties properties, string brickName)
		{
			Image = image;
			Properties = properties;
			BrickName = brickName;
		}
	}
}
