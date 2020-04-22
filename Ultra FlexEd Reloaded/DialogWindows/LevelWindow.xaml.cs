using LevelSetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
			DataContext = new Level();
		}

		public LevelWindow(LevelProperties levelProperties) : this()
		{
			DataContext = levelProperties;
			Title = "Edit Level";
		}

		public void Ok_Clicked(object sender, RoutedEventArgs e) => DialogResult = true;

		public void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
