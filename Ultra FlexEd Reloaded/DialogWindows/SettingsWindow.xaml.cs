using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public SettingsWindow(AppSettings appSettings)
		{
			InitializeComponent();
			DataContext = appSettings.Clone();
		}

		private bool IsPathCorrect(string path) => File.Exists(path) && Path.GetExtension(path) == ".exe";

		private void TextChanged(TextBox textBox, Label label, string gamePath)
		{
			label.Content = !IsPathCorrect(textBox.Text)
				? $"Path does not lead to correct {gamePath} executable."
				: string.Empty;
		}

		private void UFB2000PathChanged(object sender, TextChangedEventArgs e) => TextChanged(UFB2000PathTextBox, Path1ErrorText, "Ultra FlexBall 2000");

		private void UFBReloadedPathChanged(object sender, TextChangedEventArgs e) => TextChanged(UFBReloadedPathTextBox, Path2ErrorText, "Ultra FlexBall Reloaded");

		private string GetPath(string flexBallVersionName)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog()
			{
				Filter = $"Ultra FlexBall {flexBallVersionName} executable (.exe)|*.exe",
				DefaultExt = "*.exe"
			};
			return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
		}

		private void BrowseForOldFlexBallPath_Clicked(object sender, RoutedEventArgs e)
		{
			string path = GetPath("2000");
			if (path != null)
				UFB2000PathTextBox.Text = (DataContext as AppSettings).UltraFlexBall2000Path = path;
		}

		private void BrowseForNewFlexBallPath_Clicked(object sender, RoutedEventArgs e)
		{
			string path = GetPath("Reloaded");
			if (path != null)
				UFBReloadedPathTextBox.Text = (DataContext as AppSettings).UltraFlexBallReloadedPath = path;
		}

		private void OldFlexBallSetDefaultPath_Clicked(object sender, RoutedEventArgs e) =>
			UFB2000PathTextBox.Text = (DataContext as AppSettings).UltraFlexBall2000Path = AppSettings.UFB2000_DEFAULT_PATH;

		private void NewFlexBallSetDefaultPath_Clicked(object sender, RoutedEventArgs e) =>
			UFBReloadedPathTextBox.Text = (DataContext as AppSettings).UltraFlexBallReloadedPath = AppSettings.UFBR_DEFAULT_PATH;

		private void Ok_Clicked(object sender, RoutedEventArgs e) => DialogResult = true;

		private void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
