using LevelSetData;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ultra_FlexEd_Reloaded.UserControls
{
	/// <summary>
	/// Interaction logic for ImageListBoxItem.xaml
	/// </summary>
	public partial class ImageListBoxItem : ListBoxItem
	{
		public int BrickId { get; set; }
		public new string Content => Label.Content as string;

		public ImageListBoxItem() : this(0, "<none>") {}

		public ImageListBoxItem(int id, string brickName)
		{
			InitializeComponent();
			Label.Content = brickName;
			BrickId = id;
		}

		public ImageListBoxItem(int id, string brickDirectory, string brickName) : this(id, brickName)
		{
			Update(brickDirectory, brickName);
		}

		public ImageListBoxItem(int id, ImageSource imageSource, string brickName) : this(id, brickName)
		{
			Update(imageSource, brickName);
		}

		public void Update(string brickDirectory, string brickName)
		{
			string singleBrickDirectory = $"{brickDirectory}/{brickName}";
			string extension = Path.GetExtension($"{singleBrickDirectory}/frames.png");
			BitmapImage bitmapImage = BitmapMethods.GetImageWithCacheOnLoad(new Uri($"{singleBrickDirectory}/frames.png", UriKind.Relative));
			Image.Source = new CroppedBitmap(bitmapImage, new Int32Rect(0, 0, BrickProperties.PIXEL_WIDTH, BrickProperties.PIXEL_HEIGHT));
			Label.Content = brickName;
		}

		public void Update(ImageSource source, string brickName)
		{
			Image.Source = source;
			Label.Content = brickName;
		}

		public void ClearImage() => Image.Source = null;
	}
}
