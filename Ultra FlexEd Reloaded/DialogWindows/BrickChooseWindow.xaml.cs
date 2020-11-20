using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	/// <summary>
	/// Interaction logic for BrickChooseWindow.xaml
	/// </summary>
	public partial class BrickChooseWindow : Window
    {
		public int ChosenId
		{
			get { return (int)GetValue(ChosenIdProperty); }
			set { SetValue(ChosenIdProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ChosenId.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ChosenIdProperty =
			DependencyProperty.Register("ChosenId", typeof(int), typeof(BrickChooseWindow));

		public List<ImageListBoxItem> BrickListBoxItems
		{
			get { return (List<ImageListBoxItem>)GetValue(BrickListBoxItemsProperty); }
			set { SetValue(BrickListBoxItemsProperty, value); }
		}
		// Using a DependencyProperty as the backing store for BrickListBoxItems.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BrickListBoxItemsProperty =
			DependencyProperty.Register("BrickListBoxItems", typeof(List<ImageListBoxItem>), typeof(BrickChooseWindow));


		public BrickChooseWindow(List<BrickMetadata> brickInfo)
        {
			InitializeComponent();
			BrickListBoxItems = new List<ImageListBoxItem> { new ImageListBoxItem() };
			BrickListBoxItems.AddRange(brickInfo.Select(bi => new ImageListBoxItem(bi.BrickId, bi.ImageSource, bi.BrickName)));
			BrickListBox.SelectedIndex = 0;
        }

		private void Ok_Clicked(object sender, RoutedEventArgs e)
		{
			ChosenId = (BrickListBox.SelectedItem as ImageListBoxItem).BrickId;
			DialogResult = true;
		}

		private void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
