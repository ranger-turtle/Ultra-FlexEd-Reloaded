using LevelSetData;
using LevelSetManagement;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	internal class NewLevelEnumConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return false;

			string checkValue = value.ToString();
			string targetValue = parameter.ToString();

			return checkValue.Equals(targetValue, StringComparison.InvariantCultureIgnoreCase);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return null;

			bool useValue = (bool)value;
			string targetValue = parameter.ToString();
			return useValue ? Enum.Parse(targetType, targetValue) : null;
		}
	}

	/// <summary>
	/// Interaction logic for CreateOrImportLevelWindow.xaml
	/// </summary>
	public partial class CreateOrImportLevelWindow : Window
	{
		public enum NewLevelMethod
		{
			Create, Import
		}

		private readonly Dictionary<FormatType, string> formatTypes = new Dictionary<FormatType, string>()
		{
			[FormatType.New] = "Ultra FlexBall Reloaded Level Sets (.nlev)|*.nlev",
			[FormatType.Old] = "Ultra FlexBall 2000 Level Sets (.lev)|*.lev",
		};

		public static readonly DependencyProperty NewLevelMethodProperty =
			DependencyProperty.Register("Method", typeof(NewLevelMethod), typeof(CreateOrImportLevelWindow));

		public NewLevelMethod Method
		{
			get => (NewLevelMethod)GetValue(NewLevelMethodProperty);
			set => SetValue(NewLevelMethodProperty, value);
		}

		public Level Level { get; private set; }
		private LevelSetManager _levelSetManager;

		public CreateOrImportLevelWindow(LevelSetManager levelSetManager)
		{
			InitializeComponent();
			_levelSetManager = levelSetManager;
		}

		private void Ok_Clicked(object sender, RoutedEventArgs e)
		{
			switch (Method)
			{
				case NewLevelMethod.Create:
					LevelWindow levelWindow = new LevelWindow();
					bool? result = levelWindow.ShowDialog();
					if (result == true)
					{
						Level = new Level()
						{
							LevelProperties = levelWindow.DataContext as LevelProperties
						};
						DialogResult = true;
					}
					break;
				case NewLevelMethod.Import:
					OpenFileDialog openFileDialog = new OpenFileDialog()
					{
						Filter = formatTypes[_levelSetManager.CurrentFormatType]
					};
					if (openFileDialog.ShowDialog(this) == true)
					{
						LevelChooseWindow levelChooseWindow = new LevelChooseWindow(_levelSetManager.GetLevelsFromFile(openFileDialog.FileName), Path.GetFileName(openFileDialog.FileName));
						if (levelChooseWindow.ShowDialog() == true)
						{
							Level = levelChooseWindow.ChosenLevel;
							DialogResult = true;
						}
					}
					break;
				default:
					break;
			}
		}

		private void Cancel_Clicked(object sender, RoutedEventArgs e) => DialogResult = false;
	}
}
