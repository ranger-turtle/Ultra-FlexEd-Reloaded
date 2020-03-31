using LevelSetData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ultra_FlexEd_Reloaded.DialogWindows;
using Ultra_FlexEd_Reloaded.LevelManagement;
using Ultra_FlexEd_Reloaded.UserControls;

namespace Ultra_FlexEd_Reloaded
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	//TODO Implement save and load for old and new level sets
	//TODO Make level handling
	//TODO Implement edit and remove brick types
	public partial class MainWindow : Window
	{
		private BrickView[,] _bricksInEditor = new BrickView[LevelSet.ROWS, LevelSet.COLUMNS];
		private LevelSetManager _levelSetManager = new LevelSetManager();
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
		internal ImageSource CurrentBitmap { get; set; }

		private bool _ready;

		private int _beginX, _beginY;

		public MainWindow()
		{
			InitializeComponent();
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
			SingleBrickButton.IsChecked = true;
			_currentPaintModeButton = SingleBrickButton;
			_currentPaintModeButton.LockToggle = true;
			SwitchHoverErase(EraseHoverEvent);
			_ready = true;
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
			BrickListBoxItems.Add(imageListBoxItem);
			List<ImageListBoxItem> list = BrickListBoxItems.ToList();
			list.Sort((blbi1, blbi2) => blbi1.BrickId.CompareTo(blbi2.BrickId));
			BrickListBoxItems = new ObservableCollection<ImageListBoxItem>(list);
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
			BrickWindow brickWindow = new BrickWindow(_levelSetManager)
			{
				Owner = this
			};
			bool? confirmed = brickWindow.ShowDialog();
			if (confirmed == true)
			{
				BrickProperties brickProperties = brickWindow.BrickProperties;
				_levelSetManager.AddBrickToLevelSet(brickWindow.BrickName, brickProperties, brickWindow.FrameSheetNames, null);
				AddNewBrickTypeToListBox(brickProperties.Id, brickWindow.BrickName);
				MessageBox.Show($@"Brick ""{brickWindow.BrickName}"" saved successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		private void RefreshBoard()
		{
			int x, y;
			foreach (BrickView brickView in _bricksInEditor)
			{
				x = Grid.GetColumn(brickView);
				y = Grid.GetRow(brickView);
				BrickInLevel brickInLevel = _levelSetManager.CurrentLevel.Bricks[y, x];
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
			BrickInLevel brickInLevel = _levelSetManager.LevelSet.Levels[_levelSetManager.CurrentLevelIndex].Bricks[brickY, brickX];
			brickInLevel.Hidden = _bricksInEditor[brickY, brickX].Hidden;
			brickInLevel.BrickId = _bricksInEditor[brickY, brickX].BrickId;
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
			foreach (BrickView brick in _bricksInEditor)
			{
				brick.MouseEnter -= _currentBrickMouseHoverHandler;
			}
			_currentBrickMouseHoverHandler = null;
		}

		private void PutSingleBrick(object sender, RoutedEventArgs e)
		{
			PutBrickAndUpdateItInLevel(sender as BrickView, false);
			SwitchHoverEvent(PutSingleBrick);
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
				BrickInLevel brickInLevel = _levelSetManager.LevelSet.Levels[_levelSetManager.CurrentLevelIndex].Bricks[brickY, brickX];
				brickInLevel.BrickId = brickView.BrickId;
				brickInLevel.Hidden = brickView.Hidden;
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

		private void Exit(object sender, RoutedEventArgs e) => Close();
	}
}
