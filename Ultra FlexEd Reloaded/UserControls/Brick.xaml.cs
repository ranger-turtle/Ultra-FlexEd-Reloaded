using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ultra_FlexEd_Reloaded.UserControls
{
	/// <summary>
	/// Interaction logic for Brick.xaml
	/// </summary>
	public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType,
							  object parameter, CultureInfo culture)
		{
			return ((bool)value) ? Visibility.Visible : Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType,
								  object parameter, CultureInfo culture)
		{
			return Binding.DoNothing;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}
	}

	public partial class Brick : UserControl
    {
		public static readonly DependencyProperty HiddenProperty =
			DependencyProperty.Register("Hidden", typeof(bool), typeof(Brick), new PropertyMetadata(false));

		public bool Hidden
		{
			get { return (bool)GetValue(HiddenProperty); }
			set { SetValue(HiddenProperty, value); }
		}

        public Brick()
        {
            InitializeComponent();
        }
	}
}
