using System.Windows;
using System.Windows.Controls.Primitives;

namespace Ultra_FlexEd_Reloaded.UserControls
{
	/// <summary>
	/// Interaction logic for LockableToggleButton.xaml
	/// </summary>
	public partial class LockableToggleButton : ToggleButton
	{
		protected override void OnToggle()
		{
			if (!LockToggle)
				base.OnToggle();
			if (IsChecked == true)
				LockToggle = true;
		}

		public bool LockToggle
		{
			get { return (bool)GetValue(LockToggleProperty); }
			set
			{
				SetValue(LockToggleProperty, value);
				if (value == false) IsChecked = false;
			}
		}

		public static readonly DependencyProperty LockToggleProperty =
			DependencyProperty.Register("LockToggle", typeof(bool), typeof(LockableToggleButton));

		public LockableToggleButton()
		{
			InitializeComponent();
		}
	}
}
