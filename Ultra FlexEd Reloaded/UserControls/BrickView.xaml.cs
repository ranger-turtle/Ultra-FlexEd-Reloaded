using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Ultra_FlexEd_Reloaded.UserControls
{
	/// <summary>
	/// Interaction logic for Brick.xaml
	/// </summary>
	public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
	{
		public object Convert(object value, Type targetType,
							  object parameter, CultureInfo culture) => ((bool)value) ? Visibility.Visible : Visibility.Hidden;

		public object ConvertBack(object value, Type targetType,
								  object parameter, CultureInfo culture) => Binding.DoNothing;

		public override object ProvideValue(IServiceProvider serviceProvider) => this;
	}

	public partial class BrickView : UserControl, IEquatable<BrickView>
    {
		public static readonly DependencyProperty HiddenProperty =
			DependencyProperty.Register("Hidden", typeof(bool), typeof(BrickView));

		public bool Hidden
		{
			get => (bool)GetValue(HiddenProperty);
			set => SetValue(HiddenProperty, value);
		}

		public static readonly DependencyProperty SelectionBorderProperty =
			DependencyProperty.Register("SelectionBorder", typeof(bool), typeof(BrickView));

		public bool SelectionBorder
		{
			get => (bool)GetValue(SelectionBorderProperty);
			set => SetValue(SelectionBorderProperty, value);
		}

		public int BrickId { get; set; }

		public BrickView() => InitializeComponent();

		public void Clear()
		{
			Image.Source = null;
			BrickId = 0;
			Hidden = false;
		}

		public bool Equals(BrickView other)
		{
			return BrickId == other.BrickId && Hidden == other.Hidden;
		}
	}
}
