using LevelSetData;
using System.Windows;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	public partial class BrickWindow : Window
	{
		private void SetDetonatorSectionsVisibility(bool visible)
		{
			if (visible)
			{
				DetonatorTypeSection.Visibility = Visibility.Visible;
				DetonatorRangeSection.Visibility = Visibility.Visible;
				DetonatorTriggerSection.Visibility = Visibility.Visible;
				if ((DataContext as BrickProperties).DetonatorType == DetonatorType.Changing)
				{
					OldBrickTypeSection.Visibility = Visibility.Visible;
					NewBrickTypeSection.Visibility = Visibility.Visible;
				}
			}
			else
			{
				DetonatorTypeSection.Visibility = Visibility.Collapsed;
				DetonatorRangeSection.Visibility = Visibility.Collapsed;
				DetonatorTriggerSection.Visibility = Visibility.Collapsed;
				OldBrickTypeSection.Visibility = Visibility.Collapsed;
				NewBrickTypeSection.Visibility = Visibility.Collapsed;
			}
		}

		private void DetonateBrickButton_Clicked(object sender, RoutedEventArgs e)
		{
			ChangeReferenceBrickId((id, imgsrc, imglbl) =>
					   {
						   (DataContext as BrickProperties).DetonateId = id;
						   DetonateBrickImage.Source = imgsrc;
						   DetonateBrickLabel.Content = imglbl;
					   });
			BrickProperties brickProperties = (DataContext as BrickProperties);
			SetDetonatorSectionsVisibility(brickProperties.DetonateId > 0);
		}

		private void OldBrickButton_Clicked(object sender, RoutedEventArgs e)
			=> ChangeReferenceBrickId((id, imgsrc, imglbl) =>
			{
				(DataContext as BrickProperties).OldBrickTypeId = id;
				OldBrickImage.Source = imgsrc;
				OldBrickLabel.Content = imglbl;
			});

		private void NewBrickButton_Clicked(object sender, RoutedEventArgs e)
			=> ChangeReferenceBrickId((id, imgsrc, imglbl) =>
			{
				(DataContext as BrickProperties).NewBrickTypeId = id;
				NewBrickImage.Source = imgsrc;
				NewBrickLabel.Content = imglbl;
			});


		private void SetChangeBrickSectionVisibility(bool visible)
		{
			OldBrickTypeSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
			NewBrickTypeSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
		}

		private void DetonatorTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) =>
			SetChangeBrickSectionVisibility((DetonatorType)e.AddedItems[0] == DetonatorType.Changing);

		private void SetFuseTriggerSectionVisibility(bool visible) =>
			FuseTriggerSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

		private void FuseDirectionComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) =>
			SetFuseTriggerSectionVisibility((Direction)e.AddedItems[0] != Direction.None);


		private void SetBallBreakAnimationTypeSectionVisibility(bool visible)
			=> BallBreakAnimationSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

		private void BallBreakAnimationTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
			=> SetBallBreakAnimationTypeSectionVisibility((BreakAnimationType)e.AddedItems[0] == BreakAnimationType.Custom);

		private void ImportBallBreakAnimationFramesheet(object sender, RoutedEventArgs e)
			=> ImportImage((bitmapImage, fileName) => AddOptionalImageMetadata("ballbreak", BallBreakAnimationSpritesheet, bitmapImage, fileName, CheckImageDimensions));


		private void SetExplosionBreakAnimationTypeSectionVisibility(bool visible)
			=> ExplosionBreakAnimationSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

		private void ExplosionBreakAnimationTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
			=> SetExplosionBreakAnimationTypeSectionVisibility((BreakAnimationType)e.AddedItems[0] == BreakAnimationType.Custom);

		private void ImportExplosionBreakAnimationFramesheet(object sender, RoutedEventArgs e)
			=> ImportImage((bitmapImage, fileName) => AddOptionalImageMetadata("explosionbreak", ExplosionBreakAnimationSpritesheet, bitmapImage, fileName, CheckImageDimensions));


		private void SetBulletBreakAnimationTypeSectionVisibility(bool visible)
			=> BulletBreakAnimationSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

		private void BulletBreakAnimationTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
			=> SetBulletBreakAnimationTypeSectionVisibility((BreakAnimationType)e.AddedItems[0] == BreakAnimationType.Custom);

		private void ImportBulletBreakAnimationFramesheet(object sender, RoutedEventArgs e)
			=> ImportImage((bitmapImage, fileName) => AddOptionalImageMetadata("bulletbreak", BulletBreakAnimationSpritesheet, bitmapImage, fileName, CheckImageDimensions));
	}
}
