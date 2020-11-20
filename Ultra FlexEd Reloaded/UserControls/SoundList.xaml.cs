using LevelSetData;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Ultra_FlexEd_Reloaded.DialogWindows;

namespace Ultra_FlexEd_Reloaded.UserControls
{
	/// <summary>
	/// Interaction logic for SoundList.xaml
	/// </summary>
	public partial class SoundList : UserControl
	{
		public SoundLibrary SoundLibrary
		{
			get { return (SoundLibrary)GetValue(SoundLibraryProperty); }
			set { SetValue(SoundLibraryProperty, value); }
		}

		// Using a DependencyProperty as the backing store for SoundLibrary.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SoundLibraryProperty =
			DependencyProperty.Register("SoundLibrary", typeof(SoundLibrary), typeof(SoundList));

		public string Caption
		{
			get { return (string)GetValue(CaptionProperty); }
			set { SetValue(CaptionProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CaptionProperty =
			DependencyProperty.Register("Caption", typeof(string), typeof(SoundList), new PropertyMetadata("Sound"));

		public Window Owner
		{
			get { return (Window)GetValue(OwnerProperty); }
			set { SetValue(OwnerProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Owner.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty OwnerProperty =
			DependencyProperty.Register("Owner", typeof(Window), typeof(SoundList));

		public bool IncludeLevelSetDefault
		{
			get { return (bool)GetValue(IncludeLevelSetDefaultProperty); }
			set { SetValue(IncludeLevelSetDefaultProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IncludeLevelSetDefault.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IncludeLevelSetDefaultProperty =
			DependencyProperty.Register("IncludeLevelSetDefault", typeof(bool), typeof(SoundList));


		private Dictionary<string, SoundLibrary.SoundLibraryPropertyAccessor> changeSoundFileActions;

		public SoundList()
		{
			InitializeComponent();
			foreach (var key in SoundLibrary.SoundNames)
				ComboBox.Items.Add(key);
		}

		private void SetHitSound()
		{
			FileChooseAndImportWindow soundChooseWindow = new FileChooseAndImportWindow("Sounds", ".wav", $"{ComboBox.SelectedItem.ToString()} sound file", includeGameDefault: true, IncludeLevelSetDefault)
			{
				Owner = Owner
			};
			if (soundChooseWindow.ShowDialog() == true)
			{
				SoundLibrary.FromStringKey(ComboBox.SelectedItem.ToString()).SetValue(soundChooseWindow.ChosenFileName);
				HitSound.Text = soundChooseWindow.ChosenFileName;
			}
		}

		private void ChangeButton_Click(object sender, RoutedEventArgs e)
		{
			SetHitSound();
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			HitSound.Text = SoundLibrary.FromStringKey(e.AddedItems[0].ToString()).GetValue();
			ChangeButton.IsEnabled = true;
		}
	}
}
