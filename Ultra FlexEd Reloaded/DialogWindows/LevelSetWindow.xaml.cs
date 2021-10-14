using LevelSetData;
using System.Windows;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	/// <summary>
	/// Interaction logic for LevelSetWindow.xaml
	/// </summary>
	public partial class LevelSetWindow : Window
	{
		public LevelSetWindow(LevelSetProperties levelSetProperties)
		{
			InitializeComponent();
			DataContext = levelSetProperties;
			SoundSection.Owner = this;
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
				fileTypeToChooseExtension: ".ogg",
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

		private void SetDefaultLeftWall()
		{
			FileChooseAndImportWindow leftWallChooseWindow = new FileChooseAndImportWindow(
				fileFolder: "Walls",
				fileTypeToChooseExtension: ".png",
				fileTypeToChooseName: "default left wall texture file")
			{
				Owner = this
			};
			if (leftWallChooseWindow.ShowDialog() == true)
			{
				(DataContext as LevelSetProperties).DefaultLeftWallName = leftWallChooseWindow.ChosenFileName;
				DefaultLeftTextBox.Text = leftWallChooseWindow.ChosenFileName;
			}
		}

		private void SetDefaultRightWall()
		{
			FileChooseAndImportWindow rightWallChooseWindow = new FileChooseAndImportWindow(
				fileFolder: "Walls",
				fileTypeToChooseExtension: ".png",
				fileTypeToChooseName: "default right wall texture file")
			{
				Owner = this
			};
			if (rightWallChooseWindow.ShowDialog() == true)
			{
				(DataContext as LevelSetProperties).DefaultRightWallName = rightWallChooseWindow.ChosenFileName;
				DefaultRightTextBox.Text = rightWallChooseWindow.ChosenFileName;
			}
		}

		private void EditCutscene(string cutsceneName)
		{
			MainWindow mainWindow = Owner as MainWindow;
			mainWindow.PromptToAddFile(this, () =>
			{
				CutsceneWindow cutsceneWindow = new CutsceneWindow(cutsceneName);
				if (cutsceneWindow.ShowDialog() == true)
				{
					MessageBox.Show($"{cutsceneName} has been successfully changed.", "Cutscene edit", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			});
		}

		private void EditIntro_Clicked(object sender, RoutedEventArgs e)
		{
			EditCutscene("Intro");
		}

		private void EditOutro_Clicked(object sender, RoutedEventArgs e)
		{
			EditCutscene("Outro");
		}

		private void DefaultBackgroundButton_Click(object sender, RoutedEventArgs e)
		{
			SetDefaultBackground();
		}

		private void DefaultMusicButton_Click(object sender, RoutedEventArgs e)
		{
			SetDefaultMusic();
		}

		private void DefaultLeftWallButton_Click(object sender, RoutedEventArgs e)
		{
			SetDefaultLeftWall();
		}

		private void DefaultRightWallButton_Click(object sender, RoutedEventArgs e)
		{
			SetDefaultRightWall();
		}

		public void OK_Clicked(object sender, RoutedEventArgs e) => DialogResult = true;

		public void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
