using LevelSetData;
using System.Windows;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	public partial class BrickWindow : Window
    {
		private void NextBrickButton_Clicked(object sender, RoutedEventArgs e)
			=> ChangeReferenceBrickId((id, imgsrc, imglbl) =>
			{
				(DataContext as BrickProperties).NextBrickTypeId = id;
				NextBrickImage.Source = imgsrc;
				NextBrickLabel.Content = imglbl;
			});

		private void NormalResistantCheckBox_Checked(object sender, RoutedEventArgs e) => RequiredToCompleteCheckBox.IsChecked = false;

		private void RequiredToCompleteCheckBox_Checked(object sender, RoutedEventArgs e) => NormalResistantCheckBox.IsChecked = false;


		private void SetPowerUpMeterSectionVisibility(bool visible)
			=> PowerUpMeterSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

		private void AlwaysYieldPowerUpCheckBox_Checked(object sender, RoutedEventArgs e)
			=> SetPowerUpMeterSectionVisibility(!(DataContext as BrickProperties).AlwaysPowerUpYielding);


		private void SetExplosionTriggerSectionVisibility(bool visible)
			=> ExplosionTriggerSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

		private void ExplosionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
			=> SetExplosionTriggerSectionVisibility(e.NewValue > 0);


		private void SetStartFromRandomFrameSectionSectionVisibility(bool visible)
		{
			FrameSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
			FrameDurationSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
			StartFromRandomFrameSection.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
		}

		private void SetHitSound()
		{
			FileChooseAndImportWindow soundChooseWindow = new FileChooseAndImportWindow("Sounds", ".wav", "brick hit sound file", includePredefinedSoundsTypes: true)
			{
				Owner = this
			};
			if (soundChooseWindow.ShowDialog() == true)
			{
				(DataContext as BrickProperties).HitSoundName = soundChooseWindow.ChosenFileName;
				HitSound.Text = soundChooseWindow.ChosenFileName;
			}
		}

		private void HitSoundButton_Click(object sender, RoutedEventArgs e)
		{
			SetHitSound();
		}
	}
}
