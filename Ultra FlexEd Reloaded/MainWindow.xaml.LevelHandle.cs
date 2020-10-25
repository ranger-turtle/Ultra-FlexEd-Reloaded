using LevelSetData;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ultra_FlexEd_Reloaded.DialogWindows;

namespace Ultra_FlexEd_Reloaded
{
	public partial class MainWindow : Window
	{
		//TODO try adding level options to menu bar
		public static readonly RoutedCommand MoveLevelUpCommand = new RoutedCommand("MoveLevelUpCommand", typeof(MenuItem), new InputGestureCollection { new KeyGesture(Key.Up, ModifierKeys.Alt) });
		public static readonly RoutedCommand MoveLevelDownCommand = new RoutedCommand("MoveLevelDownCommand", typeof(MenuItem), new InputGestureCollection { new KeyGesture(Key.Down, ModifierKeys.Alt) });
		public static readonly RoutedCommand RemoveLevelCommand = new RoutedCommand("RemoveLevelCommand", typeof(MenuItem));

		private void ShowSuccessMessage(string name)
		{
			MessageBox.Show($@"Level ""{name}"" added successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		private void AddLevel_Clicked(object sender, RoutedEventArgs e)
		{
			AddLevel();
		}

		private void AddLevel()
		{
			CreateOrImportLevelWindow createOrImportLevelWindow = new CreateOrImportLevelWindow(levelSetManager)
			{
				Owner = this
			};
			bool? result = createOrImportLevelWindow.ShowDialog();
			if (result == true)
			{
				Level level = createOrImportLevelWindow.Level;
				levelSetManager.AddLevel(level);
				LevelListBoxItems.Add(PrepareLevelToListBox(level.LevelProperties.Name));
				ShowSuccessMessage(level.LevelProperties.Name);
			}
		}

		private void InsertBeforeLevel_Clicked(object sender, RoutedEventArgs e)
		{
			ExecuteMenuCommandFromListItem<ListBoxItem>(sender, InsertBeforeLevel);
		}

		private void InsertBeforeLevel(ListBoxItem levelListBoxItem)
		{
			int index = LevelListBoxItems.IndexOf(levelListBoxItem);
			InsertLevel(index);
			levelSetManager.CurrentLevelIndex++;
		}

		private void InsertAfterLevel_Clicked(object sender, RoutedEventArgs e)
		{
			ExecuteMenuCommandFromListItem<ListBoxItem>(sender, InsertAfterLevel);
		}

		private void InsertAfterLevel(ListBoxItem levelListBoxItem)
		{
			int index = LevelListBoxItems.IndexOf(levelListBoxItem);
			if (index != LevelListBoxItems.Count - 1)
				InsertLevel(index + 1);
			else
				AddLevel();
		}

		private void InsertLevel(int index)
		{
			CreateOrImportLevelWindow createOrImportLevelWindow = new CreateOrImportLevelWindow(levelSetManager)
			{
				Owner = this
			};
			bool? result = createOrImportLevelWindow.ShowDialog();
			if (result == true)
			{
				Level level = createOrImportLevelWindow.Level;
				levelSetManager.InsertLevel(index, level);
				LevelListBoxItems.Insert(index, PrepareLevelToListBox(level.LevelProperties.Name));
				ShowSuccessMessage(level.LevelProperties.Name);
			}
		}

		private void EditLevel_Clicked(object sender, RoutedEventArgs e)
		{
			ExecuteMenuCommandFromListItem<ListBoxItem>(sender, EditLevel);
		}

		private void EditLevel(ListBoxItem listBoxItem)
		{
			int index = LevelListBoxItems.IndexOf(listBoxItem);
			LevelWindow levelWindow = new LevelWindow(levelSetManager.CopyCurrentLevelProperties())
			{
				Owner = this
			};
			if (levelWindow.ShowDialog() == true)
			{
				LevelProperties levelProperties = levelWindow.DataContext as LevelProperties;
				levelSetManager.UpdateLevelProperties(index, levelProperties);
				LevelListBoxItems[index].Content = levelProperties.Name;
			}
		}

		private void MoveLevelUp(object sender, ExecutedRoutedEventArgs e)
		{
			int oldIndex = LevelListBox.SelectedIndex;
			int newIndex = LevelListBox.SelectedIndex - 1;
			LevelListBoxItems.Move(oldIndex, newIndex);
			levelSetManager.MoveLevel(oldIndex, newIndex);
		}

		private void MoveLevelDown(object sender, ExecutedRoutedEventArgs e)
		{
			int oldIndex = LevelListBox.SelectedIndex;
			int newIndex = LevelListBox.SelectedIndex + 1;
			LevelListBoxItems.Move(oldIndex, newIndex);
			levelSetManager.MoveLevel(oldIndex, newIndex);
		}

		private void CanMoveLevelUp(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = LevelListBox.SelectedIndex > 0;

		private void CanMoveLevelDown(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = LevelListBoxItems != null && LevelListBox.SelectedIndex < LevelListBoxItems.Count - 1;

		private void RemoveLevel_Clicked(object sender, ExecutedRoutedEventArgs e)
		{
			ListBoxItem listBoxItem = LevelListBox.SelectedItem as ListBoxItem;
			RemoveLevel(listBoxItem);
		}

		private void RemoveLevel(ListBoxItem levelListBoxItem)
		{
			int index = LevelListBoxItems.IndexOf(levelListBoxItem);
			MessageBoxResult result = MessageBox.Show($"Are you sure to delete level {levelSetManager.CurrentLevelName}?", LevelSetManagement.LevelSetManager.MAIN_TITLE, MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (result == MessageBoxResult.Yes)
			{
				levelSetManager.RemoveLevel(index);
				LevelListBoxItems.RemoveAt(index);
			}
		}

		private void CanRemoveLevel(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = levelSetManager.LevelCount > 1;
	}
}
