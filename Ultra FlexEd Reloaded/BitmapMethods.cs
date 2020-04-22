using System;
using System.Windows.Media.Imaging;

namespace Ultra_FlexEd_Reloaded
{
	internal class BitmapMethods
	{
		internal static BitmapImage GetImageWithCacheOnLoad(Uri source)
		{
			var image = new BitmapImage();

			image.BeginInit();
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.UriSource = source;
			image.EndInit();

			return image;
		}

	}
}
