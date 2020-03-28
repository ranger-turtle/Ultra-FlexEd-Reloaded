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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ultra_FlexEd_Reloaded.UserControls
{
	/// <summary>
	/// Interaction logic for ImageListBoxItem.xaml
	/// </summary>
	public partial class ImageListBoxItem : ListBoxItem
	{
		public ImageListBoxItem()
		{
			InitializeComponent();
			Height = 19;
			Padding = new Thickness(2, 0, 0, 0);
		}
	}
}
