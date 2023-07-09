using LevelSetManagement;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	/// <summary>
	/// Interaction logic for CutsceneWindow.xaml
	/// </summary>
	public partial class CutsceneWindow : Window
	{
		public int FrameNum
		{
			get { return (int)GetValue(FrameNumProperty); }
			set { SetValue(FrameNumProperty, value); }
		}

		// Using a DependencyProperty as the backing store for FrameNum.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FrameNumProperty =
			DependencyProperty.Register("FrameNum", typeof(int), typeof(CutsceneWindow), new PropertyMetadata(1));

		private List<string> ImagePaths
		{
			get { return (List<string>)GetValue(ImagePathsProperty); }
			set { SetValue(ImagePathsProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ImagePaths.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ImagePathsProperty =
			DependencyProperty.Register("ImagePaths", typeof(List<string>), typeof(CutsceneWindow));


		private List<string> dialogues;

		private string cutsceneName;

		private string musicPath;

		public CutsceneWindow(string cutsceneName)
		{
			InitializeComponent();
			this.cutsceneName = cutsceneName;
			Title = $"Cutscene \"{cutsceneName}\" Edit";
			LevelSetManager levelSetManager = LevelSetManager.GetInstance();
			levelSetManager.ImportCutscene(cutsceneName, out string[] dialogues, out string[] imagePaths, out musicPath);
			this.dialogues = new List<string>();
			this.ImagePaths = new List<string>();
			if (dialogues != null)
			{
				for (int i = 0; i < dialogues.Length; i++)
				{
					this.dialogues.Add(dialogues[i]);
					this.ImagePaths.Add(imagePaths[i]);
				}
				UpdateFrameImage();
				UpdateSlider();
			}
			FrameSlider.ValueChanged += FrameSlider_ValueChanged;
		}

		private void UpdateFrameImage()
		{
			if (!string.IsNullOrEmpty(ImagePaths[FrameNum - 1]))
			{
				BitmapImage bitmapImage = BitmapMethods.GetImageWithCacheOnLoad(new Uri(ImagePaths[FrameNum - 1], UriKind.RelativeOrAbsolute));
				FrameImage.Source = bitmapImage;
				DialogField.Text = dialogues[FrameNum - 1];
			}
			else
			{
				FrameImage.Source = null;
				DialogField.Text = string.Empty;
			}
		}

		private void DialogField_LostFocus(object sender, RoutedEventArgs e)
		{
			dialogues[FrameNum - 1] = DialogField.Text;
		}

		private void AddFrame_Clicked(object sender, RoutedEventArgs e)
		{
			dialogues.Add(string.Empty);
			ImagePaths.Add(string.Empty);
			UpdateSlider();
		}

		private void RemoveFrame_Clicked(object sender, RoutedEventArgs e)
		{
			dialogues.Remove(dialogues[FrameNum]);
			ImagePaths.Remove(ImagePaths[FrameNum]);
			if (FrameNum == ImagePaths.Count + 1 && FrameNum != 1)
				FrameNum--;
			UpdateFrameImage();
			UpdateSlider();
		}

		private void UpdateSlider()
		{
			if (ImagePaths.Count > 0)
			{
				ImportImageBtn.IsEnabled = true;
				ImportMusicBtn.IsEnabled = true;
				FrameSlider.IsEnabled = true;
				FrameSlider.Maximum = ImagePaths.Count;
			}
			else
			{
				ImportImageBtn.IsEnabled = false;
				ImportMusicBtn.IsEnabled = false;
				FrameSlider.IsEnabled = false;
			}
		}

		private void FrameSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			UpdateFrameImage();
		}

		private void ImportImage_Clicked(object sender, RoutedEventArgs e) => OpenImage();

		private void ImportMusic_Clicked(object sender, RoutedEventArgs e) => OpenMusic();

		private void OpenImage()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				DefaultExt = ".jpg",
				Filter = "JPG images (.jpg)|*.jpg"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				ImagePaths[FrameNum - 1] = openFileDialog.FileName;
				UpdateFrameImage();
			}
		}

		private void OpenMusic()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				DefaultExt = ".ogg",
				Filter = "OGG music file (.ogg)|*.ogg"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				musicPath = openFileDialog.FileName;
				MusicTextBox.Text = Path.GetFileNameWithoutExtension(openFileDialog.SafeFileName);
			}
		}


		private void Ok_Clicked(object sender, RoutedEventArgs e)
		{
			if (!ImagePaths.Any(ip => string.IsNullOrEmpty(ip)))
			{
				LevelSetManager.GetInstance().CreateCutscene(cutsceneName, dialogues.ToArray(), ImagePaths.ToArray(), musicPath);
				DialogResult = true;
			}
			else
				MessageBox.Show("You must assign all images to each brick state.", "Info", MessageBoxButton.OK, MessageBoxImage.Warning);
		}
	}
}
