using LevelSetData;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Controls;
using Ultra_FlexEd_Reloaded.UserControls;
using System.Linq;
using System.Collections.ObjectModel;
using ColorPickerWPF;
using System.Windows.Markup;
using System.Windows.Data;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	public class ParticleColorToWPFColorConverter : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			BrickProperties.Color chimneyParticleColor = (BrickProperties.Color)value;
			return new SolidColorBrush { Color = new Color { R = chimneyParticleColor.Red, G = chimneyParticleColor.Green, B = chimneyParticleColor.Blue, A = 255 } };
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Color brushColor = (value as SolidColorBrush).Color;
			return new BrickProperties.Color { Red = brushColor.R, Green = brushColor.G, Blue = brushColor.B};
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
			=> this;
	}

	/// <summary>
	/// Interaction logic for BrickWindow.xaml
	/// </summary>
	//TODO Add brick search in selection menu
	//TODO Do normal brick hit frame (for example, in golden plates)
	public partial class BrickWindow : Window
	{
		private class BrickImageEditMetadata
		{
			public BitmapImage MainImage { get; set; }
			public int Frames { get; set; }
			public BitmapImage HitImage { get; set; }
			public BitmapImage BreakAnimationSpritesheet { get; set; }
			public bool Changed { get; internal set; } = true;
		}

		private enum Mode
		{
			ADD, EDIT
		}

		private const string DefaultBrickLabelPlaceholder = "<none>";
		private BrickImageEditMetadata _brickMetadata = null;
		private Mode mode = Mode.ADD;

		private List<BrickMetadata> brickData;

		internal string FrameSheetPath { get; private set; }
		internal string HitImagePath { get; private set; }
		internal string BreakAnimationFrameSheetPath { get; private set; }

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

		internal float[] FrameDurations => (DataContext as BrickProperties).FrameDurations;

		internal BrickWindow(List<BrickMetadata> brickData)
		{
			InitializeComponent();
			DataContext = new BrickProperties();
			FrameSlider.ValueChanged += FrameSlider_ValueChanged;
			//RequiredToCompleteCheckBox.Checked += RequiredToCompleteCheckBox_Checked;
			//NormalResistantCheckBox.Checked += NormalResistantCheckBox_Checked;
			this.brickData = brickData;
			BrickName = "New Brick";
			brickNameField.Focus();
			_brickMetadata = new BrickImageEditMetadata();
		}

		internal BrickWindow(List<BrickMetadata> brickData, BrickProperties brickProperties, string brickName, string levelName) : this(brickData)
		{
			BrickName = brickName;
			DataContext = SerializableCopier.Clone(brickProperties);
			Title = $"Edit Brick {brickName}";
			//TODO Uncomment when you create all fixed brick types var brickFilePaths = Directory.EnumerateFiles($"Custom/{levelName}/{brickName}", "brick*.*");
			var brickFilePath = Path.GetFullPath($"Default Bricks/{brickName}/frames.png");
			BitmapImage bitmapImage = BitmapMethods.GetImageWithCacheOnLoad(new Uri(brickFilePath, UriKind.Relative));
			_brickMetadata.MainImage = bitmapImage;
			_brickMetadata.Frames = EvaluateFrameNumberFromImage(bitmapImage);
			_brickMetadata.Changed = false;
			ReplaceBrickImage(MainFrameBrickImage, _brickMetadata.MainImage, 0, 0);
			InitializeFrameDurations();
			DurationField.Text = FrameDurations[0].ToString("0.##", CultureInfo.InvariantCulture);
			UpdateFrameSliderAfterUncover();
			InitConditionalSections();
			InitReferenceBrickIds();
			UpdateTeleportList();
			mode = Mode.EDIT;
		}

		private void InitConditionalSections()
		{
			BrickProperties brickProperties = DataContext as BrickProperties;
			SetStartFromRandomFrameSectionSectionVisibility(brickProperties.FrameDurations.Length > 1);
			SetExplosionTriggerSectionVisibility(brickProperties.IsExplosive);
			SetPowerUpMeterSectionVisibility(!brickProperties.AlwaysPowerUpYielding);
			SetRequiredHiddenSectionVisibility(brickProperties.Hidden);
			SetBreakAnimationTypeSectionVisibility(brickProperties.BreakAnimationType == BreakAnimationType.Custom);
			SetChimneyLikeBrickSectionsVisibility(brickProperties.IsChimneyLike);
			SetHittingBottomSectionVisibility(brickProperties.IsDescending);
			SetDetonatorSectionsVisibility(brickProperties.IsDetonator);
			SetChangeBrickSectionVisibility(brickProperties.DetonatorType == DetonatorType.Changing);
			SetFuseTriggerSectionVisibility(brickProperties.FuseDirection != Direction.None);
		}

		public void UpdateTeleportList()
		{
			int[] teleportExitIds = (DataContext as BrickProperties).TeleportOutputs;
			if (teleportExitIds != null)
			{
				TeleportListItems = new ObservableCollection<ImageListBoxItem>(teleportExitIds?.Select(id =>
				{
					BrickMetadata teleportExitMetadata = brickData.Find(bd => bd.BrickId == id);
					return new ImageListBoxItem(teleportExitMetadata.BrickId, teleportExitMetadata.ImageSource, teleportExitMetadata.ImageContent);
				}));
			}
			//foreach (int teleportExitId in teleportExitIds)
			//{
			//	BrickMetadata teleportExitMetadata = brickData.Find(bd => bd.BrickId == teleportExitId);
			//	TeleportEntries.Items.Add();
			//}
		}

		private void ReplaceBrickImage(Image imageToChange, BitmapImage framesheet, int x, int y)
		{
			CroppedBitmap croppedBitmap = new CroppedBitmap(framesheet, new Int32Rect(x * BrickProperties.PIXEL_WIDTH, y * BrickProperties.PIXEL_HEIGHT, BrickProperties.PIXEL_WIDTH, BrickProperties.PIXEL_HEIGHT));
			imageToChange.Source = croppedBitmap;
		}

		//private void FrameSheetSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		//{
		//	if (_brickMetadata != null)
		//	{
		//		ReplaceBrickImage(_brickMetadata.Image, 0, 0);
		//		bool shouldEnableFrameSlider = _brickMetadata.Frames > 1;
		//		//EnableFrameSliderWhenTrue();
		//		if (shouldEnableFrameSlider)
		//			FrameSlider.Maximum = _brickMetadata.Frames;
		//	}
		//	else
		//		FrameBrickImage.Source = null;
		//}

		private void FrameSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			TwoIndices spriteIndices = TwoIndices.EvaluateIndices(FrameNum - 1, _brickMetadata.MainImage.PixelWidth / BrickProperties.PIXEL_WIDTH);
			ReplaceBrickImage(MainFrameBrickImage, _brickMetadata.MainImage, spriteIndices.X, spriteIndices.Y);
			DurationField.Text = FrameDurations[FrameNum - 1].ToString("0.##", CultureInfo.InvariantCulture);
		}

		private void DurationField_LostFocus(object sender, RoutedEventArgs e)
		{
			bool success = float.TryParse(DurationField.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
			FrameDurations[FrameNum - 1] = success ? value : 0;
		}

		private void UpdateFrameSliderAfterUncover()
		{
			SetStartFromRandomFrameSectionSectionVisibility(_brickMetadata.Frames > 1);
			if (FrameSection.Visibility == Visibility.Visible)
			{
				FrameSlider.Maximum = _brickMetadata.Frames;
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

		private void CheckIfImageIsSingle(BitmapImage bitmapImage, string fileName)
		{
			CheckImageDimensions(bitmapImage, fileName);
			float rightHitImageRatio = (float)bitmapImage.PixelWidth / bitmapImage.PixelHeight;
			float readHitImageRatio = (float)BrickProperties.PIXEL_WIDTH / BrickProperties.PIXEL_HEIGHT;
			if (rightHitImageRatio != readHitImageRatio)
				throw new BadImageFormatException($"Hit brick image must contain exactly one frame.", fileName);
		}

		private void ImportMainBrickFramesheet(object sender, RoutedEventArgs e)
			=> ImportImage((bitmapImage, fileName) =>
			{
				CheckImageDimensions(bitmapImage, fileName);
				_brickMetadata.MainImage = bitmapImage;
				_brickMetadata.Frames = EvaluateFrameNumberFromImage(bitmapImage);
				_brickMetadata.Changed = true;
				InitializeFrameDurations(true);
				ReplaceBrickImage(MainFrameBrickImage, bitmapImage, 0, 0);
				UpdateFrameSliderAfterUncover();
			});

		private void ImportHitBrickImage(object sender, RoutedEventArgs e)
			=> ImportImage((bitmapImage, fileName) =>
			{
				CheckIfImageIsSingle(bitmapImage, fileName);
				_brickMetadata.HitImage = bitmapImage;
				_brickMetadata.Frames = EvaluateFrameNumberFromImage(bitmapImage);
				_brickMetadata.Changed = true;
				ReplaceBrickImage(HitSpriteImage, bitmapImage, 0, 0);
			});

		private void ImportBreakAnimationFramesheet(object sender, RoutedEventArgs e)
			=> ImportImage((bitmapImage, fileName) =>
			{
				CheckImageDimensions(bitmapImage, fileName);
				_brickMetadata.BreakAnimationSpritesheet = bitmapImage;
				_brickMetadata.Changed = true;
				ReplaceBrickImage(BreakAnimationSpritesheet, bitmapImage, 0, 0);
			});

		private void ImportImage(Action<BitmapImage, string> actionAfterFileOpen)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				DefaultExt = ".png",
				Filter = "PNG images (.png)|*.png"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				BitmapImage bitmapImage = BitmapMethods.GetImageWithCacheOnLoad(new Uri(openFileDialog.FileName));
				try
				{
					actionAfterFileOpen(bitmapImage, openFileDialog.FileName);
				}
				catch (BadImageFormatException ex)
				{
					MessageBox.Show($"Error occured at opening {ex.FileName}: \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private int EvaluateFrameNumberFromImage(BitmapImage bitmapImage) =>
			(bitmapImage.PixelWidth / BrickProperties.PIXEL_WIDTH) * (bitmapImage.PixelHeight / BrickProperties.PIXEL_HEIGHT);

		private void InitReferenceBrickIds()
		{
			BrickProperties brickProperties = DataContext as BrickProperties;
			if (brickProperties.NextBrickId > 0)
			{
				BrickMetadata NextBrickItem = brickData.Find(nbi => nbi.BrickId == brickProperties.NextBrickId);
				NextBrickImage.Source = NextBrickItem.ImageSource;
				NextBrickLabel.Content = NextBrickItem.ImageContent;
			}
			if (brickProperties.DescendingBottomTurnId > 0)
			{
				BrickMetadata DescendingBottomTurnBrickItem = brickData.Find(nbi => nbi.BrickId == brickProperties.DescendingBottomTurnId);
				DescendingBottomImage.Source = DescendingBottomTurnBrickItem.ImageSource;
				DescendingBottomLabel.Content = DescendingBottomTurnBrickItem.ImageContent;
			}
			if (brickProperties.DescendingPressTurnId > 0)
			{
				BrickMetadata DescendingPressTurnBrickItem = brickData.Find(nbi => nbi.BrickId == brickProperties.DescendingPressTurnId);
				DescendingPressImage.Source = DescendingPressTurnBrickItem.ImageSource;
				DescendingPressLabel.Content = DescendingPressTurnBrickItem.ImageContent;
			}
			if (brickProperties.DetonateId > 0)
			{
				BrickMetadata DetonateBrickItem = brickData.Find(nbi => nbi.BrickId == brickProperties.DetonateId);
				DetonateBrickImage.Source = DetonateBrickItem.ImageSource;
				DetonateBrickLabel.Content = DetonateBrickItem.ImageContent;
			}
			if (brickProperties.OldBrickTypeId > 0)
			{
				BrickMetadata OldBrickTypeItem = brickData.Find(nbi => nbi.BrickId == brickProperties.OldBrickTypeId);
				OldBrickImage.Source = OldBrickTypeItem.ImageSource;
				OldBrickLabel.Content = OldBrickTypeItem.ImageContent;
			}
			if (brickProperties.NewBrickTypeId > 0)
			{
				BrickMetadata DetonateBrickItem = brickData.Find(nbi => nbi.BrickId == brickProperties.NewBrickTypeId);
				NewBrickImage.Source = DetonateBrickItem.ImageSource;
				NewBrickLabel.Content = DetonateBrickItem.ImageContent;
			}
		}

		private void ChangeReferenceBrickId(Action<int, ImageSource, string> propertyChangefunction)
		{
			BrickChooseWindow brickChooseWindow = new BrickChooseWindow(brickData);
			if (brickChooseWindow.ShowDialog() == true)
			{
				int chosenId = brickChooseWindow.ChosenId;
				if (chosenId > 0)
					propertyChangefunction(chosenId, brickData[chosenId - 1].ImageSource, brickData[chosenId - 1].ImageContent as string);
				else
					propertyChangefunction(chosenId, null, DefaultBrickLabelPlaceholder);

			}
		}

		private void Ok_Clicked(object sender, RoutedEventArgs e)
		{
			if (_brickMetadata.MainImage != null)
			{
				switch (mode)
				{
					case Mode.ADD:
						FrameSheetPath = _brickMetadata.MainImage.UriSource.OriginalString;
						HitImagePath = _brickMetadata.HitImage.UriSource.OriginalString;
						BreakAnimationFrameSheetPath = _brickMetadata.BreakAnimationSpritesheet.UriSource.OriginalString;
						break;
					case Mode.EDIT:
						FrameSheetPath = _brickMetadata.Changed ? _brickMetadata.MainImage.UriSource.OriginalString : null;
						HitImagePath = _brickMetadata.Changed ? _brickMetadata.HitImage?.UriSource.OriginalString : null;
						BreakAnimationFrameSheetPath = _brickMetadata.Changed ? _brickMetadata.BreakAnimationSpritesheet?.UriSource.OriginalString : null;
						break;
				}
				DialogResult = true;
			}
			else
				MessageBox.Show("You must assign all frame sheets to each brick state.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
		}

		private void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
