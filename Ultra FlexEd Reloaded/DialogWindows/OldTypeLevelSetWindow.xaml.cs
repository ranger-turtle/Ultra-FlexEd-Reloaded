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
	/// Interaction logic for OldTypeLevelWindow.xaml
	/// </summary>
	public partial class OldTypeLevelSetWindow : Window
	{
		public OldTypeLevelSetWindow(LevelSetProperties levelSetProperties)
		{
			InitializeComponent();
			DataContext = levelSetProperties;
		}

		public void OK_Clicked(object sender, RoutedEventArgs e) => DialogResult = true;

		public void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
