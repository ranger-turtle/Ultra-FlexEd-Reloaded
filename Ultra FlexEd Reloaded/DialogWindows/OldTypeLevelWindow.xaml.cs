using LevelSetData;
using System.Windows;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	/// <summary>
	/// Interaction logic for OldTypeLevelWindow.xaml
	/// </summary>
	public partial class OldTypeLevelWindow : Window
	{
		public OldTypeLevelWindow()
		{
			InitializeComponent();
			DataContext = new LevelProperties();
		}

		public OldTypeLevelWindow(LevelProperties levelProperties) : this()
		{
			DataContext = levelProperties;
		}

		public void OK_Clicked(object sender, RoutedEventArgs e) => DialogResult = true;

		public void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
