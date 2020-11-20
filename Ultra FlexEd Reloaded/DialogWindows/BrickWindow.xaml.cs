﻿using LevelSetData;
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
	//TODO Do normal brick hit frame (for example, in golden plates)
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
			mainBrickImageMetadata = new BrickImageEditMetadata();
			optionalBrickImageMetadata = new Dictionary<string, BrickImageEditMetadata>();
			OptionalImagePaths = new Dictionary<string, string>();
		}

		internal BrickWindow(List<BrickMetadata> brickData, BrickProperties brickProperties, string brickName, string levelName) : this(brickData)
		{
			BrickName = brickName;
			DataContext = SerializableCopier.Clone(brickProperties);
			Title = $"Edit Brick {brickName}";
			//TODO Uncomment when you create all fixed brick types var brickFilePaths = Directory.EnumerateFiles($"Custom/{levelName}/{brickName}", "brick*.*");
			#region Main Brick Metadata load
			var brickFilePath = Path.GetFullPath($"Default Bricks/{brickName}/frames.png");
			BitmapImage bitmapImage = BitmapMethods.GetImageWithCacheOnLoad(new Uri(brickFilePath, UriKind.Relative));
			mainBrickImageMetadata.Image = bitmapImage;
			mainBrickImageMetadata.Changed = false;
			mainBrickImageMetadata.Frames = EvaluateFrameNumberFromImage(bitmapImage);
			ReplaceBrickImage(MainFrameBrickImage, mainBrickImageMetadata.Image, 0, 0);
			InitializeFrameDurations();
			#endregion
			InitFramesheetEditMetadata(HitSpriteImage, Path.GetFullPath($"Default Bricks/{brickName}/hit.png"));
			InitFramesheetEditMetadata(BallBreakAnimationSpritesheet, Path.GetFullPath($"Default Bricks/{brickName}/ballbreak.png"));
			InitFramesheetEditMetadata(ExplosionBreakAnimationSpritesheet, Path.GetFullPath($"Default Bricks/{brickName}/explosionbreak.png"));
			InitFramesheetEditMetadata(BulletBreakAnimationSpritesheet, Path.GetFullPath($"Default Bricks/{brickName}/bulletbreak.png"));
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
			SetChangeBrickSectionVisibility(brickProperties.DetonatorType == DetonatorType.Changing);
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
			TwoIndices spriteIndices = TwoIndices.EvaluateIndices(FrameNum - 1, mainBrickImageMetadata.Image.PixelWidth / BrickProperties.PIXEL_WIDTH);
			ReplaceBrickImage(MainFrameBrickImage, mainBrickImageMetadata.Image, spriteIndices.X, spriteIndices.Y);
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
			=> ImportImage((bitmapImage, externalFileName) =>
			{
				CheckImageDimensions(bitmapImage, externalFileName);
				mainBrickImageMetadata.Image = bitmapImage;
				mainBrickImageMetadata.Frames = EvaluateFrameNumberFromImage(bitmapImage);
				mainBrickImageMetadata.Changed = true;
				InitializeFrameDurations();
				ReplaceBrickImage(MainFrameBrickImage, bitmapImage, 0, 0);
				UpdateFrameSliderAfterUncover();
			});

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
			=> ImportImage((bitmapImage, fileName) => AddOptionalImageMetadata("hit", HitSpriteImage, bitmapImage, fileName, checkMethod: CheckIfImageIsSingle));

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
			if (brickProperties.DetonateId > 0)
			{
				BrickMetadata DetonateBrickItem = brickData.Find(nbi => nbi.BrickId == brickProperties.DetonateId);
				DetonateBrickImage.Source = DetonateBrickItem.ImageSource;
				DetonateBrickLabel.Content = DetonateBrickItem.BrickName;
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
				MessageBox.Show("You must assign all frame sheets to each brick state.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
		}

		private void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
