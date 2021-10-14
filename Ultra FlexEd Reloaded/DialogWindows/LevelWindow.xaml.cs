//#define LEVELFIX
using LevelSetData;
using System.IO;
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
				fileTypeToChooseExtension: ".ogg",
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

		private void SetLeftWall()
		{
			FileChooseAndImportWindow leftWallChooseWindow = new FileChooseAndImportWindow(
				fileFolder: "Walls",
				fileTypeToChooseExtension: ".png",
				fileTypeToChooseName: "left wall texture file")
			{
				Owner = this
			};
			if (leftWallChooseWindow.ShowDialog() == true)
			{
				(DataContext as LevelProperties).LeftWallName = leftWallChooseWindow.ChosenFileName;
				LeftWallTextBox.Text = leftWallChooseWindow.ChosenFileName;
			}
		}

		private void SetRightWall()
		{
			FileChooseAndImportWindow rightWallChooseWindow = new FileChooseAndImportWindow(
				fileFolder: "Walls",
				fileTypeToChooseExtension: ".png",
				fileTypeToChooseName: "right wall texture file")
			{
				Owner = this
			};
			if (rightWallChooseWindow.ShowDialog() == true)
			{
				(DataContext as LevelProperties).RightWallName = rightWallChooseWindow.ChosenFileName;
				RightWallTextBox.Text = rightWallChooseWindow.ChosenFileName;
			}
		}

		private void SetAvatar()
		{
			FileChooseAndImportWindow avatarChooseWindow = new FileChooseAndImportWindow(
				fileFolder: "Avatars",
				fileTypeToChooseExtension: ".png",
				fileTypeToChooseName: "avatar file")
			{
				Owner = this
			};
			if (avatarChooseWindow.ShowDialog() == true)
			{
				(DataContext as LevelProperties).CharacterName = avatarChooseWindow.ChosenFileName;
				string fullFilePath = $"{LevelSetManagement.LevelSetManager.GetInstance().LevelSetResourceDirectory}/Avatars/{avatarChooseWindow.ChosenFileName}.png";
				System.Drawing.Image avatar = System.Drawing.Image.FromFile(fullFilePath);
				if (avatar.Width == avatar.Height)
					AvatarTextBox.Text = avatarChooseWindow.ChosenFileName;
				else
					MessageBox.Show("Avatar image must have equal dimensions.", "Not valid avatar", MessageBoxButton.OK, MessageBoxImage.Warning);
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

		private void AvatarButton_Click(object sender, RoutedEventArgs e)
		{
			SetAvatar();
		}

		private void LeftWallButton_Click(object sender, RoutedEventArgs e)
		{
			SetLeftWall();
		}

		private void RightWallButton_Click(object sender, RoutedEventArgs e)
		{
			SetRightWall();
		}


		public void Ok_Clicked(object sender, RoutedEventArgs e) => DialogResult = true;

		public void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
