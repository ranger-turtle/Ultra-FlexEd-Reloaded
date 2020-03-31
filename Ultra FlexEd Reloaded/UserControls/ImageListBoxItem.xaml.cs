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
using Ultra_FlexEd_Reloaded.LevelManagement;

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
			brickDirectory = $"{brickDirectory}/{brickName}";
			string extension = Path.GetExtension(Directory.GetFiles(brickDirectory, "brick0.*")[0]);
			BitmapImage bitmapImage = new BitmapImage(new Uri($"{brickDirectory}/brick0{extension}", UriKind.Relative));
			Image.Source = new CroppedBitmap(bitmapImage, new Int32Rect(0, 0, BrickProperties.PIXEL_WIDTH, BrickProperties.PIXEL_HEIGHT));
			BrickId = id;
		}
	}
}
