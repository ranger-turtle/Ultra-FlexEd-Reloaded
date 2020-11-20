using ColorPickerWPF;
using LevelSetData;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	public partial class BrickWindow : Window
    {
		public ObservableCollection<ImageListBoxItem> TeleportListItems
		{
			get { return (ObservableCollection<ImageListBoxItem>)GetValue(TeleportListItemsProperty); }
			set { SetValue(TeleportListItemsProperty, value); }
		}

		// Using a DependencyProperty as the backing store for TeleportListItems.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TeleportListItemsProperty =
			DependencyProperty.Register("TeleportListItems", typeof(ObservableCollection<ImageListBoxItem>), typeof(BrickWindow));


		private void EditTeleportExitsButton_Click(object sender, RoutedEventArgs e)
		{
			BrickProperties brickProperties = (DataContext as BrickProperties);
			MultipleBrickChooseWindow multipleBrickChooseWindow = new MultipleBrickChooseWindow(brickData, brickProperties.TeleportExits)
			{
				Title = "Choose Teleport Exits",
				Owner = this
			};
			if (multipleBrickChooseWindow.ShowDialog() == true)
			{
				brickProperties.TeleportExits = multipleBrickChooseWindow.ChosenIds;
				UpdateTeleportList();
			}
		}

		private void DescendingBottomButton_Clicked(object sender, RoutedEventArgs e)
			=> ChangeReferenceBrickId((id, imgsrc, imglbl) =>
			{
				(DataContext as BrickProperties).DescendingBottomTurnId = id;
				DescendingBottomImage.Source = imgsrc;
				DescendingBottomLabel.Content = imglbl;
			});

		private void DescendingPressButton_Clicked(object sender, RoutedEventArgs e)
			=> ChangeReferenceBrickId((id, imgsrc, imglbl) =>
			{
				(DataContext as BrickProperties).DescendingPressTurnId = id;
				DescendingPressImage.Source = imgsrc;
				DescendingPressLabel.Content = imglbl;
			});


		private void SetRequiredHiddenSectionVisibility(bool visible) =>
			RequiredHiddenSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

		private void HiddenCheckBox_Checked(object sender, RoutedEventArgs e) =>
			SetRequiredHiddenSectionVisibility((DataContext as BrickProperties).Hidden);


		private void SetChimneyLikeBrickSectionsVisibility(bool visible)
		{
			ParticleXSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
			ParticleYSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
			ParticleColorSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
		}

		private void ChimneyLikeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) =>
			SetChimneyLikeBrickSectionsVisibility((DataContext as BrickProperties).IsChimneyLike);


		private void ChangeColor(Action<Color> colorChange)
		{
			if (ColorPickerWindow.ShowDialog(out Color color) == true)
			{
				colorChange(color);
			}
		}

		private void PickColor1Button_Click(object sender, RoutedEventArgs e)
			=> ChangeColor(color => Color1View.Background = new SolidColorBrush { Color = color } );

		private void PickColor2Button_Click(object sender, RoutedEventArgs e)
			=> ChangeColor(color => Color2View.Background = new SolidColorBrush { Color = color });
	

		private void SetHittingBottomSectionVisibility(bool visible) =>
			HittingBottomSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

		private void DescendingCheckBox_Checked(object sender, RoutedEventArgs e) =>
			SetHittingBottomSectionVisibility((DataContext as BrickProperties).IsDescending);
    }
}
