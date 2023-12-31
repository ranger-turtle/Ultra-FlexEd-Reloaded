﻿using LevelSetData;
using LevelSetManagement;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ultra_FlexEd_Reloaded.DialogWindows;
using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded
{
	public partial class MainWindow : Window
	{
		public static readonly RoutedCommand StartFlexBallCommand = new RoutedCommand("StartFlexBallCommand", typeof(MenuItem), new InputGestureCollection { new KeyGesture(Key.T, ModifierKeys.Control) });
		public static readonly RoutedCommand TestLevelCommand = new RoutedCommand("TestLevelCommand", typeof(MenuItem), new InputGestureCollection { new KeyGesture(Key.T, ModifierKeys.Control | ModifierKeys.Shift) });

		public static readonly RoutedCommand CheckResourcesCommand = new RoutedCommand("CheckResourcesCommand", typeof(MenuItem), new InputGestureCollection { new KeyGesture(Key.R, ModifierKeys.Control) });

		private const string FileTypeFilter = "Ultra FlexBall Reloaded Level Sets (.nlev)|*.nlev|Ultra FlexBall 2000 Level Sets (.lev)|*.lev";
		private AppSettings appSettings = AppSettings.LoadSettings();

		private void New_Clicked(object sender, ExecutedRoutedEventArgs e)
		{
			if (LevelSetManager.Changed)
			{
				MessageBoxResult result = ShowSaveQuestion();
				if (result != MessageBoxResult.Cancel)
				{
					if (result == MessageBoxResult.OK)
						LevelSetManager.SaveFile();
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
			if (LevelSetManager.Changed)
			{
				result = ShowSaveQuestion();
				if (result == MessageBoxResult.Yes)
					Save_Clicked(sender, e);
			}
			if (dialogResult == true && result != MessageBoxResult.Cancel)
			{
				try
				{
					try
					{
						LevelSetManager.LoadLevelSetFile(openFileDialog.FileName);
					}
					catch (ResourceCheckFailException rcf)
					{
						//MessageBox.Show("Level set contains bricks whose type files are missing. They will be cleared.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						new ResourceCheckErrorMessageBox(rcf.MissingResourceNames).ShowDialog();
					}
					LevelListBoxItems.Clear();
					BrickListBoxItems = new ObservableCollection<ImageListBoxItem>(BrickListBoxItems.TakeWhile(blbi => blbi.BrickId <= LevelSetManager.DEFAULT_BRICK_QUANTITY));
					SortedDictionary<int, string> brickNames = LevelSetManager.GetCustomBrickNames();
					foreach (var name in brickNames)
						AddNewBrickTypeToListBox(name.Key, name.Value);
					for (int i = 0; i < LevelSetManager.LevelCount; i++)
						LevelListBoxItems.Add(PrepareLevelToListBox(SmallUtilities.GetLevelNameForGUI(LevelSetManager.GetLevel(i).LevelProperties.Name, i)));
					BrickListBox.SelectedIndex = 0;
					LevelListBox.SelectedIndex = 0;
					RefreshBoard();
				}
				catch (IOException ioe)
				{
					MessageBox.Show(ioe.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
				catch (KeyNotFoundException)//After file with unknown extension open attempt
				{
					MessageBox.Show("Could not recognize extension.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private bool? Save()
		{
			if (LevelSetManager.LevelSetLoaded)
			{
				LevelSetManager.SaveFile();
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
				LevelSetManager.SaveFile(saveFileDialog.FileName);
			}
			return dialogResult;
		}

		private void CanSave(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = LevelSetManager.Changed;

		internal void PromptToAddFile(Window owner, Action setLevelAttribute)
		{
			LevelSetManager levelSetManager = LevelSetManager.GetInstance();
			if (!levelSetManager.LevelSetLoaded || levelSetManager.CurrentFormatType != FormatType.New)
			{
				MessageBoxResult dialogResult = MessageBox.Show($"To add custom resource, you need do save your level set in new format first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
				if (SaveAs(owner) == true)
					setLevelAttribute();
			}
			else
				setLevelAttribute();
		}

		private void LevelSetProperties_Clicked(object sender, RoutedEventArgs e)
		{
			Window levelSetWindow = LevelEditWindowFactory.GenerateLevelSetWindow(LevelSetManager.CurrentFormatType, LevelSetManager.CopyCurrentLevelSetProperties());
			levelSetWindow.Owner = this;
			bool? confirmed = levelSetWindow.ShowDialog();
			if (confirmed == true)
			{
				LevelSetManager.UpdateLevelSet(levelSetWindow.DataContext as LevelSetProperties);
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
					LevelSetManager.SetTesterPaths(appSettings.UltraFlexBall2000Path, appSettings.UltraFlexBallReloadedPath);
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
			if (!LevelSetManager.LevelSetLoaded && !LevelSetManager.Changed)
			{
				MessageBox.Show("This level set is empty. Please add something.", LevelSetManager.MAIN_TITLE, MessageBoxButton.OK, MessageBoxImage.Warning);
				test = false;
			}
			if (LevelSetManager.Changed)
			{
				MessageBoxResult result = MessageBox.Show("To test level, you must save your level first. Do you want to save?", LevelSetManager.MAIN_TITLE, MessageBoxButton.YesNo, MessageBoxImage.Warning);
				test = result == MessageBoxResult.Yes ? Save().GetValueOrDefault() : false;
			}
			if (test)
			{
				try
				{
					LevelSetManager.CurrentTester.TestLevel(LevelSetManager.FilePath, LevelSetManager.CurrentLevelIndex);
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
			if (!LevelSetManager.LevelSetLoaded && !LevelSetManager.Changed)
			{
				MessageBox.Show("This level set is empty. Please add something.", LevelSetManager.MAIN_TITLE, MessageBoxButton.OK, MessageBoxImage.Warning);
				test = false;
			}
			if (LevelSetManager.Changed)
			{
				MessageBoxResult result = MessageBox.Show("To test level set, you must save your level first. Do you want to save?", LevelSetManager.MAIN_TITLE, MessageBoxButton.YesNo, MessageBoxImage.Warning);
				test = result == MessageBoxResult.Yes ? Save().GetValueOrDefault() : false;
			}
			if (test)
			{
				try
				{
					LevelSetManager.CurrentTester.TestLevelSet(LevelSetManager.FilePath);
				}
				catch (Win32Exception w32e)
				{
					MessageBox.Show(w32e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					Directory.SetCurrentDirectory(DIRECTORY);
				}
			}
		}

		private void CheckLevelSetResources_Clicked(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				LevelSetManager.CheckResources();
				MessageBox.Show("Level set resource check succeeded.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (ResourceCheckFailException rcf)
			{
				new ResourceCheckErrorMessageBox(rcf.MissingResourceNames).ShowDialog();
				foreach (var id in rcf.MissingBrickIds)
					LevelSetManager.ClearBlocksOfType(id);
			}
		}

		private void CanCheckLevelSetResources(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = LevelSetManager.LevelSetLoaded && LevelSetManager.CurrentFormatType == FormatType.New;

		protected override void OnClosing(CancelEventArgs e)
		{
			EraseSelection();
			if (LevelSetManager != null)
			{
				if (LevelSetManager.Changed)
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
