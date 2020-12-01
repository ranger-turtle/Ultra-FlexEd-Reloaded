using LevelSetManagement;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	/// <summary>
	/// Interaction logic for LevelChooseWindow.xaml
	/// </summary>
	public partial class FileChooseAndImportWindow : Window
	{
		public static readonly DependencyProperty ChooseFileListBoxItemsProperty =
			DependencyProperty.Register("ChooseFileListBoxItems", typeof(ObservableCollection<ListBoxItem>), typeof(FileChooseAndImportWindow));

		public ObservableCollection<ListBoxItem> ChooseFileListBoxItems
		{
			get => GetValue(ChooseFileListBoxItemsProperty) as ObservableCollection<ListBoxItem>;
			set => SetValue(ChooseFileListBoxItemsProperty, value);
		}

		private string[] _readFileNames;
		private string fullFileDirectoryPath;
		private string fileTypeName;
		private string fileTypeExtension;
		public string ChosenFileName { get; private set; }

		public FileChooseAndImportWindow(string fileFolder, string fileTypeToChooseExtension, string fileTypeToChooseName, bool includeGameDefault = false, bool includeLevelSetDefault = false, bool includePredefinedSoundsTypes = false)
		{
			InitializeComponent();
			if (LevelSetManager.GetInstance().LevelSetLoaded)
				fullFileDirectoryPath = Path.Combine(LevelSetManager.GetInstance().LevelSetResourceDirectory, fileFolder);
			ChooseFileListBoxItems = new ObservableCollection<ListBoxItem>
			{
				new ListBoxItem() { Content = "<none>" }
			};
			if (includeGameDefault)
				ChooseFileListBoxItems.Add(new ListBoxItem() { Content = "<game-default>" });
			if (includeLevelSetDefault)
				ChooseFileListBoxItems.Add(new ListBoxItem() { Content = "<level-set-default>" });
			if (includePredefinedSoundsTypes)
			{
				ChooseFileListBoxItems.Add(new ListBoxItem() { Content = "<default>" });
				ChooseFileListBoxItems.Add(new ListBoxItem() { Content = "<indestructible>" });
				ChooseFileListBoxItems.Add(new ListBoxItem() { Content = "<plate>" });
				ChooseFileListBoxItems.Add(new ListBoxItem() { Content = "<multi>" });
				ChooseFileListBoxItems.Add(new ListBoxItem() { Content = "<bang>" });
			}
			if (Directory.Exists(fullFileDirectoryPath))
			{
				_readFileNames = Directory.EnumerateFiles(fullFileDirectoryPath, $"*{fileTypeToChooseExtension}").ToArray();
				for (int i = 0; i < _readFileNames.Length; i++)
				{
					ChooseFileListBoxItems.Add(new ListBoxItem() { Content = Path.GetFileNameWithoutExtension(_readFileNames[i]) });
				}
			}
			Title = $"Choose {fileTypeToChooseName}";
			fileTypeName = fileTypeToChooseName;
			fileTypeExtension = fileTypeToChooseExtension;
			FileListBox.SelectedIndex = 0;
		}

		private void Ok_Clicked(object sender, RoutedEventArgs e)
		{
			ChosenFileName = (FileListBox.SelectedItem as ListBoxItem).Content as string;
			DialogResult = true;
		}

		private void Import_Clicked(object sender, RoutedEventArgs e)
		{
			Window probablyMainWindow = Owner.Owner;
			while (!(probablyMainWindow is MainWindow)) probablyMainWindow = probablyMainWindow.Owner;
			MainWindow mainWindow = probablyMainWindow as MainWindow;
			mainWindow.PromptToAddFile(this, ImportFile);
		}

		private void ImportFile()
		{
			if (string.IsNullOrEmpty(fullFileDirectoryPath))
				fullFileDirectoryPath = LevelSetManager.GetInstance().LevelSetResourceDirectory;
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				DefaultExt = fileTypeExtension,
				Filter = $"{fileTypeName} ({fileTypeExtension})|*{fileTypeExtension}",
			};

			if (openFileDialog.ShowDialog(this) == true)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(openFileDialog.SafeFileName);
				string fileName = Path.GetFileName(openFileDialog.SafeFileName);
				ChooseFileListBoxItems.Add(new ListBoxItem() { Content = fileNameWithoutExtension });
				if (!Directory.Exists(fullFileDirectoryPath))
					Directory.CreateDirectory(fullFileDirectoryPath);
				File.Copy(openFileDialog.FileName, Path.Combine(fullFileDirectoryPath, fileName));
			}
		}

		private void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
