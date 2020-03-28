using LevelSetData;
using Microsoft.Win32;
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
using System.Windows.Shapes;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	/// <summary>
	/// Interaction logic for BrickWindow.xaml
	/// </summary>
	public partial class BrickWindow : Window
	{
		private List<string> spriteNameList = new List<string>() { null };
		public BrickProperties BrickProperties { get; set; }

		public static readonly DependencyProperty SpriteNumProperty = 
		  DependencyProperty.Register("SpriteNum", typeof(int), typeof(BrickWindow));

		public int SpriteNum
		{
			get { return (int)GetValue(SpriteNumProperty); }
			set { SetValue(SpriteNumProperty, value); }
		}

		public BrickWindow()
		{
			InitializeComponent();
			DurabilitySlider.ValueChanged += DurabilitySlider_ValueChanged;
			SpriteNum = 1;
		}

		private void DurabilitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			int durability = (int)Math.Round(DurabilitySlider.Value);
			if (spriteNameList.Count < durability)
				spriteNameList.AddRange(new List<string>(durability - spriteNameList.Count()));
			else if (spriteNameList.Count < durability)
				spriteNameList.RemoveRange(durability, spriteNameList.Count - durability);
			if (SpriteNum > DurabilitySlider.Value)
			{
				SpriteNum = (int)Math.Round(SpriteNumSlider.Value);
			}
		}

		private void ImportImage(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				DefaultExt = ".png",
				Filter = "PNG images (.png)|*.png|JPEG images (.jpg, .jpeg)|*.jpg;*.jpeg"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				BitmapImage bitmapImage = new BitmapImage(new Uri(openFileDialog.FileName));
				byte[] bytes = File.ReadAllBytes(openFileDialog.FileName);
				//File.WriteAllBytes()
				//TODO finish it BrickImage.Image.Source = 
			}
		}

		[Serializable]
		class WrongDimensionsException : Exception { }
	}
}
