using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;

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
