using LevelSetData;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using LevelSetManagement;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	/// <summary>
	/// Interaction logic for BrickWindow.xaml
	/// </summary>
	//TODO Add brick search and selection menu
	public partial class BrickWindow : Window
	{
		private class BrickEditMetadata
		{
			public BitmapImage Image { get; set; }
			public int Frames { get; set; }
			public bool Changed { get; internal set; } = true;
		}
		private enum Mode
		{
			ADD,EDIT
		}
		private List<BrickEditMetadata> _brickMetadata = new List<BrickEditMetadata>() { null };
		internal FramesheetMetadata[] FrameSheetMetadata { get; private set; }
		private Mode mode = Mode.ADD;

		internal static readonly DependencyProperty FrameSheetNumProperty = 
		  DependencyProperty.Register("FrameSheetNum", typeof(int), typeof(BrickWindow));

		internal int FrameSheetNum
		{
			get => (int)GetValue(FrameSheetNumProperty); 
			set => SetValue(FrameSheetNumProperty, value);
		}

		internal static readonly DependencyProperty FrameNumProperty = 
		  DependencyProperty.Register("FrameNum", typeof(int), typeof(BrickWindow));

		internal int FrameNum
		{
			get => (int)GetValue(FrameNumProperty);
			set => SetValue(FrameNumProperty, value);
		}

		internal static readonly DependencyProperty BrickNameProperty =
			DependencyProperty.Register("BrickName", typeof(string), typeof(BrickWindow));

		internal string BrickName
		{
			get => GetValue(BrickNameProperty) as string;
			set => SetValue(BrickNameProperty, value); 
		}

		internal BrickWindow()
		{
			InitializeComponent();
			DataContext = new BrickProperties();
			DurabilitySlider.ValueChanged += DurabilitySlider_ValueChanged;
			FrameSheetSlider.ValueChanged += FrameSheetSlider_ValueChanged;
			FrameSlider.ValueChanged += FrameSlider_ValueChanged;
			RequiredToCompleteCheckBox.Checked += RequiredToCompleteCheckBox_Checked;
			NormalResistantCheckBox.Checked += NormalResistantCheckBox_Checked;
			FrameSheetNum = FrameNum = 1;
			BrickName = "New Brick";
			brickNameField.Focus();
		}

		internal BrickWindow(BrickProperties brickProperties, string brickName, string levelName) : this()
		{
			BrickName = brickName;
			DataContext = SerializableCopier.Clone(brickProperties);
			Title = $"Edit Brick {brickName}";
			_brickMetadata = new List<BrickEditMetadata>();
			//TODO Uncomment when you create all fixed brick types var brickFilePaths = Directory.EnumerateFiles($"Custom/{levelName}/{brickName}", "brick*.*");
			var brickFilePaths = Directory.EnumerateFiles($"Default Bricks/{brickName}", "brick*.*");
			foreach (string brickFilePath in brickFilePaths)
			{
				BitmapImage bitmapImage = BitmapMethods.GetImageWithCacheOnLoad(new Uri(brickFilePath, UriKind.Relative));
				_brickMetadata.Add(new BrickEditMetadata
				{
					Image = bitmapImage,
					Frames = EvaluateFrameNumberFromImage(bitmapImage),
					Changed = false
				});
			}
			ReplaceBrickImage(_brickMetadata[0].Image, 0, 0);
			EnableFrameSliderWhenTrue(CurrentBrickEditMetadata.Frames > 1);
			mode = Mode.EDIT;
		}

		private void DurabilitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			int durability = (int)Math.Round(DurabilitySlider.Value);
			if (_brickMetadata.Count < durability)
				for (int i = 0; i < durability - _brickMetadata.Count; i++)
					_brickMetadata.Add(null);
			else if (_brickMetadata.Count > durability)
				_brickMetadata.RemoveRange(durability, _brickMetadata.Count - durability);
			if (FrameSheetNum > DurabilitySlider.Value)
				FrameSheetNum = (int)Math.Round(FrameSheetSlider.Value);
		}

		private void ReplaceBrickImage(BitmapImage framesheet, int x, int y)
		{
			CroppedBitmap croppedBitmap = new CroppedBitmap(framesheet, new Int32Rect(x * BrickProperties.PIXEL_WIDTH, y * BrickProperties.PIXEL_HEIGHT, BrickProperties.PIXEL_WIDTH, BrickProperties.PIXEL_HEIGHT));
			BrickImage.Source = croppedBitmap;
		}

		private void FrameSheetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (CurrentBrickEditMetadata != null)
			{
				ReplaceBrickImage(CurrentBrickEditMetadata.Image, 0, 0);
				bool shouldEnableFrameSlider = CurrentBrickEditMetadata.Frames > 1;
				EnableFrameSliderWhenTrue(shouldEnableFrameSlider);
				if (shouldEnableFrameSlider)
					FrameSlider.Maximum = CurrentBrickEditMetadata.Frames;
			}
			else
				BrickImage.Source = null;
		}

		private void FrameSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			TwoIndices spriteIndices = TwoIndices.EvaluateIndices(FrameNum - 1, CurrentBrickEditMetadata.Image.PixelWidth / BrickProperties.PIXEL_WIDTH);
			ReplaceBrickImage(CurrentBrickEditMetadata.Image, spriteIndices.X, spriteIndices.Y);
		}

		private void EnableFrameSliderWhenTrue(bool enable)
		{
			FrameSection.IsEnabled = enable;
			if (FrameSection.IsEnabled)
			{
				FrameSlider.Maximum = _brickMetadata[FrameSheetNum - 1].Frames;
				FrameSlider.Value = 1;
			}
		}

		/**<summary>
		 <para>Checks if width and height of <c><paramref name="bitmapImage"/></c> are multiplies of standard dimensions
		 of brick image declared in BrickProperties.</para>
		 <para>See <see cref="DataContext"/> to check standard brick image dimensions.</para>
		 </summary>*/
		private void CheckImageDimensions(BitmapImage bitmapImage, string fileName)
		{
			if (bitmapImage.PixelWidth % BrickProperties.PIXEL_WIDTH != 0 && bitmapImage.PixelHeight % BrickProperties.PIXEL_HEIGHT != 0)
				throw new BadImageFormatException($"Width is not multiply of {BrickProperties.PIXEL_WIDTH} or height is not multiply of {BrickProperties.PIXEL_HEIGHT}", fileName);
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
				BitmapImage bitmapImage = BitmapMethods.GetImageWithCacheOnLoad(new Uri(openFileDialog.FileName));
				try
				{
					CheckImageDimensions(bitmapImage, openFileDialog.FileName);
					CurrentBrickEditMetadata = new BrickEditMetadata
					{
						Image = bitmapImage,
						Frames = EvaluateFrameNumberFromImage(bitmapImage),
						Changed = true
					};
					ReplaceBrickImage(bitmapImage, 0, 0);
					EnableFrameSliderWhenTrue(CurrentBrickEditMetadata.Frames > 1);
				}
				catch (BadImageFormatException ex)
				{
					MessageBox.Show($"Error occured at opening {ex.FileName}: \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private static int EvaluateFrameNumberFromImage(BitmapImage bitmapImage) =>
			(bitmapImage.PixelWidth / BrickProperties.PIXEL_WIDTH) * (bitmapImage.PixelHeight / BrickProperties.PIXEL_HEIGHT);

		private void NormalResistantCheckBox_Checked(object sender, RoutedEventArgs e) => RequiredToCompleteCheckBox.IsChecked = false;

		private void RequiredToCompleteCheckBox_Checked(object sender, RoutedEventArgs e) => NormalResistantCheckBox.IsChecked = false;

		private void Ok_Clicked(object sender, RoutedEventArgs e)
		{
			switch(mode)
			{
				case Mode.ADD:
					if (!_brickMetadata.Any(bm => bm == null))
					{
						FrameSheetMetadata = _brickMetadata.Select(bm => new FramesheetMetadata { FrameSheetPath = bm.Image.UriSource.OriginalString}).ToArray();
						for (int i = 0; i < FrameSheetMetadata.Length; i++) FrameSheetMetadata[i].FrameSheetIndex = i;
						DialogResult = true;
					}
					else
						MessageBox.Show("You must assign all frame sheets to each brick state.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
					break;
				case Mode.EDIT:
					FrameSheetMetadata = _brickMetadata.Where(bm => bm.Changed).Select(bm => new FramesheetMetadata { FrameSheetPath = bm.Image.UriSource.OriginalString}).ToArray();
					for (int i = 0, j = 0; i < _brickMetadata.Count; i++)
						if (_brickMetadata[i].Changed)
						{
							FrameSheetMetadata[j].FrameSheetIndex = i;
							j++;
						}
					DialogResult = true;
					break;
			}
		}

		private void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
