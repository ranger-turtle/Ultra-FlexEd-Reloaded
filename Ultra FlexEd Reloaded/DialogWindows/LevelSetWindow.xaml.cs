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
	/// Interaction logic for LevelSetWindow.xaml
	/// </summary>
	public partial class LevelSetWindow : Window
	{
		public DependencyProperty LevelSetNameProperty =
			DependencyProperty.Register("LevelSetName", typeof(string), typeof(LevelSetWindow));

		public string LevelSetName
		{
			get => GetValue(LevelSetNameProperty) as string;
			set => SetValue(LevelSetNameProperty, value);
		}

		public LevelSetWindow()
		{
			InitializeComponent();
		}

		public void OK_Clicked(object sender, RoutedEventArgs e) => DialogResult = true;

		public void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
