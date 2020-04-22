using LevelSetData;
using LevelSetManagement;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Ultra_FlexEd_Reloaded.DialogWindows;

namespace Ultra_FlexEd_Reloaded
{
	public partial class MainWindow : Window
	{
		private const string FileTypeFilter = "Ultra FlexBall Reloaded Level Sets (.nlev)|*.nlev|Ultra FlexBall 2000 Level Sets (.lev)|*.lev";

		private void New_Clicked(object sender, ExecutedRoutedEventArgs e)
		{
			if (_levelSetManager.Changed)
			{
				MessageBoxResult result = ShowSaveQuestion();
				if (result != MessageBoxResult.Cancel)
				{
					if (result == MessageBoxResult.OK)
						_levelSetManager.SaveFile();
					Reset();
				}
			}
			else
				Reset();
		}

		private void Open_Clicked(object sender, ExecutedRoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog()
			{
				DefaultExt = "*.nlev",
				Filter = FileTypeFilter
			};
			bool? dialogResult = openFileDialog.ShowDialog(this);
			MessageBoxResult result = MessageBoxResult.Yes;
			if (_levelSetManager.Changed)
			{
				result = ShowSaveQuestion();
				if (result == MessageBoxResult.Yes)
					Save_Clicked(sender, e);
			}
			if (dialogResult == true && result != MessageBoxResult.Cancel)
			{
				_levelSetManager.LoadFile(openFileDialog.FileName);
				LevelListBoxItems.Clear();
				for (int i = 0; i < _levelSetManager.LevelCount; i++)
					LevelListBoxItems.Add(PrepareLevelToListBox(SmallUtilities.GetLevelNameForGUI(_levelSetManager.GetLevel(i).LevelProperties.Name, i)));
				BrickListBox.SelectedIndex = 0;
				LevelListBox.SelectedIndex = 0;
				RefreshBoard();
			}
		}

		private void Save_Clicked(object sender, ExecutedRoutedEventArgs e)
		{
			if (_levelSetManager.LevelLoaded)
			{
				_levelSetManager.SaveFile();
				Title = SmallUtilities.MakeTitle(_levelSetManager.FilePath, false);
			}
			else
				SaveAs_Clicked(sender, e);
		}

		private void SaveAs_Clicked(object sender, ExecutedRoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog()
			{
				DefaultExt = "*.nlev",
				Filter = FileTypeFilter
			};
			if (saveFileDialog.ShowDialog(this) == true)
			{
				if (Path.GetExtension(saveFileDialog.FileName) == ".lev")
					MessageBox.Show($"This old format does not support custom brick types, music and sounds.{Environment.NewLine}More info in Readme.txt", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				_levelSetManager.SaveFile(saveFileDialog.FileName);
				Title = SmallUtilities.MakeTitle(saveFileDialog.FileName, false);
			}
		}

		private void CanSave(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = _levelSetManager.Changed;

		private void LevelSetProperties_Clicked(object sender, RoutedEventArgs e)
		{
			LevelSetWindow levelSetWindow = new LevelSetWindow();
			bool? confirmed = levelSetWindow.ShowDialog();
			if (confirmed == true)
			{
				_levelSetManager.UpdateLevelSet(levelSetWindow.LevelSetName);
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (_levelSetManager.Changed)
			{
				MessageBoxResult result = ShowSaveQuestion();
				e.Cancel = result == MessageBoxResult.Cancel;
				if (!e.Cancel && result == MessageBoxResult.Yes)
					_levelSetManager.SaveFile();
			}
			else
				e.Cancel = false;
			base.OnClosing(e);
		}

		private static MessageBoxResult ShowSaveQuestion() =>
			MessageBox.Show("Are you sure to save?", LevelSetManager.MAIN_TITLE, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

		private void Exit(object sender, RoutedEventArgs e) => Close();
	}
}
