using System.Windows.Media;

namespace Ultra_FlexEd_Reloaded.UserControls
{
	/// <summary>
	/// Interaction logic for CheckableBrickListBoxItem.xaml
	/// </summary>
	public partial class CheckableBrickListBoxItem
	{
		private ImageListBoxItem imageListBoxItem;

		public bool Chosen => ChosenCheckBox.IsChecked == true;
		public int BrickId => imageListBoxItem.BrickId;
		public string BrickName => imageListBoxItem.Label.Content as string;

		public CheckableBrickListBoxItem(int id, ImageSource source, string brickName, bool chosen)
		{
			InitializeComponent();
			imageListBoxItem = ChosenCheckBox.Content as ImageListBoxItem;
			imageListBoxItem.BrickId = id;
			imageListBoxItem.Image.Source = source;
			imageListBoxItem.Label.Content = brickName;
			ChosenCheckBox.IsChecked = chosen;
		}
	}
}
