using LevelSetData;
using LevelSetManagement;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ultra_FlexEd_Reloaded.DialogWindows;

namespace Ultra_FlexEd_Reloaded
{
	public partial class MainWindow : Window
	{
		public static readonly RoutedCommand StartFlexBallCommand = new RoutedCommand("StartFlexBallCommand", typeof(MenuItem), new InputGestureCollection { new KeyGesture(Key.T, ModifierKeys.Control) });
		public static readonly RoutedCommand TestLevelCommand = new RoutedCommand("TestLevelCommand", typeof(MenuItem), new InputGestureCollection { new KeyGesture(Key.T, ModifierKeys.Control | ModifierKeys.Shift) });

		private const string FileTypeFilter = "Ultra FlexBall Reloaded Level Sets (.nlev)|*.nlev|Ultra FlexBall 2000 Level Sets (.lev)|*.lev";
		private AppSettings appSettings = AppSettings.LoadSettings();

		private void New_Clicked(object sender, ExecutedRoutedEventArgs e)
		{
			if (levelSetManager.Changed)
			{
				MessageBoxResult result = ShowSaveQuestion();
				if (result != MessageBoxResult.Cancel)
				{
					if (result == MessageBoxResult.OK)
						levelSetManager.SaveFile();
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
			if (levelSetManager.Changed)
			{
				result = ShowSaveQuestion();
				if (result == MessageBoxResult.Yes)
					Save_Clicked(sender, e);
			}
			if (dialogResult == true && result != MessageBoxResult.Cancel)
			{
				try
				{
					levelSetManager.LoadLevelSetFile(openFileDialog.FileName);
					LevelListBoxItems.Clear();
					for (int i = 0; i < levelSetManager.LevelCount; i++)
						LevelListBoxItems.Add(PrepareLevelToListBox(SmallUtilities.GetLevelNameForGUI(levelSetManager.GetLevel(i).LevelProperties.Name, i)));
					BrickListBox.SelectedIndex = 0;
					LevelListBox.SelectedIndex = 0;
					RefreshBoard();
				}
				catch (FileFormatException ffe)
				{
					MessageBox.Show(ffe.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				catch (KeyNotFoundException)//After file with unknown extension open attempt
				{
					MessageBox.Show("Could not recognize extension.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private bool? Save()
		{
			if (levelSetManager.LevelLoaded)
			{
				levelSetManager.SaveFile();
				return true;
			}
			else
				return SaveAs();
		}

		private void Save_Clicked(object sender, ExecutedRoutedEventArgs e) => Save();

		private void SaveAs_Clicked(object sender = null, ExecutedRoutedEventArgs e = null) => SaveAs();

		internal bool? SaveAs() => SaveAs(this);

		private bool? SaveAs(Window owner)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog()
			{
				DefaultExt = "*.nlev",
				Filter = FileTypeFilter
			};
			bool? dialogResult = saveFileDialog.ShowDialog(owner);
			if (dialogResult == true)
			{
				if (Path.GetExtension(saveFileDialog.FileName) == ".lev")
					MessageBox.Show($"This old format does not support custom brick types, music and sounds.{Environment.NewLine}More info in Readme.txt", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				levelSetManager.SaveFile(saveFileDialog.FileName);
			}
			return dialogResult;
		}

		private void CanSave(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = levelSetManager.Changed;

		internal void PromptToAddFile(Window owner, Action setLevelAttribute)
		{
			LevelSetManager levelSetManager = LevelSetManager.GetInstance();
			if (!levelSetManager.LevelLoaded || levelSetManager.CurrentFormatType != FormatType.New)
			{
				MessageBoxResult dialogResult = MessageBox.Show($"To add custom hit brick sound, you need do save your level set in new format first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
				if (SaveAs(owner) == true)
					setLevelAttribute();
			}
			else
				setLevelAttribute();
		}

		private void LevelSetProperties_Clicked(object sender, RoutedEventArgs e)
		{
			LevelSetWindow levelSetWindow = new LevelSetWindow(levelSetManager.CopyCurrentLevelSetProperties());
			bool? confirmed = levelSetWindow.ShowDialog();
			if (confirmed == true)
			{
				levelSetManager.UpdateLevelSet(levelSetWindow.DataContext as LevelSetProperties);
			}
		}

		private void Settings_Clicked(object sender, RoutedEventArgs e)
		{
			SettingsWindow settingsWindow = new SettingsWindow(appSettings);
			bool? confirmed = settingsWindow.ShowDialog();
			if (confirmed == true)
			{
				appSettings = settingsWindow.DataContext as AppSettings;
				appSettings.SaveSettings();
				try
				{
					levelSetManager.SetTesterPaths(appSettings.UltraFlexBall2000Path, appSettings.UltraFlexBallReloadedPath);
				}
				catch (FileNotFoundException ex)
				{
					MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
		}

		private void TestLevel_Clicked(object sender, ExecutedRoutedEventArgs e)
		{
			bool test = true;
			if (!levelSetManager.LevelLoaded && !levelSetManager.Changed)
			{
				MessageBox.Show("This level set is empty. Please add something.", LevelSetManager.MAIN_TITLE, MessageBoxButton.OK, MessageBoxImage.Warning);
				test = false;
			}
			if (levelSetManager.Changed)
			{
				MessageBoxResult result = MessageBox.Show("To test level, you must save your level first. Do you want to save?", LevelSetManager.MAIN_TITLE, MessageBoxButton.YesNo, MessageBoxImage.Warning);
				test = result == MessageBoxResult.Yes ? Save().GetValueOrDefault() : false;
			}
			if (test)
			{
				try
				{
					levelSetManager.CurrentTester.TestLevel(levelSetManager.FilePath, levelSetManager.CurrentLevelIndex);
				}
				catch (Win32Exception w32e)
				{
					MessageBox.Show(w32e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					Directory.SetCurrentDirectory(DIRECTORY);
				}
			}
		}

		private void StartGame_Clicked(object sender, ExecutedRoutedEventArgs e)
		{
			bool test = true;
			if (!levelSetManager.LevelLoaded && !levelSetManager.Changed)
			{
				MessageBox.Show("This level set is empty. Please add something.", LevelSetManager.MAIN_TITLE, MessageBoxButton.OK, MessageBoxImage.Warning);
				test = false;
			}
			if (levelSetManager.Changed)
			{
				MessageBoxResult result = MessageBox.Show("To test level set, you must save your level first. Do you want to save?", LevelSetManager.MAIN_TITLE, MessageBoxButton.YesNo, MessageBoxImage.Warning);
				test = result == MessageBoxResult.Yes ? Save().GetValueOrDefault() : false;
			}
			if (test)
			{
				try
				{
					levelSetManager.CurrentTester.TestLevelSet(levelSetManager.FilePath);
				}
				catch (Win32Exception w32e)
				{
					MessageBox.Show(w32e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					Directory.SetCurrentDirectory(DIRECTORY);
				}
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			EraseSelection();
			if (levelSetManager != null)
			{
				if (levelSetManager.Changed)
				{
					MessageBoxResult result = ShowSaveQuestion();
					e.Cancel = result == MessageBoxResult.Cancel;
					if (result == MessageBoxResult.Yes)
						e.Cancel = !Save().GetValueOrDefault();
				}
				else
					e.Cancel = false;
			}
			else
				e.Cancel = true;
			base.OnClosing(e);
		}

		private static MessageBoxResult ShowSaveQuestion() =>
			MessageBox.Show("Are you sure to save?", LevelSetManager.MAIN_TITLE, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

		private void Exit(object sender, RoutedEventArgs e) => Close();
	}
}
