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
using System.Windows.Markup;
using System.Windows.Data;
using LevelSetManagement;

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

		public override object ProvideValue(IServiceProvider serviceProvider) => this;
	}

	/// <summary>
	/// Interaction logic for BrickWindow.xaml
	/// </summary>
	//TODO Add brick search in selection menu
	public partial class BrickWindow : Window
	{
		private class BrickImageEditMetadata
		{
			public BitmapImage Image { get; internal set; }
			public int Frames { get; internal set; }
			public bool Changed { get; internal set; } = true;
		}

		private enum Mode
		{
			ADD, EDIT
		}

		private const string DefaultBrickLabelPlaceholder = "<none>";
		private BrickImageEditMetadata mainBrickImageMetadata = null;
		//key is file name
		private Dictionary<string, BrickImageEditMetadata> optionalBrickImageMetadata = null;
		private Mode mode = Mode.ADD;

		private List<BrickMetadata> brickData;

		internal string MainFrameSheetPath { get; private set; }
		internal Dictionary<string, string> OptionalImagePaths { get; private set; }

		internal static readonly DependencyProperty FrameNumProperty =
		  DependencyProperty.Register("FrameNum", typeof(int), typeof(BrickWindow));

		internal int FrameNum
		{
			get => (int)GetValue(FrameNumProperty);
			set => SetValue(FrameNumProperty, value);
		}

		internal float[] FrameDurations => (DataContext as BrickProperties).FrameDurations;

		internal BrickWindow(List<BrickMetadata> brickData)
		{
			InitializeComponent();
			DataContext = new BrickProperties(126);
			FrameSlider.ValueChanged += FrameSlider_ValueChanged;
			//RequiredToCompleteCheckBox.Checked += RequiredToCompleteCheckBox_Checked;
			//NormalResistantCheckBox.Checked += NormalResistantCheckBox_Checked;
			this.brickData = brickData;
			brickNameField.Focus();
			mainBrickImageMetadata = new BrickImageEditMetadata();
			optionalBrickImageMetadata = new Dictionary<string, BrickImageEditMetadata>();
			OptionalImagePaths = new Dictionary<string, string>();
		}

		internal BrickWindow(List<BrickMetadata> brickData, BrickProperties brickProperties, string levelName) : this(brickData)
		{
			DataContext = SerializableCopier.Clone(brickProperties);
			Title = $"Edit Brick {brickProperties.Name}";
			#region Main Brick Metadata load
			string brickFileDirectory = LevelSetManager.GetInstance().GetBrickFolder(brickProperties.Id);
			var brickFilePath = Path.GetFullPath($"{brickFileDirectory}/{brickProperties.Name}/frames.png");
			BitmapImage bitmapImage = BitmapMethods.GetImageWithCacheOnLoad(new Uri(brickFilePath, UriKind.Relative));
			mainBrickImageMetadata.Image = bitmapImage;
			mainBrickImageMetadata.Changed = false;
			mainBrickImageMetadata.Frames = EvaluateFrameNumberFromImage(bitmapImage);
			ReplaceBrickImage(MainFrameBrickImage, mainBrickImageMetadata.Image, 0, 0);
			InitializeFrameDurations();
			#endregion
			InitFramesheetEditMetadata(HitSpriteImage, Path.GetFullPath($"{brickFileDirectory}/{brickProperties.Name}/hit.png"));
			InitFramesheetEditMetadata(BallBreakAnimationSpritesheet, Path.GetFullPath($"{brickFileDirectory}/{brickProperties.Name}/ballbreak.png"));
			InitFramesheetEditMetadata(ExplosionBreakAnimationSpritesheet, Path.GetFullPath($"{brickFileDirectory}/{brickProperties.Name}/explosionbreak.png"));
			InitFramesheetEditMetadata(BulletBreakAnimationSpritesheet, Path.GetFullPath($"{brickFileDirectory}/{brickProperties.Name}/bulletbreak.png"));
			DurationField.Text = FrameDurations[0].ToString("0.##", CultureInfo.InvariantCulture);
			UpdateFrameSliderAfterUncover();
			InitConditionalSections();
			InitReferenceBrickIds();
			UpdateTeleportList();
			mode = Mode.EDIT;
		}

		private void InitFramesheetEditMetadata(Image framesheetImage, string brickFilePath)
		{
			if (File.Exists(brickFilePath))
			{
				BitmapImage bitmapImage = BitmapMethods.GetImageWithCacheOnLoad(new Uri(brickFilePath, UriKind.Relative));
				BrickImageEditMetadata brickImageEditMetadata = new BrickImageEditMetadata()
				{
					Image = bitmapImage,
					Frames = EvaluateFrameNumberFromImage(bitmapImage),
					Changed = false
				};
				optionalBrickImageMetadata.Add(Path.GetFileNameWithoutExtension(brickFilePath), brickImageEditMetadata);
				ReplaceBrickImage(framesheetImage, brickImageEditMetadata.Image, 0, 0);
			}
		}

		private void InitConditionalSections()
		{
			BrickProperties brickProperties = DataContext as BrickProperties;
			SetStartFromRandomFrameSectionSectionVisibility(brickProperties.FrameDurations.Length > 1);
			SetExplosionTriggerSectionVisibility(brickProperties.IsExplosive);
			SetPowerUpMeterSectionVisibility(!brickProperties.AlwaysPowerUpYielding);
			SetRequiredHiddenSectionVisibility(brickProperties.Hidden);
			SetBallBreakAnimationTypeSectionVisibility(brickProperties.BallBreakAnimationType == BreakAnimationType.Custom);
			SetExplosionBreakAnimationTypeSectionVisibility(brickProperties.ExplosionBreakAnimationType == BreakAnimationType.Custom);
			SetBulletBreakAnimationTypeSectionVisibility(brickProperties.BulletBreakAnimationType == BreakAnimationType.Custom);
			SetChimneyLikeBrickSectionsVisibility(brickProperties.IsChimneyLike);
			SetHittingBottomSectionVisibility(brickProperties.IsDescending);
			SetDetonatorSectionsVisibility(brickProperties.IsDetonator);
			SetFuseTriggerSectionVisibility(brickProperties.FuseDirection != Direction.None);
		}

		public void UpdateTeleportList()
		{
			int[] teleportExitIds = (DataContext as BrickProperties).TeleportExits;
			if (teleportExitIds != null)
			{
				TeleportListItems = new ObservableCollection<ImageListBoxItem>(teleportExitIds?.Select(id =>
				{
					BrickMetadata teleportExitMetadata = brickData.Find(bd => bd.BrickId == id);
					return new ImageListBoxItem(teleportExitMetadata.BrickId, teleportExitMetadata.ImageSource, teleportExitMetadata.BrickName);
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
			int singleFrameHeight = EvaluateFrameNumberFromImage(framesheet);
			CroppedBitmap croppedBitmap = new CroppedBitmap(framesheet, new Int32Rect(x * framesheet.PixelWidth, y * (framesheet.PixelHeight / singleFrameHeight), framesheet.PixelWidth, framesheet.PixelHeight / singleFrameHeight));
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
			//TwoIndices spriteIndices = TwoIndices.EvaluateIndices(FrameNum - 1, mainBrickImageMetadata.Image.PixelWidth / BrickProperties.PIXEL_WIDTH);
			int y = FrameNum - 1;
			ReplaceBrickImage(MainFrameBrickImage, mainBrickImageMetadata.Image, 0, y);
			DurationField.Text = FrameDurations[FrameNum - 1].ToString("0.##", CultureInfo.InvariantCulture);
		}

		private void DurationField_LostFocus(object sender, RoutedEventArgs e)
		{
			bool success = float.TryParse(DurationField.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
			FrameDurations[FrameNum - 1] = success ? value : 0;
		}

		private void UpdateFrameSliderAfterUncover()
		{
			SetStartFromRandomFrameSectionSectionVisibility(mainBrickImageMetadata.Frames > 1);
			if (FrameSection.Visibility == Visibility.Visible)
			{
				FrameSlider.Maximum = mainBrickImageMetadata.Frames;
				FrameSlider.Value = 1;
				DurationField.Text = FrameDurations[0].ToString("0.##", CultureInfo.InvariantCulture);
			}
		}

		/**<summary>
		 <para>Checks if width and height of <c><paramref name="bitmapImage"/></c> are multiplies of standard dimensions
		 of brick image declared in BrickProperties.</para>
		 <para>See <see cref="DataContext"/> to check standard brick image dimensions.</para>
		 </summary>*/
		private void CheckImageDimensions(BitmapImage bitmapImage, string fileName)
		{
			if (bitmapImage.PixelWidth % BrickProperties.PIXEL_WIDTH != 0 || bitmapImage.PixelHeight % BrickProperties.PIXEL_HEIGHT != 0)
				throw new BadImageFormatException($"Width is not multiply of {BrickProperties.PIXEL_WIDTH} or height is not multiply of {BrickProperties.PIXEL_HEIGHT}", fileName);
		}

		private void CheckIfImageIsSingle(BitmapImage bitmapImage, string fileName)
		{
			CheckImageDimensions(bitmapImage, fileName);
			float readHitImageRatio = (float)bitmapImage.PixelHeight / bitmapImage.PixelWidth;
			if (BrickProperties.STANDARD_DIMENSION_RATIO != readHitImageRatio)
				throw new BadImageFormatException($"Hit brick image must contain exactly one frame.", fileName);
		}

		private void ImportMainBrickFramesheet_Clicked(object sender, RoutedEventArgs e)
			=> OpenImage(ImportMainBrickFramesheet);

		private void ImportMainBrickFramesheet(BitmapImage bitmapImage, string externalFramesheetPath)
		{
			CheckImageDimensions(bitmapImage, externalFramesheetPath);
			mainBrickImageMetadata.Image = bitmapImage;
			mainBrickImageMetadata.Frames = EvaluateFrameNumberFromImage(bitmapImage);
			mainBrickImageMetadata.Changed = true;
			InitializeFrameDurations();
			ReplaceBrickImage(MainFrameBrickImage, bitmapImage, 0, 0);
			UpdateFrameSliderAfterUncover();
		}

		private void AddOptionalImageMetadata(string outFileName, Image imageToChange, BitmapImage bitmapImage, string externalFileName, Action<BitmapImage, string> checkMethod)
		{
			checkMethod(bitmapImage, externalFileName);
			if (optionalBrickImageMetadata.ContainsKey(outFileName))
			{
				optionalBrickImageMetadata[outFileName].Image = bitmapImage;
				optionalBrickImageMetadata[outFileName].Changed = true;
			}
			else
			{
				BrickImageEditMetadata brickImageEditMetadata = new BrickImageEditMetadata()
				{
					Image = bitmapImage,
					Changed = true
				};
				optionalBrickImageMetadata.Add(outFileName, brickImageEditMetadata);
			}
			ReplaceBrickImage(imageToChange, bitmapImage, 0, 0);
		}

		private void ImportHitBrickImage(object sender, RoutedEventArgs e)
			=> OpenImage((bitmapImage, fileName) => AddOptionalImageMetadata("hit", HitSpriteImage, bitmapImage, fileName, checkMethod: CheckIfImageIsSingle));

		private void OpenImage(Action<BitmapImage, string> actionAfterFileOpen)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				DefaultExt = ".png",
				Filter = "PNG images (.png)|*.png"
			};

			if (openFileDialog.ShowDialog() == true)
				ImportImage(actionAfterFileOpen, openFileDialog.FileName);
		}

		private void ImportImage(Action<BitmapImage, string> actionAfterFileOpen, string openFilename)
		{
			BitmapImage bitmapImage = BitmapMethods.GetImageWithCacheOnLoad(new Uri(openFilename, UriKind.RelativeOrAbsolute));
			try
			{
				actionAfterFileOpen(bitmapImage, openFilename);
			}
			catch (BadImageFormatException ex)
			{
				MessageBox.Show($"Error occured at opening {ex.FileName}: \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void RemoveImage(Image image, string spritesheetName)
		{
			image.Source = null;
			optionalBrickImageMetadata.Remove(spritesheetName);
		}

		private int EvaluateFrameNumberFromImage(BitmapImage bitmapImage) =>
			(int)Math.Floor(bitmapImage.PixelHeight / (bitmapImage.PixelWidth * BrickProperties.STANDARD_DIMENSION_RATIO));

		private void InitReferenceBrickIds()
		{
			BrickProperties brickProperties = DataContext as BrickProperties;
			if (brickProperties.NextBrickTypeId > 0)
			{
				BrickMetadata NextBrickItem = brickData.Find(nbi => nbi.BrickId == brickProperties.NextBrickTypeId);
				NextBrickImage.Source = NextBrickItem.ImageSource;
				NextBrickLabel.Content = NextBrickItem.BrickName;
			}
			if (brickProperties.DescendingBottomTurnId > 0)
			{
				BrickMetadata DescendingBottomTurnBrickItem = brickData.Find(nbi => nbi.BrickId == brickProperties.DescendingBottomTurnId);
				DescendingBottomImage.Source = DescendingBottomTurnBrickItem.ImageSource;
				DescendingBottomLabel.Content = DescendingBottomTurnBrickItem.BrickName;
			}
			if (brickProperties.DescendingPressTurnId > 0)
			{
				BrickMetadata DescendingPressTurnBrickItem = brickData.Find(nbi => nbi.BrickId == brickProperties.DescendingPressTurnId);
				DescendingPressImage.Source = DescendingPressTurnBrickItem.ImageSource;
				DescendingPressLabel.Content = DescendingPressTurnBrickItem.BrickName;
			}
			if (brickProperties.OldBrickTypeId > 0)
			{
				BrickMetadata OldBrickTypeItem = brickData.Find(nbi => nbi.BrickId == brickProperties.OldBrickTypeId);
				OldBrickImage.Source = OldBrickTypeItem.ImageSource;
				OldBrickLabel.Content = OldBrickTypeItem.BrickName;
			}
			if (brickProperties.NewBrickTypeId > 0)
			{
				BrickMetadata DetonateBrickItem = brickData.Find(nbi => nbi.BrickId == brickProperties.NewBrickTypeId);
				NewBrickImage.Source = DetonateBrickItem.ImageSource;
				NewBrickLabel.Content = DetonateBrickItem.BrickName;
			}
		}

		private void ChangeReferenceBrickId(Action<int, ImageSource, string> propertyChangefunction)
		{
			BrickChooseWindow brickChooseWindow = new BrickChooseWindow(brickData);
			if (brickChooseWindow.ShowDialog() == true)
			{
				int chosenId = brickChooseWindow.ChosenId;
				if (chosenId > 0)
					propertyChangefunction(chosenId, brickData[chosenId - 1].ImageSource, brickData[chosenId - 1].BrickName as string);
				else
					propertyChangefunction(chosenId, null, DefaultBrickLabelPlaceholder);

			}
		}

		private void BrickNameField_LostFocus(object sender, RoutedEventArgs e)
		{
			if (brickNameField.Text == string.Empty)
				brickNameField.Text = "New Brick";
		}

		private void CopyLocalBrick_Clicked(object sender, RoutedEventArgs e)
		{
			BrickChooseWindow brickChooseWindow = new BrickChooseWindow(brickData);
			if (brickChooseWindow.ShowDialog() == true)
			{
				DataContext = LevelSetManager.GetInstance().ImportLocalBrick(brickChooseWindow.ChosenId, out string frameSheetPath, out Dictionary<string, string> optionalImagePaths);

				ImportImage(ImportMainBrickFramesheet, frameSheetPath);

				if (optionalImagePaths.ContainsKey("hit"))
					ImportImage((bitmapImage, fileName) => AddOptionalImageMetadata("hit", HitSpriteImage, bitmapImage, fileName, checkMethod: CheckIfImageIsSingle), "hit");
				else
					RemoveImage(HitSpriteImage, "hit");

				if (optionalImagePaths.ContainsKey("ballbreak"))
					ImportImage((bitmapImage, fileName) => AddOptionalImageMetadata("ballbreak", BallBreakAnimationSpritesheet, bitmapImage, fileName, CheckImageDimensions), "ballbreak");
				else
					RemoveImage(HitSpriteImage, "ballbreak");
				if (optionalImagePaths.ContainsKey("explosionbreak"))
					ImportImage((bitmapImage, fileName) => AddOptionalImageMetadata("explosionbreak", ExplosionBreakAnimationSpritesheet, bitmapImage, fileName, CheckImageDimensions), "explosionbreak");
				else
					RemoveImage(HitSpriteImage, "explosionbreak");
				if (optionalImagePaths.ContainsKey("bulletbreak"))
					ImportImage((bitmapImage, fileName) => AddOptionalImageMetadata("bulletbreak", BulletBreakAnimationSpritesheet, bitmapImage, fileName, CheckImageDimensions), "bulletbreak");
				else
					RemoveImage(HitSpriteImage, "bulletbreak");

			}
		}

		private void Ok_Clicked(object sender, RoutedEventArgs e)
		{
			if (mainBrickImageMetadata.Image != null)
			{
				switch (mode)
				{
					case Mode.ADD:
						MainFrameSheetPath = mainBrickImageMetadata.Image.UriSource.OriginalString;
						//HitImagePath = optionalBrickImageMetadata.HitImage.UriSource.OriginalString;
						//BallBreakAnimationFrameSheetPath = optionalBrickImageMetadata.BallBreakAnimationSpritesheet.UriSource.OriginalString;
						break;
					case Mode.EDIT:
						MainFrameSheetPath = mainBrickImageMetadata.Changed ? mainBrickImageMetadata.Image.UriSource.OriginalString : null;
						break;
				}
				foreach (var optionalImageMetadata in optionalBrickImageMetadata)
				{
					if (optionalImageMetadata.Value.Changed)
						OptionalImagePaths.Add(optionalImageMetadata.Key, optionalImageMetadata.Value.Image?.UriSource.OriginalString);
				}
				DialogResult = true;
			}
			else
				MessageBox.Show("You must assign all frame sheets to each brick state.", "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
		}

		private void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
