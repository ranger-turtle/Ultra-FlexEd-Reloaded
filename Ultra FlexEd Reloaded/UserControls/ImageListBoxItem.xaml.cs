using LevelSetData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Ultra_FlexEd_Reloaded.UserControls
{
	/// <summary>
	/// Interaction logic for ImageListBoxItem.xaml
	/// </summary>
	public partial class ImageListBoxItem : ListBoxItem
	{
		public int BrickId { get; set; }

		public ImageListBoxItem(int id, string brickDirectory, string brickName)
		{
			InitializeComponent();
			Label.Content = brickName;
			Update(brickDirectory, brickName);
			BrickId = id;
		}

		public void Update(string brickDirectory, string brickName)
		{
			string singleBrickDirectory = $"{brickDirectory}/{brickName}";
			string extension = Path.GetExtension(Directory.GetFiles(singleBrickDirectory, "brick0.*")[0]);
			BitmapImage bitmapImage = BitmapMethods.GetImageWithCacheOnLoad(new Uri($"{singleBrickDirectory}/brick0{extension}", UriKind.Relative));
			Image.Source = new CroppedBitmap(bitmapImage, new Int32Rect(0, 0, BrickProperties.PIXEL_WIDTH, BrickProperties.PIXEL_HEIGHT));
			Label.Content = brickName;
		}

		public void ClearImage() => Image.Source = null;
	}
}
