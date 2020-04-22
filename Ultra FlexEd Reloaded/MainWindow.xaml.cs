using LevelSetData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Ultra_FlexEd_Reloaded.DialogWindows;
using LevelSetManagement;
using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private BrickView[,] _bricksInEditor = new BrickView[LevelSet.ROWS, LevelSet.COLUMNS];
		private LevelSetManager _levelSetManager = LevelSetManager.GetInstance();
		private MouseEventHandler _currentBrickMouseHoverHandler;
		private MouseButtonEventHandler _currentBrickMouseLeftHandler, _currentBrickMouseRightHandler, _currentBrickMouseReleaseHandler;
		private LockableToggleButton _currentPaintModeButton;
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

		private bool _ready;

		private int _beginX, _beginY;

		public MainWindow()
		{
			ApplicationCommands.SaveAs.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));
			InitializeComponent();
			Title = SmallUtilities.MakeTitle();
			for (int i = 0; i < LevelSet.ROWS; i++)
			{
				for (int j = 0; j < LevelSet.COLUMNS; j++)
				{
					BrickView brick = new BrickView();
					_bricksInEditor[i, j] = brick;
					brick.Width = 30;
					brick.Height = 15;
					Bricks.Children.Add(brick);
					Grid.SetRow(brick, i);
					Grid.SetColumn(brick, j);
					brick.MouseLeftButtonDown += _currentBrickMouseLeftHandler = PutSingleBrick;
					brick.MouseRightButtonDown += _currentBrickMouseRightHandler = RemoveSingleBrick;
					brick.MouseEnter += UpdateCoordinates;
				}
			}
			BrickListBoxItems = new ObservableCollection<ImageListBoxItem>();
			SortedDictionary<int, string> brickNames = _levelSetManager.BrickNames;
			foreach (var name in brickNames)
				AddNewBrickTypeToListBox(name.Key, name.Value);
			if (_levelSetManager.Bricks.Count > 0)
			{
				BrickListBox.SelectedItem = BrickListBoxItems[0];
				CurrentBitmap = BrickListBoxItems[0].Image.Source;
			}
			LevelListBoxItems = new ObservableCollection<ListBoxItem>
			{
				PrepareLevelToListBox(_levelSetManager.CurrentLevelName)
			};
			LevelListBox.SelectedIndex = 0;
			SingleBrickButton.IsChecked = true;
			_currentPaintModeButton = SingleBrickButton;
			_currentPaintModeButton.LockToggle = true;
			SwitchHoverErase(EraseHoverEvent);
			_ready = true;
			_levelSetManager.UpdateTitle = Change;
		}

		private void Change(object sender, EventArgs e)
		{
			Title = SmallUtilities.MakeTitle(_levelSetManager.FilePath, _levelSetManager.Changed);
		}

		private void UpdateCoordinates(object sender, RoutedEventArgs e)
		{
			BrickView brickView = sender as BrickView;
			xCord.Content = Grid.GetColumn(brickView);
			yCord.Content = Grid.GetRow(brickView);
		}
		
		private void AddNewBrickTypeToListBox(int id, string brickName)
		{
			ImageListBoxItem imageListBoxItem = new ImageListBoxItem(id, _levelSetManager.GetBrickFolder(id), brickName);
			imageListBoxItem.Selected += SetCurrentBrick_Selected;
			imageListBoxItem.ContextMenu = FindResource("BrickContextMenu") as ContextMenu;
			BrickListBoxItems.Add(imageListBoxItem);
			List<ImageListBoxItem> list = BrickListBoxItems.ToList();
			list.Sort((blbi1, blbi2) => blbi1.BrickId.CompareTo(blbi2.BrickId));
			BrickListBoxItems = new ObservableCollection<ImageListBoxItem>(list);
		}

		private ListBoxItem PrepareLevelToListBox(string levelName)
		{
			ListBoxItem listBoxItem = new ListBoxItem() { Content = levelName != "" && levelName != null ? levelName : "[Unnamed]" };
			listBoxItem.Selected += SetCurrentLevel_Selected;
			listBoxItem.ContextMenu = FindResource("LevelContextMenu") as ContextMenu;
			return listBoxItem;
		}

		private void SwitchHoverErase(MouseButtonEventHandler @event)
		{
			if (_currentBrickMouseReleaseHandler != null)
			{
				Bricks.MouseLeftButtonUp -= _currentBrickMouseReleaseHandler;
				Bricks.MouseRightButtonUp -= _currentBrickMouseReleaseHandler;
			}
			_currentBrickMouseReleaseHandler = @event;
			if (@event != null)
			{
				Bricks.MouseLeftButtonUp += @event;
				Bricks.MouseRightButtonUp += @event;
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			MinWidth = Width;
			MinHeight = Height;
		}

		private void SetCurrentBrick_Selected(object sender, RoutedEventArgs e)
		{
			ImageListBoxItem brickListBoxItem = sender as ImageListBoxItem;
			_levelSetManager.CurrentBrickId = brickListBoxItem.BrickId;
			CurrentBitmap = brickListBoxItem.Image.Source;
		}

		private void AddBrick_Clicked(object sender, RoutedEventArgs e)
		{
			BrickWindow brickWindow = new BrickWindow()
			{
				Owner = this
			};
			bool? confirmed = brickWindow.ShowDialog();
			if (confirmed == true)
			{
				BrickProperties brickProperties = brickWindow.DataContext as BrickProperties;
				_levelSetManager.AddBrickToLevelSet(brickWindow.BrickName, brickProperties, brickWindow.FrameSheetMetadata, null);
				AddNewBrickTypeToListBox(brickProperties.Id, brickWindow.BrickName);
				MessageBox.Show($@"Brick ""{brickWindow.BrickName}"" added successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
			}
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
			BrickWindow brickWindow = new BrickWindow(_levelSetManager.GetBrickById(imageListBoxItem.BrickId), imageListBoxItem.Label.Content as string, _levelSetManager.CurrentLevelName)
			{
				Owner = this
			};
			bool? confirmed = brickWindow.ShowDialog();
			if (confirmed == true)
			{
				BrickProperties brickProperties = brickWindow.DataContext as BrickProperties;
				imageListBoxItem.ClearImage();
				_levelSetManager.UpdateBrick(brickWindow.BrickName, brickProperties, brickWindow.FrameSheetMetadata, null);
				imageListBoxItem.Update(_levelSetManager.GetBrickFolder(imageListBoxItem.BrickId), brickWindow.BrickName);
				foreach (BrickView brickView in _bricksInEditor)
					if (imageListBoxItem.BrickId == brickView.BrickId)
						brickView.Image.Source = imageListBoxItem.Image.Source;
				if (imageListBoxItem.BrickId == _levelSetManager.CurrentBrickId)
					CurrentBitmap = imageListBoxItem.Image.Source;
				MessageBox.Show($@"Brick ""{brickWindow.BrickName}"" changed successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
				_levelSetManager.RemoveBrick(imageListBoxItem.BrickId);
				RefreshBoard();
				int removedItemIndex = BrickListBoxItems.IndexOf(imageListBoxItem);
				int offset = removedItemIndex != BrickListBoxItems.Count - 1 ? 1 : -1;
				BrickListBox.SelectedItem = BrickListBoxItems[removedItemIndex + offset];
				BrickListBoxItems.Remove(imageListBoxItem);
			}
		}

		private void SetCurrentLevel_Selected(object sender, RoutedEventArgs e)
		{
			_levelSetManager.CurrentLevelIndex = LevelListBoxItems.IndexOf(sender as ListBoxItem);
			RefreshBoard();
		}

		private void RefreshBoard()
		{
			int x, y;
			foreach (BrickView brickView in _bricksInEditor)
			{
				x = Grid.GetColumn(brickView);
				y = Grid.GetRow(brickView);
				BrickInLevel brickInLevel = _levelSetManager.CopyBrickInCurrentLevel(x, y);
				brickView.BrickId = brickInLevel.BrickId;
				brickView.Image.Source = brickView.BrickId != 0 ? BrickListBoxItems.First(blbi => blbi.BrickId == brickView.BrickId).Image.Source : null;
				brickView.Hidden = brickInLevel.Hidden;
			}
		}

		private void PutBrick(int brickX, int brickY, bool erase)
		{
			_bricksInEditor[brickY, brickX].Image.Source = !erase ? CurrentBitmap : null;
			_bricksInEditor[brickY, brickX].Hidden = !erase ? _levelSetManager.Hidden : false;
			_bricksInEditor[brickY, brickX].BrickId = erase ? 0 : _levelSetManager.CurrentBrickId;
		}

		private void UpdateBrickInLevel(int brickX, int brickY, bool erase)
		{
			BrickInLevel brickInLevel = new BrickInLevel
			{
				Hidden = erase ? false : _bricksInEditor[brickY, brickX].Hidden,
				BrickId = erase ? 0 : _bricksInEditor[brickY, brickX].BrickId
			};
			_levelSetManager.UpdateBrickInLevel(brickX, brickY, brickInLevel);
		}

		private void PutBrickAndUpdateItInLevel(BrickView brick, bool erase)
		{
			int brickX = Grid.GetColumn(brick);
			int brickY = Grid.GetRow(brick);
			PutBrick(brickX, brickY, erase);
			UpdateBrickInLevel(brickX, brickY, erase);
		}

		private void EraseHoverEvent(object sender, MouseButtonEventArgs e)
		{
			if (_currentBrickMouseHoverHandler != null)//Bug fix on click file in filedialog over board preview
			{
				foreach (BrickView brick in _bricksInEditor)
				{
					brick.MouseEnter -= _currentBrickMouseHoverHandler;
				}
				_currentBrickMouseHoverHandler = null;
			}
		}

		private void PutSingleBrick(object sender, RoutedEventArgs e)
		{
			BrickView brickView = sender as BrickView;
			//TODO Move condition to independent event handler which picks current brick after click
			//TODO add brick pick
			if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
			{
				PutBrickAndUpdateItInLevel(brickView, false);
				SwitchHoverEvent(PutSingleBrick);
			}
			else
			{
				BrickListBox.SelectedItem = BrickListBoxItems.First(blbi => blbi.BrickId == brickView.BrickId);
			}
		}

		private void RemoveSingleBrick(object sender, RoutedEventArgs e)
		{
			PutBrickAndUpdateItInLevel(sender as BrickView, true);
			SwitchHoverEvent(RemoveSingleBrick);
		}

		private void FillWithBricks(BrickView brickView, bool erase)
		{
			int brickIdForFill = erase ? 0 : _levelSetManager.CurrentBrickId;
			int columnIndex = Grid.GetColumn(brickView);
			int rowIndex = Grid.GetRow(brickView);
			int clickedBrickId = brickView.BrickId;
			bool hidden = brickView.Hidden;
			//Skip when clicked brick type is the same as selected in listbox
			if (clickedBrickId == 0 && erase || clickedBrickId == brickIdForFill && brickView.Hidden == _levelSetManager.Hidden) return;
			PutBrickAndUpdateItInLevel(brickView, erase);
			//TODO make BrickView Comparable
			if (rowIndex > 0 && _bricksInEditor[rowIndex - 1, columnIndex].BrickId == clickedBrickId && _bricksInEditor[rowIndex - 1, columnIndex].Hidden == hidden)
				FillWithBricks(_bricksInEditor[rowIndex - 1, columnIndex], erase);
			if (columnIndex > 0 && _bricksInEditor[rowIndex, columnIndex - 1].BrickId == clickedBrickId && _bricksInEditor[rowIndex, columnIndex - 1].Hidden == hidden)
				FillWithBricks(_bricksInEditor[rowIndex, columnIndex - 1], erase);
			if (rowIndex < LevelSet.ROWS - 1 && _bricksInEditor[rowIndex + 1, columnIndex].BrickId == clickedBrickId && _bricksInEditor[rowIndex + 1, columnIndex].Hidden == hidden)
				FillWithBricks(_bricksInEditor[rowIndex + 1, columnIndex], erase);
			if (columnIndex < LevelSet.COLUMNS - 1 && _bricksInEditor[rowIndex, columnIndex + 1].BrickId == clickedBrickId && _bricksInEditor[rowIndex, columnIndex + 1].Hidden == hidden)
				FillWithBricks(_bricksInEditor[rowIndex, columnIndex + 1], erase);
		}

		private void BrushFill(object sender, RoutedEventArgs e) => FillWithBricks(sender as BrickView, false);

		private void EraseFill(object sender, RoutedEventArgs e) => FillWithBricks(sender as BrickView, true);

		private void DrawRectangle(BrickView brick, bool erase)
		{
			RefreshBoard();
			int brickIdForFill = erase ? 0 : _levelSetManager.CurrentBrickId;
			int endX = Grid.GetColumn(brick);
			int endY = Grid.GetRow(brick);
			int swapBeginX = _beginX;
			int swapBeginY = _beginY;
			if (Math.Sign(endX - _beginX) == -1)
			{
				int tmp = endX;
				endX = swapBeginX;
				swapBeginX = tmp;
			}
			if (Math.Sign(endY - _beginY) == -1)
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

		private void BrickRectangle(object sender, RoutedEventArgs e)
		{
			DrawRectangle(sender as BrickView, false);
		}

		private void BeginBrickRectangle(object sender, RoutedEventArgs e)
		{
			BrickView brickView = sender as BrickView;
			_beginX = Grid.GetColumn(brickView);
			_beginY = Grid.GetRow(brickView);
			BrickRectangle(brickView, e);
			SwitchHoverEvent(BrickRectangle);
		}

		private void EraseRectangle(object sender, RoutedEventArgs e)
		{
			DrawRectangle(sender as BrickView, true);
		}

		private void BeginEraseRectangle(object sender, RoutedEventArgs e)
		{
			BrickView brickView = sender as BrickView;
			_beginX = Grid.GetColumn(brickView);
			_beginY = Grid.GetRow(brickView);
			EraseRectangle(brickView, e);
			SwitchHoverEvent(EraseRectangle);
		}

		private void DrawLine(BrickView brick, bool erase)
		{
			RefreshBoard();
			int brickIdForFill = erase ? 0 : _levelSetManager.CurrentBrickId;
			int endX = Grid.GetColumn(brick);
			int endY = Grid.GetRow(brick);
			int swapBeginX = _beginX;
			int swapBeginY = _beginY;
			if (endX == swapBeginX && endY == swapBeginY)
			{
				PutBrick(endX, endY, erase);
				return;
			}
			float dx = endX - swapBeginX;
			float dy = endY - swapBeginY;
			if (dx == 0)
			{
				if (_beginY > endY)
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
					if (_beginX > endX)
					{
						int tmp = endX;
						endX = swapBeginX;
						swapBeginX = tmp;
						if (m > 0 || swapBeginY < endY)
						{
							tmp = endY;
							endY = swapBeginY;
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
					if (_beginY > endY)
					{
						int tmp = endY;
						endY = swapBeginY;
						swapBeginY = tmp;
						if (m < 0 || swapBeginX > endX)
						{
							tmp = endX;
							endX = swapBeginX;
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

		private void BrickLine(object sender, RoutedEventArgs e)
		{
			DrawLine(sender as BrickView, false);
		}

		private void BeginBrickLine(object sender, RoutedEventArgs e)
		{
			BrickView brickView = sender as BrickView;
			_beginX = Grid.GetColumn(brickView);
			_beginY = Grid.GetRow(brickView);
			BrickLine(brickView, e);
			SwitchHoverEvent(BrickLine);
		}

		private void EraseLine(object sender, RoutedEventArgs e)
		{
			DrawLine(sender as BrickView, true);
		}

		private void BeginEraseLine(object sender, RoutedEventArgs e)
		{
			BrickView brickView = sender as BrickView;
			_beginX = Grid.GetColumn(brickView);
			_beginY = Grid.GetRow(brickView);
			EraseLine(brickView, e);
			SwitchHoverEvent(EraseLine);
		}


		private void UpdateLevelSet(object sender, MouseButtonEventArgs e)
		{
			foreach (BrickView brickView in _bricksInEditor)
			{
				int brickX = Grid.GetColumn(brickView);
				int brickY = Grid.GetRow(brickView);
				UpdateBrickInLevel(brickX, brickY, brickView.Hidden);
			}
			EraseHoverEvent(sender, e);
		}

		private void SwitchEvents(/*MouseEventHandler brickHoverHandler, */MouseButtonEventHandler brickMouseLeftHandler, MouseButtonEventHandler brickMouseRightHandler)
		{
			//MouseEventHandler oldMouseHoverHandler = _currentBrickMouseHoverHandler;
			//_currentBrickMouseHoverHandler = brickHoverHandler;
			MouseButtonEventHandler oldMouseLeftHandler = _currentBrickMouseLeftHandler;
			_currentBrickMouseLeftHandler = brickMouseLeftHandler;
			MouseButtonEventHandler oldMouseRightHandler = _currentBrickMouseRightHandler;
			_currentBrickMouseRightHandler = brickMouseRightHandler;
			foreach (BrickView brick in _bricksInEditor)
			{
				//if (_currentBrickMouseHoverHandler != null)
				//	brick.MouseEnter -= _currentBrickMouseHoverHandler;
				//if (brickHoverHandler != null)
				//	brick.MouseEnter += _currentBrickMouseHoverHandler;
				brick.MouseLeftButtonDown -= oldMouseLeftHandler;
				brick.MouseLeftButtonDown += _currentBrickMouseLeftHandler;
				brick.MouseRightButtonDown -= oldMouseRightHandler;
				brick.MouseRightButtonDown += _currentBrickMouseRightHandler;
			}
		}

		private void SwitchHoverEvent(MouseEventHandler brickHoverHandler)
		{
			foreach (BrickView brick in _bricksInEditor)
			{
				if (_currentBrickMouseHoverHandler != null)
					brick.MouseEnter -= _currentBrickMouseHoverHandler;
				brick.MouseEnter += _currentBrickMouseHoverHandler = brickHoverHandler;
			}
		}

		private void SwitchPaintModeButton(LockableToggleButton paintModeButton)
		{
			_currentPaintModeButton.LockToggle = false;
			_currentPaintModeButton = paintModeButton;
		}

		private void SingleBrickButton_Checked(object sender, RoutedEventArgs e)
		{
			if (_ready)
			{
				SwitchPaintModeButton(SingleBrickButton);
				FillButton.IsChecked = false;
				LineButton.IsChecked = false;
				RectangleButton.IsChecked = false;
				SwitchEvents(PutSingleBrick, RemoveSingleBrick);
				SwitchHoverErase(EraseHoverEvent);
			}
		}

		private void FillButton_Checked(object sender, RoutedEventArgs e)
		{
			SwitchPaintModeButton(FillButton);
			SingleBrickButton.IsChecked = false;
			LineButton.IsChecked = false;
			RectangleButton.IsChecked = false;
			SwitchEvents(BrushFill, EraseFill);
			SwitchHoverErase(null);
		}

		private void LineButton_Checked(object sender, RoutedEventArgs e)
		{
			SwitchPaintModeButton(LineButton);
			FillButton.IsChecked = false;
			SingleBrickButton.IsChecked = false;
			RectangleButton.IsChecked = false;
			SwitchEvents(BeginBrickLine, BeginEraseLine);
			SwitchHoverErase(UpdateLevelSet);
		}

		private void RectangleButton_Checked(object sender, RoutedEventArgs e)
		{
			SwitchPaintModeButton(RectangleButton);
			FillButton.IsChecked = false;
			LineButton.IsChecked = false;
			SingleBrickButton.IsChecked = false;
			SwitchEvents(BeginBrickRectangle, BeginEraseRectangle);
			SwitchHoverErase(UpdateLevelSet);
		}

		private void HiddenMode_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton toggleButton = sender as ToggleButton;
			_levelSetManager.Hidden = toggleButton.IsChecked.GetValueOrDefault();
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			for (int i = 0; i < LevelSet.ROWS; ++i)
			{
				for (int j = 0; j < LevelSet.COLUMNS; ++j)
				{
					_bricksInEditor[i, j].Width = Bricks.ColumnDefinitions[j].ActualWidth;
					_bricksInEditor[i, j].Height = Bricks.RowDefinitions[i].ActualHeight;
				}
			}
		}

		private void Reset()
		{
			_levelSetManager.Reset();
			BrickListBoxItems = new ObservableCollection<ImageListBoxItem>(BrickListBoxItems.TakeWhile(blbi => blbi.BrickId <= LevelSetManager.DEFAULT_BRICK_QUANTITY));
			LevelListBoxItems = new ObservableCollection<ListBoxItem>() { PrepareLevelToListBox(_levelSetManager.CurrentLevelName) };
			RefreshBoard();
			Title = SmallUtilities.MakeTitle();
		}
	}
}
