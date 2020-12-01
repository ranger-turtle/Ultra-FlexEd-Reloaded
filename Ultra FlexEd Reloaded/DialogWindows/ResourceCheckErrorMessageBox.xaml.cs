using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
	/// Interaction logic for ResourceCheckErrorMessageBox.xaml
	/// </summary>
	public partial class ResourceCheckErrorMessageBox : Window
    {
		public ResourceCheckErrorMessageBox(List<string> missingFileNameList)
		{
			InitializeComponent();
			SystemSounds.Hand.Play();
			foreach (string fileName in missingFileNameList)
			{
				MissingFilesTextBlock.Text += fileName + Environment.NewLine;
			}
		}

		public ResourceCheckErrorMessageBox(List<string> missingFileNameList, string message) : this(missingFileNameList)
			=> Message.Content = message;

		private void Ok_Clicked(object sender, RoutedEventArgs e) => DialogResult = true;
	}
}
