using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	/// <summary>
	/// Interaction logic for BrickChooseWindow.xaml
	/// </summary>
	public partial class MultipleBrickChooseWindow : Window
    {
		public int[] ChosenIds
		{
			get => (int[])GetValue(ChosenIdProperty);
			set => SetValue(ChosenIdProperty, value);
		}

		// Using a DependencyProperty as the backing store for ChosenId.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ChosenIdProperty =
			DependencyProperty.Register("ChosenId", typeof(int[]), typeof(MultipleBrickChooseWindow));

		public List<CheckableBrickListBoxItem> BrickListBoxItems
		{
			get { return (List<CheckableBrickListBoxItem>)GetValue(BrickListBoxItemsProperty); }
			set { SetValue(BrickListBoxItemsProperty, value); }
		}
		// Using a DependencyProperty as the backing store for BrickListBoxItems.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BrickListBoxItemsProperty =
			DependencyProperty.Register("BrickListBoxItems", typeof(List<CheckableBrickListBoxItem>), typeof(MultipleBrickChooseWindow));


		public MultipleBrickChooseWindow(List<BrickMetadata> brickInfo, int[] ids)
        {
			InitializeComponent();//BONUS delete null check when you make improved BrickProperties serialization
			BrickListBoxItems = new List<CheckableBrickListBoxItem>(brickInfo.Select(bi => new CheckableBrickListBoxItem(bi.BrickId, bi.ImageSource, bi.BrickName, ids?.Contains(bi.BrickId) == true)));
			BrickListBox.SelectedIndex = 0;
        }

		private void Ok_Clicked(object sender, RoutedEventArgs e)
		{
			ChosenIds = BrickListBoxItems.Where(bi => bi.Chosen).Select(bi => bi.BrickId).ToArray();
			DialogResult = true;
		}

		private void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
