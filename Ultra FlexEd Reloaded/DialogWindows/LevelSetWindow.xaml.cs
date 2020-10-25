using LevelSetData;
using LevelSetManagement;
using System.Windows;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	/// <summary>
	/// Interaction logic for LevelSetWindow.xaml
	/// </summary>
	public partial class LevelSetWindow : Window
	{
		public LevelSetWindow()
		{
			InitializeComponent();
			DataContext = new LevelSetProperties();
		}

		public LevelSetWindow(LevelSetProperties levelSetProperties) : this()
		{
			DataContext = levelSetProperties;
		}

		private void SetDefaultBackground()
		{
			FileChooseAndImportWindow backgroundChooseWindow = new FileChooseAndImportWindow(
				fileFolder: "Backgrounds",
				fileTypeToChooseExtension: ".png",
				fileTypeToChooseName: "default background file",
				includeGameDefault: true)
			{
				Owner = this
			};
			if (backgroundChooseWindow.ShowDialog() == true)
			{
				(DataContext as LevelSetProperties).DefaultBackgroundName = backgroundChooseWindow.ChosenFileName;
				DefaultBackgroundTextBox.Text = backgroundChooseWindow.ChosenFileName;
			}
		}

		private void SetDefaultMusic()
		{
			FileChooseAndImportWindow musicChooseWindow = new FileChooseAndImportWindow(
				fileFolder: "Music",
				fileTypeToChooseExtension: ".mp3",
				fileTypeToChooseName: "default music file",
				includeGameDefault: true)
			{
				Owner = this
			};
			if (musicChooseWindow.ShowDialog() == true)
			{
				(DataContext as LevelSetProperties).DefaultMusic = musicChooseWindow.ChosenFileName;
				DefaultMusicTextBox.Text = musicChooseWindow.ChosenFileName;
			}
		}

		private void DefaultBackgroundButton_Click(object sender, RoutedEventArgs e)
		{
			SetDefaultBackground();
		}

		private void DefaultMusicButton_Click(object sender, RoutedEventArgs e)
		{
			SetDefaultMusic();
		}

		public void OK_Clicked(object sender, RoutedEventArgs e) => DialogResult = true;

		public void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
