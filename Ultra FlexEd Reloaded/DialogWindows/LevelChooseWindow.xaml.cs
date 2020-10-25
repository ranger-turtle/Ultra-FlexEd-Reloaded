using LevelSetData;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	/// <summary>
	/// Interaction logic for LevelChooseWindow.xaml
	/// </summary>
	public partial class LevelChooseWindow : Window
	{
		public static readonly DependencyProperty ChooseLevelListBoxItemsProperty =
			DependencyProperty.Register("ChooseLevelListBoxItems", typeof(List<ListBoxItem>), typeof(LevelChooseWindow));

		public List<ListBoxItem> ChooseLevelListBoxItems
		{
			get => GetValue(ChooseLevelListBoxItemsProperty) as List<ListBoxItem>;
			set => SetValue(ChooseLevelListBoxItemsProperty, value);
		}

		private Level[] _levels;
		public Level ChosenLevel { get; private set; }

		public LevelChooseWindow(Level[] levels, string levelSetName)
		{
			InitializeComponent();
			ChooseLevelListBoxItems = new List<ListBoxItem>();
			for (int i = 0; i < levels.Length; i++)
			{
				ChooseLevelListBoxItems.Add(new ListBoxItem() { Content = levels[i].LevelProperties.Name != string.Empty ? levels[i].LevelProperties.Name : $"Level {i + 1}" });
			}
			_levels = levels;
			LevelListBox.SelectedIndex = 0;
			Title = $"Choose level from level set {levelSetName}";
		}

		private void Ok_Clicked(object sender, RoutedEventArgs e)
		{
			ChosenLevel = _levels[LevelListBox.SelectedIndex];
			DialogResult = true;
		}

		private void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
