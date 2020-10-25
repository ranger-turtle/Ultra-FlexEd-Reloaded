//#define LEVELFIX
using LevelSetData;
using LevelSetManagement;
using System.Windows;


namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	/// <summary>
	/// Interaction logic for LevelWindow.xaml
	/// </summary>
	public partial class LevelWindow : Window
	{
		public LevelWindow()
		{
			InitializeComponent();
			DataContext = new LevelProperties();
			SoundSection.Owner = this;
		}

		public LevelWindow(LevelProperties levelProperties) : this()
		{
#if LEVELFIX
			levelProperties.SoundLibrary = new SoundLibrary("<level-set-default>");
#endif
			DataContext = levelProperties;
			Title = "Edit Level";
		}

		private void SetBackground()
		{
			FileChooseAndImportWindow backgroundChooseWindow = new FileChooseAndImportWindow(
				fileFolder: "Backgrounds",
				fileTypeToChooseExtension: ".png",
				fileTypeToChooseName: "background file",
				includeGameDefault: true,
				includeLevelSetDefault: true)
			{
				Owner = this
			};
			if (backgroundChooseWindow.ShowDialog() == true)
			{
				(DataContext as LevelProperties).BackgroundName = backgroundChooseWindow.ChosenFileName;
				DefaultBackgroundTextBox.Text = backgroundChooseWindow.ChosenFileName;
			}
		}

		private void SetMusic()
		{
			FileChooseAndImportWindow musicChooseWindow = new FileChooseAndImportWindow(
				fileFolder: "Music",
				fileTypeToChooseExtension: ".mp3",
				fileTypeToChooseName: "music file",
				includeGameDefault: true,
				includeLevelSetDefault: true)
			{
				Owner = this
			};
			if (musicChooseWindow.ShowDialog() == true)
			{
				(DataContext as LevelProperties).Music = musicChooseWindow.ChosenFileName;
				MusicTextBox.Text = musicChooseWindow.ChosenFileName;
			}
		}

		private void BackgroundButton_Click(object sender, RoutedEventArgs e)
		{
			SetBackground();
		}

		private void MusicButton_Click(object sender, RoutedEventArgs e)
		{
			SetMusic();
		}


		public void Ok_Clicked(object sender, RoutedEventArgs e) => DialogResult = true;

		public void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
