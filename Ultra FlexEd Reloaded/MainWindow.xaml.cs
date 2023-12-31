﻿using LevelSetData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Ultra_FlexEd_Reloaded.DialogWindows;
using LevelSetManagement;
using Ultra_FlexEd_Reloaded.UserControls;
using System.IO;

namespace Ultra_FlexEd_Reloaded
{
	public class BrickMetadata
	{
		public ImageSource ImageSource { get; set; }
		public string BrickName { get; set; }
		public int BrickId { get; set; }
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		internal static string DIRECTORY = Directory.GetCurrentDirectory();

		internal BrickView[,] bricksInEditor = new BrickView[LevelSet.ROWS, LevelSet.COLUMNS];
		public LevelSetManager LevelSetManager { get; private set; }
		internal ClipboardManager ClipboardManager { get; private set; } = new ClipboardManager();
		private readonly ToolManager toolManager;
		public static readonly DependencyProperty BrickListBoxItemsProperty =
			DependencyProperty.Register("BrickListBoxItems", typeof(ObservableCollection<ImageListBoxItem>), typeof(MainWindow));
		public ObservableCollection<ImageListBoxItem> BrickListBoxItems
		{
			get => GetValue(BrickListBoxItemsProperty) as ObservableCollection<ImageListBoxItem>;
			set => SetValue(BrickListBoxItemsProperty, value);
		}
		public static readonly DependencyProperty LevelListBoxItemsProperty =
			DependencyProperty.Register("LevelListBoxItems", typeof(ObservableCollection<ListBoxItem>), typeof(MainWindow));
		public ObservableCollection<ListBoxItem> LevelListBoxItems
		{
			get => GetValue(LevelListBoxItemsProperty) as ObservableCollection<ListBoxItem>;
			set => SetValue(LevelListBoxItemsProperty, value);
		}
		internal ImageSource CurrentBitmap { get; set; }

		//private bool ready;

		private int beginX, beginY;

