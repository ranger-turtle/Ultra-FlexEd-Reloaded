﻿using LevelSetData;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Ultra_FlexEd_Reloaded.DialogWindows
{
	public partial class BrickWindow
	{
		private abstract class EnumConverter : MarkupExtension, IValueConverter
		{
			public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

			protected object ConvertGeneric<T>(string value) where T : Enum
			{
				if (value != "None")
					return Enum.Parse(typeof(T), value);
				else
					return null;
			}

			public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

			protected object ConvertBackGeneric<T>(T value) where T : Enum
			{
				Enum @enum = value as Enum;
				return @enum.ToString().ToLower() as string;
			}

			public override object ProvideValue(IServiceProvider serviceProvider) => this;
		}

		private class TwoIndices
		{
			public int X { get; set; }
			public int Y { get; set; }

			private TwoIndices() { }

			/** <summary>
			 * Evaluate indices with one-dimensional index and width of two-dimensional array
			 * </summary>
			 * <param name="index">Index of one-dimensional version of two-dimensional array</param>
			 * <param name="width">Width of two-dimensional array</param>
			 */
			public static TwoIndices EvaluateIndices(int index, int width)
			{
				return new TwoIndices()
				{
					X = index % width,
					Y = index / width
				};
			}
		}

		private void InitializeFrameDurations()
		{
			BrickProperties brickProperties = DataContext as BrickProperties;
			if (brickProperties.FrameDurations == null || brickProperties.FrameDurations.Length != mainBrickImageMetadata.Frames)
			{
				brickProperties.FrameDurations = new float[mainBrickImageMetadata.Frames];
				for (int i = 0; i < brickProperties.FrameDurations.Length; i++) brickProperties.FrameDurations[i] = 0.4f;
			}
		}
	}
}