		public MainWindow()
		{
			//line below works when it is executed before component initialization
			ApplicationCommands.SaveAs.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));
			try
			{
				InitializeComponent();
				LevelSetManager = LevelSetManager.GetInstance();
				for (int i = 0; i < LevelSet.ROWS; i++)
				{
					for (int j = 0; j < LevelSet.COLUMNS; j++)
					{
						BrickView brick = new BrickView();
						bricksInEditor[i, j] = brick;
						brick.Width = 30;
						brick.Height = 15;
						Bricks.Children.Add(brick);
						Grid.SetRow(brick, i);
						Grid.SetColumn(brick, j);
						brick.MouseLeftButtonDown += Brick_LeftClick;
						brick.MouseRightButtonDown += Brick_RightClick;
						brick.MouseEnter += UpdateCoordinates;
						brick.MouseEnter += OnHover;
						brick.MouseUp += OnRelease;
					}
				}
				BrickListBoxItems = new ObservableCollection<ImageListBoxItem>();
				List<BrickProperties> brickTypes = LevelSetManager.Bricks;
				foreach (BrickProperties brickType in brickTypes)
					AddNewBrickTypeToListBox(brickType.Id, brickType.Name);
				if (LevelSetManager.Bricks.Count > 0)
				{
					BrickListBox.SelectedItem = BrickListBoxItems[0];
					CurrentBitmap = BrickListBoxItems[0].Image.Source;
				}
				LevelListBoxItems = new ObservableCollection<ListBoxItem>
				{
					PrepareLevelToListBox(LevelSetManager.CurrentLevelName)
				};
				toolManager = new ToolManager(this);
				SingleBrickButton.IsChecked = true;
				LevelListBox.SelectedIndex = 0;
				LevelSetManager.SetTesters(new UltraFlexBall2000LevelTester(appSettings.UltraFlexBall2000Path), new UltraFlexBallReloadedLevelTester(appSettings.UltraFlexBallReloadedPath));
				LevelSetManager.UpdateTitle = ChangeTitle;
			}
			catch (ResourceCheckFailException rcfe)
			{
				new ResourceCheckErrorMessageBox(rcfe.MissingResourceNames, "Some default bricks are corrupt. Program will shut down.").ShowDialog();
				Application.Current.Shutdown(1);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown(1);
			}
		}

		private void ChangeTitle(string title) => Title = title;

		private List<BrickMetadata> ConvertListBoxItemsToBrickMetadata()
			=> BrickListBoxItems.Select(item => new BrickMetadata { BrickId = item.BrickId, ImageSource = item.Image.Source, BrickName = item.Label.Content as string }).ToList();

		private void UpdateCoordinates(object sender, RoutedEventArgs e)
		{
			BrickView brickView = sender as BrickView;
			xCord.Content = Grid.GetColumn(brickView);
			yCord.Content = Grid.GetRow(brickView);
		}

		private void AddNewBrickTypeToListBox(int id, string brickName)
		{
			ImageListBoxItem imageListBoxItem = new ImageListBoxItem(id, LevelSetManager.GetBrickFolder(id), brickName);
			imageListBoxItem.Selected += SetCurrentBrick_Selected;
			imageListBoxItem.ContextMenu = FindResource("BrickTypeContextMenu") as ContextMenu;
			BrickListBoxItems.Add(imageListBoxItem);
			List<ImageListBoxItem> list = BrickListBoxItems.ToList();
			list.Sort((blbi1, blbi2) => blbi1.BrickId.CompareTo(blbi2.BrickId));
			BrickListBoxItems = new ObservableCollection<ImageListBoxItem>(list);
		}

		private ListBoxItem PrepareLevelToListBox(string levelName)
		{
			ListBoxItem listBoxItem = new ListBoxItem() { Content = levelName != string.Empty && levelName != null ? levelName : "[Unnamed]" };
			listBoxItem.Selected += SetCurrentLevel_Selected;
			listBoxItem.ContextMenu = FindResource("LevelContextMenu") as ContextMenu;
			return listBoxItem;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			MinWidth = Width;
			MinHeight = Height;
		}

		private void SetCurrentBrick_Selected(object sender, RoutedEventArgs e)
		{
			ImageListBoxItem brickListBoxItem = sender as ImageListBoxItem;
			LevelSetManager.CurrentBrickId = brickListBoxItem.BrickId;
			CurrentBitmap = brickListBoxItem.Image.Source;
		}

		private void AddBrick_Clicked(object sender, RoutedEventArgs e)
		{
			PromptToAddFile(this, () =>
			{
				BrickWindow brickWindow = new BrickWindow(ConvertListBoxItemsToBrickMetadata())
				{
					Owner = this
				};
				bool? confirmed = brickWindow.ShowDialog();
				if (confirmed == true)
				{
					try
					{
						BrickProperties brickProperties = brickWindow.DataContext as BrickProperties;
						LevelSetManager.AddBrickToLevelSet(brickProperties, brickWindow.MainFrameSheetPath, null);
						AddNewBrickTypeToListBox(brickProperties.Id, brickProperties.Name);
						MessageBox.Show($@"Brick ""{brickProperties.Name}"" added successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
					}
					catch (IOException ioe)
					{
						MessageBox.Show(ioe.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			});
		}

		private void ExecuteMenuCommandFromListItem<T>(object sender, Action<T> action) where T : UIElement
		{
			if (sender is MenuItem mi)
			{
				if (mi.CommandParameter is ContextMenu cm)
				{
					T imageListBoxItem = (T)cm.PlacementTarget;
					action(imageListBoxItem);
				}
			}
		}

		private void EditBrick_Clicked(object sender, RoutedEventArgs e)
		{
			ExecuteMenuCommandFromListItem<ImageListBoxItem>(sender, EditBrick);
		}

		private void EditBrick(ImageListBoxItem imageListBoxItem)
		{
			BrickWindow brickWindow = new BrickWindow(ConvertListBoxItemsToBrickMetadata(), LevelSetManager.GetBrickById(imageListBoxItem.BrickId), LevelSetManager.CurrentLevelName)
			{
				Owner = this
			};
			bool? confirmed = brickWindow.ShowDialog();
			if (confirmed == true)
			{
				try
				{
					BrickProperties brickProperties = brickWindow.DataContext as BrickProperties;
					imageListBoxItem.ClearImage();
					LevelSetManager.UpdateBrick(brickProperties, brickWindow.MainFrameSheetPath, brickWindow.OptionalImagePaths);
					imageListBoxItem.Update(LevelSetManager.GetBrickFolder(imageListBoxItem.BrickId), brickProperties.Name);
					foreach (BrickView brickView in bricksInEditor)
						if (imageListBoxItem.BrickId == brickView.BrickId)
							brickView.Image.Source = imageListBoxItem.Image.Source;
					if (imageListBoxItem.BrickId == LevelSetManager.CurrentBrickId)
						CurrentBitmap = imageListBoxItem.Image.Source;
					MessageBox.Show($@"Brick ""{brickProperties.Name}"" changed successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				catch (IOException ioe)
				{
					MessageBox.Show(ioe.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void RemoveBrick_Clicked(object sender, RoutedEventArgs e)
		{
			ExecuteMenuCommandFromListItem<ImageListBoxItem>(sender, RemoveBrick);
		}

		private void RemoveBrick(ImageListBoxItem imageListBoxItem)
		{
			MessageBoxResult result = MessageBox.Show($"Are you sure to delete brick?{Environment.NewLine}If you do so, bricks existing in levels will disappear and level set will be saved.", "Brick remove", MessageBoxButton.YesNo, MessageBoxImage.Question);
			if (result == MessageBoxResult.Yes)
			{
				LevelSetManager.RemoveBrick(imageListBoxItem.BrickId);
				int removedItemIndex = BrickListBoxItems.IndexOf(imageListBoxItem);
				int offset = removedItemIndex != BrickListBoxItems.Count - 1 ? 1 : -1;
				BrickListBox.SelectedItem = BrickListBoxItems[removedItemIndex + offset];
				BrickListBoxItems.Remove(imageListBoxItem);
				RefreshBoard();
			}
		}

		private void SetCurrentLevel_Selected(object sender, RoutedEventArgs e)
		{
			EraseSelection();
			LevelSetManager.CurrentLevelIndex = LevelListBoxItems.IndexOf(sender as ListBoxItem);
			RefreshBoard();
		}

		private void RefreshBoard()
		{
			int x, y;
			foreach (BrickView brickView in bricksInEditor)
			{
				x = Grid.GetColumn(brickView);
				y = Grid.GetRow(brickView);
				BrickInLevel brickInLevel = LevelSetManager.CopyBrickInCurrentLevel(x, y);
				brickView.BrickId = brickInLevel.BrickId;
				brickView.Image.Source = brickView.BrickId != 0 ? BrickListBoxItems.First(blbi => blbi.BrickId == brickView.BrickId).Image.Source : null;
				brickView.Hidden = brickInLevel.BrickId != 0 && LevelSetManager.GetBrickById(brickView.BrickId).Hidden;
				brickView.SelectionBorder = false;
			}
		}

		private void PutBrick(int brickX, int brickY, bool erase)
		{
			bricksInEditor[brickY, brickX].Image.Source = !erase ? CurrentBitmap : null;
			bricksInEditor[brickY, brickX].Hidden = !erase && LevelSetManager.CurrentBrick.Hidden;
			bricksInEditor[brickY, brickX].BrickId = erase ? 0 : LevelSetManager.CurrentBrickId;
		}

		private void UpdateBrickInLevel(int brickX, int brickY)
		{
			BrickInLevel brickInLevel = new BrickInLevel
			{
				BrickId = bricksInEditor[brickY, brickX].BrickId
			};
			LevelSetManager.UpdateBrickInLevel(brickX, brickY, brickInLevel);
		}

		private void PutBrickAndUpdateItInLevel(BrickView brick, bool erase = false)
		{
			int brickX = Grid.GetColumn(brick);
			int brickY = Grid.GetRow(brick);
			PutBrick(brickX, brickY, erase);
			UpdateBrickInLevel(brickX, brickY);
		}

		private void OnHover(object sender, MouseEventArgs e)
		{
			toolManager.CurrentToolCommand.OnHover(sender as BrickView);
		}
		private void OnRelease(object sender, MouseEventArgs e)
		{
			toolManager.CurrentToolCommand.OnRelease();
		}

		internal void PickBrick(int brickId)
		{
			if (brickId != 0)
			{
				BrickListBox.SelectedItem = BrickListBoxItems.First(blbi => blbi.BrickId == brickId);
				BrickListBox.ScrollIntoView(BrickListBox.SelectedItem);
			}
		}

		private void Brick_Click(Action<BrickView> @event, BrickView brickView)
		{
			//TODO add brick pick mode from button
			if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
				@event(brickView);
			else
				PickBrick(brickView.BrickId);
		}

		private void Brick_LeftClick(object sender, MouseButtonEventArgs e) => Brick_Click(toolManager.CurrentToolCommand.OnLeftClick, sender as BrickView);

		private void Brick_RightClick(object sender, MouseButtonEventArgs e) => Brick_Click(toolManager.CurrentToolCommand.OnRightClick, sender as BrickView);

		internal void PutSingleBrick(BrickView brickView, bool erase)
		{
			PutBrickAndUpdateItInLevel(brickView, erase);
			LevelSetManager.SetChangedState();
		}

		private void FillWithBricks(BrickView brickView, bool erase)
		{
			int brickIdForFill = erase ? 0 : LevelSetManager.CurrentBrickId;
			int columnIndex = Grid.GetColumn(brickView);
			int rowIndex = Grid.GetRow(brickView);
			int clickedBrickId = brickView.BrickId;
			bool hidden = brickView.Hidden;
			//Skip when clicked brick type is the same as selected in listbox
			if ((clickedBrickId == 0 && erase) || (clickedBrickId == brickIdForFill && brickView.Hidden == LevelSetManager.CurrentBrick.Hidden)) return;
			PutBrickAndUpdateItInLevel(brickView, erase);
			if (rowIndex > 0 && bricksInEditor[rowIndex - 1, columnIndex].BrickId == clickedBrickId && bricksInEditor[rowIndex - 1, columnIndex].Hidden == hidden)
				FillWithBricks(bricksInEditor[rowIndex - 1, columnIndex], erase);
			if (columnIndex > 0 && bricksInEditor[rowIndex, columnIndex - 1].BrickId == clickedBrickId && bricksInEditor[rowIndex, columnIndex - 1].Hidden == hidden)
				FillWithBricks(bricksInEditor[rowIndex, columnIndex - 1], erase);
			if (rowIndex < LevelSet.ROWS - 1 && bricksInEditor[rowIndex + 1, columnIndex].BrickId == clickedBrickId && bricksInEditor[rowIndex + 1, columnIndex].Hidden == hidden)
				FillWithBricks(bricksInEditor[rowIndex + 1, columnIndex], erase);
			if (columnIndex < LevelSet.COLUMNS - 1 && bricksInEditor[rowIndex, columnIndex + 1].BrickId == clickedBrickId && bricksInEditor[rowIndex, columnIndex + 1].Hidden == hidden)
				FillWithBricks(bricksInEditor[rowIndex, columnIndex + 1], erase);
		}

		internal void BrushFill(BrickView brickView)
		{
			FillWithBricks(brickView, false);
			LevelSetManager.SetChangedState();
		}

		internal void EraseFill(BrickView brickView)
		{
			FillWithBricks(brickView, true);
			LevelSetManager.SetChangedState();
		}

		internal void DrawRectangle(BrickView brick, bool erase)
		{
			RefreshBoard();
			int endX = Grid.GetColumn(brick);
			int endY = Grid.GetRow(brick);
			int swapBeginX = beginX;
			int swapBeginY = beginY;
			if (Math.Sign(endX - beginX) == -1)
			{
				int tmp = endX;
				endX = swapBeginX;
				swapBeginX = tmp;
			}
			if (Math.Sign(endY - beginY) == -1)
			{
				int tmp = endY;
				endY = swapBeginY;
				swapBeginY = tmp;
			}
			for (int y = swapBeginY; y <= endY; y++)
			{
				PutBrick(swapBeginX, y, erase);
				PutBrick(endX, y, erase);
			}
			for (int x = swapBeginX + 1; x <= endX - 1; x++)
			{
				PutBrick(x, swapBeginY, erase);
				PutBrick(x, endY, erase);
			}
		}

		internal void BeginBrickRectangle(BrickView brickView)
		{
			beginX = Grid.GetColumn(brickView);
			beginY = Grid.GetRow(brickView);
			DrawRectangle(brickView, false);
		}

		internal void BeginEraseRectangle(BrickView brickView)
		{
			beginX = Grid.GetColumn(brickView);
			beginY = Grid.GetRow(brickView);
			DrawRectangle(brickView, true);
		}

		internal void DrawLine(BrickView brick, bool erase)
		{
			RefreshBoard();
			int endX = Grid.GetColumn(brick);
			int endY = Grid.GetRow(brick);
			int swapBeginX = beginX;
			int swapBeginY = beginY;
			if (endX == swapBeginX && endY == swapBeginY)
			{
				PutBrick(endX, endY, erase);
				return;
			}
			float dx = endX - swapBeginX;
			float dy = endY - swapBeginY;
			if (dx == 0)
			{
				if (beginY > endY)
				{
					int tmp = endY;
					endY = swapBeginY;
					swapBeginY = tmp;
				}
				for (int iy = swapBeginY; iy <= endY; iy++)
					PutBrick(swapBeginX, iy, erase);
			}
			else
			{
				float m = dy / dx;
				if (Math.Abs(m) <= 1)
				{
					if (beginX > endX)
					{
						int tmp = endX;
						endX = swapBeginX;
						swapBeginX = tmp;
						if (m > 0 || swapBeginY < endY)
						{
							tmp = endY;
							swapBeginY = tmp;
						}
					}
					float y = swapBeginY;
					for (int x = swapBeginX; x <= endX; x++)
					{
						PutBrick(x, (int)Math.Round(y), erase);
						y += m;
					}
				}
				else
				{
					if (beginY > endY)
					{
						int tmp = endY;
						endY = swapBeginY;
						swapBeginY = tmp;
						if (m < 0 || swapBeginX > endX)
						{
							tmp = endX;
							swapBeginX = tmp;
						}
					}
					m = dx / dy;
					float x = swapBeginX;
					for (int y = swapBeginY; y <= endY; y++)
					{
						PutBrick((int)Math.Round(x), y, erase);
						x += m;
					}
				}
			}
		}

		internal void BeginBrickLine(BrickView brickView)
		{
			beginX = Grid.GetColumn(brickView);
			beginY = Grid.GetRow(brickView);
			DrawLine(brickView, false);
		}

		internal void BeginEraseLine(BrickView brickView)
		{
			beginX = Grid.GetColumn(brickView);
			beginY = Grid.GetRow(brickView);
			DrawLine(brickView, true);
		}

		internal void UpdateLevelSet()
		{
			foreach (BrickView brickView in bricksInEditor)
			{
				int brickX = Grid.GetColumn(brickView);
				int brickY = Grid.GetRow(brickView);
				UpdateBrickInLevel(brickX, brickY);
			}
			LevelSetManager.SetChangedState();
		}

		private void SingleBrickButton_Checked(object sender, RoutedEventArgs e)
		{
			toolManager.SwitchCommand("Single Brick");
		}

		private void FillButton_Checked(object sender, RoutedEventArgs e)
		{
			toolManager.SwitchCommand("Fill");
		}

		private void LineButton_Checked(object sender, RoutedEventArgs e)
		{
			toolManager.SwitchCommand("Line");
		}

		private void RectangleButton_Checked(object sender, RoutedEventArgs e)
		{
			toolManager.SwitchCommand("Rectangle");
		}

		private void SelectButton_Checked(object sender, RoutedEventArgs e)
		{
			SwitchContextMenuOnBoard(true);
			toolManager.SwitchCommand("Select");
		}

		private void PickBrick_Checked(object sender, RoutedEventArgs e)
		{
			toolManager.SwitchCommand("Pick Brick");
		}

		internal void SwitchToPreviousCommand() => toolManager.SwitchToPreviousCommand();

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			for (int i = 0; i < LevelSet.ROWS; ++i)
			{
				for (int j = 0; j < LevelSet.COLUMNS; ++j)
				{
					bricksInEditor[i, j].Width = Bricks.ColumnDefinitions[j].ActualWidth;
					bricksInEditor[i, j].Height = Bricks.RowDefinitions[i].ActualHeight;
				}
			}
		}

		private void Reset()
		{
			LevelSetManager.Reset();
			BrickListBoxItems = new ObservableCollection<ImageListBoxItem>(BrickListBoxItems.TakeWhile(blbi => blbi.BrickId <= LevelSetManager.DEFAULT_BRICK_QUANTITY));
			LevelListBoxItems = new ObservableCollection<ListBoxItem>() { PrepareLevelToListBox(LevelSetManager.CurrentLevelName) };
			RefreshBoard();
		}
	}
}
